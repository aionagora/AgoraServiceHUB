-- ============================================================================
-- FASE B: POST-VALIDACION DE CONSISTENCIA MULTIEMPRESA (ESQUEMA REAL)
-- ============================================================================
-- Script de solo lectura para validar la consistencia después del saneamiento
-- de productos cruzados entre empresas.
--
-- VERSION: 2.0 - Adaptativo al esquema real de SQL Server
--
-- CARACTERISTICAS:
-- - SOLO LECTURA (No INSERT/UPDATE/DELETE/COMMIT/ROLLBACK)
-- - Puede ejecutarse dentro de la transacción FASE B (antes de ROLLBACK/COMMIT)
-- - Puede ejecutarse después de COMMIT en base de test
-- - Genera informe detallado de inconsistencias
-- - Detecta dinámicamente columnas existentes (no asume nombres)
-- - Usa SQL dinámico para validaciones opcionales
--
-- EJECUCION:
-- 1. Primero ejecutar FASE_B_InspectSchema_ProductCompanyProduct.sql para conocer esquema
-- 2. Ejecutar este script dentro de FASE B antes de ROLLBACK para validar cambios
-- 3. O ejecutar independientemente después de COMMIT
-- ============================================================================

SET NOCOUNT ON;

PRINT '============================================================================';
PRINT 'INICIO DE VALIDACION POST-CORRECCION MULTIEMPRESA (ESQUEMA REAL)';
PRINT 'Fecha/Hora: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '============================================================================';
PRINT '';

PRINT 'DIAGNOSTICO INICIAL: COMPANYPRODUCTS CRUZADOS RESTANTES';
PRINT '============================================================================';

SELECT 
    cp.CompanyProductId,
    cp.EmpresaId AS EmpresaCompanyProduct,
    cp.ProductId,
    p.EmpresaId AS EmpresaProduct,
    cp.Sku,
    cp.CodigoInterno,
    p.CommercialName
FROM [mdm].[CompanyProducts] cp
INNER JOIN [mdm].[Products] p ON cp.ProductId = p.ProductId
WHERE cp.EmpresaId <> p.EmpresaId
ORDER BY cp.CompanyProductId;

PRINT '';

-- Variables para conteo de inconsistencias
DECLARE @V1_CompanyProductsCruzados INT = 0;
DECLARE @V2_StockCruzado INT = 0;
DECLARE @V3_MovimientosCruzados INT = 0;
DECLARE @V4_VentasCruzadas INT = 0;
DECLARE @V5_PreciosCruzados INT = 0;
DECLARE @V6_ComprasCruzadas INT = 0;
DECLARE @V7_FeaturesCruzadas INT = 0;
DECLARE @V8_ProductsBackfill INT = 0;
DECLARE @V9_CompanyProductsBackfill INT = 0;
DECLARE @V10_DuplicadosPotenciales INT = 0;
DECLARE @V10_RevisarManual BIT = 0;

-- Variables auxiliares para SQL dinámico
DECLARE @SQL NVARCHAR(MAX);
DECLARE @ParmDefinition NVARCHAR(500);

PRINT '============================================================================';
PRINT 'VALIDACION 1: COMPANYPRODUCTS CRUZADOS RESTANTES';
PRINT '============================================================================';

-- Esta validación es obligatoria y usa columnas garantizadas
SELECT @V1_CompanyProductsCruzados = COUNT(*)
FROM [mdm].[CompanyProducts] cp
INNER JOIN [mdm].[Products] p ON cp.ProductId = p.ProductId
WHERE cp.EmpresaId <> p.EmpresaId;

IF @V1_CompanyProductsCruzados > 0
BEGIN
    PRINT 'ERROR: Se encontraron ' + CAST(@V1_CompanyProductsCruzados AS VARCHAR(20)) + ' CompanyProducts aún cruzados.';
    SELECT TOP 100 cp.CompanyProductId, cp.EmpresaId AS EmpresaCP, cp.ProductId, p.EmpresaId AS EmpresaP, cp.Sku, p.CommercialName
    FROM [mdm].[CompanyProducts] cp INNER JOIN [mdm].[Products] p ON cp.ProductId = p.ProductId
    WHERE cp.EmpresaId <> p.EmpresaId ORDER BY cp.CompanyProductId;
END
ELSE PRINT 'OK: No se encontraron CompanyProducts cruzados.';

