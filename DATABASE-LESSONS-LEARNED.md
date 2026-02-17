# ?? Lecciones Aprendidas - Configuración de Base de Datos

## ?? Problema y Solución

### Contexto
Al intentar ejecutar migraciones de Entity Framework Core con `dotnet ef database update`, se producía un error de conexión a SQL Server, a pesar de que la aplicación en runtime podía conectarse correctamente.

---

## ?? Descubrimiento Clave

### El Problema de los "Dos Contextos"

Entity Framework Core opera en **dos modos diferentes**:

1. **Runtime (Aplicación en ejecución)**
   - Usa `Startup.cs` o `Program.cs`
   - Lee `appsettings.json` automáticamente
   - Inyecta `DbContext` con configuración

2. **Design-Time (Herramientas de migración)**
   - Usa `IDesignTimeDbContextFactory<T>`
   - NO lee `appsettings.json` automáticamente
   - Necesita configuración explícita

### La Confusión Común

? **Error típico:** Asumir que cambiar `appsettings.json` es suficiente para que las migraciones funcionen.

? **Realidad:** Las migraciones usan `AgoraDbContextFactory`, que necesita ser configurado para leer `appsettings.json`.

---

## ??? Soluciones y Mejores Prácticas

### 1?? Configuración Dinámica del DbContextFactory

**? Mala Práctica (Hardcoded):**
```csharp
public class AgoraDbContextFactory : IDesignTimeDbContextFactory<AgoraDbContext>
{
    public AgoraDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AgoraDbContext>();
        
        // ? Hardcoded - se desincroniza de appsettings.json
        optionsBuilder.UseSqlServer("Server=localhost;Database=...");
        
        return new AgoraDbContext(optionsBuilder.Options);
    }
}
```

**? Buena Práctica (Configuración Dinámica):**
```csharp
public class AgoraDbContextFactory : IDesignTimeDbContextFactory<AgoraDbContext>
{
    public AgoraDbContext CreateDbContext(string[] args)
    {
        // Leer desde appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "AgoraHub360.ERP.Api"))
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");

        var optionsBuilder = new DbContextOptionsBuilder<AgoraDbContext>();
        optionsBuilder.UseSqlServer(connectionString, 
            sqlOptions => sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "core"));

        return new AgoraDbContext(optionsBuilder.Options);
    }
}
```

**Ventajas:**
- ? Una sola fuente de verdad (`appsettings.json`)
- ? Soporta múltiples ambientes (Development, Staging, Production)
- ? Fácil mantenimiento
- ? No requiere recompilar para cambiar conexión

---

### 2?? Sintaxis de Connection String para SQL Server

#### Caso 1: Puerto Específico (Recomendado)
```
Server=IP,PUERTO;Database=...
```

**Ejemplos:**
```
? Server=192.168.88.14,56885;Database=db_AgoraERP_Core;...
? Server=localhost,1433;Database=...
? Server=sqlserver.example.com,1433;Database=...
```

**Cuándo usar:**
- Puerto no estándar (diferente de 1433)
- Múltiples instancias en el mismo servidor
- Configuraciones corporativas con puertos específicos

#### Caso 2: Instancia Nombrada (Sin Puerto)
```
Server=IP\INSTANCIA;Database=...
```

**Ejemplos:**
```
? Server=192.168.88.14\SQLEXPRESS;Database=...
? Server=localhost\OPENLOGISTIC;Database=...
```

**Cuándo usar:**
- Instancias nombradas de SQL Server
- SQL Server Express
- Configuración por defecto sin puerto personalizado

#### ? Error Común: Mezclar Ambos
```
? Server=192.168.88.14,56885\OPENLOGISTIC;Database=...
```
**Problema:** Cuando especificas un puerto, el servidor ya sabe a qué instancia conectarse. Agregar `\INSTANCIA` causa confusión y errores.

---

### 3?? Paquetes NuGet Necesarios

Para que el `DbContextFactory` pueda leer archivos JSON:

```xml
<!-- En tu proyecto .Persistence.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
  <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.1" />
  <PackageReference Include="Microsoft.Extensions.Configuration.FileExtensions" Version="8.0.1" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.12">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

---

### 4?? Configuración de Seguridad SSL/TLS

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "...;TrustServerCertificate=true;Encrypt=false"
  }
}
```

**Opciones:**

