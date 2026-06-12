# FE-00 AUDITORÍA INICIAL — READINESS CHECK

> **Proyecto:** AgoraHUB360 ERP
> **Fecha:** 2026-06-10
> **Propósito:** Diagnóstico técnico completo antes de implementar Facturación Electrónica
> **Modo:** Solo lectura / No code changes

---

## 1. Resumen Ejecutivo

### ¿El proyecto está listo para iniciar FE-S01?

**Estado: LISTO CON OBSERVACIONES**

El proyecto tiene una base sólida: Clean Architecture, multiempresa funcional, entidad `FacturaVenta` existente con campos SIAT, servicio `FacturaVentaService` implementado, enums de estado SIAT creados, y flujo venta→factura operativo. Sin embargo, existen brechas específicas que deben documentarse y planificarse antes de la implementación.

### Bloqueadores principales

| Prioridad | Bloqueador | Impacto |
|---|---|---|
| P1 | No existe `ICifradoService` — secretos (ClientSecret, PosToken) viajarían en texto plano | Seguridad |
| P1 | No existe `IFacturacionElectronicaProvider` ni patrón provider | Arquitectura |
| P1 | Blazor WebAssembly (no Server) — prompts FE asumen Server | UI |
| P2 | `BillUuid`, `Cuf`, `Cufd` existen en FacturaVenta pero podrían necesitar ajustes de longitud/tipo | Datos |
| P2 | `EstadoSiat` enum necesita mapeo a estados Cirrus | Integración |
| P2 | No existe esquema `cfg` en base de datos — se debe crear | BD |

**Ningún P0 (bloqueante total) detectado.** Se puede iniciar FE-S01 con ajustes menores.

---

## 2. Mapa Real de Arquitectura

| Capa | Proyecto | Responsabilidad | Patrones detectados | Observaciones |
|---|---|---|---|---|
| Domain | `AgoraHub360.ERP.Domain` | Entidades, enums, interfaces de repositorio | `TenantEntity`, `AuditableEntity`, `IRepository<T>`, `IUnitOfWork` | ✅ Sólido |
| Application | `AgoraHub360.ERP.Application` | Servicios de aplicación, interfaces, validadores | Service-per-entity, DI, Result<T> pattern | ✅ ~76 servicios |
| Persistence | `AgoraHub360.ERP.Persistence` | EF Core DbContext, configs, repos, migraciones | Repository<T>, QueryFilter global, AuditableEntityInterceptor | ✅ Robusto |
| Infrastructure | `AgoraHub360.ERP.Infrastructure` | Servicios externos | Register in DI, `IExportService` | ⚠️ Vacío para FE |
| API | `AgoraHub360.ERP.Api` | REST API, middleware, JWT | Controllers/V1, ApiResponse<T>, GlobalExceptionMiddleware, TenantRequiredMiddleware | ✅ Listo |
| Web | `AgoraHub360.ERP.Web` | UI Blazor, servicios HTTP | Blazor **WebAssembly** (no Server), AuthMessageHandler, JwtAuthStateProvider | ⚠️ Es WASM |
| Shared | `AgoraHub360.ERP.Shared` | DTOs, constantes, utilerías | DTOs por módulo, ApiResponse<T>, ClaimTypesCustom | ✅ |
| Tests | `AgoraHub360.ERP.Tests` | Pruebas unitarias | xUnit 2.4.2, FakeRepo<T>, sin Moq/FluentAssertions/WebApplicationFactory | ⚠️ Sin Moq |

### Namespaces reales

| Proyecto | Namespace raíz |
|---|---|
| Domain | `AgoraHub360.ERP.Domain` |
| Application | `AgoraHub360.ERP.Application` |
| Persistence | `AgoraHub360.ERP.Persistence` |
| Infrastructure | `AgoraHub360.ERP.Infrastructure` |
| API | `AgoraHub360.ERP.Api` |
| Web | `AgoraHub360.ERP.Web` |
| Shared | `AgoraHub360.ERP.Shared` |
| Tests | `AgoraHub360.ERP.Tests` |

