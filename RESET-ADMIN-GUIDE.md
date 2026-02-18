# ?? GUÍA DE RESETEO DE USUARIO ADMINISTRADOR

## ?? Descripción

Esta guía te ayudará a resetear el usuario administrador del sistema AgoraHub360 ERP cuando:
- Olvidaste la contraseña del administrador
- Necesitas restaurar las credenciales por defecto
- El usuario administrador está corrupto o bloqueado

---

## ?? Advertencias Importantes

- ? **Este proceso ELIMINARÁ el usuario admin existente**
- ? **Se perderán todas las configuraciones personalizadas del usuario**
- ? **Los datos de empresas, productos, clientes, etc. NO se verán afectados**
- ? **Solo afecta al usuario con ID = 1**

---

## ?? Métodos de Reseteo

### **Método 1: Script Automático (Recomendado)**

Este método ejecuta todo automáticamente si tienes el módulo SqlServer instalado.

```powershell
.\reset-admin-user.ps1
```

**Requisitos:**
- PowerShell 5.1 o superior
- Módulo SqlServer (se instala automáticamente si no existe)
- Permisos de administrador

**Proceso:**
1. Lee la configuración de `appsettings.json`
2. Genera el hash de contraseña
3. Conecta a la base de datos
4. Ejecuta el script de reseteo
5. Verifica el resultado

---

### **Método 2: Script Simple (Sin Dependencias)**

Genera un archivo SQL que puedes ejecutar manualmente.

```powershell
.\reset-admin-simple.ps1
```

**Resultado:**
Se crea el archivo `reset-admin-user.sql` que puedes ejecutar con:

#### Opción A - SQL Server Management Studio (SSMS)
```
1. Abre SSMS
2. Conecta a tu servidor
3. File ? Open ? File ? reset-admin-user.sql
4. Presiona F5 para ejecutar
```

#### Opción B - sqlcmd (Línea de comandos)
```bash
sqlcmd -S localhost,1433 -U sa -P TuPassword -i reset-admin-user.sql
```

#### Opción C - Azure Data Studio
```
1. Abre Azure Data Studio
2. Conecta a la base de datos
3. File ? Open File ? reset-admin-user.sql
4. Run (Ctrl+Shift+E)
```

---

### **Método 3: Manual (SQL Directo)**

Si prefieres ejecutar SQL directamente:

```sql
USE [AgoraHub360ERP];
GO

-- 1. Limpiar usuario existente
DELETE FROM [core].[UsuarioEmpresas] WHERE UsuarioId = 1;
DELETE FROM [core].[Usuarios] WHERE Id = 1;

-- 2. Crear usuario admin
SET IDENTITY_INSERT [core].[Usuarios] ON;

INSERT INTO [core].[Usuarios] 
    (Id, NombreUsuario, Email, PasswordHash, NombreCompleto, EmpresaActivaId, FechaCreacion, Activo)
VALUES 
    (1, 'admin', 'admin@agorahub360.com', 'jGQjcjUok1QVxKXOLRG+HuJ95WI0Mf/Xa6cOye1xICM=', 
     'Administrador del Sistema', 1, GETDATE(), 1);

SET IDENTITY_INSERT [core].[Usuarios] OFF;

-- 3. Asignar rol
INSERT INTO [core].[UsuarioEmpresas] (UsuarioId, EmpresaId, Rol)
VALUES (1, 1, 'Admin');

-- 4. Verificar
SELECT * FROM [core].[Usuarios] WHERE Id = 1;
SELECT * FROM [core].[UsuarioEmpresas] WHERE UsuarioId = 1;
```

---

## ?? Credenciales Resultantes

Después de cualquier método de reseteo, las credenciales serán:

| Campo | Valor |
|-------|-------|
| **Email** | `admin@agorahub360.com` |
| **Usuario** | `admin` |
| **Contraseña** | `Admin123` |
| **Rol** | Admin |
| **Empresa** | AgoraHub360 - Empresa Demo (ID: 1) |

---

## ? Verificación del Reseteo

### 1. Verificar en Base de Datos

```sql
-- Ver usuario
SELECT 
    U.Id,
    U.NombreUsuario,
    U.Email,
    U.Activo,
    U.EmpresaActivaId,
    E.Nombre AS EmpresaActiva
FROM [core].[Usuarios] U
LEFT JOIN [core].[Empresas] E ON U.EmpresaActivaId = E.Id
WHERE U.Id = 1;

-- Ver asignaciones de empresa
SELECT 
    UE.UsuarioId,
    UE.EmpresaId,
    UE.Rol,
    E.Nombre AS EmpresaNombre
FROM [core].[UsuarioEmpresas] UE
INNER JOIN [core].[Empresas] E ON UE.EmpresaId = E.Id
WHERE UE.UsuarioId = 1;
```

### 2. Probar Login

```powershell
# Iniciar sistema
.\start-system.ps1

# Acceder a
https://localhost:5002/login

# Credenciales
Email: admin@agorahub360.com
Password: Admin123
```

---

## ?? Solución de Problemas

### ? Error: "No se pudo conectar a la base de datos"

**Solución:**
1. Verifica que SQL Server esté corriendo
2. Confirma la cadena de conexión en `appsettings.json`
3. Verifica credenciales de SQL Server

```powershell
# Verificar servicio SQL Server
Get-Service MSSQLSERVER
```

### ? Error: "The INSERT statement conflicted with the FOREIGN KEY constraint"

**Causa:** No existe la empresa con ID = 1

**Solución:**
Primero crea la empresa demo:

```sql
SET IDENTITY_INSERT [core].[Empresas] ON;

INSERT INTO [core].[Empresas] 
    (Id, Nombre, NIT, Direccion, Telefono, Email, MonedaBaseId, FechaCreacion, Activo)
VALUES 
    (1, 'AgoraHub360 - Empresa Demo', '1234567890', 'Av. Principal #123', 
     '+591 12345678', 'demo@agorahub360.com', 'BOB', GETDATE(), 1);

SET IDENTITY_INSERT [core].[Empresas] OFF;
```

### ? Error: "Cannot insert explicit value for identity column"

**Causa:** IDENTITY_INSERT no está activado

**Solución:**
Asegúrate de usar `SET IDENTITY_INSERT` antes de insertar:

```sql
SET IDENTITY_INSERT [core].[Usuarios] ON;
-- INSERT statement
SET IDENTITY_INSERT [core].[Usuarios] OFF;
```

### ? Login falla: "Credenciales inválidas"

**Verificar hash de contraseña:**

```sql
SELECT PasswordHash FROM [core].[Usuarios] WHERE Id = 1;
```

Debe ser: `jGQjcjUok1QVxKXOLRG+HuJ95WI0Mf/Xa6cOye1xICM=`

Si es diferente, actualizar:

```sql
UPDATE [core].[Usuarios] 
SET PasswordHash = 'jGQjcjUok1QVxKXOLRG+HuJ95WI0Mf/Xa6cOye1xICM='
WHERE Id = 1;
```

---

## ?? Cómo Funciona el Hash de Contraseña

El sistema usa **SHA256** para hashear contraseñas:

```csharp
// C# (usado por el sistema)
using var sha = SHA256.Create();
var bytes = Encoding.UTF8.GetBytes("Admin123");
var hashBytes = sha.ComputeHash(bytes);
var hash = Convert.ToBase64String(hashBytes);
// Resultado: jGQjcjUok1QVxKXOLRG+HuJ95WI0Mf/Xa6cOye1xICM=
```

```powershell
# PowerShell
$password = "Admin123"
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$bytes = [System.Text.Encoding]::UTF8.GetBytes($password)
$hashBytes = $sha256.ComputeHash($bytes)
$hash = [Convert]::ToBase64String($hashBytes)
```

---

## ?? Cambiar Contraseña Después del Reset

### Método 1: Desde la aplicación
1. Login con `Admin123`
2. Ir a **Configuración ? Usuarios**
3. Editar usuario admin
4. Cambiar contraseña

### Método 2: SQL Directo
```sql
-- Para nueva contraseña: "MiPassword456"
-- Hash: <generar con script PowerShell>

UPDATE [core].[Usuarios]
SET PasswordHash = '<nuevo_hash>'
WHERE Id = 1;
```

---

## ?? Estado de las Tablas Afectadas

| Tabla | Operación | Impacto |
|-------|-----------|---------|
| `[core].[Usuarios]` | DELETE + INSERT | Usuario ID=1 recreado |
| `[core].[UsuarioEmpresas]` | DELETE + INSERT | Rol Admin reasignado |
| `[core].[Empresas]` | (Verificar) | Se crea si no existe ID=1 |
| `[core].[Roles]` | (Sin cambios) | Usa rol existente |
| Otras tablas | (Sin cambios) | No afectadas |

---

## ?? Casos de Uso

### 1. Olvidé la contraseña del admin
```powershell
.\reset-admin-simple.ps1
# Ejecutar el SQL generado
# Login con Admin123
```

### 2. Usuario admin corrupto
```powershell
.\reset-admin-user.ps1
# Verificar con
.\test-database-connection.ps1
```

### 3. Setup inicial de desarrollo
```powershell
# Ejecutar migraciones
dotnet ef database update --project src/AgoraHub360.ERP.Persistence

# Reset admin (opcional si ya existe)
.\reset-admin-user.ps1

# Iniciar sistema
.\start-system.ps1
```

---

## ?? Scripts Relacionados

| Script | Descripción |
|--------|-------------|
| `reset-admin-user.ps1` | Reset automático con SqlServer |
| `reset-admin-simple.ps1` | Genera SQL para ejecución manual |
| `test-database-connection.ps1` | Verifica conexión a BD |
| `start-system.ps1` | Inicia API + Web |
| `quick-db-test.ps1` | Test rápido de BD |

---

## ?? Referencias

- [CREDENCIALES-DEFAULT.md](CREDENCIALES-DEFAULT.md) - Credenciales por defecto
- [DATABASE-CONNECTION-FIX.md](DATABASE-CONNECTION-FIX.md) - Solución de problemas de conexión
- [DATABASE-TEST-GUIDE.md](DATABASE-TEST-GUIDE.md) - Guía de testing de BD
- [LOGIN-ERROR-FIX.md](LOGIN-ERROR-FIX.md) - Solución de errores de login

---

## ?? Seguridad

### Para Desarrollo ?
- Contraseña simple: `Admin123`
- Usuario conocido: `admin`
- Email genérico

### Para Producción ?
**NUNCA usar estas credenciales en producción:**

1. Cambiar contraseña inmediatamente
2. Usar contraseñas complejas (min. 12 caracteres)
3. Considerar autenticación de dos factores
4. Implementar políticas de contraseña
5. Rotar credenciales periódicamente

---

## ?? Soporte

Si tienes problemas:
1. Revisa la sección de Solución de Problemas
2. Verifica los logs de la API
3. Consulta la documentación del proyecto
4. Contacta al equipo de desarrollo

---

**Fecha:** 2024-01-17  
**Versión del Sistema:** 1.0  
**Última Actualización:** 2024-01-17
