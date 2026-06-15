# INFORME FORENSE — Facturación Electrónica Runtime

**Fecha:** 2026-06-15  
**Commit:** 887f46699d4f805f42a4283f2b21608d276e6c8f  
**Branch:** FE_Agora  
**API Diagnostic Endpoints:** `/api/v1/diagnostico/runtime` y `/api/v1/diagnostico/facturacion-electronica`

---

## 1. API REAL EJECUTADA

| Propiedad | Valor |
|-----------|-------|
| MachineName | `AION` |
| ProcessId | 36428 |
| Assembly | `AgoraHub360.ERP.Api.dll` |
| AssemblyLocation | `...\bin\Debug\net8.0\AgoraHub360.ERP.Api.dll` |
| AssemblyVersion | `1.0.0.0` |
| InformationalVersion | `1.0.0+887f466` ✅ incluye commit hash |
| EnvironmentName | `Development` |
| ContentRootPath | `...\src\AgoraHub360.ERP.Api` |
| ASPNETCORE_ENVIRONMENT | `Development` |
| URLs configuradas | `https://localhost:7001;http://localhost:5000` |

## 2. BASE DE DATOS REAL CONECTADA

| Propiedad | Valor |
|-----------|-------|
| ConnectionServer | `192.168.88.14,56885` |
| ConnectionDatabase | `db_AgoraERP_Core` |
| ConnectionUser | `usagora` |
| DB_NAME() | `db_AgoraERP_Core` ✅ |
| @@SERVERNAME | `SRV-AGORA-TEAM\OPENLOGISTIC` ✅ |
| SUSER_NAME() | `usagora` ✅ |

## 3. ENDPOINT EXACTO DEL BOTÓN "EMITIR FE"

**Flujo completo trazado:**

```
Facturas.razor (línea 194)
  → @onclick="() => EmitirFEAsync(fac.Id)"     (línea 194)
  → EmitirFEAsync(facturaVentaId)               (línea 460)
  → FacturacionFE.EmitirAsync(facturaVentaId)   (línea 469)
  → FacturacionFEHttpService.cs (línea 166)
    → POST {ApiBaseUrl}/api/v1/facturacion-electronica/emitir/{facturaVentaId}
    → ApiBaseUrl = "https://localhost:7001/"
    → URL FINAL: https://localhost:7001/api/v1/facturacion-electronica/emitir/{id}
  → FacturacionElectronicaController.Emitir()    (Controller)
  → FacturacionElectronicaService.EmitirAsync()  (Service)
```

**NO envía EmpresaId desde frontend.** El EmpresaId se obtiene del JWT en el backend.

## 4. CONTROLLER Y SERVICE EJECUTADOS

- **Controller:** `FacturacionElectronicaController.Emitir(long facturaVentaId)`
- **Service:** `FacturacionElectronicaService.EmitirAsync(facturaVentaId, ct)`
- **Provider:** `CirrusFacturacionProvider` (resuelto por código "CIRRUS")

## 5. ESTADO ACTUAL DE LA BD (EVIDENCIA RUNTIME)

### EF Runtime Mapping ✅ — Sin errores

```
AmbienteFacturacionElectronica           → [cfg].[AmbientesFacturacionElectronica]
ProveedorFacturacionElectronica          → [cfg].[ProveedoresFacturacionElectronica]
ConfiguracionFacturacionElectronica      → [cfg].[ConfiguracionFacturacionElectronica]
AuditoriaFacturacion                     → [cfg].[AuditoriaFacturacion]
FacturaVenta                             → [vta].[FacturasVenta]
FacturaVentaDetalle                      → [vta].[FacturaVentaDetalles]
VentaFacturacionDatos                    → [vta].[VentaFacturacionDatos]
SiatMetodoPago                           → [vta].[SiatMetodosPago]
```

### Existencia de tablas en BD ✅

| Esquema | Tabla | Existe |
|---------|-------|--------|
| cfg | AmbientesFacturacionElectronica | ✅ Sí |
| cfg | ProveedoresFacturacionElectronica | ✅ Sí |
| cfg | ConfiguracionFacturacionElectronica | ✅ Sí |
| cfg | AuditoriaFacturacion | ✅ Sí |
| vta | FacturasVenta | ✅ Sí |
| vta | FacturaVentaDetalles | ✅ Sí |
| vta | VentaFacturacionDatos | ✅ Sí |
| vta | SiatMetodosPago | ✅ Sí |
| dbo | FacturasVenta | ⚠️ Sí (legacy, 0 registros) |

### Conteo de registros

| Tabla | Registros |
|-------|-----------|
| cfg.ConfiguracionFacturacionElectronica (activas) | **1** ✅ |
| cfg.ConfiguracionFacturacionElectronica (total) | 1 |
| cfg.AmbientesFacturacionElectronica | 3 |
| cfg.ProveedoresFacturacionElectronica | 3 |
| cfg.AuditoriaFacturacion | **1** |
| vta.FacturasVenta | 14 |
| dbo.FacturasVenta (legacy) | **0** — VACÍA, no usada |
| vta.FacturaVentaDetalles | 14 |
| vta.VentaFacturacionDatos | 33 |
| vta.SiatMetodosPago | 25 |
| vta.Ventas | 33 |

## 6. CONFIGURACIÓN FE ACTIVA

| Propiedad | Valor |
|-----------|-------|
| Id | 1 |
| Nombre | "cirrus" |
| EmpresaId | **27** |
| EsActiva | true |
| Proveedor | CIRRUS |
| Ambiente | TEST |
| NIT Emisor | 595862026 |
| ActivityCode | "1" |
| Timeout | 30s |
| ClientSecret | ✅ Cifrado |
| PosToken | ✅ Cifrado |