---

## 3. Readiness de Ventas / FacturaVenta

| Elemento | Existe | Ruta | Estado | Brecha | Acción |
|---|---|---|---|---|---|
| `FacturaVenta` | ✅ Sí | `Domain/Entities/VTA/FacturaVenta.cs` | Completa | — | — |
| `Venta` | ✅ Sí | `Domain/Entities/VTA/Venta.cs` | Completa | — | — |
| `FacturaVentaDetalle` | ✅ Sí | `Domain/Entities/VTA/FacturaVentaDetalle.cs` | Completa | — | — |
| `VentaDetalle` | ✅ Sí | `Domain/Entities/VTA/VentaDetalle.cs` | Completa | — | — |
| `PedidoVenta` | ✅ Sí | `Domain/Entities/VTA/PedidoVenta.cs` | Completa | — | — |
| `PedidoVentaDetalle` | ✅ Sí | `Domain/Entities/VTA/PedidoVentaDetalle.cs` | Completa | — | — |
| `FacturaVenta.Id` | ✅ Sí | `long` | Correcto | — | — |
| `FacturaVenta.EmpresaId` | ✅ Sí | Hereda de `TenantEntity` | Aislado | — | — |
| `FacturaVenta.VentaId` | ✅ Sí | `long VentaId` + FK | Correcto | — | — |
| `FacturaVentaDetalle.FacturaVentaId` | ✅ Sí | `long FacturaVentaId` + FK | Correcto | — | — |
| `BillUuid` | ✅ Sí | `FacturaVenta.BillUuid` (string?) | Existe | Pendiente definir formato/longitud Cirrus | Validar en FE-S01 |
| `Cuf` | ✅ Sí | `FacturaVenta.Cuf` (string?) | Existe | Pendiente definir formato | Validar en FE-S01 |
| `Cufd` | ✅ Sí | `FacturaVenta.Cufd` (string?) | Existe | Pendiente definir formato | Validar en FE-S01 |
| `SiatQr` | ✅ Sí | `FacturaVenta.SiatQr` (string?) | Existe | — | — |
| `EnlacePdf` | ✅ Sí | `FacturaVenta.EnlacePdf` (string?) | Existe | — | — |
| `EnlaceXml` | ✅ Sí | `FacturaVenta.EnlaceXml` (string?) | Existe | — | — |
| `EstadoSiat` | ✅ Sí | `EstadoSiatFactura` enum | Existe | Valores: NoEnviada,Pendiente,Validada,Rechazada,Anulada | Mapear a Cirrus |
| `EstadoFactura` | ✅ Sí | `EstadoFacturaVentaComercial` enum + `EstadoFacturaVenta` | Dos enums | `EstadoFacturaVentaComercial`: Borrador,Generada,Anulada. `EstadoFacturaVenta`: NoGenerada,Pendiente,Generada,Anulada | Unificar o documentar diferencia |
| `ActivityCode` | ✅ Sí | `FacturaVenta.ActivityCode` (string?) | Existe | — | — |
| `ItemCode` | ✅ Sí | `FacturaVentaDetalle.ItemCode` (string?) | Existe | Se asigna desde `CompanyProduct.Sku` o `CompanyProduct.CodigoInterno` en FacturaVentaService | Correcto |
| `IFacturaVentaService` | ✅ Sí | `Application/Interfaces/IFacturaVentaService.cs` | Completa | `GetAllAsync(FacturaVentaFilterDto?, ...)`, crud completo | — |
| `FacturaVentaService` | ✅ Sí | `Application/Services/FacturaVentaService.cs` | Completo | `GenerarDesdeVentaAsync`, `AnularAsync` implementados | — |
| `FacturasVentaController` | ✅ Sí | `Api/Controllers/V1/FacturasVentaController.cs` | Completo | CRUD + generar/anular | — |
| `VentasController` | ✅ Sí | `Api/Controllers/V1/VentasController.cs` | Completo | CRUD + confirmar + pagos + anular | — |
| Flujo venta→factura | ✅ Sí | `FacturaVentaService.GenerarDesdeVentaAsync` | Opera | Crea FacturaVenta desde Venta confirmada | — |
| `FacturaVentaDto` | ✅ Sí | `Shared/DTOs/Ventas/FacturaVentaDtos.cs` | Completo | Incluye ActivityCode, ItemCode en detalle | — |
| Página Facturas (UI) | ✅ Sí | `Web/Pages/Ventas/Facturas.razor` | Existe | Lista paginada con filtros | — |
| Página Config FE | ❌ No | — | — | No existe sección de configuración FE | Crear en FE-S05 |
| `Shared/DTOs/FacturacionElectronica` | ❌ No | — | — | No existe el subdirectorio | Crear en FE-S03 |