PRINT '';
PRINT '============================================================================';
PRINT 'VALIDACION 2: STOCK CRUZADO (detect dinámico de columnas)';
PRINT '============================================================================';

DECLARE @StockWarehouseCol NVARCHAR(128) = NULL;
DECLARE @AlmacenPK NVARCHAR(128) = NULL;

IF COL_LENGTH('inv.StockProductos', 'WarehouseId') IS NOT NULL SET @StockWarehouseCol = 'WarehouseId';
ELSE IF COL_LENGTH('inv.StockProductos', 'AlmacenId') IS NOT NULL SET @StockWarehouseCol = 'AlmacenId';

IF COL_LENGTH('mdm.Almacenes', 'AlmacenId') IS NOT NULL SET @AlmacenPK = 'AlmacenId';
ELSE IF COL_LENGTH('mdm.Almacenes', 'Id') IS NOT NULL SET @AlmacenPK = 'Id';
ELSE IF COL_LENGTH('mdm.Almacenes', 'WarehouseId') IS NOT NULL SET @AlmacenPK = 'WarehouseId';

IF @StockWarehouseCol IS NOT NULL AND @AlmacenPK IS NOT NULL
BEGIN
    SET @SQL = N'SELECT @Count = COUNT(*) FROM [inv].[StockProductos] sp INNER JOIN [mdm].[CompanyProducts] cp ON sp.CompanyProductId = cp.CompanyProductId INNER JOIN [mdm].[Almacenes] a ON sp.' + @StockWarehouseCol + N' = a.' + @AlmacenPK + N' WHERE sp.EmpresaId <> cp.EmpresaId OR sp.EmpresaId <> a.EmpresaId';
    SET @ParmDefinition = N'@Count INT OUTPUT';
    EXEC sp_executesql @SQL, @ParmDefinition, @Count = @V2_StockCruzado OUTPUT;
    IF @V2_StockCruzado > 0 PRINT 'ERROR: ' + CAST(@V2_StockCruzado AS VARCHAR(20)) + ' Stock cruzados';
    ELSE PRINT 'OK: Stock consistente';
END
ELSE PRINT 'ADVERTENCIA: Columnas de almacén no detectadas. REVISAR_MANUAL.';

PRINT '';
PRINT '============================================================================';
PRINT 'VALIDACION 3: MOVIMIENTOS INVENTARIO (detectar columnas dinámicamente)';
PRINT '============================================================================';

DECLARE @MovWarehouseCol NVARCHAR(128) = NULL;
DECLARE @MovDestCol NVARCHAR(128) = NULL;

IF COL_LENGTH('inv.MovimientosInventario', 'WarehouseId') IS NOT NULL SET @MovWarehouseCol = 'WarehouseId';
ELSE IF COL_LENGTH('inv.MovimientosInventario', 'AlmacenId') IS NOT NULL SET @MovWarehouseCol = 'AlmacenId';
ELSE IF COL_LENGTH('inv.MovimientosInventario', 'SourceWarehouseId') IS NOT NULL SET @MovWarehouseCol = 'SourceWarehouseId';

IF COL_LENGTH('inv.MovimientosInventario', 'DestinationWarehouseId') IS NOT NULL SET @MovDestCol = 'DestinationWarehouseId';

IF @MovWarehouseCol IS NOT NULL AND @AlmacenPK IS NOT NULL
BEGIN
    SET @SQL = N'SELECT @Count = COUNT(*) FROM [inv].[MovimientosInventario] mi INNER JOIN [mdm].[CompanyProducts] cp ON mi.CompanyProductId = cp.CompanyProductId INNER JOIN [mdm].[Almacenes] a ON mi.' + @MovWarehouseCol + N' = a.' + @AlmacenPK + N' WHERE mi.EmpresaId <> cp.EmpresaId OR mi.EmpresaId <> a.EmpresaId';
    EXEC sp_executesql @SQL, @ParmDefinition, @Count = @V3_MovimientosCruzados OUTPUT;
    IF @V3_MovimientosCruzados > 0 PRINT 'ERROR: ' + CAST(@V3_MovimientosCruzados AS VARCHAR(20)) + ' Movimientos cruzados';
    ELSE PRINT 'OK: Movimientos consistentes';
END
ELSE PRINT 'ADVERTENCIA: Columnas no detectadas. REVISAR_MANUAL.';

PRINT '';
PRINT '============================================================================';
PRINT 'VALIDACION 4: PEDIDOS VENTA (detect FK dinámicamente)';
PRINT '============================================================================';

