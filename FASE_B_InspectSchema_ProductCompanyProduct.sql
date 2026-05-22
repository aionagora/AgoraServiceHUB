-- ============================================================================
-- SCRIPT: FASE_B_InspectSchema_ProductCompanyProduct.sql
-- PROPOSITO: Inspeccionar esquema real de SQL Server para tablas relacionadas
--            con Products, CompanyProducts y sus dependencias operativas.
-- TIPO: SOLO LECTURA (SELECT)
-- AUTOR: Senior SQL Server DBA + EF Core Architect
-- FECHA: 2025-01-XX
-- ============================================================================

-- CONTEXTO:
-- El script FASE_B_PostValidacion_Cruzados.sql asumió nombres de columnas 
-- que no existen en el esquema real. Este script descubre la estructura real
-- para corregir las validaciones.

-- ALCANCE:
-- Listar columnas y FKs reales de tablas relacionadas con:
-- - Products / CompanyProducts
-- - Stock / Movimientos de Inventario
-- - Ventas / Compras
-- - Price Lists
-- - Metadatos de productos

SET NOCOUNT ON;

PRINT '============================================================================';
PRINT 'INSPECCION DE ESQUEMA: PRODUCTS / COMPANYPRODUCTS Y DEPENDENCIAS';
PRINT '============================================================================';
PRINT 'Fecha/Hora: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '';

-- ============================================================================
-- PARTE 1: COLUMNAS DE TABLAS RELEVANTES
-- ============================================================================

PRINT '============================================================================';
PRINT 'PARTE 1: COLUMNAS DE TABLAS';
PRINT '============================================================================';
PRINT '';

-- Crear tabla temporal para almacenar columnas
IF OBJECT_ID('tempdb..#TempColumns') IS NOT NULL DROP TABLE #TempColumns;

CREATE TABLE #TempColumns (
    SchemaName NVARCHAR(128),
    TableName NVARCHAR(128),
    ColumnName NVARCHAR(128),
    DataType NVARCHAR(128),
    MaxLength INT,
    IsNullable BIT,
    IsPrimaryKey BIT,
    OrdinalPosition INT
);

-- Tablas a inspeccionar
DECLARE @TablesToInspect TABLE (SchemaName NVARCHAR(128), TableName NVARCHAR(128));

INSERT INTO @TablesToInspect (SchemaName, TableName) VALUES
    ('mdm', 'Products'),
    ('mdm', 'CompanyProducts'),
    ('inv', 'StockProductos'),
    ('inv', 'MovimientosInventario'),
    ('vta', 'PedidosVenta'),
    ('vta', 'PedidoVentaDetalles'),
    ('prc', 'PriceLists'),
    ('prc', 'PriceListItems'),
    ('cmp', 'OrdenCompraLineas'),
    ('cmp', 'OrdenPedidoLineas'),
    ('imp', 'OrdenPedidoLineas'),
    ('mdm', 'CompanyProductFeatures'),
    ('mdm', 'ProductAttributes'),
    ('mdm', 'ProductCategories'),
    ('mdm', 'ProductClassificationLinks'),
    ('mdm', 'ProductCodes'),
    ('mdm', 'ProductUoms'),
    ('doc', 'ProductDocuments'),
    ('mdm', 'Almacenes');

-- Recopilar información de columnas
INSERT INTO #TempColumns (SchemaName, TableName, ColumnName, DataType, MaxLength, IsNullable, IsPrimaryKey, OrdinalPosition)
SELECT 
    s.name AS SchemaName,
    t.name AS TableName,
    c.name AS ColumnName,
    ty.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable,
    CASE WHEN pk.column_id IS NOT NULL THEN 1 ELSE 0 END AS IsPrimaryKey,
    c.column_id AS OrdinalPosition