---

## 4. Readiness Multiempresa / Tenant Isolation

| Control | Estado | Riesgo para FE | Acción mínima |
|---|---|---|---|
| `TenantEntity` con `EmpresaId` | ✅ Implementado | Bajo — FE hereda automáticamente | Ninguna |
| Query filter global (`ApplyTenantFilter`) | ✅ En AgoraDbContext.OnModelCreating | Bajo — FE usa DbContext existente | Ninguna |
| `ICurrentUserService.EmpresaId` | ✅ Desde claim JWT | Bajo | Ninguna |
| `CurrentUserService` en API | ✅ Implementado | Bajo | Ninguna |
| `TenantRequiredMiddleware` | ✅ Valida EmpresaId en rutas tenant-aware | Medio — verificar que ruta FE esté exenta o no | Añadir ruta FE a exempt paths si es necesario |
| Cambio de empresa en frontend | ⚠️ Vía `JwtAuthStateProvider` + estado local | **Alto** — si solo cambia estado local sin renovar token, el backend podría usar EmpresaId incorrecto | **Verificar** que `CambiarEmpresaActivaAsync` renueve token JWT |
| `AuthService.CambiarEmpresaActivaAsync` | ✅ Renueva token con nuevo EmpresaId | Bajo | Ninguna |
| Usos de `IgnoreQueryFilters` | Solo en `EmpresaDemoService` y `Repository` | Bajo — solo semilla | Ninguna |
| Servicios que aceptan EmpresaId desde DTO | ⚠️ No se detectaron en Application | Medio | Verificar en FE-S01 que no se acepte EmpresaId desde request |
| Riesgo de emitir factura con configuración de otra empresa | ⚠️ Potencial si config FE se guarda por empresa pero se lee sin filtro | **Alto** — la configuración FE (credenciales Cirrus) debe ser tenant-aware | Asegurar que `ConfiguracionFacturacionElectronica` herede de `TenantEntity` |

---

## 5. Readiness de Base de Datos / EF Core

| Esquema | Entidad | Configuración | Migraciones | Observaciones |
|---|---|---|---|---|
| `core` | `Empresa`, `Sucursal`, `Usuario`, etc. | ✅ `Configurations/*.cs` | ✅ Múltiples | — |
| `vta` | `Ventas`, `FacturasVenta`, `PedidosVenta` | ✅ `Configurations/VTA/*.cs` | ✅ `20260531110128_AddSiatFieldsToFacturaVenta` | Migración SIAT ya existe |
| `cxc` | `CuentasPorCobrar`, `ClienteCreditoConfiguraciones` | ✅ `Configurations/CXC/*.cs` | ✅ Reciente | — |
| `cfg` | ❌ No existe | ❌ | ❌ | **Crear esquema `cfg` para FE** |
| `ParametroSistema` | ✅ `Entities/Core/ParametroSistema.cs` | ✅ `Configurations/` | ✅ | No usar para FE — crear entidades dedicadas |
| `NumeracionDocumento` | ✅ `Entities/Core/NumeracionDocumento.cs` | ✅ | ✅ | Reutilizable para FE si aplica |

