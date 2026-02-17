# ?? Solución de Problema: Error de Conexión en Migraciones

## ?? Problema Identificado

**Error Original:**
```
A network-related or instance-specific error occurred while establishing a connection to SQL Server. 
The server was not found or was not accessible. 
(provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
```

---

## ?? Diagnóstico

El problema tenía **dos causas principales**:

### 1?? Cadena de Conexión Incorrecta en `appsettings.json`
**Antes:**
```json
"Server=192.168.88.14,56885\\OPENLOGISTIC;Database=db_AgoraERP_Core;..."
```

**Problema:** La sintaxis `\\OPENLOGISTIC` es incorrecta cuando se especifica un puerto. El puerto `56885` ya indica la instancia específica de SQL Server.

**Después:**
```json
"Server=192.168.88.14,56885;Database=db_AgoraERP_Core;..."
```

**Cambios adicionales:**
- `TrustServerCertificate=false` ? `TrustServerCertificate=true` (para evitar problemas con certificados SSL)

---

### 2?? Cadena de Conexión Hardcodeada en `AgoraDbContextFactory`

**Antes:**
```csharp
optionsBuilder.UseSqlServer(
    "Server=localhost;Database=AgoraHub360;Trusted_Connection=true;TrustServerCertificate=true",
    sqlOptions => sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "core"));
```

**Problema:** El `AgoraDbContextFactory` (usado por EF Core para ejecutar migraciones) tenía una cadena de conexión hardcodeada que apuntaba a `localhost`, ignorando completamente la configuración en `appsettings.json`.

**Después:**
```csharp
// Leer configuración desde appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "AgoraHub360.ERP.Api"))
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

optionsBuilder.UseSqlServer(connectionString, ...);
```

---

## ? Solución Implementada

### Archivos Modificados:

1. **`src/AgoraHub360.ERP.Api/appsettings.json`**
   - ? Corregida la cadena de conexión
   - ? Eliminada la sintaxis incorrecta `\\OPENLOGISTIC`
   - ? Cambiado `TrustServerCertificate` a `true`

2. **`src/AgoraHub360.ERP.Persistence/Context/AgoraDbContextFactory.cs`**
   - ? Implementada lectura dinámica desde `appsettings.json`
   - ? Eliminada cadena de conexión hardcodeada
   - ? Agregado soporte para `appsettings.Development.json`

3. **`src/AgoraHub360.ERP.Persistence/AgoraHub360.ERP.Persistence.csproj`**
   - ? Agregados paquetes NuGet necesarios:
     - `Microsoft.Extensions.Configuration`
     - `Microsoft.Extensions.Configuration.Json`
     - `Microsoft.Extensions.Configuration.FileExtensions`
     - `Microsoft.EntityFrameworkCore.Design`

---

## ?? Validación

### ? Test 1: Conexión SQL Directa
```powershell
sqlcmd -S "192.168.88.14,56885" -U usagora -P "Sinnada123.**" -d db_AgoraERP_Core -Q "SELECT @@VERSION"
```
**Resultado:** ? Exitoso - SQL Server 2019 Enterprise Edition

### ? Test 2: Migraciones de Entity Framework
```powershell
dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api
```
**Resultado:** ? Exitoso - 4 migraciones aplicadas:
- `20260217065412_BaseCore`
- `20260217084352_AddRolesTable`
- `20260217090218_AddAuditLogTable`
- `20260217091857_AddParametroSistemaNumeracionDocumento`

### ? Test 3: Quick DB Test
```powershell
.\quick-db-test.ps1
```
**Resultado:** ? Todos los tests pasaron

---

## ?? Lecciones Aprendidas

### 1. Sintaxis de Connection String para SQL Server

**Cuando uses puerto específico:**
```
Server=IP,PUERTO;Database=...
```
? Correcto: `Server=192.168.88.14,56885;Database=...`
? Incorrecto: `Server=192.168.88.14,56885\INSTANCIA;Database=...`

**Cuando uses instancia nombrada (sin puerto):**
```
Server=IP\INSTANCIA;Database=...
```
? Correcto: `Server=192.168.88.14\OPENLOGISTIC;Database=...`

### 2. Design-Time DbContext Factory

El `IDesignTimeDbContextFactory<T>` es usado por:
- `dotnet ef migrations add`
- `dotnet ef database update`
- `dotnet ef migrations script`

**Mejores prácticas:**
- ? Leer configuración desde `appsettings.json`
- ? NO hardcodear cadenas de conexión
- ? Soportar múltiples ambientes (`appsettings.Development.json`, etc.)
- ? Incluir manejo de errores claro

### 3. Paquetes Necesarios

Para que el `DbContextFactory` pueda leer `appsettings.json`:
```xml
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.1" />
<PackageReference Include="Microsoft.Extensions.Configuration.FileExtensions" Version="8.0.1" />
```

---

## ?? Estado Final

? **Base de datos:** Operativa y accesible  
? **Migraciones:** Todas aplicadas correctamente  
? **Conexión:** Validada en runtime y design-time  
? **Configuración:** Centralizada en `appsettings.json`  

---

## ?? Recursos Relacionados

- [DATABASE-TEST-GUIDE.md](DATABASE-TEST-GUIDE.md) - Guía de testing
- [QUICK-START-DB-TEST.md](QUICK-START-DB-TEST.md) - Quick start
- [DATABASE-TEST-CHECKLIST.md](DATABASE-TEST-CHECKLIST.md) - Checklist de validación

---

## ?? Comandos Útiles

```powershell
# Test rápido de conexión
.\quick-db-test.ps1

# Aplicar migraciones
dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api

# Ver migraciones aplicadas
dotnet ef migrations list --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api

# Test SQL directo
sqlcmd -S "192.168.88.14,56885" -U usagora -P "Sinnada123.**" -d db_AgoraERP_Core -Q "SELECT @@VERSION"
```

---

**? Problema Resuelto Exitosamente**  
_Fecha: $(Get-Date -Format "dd/MM/yyyy HH:mm:ss")_
