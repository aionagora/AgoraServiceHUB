-- ============================================================================
-- SCRIPT: FASE_B_InspectSchema_ProductCompanyProduct.sql
-- PROPOSITO: Inspeccionar esquema real de SQL Server para tablas relacionadas
--            con Products, CompanyProducts y sus dependencias operativas.
-- TIPO: SOLO LECTURA (SELECT)
-- AUTOR: Senior SQL Server DBA + EF Core Architect
-- FECHA: 2025-01-XX
-- ============================================================================

-- CONTEXTO:
-- El script FASE_B_PostValidacion_Cruzados.sql asumiÃ³ nombres de columnas 
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