### Convenciones actuales

| Convención | Patrón detectado |
|---|---|
| Naming migraciones | `YYYYMMDDHHMMSS_DescripcionEnPascalCase.cs` |
| Esquemas | 3-letras minúsculas: `vta`, `mdm`, `acc`, `cxc`, `inv`, `cmp`, etc. |
| Tablas | Plural PascalCase: `FacturasVenta`, `CuentasPorCobrar` |
| PK | `Id` autoincremental (Identity) |
| FK | `EntidadId` (ej: `FacturaVentaId`) |
| Soft delete | `Activo` bool en `AuditableEntity` |

### Migración recomendada para FE-S01

```
20260610_AddFacturacionElectronicaSchema.sql
→ Crear esquema cfg y tablas:
  cfg.ConfiguracionFacturacionElectronica
  cfg.AuditoriaFacturacion
```

Nombre de migración C#:
```
20260610000000_AddFacturacionElectronicaSchema
```

---

## 6. Readiness de Seguridad (Secretos)

| Secreto | Ubicación actual | Riesgo | Recomendación |
|---|---|---|---|
| `ClientSecret` | No existe aún | 🔴 Crítico — si se guarda en texto plano | Implementar `ICifradoService` con AES-CBC + IV aleatorio |
| `PosToken` | No existe aún | 🔴 Crítico — si se guarda en texto plano | Misma recomendación |
| ConnectionString | `appsettings.json` (texto plano) | 🟡 Medio — acceso a BD | Ya existe, no empeorar con FE |
| JWT Key | `appsettings.json` (texto plano) | 🟡 Medio | Usar User Secrets en desarrollo |

### Diagnóstico de cifrado

| Búsqueda | Resultado |
|---|---|
| `ICifradoService` | ❌ No existe |
| `Encrypt/Decrypt/Cifrar/Descifrar` | ❌ No existe |
| `AES` en código | ❌ No existe |
| `PasswordHasher` | ❌ No existe (se usa hash manual en AuthService) |
| `Hash` en código | ✅ Existe en `AuthService` para password |

### Recomendación

| Aspecto | Detalle |
|---|---|
| Capa de interfaz | `Application/Interfaces/ICifradoService.cs` |
| Capa de implementación | `Infrastructure/Services/AesCifradoService.cs` |
| Algoritmo | AES-256-CBC con IV aleatorio de 16 bytes almacenado junto al ciphertext (formato: `Base64(IV + Ciphertext)`) |
| Registro DI | `Infrastructure/DependencyInjection.cs` |
| Logging | NO loggear secretos — usar `[Sensitive]` o `[Redacted]` |
| Configuración | `IConfiguration` — leer clave desde `appsettings.json` o User Secrets |

---

## 7. Readiness API

| Elemento | Estado | Observaciones |
|---|---|---|
| `ApiResponse<T>` | ✅ Usado en todos los controllers | Envoltura estándar |
| `ApiVersion("1.0")` | ✅ Todos los controllers versionados | Ruta: `/api/v1/...` |
| `[Authorize]` | ✅ Todos los controllers | FE debe ser `[Authorize]` |
| `[AllowAnonymous]` | Solo en login y health | FE no debe ser anónimo |
| `ProducesResponseType` | ✅ Usado en algunos controllers (ConciliacionBancaria, EstadosFinancieros) | Opcional — no es obligatorio |
| `GlobalExceptionMiddleware` | ✅ Captura excepciones no manejadas | Retorna 500 |
| `TenantRequiredMiddleware` | ✅ Valida EmpresaId | Añadir ruta FE si necesario |
| Patrón de controller | Similar a `FacturasVentaController` | Usar como template |
| Errores de negocio | Retornan `BadRequest(ApiResponse.Fail(error))` | FE debe seguir mismo patrón |
| `InvalidOperationException` | Capturada por `GlobalExceptionMiddleware` | Retorna 500 |
| `UnauthorizedAccessException` | Manejado por JWT + middleware | Retorna 401 |

