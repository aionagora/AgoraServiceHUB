-- ============================================================================
-- SCRIPT DE LIMPIEZA: MODULOS VENTAS / PRODUCTOS / COMPRAS-IMPORTACION RELACIONADA
-- ============================================================================
-- PROPOSITO:
--   Eliminar datos operativos de ventas, inventario, precios, productos y
--   compras/importación relacionadas a CompanyProducts objetivo, preservando:
--   usuarios, roles, empresas, sucursales internas y cualquier esquema de
--   contabilidad (acc / contabilidad).
--
-- REGLAS DE SEGURIDAD:
-- - BEGIN TRANSACTION activo
-- - @ConfirmarCommit = 0 por defecto
-- - ROLLBACK por defecto
-- - No toca usuarios / roles / empresas / sucursales internas
-- - No toca contabilidad
-- - Usa IF OBJECT_ID por tabla
-- - Usa tablas temporales de IDs objetivo
-- - Borra hijos primero, luego cabeceras
-- ============================================================================

SET NOCOUNT ON;

DECLARE @ConfirmarCommit BIT = 0;
DECLARE @Sql NVARCHAR(MAX);
DECLARE @SchemaName SYSNAME;
DECLARE @TableName SYSNAME;
DECLARE @PkColumn SYSNAME;
DECLARE @HeaderSchema SYSNAME;
DECLARE @HeaderTable SYSNAME;
DECLARE @HeaderPkColumn SYSNAME;
DECLARE @FkColumn SYSNAME;
DECLARE @Count INT;
DECLARE @NotExists NVARCHAR(MAX);

PRINT '============================================================================';
PRINT 'INICIO DE LIMPIEZA CONTROLADA: VENTAS / PRODUCTOS / COMPRAS-IMPORTACION';
PRINT 'Fecha/Hora: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT 'ConfirmarCommit: ' + CAST(@ConfirmarCommit AS VARCHAR(1));
PRINT '============================================================================';
PRINT '';

BEGIN TRANSACTION;

IF OBJECT_ID('tempdb..#TargetCompanyProducts') IS NOT NULL DROP TABLE #TargetCompanyProducts;
IF OBJECT_ID('tempdb..#TargetProducts') IS NOT NULL DROP TABLE #TargetProducts;
IF OBJECT_ID('tempdb..#ExplicitHeaders') IS NOT NULL DROP TABLE #ExplicitHeaders;
IF OBJECT_ID('tempdb..#HeaderCandidates') IS NOT NULL DROP TABLE #HeaderCandidates;
IF OBJECT_ID('tempdb..#CpLineTables') IS NOT NULL DROP TABLE #CpLineTables;
IF OBJECT_ID('tempdb..#CpHeaderRelations') IS NOT NULL DROP TABLE #CpHeaderRelations;
IF OBJECT_ID('tempdb..#HeaderChildRelations') IS NOT NULL DROP TABLE #HeaderChildRelations;
IF OBJECT_ID('tempdb..#TargetCmpImpHeaders') IS NOT NULL DROP TABLE #TargetCmpImpHeaders;
IF OBJECT_ID('tempdb..#AuditTables') IS NOT NULL DROP TABLE #AuditTables;
IF OBJECT_ID('tempdb..#ConteosAntes') IS NOT NULL DROP TABLE #ConteosAntes;
IF OBJECT_ID('tempdb..#ConteosDespues') IS NOT NULL DROP TABLE #ConteosDespues;

