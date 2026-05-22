-- FASE B: Auditoría y Saneamiento Multiempresa (CompanyProduct vs Product cruzados)
-- Ejecutar en SQL Server Management Studio o Azure Data Studio.
-- NOTA: El script inicia en transaccion y hace ROLLBACK al final; no realiza cambios físicos permanentes salvo que descomentes el COMMIT.

BEGIN TRANSACTION;

PRINT '==========================================================='
PRINT '1. AUDITORIA: IDENTIFICACION DE REGISTROS CRUZADOS'
PRINT '==========================================================='

SELECT 
    cp.CompanyProductId, 
    cp.EmpresaId AS EmpresaCompanyProduct,
    cp.ProductId,
    p.EmpresaId  AS EmpresaProduct,
    cp.Sku, 
    cp.CodigoInterno, 
    p.CommercialName,
    cp.Activo
INTO #TempCruzados
FROM [mdm].[CompanyProducts] cp
INNER JOIN [mdm].[Products] p ON cp.ProductId = p.ProductId
WHERE cp.EmpresaId <> p.EmpresaId;

DECLARE @TotalCruzados INT = (SELECT COUNT(*) FROM #TempCruzados);
PRINT 'Total de CompanyProducts cruzados encontrados: ' + CAST(@TotalCruzados AS VARCHAR(20));

SELECT * FROM #TempCruzados;

PRINT '==========================================================='
PRINT '2. AUDITORIA: VERIFICANDO DEPENDENCIAS (USO OPERATIVO)'
PRINT '==========================================================='

SELECT 
    tc.CompanyProductId,
    tc.EmpresaCompanyProduct,
    tc.ProductId,
    tc.EmpresaProduct,
    tc.Sku,
    tc.CommercialName,
    (SELECT COUNT(*) FROM [inv].[StockProductos] s WHERE s.CompanyProductId = tc.CompanyProductId) AS C_Stock,
    (SELECT COUNT(*) FROM [inv].[MovimientosInventario] m WHERE m.CompanyProductId = tc.CompanyProductId) AS C_Movimientos,
    (SELECT COUNT(*) FROM [vta].[PedidoVentaDetalles] v WHERE v.CompanyProductId = tc.CompanyProductId) AS C_Ventas,
    (SELECT COUNT(*) FROM [prc].[PriceListItems] pl WHERE pl.CompanyProductId = tc.CompanyProductId) AS C_Precios,
    (SELECT COUNT(*) FROM [cmp].[OrdenCompraLineas] ocl WHERE ocl.CompanyProductId = tc.CompanyProductId) AS C_OrdenCompraLineas,
    (SELECT COUNT(*) FROM [cmp].[OrdenPedidoLineas] opl WHERE opl.CompanyProductId = tc.CompanyProductId) AS C_OrdenPedidoLineasCmp,
    (SELECT COUNT(*) FROM [imp].[OrdenPedidoLineas] impl WHERE impl.CompanyProductId = tc.CompanyProductId) AS C_OrdenPedidoLineasImp,
    (SELECT COUNT(*) FROM [mdm].[CompanyProductFeatures] cpf WHERE cpf.CompanyProductId = tc.CompanyProductId) AS C_CompanyProductFeatures
INTO #TempUso
FROM #TempCruzados tc;

SELECT 
    CompanyProductId, EmpresaCompanyProduct, ProductId, EmpresaProduct, CommercialName,
    C_Stock, C_Movimientos, C_Ventas, C_Precios,
    C_OrdenCompraLineas, C_OrdenPedidoLineasCmp, C_OrdenPedidoLineasImp, C_CompanyProductFeatures,
    (C_Stock + C_Movimientos + C_Ventas + C_Precios + C_OrdenCompraLineas + 
     C_OrdenPedidoLineasCmp + C_OrdenPedidoLineasImp + C_CompanyProductFeatures) AS TotalDependencias,
    CASE 
        WHEN (C_Stock + C_Movimientos + C_Ventas + C_Precios + C_OrdenCompraLineas + 
              C_OrdenPedidoLineasCmp + C_OrdenPedidoLineasImp + C_CompanyProductFeatures) = 0 
        THEN 'SIN_USO_OPERATIVO'
        ELSE 'CON_USO_OPERATIVO' 
    END AS EstadoDependencias
FROM #TempUso;

DECLARE @SinUso INT = (SELECT COUNT(*) FROM #TempUso 
    WHERE (C_Stock + C_Movimientos + C_Ventas + C_Precios + C_OrdenCompraLineas + 
           C_OrdenPedidoLineasCmp + C_OrdenPedidoLineasImp + C_CompanyProductFeatures) = 0);
DECLARE @ConUso INT = @TotalCruzados - @SinUso;

PRINT 'Cruzados CON uso operativo: ' + CAST(@ConUso AS VARCHAR(20));
PRINT 'Cruzados SIN uso operativo: ' + CAST(@SinUso AS VARCHAR(20));


PRINT '==========================================================='
PRINT '3. AUDITORIA: METADATOS DE PRODUCTOS CRUZADOS'
PRINT '==========================================================='

SELECT 
    tc.ProductId,
    tc.EmpresaProduct,
    tc.CommercialName,
    (SELECT COUNT(*) FROM [mdm].[ProductAttributes] pa WHERE pa.ProductId = tc.ProductId) AS C_ProductAttributes,
    (SELECT COUNT(*) FROM [mdm].[ProductCategories] pc WHERE pc.ProductId = tc.ProductId) AS C_ProductCategories,
    (SELECT COUNT(*) FROM [mdm].[ProductClassificationLinks] pcl WHERE pcl.ProductId = tc.ProductId) AS C_ProductClassificationLinks,
    (SELECT COUNT(*) FROM [mdm].[ProductCodes] pco WHERE pco.ProductId = tc.ProductId) AS C_ProductCodes,
    (SELECT COUNT(*) FROM [mdm].[ProductUoms] pu WHERE pu.ProductId = tc.ProductId) AS C_ProductUoms,
    (SELECT COUNT(*) FROM [doc].[ProductDocuments] pd WHERE pd.ProductId = tc.ProductId) AS C_ProductDocuments
INTO #TempMetadatos
FROM #TempCruzados tc;

SELECT 
    ProductId, EmpresaProduct, CommercialName,
    C_ProductAttributes, C_ProductCategories, C_ProductClassificationLinks,
    C_ProductCodes, C_ProductUoms, C_ProductDocuments,
    (C_ProductAttributes + C_ProductCategories + C_ProductClassificationLinks + 
     C_ProductCodes + C_ProductUoms + C_ProductDocuments) AS TotalMetadatos
FROM #TempMetadatos
ORDER BY ProductId;

PRINT 'Auditoría de metadatos completada.';

PRINT '==========================================================='
PRINT '4. REPARACION: ESTRATEGIA A (CLONACION O USO DE PRODUCT EXISTENTE)'
PRINT '==========================================================='
-- Esta lógica recorrerá los registros cruzados.
-- Verificará si ya existe un Product equivalente en la empresa destino.
-- Si existe, usará ese ProductId.
-- Si no existe, clonará el Product original.

DECLARE @IdCP BIGINT;
DECLARE @IdP BIGINT;
DECLARE @EmpresaReal INT;
DECLARE @ProductKind TINYINT;
DECLARE @CommercialName NVARCHAR(180);
DECLARE @GenericName NVARCHAR(180);
DECLARE @DefaultUomId INT;

DECLARE RepairCursor CURSOR FOR 
SELECT CompanyProductId, ProductId, EmpresaCompanyProduct 
FROM #TempCruzados;

OPEN RepairCursor;
FETCH NEXT FROM RepairCursor INTO @IdCP, @IdP, @EmpresaReal;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @NewProductId BIGINT = 0;
    DECLARE @ExistingProductId BIGINT = NULL;

    -- Obtenemos características del producto original
    SELECT 
        @ProductKind = ProductKind,
        @CommercialName = CommercialName,
        @GenericName = GenericName,
        @DefaultUomId = DefaultUomId
    FROM [mdm].[Products]
    WHERE ProductId = @IdP;

    -- Buscamos si ya existe un Product equivalente en la empresa destino
    SELECT TOP 1 @ExistingProductId = ProductId
    FROM [mdm].[Products]
    WHERE EmpresaId = @EmpresaReal
      AND ProductKind = @ProductKind
      AND CommercialName = @CommercialName
      AND (GenericName = @GenericName OR (GenericName IS NULL AND @GenericName IS NULL))
      AND DefaultUomId = @DefaultUomId;

    IF @ExistingProductId IS NOT NULL
    BEGIN
        -- Usamos el producto existente
        SET @NewProductId = @ExistingProductId;
        PRINT 'CompanyProduct ID: ' + CAST(@IdCP AS VARCHAR(20)) + ' - Producto equivalente encontrado: Product ID: ' + CAST(@NewProductId AS VARCHAR(20));
    END
    ELSE
    BEGIN
        -- Insertamos clon del producto a la empresa del CP 
        INSERT INTO [mdm].[Products] (
            [EmpresaId], [CatalogId], [ProductKind], [GenericName], [CommercialName],
            [ShortDescription], [LongDescription], [BrandId], [ManufacturerId], [DefaultUomId],
            [IsStockable], [IsSellable], [IsPurchasable], [LifecycleStatusId],
            [FechaCreacion], [CreadoPor], [Activo]
        )
        SELECT 
            @EmpresaReal, [CatalogId], [ProductKind], [GenericName], [CommercialName],
            [ShortDescription], [LongDescription], [BrandId], [ManufacturerId], [DefaultUomId],
            [IsStockable], [IsSellable], [IsPurchasable], [LifecycleStatusId],
            SYSUTCDATETIME(), 'system-product-tenant-backfill', 1
        FROM [mdm].[Products]
        WHERE [ProductId] = @IdP;

        SET @NewProductId = SCOPE_IDENTITY();
        PRINT 'CompanyProduct ID: ' + CAST(@IdCP AS VARCHAR(20)) + ' - Producto clonado como nuevo: Product ID: ' + CAST(@NewProductId AS VARCHAR(20));
    END

    -- Actualizamos la referencia cruzada al producto seguro
    UPDATE [mdm].[CompanyProducts]
    SET 
        ProductId = @NewProductId,
        FechaModificacion = SYSUTCDATETIME(),
        ModificadoPor = 'system-product-tenant-backfill'
    WHERE CompanyProductId = @IdCP;

    FETCH NEXT FROM RepairCursor INTO @IdCP, @IdP, @EmpresaReal;
END

CLOSE RepairCursor;
DEALLOCATE RepairCursor;

PRINT 'Estrategia A completada: Referencias corregidas.';

PRINT 'Estrategia A completada: Referencias corregidas.';

PRINT '==========================================================='
PRINT '5. ESTRATEGIA B (OPCIONAL/COMENTADA): DESACTIVAR SIN USO'
PRINT '==========================================================='
/* 
-- Solo para CompanyProducts sin uso operativo
UPDATE cp
SET cp.Activo = 0,
    cp.FechaModificacion = SYSUTCDATETIME(),
    cp.ModificadoPor = 'system-product-tenant-backfill'
FROM [mdm].[CompanyProducts] cp
INNER JOIN #TempUso tu ON cp.CompanyProductId = tu.CompanyProductId
WHERE (tu.C_Stock + tu.C_Movimientos + tu.C_Ventas + tu.C_Precios + 
       tu.C_OrdenCompraLineas + tu.C_OrdenPedidoLineasCmp + 
       tu.C_OrdenPedidoLineasImp + tu.C_CompanyProductFeatures) = 0;

PRINT 'Estrategia B: CompanyProducts sin uso desactivados.';
*/

PRINT '==========================================================='
PRINT '6. CLONACION DE METADATOS (OPCIONAL/COMENTADA)'
PRINT '==========================================================='
/*
-- Si se decide clonar metadatos del producto original al producto clonado:
-- ADVERTENCIA: Esto puede duplicar información y debe evaluarse caso por caso.
-- Se recomienda ejecutar esta sección solo si se confirma que los metadatos deben ser copiados.

DECLARE @OriginalProductId BIGINT;
DECLARE @ClonedProductId BIGINT;

DECLARE MetadataCursor CURSOR FOR 
SELECT tc.ProductId, cp.ProductId AS NewProductId
FROM #TempCruzados tc
INNER JOIN [mdm].[CompanyProducts] cp ON tc.CompanyProductId = cp.CompanyProductId;

OPEN MetadataCursor;
FETCH NEXT FROM MetadataCursor INTO @OriginalProductId, @ClonedProductId;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Clonar ProductAttributes
    INSERT INTO [mdm].[ProductAttributes] 
        (ProductId, AttributeKey, AttributeValue, FechaCreacion, CreadoPor, Activo)
    SELECT @ClonedProductId, AttributeKey, AttributeValue, 
           SYSUTCDATETIME(), 'system-product-tenant-backfill', Activo
    FROM [mdm].[ProductAttributes]
    WHERE ProductId = @OriginalProductId;

    -- Clonar ProductCategories
    INSERT INTO [mdm].[ProductCategories] 
        (ProductId, CategoryId, IsPrimary, FechaCreacion, CreadoPor, Activo)
    SELECT @ClonedProductId, CategoryId, IsPrimary, 
           SYSUTCDATETIME(), 'system-product-tenant-backfill', Activo
    FROM [mdm].[ProductCategories]
    WHERE ProductId = @OriginalProductId;

    -- Clonar ProductClassificationLinks
    INSERT INTO [mdm].[ProductClassificationLinks] 
        (ProductId, ClassificationId, FechaCreacion, CreadoPor, Activo)
    SELECT @ClonedProductId, ClassificationId, 
           SYSUTCDATETIME(), 'system-product-tenant-backfill', Activo
    FROM [mdm].[ProductClassificationLinks]
    WHERE ProductId = @OriginalProductId;

    -- Clonar ProductCodes
    INSERT INTO [mdm].[ProductCodes] 
        (ProductId, CodeType, CodeValue, FechaCreacion, CreadoPor, Activo)
    SELECT @ClonedProductId, CodeType, CodeValue, 
           SYSUTCDATETIME(), 'system-product-tenant-backfill', Activo
    FROM [mdm].[ProductCodes]
    WHERE ProductId = @OriginalProductId;

    -- Clonar ProductUoms
    INSERT INTO [mdm].[ProductUoms] 
        (ProductId, UomId, ConversionFactor, FechaCreacion, CreadoPor, Activo)
    SELECT @ClonedProductId, UomId, ConversionFactor, 
           SYSUTCDATETIME(), 'system-product-tenant-backfill', Activo
    FROM [mdm].[ProductUoms]
    WHERE ProductId = @OriginalProductId;

    -- NO clonar ProductDocuments por seguridad (pueden referenciar archivos físicos)

    PRINT 'Metadatos clonados para Product ID: ' + CAST(@ClonedProductId AS VARCHAR(20));

    FETCH NEXT FROM MetadataCursor INTO @OriginalProductId, @ClonedProductId;
END

CLOSE MetadataCursor;
DEALLOCATE MetadataCursor;

PRINT 'Clonación de metadatos completada.';
*/

PRINT 'Clonación de metadatos completada.';
*/

PRINT '==========================================================='
PRINT '7. VERIFICACION POST-CORRECCION'
PRINT '==========================================================='

-- Verificamos si aún quedan CompanyProducts cruzados
SELECT COUNT(*) AS CompanyProductsCruzadosRestantes
FROM [mdm].[CompanyProducts] cp
INNER JOIN [mdm].[Products] p ON cp.ProductId = p.ProductId
WHERE cp.EmpresaId <> p.EmpresaId;

PRINT 'Verificación completada. Si el conteo es 0, la corrección fue exitosa.';

-- Limpieza de tablas temporales
DROP TABLE #TempCruzados;
DROP TABLE #TempUso;
DROP TABLE #TempMetadatos;

PRINT '==========================================================='
PRINT 'FIN DEL PROCESO. EJECUTANDO ROLLBACK POR SEGURIDAD.'
PRINT 'IMPORTANTE: Revise los resultados antes de ejecutar COMMIT.'
PRINT 'RECOMENDACION: Hacer backup de la base de datos antes de COMMIT.'
PRINT '==========================================================='

-- Mantenemos ROLLBACK como protección por defecto
ROLLBACK TRANSACTION;

-- Descomentar la siguiente línea solo después de verificar los resultados:
-- COMMIT TRANSACTION;

GO