FROM @TablesToInspect ti
INNER JOIN sys.schemas s ON s.name = ti.SchemaName
INNER JOIN sys.tables t ON t.name = ti.TableName AND t.schema_id = s.schema_id
INNER JOIN sys.columns c ON c.object_id = t.object_id
INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
LEFT JOIN (
    SELECT ic.object_id, ic.column_id
    FROM sys.index_columns ic
    INNER JOIN sys.indexes i ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    WHERE i.is_primary_key = 1
) pk ON pk.object_id = t.object_id AND pk.column_id = c.column_id
ORDER BY s.name, t.name, c.column_id;

-- Mostrar columnas agrupadas por tabla
DECLARE @CurrentSchema NVARCHAR(128) = '';
DECLARE @CurrentTable NVARCHAR(128) = '';

DECLARE cursor_columns CURSOR FOR 
    SELECT SchemaName, TableName, ColumnName, DataType, MaxLength, IsNullable, IsPrimaryKey, OrdinalPosition
    FROM #TempColumns
    ORDER BY SchemaName, TableName, OrdinalPosition;

DECLARE @Schema NVARCHAR(128), @Table NVARCHAR(128), @Column NVARCHAR(128), 
        @DataType NVARCHAR(128), @MaxLen INT, @IsNull BIT, @IsPK BIT, @OrdPos INT;

OPEN cursor_columns;
FETCH NEXT FROM cursor_columns INTO @Schema, @Table, @Column, @DataType, @MaxLen, @IsNull, @IsPK, @OrdPos;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Encabezado de tabla si cambió
    IF @CurrentSchema <> @Schema OR @CurrentTable <> @Table
    BEGIN
        IF @CurrentTable <> ''
            PRINT '';

        PRINT '----------------------------------------------------------------------------';
        PRINT 'Tabla: [' + @Schema + '].[' + @Table + ']';
        PRINT '----------------------------------------------------------------------------';

        SET @CurrentSchema = @Schema;
        SET @CurrentTable = @Table;
    END

    -- Imprimir columna
    DECLARE @Line NVARCHAR(500);
    SET @Line = '  ' + CAST(@OrdPos AS VARCHAR(3)) + '. ' + @Column + 
                ' (' + @DataType + 
                CASE 
                    WHEN @DataType IN ('nvarchar', 'varchar', 'char', 'nchar') THEN 
                        '(' + CASE WHEN @MaxLen = -1 THEN 'MAX' ELSE CAST(@MaxLen AS VARCHAR(10)) END + ')'
                    ELSE ''
                END + ')' +
                CASE WHEN @IsNull = 1 THEN ' NULL' ELSE ' NOT NULL' END +
                CASE WHEN @IsPK = 1 THEN ' [PK]' ELSE '' END;

    PRINT @Line;

    FETCH NEXT FROM cursor_columns INTO @Schema, @Table, @Column, @DataType, @MaxLen, @IsNull, @IsPK, @OrdPos;
END

CLOSE cursor_columns;
DEALLOCATE cursor_columns;

PRINT '';
PRINT 'Columnas listadas correctamente.';
PRINT '';

-- ============================================================================
-- PARTE 2: FOREIGN KEYS
-- ============================================================================

PRINT '============================================================================';
PRINT 'PARTE 2: FOREIGN KEYS (Relaciones entre tablas)';
PRINT '============================================================================';
PRINT '';

-- Crear tabla temporal para FKs
IF OBJECT_ID('tempdb..#TempFKs') IS NOT NULL DROP TABLE #TempFKs;

CREATE TABLE #TempFKs (
    ForeignKeyName NVARCHAR(128),
    ParentSchema NVARCHAR(128),
    ParentTable NVARCHAR(128),
    ParentColumn NVARCHAR(128),
    ReferencedSchema NVARCHAR(128),
    ReferencedTable NVARCHAR(128),
    ReferencedColumn NVARCHAR(128)
);

