# ?? SOLUCIÓN RÁPIDA - Error 403 Tenant Required

## ? SOLUCIÓN EN 3 PASOS (5 minutos)

---

### ?? **Paso 1: Ejecutar Script SQL** (2 minutos)

1. Abre **SQL Server Management Studio (SSMS)**
2. Conéctate a tu servidor: `localhost,1433`
3. Abre el archivo: **`fix-tenant-required.sql`**
4. Presiona **F5** para ejecutar
5. Verifica que veas el mensaje: `? Correcciones aplicadas`

**Resultado esperado:**
```
UsuarioId | NombreUsuario | Email                    | EmpresaActivaId | EmpresaAsignada | Rol
1         | admin         | admin@agorahub360.com    | 1               | 1               | Admin
```

---

### ?? **Paso 2: Cerrar Sesión** (1 minuto)

1. En la aplicación web: `https://localhost:5002`
2. Haz clic en tu **nombre** (esquina superior derecha)
3. Haz clic en **"Cerrar Sesión"** ??
4. Deberías ver la página de login

---

### ?? **Paso 3: Volver a Iniciar Sesión** (1 minuto)

1. En la página de login:
   - **Email:** `admin@agorahub360.com`
   - **Contraseña:** `Admin123`
2. Haz clic en **"Iniciar Sesión"**
3. Deberías ver el dashboard

---

### ? **Verificación** (1 minuto)

**Opción A: Desde la Consola del Navegador**

1. Presiona **F12** para abrir Developer Tools
2. Ve a la pestaña **Console**
3. Pega y ejecuta este código:

```javascript
const token = localStorage.getItem('agorahub360_auth_token');
const payload = JSON.parse(atob(token.split('.')[1]));
console.log('? Claims del token:', payload);
console.log('? EmpresaId:', payload.EmpresaId);
```

**Resultado esperado:**
```javascript
? Claims del token: {
  nameid: "1",
  unique_name: "admin",
  email: "admin@agorahub360.com",
  role: "Admin",
  EmpresaId: "1",  // ? ¡DEBE ESTAR PRESENTE!
  exp: 1739123456,
  iss: "AgoraHub360.ERP",
  aud: "AgoraHub360.ERP.Web"
}
? EmpresaId: 1
```

**Opción B: Intentar Crear Parámetro**

1. Ve a: **Sistema ? Parámetros y Numeración**
2. Haz clic en **"Nuevo Parámetro"**
3. Completa:
   - **Clave:** `TEST_PARAM`
   - **Valor:** `123`
   - **Categoría:** `General`
4. Haz clic en **"Guardar"**

**Si todo está correcto:**
- ? Verás el mensaje: "Parámetro guardado exitosamente"
- ? El modal se cerrará
- ? Verás el parámetro en la lista

**Si sigue el error 403:**
- ? Ve a la sección "Troubleshooting" abajo

---

## ?? Troubleshooting

### Problema: El script SQL falla con "Usuario no encontrado"

**Solución:**
```sql
-- Verificar que el usuario existe
SELECT * FROM core.Usuarios WHERE Email = 'admin@agorahub360.com';

-- Si no existe, crearlo
SET IDENTITY_INSERT core.Usuarios ON;

INSERT INTO core.Usuarios (Id, NombreUsuario, Email, PasswordHash, NombreCompleto, EmpresaActivaId, FechaCreacion, Activo)
VALUES (1, 'admin', 'admin@agorahub360.com', 'O2Esdae1BIpDX7bsgeUv+S1teVqLWpwXBw9qY8l6U7I=', 
        'Administrador del Sistema', 1, GETDATE(), 1);

SET IDENTITY_INSERT core.Usuarios OFF;
```

---

### Problema: Token sigue sin tener EmpresaId

**Diagnóstico:**
```javascript
// En la consola del navegador (F12)
const token = localStorage.getItem('agorahub360_auth_token');
const payload = JSON.parse(atob(token.split('.')[1]));

if (payload.EmpresaId) {
    console.log('? Token correcto con EmpresaId:', payload.EmpresaId);
} else {
    console.log('? Token SIN EmpresaId');
    console.log('?? SOLUCIÓN: Borra el token antiguo');
    localStorage.removeItem('agorahub360_auth_token');
    console.log('? Token borrado. Recarga la página y vuelve a iniciar sesión.');
    location.reload();
}
```

---

### Problema: Error al conectar con SQL Server

**Verifica:**
1. SQL Server está corriendo
2. Puerto 1433 está abierto
3. Autenticación SQL Server habilitada
4. Usuario `sa` tiene permisos

**Test de conexión:**
```powershell
Test-NetConnection -ComputerName localhost -Port 1433
```

---

### Problema: Sigue error 403 después de todo

**Verificación completa:**

1. **Verifica la BD:**
```sql
SELECT 
    u.Id,
    u.NombreUsuario,
    u.EmpresaActivaId,
    ue.EmpresaId,
    ue.Rol
FROM core.Usuarios u
LEFT JOIN core.UsuarioEmpresas ue ON u.Id = ue.UsuarioId
WHERE u.Id = 1;
```

Debe mostrar:
- `EmpresaActivaId = 1`
- `EmpresaId = 1`
- `Rol = Admin`

2. **Verifica el token:**
```javascript
// Consola del navegador
const token = localStorage.getItem('agorahub360_auth_token');
console.log('Token length:', token.length);
console.log('Token:', token.substring(0, 50) + '...');

const payload = JSON.parse(atob(token.split('.')[1]));
console.log('Payload:', payload);
```

3. **Borra cache y cookies:**
   - Chrome: `Ctrl + Shift + Delete`
   - Selecciona "Cookies" y "Cached images"
   - Haz clic en "Clear data"

4. **Reinicia la aplicación:**
```powershell
# Detener procesos
Get-Process | Where-Object {$_.ProcessName -like "*AgoraHub*"} | Stop-Process -Force

# Reiniciar
.\start-system.ps1
```

---

## ?? Soporte

Si después de seguir todos los pasos el problema persiste:

1. **Captura estos datos:**
   - Resultado del script SQL
   - Payload del token JWT (desde consola)
   - Error exacto que aparece
   - Logs de la terminal de la API

2. **Verifica logs de la API:**
   - Busca líneas que contengan: `TenantRequiredMiddleware`
   - Busca líneas que contengan: `ParametrosController`

3. **Documentos de referencia:**
   - `TENANT-REQUIRED-FIX.md` - Explicación detallada
   - `fix-tenant-required.ps1` - Script alternativo PowerShell
   - `PARAMETROS-MULTIEMPRESA-FIX.md` - Contexto del problema

---

## ? Checklist Final

- [ ] Script SQL ejecutado exitosamente
- [ ] Usuario tiene `EmpresaActivaId = 1`
- [ ] Usuario tiene empresa asignada en `UsuarioEmpresas`
- [ ] Sesión cerrada en la aplicación web
- [ ] Sesión iniciada nuevamente
- [ ] Token contiene claim `EmpresaId = 1` (verificado en consola)
- [ ] Parámetro se puede crear sin error 403

---

**¡Una vez completados todos los checkmarks, el sistema funcionará correctamente!** ?

---

_Última actualización: 17/02/2026_