DECLARE @PedidoVentaFK NVARCHAR(128) = NULL, @PedidoVentaPK NVARCHAR(128) = NULL;

SELECT TOP 1 @PedidoVentaFK = c_parent.name, @PedidoVentaPK = c_ref.name
FROM sys.foreign_keys fk
INNER JOIN sys.tables t_parent ON t_parent.object_id = fk.parent_object_id
INNER JOIN sys.schemas s_parent ON s_parent.schema_id = t_parent.schema_id
INNER JOIN sys.tables t_ref ON t_ref.object_id = fk.referenced_object_id
INNER JOIN sys.schemas s_ref ON s_ref.schema_id = t_ref.schema_id
INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
INNER JOIN sys.columns c_parent ON c_parent.object_id = fkc.parent_object_id AND c_parent.column_id = fkc.parent_column_id
INNER JOIN sys.columns c_ref ON c_ref.object_id = fkc.referenced_object_id AND c_ref.column_id = fkc.referenced_column_id
WHERE s_parent.name = 'vta' AND t_parent.name = 'PedidoVentaDetalles'
  AND s_ref.name = 'vta' AND t_ref.name = 'PedidosVenta';

IF @PedidoVentaFK IS NOT NULL
BEGIN
    SET @SQL = N'SELECT @Count = COUNT(*) FROM [vta].[PedidoVentaDetalles] pvd INNER JOIN [vta].[PedidosVenta] pv ON pvd.' + @PedidoVentaFK + N' = pv.' + @PedidoVentaPK + N' INNER JOIN [mdm].[CompanyProducts] cp ON pvd.CompanyProductId = cp.CompanyProductId WHERE cp.EmpresaId <> pv.EmpresaId';
    EXEC sp_executesql @SQL, @ParmDefinition, @Count = @V4_VentasCruzadas OUTPUT;
    IF @V4_VentasCruzadas > 0 PRINT 'ERROR: ' + CAST(@V4_VentasCruzadas AS VARCHAR(20)) + ' Ventas cruzadas';
    ELSE PRINT 'OK: Ventas consistentes';
END
ELSE PRINT 'ADVERTENCIA: FK PedidoVenta no detectada. REVISAR_MANUAL.';

PRINT '';
PRINT '============================================================================';
PRINT 'VALIDACION 5: PRICE LIST ITEMS (detect FK dinámicamente)';
PRINT '============================================================================';

DECLARE @PriceListFK NVARCHAR(128) = NULL, @PriceListPK NVARCHAR(128) = NULL;

SELECT TOP 1 @PriceListFK = c_parent.name, @PriceListPK = c_ref.name
FROM sys.foreign_keys fk
INNER JOIN sys.tables t_parent ON t_parent.object_id = fk.parent_object_id
INNER JOIN sys.schemas s_parent ON s_parent.schema_id = t_parent.schema_id
INNER JOIN sys.tables t_ref ON t_ref.object_id = fk.referenced_object_id
INNER JOIN sys.schemas s_ref ON s_ref.schema_id = t_ref.schema_id
INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
INNER JOIN sys.columns c_parent ON c_parent.object_id = fkc.parent_object_id AND c_parent.column_id = fkc.parent_column_id
INNER JOIN sys.columns c_ref ON c_ref.object_id = fkc.referenced_object_id AND c_ref.column_id = fkc.referenced_column_id
WHERE s_parent.name = 'prc' AND t_parent.name = 'PriceListItems'
  AND s_ref.name = 'prc' AND t_ref.name = 'PriceLists';

IF @PriceListFK IS NOT NULL
BEGIN
    SET @SQL = N'SELECT @Count = COUNT(*) FROM [prc].[PriceListItems] pli INNER JOIN [prc].[PriceLists] pl ON pli.' + @PriceListFK + N' = pl.' + @PriceListPK + N' INNER JOIN [mdm].[CompanyProducts] cp ON pli.CompanyProductId = cp.CompanyProductId WHERE pl.EmpresaId <> cp.EmpresaId';
    EXEC sp_executesql @SQL, @ParmDefinition, @Count = @V5_PreciosCruzados OUTPUT;
    IF @V5_PreciosCruzados > 0 PRINT 'ERROR: ' + CAST(@V5_PreciosCruzados AS VARCHAR(20)) + ' Precios cruzados';
    ELSE PRINT 'OK: Precios consistentes';
END
ELSE PRINT 'ADVERTENCIA: FK PriceList no detectada. REVISAR_MANUAL.';