### Endpoint FE propuesto vs patrón actual

| Endpoint propuesto | Patrón equivalente | Recomendación |
|---|---|---|
| `POST /api/v1/facturacion-electronica/emitir` | `FacturasVentaController.GenerarDesdeVenta` | Usar mismo patrón: `Result<T>` → `ApiResponse<T>` |
| `GET /api/v1/facturacion-electronica/{id}` | `FacturasVentaController.GetById` | Mismo patrón |
| `POST /api/v1/facturacion-electronica/anular` | `FacturasVentaController.Anular` | Mismo patrón |
| `GET /api/v1/facturacion-electronica/configuracion` | Sin equivalente exacto | Como `ParametrosController` |

### Manejo de rechazo Cirrus

Si Cirrus rechaza por regla de negocio, el endpoint debe retornar `HTTP 200` con `ApiResponse<T>.Fail("Motivo del rechazo")` — **NO** HTTP 400/500, porque la comunicación con Cirrus fue exitosa.

---

## 8. Readiness UI Blazor WebAssembly

| Elemento | Estado | Observaciones |
|---|---|---|
| Tipo de Blazor | ✅ **WebAssembly** (no Server) | Confirmado en `Program.cs`: `builder = WebAssemblyHostBuilder`, `AddTransient<AuthMessageHandler>()`, sin `AddInteractiveServerComponents()` |
| `HttpClient` | ✅ Configurado con `AuthMessageHandler` para JWT | Correcto |
| `AuthMessageHandler` | ✅ Interceptor que agrega Bearer token | Correcto |
| `JwtAuthStateProvider` | ✅ AuthenticationStateProvider personalizado | Correcto |
| `ApiResponse<T>` deserialización | ✅ Servicios HTTP devuelven `ApiResponse<T>` | Correcto |
| Servicios HTTP | ✅ ~50 servicios en `Services/` | Sin servicio de facturación aún |
| Página `Facturas.razor` | ✅ `/ventas/facturas` | Lista paginada con filtros |
| Página de configuración | ✅ `Config/` existe | Sin sección FE aún |
| Modales | ✅ `ModalRegistrarPago.razor` como ejemplo | Usar mismo patrón |
| Alertas/notificaciones | ✅ Alertas Bootstrap + mensajes de éxito/error | Usar mismo patrón |

### Ajustes necesarios para prompts FE-S05

| Aspecto | Cambio requerido |
|---|---|
| Reemplazar `AddInteractiveServerComponents()` | No usar — es WASM |
| Reemplazar `@rendermode InteractiveServer` | No existe en WASM |
| Reemplazar `builder.Services.AddServerSideBlazor()` | No existe en WASM |
| Mantener `builder.Services.AddScoped<HttpClient>(...)` | Ya existe con AuthMessageHandler |
| Mantener patrón de servicios HTTP | `ApiResponse<T>` deserializado con `System.Text.Json` |

---

## 9. Riesgos Antes de Implementar FE

### P1 — Importante

| ID | Riesgo | Descripción | Mitigación |
|---|---|---|---|
| R01 | **Secretos en texto plano** | No existe cifrado para ClientSecret/PosToken | Implementar `ICifradoService` en FE-S01 o S02 |
| R02 | **Sin provider pattern** | No existe `IFacturacionElectronicaProvider` | Crear interfaz + implementación Cirrus en FE-S01 |
| R03 | **UI asume Blazor Server** | Prompts FE originales asumen Server | Actualizar a WASM antes de FE-S05 |
| R04 | **Configuración FE sin tenant filter** | Riesgo de emitir factura con credenciales de otra empresa | `ConfiguracionFacturacionElectronica` debe heredar de `TenantEntity` |

### P2 — Mejora

