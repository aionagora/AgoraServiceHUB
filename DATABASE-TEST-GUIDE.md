# ?? Guía para Testear la Conexión a la Base de Datos

## Configuración Actual

**Cadena de Conexión** (en `src/AgoraHub360.ERP.Api/appsettings.json`):
```
Server: 192.168.88.14,56885\OPENLOGISTIC
Database: db_AgoraERP_Core
User: usagora
```

---

## ? Métodos de Prueba

### **1?? Usando el Script PowerShell (Recomendado)**

Ejecuta el script desde la raíz del proyecto:

```powershell
.\test-database-connection.ps1
```

**Lo que hace el script:**
- ? Verifica que la API esté activa
- ? Prueba el Health Check
- ? Ejecuta tests detallados de base de datos:
  - Conectividad
  - Lectura de tablas (Monedas, Roles, Empresas)
  - Estado de migraciones
- ?? Guarda un reporte completo en JSON

---

### **2?? Usando Swagger UI (Manual)**

1. **Inicia la API:**
   ```powershell
   cd src\AgoraHub360.ERP.Api
   dotnet run
   ```

2. **Abre Swagger:**
   ```
   https://localhost:7001/swagger
   ```

3. **Ejecuta estos endpoints:**
   - **GET** `/api/v1/diagnostics/ping` - Verifica que la API esté activa
   - **GET** `/health` - Health check general
   - **GET** `/api/v1/diagnostics/database-test` - Test completo de BD

---

### **3?? Usando cURL o PowerShell (Línea de comandos)**

```powershell
# Test básico
Invoke-RestMethod -Uri "https://localhost:7001/api/v1/diagnostics/ping" -SkipCertificateCheck

# Health check
Invoke-RestMethod -Uri "https://localhost:7001/health" -SkipCertificateCheck

# Test completo de base de datos
Invoke-RestMethod -Uri "https://localhost:7001/api/v1/diagnostics/database-test" -SkipCertificateCheck | ConvertTo-Json -Depth 10
```

---

### **4?? Test directo con Entity Framework (Desde código)**

Si prefieres probar directamente desde código C#:

```csharp
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AgoraDbContext>();

// Probar conexión
var canConnect = await dbContext.Database.CanConnectAsync();
Console.WriteLine($"Conexión: {(canConnect ? "? OK" : "? FAIL")}");

// Contar registros
var monedasCount = await dbContext.Monedas.CountAsync();
Console.WriteLine($"Monedas: {monedasCount}");
```

---

## ?? Solución de Problemas

### ? Error: "Cannot connect to database"

**Posibles causas:**
1. SQL Server no está corriendo
2. Firewall bloqueando el puerto `56885`
3. Credenciales incorrectas
4. Instancia `OPENLOGISTIC` no disponible

**Soluciones:**
```powershell
# Verificar que SQL Server esté corriendo
Get-Service | Where-Object {$_.Name -like "*SQL*"}

# Probar conexión directa con sqlcmd
sqlcmd -S "192.168.88.14,56885\OPENLOGISTIC" -U usagora -P "Sinnada123.**" -Q "SELECT @@VERSION"

# Verificar conectividad de red
Test-NetConnection -ComputerName 192.168.88.14 -Port 56885
```

---

### ?? Migraciones Pendientes

Si el test indica migraciones pendientes, aplícalas:

```powershell
dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api
```

---

### ?? Cambiar Credenciales de Conexión

Edita `src/AgoraHub360.ERP.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=TU_BD;User Id=TU_USUARIO;Password=TU_PASSWORD;..."
  }
}
```

Para **desarrollo local** con Windows Authentication:

```json
"DefaultConnection": "Server=localhost;Database=AgoraHub360;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
```

---

## ?? Interpretación de Resultados

### ? Conexión Exitosa
```
Database Connectivity: ? PASS
Read Monedas Table: ? PASS (3 monedas)
Read Roles Table: ? PASS (4 roles)
Read Empresas Table: ? PASS
Database Migrations: ? PASS
```

### ?? Advertencias
```
Database Migrations: ?? WARNING (Hay migraciones pendientes)
```
**Acción:** Aplicar migraciones con `dotnet ef database update`

### ? Error Crítico
```
Database Connectivity: ? FAIL
Error: A network-related or instance-specific error occurred...
```
**Acción:** Verificar servidor SQL, credenciales y conectividad de red

---

## ?? Comandos Útiles

```powershell
# Compilar el proyecto
dotnet build

# Ejecutar la API
cd src\AgoraHub360.ERP.Api
dotnet run

# Aplicar migraciones
dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api

# Ver migraciones aplicadas
dotnet ef migrations list --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api

# Crear nueva migración
dotnet ef migrations add NombreMigracion --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api
```

---

## ?? Notas

- La conexión usa **SQL Server Authentication** (User Id y Password)
- El puerto `56885` es no estándar, asegúrate de que esté abierto
- La instancia es `OPENLOGISTIC`
- `Encrypt=false` está configurado para permitir conexiones sin cifrado
- `TrustServerCertificate=false` requiere certificado válido

---

## ?? Contacto y Soporte

Si los problemas persisten:
1. Verifica los logs de la API en la consola
2. Revisa el archivo `database-test-result_[timestamp].json`
3. Contacta al administrador de base de datos para verificar permisos del usuario `usagora`
