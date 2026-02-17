# ?? Credenciales de Acceso por Defecto

## ? Usuario Administrador Creado

Se ha creado automáticamente un usuario administrador para que puedas acceder al sistema.

---

## ?? Credenciales de Acceso

| Campo | Valor |
|-------|-------|
| **Email** | `admin@agorahub360.com` |
| **Contraseña** | `Admin123` |
| **Usuario** | `admin` |
| **Nombre Completo** | Administrador del Sistema |
| **Rol** | Admin |
| **Empresa Activa** | AgoraHub360 - Empresa Demo |

---

## ?? Empresa Demo Creada

Junto con el usuario administrador, se creó una empresa de demostración:

| Campo | Valor |
|-------|-------|
| **Nombre** | AgoraHub360 - Empresa Demo |
| **NIT** | 1234567890 |
| **Dirección** | Av. Principal #123 |
| **Teléfono** | +591 12345678 |
| **Email** | demo@agorahub360.com |
| **Moneda Base** | BOB (Boliviano) |

---

## ?? Cómo Iniciar Sesión

### Opción 1: Usando la Aplicación Web Blazor

1. **Inicia la API:**
   ```powershell
   cd src\AgoraHub360.ERP.Api
   dotnet run
   ```
   La API estará disponible en: `https://localhost:7001`

2. **Inicia la aplicación Web:**
   ```powershell
   cd src\AgoraHub360.ERP.Web
   dotnet run
   ```
   La aplicación estará disponible en: `https://localhost:5002`

3. **Accede al login:**
   - Abre tu navegador en: `https://localhost:5002/login`
   - Ingresa el email: `admin@agorahub360.com`
   - Ingresa la contraseña: `Admin123`
   - Haz clic en "Iniciar Sesión"

### Opción 2: Usando Swagger (Prueba de API)

1. **Inicia la API:**
   ```powershell
   cd src\AgoraHub360.ERP.Api
   dotnet run
   ```

2. **Abre Swagger:**
   ```
   https://localhost:7001/swagger
   ```

3. **Prueba el login:**
   - Busca el endpoint: `POST /api/v1/auth/login`
   - Haz clic en "Try it out"
   - Ingresa el JSON:
     ```json
     {
       "email": "admin@agorahub360.com",
       "password": "Admin123"
     }
     ```
   - Haz clic en "Execute"
   - Copia el token JWT de la respuesta

4. **Autoriza con el token:**
   - Haz clic en el botón "Authorize" (??) en la parte superior de Swagger
   - Ingresa: `Bearer TU_TOKEN_AQUI`
   - Haz clic en "Authorize"

---

## ?? Seguridad

### ?? Importante - Cambiar Contraseña en Producción

Este usuario y contraseña son **SOLO PARA DESARROLLO**. 

**Antes de pasar a producción:**
1. ? Cambia la contraseña del usuario `admin`
2. ? Crea usuarios individuales para cada persona
3. ? Asigna roles apropiados según las necesidades
4. ? Desactiva o elimina la empresa demo
5. ? Implementa una política de contraseñas fuertes
6. ? Considera implementar autenticación de dos factores (2FA)

### ?? Hash de Contraseña

La contraseña está hasheada usando **SHA256**:
```
Password: Admin123
Hash: O2Esdae1BIpDX7bsgeUv+S1teVqLWpwXBw9qY8l6U7I=
```

**Nota:** En producción, se recomienda usar algoritmos más seguros como:
- **BCrypt** (recomendado)
- **Argon2** (más seguro)
- **PBKDF2** (alternativa)

---

## ?? Roles Disponibles

El sistema incluye los siguientes roles predefinidos:

| Rol | Descripción | Permisos |
|-----|-------------|----------|
| **Admin** | Administrador con acceso total | Acceso completo al sistema |
| **Manager** | Gerente con acceso a reportes y aprobaciones | Lectura + Aprobaciones + Reportes |
| **User** | Usuario operativo | Acceso a módulos asignados |
| **Viewer** | Solo lectura | Consulta únicamente |

---

## ??? Gestión de Usuarios

### Crear Nuevo Usuario (desde la UI)

1. Inicia sesión con el usuario admin
2. Ve a: **Sistema ? Usuarios**
3. Haz clic en "Nuevo Usuario"
4. Completa el formulario:
   - Usuario
   - Email
   - Contraseña (mínimo 6 caracteres)
   - Nombre Completo
