# ?? Solución: Error 403 - Tenant Required

## ? Error Identificado

```json
{
  "type": "https://httpstatuses.com/403",
  "title": "Tenant Required",
  "status": 403,
  "detail": "Debe seleccionar una empresa activa para realizar esta operacion.",
  "traceId": "0HNJE8677LMFU:0000001B"
}
```

## ?? Causa Raíz

El token JWT del usuario **NO contiene el claim `EmpresaId`**, por lo tanto el middleware `TenantRequiredMiddleware` rechaza las operaciones de escritura (POST/PUT/DELETE).

### ¿Por qué no tiene EmpresaId?

Posibles causas:
1. El usuario no tiene empresa asignada en la tabla `UsuarioEmpresas`
2. El usuario no tiene `EmpresaActivaId` configurado
3. El token fue generado antes de la corrección del sistema

---

## ? Solución Paso a Paso

### Opción 1: Verificar y Corregir en Base de Datos

**Paso 1: Verificar Usuario y Empresas**

```sql
-- Ver usuario admin
SELECT 
    u.Id,
    u.NombreUsuario,
    u.Email,
    u.EmpresaActivaId
FROM core.Usuarios u
WHERE u.Email = 'admin@agorahub360.com';

-- Ver empresas asignadas al usuario
SELECT 
    ue.UsuarioId,
    ue.EmpresaId,
    ue.Rol,
    e.Nombre AS EmpresaNombre
FROM core.UsuarioEmpresas ue
INNER JOIN core.Empresas e ON ue.EmpresaId = e.Id
WHERE ue.UsuarioId = 1;
```

**Paso 2: Si no tiene empresa asignada, asignarla**

```sql
-- Verificar que la empresa existe
SELECT * FROM core.Empresas WHERE Id = 1;

-- Asignar empresa al usuario si no existe
IF NOT EXISTS (SELECT 1 FROM core.UsuarioEmpresas WHERE UsuarioId = 1 AND EmpresaId = 1)
BEGIN
    INSERT INTO core.UsuarioEmpresas (UsuarioId, EmpresaId, Rol)
    VALUES (1, 1, 'Admin');
END

-- Actualizar empresa activa del usuario
UPDATE core.Usuarios
SET EmpresaActivaId = 1
WHERE Id = 1 AND EmpresaActivaId IS NULL;
```

**Paso 3: Cerrar sesión y volver a iniciar sesión**

Esto generará un nuevo token JWT con el claim `EmpresaId`.

---

### Opción 2: Script PowerShell Automatizado

Ejecuta este script para verificar y corregir todo automáticamente:

```powershell
# Script: fix-tenant-required.ps1

Write-Host "`n???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  ?? Corrección: Tenant Required (403)" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Configuración
$server = "localhost,1433"
$database = "AgoraHub360_ERP_Dev"
$username = "sa"
$password = "YourStrong@Passw0rd"

Write-Host "1?? Verificando asignación de empresa..." -ForegroundColor Yellow

$sqlQuery = @"
-- Verificar asignación
SELECT 
    u.Id AS UsuarioId,
    u.NombreUsuario,
    u.Email,
    u.EmpresaActivaId,
    ue.EmpresaId AS EmpresaAsignada,
    ue.Rol
FROM core.Usuarios u
LEFT JOIN core.UsuarioEmpresas ue ON u.Id = ue.UsuarioId
WHERE u.Email = 'admin@agorahub360.com';

-- Corregir si es necesario
IF NOT EXISTS (SELECT 1 FROM core.UsuarioEmpresas WHERE UsuarioId = 1 AND EmpresaId = 1)
BEGIN
    INSERT INTO core.UsuarioEmpresas (UsuarioId, EmpresaId, Rol)
    VALUES (1, 1, 'Admin');
    PRINT 'Empresa asignada al usuario';
END

UPDATE core.Usuarios
SET EmpresaActivaId = 1
WHERE Id = 1 AND EmpresaActivaId IS NULL;

-- Verificar resultado
SELECT 
    u.Id,
    u.NombreUsuario,
    u.EmpresaActivaId,
    ue.EmpresaId,
    ue.Rol