-- Recopilar FKs
INSERT INTO #TempFKs
SELECT 
    fk.name AS ForeignKeyName,
    s_parent.name AS ParentSchema,
    t_parent.name AS ParentTable,
    c_parent.name AS ParentColumn,
    s_ref.name AS ReferencedSchema,
    t_ref.name AS ReferencedTable,
    c_ref.name AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.tables t_parent ON t_parent.object_id = fk.parent_object_id
INNER JOIN sys.schemas s_parent ON s_parent.schema_id = t_parent.schema_id
INNER JOIN sys.tables t_ref ON t_ref.object_id = fk.referenced_object_id
INNER JOIN sys.schemas s_ref ON s_ref.schema_id = t_ref.schema_id
INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
INNER JOIN sys.columns c_parent ON c_parent.object_id = fkc.parent_object_id AND c_parent.column_id = fkc.parent_column_id
INNER JOIN sys.columns c_ref ON c_ref.object_id = fkc.referenced_object_id AND c_ref.column_id = fkc.referenced_column_id
WHERE 
    (s_parent.name IN ('mdm', 'inv', 'vta', 'prc', 'cmp', 'imp', 'doc') 
     AND t_parent.name IN (SELECT TableName FROM @TablesToInspect WHERE SchemaName = s_parent.name))
    OR
    (s_ref.name IN ('mdm', 'inv', 'vta', 'prc', 'cmp', 'imp', 'doc') 
     AND t_ref.name IN (SELECT TableName FROM @TablesToInspect WHERE SchemaName = s_ref.name))
ORDER BY s_parent.name, t_parent.name, fk.name;

-- Mostrar FKs
PRINT '----------------------------------------------------------------------------';
PRINT 'Foreign Keys detectadas:';
PRINT '----------------------------------------------------------------------------';

SELECT 
    ForeignKeyName,
    ParentSchema + '.' + ParentTable + '.' + ParentColumn AS [From],
    ReferencedSchema + '.' + ReferencedTable + '.' + ReferencedColumn AS [To]
FROM #TempFKs
ORDER BY ParentSchema, ParentTable, ForeignKeyName;

PRINT '';
PRINT 'Foreign Keys listadas correctamente.';
PRINT '';

-- ============================================================================
-- PARTE 3: DETECCION DE COLUMNAS CRITICAS PARA VALIDACION
-- ============================================================================

PRINT '============================================================================';
PRINT 'PARTE 3: COLUMNAS CRITICAS PARA VALIDACION';
PRINT '============================================================================';
PRINT '';