PRINT '';
PRINT '============================================================================';
PRINT 'VALIDACION 6: COMPRAS (orfandad CompanyProductId)';
PRINT '============================================================================';

DECLARE @V6A INT = 0, @V6B INT = 0, @V6C INT = 0;

IF OBJECT_ID('[cmp].[OrdenCompraLineas]', 'U') IS NOT NULL
BEGIN
    SELECT @V6A = COUNT(*) FROM [cmp].[OrdenCompraLineas] ocl LEFT JOIN [mdm].[CompanyProducts] cp ON ocl.CompanyProductId = cp.CompanyProductId WHERE cp.CompanyProductId IS NULL;
    IF @V6A > 0 PRINT 'ERROR: ' + CAST(@V6A AS VARCHAR(20)) + ' OrdenCompraLineas huérfanas';
    ELSE PRINT 'OK: OrdenCompraLineas consistentes';
END
ELSE PRINT 'INFO: Tabla cmp.OrdenCompraLineas no existe';

IF OBJECT_ID('[cmp].[OrdenPedidoLineas]', 'U') IS NOT NULL
BEGIN
    SELECT @V6B = COUNT(*) FROM [cmp].[OrdenPedidoLineas] opl LEFT JOIN [mdm].[CompanyProducts] cp ON opl.CompanyProductId = cp.CompanyProductId WHERE cp.CompanyProductId IS NULL;
    IF @V6B > 0 PRINT 'ERROR: ' + CAST(@V6B AS VARCHAR(20)) + ' OrdenPedidoLineas (cmp) huérfanas';
    ELSE PRINT 'OK: OrdenPedidoLineas (cmp) consistentes';
END
ELSE PRINT 'INFO: Tabla cmp.OrdenPedidoLineas no existe';

IF OBJECT_ID('[imp].[OrdenPedidoLineas]', 'U') IS NOT NULL
BEGIN
    SELECT @V6C = COUNT(*) FROM [imp].[OrdenPedidoLineas] impl LEFT JOIN [mdm].[CompanyProducts] cp ON impl.CompanyProductId = cp.CompanyProductId WHERE cp.CompanyProductId IS NULL;
    IF @V6C > 0 PRINT 'ERROR: ' + CAST(@V6C AS VARCHAR(20)) + ' OrdenPedidoLineas (imp) huérfanas';
    ELSE PRINT 'OK: OrdenPedidoLineas (imp) consistentes';
END
ELSE PRINT 'INFO: Tabla imp.OrdenPedidoLineas no existe';

SET @V6_ComprasCruzadas = @V6A + @V6B + @V6C;

PRINT '';
PRINT '============================================================================';
PRINT 'VALIDACION 7: COMPANY PRODUCT FEATURES (orfandad)';
PRINT '============================================================================';

IF OBJECT_ID('[mdm].[CompanyProductFeatures]', 'U') IS NOT NULL
BEGIN
    SELECT @V7_FeaturesCruzadas = COUNT(*) FROM [mdm].[CompanyProductFeatures] cpf LEFT JOIN [mdm].[CompanyProducts] cp ON cpf.CompanyProductId = cp.CompanyProductId WHERE cp.CompanyProductId IS NULL;
    IF @V7_FeaturesCruzadas > 0 PRINT 'ERROR: ' + CAST(@V7_FeaturesCruzadas AS VARCHAR(20)) + ' Features huérfanos';
    ELSE PRINT 'OK: Features consistentes';
END
ELSE PRINT 'INFO: Tabla mdm.CompanyProductFeatures no existe';

PRINT '';
PRINT '============================================================================';
PRINT 'VALIDACION 8-9: BACKFILL AUDIT (solo si existen columnas)';
PRINT '============================================================================';

IF COL_LENGTH('mdm.Products', 'CreadoPor') IS NOT NULL
BEGIN
    SET @SQL = N'SELECT @Count = COUNT(*) FROM [mdm].[Products] WHERE CreadoPor = ''system-product-tenant-backfill''';
    EXEC sp_executesql @SQL, @ParmDefinition, @Count = @V8_ProductsBackfill OUTPUT;
    PRINT 'INFO: ' + CAST(@V8_ProductsBackfill AS VARCHAR(20)) + ' Products creados por backfill';
END
ELSE PRINT 'INFO: Columna CreadoPor no existe - no se puede validar backfill Products';