CREATE TABLE #TargetCompanyProducts (CompanyProductId BIGINT NOT NULL PRIMARY KEY);
CREATE TABLE #TargetProducts (ProductId BIGINT NOT NULL PRIMARY KEY);
CREATE TABLE #ExplicitHeaders (SchemaName SYSNAME NOT NULL, TableName SYSNAME NOT NULL, PRIMARY KEY (SchemaName, TableName));
CREATE TABLE #HeaderCandidates (HeaderSchema SYSNAME NOT NULL, HeaderTable SYSNAME NOT NULL, HeaderPkColumn SYSNAME NULL, PRIMARY KEY (HeaderSchema, HeaderTable));
CREATE TABLE #CpLineTables (SchemaName SYSNAME NOT NULL, TableName SYSNAME NOT NULL, PkColumn SYSNAME NULL, PRIMARY KEY (SchemaName, TableName));
CREATE TABLE #CpHeaderRelations (LineSchema SYSNAME NOT NULL, LineTable SYSNAME NOT NULL, HeaderSchema SYSNAME NOT NULL, HeaderTable SYSNAME NOT NULL, LineFkColumn SYSNAME NOT NULL, HeaderPkColumn SYSNAME NOT NULL);
CREATE TABLE #HeaderChildRelations (ChildSchema SYSNAME NOT NULL, ChildTable SYSNAME NOT NULL, ChildPkColumn SYSNAME NULL, HeaderSchema SYSNAME NOT NULL, HeaderTable SYSNAME NOT NULL, ChildFkColumn SYSNAME NOT NULL, HeaderPkColumn SYSNAME NOT NULL, HasCompanyProductId BIT NOT NULL, PRIMARY KEY (ChildSchema, ChildTable, HeaderSchema, HeaderTable, ChildFkColumn));
CREATE TABLE #TargetCmpImpHeaders (SchemaName SYSNAME NOT NULL, TableName SYSNAME NOT NULL, PkColumn SYSNAME NOT NULL, PkValue NVARCHAR(4000) NOT NULL, PRIMARY KEY (SchemaName, TableName, PkColumn, PkValue));
CREATE TABLE #AuditTables (SchemaName SYSNAME NOT NULL, TableName SYSNAME NOT NULL, SortOrder INT NOT NULL, PRIMARY KEY (SchemaName, TableName));
CREATE TABLE #ConteosAntes (SchemaName SYSNAME NOT NULL, TableName SYSNAME NOT NULL, TotalRows INT NOT NULL, PRIMARY KEY (SchemaName, TableName));
CREATE TABLE #ConteosDespues (SchemaName SYSNAME NOT NULL, TableName SYSNAME NOT NULL, TotalRows INT NOT NULL, PRIMARY KEY (SchemaName, TableName));

IF OBJECT_ID('[mdm].[CompanyProducts]', 'U') IS NOT NULL
BEGIN
    INSERT INTO #TargetCompanyProducts (CompanyProductId)
    SELECT DISTINCT CONVERT(BIGINT, CompanyProductId)
    FROM [mdm].[CompanyProducts];
END

IF OBJECT_ID('[mdm].[Products]', 'U') IS NOT NULL
BEGIN
    INSERT INTO #TargetProducts (ProductId)
    SELECT DISTINCT CONVERT(BIGINT, ProductId)
    FROM [mdm].[Products];
END