-- StockProductos: columna de almacén
PRINT '--- StockProductos: Columna de Almacén ---';
IF EXISTS (SELECT 1 FROM #TempColumns WHERE SchemaName = 'inv' AND TableName = 'StockProductos' AND ColumnName = 'AlmacenId')
    PRINT '  ✓ Detectada: AlmacenId';
ELSE IF EXISTS (SELECT 1 FROM #TempColumns WHERE SchemaName = 'inv' AND TableName = 'StockProductos' AND ColumnName = 'WarehouseId')
    PRINT '  ✓ Detectada: WarehouseId';
ELSE IF EXISTS (SELECT 1 FROM #TempColumns WHERE SchemaName = 'inv' AND TableName = 'StockProductos' AND ColumnName = 'BodegaId')
    PRINT '  ✓ Detectada: BodegaId';
ELSE
    PRINT '  ✗ No detectada (AlmacenId/WarehouseId/BodegaId)';
PRINT '';

-- MovimientosInventario: columnas de almacén
PRINT '--- MovimientosInventario: Columnas de Almacén ---';
IF EXISTS (SELECT 1 FROM #TempColumns WHERE SchemaName = 'inv' AND TableName = 'MovimientosInventario' AND ColumnName = 'AlmacenId')
    PRINT '  ✓ AlmacenId';
IF EXISTS (SELECT 1 FROM #TempColumns WHERE SchemaName = 'inv' AND TableName = 'MovimientosInventario' AND ColumnName = 'WarehouseId')
    PRINT '  ✓ WarehouseId';
IF EXISTS (SELECT 1 FROM #TempColumns WHERE SchemaName = 'inv' AND TableName = 'MovimientosInventario' AND ColumnName = 'SourceWarehouseId')
    PRINT '  ✓ SourceWarehouseId';
IF EXISTS (SELECT 1 FROM #TempColumns WHERE SchemaName = 'inv' AND TableName = 'MovimientosInventario' AND ColumnName = 'DestinationWarehouseId')
    PRINT '  ✓ DestinationWarehouseId';
PRINT '';

-- PedidoVentaDetalles: FK hacia cabecera
PRINT '--- PedidoVentaDetalles: FK hacia PedidosVenta ---';
SELECT 
    '  ✓ ' + ParentColumn + ' -> ' + ReferencedSchema + '.' + ReferencedTable + '.' + ReferencedColumn AS FK
FROM #TempFKs
WHERE ParentSchema = 'vta' AND ParentTable = 'PedidoVentaDetalles'
  AND ReferencedSchema = 'vta' AND ReferencedTable = 'PedidosVenta';
PRINT '';

-- Columnas de auditoría
PRINT '--- Columnas de Auditoría ---';
IF EXISTS (SELECT 1 FROM #TempColumns WHERE SchemaName = 'mdm' AND TableName = 'Products' AND ColumnName = 'CreadoPor')
    PRINT '  ✓ mdm.Products.CreadoPor existe';
ELSE
    PRINT '  ✗ mdm.Products.CreadoPor NO existe';

IF EXISTS (SELECT 1 FROM #TempColumns WHERE SchemaName = 'mdm' AND TableName = 'CompanyProducts' AND ColumnName = 'ModificadoPor')
    PRINT '  ✓ mdm.CompanyProducts.ModificadoPor existe';
ELSE
    PRINT '  ✗ mdm.CompanyProducts.ModificadoPor NO existe';
PRINT '';

-- CompanyProductFeatures: estructura
PRINT '--- CompanyProductFeatures: Estructura ---';
IF EXISTS (SELECT 1 FROM sys.tables t INNER JOIN sys.schemas s ON s.schema_id = t.schema_id WHERE s.name = 'mdm' AND t.name = 'CompanyProductFeatures')
BEGIN
    SELECT '  ✓ ' + ColumnName + ' (' + DataType + ')' AS Columna
    FROM #TempColumns
    WHERE SchemaName = 'mdm' AND TableName = 'CompanyProductFeatures'
    ORDER BY OrdinalPosition;
END
ELSE
    PRINT '  ✗ Tabla mdm.CompanyProductFeatures NO existe';
PRINT '';

-- ============================================================================
-- PARTE 4: RESUMEN
-- ============================================================================

PRINT '============================================================================';
PRINT 'RESUMEN DE INSPECCION';
PRINT '============================================================================';
PRINT '';

DECLARE @TotalTables INT = (SELECT COUNT(DISTINCT SchemaName + '.' + TableName) FROM #TempColumns);
DECLARE @TotalColumns INT = (SELECT COUNT(*) FROM #TempColumns);
DECLARE @TotalFKs INT = (SELECT COUNT(*) FROM #TempFKs);

PRINT 'Tablas inspeccionadas: ' + CAST(@TotalTables AS VARCHAR(10));
PRINT 'Columnas encontradas: ' + CAST(@TotalColumns AS VARCHAR(10));
PRINT 'Foreign Keys encontradas: ' + CAST(@TotalFKs AS VARCHAR(10));
PRINT '';

PRINT '============================================================================';
PRINT 'FIN DE INSPECCION';
PRINT 'Fecha/Hora: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '============================================================================';
PRINT '';
PRINT 'Usar esta información para corregir FASE_B_PostValidacion_Cruzados.sql';
PRINT '';

-- Limpiar tablas temporales
DROP TABLE #TempColumns;
DROP TABLE #TempFKs;

SET NOCOUNT OFF;

GO