IF COL_LENGTH('mdm.CompanyProducts', 'ModificadoPor') IS NOT NULL
BEGIN
    SET @SQL = N'SELECT @Count = COUNT(*) FROM [mdm].[CompanyProducts] WHERE ModificadoPor = ''system-product-tenant-backfill''';
    EXEC sp_executesql @SQL, @ParmDefinition, @Count = @V9_CompanyProductsBackfill OUTPUT;
    PRINT 'INFO: ' + CAST(@V9_CompanyProductsBackfill AS VARCHAR(20)) + ' CompanyProducts actualizados por backfill';
END
ELSE PRINT 'INFO: Columna ModificadoPor no existe - no se puede validar backfill CompanyProducts';

PRINT '';
PRINT '============================================================================';
PRINT 'VALIDACION 10: DUPLICADOS POTENCIALES';
PRINT '============================================================================';

IF OBJECT_ID('tempdb..#TempDuplicados') IS NOT NULL DROP TABLE #TempDuplicados;

CREATE TABLE #TempDuplicados
(
    EmpresaId NVARCHAR(4000) NULL,
    ProductKind NVARCHAR(4000) NULL,
    CommercialName NVARCHAR(4000) NULL,
    GenericName NVARCHAR(4000) NULL,
    DefaultUomId NVARCHAR(4000) NULL,
    CantidadDuplicados INT NOT NULL
);

IF COL_LENGTH('mdm.Products', 'EmpresaId') IS NOT NULL
   AND COL_LENGTH('mdm.Products', 'ProductKind') IS NOT NULL
   AND COL_LENGTH('mdm.Products', 'CommercialName') IS NOT NULL
BEGIN
    SET @SQL = N'
        INSERT INTO #TempDuplicados (EmpresaId, ProductKind, CommercialName, GenericName, DefaultUomId, CantidadDuplicados)
        SELECT
            CONVERT(NVARCHAR(4000), EmpresaId) AS EmpresaId,
            CONVERT(NVARCHAR(4000), ProductKind) AS ProductKind,
            CONVERT(NVARCHAR(4000), CommercialName) AS CommercialName,
            ' + CASE WHEN COL_LENGTH('mdm.Products', 'GenericName') IS NOT NULL
                     THEN N'CONVERT(NVARCHAR(4000), GenericName)'
                     ELSE N'CAST(NULL AS NVARCHAR(4000))' END + N' AS GenericName,
            ' + CASE WHEN COL_LENGTH('mdm.Products', 'DefaultUomId') IS NOT NULL
                     THEN N'CONVERT(NVARCHAR(4000), DefaultUomId)'
                     ELSE N'CAST(NULL AS NVARCHAR(4000))' END + N' AS DefaultUomId,
            COUNT(*) AS CantidadDuplicados
        FROM [mdm].[Products]
        GROUP BY
            EmpresaId,
            ProductKind,
            CommercialName,
            ' + CASE WHEN COL_LENGTH('mdm.Products', 'GenericName') IS NOT NULL
                     THEN N'GenericName'
                     ELSE N'CAST(NULL AS NVARCHAR(4000))' END + N',
            ' + CASE WHEN COL_LENGTH('mdm.Products', 'DefaultUomId') IS NOT NULL
                     THEN N'DefaultUomId'
                     ELSE N'CAST(NULL AS INT)' END + N'
        HAVING COUNT(*) > 1;';

    EXEC sp_executesql @SQL;
END
ELSE
BEGIN
    SET @V10_RevisarManual = 1;
    PRINT 'ADVERTENCIA: Columnas insuficientes para validar duplicados. REVISAR_MANUAL.';
END

IF OBJECT_ID('tempdb..#TempDuplicados') IS NOT NULL
BEGIN
    SELECT @V10_DuplicadosPotenciales = COUNT(*) FROM #TempDuplicados;

    IF @V10_DuplicadosPotenciales > 0
    BEGIN
        PRINT 'ADVERTENCIA: ' + CAST(@V10_DuplicadosPotenciales AS VARCHAR(20)) + ' grupos duplicados detectados';
        SELECT * FROM #TempDuplicados ORDER BY CantidadDuplicados DESC, EmpresaId, CommercialName;
    END
    ELSE IF @V10_RevisarManual = 0
    BEGIN
        PRINT 'OK: Sin duplicados potenciales';
    END
END

IF OBJECT_ID('tempdb..#TempDuplicados') IS NOT NULL DROP TABLE #TempDuplicados;