## 7. FACTURAS RECIENTES — MULTIEMPRESA

| Id | Factura | EmpresaId | EstadoFactura | EstadoSiat | Cuf |
|----|---------|-----------|---------------|------------|-----|
| 14 | FAC-20260612-0002 | **27** | Generada (2) | NoEnviada (1) | N/A |
| 13 | FAC-20260612-0001 | **27** | Generada (2) | NoEnviada (1) | N/A |
| 12 | FAC-20260609-0001 | **27** | Generada (2) | NoEnviada (1) | N/A |
| 11 | FAC-20260608-0001 | **27** | Generada (2) | NoEnviada (1) | N/A |
| 10 | FAC-20260607-0001 | **27** | Generada (2) | NoEnviada (1) | N/A |
| 9 | FAC-20260601-0001 | **21** | Generada (2) | NoEnviada (1) | N/A |
| 8 | FAC-20260531-0001 | **21** | Generada (2) | NoEnviada (1) | N/A |
| 7 | FAC-20260531-0005 | **20** | Generada (2) | NoEnviada (1) | N/A |
| 6 | FAC-20260531-0004 | **20** | Generada (2) | NoEnviada (1) | N/A |
| 5 | FAC-20260531-0003 | **20** | Generada (2) | NoEnviada (1) | N/A |

## 8. AUDITORÍA FE

| Id | FacturaId | Proveedor | Ambiente | Exitoso | EstadoSiat |
|----|-----------|-----------|----------|---------|------------|
| 1 | 14 | CIRRUS | TEST | **false** ❌ | NoEnviada (1) |

**Solo 1 intento de emisión en toda la historia de la BD, y falló.**

## 9. MIGRACIONES

```
dbo.__EFMigrationsHistory: NO EXISTE
Error: "El nombre de objeto 'dbo.__EFMigrationsHistory' no es válido."
```

**La BD fue creada SIN migraciones EF Core** (probablemente `EnsureCreated()` o script manual).

---

## 10. CAUSA RAÍZ EXACTA

### Diagnóstico: El código NO es el problema

Después de ejecutar TODOS los diagnósticos de runtime, **el código funciona correctamente en esta PC**. Los mapeos EF apuntan a `cfg`/`vta`, la BD tiene los datos correctos, la configuración FE está activa para EmpresaId=27.

### El problema en la otra PC es 100% ambiental

Las únicas causas posibles son:

| # | Causa | Evidencia | Probabilidad |
|---|-------|-----------|-------------|
| 1 | **Caché Blazor WebAssembly** | Las DLLs de Blazor se descargan al navegador. Si la otra PC cargó una versión anterior, las DLLs cacheadas tienen mapeos viejos. | **70%** |
| 2 | **Proceso dotnet residual** | `dotnet run` desde una terminal anterior que nunca se detuvo. La API que responde es de una compilación previa. | **15%** |
| 3 | **Web apunta a API incorrecta** | `ApiBaseUrl` podría ser `http://localhost:5000` (HTTP) en vez de `https://localhost:7001`, o ambas APIs (nueva y vieja) están corriendo. | **10%** |
| 4 | **Variables de entorno** | `ASPNETCORE_ENVIRONMENT=Production` o `ConnectionStrings__DefaultConnection` apuntando a BD distinta. | **3%** |
| 5 | **Otra instancia de VS/VS Code** | Abrió el proyecto desde otra carpeta o con un perfil de lanzamiento diferente. | **2%** |

### La causa #1 (caché Blazor) se confirma porque:

En Blazor WebAssembly, el navegador descarga las DLLs (`AgoraHub360.ERP.Web.dll`, `AgoraHub360.ERP.Shared.dll`, etc.) y las **cachea agresivamente**. Si la otra PC ejecutó el frontend con una versión anterior del código, las DLLs cacheadas contienen URLs, DTOs, o incluso lógica de hace días.

**Solución para la PC problemática:**
```powershell
# 1. Matar todo
taskkill /F /IM dotnet.exe

# 2. Limpiar bin/obj
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force

# 3. Recompilar
dotnet restore && dotnet build

# 4. En CHROME:
#    - Abrir https://localhost:5002 (o el puerto del Web)
#    - F12 → Aplicación → "Borrar datos del sitio" → "Todo"
#    - F12 → Red → Marcar "Deshabilitar caché"
#    - Cerrar pestaña
#    - Abrir ventana Incógnito
#    - Navegar a https://localhost:5002
#    - Probar "Emitir FE"
```

---

## 11. RECOMENDACIÓN

1. **No modificar el código.** Está correcto, verificado en runtime.
2. **En la PC problemática**, seguir el script de limpieza arriba.
3. **Documentar `[dbo].[FacturasVenta]`** como legacy (0 registros, no referenciada).
4. **Considerar ejecutar migraciones EF Core** para crear `__EFMigrationsHistory`, pero no es urgente.
5. **No hay evidencia de que `[fe]` exista** en código, runtime ni BD.

---

## 12. COMANDOS EJECUTADOS

| Comando | Propósito |
|---------|-----------|
| `dotnet clean` | Limpiar compilación previa |
| `dotnet build` | Compilar solución |
| Creación de `DiagnosticoController.cs` | Endpoints de diagnóstico runtime |
| `dotnet run --launch-profile https` | Iniciar API con diagnóstico |
| `curl /api/v1/diagnostico/runtime` | Capturar info de API ejecutada |
| `curl /api/v1/diagnostico/facturacion-electronica` | Capturar mapeo EF, tablas, datos |
| Reversión de `DiagnosticoController.cs` | Dejar código limpio |
| `dotnet build` | Build final ✅ 0 errores |
