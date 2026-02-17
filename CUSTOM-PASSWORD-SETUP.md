# ?? CONTRASEÑA PERSONALIZADA CONFIGURADA

## ? Contraseña Actualizada

Se ha generado el script para actualizar la contraseña del administrador.

---

## ?? NUEVAS CREDENCIALES

```
Email:      admin@agorahub360.com
Usuario:    admin
Contraseña: Sinnada123.**
```

**Hash SHA256:**
```
qRRdC7zSbQSmlAFpL39uULwISrvadOAqkCzh5luVASw=
```

---

## ?? Instrucciones de Aplicación

### Opción 1: SQL Server Management Studio (SSMS)

1. Abre **SQL Server Management Studio**
2. Conecta a tu servidor SQL Server
3. Abre el archivo: `reset-admin-password-custom.sql`
4. Ejecuta el script (F5)

### Opción 2: Azure Data Studio

1. Abre **Azure Data Studio**
2. Conecta a la base de datos `AgoraHub360ERP`
3. File ? Open File ? `reset-admin-password-custom.sql`
4. Run (Ctrl+Shift+E)

### Opción 3: sqlcmd (Línea de comandos)

```bash
sqlcmd -S localhost,1433 -U sa -P TuPassword -i reset-admin-password-custom.sql
```

---

## ? Verificación

### 1. Ejecutar el Script SQL

Primero ejecuta el archivo `reset-admin-password-custom.sql` usando una de las opciones anteriores.

### 2. Iniciar el Sistema

```powershell
.\start-system.ps1
```

### 3. Acceder al Login

```
https://localhost:5002/login
```

### 4. Usar las Nuevas Credenciales

```
Email:      admin@agorahub360.com
Contraseña: Sinnada123.**
```

---

## ?? Verificación en Base de Datos

Después de ejecutar el script, verifica que la contraseña se actualizó:

```sql
-- Ver hash actual
SELECT 
    Id,
    NombreUsuario,
    Email,
    PasswordHash
FROM [core].[Usuarios]
WHERE Id = 1;
```

**El hash debe ser:**
```
qRRdC7zSbQSmlAFpL39uULwISrvadOAqkCzh5luVASw=
```

---

## ?? Detalles Técnicos

| Campo | Valor |
|-------|-------|
| **Usuario ID** | 1 |
| **Contraseña** | `Sinnada123.**` |
| **Método Hash** | SHA256 |
| **Hash (Base64)** | `qRRdC7zSbQSmlAFpL39uULwISrvadOAqkCzh5luVASw=` |
| **Longitud Hash** | 44 caracteres |
| **Tabla Afectada** | `[core].[Usuarios]` |
| **Registros Modificados** | 1 |

---

## ?? Generación del Hash

La contraseña se hashea usando SHA256:

```csharp
// C# - Como lo hace el sistema
using var sha = SHA256.Create();
var bytes = Encoding.UTF8.GetBytes("Sinnada123.**");
var hashBytes = sha.ComputeHash(bytes);
var hash = Convert.ToBase64String(hashBytes);
// Resultado: qRRdC7zSbQSmlAFpL39uULwISrvadOAqkCzh5luVASw=
```

```powershell
# PowerShell - Como lo genera el script
$password = "Sinnada123.**"
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$bytes = [System.Text.Encoding]::UTF8.GetBytes($password)
$hashBytes = $sha256.ComputeHash($bytes)
$hash = [Convert]::ToBase64String($hashBytes)
```

---

## ?? Importante

### ? Lo que SE modificó:
- Hash de contraseña del usuario admin (ID=1)

### ? Lo que NO se modificó:
- Email del usuario
- Nombre de usuario
- Rol asignado
- Empresa activa
- Otros datos del usuario
- Datos de otros usuarios
- Datos de empresas, productos, etc.

---

## ?? Solución de Problemas

### Error: "Cannot open database AgoraHub360ERP"

**Solución:** Verifica el nombre de la base de datos en tu servidor.

```sql
-- Ver bases de datos disponibles
SELECT name FROM sys.databases;
```

### Error: Login falla después de actualizar

**Verificar:**

1. **Hash correcto en BD:**
```sql
SELECT PasswordHash FROM [core].[Usuarios] WHERE Id = 1;
```

2. **Usuario activo:**
```sql
SELECT Activo FROM [core].[Usuarios] WHERE Id = 1;
```

3. **Empresa asignada:**
```sql
SELECT * FROM [core].[UsuarioEmpresas] WHERE UsuarioId = 1;
```

### Volver a la contraseña anterior

Si necesitas volver a `Admin123`:

```sql
UPDATE [core].[Usuarios]
SET PasswordHash = 'jGQjcjUok1QVxKXOLRG+HuJ95WI0Mf/Xa6cOye1xICM='
WHERE Id = 1;
```

---

## ?? Archivos Generados

| Archivo | Descripción |
|---------|-------------|
| `reset-password-custom.ps1` | Script PowerShell que genera SQL |
| `reset-admin-password-custom.sql` | Script SQL para ejecutar |
| `CUSTOM-PASSWORD-SETUP.md` | Este documento |

---

## ?? Resumen

```
? Contraseña personalizada: Sinnada123.**
? Hash generado: qRRdC7zSbQSmlAFpL39uULwISrvadOAqkCzh5luVASw=
? Script SQL creado: reset-admin-password-custom.sql
? Pendiente: Ejecutar el script SQL en tu base de datos
```

---

## ?? Siguientes Pasos

1. ? **Ejecutar el script SQL** (ver Opciones 1, 2 o 3 arriba)
2. ? **Iniciar el sistema** (`.\start-system.ps1`)
3. ? **Hacer login** con las nuevas credenciales
4. ? **Verificar acceso** al sistema

---

**Fecha:** 2026-02-17  
**Contraseña:** `Sinnada123.**`  
**Estado:** ? Pendiente de aplicar en BD