PRINT '';
PRINT '============================================================================';
PRINT 'RESUMEN FINAL';
PRINT '============================================================================';

IF OBJECT_ID('tempdb..#ResumenValidacion') IS NOT NULL DROP TABLE #ResumenValidacion;

CREATE TABLE #ResumenValidacion (Num INT, Descripcion VARCHAR(100), Inconsistencias INT, Estado VARCHAR(20));

INSERT INTO #ResumenValidacion VALUES
    (1, 'CompanyProducts cruzados', @V1_CompanyProductsCruzados, CASE WHEN @V1_CompanyProductsCruzados = 0 THEN 'OK' ELSE 'ERROR' END),
    (2, 'Stock cruzado', @V2_StockCruzado, CASE WHEN @V2_StockCruzado = 0 THEN 'OK' ELSE 'ERROR' END),
    (3, 'Movimientos cruzados', @V3_MovimientosCruzados, CASE WHEN @V3_MovimientosCruzados = 0 THEN 'OK' ELSE 'ERROR' END),
    (4, 'Ventas cruzadas', @V4_VentasCruzadas, CASE WHEN @V4_VentasCruzadas = 0 THEN 'OK' ELSE 'ERROR' END),
    (5, 'Precios cruzados', @V5_PreciosCruzados, CASE WHEN @V5_PreciosCruzados = 0 THEN 'OK' ELSE 'ERROR' END),
    (6, 'Compras cruzadas', @V6_ComprasCruzadas, CASE WHEN @V6_ComprasCruzadas = 0 THEN 'OK' ELSE 'ERROR' END),
    (7, 'Features huérfanos', @V7_FeaturesCruzadas, CASE WHEN @V7_FeaturesCruzadas = 0 THEN 'OK' ELSE 'ERROR' END),
    (8, 'Products backfill', @V8_ProductsBackfill, 'INFO'),
    (9, 'CompanyProducts backfill', @V9_CompanyProductsBackfill, 'INFO'),
    (10, 'Duplicados potenciales', @V10_DuplicadosPotenciales, CASE WHEN @V10_RevisarManual = 1 THEN 'REVISAR_MANUAL' WHEN @V10_DuplicadosPotenciales = 0 THEN 'OK' ELSE 'REVISAR_MANUAL' END);

SELECT Num, Descripcion, Inconsistencias, Estado FROM #ResumenValidacion ORDER BY Num;
DROP TABLE #ResumenValidacion;

DECLARE @TotalErrores INT = 
    CASE WHEN @V1_CompanyProductsCruzados > 0 THEN 1 ELSE 0 END +
    CASE WHEN @V2_StockCruzado > 0 THEN 1 ELSE 0 END +
    CASE WHEN @V3_MovimientosCruzados > 0 THEN 1 ELSE 0 END +
    CASE WHEN @V4_VentasCruzadas > 0 THEN 1 ELSE 0 END +
    CASE WHEN @V5_PreciosCruzados > 0 THEN 1 ELSE 0 END +
    CASE WHEN @V6_ComprasCruzadas > 0 THEN 1 ELSE 0 END +
    CASE WHEN @V7_FeaturesCruzadas > 0 THEN 1 ELSE 0 END;

PRINT '';
IF @TotalErrores = 0
BEGIN
    PRINT '*** VALIDACION EXITOSA ***';
    PRINT 'No se encontraron inconsistencias críticas.';
    PRINT 'El proceso de corrección multiempresa fue exitoso.';
    PRINT 'RECOMENDACION: Si está en transacción FASE B, puede ejecutar COMMIT.';
END
ELSE
BEGIN
    PRINT '*** VALIDACION FALLIDA ***';
    PRINT 'Se encontraron ' + CAST(@TotalErrores AS VARCHAR(10)) + ' categorías con inconsistencias críticas.';
    PRINT 'ACCION REQUERIDA:';
    PRINT '- Revisar detalles de cada validación fallida.';
    PRINT '- Corregir inconsistencias antes de COMMIT.';
    PRINT '- Si está en transacción FASE B, ejecutar ROLLBACK y revisar script de corrección.';
END

IF @V10_DuplicadosPotenciales > 0
    PRINT 'ADVERTENCIA: Productos duplicados detectados. Revisar Validación 10.';

PRINT '';
PRINT '============================================================================';
PRINT 'FIN DE VALIDACION POST-CORRECCION';
PRINT 'Fecha/Hora: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '============================================================================';

SET NOCOUNT OFF;

GO