PRINT 'CompanyProducts objetivo: ' + CAST((SELECT COUNT(*) FROM #TargetCompanyProducts) AS VARCHAR(20));
PRINT 'Products objetivo: ' + CAST((SELECT COUNT(*) FROM #TargetProducts) AS VARCHAR(20));
PRINT '';

INSERT INTO #ExplicitHeaders (SchemaName, TableName)
VALUES
    ('cmp', 'OrdenCompra'),
    ('cmp', 'OrdenCompras'),
    ('cmp', 'OrdenPedidos'),
    ('imp', 'OrdenPedidos'),
    ('cmp', 'RecepcionesCompra'),
    ('cmp', 'Recepciones');

INSERT INTO #HeaderCandidates (HeaderSchema, HeaderTable, HeaderPkColumn)
SELECT eh.SchemaName, eh.TableName, pk.ColumnName
FROM #ExplicitHeaders eh
INNER JOIN sys.schemas s ON s.name = eh.SchemaName
INNER JOIN sys.tables t ON t.schema_id = s.schema_id AND t.name = eh.TableName
OUTER APPLY
(
    SELECT TOP (1) c.name AS ColumnName
    FROM sys.indexes i
    INNER JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.key_ordinal = 1
    INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE i.object_id = t.object_id AND i.is_primary_key = 1
) pk;

INSERT INTO #CpLineTables (SchemaName, TableName, PkColumn)
SELECT DISTINCT s.name, t.name, pk.ColumnName
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
INNER JOIN sys.columns cpcol ON cpcol.object_id = t.object_id AND cpcol.name = 'CompanyProductId'
OUTER APPLY
(
    SELECT TOP (1) c.name AS ColumnName
    FROM sys.indexes i
    INNER JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.key_ordinal = 1
    INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE i.object_id = t.object_id AND i.is_primary_key = 1
) pk
WHERE s.name IN ('cmp', 'imp')
  AND NOT EXISTS (SELECT 1 FROM #HeaderCandidates hc WHERE hc.HeaderSchema = s.name AND hc.HeaderTable = t.name);

INSERT INTO #CpHeaderRelations (LineSchema, LineTable, HeaderSchema, HeaderTable, LineFkColumn, HeaderPkColumn)
SELECT DISTINCT sParent.name, tParent.name, sRef.name, tRef.name, cParent.name, cRef.name
FROM sys.foreign_keys fk
INNER JOIN sys.tables tParent ON tParent.object_id = fk.parent_object_id
INNER JOIN sys.schemas sParent ON sParent.schema_id = tParent.schema_id
INNER JOIN sys.tables tRef ON tRef.object_id = fk.referenced_object_id
INNER JOIN sys.schemas sRef ON sRef.schema_id = tRef.schema_id
INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
INNER JOIN sys.columns cParent ON cParent.object_id = fkc.parent_object_id AND cParent.column_id = fkc.parent_column_id
INNER JOIN sys.columns cRef ON cRef.object_id = fkc.referenced_object_id AND cRef.column_id = fkc.referenced_column_id
INNER JOIN #CpLineTables cl ON cl.SchemaName = sParent.name AND cl.TableName = tParent.name
INNER JOIN #HeaderCandidates hc ON hc.HeaderSchema = sRef.name AND hc.HeaderTable = tRef.name;

INSERT INTO #HeaderChildRelations (ChildSchema, ChildTable, ChildPkColumn, HeaderSchema, HeaderTable, ChildFkColumn, HeaderPkColumn, HasCompanyProductId)
SELECT DISTINCT sParent.name, tParent.name, pk.ColumnName, sRef.name, tRef.name, cParent.name, cRef.name,
       CASE WHEN EXISTS (SELECT 1 FROM sys.columns x WHERE x.object_id = tParent.object_id AND x.name = 'CompanyProductId') THEN 1 ELSE 0 END
FROM sys.foreign_keys fk
INNER JOIN sys.tables tParent ON tParent.object_id = fk.parent_object_id
INNER JOIN sys.schemas sParent ON sParent.schema_id = tParent.schema_id
INNER JOIN sys.tables tRef ON tRef.object_id = fk.referenced_object_id
INNER JOIN sys.schemas sRef ON sRef.schema_id = tRef.schema_id
INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
INNER JOIN sys.columns cParent ON cParent.object_id = fkc.parent_object_id AND cParent.column_id = fkc.parent_column_id
INNER JOIN sys.columns cRef ON cRef.object_id = fkc.referenced_object_id AND cRef.column_id = fkc.referenced_column_id
INNER JOIN #HeaderCandidates hc ON hc.HeaderSchema = sRef.name AND hc.HeaderTable = tRef.name
OUTER APPLY
(
    SELECT TOP (1) c.name AS ColumnName
    FROM sys.indexes i
    INNER JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.key_ordinal = 1
    INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE i.object_id = tParent.object_id AND i.is_primary_key = 1
) pk
WHERE sParent.name IN ('cmp', 'imp');

DECLARE curCaptureHeaders CURSOR LOCAL FAST_FORWARD FOR
SELECT LineSchema, LineTable, HeaderSchema, HeaderTable, LineFkColumn, HeaderPkColumn
FROM #CpHeaderRelations
ORDER BY LineSchema, LineTable, HeaderSchema, HeaderTable;

OPEN curCaptureHeaders;
FETCH NEXT FROM curCaptureHeaders INTO @SchemaName, @TableName, @HeaderSchema, @HeaderTable, @FkColumn, @HeaderPkColumn;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Sql = N'
        INSERT INTO #TargetCmpImpHeaders (SchemaName, TableName, PkColumn, PkValue)
        SELECT DISTINCT N''' + @HeaderSchema + N''', N''' + @HeaderTable + N''', N''' + @HeaderPkColumn + N''', CONVERT(NVARCHAR(4000), t.' + QUOTENAME(@FkColumn) + N')
        FROM ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) + N' t
        INNER JOIN #TargetCompanyProducts tcp ON tcp.CompanyProductId = CONVERT(BIGINT, t.CompanyProductId)
        WHERE t.' + QUOTENAME(@FkColumn) + N' IS NOT NULL;';
    EXEC sp_executesql @Sql;
    FETCH NEXT FROM curCaptureHeaders INTO @SchemaName, @TableName, @HeaderSchema, @HeaderTable, @FkColumn, @HeaderPkColumn;
END
CLOSE curCaptureHeaders;
DEALLOCATE curCaptureHeaders;

PRINT 'Cabeceras cmp/imp objetivo: ' + CAST((SELECT COUNT(*) FROM #TargetCmpImpHeaders) AS VARCHAR(20));
PRINT '';

INSERT INTO #AuditTables (SchemaName, TableName, SortOrder)
SELECT v.SchemaName, v.TableName, v.SortOrder
FROM (VALUES
    ('vta', 'PedidoVentaDetalles', 10),
    ('vta', 'PedidosVenta', 20),
    ('prc', 'PriceListItems', 30),
    ('prc', 'PriceLists', 40),
    ('inv', 'MovimientosInventario', 50),
    ('inv', 'StockProductos', 60),
    ('mdm', 'CompanyProductFeatures', 70),
    ('mdm', 'CompanyProducts', 80),
    ('doc', 'ProductDocuments', 90),
    ('mdm', 'ProductAttributes', 100),
    ('mdm', 'ProductCategories', 110),
    ('mdm', 'ProductClassificationLinks', 120),
    ('mdm', 'ProductCodes', 130),
    ('mdm', 'ProductUoms', 140),
    ('mdm', 'Products', 150)
) v(SchemaName, TableName, SortOrder)
WHERE OBJECT_ID(QUOTENAME(v.SchemaName) + '.' + QUOTENAME(v.TableName), 'U') IS NOT NULL;

INSERT INTO #AuditTables (SchemaName, TableName, SortOrder)
SELECT x.SchemaName, x.TableName, 15
FROM
(
    SELECT SchemaName, TableName FROM #CpLineTables
    UNION
    SELECT HeaderSchema, HeaderTable FROM #HeaderCandidates
    UNION
    SELECT ChildSchema, ChildTable FROM #HeaderChildRelations
) x
WHERE OBJECT_ID(QUOTENAME(x.SchemaName) + '.' + QUOTENAME(x.TableName), 'U') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM #AuditTables a WHERE a.SchemaName = x.SchemaName AND a.TableName = x.TableName);

DECLARE curAuditAntes CURSOR LOCAL FAST_FORWARD FOR
SELECT SchemaName, TableName FROM #AuditTables ORDER BY SortOrder, SchemaName, TableName;
OPEN curAuditAntes;
FETCH NEXT FROM curAuditAntes INTO @SchemaName, @TableName;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Count = 0;
    SET @Sql = N'SELECT @Count = COUNT(*) FROM ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) + N';';
    EXEC sp_executesql @Sql, N'@Count INT OUTPUT', @Count = @Count OUTPUT;
    INSERT INTO #ConteosAntes (SchemaName, TableName, TotalRows) VALUES (@SchemaName, @TableName, ISNULL(@Count, 0));
    FETCH NEXT FROM curAuditAntes INTO @SchemaName, @TableName;
END
CLOSE curAuditAntes;
DEALLOCATE curAuditAntes;

PRINT '============================================================================';
PRINT 'FASE 1: LIMPIEZA DE COMPRAS / IMPORTACION RELACIONADA';
PRINT '============================================================================';

DECLARE curDeleteCpLines CURSOR LOCAL FAST_FORWARD FOR
SELECT SchemaName, TableName FROM #CpLineTables ORDER BY SchemaName, TableName;
OPEN curDeleteCpLines;
FETCH NEXT FROM curDeleteCpLines INTO @SchemaName, @TableName;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Sql = N'DELETE t FROM ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) + N' t INNER JOIN #TargetCompanyProducts tcp ON tcp.CompanyProductId = CONVERT(BIGINT, t.CompanyProductId);';
    EXEC sp_executesql @Sql;
    PRINT 'Eliminadas líneas relacionadas en ' + @SchemaName + '.' + @TableName;
    FETCH NEXT FROM curDeleteCpLines INTO @SchemaName, @TableName;
END
CLOSE curDeleteCpLines;
DEALLOCATE curDeleteCpLines;

DECLARE curDeleteHeaderChildren CURSOR LOCAL FAST_FORWARD FOR
SELECT ChildSchema, ChildTable, HeaderSchema, HeaderTable, ChildFkColumn, HeaderPkColumn
FROM #HeaderChildRelations
WHERE HasCompanyProductId = 0
ORDER BY ChildSchema, ChildTable, HeaderSchema, HeaderTable;
OPEN curDeleteHeaderChildren;
FETCH NEXT FROM curDeleteHeaderChildren INTO @SchemaName, @TableName, @HeaderSchema, @HeaderTable, @FkColumn, @HeaderPkColumn;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Sql = N'
        DELETE c
        FROM ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) + N' c
        INNER JOIN #TargetCmpImpHeaders th
            ON th.SchemaName = N''' + @HeaderSchema + N'''
           AND th.TableName = N''' + @HeaderTable + N'''
           AND th.PkColumn = N''' + @HeaderPkColumn + N'''
           AND CONVERT(NVARCHAR(4000), c.' + QUOTENAME(@FkColumn) + N') = th.PkValue;';
    EXEC sp_executesql @Sql;
    PRINT 'Eliminados hijos en ' + @SchemaName + '.' + @TableName + ' por cabeceras objetivo';
    FETCH NEXT FROM curDeleteHeaderChildren INTO @SchemaName, @TableName, @HeaderSchema, @HeaderTable, @FkColumn, @HeaderPkColumn;
END
CLOSE curDeleteHeaderChildren;
DEALLOCATE curDeleteHeaderChildren;

DECLARE curDeleteHeaders CURSOR LOCAL FAST_FORWARD FOR
SELECT HeaderSchema, HeaderTable, HeaderPkColumn FROM #HeaderCandidates WHERE HeaderPkColumn IS NOT NULL ORDER BY HeaderSchema, HeaderTable;
OPEN curDeleteHeaders;
FETCH NEXT FROM curDeleteHeaders INTO @HeaderSchema, @HeaderTable, @HeaderPkColumn;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @NotExists = N'';
    DECLARE curChildCheck CURSOR LOCAL FAST_FORWARD FOR
    SELECT ChildSchema, ChildTable, ChildFkColumn
    FROM #HeaderChildRelations
    WHERE HeaderSchema = @HeaderSchema AND HeaderTable = @HeaderTable
    ORDER BY ChildSchema, ChildTable, ChildFkColumn;
    OPEN curChildCheck;
    FETCH NEXT FROM curChildCheck INTO @SchemaName, @TableName, @FkColumn;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @NotExists = @NotExists + N' AND NOT EXISTS (SELECT 1 FROM ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) + N' c WHERE CONVERT(NVARCHAR(4000), c.' + QUOTENAME(@FkColumn) + N') = CONVERT(NVARCHAR(4000), h.' + QUOTENAME(@HeaderPkColumn) + N'))';
        FETCH NEXT FROM curChildCheck INTO @SchemaName, @TableName, @FkColumn;
    END
    CLOSE curChildCheck;
    DEALLOCATE curChildCheck;

    SET @Sql = N'
        DELETE h
        FROM ' + QUOTENAME(@HeaderSchema) + N'.' + QUOTENAME(@HeaderTable) + N' h
        INNER JOIN #TargetCmpImpHeaders th
            ON th.SchemaName = N''' + @HeaderSchema + N'''
           AND th.TableName = N''' + @HeaderTable + N'''
           AND th.PkColumn = N''' + @HeaderPkColumn + N'''
           AND CONVERT(NVARCHAR(4000), h.' + QUOTENAME(@HeaderPkColumn) + N') = th.PkValue
        WHERE 1 = 1 ' + ISNULL(@NotExists, N'') + N';';
    EXEC sp_executesql @Sql;
    PRINT 'Revisadas/eliminadas cabeceras vacías en ' + @HeaderSchema + '.' + @HeaderTable;
    FETCH NEXT FROM curDeleteHeaders INTO @HeaderSchema, @HeaderTable, @HeaderPkColumn;
END
CLOSE curDeleteHeaders;
DEALLOCATE curDeleteHeaders;

PRINT '============================================================================';
PRINT 'FASE 2: INVENTARIO / PRECIOS / VENTAS / PRODUCTOS';
PRINT '============================================================================';

IF OBJECT_ID('[vta].[PedidoVentaDetalles]', 'U') IS NOT NULL DELETE FROM [vta].[PedidoVentaDetalles];
IF OBJECT_ID('[vta].[PedidosVenta]', 'U') IS NOT NULL DELETE FROM [vta].[PedidosVenta];
IF OBJECT_ID('[prc].[PriceListItems]', 'U') IS NOT NULL DELETE FROM [prc].[PriceListItems];
IF OBJECT_ID('[prc].[PriceLists]', 'U') IS NOT NULL DELETE FROM [prc].[PriceLists];
IF OBJECT_ID('[inv].[MovimientosInventario]', 'U') IS NOT NULL DELETE FROM [inv].[MovimientosInventario];
IF OBJECT_ID('[inv].[StockProductos]', 'U') IS NOT NULL DELETE FROM [inv].[StockProductos];

IF OBJECT_ID('[mdm].[CompanyProductFeatures]', 'U') IS NOT NULL
    DELETE cpf FROM [mdm].[CompanyProductFeatures] cpf INNER JOIN #TargetCompanyProducts tcp ON tcp.CompanyProductId = CONVERT(BIGINT, cpf.CompanyProductId);

IF OBJECT_ID('[mdm].[CompanyProducts]', 'U') IS NOT NULL
    DELETE cp FROM [mdm].[CompanyProducts] cp INNER JOIN #TargetCompanyProducts tcp ON tcp.CompanyProductId = CONVERT(BIGINT, cp.CompanyProductId);

IF OBJECT_ID('[doc].[ProductDocuments]', 'U') IS NOT NULL DELETE FROM [doc].[ProductDocuments];
IF OBJECT_ID('[mdm].[ProductAttributes]', 'U') IS NOT NULL DELETE FROM [mdm].[ProductAttributes];
IF OBJECT_ID('[mdm].[ProductCategories]', 'U') IS NOT NULL DELETE FROM [mdm].[ProductCategories];
IF OBJECT_ID('[mdm].[ProductClassificationLinks]', 'U') IS NOT NULL DELETE FROM [mdm].[ProductClassificationLinks];
IF OBJECT_ID('[mdm].[ProductCodes]', 'U') IS NOT NULL DELETE FROM [mdm].[ProductCodes];
IF OBJECT_ID('[mdm].[ProductUoms]', 'U') IS NOT NULL DELETE FROM [mdm].[ProductUoms];

IF OBJECT_ID('[mdm].[Products]', 'U') IS NOT NULL
    DELETE p FROM [mdm].[Products] p INNER JOIN #TargetProducts tp ON tp.ProductId = CONVERT(BIGINT, p.ProductId);

DECLARE curAuditDespues CURSOR LOCAL FAST_FORWARD FOR
SELECT SchemaName, TableName FROM #AuditTables ORDER BY SortOrder, SchemaName, TableName;
OPEN curAuditDespues;
FETCH NEXT FROM curAuditDespues INTO @SchemaName, @TableName;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Count = 0;
    SET @Sql = N'SELECT @Count = COUNT(*) FROM ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) + N';';
    EXEC sp_executesql @Sql, N'@Count INT OUTPUT', @Count = @Count OUTPUT;
    INSERT INTO #ConteosDespues (SchemaName, TableName, TotalRows) VALUES (@SchemaName, @TableName, ISNULL(@Count, 0));
    FETCH NEXT FROM curAuditDespues INTO @SchemaName, @TableName;
END
CLOSE curAuditDespues;
DEALLOCATE curAuditDespues;

PRINT '============================================================================';
PRINT 'VALIDACION FINAL';
PRINT '============================================================================';
SELECT a.SchemaName + '.' + a.TableName AS Tabla, a.TotalRows AS ConteoAntes, d.TotalRows AS ConteoDespues, a.TotalRows - d.TotalRows AS Eliminados
FROM #ConteosAntes a
INNER JOIN #ConteosDespues d ON d.SchemaName = a.SchemaName AND d.TableName = a.TableName
ORDER BY Tabla;

PRINT '============================================================================';
PRINT 'CIERRE DE TRANSACCION';
PRINT '============================================================================';
IF @ConfirmarCommit = 1
BEGIN
    PRINT 'COMMIT confirmado por @ConfirmarCommit = 1';
    COMMIT TRANSACTION;
END
ELSE
BEGIN
    PRINT 'ROLLBACK por seguridad (@ConfirmarCommit = 0).';
    ROLLBACK TRANSACTION;
END

PRINT 'FIN DE LIMPIEZA';
PRINT 'Fecha/Hora: ' + CONVERT(VARCHAR, GETDATE(), 120);

SET NOCOUNT OFF;
GO