| ID | Riesgo | Descripción |
|---|---|---|
| R05 | **Sin Moq/FluentAssertions en tests** | Tests usan FakeRepo manual — posiblemente insuficiente para mockear Cirrus |
| R06 | **Sin WebApplicationFactory** | No hay tests de integración |
| R07 | **Cobertura de tests baja** | Solo ~22 clases — ~46 controllers sin tests de seguridad |
| R08 | **Dos enums EstadoFactura** | `EstadoFacturaVentaComercial` y `EstadoFacturaVenta` pueden causar confusión |
| R09 | **ConnectionString en texto plano** | Ya existe en `appsettings.json` — no empeorar con FE |

---

## 10. Ajustes Recomendados al Plan FE

| Prompt original | Ajuste necesario |
|---|---|
| FE-S01 (Domain) | Verificar nombres exactos de entidades (ya existen). Documentar campos fiscales faltantes vs existentes. `BillUuid`, `Cuf`, `Cufd` ya existen — no recrear. |
| FE-S02 (Infrastructure) | Crear `ICifradoService` en Application, `AesCifradoService` en Infrastructure. Crear `IFacturacionElectronicaProvider` en Application, `CirrusFacturacionProvider` en Infrastructure. |
| FE-S03 (Persistence) | Usar esquema `cfg`. Migración: `20260610000000_AddFacturacionElectronicaSchema`. `ConfiguracionFacturacionElectronica` debe heredar de `TenantEntity`. |
| FE-S04 (API) | Endpoint: `/api/v1/facturacion-electronica`. Seguir patrón `FacturasVentaController`. No exponer secretos en DTOs. |
| FE-S05 (UI) | **Cambiar de Blazor Server a Blazor WebAssembly.** No usar `AddInteractiveServerComponents()`. No usar `@rendermode`. Usar `HttpClient` con `AuthMessageHandler`. |
| FE-S06 (Tests) | Sin Moq/FluentAssertions — considerar agregarlos. Usar FakeRepo existente para simplicidad inicial. |

---

## 11. Plan de Siguiente Paso

```
RESULTADO FE-00: LISTO CON OBSERVACIONES
```

```
SIGUIENTE PROMPT RECOMENDADO:
FE-S01 — Creación de entidades de dominio para Facturación Electrónica
(Ajustado: documentar que FacturaVenta, BillUuid, Cuf, Cufd, SiatQr, EnlacePdf, EnlaceXml, EstadoSiatFactura ya existen)
```

### Flujo recomendado corregido

| Paso | Sprint | Cambios respecto al plan original |
|---|---|---|
| 1 | **FE-S01** | Domain: Documentar entidades existentes + crear `ConfiguracionFacturacionElectronica` + `AuditoriaFacturacion` (nuevas en `Entities/FE/`) + extender `FacturaVenta` si es necesario |
| 2 | **FE-S02** | Infrastructure: `ICifradoService` + `AesCifradoService` + `IFacturacionElectronicaProvider` + `CirrusFacturacionProvider` |
| 3 | **FE-S03** | Persistence: esquema `cfg`, migración, configuraciones EF |
| 4 | **FE-S04** | API: Controller FacturacionElectronica, DTOs en Shared |
| 5 | **FE-S05** | UI: Páginas Blazor WASM (no Server), servicios HTTP |
| 6 | **FE-S06** | Tests |

### Prompts alternativos si hay bloqueo

| Prompt | Cuándo usarlo |
|---|---|
| `FE-00-B` | Si se detecta que ventas/facturas necesitan normalización (no es el caso) |
| `FE-00-C` | Solo si el tenant isolation tiene fallas críticas (no parece ser el caso) |
| `FE-00-D` | Si faltan campos fiscales en FacturaVenta (Cuf, Cufd, BillUuid ya existen — solo si se requieren más) |
| `FE-00-E` | Solo si la UI necesita adaptación adicional (los prompts FE-S05 ya se ajustarán a WASM) |

