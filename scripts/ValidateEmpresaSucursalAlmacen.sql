-- ============================================================================
-- Script: Validación de Estructura Empresa/Sucursal/Almacén
-- Fecha: 2026-05-18
-- Propósito: Verificar que todas las sucursales operativas tienen almacén
-- ============================================================================

-- 1. Verificar estructura completa de empresas
SELECT 
    e.Id AS EmpresaId,
    e.Nombre AS Empresa,
    COUNT(DISTINCT s.Id) AS TotalSucursales,
    COUNT(DISTINCT CASE WHEN s.Activo = 1 THEN s.Id END) AS SucursalesActivas,
    COUNT(DISTINCT a.Id) AS TotalAlmacenes,
    COUNT(DISTINCT CASE WHEN a.Activo = 1 THEN a.Id END) AS AlmacenesActivos
FROM core.Empresas e
LEFT JOIN core.Sucursales s ON s.EmpresaId = e.Id
LEFT JOIN mdm.Almacenes a ON a.EmpresaId = e.Id
WHERE e.Activo = 1
GROUP BY e.Id, e.Nombre
ORDER BY e.Nombre;

-- 2. Verificar sucursales operativas SIN almacén (NO debe devolver filas)
SELECT 
    s.Id AS SucursalId,
    s.EmpresaId,
    e.Nombre AS Empresa,
    s.Codigo AS SucursalCodigo,
    s.Nombre AS SucursalNombre,
    s.ManejaAlmacen,
    s.PermiteInventario,
    s.PermiteDespacho,
    COUNT(a.Id) AS AlmacenesActivos
FROM core.Sucursales s
INNER JOIN core.Empresas e ON e.Id = s.EmpresaId
LEFT JOIN mdm.Almacenes a ON a.SucursalId = s.Id AND a.Activo = 1
WHERE s.Activo = 1 
  AND (s.ManejaAlmacen = 1 OR s.PermiteInventario = 1 OR s.PermiteDespacho = 1)
GROUP BY s.Id, s.EmpresaId, e.Nombre, s.Codigo, s.Nombre, s.ManejaAlmacen, s.PermiteInventario, s.PermiteDespacho
HAVING COUNT(a.Id) = 0;

-- 3. Verificar almacenes por sucursal
SELECT 
    e.Nombre AS Empresa,
    s.Codigo AS SucursalCodigo,
    s.Nombre AS SucursalNombre,
    s.EsCentral,
    s.ManejaAlmacen,
    s.PermiteInventario,
    s.PermiteDespacho,
    a.Codigo AS AlmacenCodigo,
    a.Nombre AS AlmacenNombre,
    a.Activo AS AlmacenActivo,
    a.CreadoPor
FROM core.Empresas e
INNER JOIN core.Sucursales s ON s.EmpresaId = e.Id
LEFT JOIN mdm.Almacenes a ON a.SucursalId = s.Id
WHERE e.Activo = 1 AND s.Activo = 1
ORDER BY e.Nombre, s.Codigo, a.Codigo;

-- 4. Verificar constraint único EmpresaId + Codigo en Almacenes
SELECT 
    EmpresaId,
    Codigo,
    COUNT(*) AS Duplicados
FROM mdm.Almacenes
GROUP BY EmpresaId, Codigo
HAVING COUNT(*) > 1;

-- 5. Verificar que todas las empresas tienen Sucursal Principal y Almacén Principal
SELECT 
    e.Id AS EmpresaId,
    e.Nombre AS Empresa,
    CASE WHEN EXISTS(
        SELECT 1 FROM core.Sucursales s 
        WHERE s.EmpresaId = e.Id 
          AND s.Codigo = 'SUC01' 
          AND s.EsCentral = 1 
          AND s.Activo = 1
    ) THEN 'SÍ' ELSE 'NO' END AS TieneSucursalPrincipal,
    CASE WHEN EXISTS(
        SELECT 1 FROM mdm.Almacenes a 
        WHERE a.EmpresaId = e.Id 
          AND a.Codigo = 'ALM-SUC01' 
          AND a.Activo = 1
    ) THEN 'SÍ' ELSE 'NO' END AS TieneAlmacenPrincipal
FROM core.Empresas e
WHERE e.Activo = 1
ORDER BY e.Nombre;
