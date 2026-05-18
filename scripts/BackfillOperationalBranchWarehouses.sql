-- ============================================================================
-- Script: Backfill Almacenes para Sucursales Operativas
-- Fecha: 2026-05-18
-- Propósito: Crear almacenes automáticos para sucursales operativas existentes
--            que no tienen almacén activo asignado.
-- ============================================================================
-- REGLA DE NEGOCIO:
-- Toda sucursal con ManejaAlmacen=1 O PermiteInventario=1 O PermiteDespacho=1
-- debe tener al menos un almacén activo.
-- ============================================================================

BEGIN TRANSACTION;

-- Paso 1: Verificar sucursales operativas sin almacén
SELECT 
    s.Id AS SucursalId,
    s.EmpresaId,
    s.Codigo AS SucursalCodigo,
    s.Nombre AS SucursalNombre,
    s.ManejaAlmacen,
    s.PermiteInventario,
    s.PermiteDespacho,
    COUNT(a.Id) AS AlmacenesActivos
FROM core.Sucursales s
LEFT JOIN mdm.Almacenes a ON a.SucursalId = s.Id AND a.Activo = 1
WHERE s.Activo = 1 
  AND (s.ManejaAlmacen = 1 OR s.PermiteInventario = 1 OR s.PermiteDespacho = 1)
GROUP BY s.Id, s.EmpresaId, s.Codigo, s.Nombre, s.ManejaAlmacen, s.PermiteInventario, s.PermiteDespacho
HAVING COUNT(a.Id) = 0;

-- Paso 2: Insertar almacenes faltantes
INSERT INTO mdm.Almacenes (
    EmpresaId, 
    SucursalId, 
    Codigo, 
    Nombre, 
    Activo, 
    FechaCreacion, 
    CreadoPor
)
SELECT 
    s.EmpresaId,
    s.Id,
    'ALM-' + s.Codigo AS Codigo,
    'Almacén ' + s.Nombre AS Nombre,
    1 AS Activo,
    GETDATE() AS FechaCreacion,
    'SYSTEM-BACKFILL' AS CreadoPor
FROM core.Sucursales s
WHERE s.Activo = 1 
  AND (s.ManejaAlmacen = 1 OR s.PermiteInventario = 1 OR s.PermiteDespacho = 1)
  AND NOT EXISTS (
      SELECT 1 
      FROM mdm.Almacenes a 
      WHERE a.SucursalId = s.Id AND a.Activo = 1
  )
  AND NOT EXISTS (
      -- Prevenir duplicados por constraint único EmpresaId + Codigo
      SELECT 1 
      FROM mdm.Almacenes a2 
      WHERE a2.EmpresaId = s.EmpresaId 
        AND a2.Codigo = 'ALM-' + s.Codigo
  );

-- Paso 3: Verificar resultados
SELECT 
    @@ROWCOUNT AS AlmacenesCreados;

-- Paso 4: Validación final - No deberían quedar sucursales operativas sin almacén
SELECT 
    s.Id AS SucursalId,
    s.EmpresaId,
    s.Codigo AS SucursalCodigo,
    s.Nombre AS SucursalNombre,
    COUNT(a.Id) AS AlmacenesActivos
FROM core.Sucursales s
LEFT JOIN mdm.Almacenes a ON a.SucursalId = s.Id AND a.Activo = 1
WHERE s.Activo = 1 
  AND (s.ManejaAlmacen = 1 OR s.PermiteInventario = 1 OR s.PermiteDespacho = 1)
GROUP BY s.Id, s.EmpresaId, s.Codigo, s.Nombre
HAVING COUNT(a.Id) = 0;

-- Si la query anterior no devuelve filas, el backfill fue exitoso
-- COMMIT TRANSACTION;
-- ROLLBACK TRANSACTION; -- Usar esto para hacer dry-run primero
