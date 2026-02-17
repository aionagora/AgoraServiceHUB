# ? Quick Start - Test de Conexión a Base de Datos

## ?? Lo que necesitas saber en 30 segundos

? **Tu base de datos está OPERATIVA**
- Servidor: `192.168.88.14:56885\OPENLOGISTIC`
- Base de datos: `db_AgoraERP_Core`
- SQL Server 2019 Enterprise Edition

---

## ?? Test Rápido (30 segundos)

```powershell
# Desde la raíz del proyecto
.\quick-db-test.ps1
```

**Esto probará:**
- ? Conectividad de red
- ? Acceso a SQL Server
- ? Estado de migraciones

---

## ?? Test Completo (2 minutos)

### Opción A: Con scripts

```powershell
# Terminal 1: Inicia la API
cd src\AgoraHub360.ERP.Api
dotnet run

# Terminal 2: Ejecuta el test (en otra ventana)
.\test-database-connection.ps1
```

### Opción B: Con Swagger

1. Inicia la API:
   ```powershell
   cd src\AgoraHub360.ERP.Api
   dotnet run
   ```

2. Abre tu navegador: https://localhost:7001/swagger

3. Ejecuta el endpoint: `GET /api/v1/diagnostics/database-test`

---

## ?? Qué esperar

### ? Test Exitoso:
```
Database Connectivity: ? PASS
Read Monedas Table: ? PASS (3 monedas)
Read Roles Table: ? PASS (4 roles)
Database Migrations: ? PASS
```

### ?? Migraciones Pendientes:
Si ves migraciones pendientes, aplícalas:
```powershell
dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api
```

---

## ?? Si algo falla

### Error: "Cannot connect to database"
```powershell
# Verifica SQL Server
Get-Service | Where-Object {$_.Name -like "*SQL*"}

# Prueba conexión manual
sqlcmd -S "192.168.88.14,56885\OPENLOGISTIC" -U usagora -P "Sinnada123.**" -Q "SELECT @@VERSION"
```

### Error: "API not running"
```powershell
cd src\AgoraHub360.ERP.Api
dotnet run
```

---

## ?? Archivos Útiles

| Archivo | Descripción |
|---------|-------------|
| `quick-db-test.ps1` | Test rápido (sin API) |
| `test-database-connection.ps1` | Test completo (con API) |
| `DATABASE-TEST-GUIDE.md` | Guía detallada |
| `DATABASE-TEST-SUMMARY.md` | Resumen ejecutivo |

---

## ?? Comandos Esenciales

```powershell
# Compilar
dotnet build

# Ejecutar API
cd src\AgoraHub360.ERP.Api && dotnet run

# Aplicar migraciones
dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api

# Ver migraciones
dotnet ef migrations list --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api
```

---

## ? Listo para producción

Si todos los tests pasan:
- ? Base de datos operativa
- ? Migraciones aplicadas
- ? Datos iniciales (seed data)
- ? Health checks funcionando

**¡Estás listo para desarrollar! ??**

---

_Para más detalles, consulta [DATABASE-TEST-GUIDE.md](DATABASE-TEST-GUIDE.md)_