---

## 12. Archivos Inspeccionados

### Domain (14 archivos)
- `Domain/Entities/VTA/FacturaVenta.cs`
- `Domain/Entities/VTA/FacturaVentaDetalle.cs`
- `Domain/Entities/VTA/Venta.cs`
- `Domain/Entities/VTA/VentaDetalle.cs`
- `Domain/Entities/VTA/VentaPago.cs`
- `Domain/Entities/VTA/PedidoVenta.cs`
- `Domain/Entities/VTA/PedidoVentaDetalle.cs`
- `Domain/Entities/VTA/VentaFacturacionDatos.cs`
- `Domain/Entities/CXC/CuentaPorCobrar.cs`
- `Domain/Entities/CXC/ClienteCreditoConfiguracion.cs`
- `Domain/Enums/EstadoSiatFactura.cs`
- `Domain/Enums/EstadoFacturaVentaComercial.cs`
- `Domain/Enums/EstadoFacturaVenta.cs`
- `Domain/Common/TenantEntity.cs`
- `Domain/Common/AuditableEntity.cs`

### Application (8 archivos)
- `Application/Interfaces/IFacturaVentaService.cs`
- `Application/Interfaces/ICuentasPorCobrarService.cs`
- `Application/Interfaces/IAuthService.cs`
- `Application/Interfaces/IClienteCreditoConfiguracionService.cs`
- `Application/Interfaces/ICurrentUserService.cs`
- `Application/Services/FacturaVentaService.cs`
- `Application/Services/VentaService.cs`
- `Application/Services/CuentasPorCobrarService.cs`

### Persistence (5 archivos)
- `Persistence/Context/AgoraDbContext.cs`
- `Persistence/Configurations/VTA/VentaConfiguration.cs`
- `Persistence/Configurations/CXC/CuentaPorCobrarConfiguration.cs`
- `Persistence/Configurations/VTA/FacturaVentaDetalleConfiguration.cs`
- `Persistence/Configurations/VTA/FacturaVentaConfiguration.cs`

### API (10 archivos)
- `Api/Program.cs`
- `Api/Controllers/V1/FacturasVentaController.cs`
- `Api/Controllers/V1/VentasController.cs`
- `Api/Controllers/V1/CuentasPorCobrarController.cs`
- `Api/Controllers/V1/ClientesCreditoController.cs`
- `Api/Controllers/V1/AuthController.cs`
- `Api/Controllers/V1/ProductosController.cs`
- `Api/Controllers/V1/MovimientosInventarioController.cs`
- `Api/Middleware/TenantRequiredMiddleware.cs`
- `Api/Services/CurrentUserService.cs`

### Web (8 archivos)
- `Web/Program.cs`
- `Web/Pages/Ventas/Facturas.razor`
- `Web/Pages/Ventas/PagoVenta.razor`
- `Web/Pages/Ventas/VentasComercial.razor`
- `Web/Pages/Ventas/Pedidos.razor`
- `Web/Pages/Ventas/CuentasPorCobrar.razor`
- `Web/Pages/Login.razor`
- `Web/Services/Ventas/VentaHttpService.cs`

### Shared (5 archivos)
- `Shared/DTOs/Ventas/FacturaVentaDtos.cs`
- `Shared/DTOs/Ventas/VentaDtos.cs`
- `Shared/DTOs/CxC/CuentaPorCobrarDtos.cs`
- `Shared/DTOs/CxC/ClienteCreditoConfiguracionDtos.cs`
- `Shared/DTOs/PaginatedResultDto.cs`

### Infrastructure (1 archivo)
- `Infrastructure/DependencyInjection.cs`

### Tests (2 archivos)
- `Tests/AgoraHub360.ERP.Tests.csproj`
- `Tests/AgoraHub360.ERP.Tests/GlobalUsings.cs`

---

*Fin del informe FE-00. Listo para proceder con FE-S01.*