FROM core.Usuarios u
LEFT JOIN core.UsuarioEmpresas ue ON u.Id = ue.UsuarioId
WHERE u.Id = 1;
"@

try {
    $connectionString = "Server=$server;Database=$database;User Id=$username;Password=$password;TrustServerCertificate=true;"
    
    # Ejecutar query
    $result = Invoke-Sqlcmd -ConnectionString $connectionString -Query $sqlQuery -ErrorAction Stop
    
    Write-Host "   ? Verificación completada" -ForegroundColor Green
    Write-Host ""
    
    if ($result) {
        Write-Host "?? Estado actual del usuario:" -ForegroundColor Cyan
        $result | Format-Table -AutoSize
    }
    
    Write-Host ""
    Write-Host "2?? Generando nuevo token..." -ForegroundColor Yellow
    Write-Host ""
    
    # Obtener nuevo token
    $loginData = @{
        email = "admin@agorahub360.com"
        password = "Admin123"
    } | ConvertTo-Json

    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
    
    $loginResponse = Invoke-RestMethod -Uri "https://localhost:7001/api/v1/auth/login" `
        -Method Post `
        -Body $loginData `
        -ContentType "application/json" `
        -ErrorAction Stop

    if ($loginResponse.success) {
        $token = $loginResponse.data.token
        
        # Decodificar y mostrar claims
        $payload = $token.Split('.')[1]
        $paddedPayload = $payload.PadRight(([Math]::Ceiling($payload.Length / 4) * 4), '=')
        $decodedBytes = [Convert]::FromBase64String($paddedPayload)
        $decodedJson = [System.Text.Encoding]::UTF8.GetString($decodedBytes)
        $claims = $decodedJson | ConvertFrom-Json
        
        Write-Host "   ? Token generado exitosamente" -ForegroundColor Green
        Write-Host ""
        Write-Host "?? Claims del token:" -ForegroundColor Cyan
        Write-Host "   • NameIdentifier: $($claims.nameid)" -ForegroundColor White
        Write-Host "   • Name: $($claims.unique_name)" -ForegroundColor White
        Write-Host "   • Email: $($claims.email)" -ForegroundColor White
        Write-Host "   • Role: $($claims.role)" -ForegroundColor White
        
        if ($claims.EmpresaId) {
            Write-Host "   • EmpresaId: $($claims.EmpresaId)" -ForegroundColor Green
            Write-Host ""
            Write-Host "? El token ahora contiene EmpresaId" -ForegroundColor Green
        } else {
            Write-Host "   • EmpresaId: [NO PRESENTE]" -ForegroundColor Red
            Write-Host ""
            Write-Host "??  El token AÚN NO contiene EmpresaId" -ForegroundColor Yellow
            Write-Host "   Verifica que el usuario tenga empresa asignada en la BD" -ForegroundColor Yellow
        }
    }
    
} catch {
    Write-Host "   ? Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "??  Por favor ejecuta manualmente las queries SQL" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Siguiente paso:" -ForegroundColor Yellow
Write-Host "   1. Cierra sesión en la aplicación web" -ForegroundColor White
Write-Host "   2. Vuelve a iniciar sesión" -ForegroundColor White
Write-Host "   3. Intenta crear el parámetro nuevamente" -ForegroundColor White
Write-Host ""
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
```

**Guardar como:** `fix-tenant-required.ps1`

**Ejecutar:**
```powershell
.\fix-tenant-required.ps1
```

---

### Opción 3: Verificación Manual Rápida

**En la consola del navegador (F12):**

```javascript
// 1. Ver el token actual
const token = localStorage.getItem('agorahub360_auth_token');
console.log('Token:', token);

// 2. Decodificar el payload
const payload = JSON.parse(atob(token.split('.')[1]));
console.log('Claims:', payload);

// 3. Verificar si tiene EmpresaId
if (payload.EmpresaId) {
    console.log('? Token válido con EmpresaId:', payload.EmpresaId);
} else {
    console.log('? Token SIN EmpresaId - Necesitas cerrar sesión y volver a iniciar sesión');
}
```

**Si NO tiene EmpresaId:**
1. Haz clic en el botón de cerrar sesión (??)
2. Vuelve a iniciar sesión con:
   - Email: `admin@agorahub360.com`
   - Contraseña: `Admin123`
3. Verifica el token nuevamente en la consola

---

## ?? ¿Cómo funciona el Tenant Middleware?

### TenantRequiredMiddleware

```csharp
// Rutas exentas (NO requieren EmpresaId):
- /api/v1/auth        ? Login/Logout
- /api/v1/empresas    ? CRUD de empresas (global)
- /api/v1/usuarios    ? CRUD de usuarios (global)
- /api/v1/roles       ? CRUD de roles (global)
- /api/v1/diagnostics ? Diagnóstico
- /health             ? Health check
- /swagger            ? Documentación

// Rutas que SÍ requieren EmpresaId:
- /api/v1/parametros    ? Operaciones POST/PUT/DELETE
- /api/v1/numeraciones  ? Operaciones POST/PUT/DELETE
- Cualquier otra ruta tenant-aware
```

### Validación

```csharp
// El middleware verifica:
1. ¿Es una petición a la API? ? Sí
2. ¿Es una ruta exenta? ? No (/parametros no está en la lista)
3. ¿Es POST/PUT/DELETE? ? Sí (POST)
4. ¿Usuario autenticado? ? Sí
5. ¿Tiene claim EmpresaId? ? ? NO ? Error 403
```

---

## ? Solución Definitiva

### Paso 1: Ejecutar SQL

```sql
-- Verificar y corregir asignación de empresa
SELECT * FROM core.UsuarioEmpresas WHERE UsuarioId = 1;

-- Si no existe, insertarla
INSERT INTO core.UsuarioEmpresas (UsuarioId, EmpresaId, Rol)
SELECT 1, 1, 'Admin'
WHERE NOT EXISTS (SELECT 1 FROM core.UsuarioEmpresas WHERE UsuarioId = 1 AND EmpresaId = 1);

-- Actualizar empresa activa
UPDATE core.Usuarios SET EmpresaActivaId = 1 WHERE Id = 1;
```

### Paso 2: Cerrar Sesión y Volver a Iniciar

1. En la aplicación web, haz clic en tu nombre de usuario (esquina superior derecha)
2. Haz clic en **"Cerrar Sesión"**
3. Vuelve a iniciar sesión:
   - Email: `admin@agorahub360.com`
   - Contraseña: `Admin123`

### Paso 3: Verificar Token

En la consola del navegador (F12):
```javascript
const token = localStorage.getItem('agorahub360_auth_token');
const payload = JSON.parse(atob(token.split('.')[1]));
console.log('EmpresaId:', payload.EmpresaId); // Debe mostrar: 1
```

### Paso 4: Intentar Crear Parámetro Nuevamente

Ahora debería funcionar sin el error 403.

---

## ?? Verificación Completa

```sql
-- Query completa de verificación
SELECT 
    'Usuario' AS Tabla,
    u.Id,
    u.NombreUsuario,
    u.Email,
    u.EmpresaActivaId,
    NULL AS EmpresaAsignada,
    NULL AS Rol
FROM core.Usuarios u
WHERE u.Id = 1

UNION ALL

SELECT 
    'UsuarioEmpresa' AS Tabla,
    ue.UsuarioId,
    NULL,
    NULL,
    NULL,
    ue.EmpresaId,
    ue.Rol
FROM core.UsuarioEmpresas ue
WHERE ue.UsuarioId = 1;

-- Resultado esperado:
-- Tabla            | Id | NombreUsuario | EmpresaActivaId | EmpresaAsignada | Rol
-- Usuario          | 1  | admin         | 1               | NULL            | NULL
-- UsuarioEmpresa   | 1  | NULL          | NULL            | 1               | Admin
```

---

## ?? Resumen

**Problema:** Token JWT sin claim `EmpresaId`  
**Causa:** Usuario sin empresa asignada o token antiguo  
**Solución:** Asignar empresa en BD + Cerrar sesión + Volver a iniciar sesión  

---

**? Una vez hecho esto, el error 403 desaparecerá y podrás crear parámetros.**

_Fecha: 17/02/2026_