| Configuración | Propósito | Cuándo usar |
|---------------|-----------|-------------|
| `TrustServerCertificate=true` | Confía en el certificado del servidor sin validación | Desarrollo, redes internas |
| `TrustServerCertificate=false` | Valida el certificado SSL | Producción, redes públicas |
| `Encrypt=true` | Encripta la conexión | Producción, datos sensibles |
| `Encrypt=false` | No encripta la conexión | Desarrollo, redes internas seguras |

**Recomendaciones:**
- **Desarrollo:** `TrustServerCertificate=true; Encrypt=false`
- **Producción:** `TrustServerCertificate=false; Encrypt=true` (con certificado válido)

---

## ?? Checklist de Configuración de BD

Usa este checklist al configurar un nuevo proyecto:

### Configuración Básica
- [ ] Cadena de conexión en `appsettings.json`
- [ ] `DbContextFactory` lee desde `appsettings.json`
- [ ] Paquetes NuGet de configuración instalados
- [ ] Sintaxis correcta de connection string

### Validación
- [ ] Test de conexión directa con `sqlcmd`
- [ ] `dotnet ef migrations list` funciona
- [ ] `dotnet ef database update` funciona
- [ ] Aplicación conecta en runtime

### Seguridad
- [ ] Password no en código fuente
- [ ] Variables de entorno para producción
- [ ] Configuración SSL/TLS apropiada
- [ ] Secrets manejados correctamente

---

## ?? Scripts de Diagnóstico

### Script 1: Test Completo
```powershell
# quick-db-test.ps1
# - Verifica red
# - Prueba SQL Server
# - Valida migraciones
# - No requiere API corriendo
```

### Script 2: Test con API
```powershell
# test-database-connection.ps1
# - Requiere API corriendo
# - Tests más detallados
# - Verifica datos
# - Genera reporte JSON
```

---

## ?? Documentación de Referencia

### Microsoft Docs
- [Design-time DbContext Creation](https://learn.microsoft.com/en-us/ef/core/cli/dbcontext-creation)
- [Connection Strings](https://learn.microsoft.com/en-us/sql/connect/ado-net/connection-string-syntax)
- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)

### Documentación del Proyecto
- `DATABASE-TEST-GUIDE.md` - Guía de testing
- `DATABASE-CONNECTION-FIX.md` - Solución de problemas
- `QUICK-START-DB-TEST.md` - Quick start

---

## ?? Tips Adicionales

### 1. Usar Variables de Entorno
```csharp
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? configuration.GetConnectionString("DefaultConnection");
```

### 2. Soporte Multi-Ambiente
```csharp
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
configuration.AddJsonFile($"appsettings.{environment}.json", optional: true);
```

### 3. Logging en DbContextFactory
```csharp
public AgoraDbContext CreateDbContext(string[] args)
{
    Console.WriteLine($"[DbContextFactory] Reading configuration from: {basePath}");
    // ...configuración...
    Console.WriteLine($"[DbContextFactory] Connection string: {MaskConnectionString(connectionString)}");
    return new AgoraDbContext(optionsBuilder.Options);
}
```

---

## ?? Errores Comunes y Soluciones

| Error | Causa | Solución |
|-------|-------|----------|
| "Server not found" | Sintaxis incorrecta de connection string | Revisar formato `Server=IP,PUERTO` |
| "Cannot find appsettings.json" | Ruta incorrecta en `DbContextFactory` | Verificar `SetBasePath()` |
| "Connection string not found" | Clave incorrecta en `GetConnectionString()` | Verificar nombre en `appsettings.json` |
| "Named Pipes error" | Puerto incorrecto o servidor no accesible | Probar con `sqlcmd` primero |

---

## ? Resumen de Mejores Prácticas

1. **? Una sola fuente de verdad:** `appsettings.json` para todas las configuraciones
2. **? DbContextFactory dinámico:** Lee configuración, no hardcodea
3. **? Sintaxis correcta:** Puerto O instancia, no ambos
4. **? Paquetes necesarios:** Incluir Microsoft.Extensions.Configuration.*
5. **? Scripts de testing:** Automatizar validación de conexión
6. **? Documentación:** Mantener guías actualizadas
7. **? Seguridad:** Configurar SSL/TLS apropiadamente
8. **? Multi-ambiente:** Soportar Development, Staging, Production

---

**?? Recuerda:** El tiempo invertido en una configuración correcta de base de datos ahorra horas de debugging futuro.

_Última actualización: $(Get-Date -Format "dd/MM/yyyy HH:mm:ss")_