5. Haz clic en "Crear Usuario"

### Asignar Rol a un Usuario

1. En la lista de usuarios, haz clic en el botón **???** (Asignar Rol)
2. Selecciona la empresa
3. Selecciona el rol (Admin, Manager, User, Viewer)
4. Haz clic en "Asignar"

---

## ?? Verificar Usuario Creado

### Usando SQL Server Management Studio (SSMS)

```sql
-- Ver el usuario admin
SELECT * FROM [core].[Usuarios] WHERE NombreUsuario = 'admin';

-- Ver la empresa demo
SELECT * FROM [core].[Empresas] WHERE Id = 1;

-- Ver la asignación de rol
SELECT 
    u.NombreUsuario,
    e.Nombre AS Empresa,
    ue.Rol
FROM [core].[UsuarioEmpresas] ue
INNER JOIN [core].[Usuarios] u ON ue.UsuarioId = u.Id
INNER JOIN [core].[Empresas] e ON ue.EmpresaId = e.Id
WHERE u.NombreUsuario = 'admin';
```

### Usando el endpoint de diagnóstico

```powershell
# Obtener token
$login = @{
    email = "admin@agorahub360.com"
    password = "Admin123"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:7001/api/v1/auth/login" `
    -Method Post -Body $login -ContentType "application/json" -SkipCertificateCheck

$token = $response.data.token
Write-Host "Token: $token"

# Verificar información del usuario
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri "https://localhost:7001/api/v1/auth/me" `
    -Headers $headers -SkipCertificateCheck | ConvertTo-Json
```

---

## ?? Datos Insertados por la Migración

La migración `AddDefaultAdminUser` insertó los siguientes registros:

### Tabla: `core.Empresas`
- **Id:** 1
- **Nombre:** AgoraHub360 - Empresa Demo
- **NIT:** 1234567890
- **MonedaBase:** BOB

### Tabla: `core.Usuarios`
- **Id:** 1
- **NombreUsuario:** admin
- **Email:** admin@agorahub360.com
- **PasswordHash:** O2Esdae1BIpDX7bsgeUv+S1teVqLWpwXBw9qY8l6U7I=
- **NombreCompleto:** Administrador del Sistema
- **EmpresaActivaId:** 1

### Tabla: `core.UsuarioEmpresas`
- **UsuarioId:** 1
- **EmpresaId:** 1
- **Rol:** Admin

---

## ?? Resetear Contraseña

Si olvidas la contraseña, puedes resetearla directamente en la base de datos:

```sql
-- Resetear a 'Admin123'
UPDATE [core].[Usuarios]
SET PasswordHash = 'O2Esdae1BIpDX7bsgeUv+S1teVqLWpwXBw9qY8l6U7I='
WHERE NombreUsuario = 'admin';
```

O puedes generar un nuevo hash:

```powershell
# Generar hash de una nueva contraseña
$password = "NuevaContraseña123"
$bytes = [System.Text.Encoding]::UTF8.GetBytes($password)
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$hash = $sha256.ComputeHash($bytes)
$hashString = [Convert]::ToBase64String($hash)
Write-Host "Password hash: $hashString"
```

---

## ?? Soporte

Si tienes problemas para iniciar sesión:

1. **Verifica que la API esté corriendo:**
   ```powershell
   Test-NetConnection -ComputerName localhost -Port 7001
   ```

2. **Verifica que la base de datos esté accesible:**
   ```powershell
   .\quick-db-test.ps1
   ```

3. **Revisa los logs de la API** en la consola donde ejecutaste `dotnet run`

4. **Verifica que el usuario existe en la base de datos:**
   ```sql
   SELECT * FROM [core].[Usuarios] WHERE Email = 'admin@agorahub360.com';
   ```

---

## ? Resumen

**Credenciales de Acceso:**
- ?? Email: `admin@agorahub360.com`
- ?? Contraseña: `Admin123`

**URLs:**
- ?? Aplicación Web: `https://localhost:5002`
- ?? API: `https://localhost:7001`
- ?? Swagger: `https://localhost:7001/swagger`

**¡Ya puedes iniciar sesión y comenzar a usar el sistema!** ??
