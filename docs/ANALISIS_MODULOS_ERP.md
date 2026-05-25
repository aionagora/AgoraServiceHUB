# Análisis Completo de Módulos — AgoraHUB360 ERP

> **Generado:** 2026-04-11  
> **Rama analizada:** `main`  
> **Stack:** .NET 8 · C# 12 · Blazor WebAssembly · ASP.NET Core API · EF Core · SQL Server  
> **Arquitectura:** Clean Architecture (Domain → Application → Infrastructure/Persistence → API → Web)

---

## Índice

1. [Arquitectura y Componentes Principales](#1-arquitectura-y-componentes-principales)
2. [Tabla de Módulos del ERP](#2-tabla-de-módulos-del-erp)
3. [Estado de Gestión del Proyecto](#3-estado-de-gestión-del-proyecto)
4. [Módulo de Contabilidad y Finanzas — Análisis Detallado](#4-módulo-de-contabilidad-y-finanzas--análisis-detallado)
5. [Recomendaciones y Próximos Pasos](#5-recomendaciones-y-próximos-pasos)

---

## 1. Arquitectura y Componentes Principales

### 1.1 Solución .NET 8 — 8 Proyectos

| Proyecto | Tipo | Descripción |
|---|---|---|
| `AgoraHub360.ERP.Domain` | Class Library | Entidades, interfaces de repositorio, enums, clases base |
| `AgoraHub360.ERP.Application` | Class Library | Servicios, interfaces, patrón Result |
| `AgoraHub360.ERP.Persistence` | Class Library | EF Core DbContext, repositorios, migraciones, interceptores |
| `AgoraHub360.ERP.Infrastructure` | Class Library | Servicios externos (email, storage, exportaciones) |
| `AgoraHub360.ERP.Api` | ASP.NET Core Web API | Controllers REST versionados `/api/v1/`, Swagger, JWT |
| `AgoraHub360.ERP.Shared` | Class Library | DTOs compartidos entre API y Web |
| `AgoraHub360.ERP.Web` | Blazor WebAssembly | SPA Frontend, HTTP Services, páginas Razor |
| `AgoraHub360.ERP.Tests` | xUnit | Pruebas unitarias (dominio) |

### 1.2 Estructura de Directorios

```
AgoraHub360.ERP.sln  (raíz)
src/
├── AgoraHub360.ERP.Domain/
│   └── Entities/
│       ├── ACC/          ← Contabilidad
│       ├── CMP/          ← Compras e Importaciones
│       ├── CST/          ← Centros de Costo
│       ├── Core/         ← Empresas, Usuarios, Roles, Monedas, Parámetros, Numeración
│       ├── DOC/          ← Documentos adjuntos
│       ├── INV/          ← Inventario
│       ├── LOG/          ← Logística / Hoja de Ruta
│       ├── MDM/          ← Datos Maestros (Productos, Clientes, Proveedores, Almacenes...)
│       ├── PRC/          ← Precios
│       ├── RUL/          ← Reglas de Industria
│       ├── VER/          ← Versionado de Entidades
│       └── Workflow/     ← Tareas y Plantillas de Workflow
├── AgoraHub360.ERP.Application/
│   ├── Services/         ← 43 servicios de aplicación
│   └── Interfaces/       ← 46 interfaces de servicios
├── AgoraHub360.ERP.Persistence/
│   ├── Migrations/       ← 33 migraciones EF Core
│   └── Configurations/   ← Fluent API por módulo (ACC, CMP, MDM...)
├── AgoraHub360.ERP.Infrastructure/
│   └── Services/         ← ExportService, etc.
├── AgoraHub360.ERP.Api/
│   └── Controllers/V1/   ← 43 controllers REST
├── AgoraHub360.ERP.Shared/
│   └── DTOs/             ← Contabilidad, Compras, MDM, Inventario, Empresa, Usuario...
└── AgoraHub360.ERP.Web/
    ├── Pages/            ← 67 páginas Blazor organizadas por módulo
    └── Services/         ← 45+ HTTP Services (clientes de API)
tests/
└── AgoraHub360.ERP.Tests/
docs/
└── 11 archivos .md (ESTADO_PROYECTO.md, MODULO-FINANCIERO-CONTABLE.md, etc.)
```

### 1.3 Patrones Transversales

| Principio | Implementación |
|---|---|
| **Multi-tenant** | `TenantEntity` con `EmpresaId`; query filters globales en EF Core aíslan datos por empresa |
| **Auditoría automática** | `AuditableEntity` + `AuditableEntityInterceptor` registra FechaCreacion/Modificacion/Usuario |
| **Versionado de entidades** | `EntityVersioningInterceptor` genera snapshots JSON en tabla `EntityVersions` |
| **Clean Architecture** | Domain no depende de nada; Application solo de Domain; Persistence/API solo de Application |
| **API versionada** | URL Segment (`/api/v1/`) + Header (`X-Api-Version`) via `Asp.Versioning` |
| **Autenticación** | JWT + `StubAuthHandler` para desarrollo |

### 1.4 Dependencias entre Proyectos

```
Domain  ←──  Application  ←──  Persistence
                  ↑                  ↑
                  └─────── Api ───────┘
                              ↑
                           Shared
                              ↑
                            Web
```

---

## 2. Tabla de Módulos del ERP

| Módulo | Código | Archivos/Carpetas clave | Endpoints principales | Estado | Observaciones |
|---|---|---|---|---|---|
| **Autenticación / Seguridad** | Core | `Core/Usuario.cs`, `Core/Rol.cs`, `Core/UsuarioEmpresa.cs` · `AuthController.cs`, `UsuariosController.cs`, `RolesController.cs` | `POST /api/v1/auth/login` · CRUD `/usuarios` · CRUD `/roles` | ✅ Completo | JWT + StubAuthHandler para dev |
| **Multiempresa / Tenant** | Core | `Core/Empresa.cs`, `Core/UsuarioEmpresa.cs` · `EmpresasController.cs`, `EmpresaService.cs` | CRUD `/api/v1/empresas` | ✅ Completo | Filtro global EF Core por `EmpresaId` |
| **Parámetros de Sistema** | Core | `Core/ParametroSistema.cs` · `ParametrosController.cs`, `ParametroSistemaService.cs` | CRUD `/api/v1/parametros` | ✅ Completo | Clave-valor por empresa; múltiples tipos de dato |
| **Numeración de Documentos** | Core | `Core/NumeracionDocumento.cs` · `NumeracionesController.cs` | CRUD `/api/v1/numeraciones` | ✅ Completo | Auto-incremento con prefijo y padding por tipo+empresa |
| **Monedas** | Core | `Core/Moneda.cs` | _(integrado en Core)_ | ✅ Completo | ISO 4217; usada por ACC |
| **Auditoría** | Core | `Core/AuditLog.cs` · `AuditLogsController.cs`, `AuditLogService.cs` | `GET /api/v1/audit-logs` | ✅ Completo | Automática vía `AuditableEntityInterceptor` |
| **MDM — Productos** | MDM | `MDM/Product.cs`, `MDM/ProductVariant.cs`, `MDM/ProductAttribute.cs` + 8 más · 10 controllers MDM | CRUD `/api/v1/mdm/products` · `/mdm/variants` · `/mdm/attributes` | ✅ Completo | Wizard 6 tabs en UI; EAV para atributos dinámicos; CompanyProducts |
| **MDM — Clientes** | MDM | `MDM/Cliente.cs` · `ClientesController.cs`, `ClienteService.cs` | CRUD `/api/v1/clientes` | ✅ Completo | |
| **MDM — Proveedores** | MDM | `MDM/Proveedor.cs` · `ProveedoresController.cs`, `ProveedorService.cs` | CRUD `/api/v1/proveedores` | ✅ Completo | |
| **MDM — Almacenes** | MDM | `MDM/Almacen.cs`, `MDM/UbicacionAlmacen.cs` · `AlmacenesController.cs` | CRUD `/api/v1/almacenes` | ✅ Completo | UI de UbicacionesAlmacen pendiente |
| **MDM — Catálogos / UdM / Marcas / Fabricantes** | MDM | `MDM/Catalog.cs`, `MDM/Uom.cs`, `MDM/Brand.cs`, `MDM/Manufacturer.cs` | CRUD `/api/v1/mdm/catalogs` · `/uom` · `/brands` | ✅ Completo | |
| **Listas de Precios** | PRC | `PRC/PriceList.cs`, `PRC/PriceListItem.cs` · `PrcPriceListsController.cs` | CRUD `/api/v1/price-lists` | ✅ Completo | |
| **Industrias / Reglas** | RUL | `RUL/Industry.cs`, `RUL/ProductIndustryRule.cs` · `RulIndustriesController.cs` | CRUD `/api/v1/rul/industries` | ⚠️ Parcial | Backend completo; **sin UI Blazor** |
| **Inventario** | INV | `INV/MovimientoInventario.cs`, `INV/StockProducto.cs` · `MovimientosInventarioController.cs` | CRUD `/api/v1/movimientos-inventario` | ✅ Completo | 4 tipos de movimiento; Kardex; Stock por almacén |
| **Compras** | CMP | `CMP/OrdenCompra.cs`, `CMP/OrdenCompraLinea.cs`, `CMP/RecepcionCompra.cs`, `CMP/PagoOrdenCompra.cs`, `CMP/ConfirmacionProveedor.cs` · `OrdenesCompraController.cs` | CRUD `/api/v1/ordenes-compra` · `/recepciones-compra` | ✅ Completo | Ciclo completo OC → aprobación → recepción parcial/total → pago; dispara asientos automáticos |
| **Importaciones (Landed Cost)** | CMP | `CMP/ExpedienteImportacion.cs`, `CMP/HojaImportacion.cs`, `CMP/GastoImportacion.cs`, `CMP/HitoExpediente.cs` · `ExpedientesImportacionController.cs` | CRUD `/api/v1/expedientes-importacion` | ✅ Completo | Landed cost; prorrateo gastos por Valor/Unidades/Peso/Volumen; contabilización automática |
| **Ventas** | `VTA` | `Domain/Entities/VTA/PedidoVenta.cs`, `PedidoVentaDetalle.cs` · `Application/Services/PedidoVentaService.cs` · `Api/Controllers/V1/PedidosVentaController.cs` · `Web/Pages/Ventas/Pedidos.razor`, `PedidoForm.razor` | `POST/GET/PUT/DELETE /api/v1/ventas/pedidos` | 🔄 En progreso | Backend inicial (modelo, servicio, controller) y UI Blazor básicos implementados. Falta completar: facturación, CxC/CxP, validación stock e integración producto selector. Branch activa: `feature/pedidos-venta`. |
| **Contabilidad General** | ACC | `ACC/AsientoContable.cs`, `ACC/CuentaContable.cs`, `ACC/PeriodoContable.cs`, `ACC/PlantillaContable.cs`, `ACC/TipoComprobante.cs`, `ACC/TipoCambio.cs`, `ACC/TipoPago.cs`, `ACC/CierreContable.cs` | CRUD `/api/v1/asientos-contables` · `/cuentas-contables` · `/periodos-contables` · `/plantillas-contables` | ✅ Completo | Ver sección §4 |
| **Reportes Financieros** | ACC | `EstadoFinancieroService.cs` · `EstadosFinancierosController.cs` | `GET /api/v1/estados-financieros/balance-general` · `/estado-resultados` · `/libro-diario` · `/sumas-saldos` · `/flujo-efectivo` | ✅ Completo | 5 reportes incluyendo Flujo de Efectivo (nuevo) |
| **Centros de Costo** | CST | `CST/CentroCosto.cs`, `CST/CostingRule.cs`, `CST/LandedCostProfile.cs` · `CentrosCostoController.cs` | CRUD `/api/v1/centros-costo` | ✅ Completo | UI para CostingRules/LandedCostProfiles pendiente |
| **Documentos Adjuntos** | DOC | `DOC/Document.cs`, `DOC/ProductDocument.cs`, `DOC/ComprobanteDocumento.cs` · `DocumentosController.cs` | `/api/v1/documentos` | ⚠️ Parcial | Entidades presentes; storage externo pendiente; **sin UI** |
| **Logística / Hoja de Ruta** | LOG | `LOG/HojaRuta.cs`, `LOG/HojaRutaHistorial.cs` · `HojasRutaController.cs` | CRUD `/api/v1/hojas-ruta` | ✅ Completo | Ligado a OrdenPedido |
| **Workflow / Tareas** | Workflow | `Workflow/Tarea.cs`, `Workflow/PlantillaTarea.cs` · `WorkflowController.cs`, `WorkflowService.cs` | `/api/v1/workflow` | ⚠️ Parcial | Motor activo; 3 PRs draft pendientes (PRs #27–#29) |
| **Versionado de Entidades** | VER | `VER/EntityVersion.cs` · `VersioningService.cs` (interceptor) | _(sin endpoint propio)_ | ⚠️ Parcial | Interceptor activo automáticamente; **sin UI** para historial |

---

## 3. Estado de Gestión del Proyecto

### 3.1 Issues

> **No hay issues abiertos** actualmente en el repositorio.

Se recomienda crear issues para el trabajo pendiente (ver §5).

### 3.2 Pull Requests

| # | Título | Estado | Rama origen → destino | Relevancia para ACC |
|---|---|---|---|---|
| #43 | `ultimoscambios` | ✅ Cerrado/Mergeado | `Contabilidad001` → `main` | **Última actualización ACC en main** (2026-04-11) |
| #42 | `docs: guía de prompts Finanzas y Contabilidad` | ✅ Cerrado | `claude/review-finance-accounting-X1f2G` → `main` | Documentación ACC |
| #41 | `Claude/review finance accounting` | ✅ Cerrado | idem → `main` | Revisión módulo ACC |
| #40 | `docs: Add comprehensive Financial-Accounting module documentation` | ✅ Cerrado | `claude/generate-financial-docs-pc8pu` → `Contabilidad001` | Documentación |
| #39 | `Fix character encoding: Convert ISO-8859-1 to UTF-8` | ✅ Cerrado | `claude/plan-financial-accounting-slBdZ` → `Contabilidad001` | Fix encoding + página CentrosCosto |
| **#38** | **[WIP] Create new repository** | **🔴 Abierto (Draft)** | `copilot/create-repository` → `main` | Artefacto IA; cerrar |
| **#29** | **feat: componente Blazor TareasTab** | **🔴 Abierto (Draft)** | `copilot/refactor-tareastab-to-generic` → `GestionTareasYProcesos` | Workflow UI |
| **#28** | **feat: IWorkflowService y WorkflowService** | **🔴 Abierto (Draft)** | `copilot/create-iworkflowservice-implementation` → `GestionTareasYProcesos` | Workflow backend |
| **#27** | **feat: schema [wf] y entidad Tarea** | **🔴 Abierto (Draft)** | `copilot/create-schema-wf-entity-tarea` → `GestionTareasYProcesos` | Workflow DB |
| #26 | `cambiosHastaFin` | ✅ Cerrado | `PedidoHastaCompra` → `main` | Compras/Pedidos |
| #-- | `feature/pedidos-venta` | 🔄 Abierto (branch) | `feature/pedidos-venta` → `main` | Implementación inicial módulo Pedidos (backend + UI). |
| #25 | `AdicionImportacion-Finanza` | ✅ Cerrado | `GestionImportacion` → `main` | Importaciones + ACC |

**4 PRs abiertos** en total: 3 drafts de Workflow (#27–#29) + 1 artefacto IA a cerrar (#38).

### 3.3 Actividad Reciente en `main`

| Fecha | SHA | Mensaje | Módulo afectado |
|---|---|---|---|
| 2026-04-11 | `4e3e8a3` | Merge PR #43 `ultimoscambios` (Contabilidad001→main) | ACC — última actualización |
| 2026-04-11 | `38c4332` | `ultimoscambios` | ACC |
| 2026-04-10 | `5e2f239` | Merge PR #42 docs finanzas | Docs ACC |
| 2026-04-10 | `b77b2a7` | Normalize line endings CRLF→LF | Limpieza global |
| 2026-04-09 | `6939f40` | `cierre` | ACC — `CierreContable` |
| 2026-04-09 | `a984ae5`–`2cc4818` | `S2-PASO*`, `Paso*` | ACC — Sprint Flujo de Efectivo + exportación Excel |
| 2026-04-09 | `216326a` | `Paso7.02` | ACC — `ClasificacionFlujoEfectivo` en `CuentaContable` |
| 2026-04-08 | `a8c8b0c` | Fix encoding + página CentrosCosto | ACC — CentrosCosto UI |
| 2026-03-17 | `d742dc5` | Merge PR #37 cambios en el menú | Web — Navegación |

La **rama `Contabilidad001`** fue la activa para el sprint completo de finanzas/contabilidad (abril 2026). Ya está mergeada a `main` (PR #43).

---

## 4. Módulo de Contabilidad y Finanzas — Análisis Detallado

### 4.1 Resumen Ejecutivo

El módulo financiero-contable está **completamente implementado en todas las capas** de la arquitectura Clean Architecture. Cubre contabilidad por partida doble, plan de cuentas jerárquico N-niveles, períodos contables, plantillas de asientos automáticos, centros de costo, cierre contable anual, y cinco reportes financieros estándar. Está integrado con el módulo de compras mediante un motor de contabilización automática.

### 4.2 Mapa de Archivos por Capa

#### Domain — Entidades ACC
**Ruta:** `src/AgoraHub360.ERP.Domain/Entities/ACC/`

| Archivo | Descripción |
|---|---|
| `AsientoContable.cs` | Cabecera del comprobante: `Numero`, `Fecha`, `Gestion`, `Estado` (Borrador/Contabilizado/Anulado), `TipoRegistro` (Manual/Automático/Ajuste), `Glosa`, `OrigenTipo`/`OrigenId`/`OrigenReferencia`, `ValorTipoCambio`, `RegistradoPorNombre` (inmutable), `TotalDebe`/`TotalHaber` |
| `AsientoContableLinea.cs` | Líneas Debe/Haber: `CuentaContableId`, `Debe`, `Haber`, `CentroCostoId` |
| `CuentaContable.cs` | Plan de cuentas jerárquico N-niveles, código estructurado (ej. `1.1.3.01`), `TipoCuenta` (Activo/Pasivo/Patrimonio/Ingreso/Gasto/Costo), `NaturalezaCuenta` (Deudora/Acreedora), `SaldoActual`, `ClasificacionFlujoEfectivo` _(nuevo — migración 20260409)_ |
| `PeriodoContable.cs` | Año/Mes, `Estado` (Abierto/Cerrado), `FechaCierre`, `CerradoPorNombre` (firma histórica inmutable) |
| `CierreContable.cs` | Cierre anual contable: `Gestion`, `FechaCierre` — _(añadido migración 20260410)_ |
| `PlantillaContable.cs` | Plantilla de asientos: `TipoDocumento`, `Descripcion`, lineas |
| `PlantillaContableLinea.cs` | Líneas de plantilla: `CampoMonto`, `Factor`, `CuentaDebeId`, `CuentaHaberId` |
| `TipoCambio.cs` | Divisa, `FechaVigencia`, `ValorCompra`, `ValorVenta` |
| `TipoComprobante.cs` | Código (CI/CE/TRA), `Nombre`, `Prefijo`, numeración secuencial |
| `TipoPago.cs` | Efectivo, Cheque, QR, S/D |

*Todas heredan de `TenantEntity` → provee `EmpresaId`, auditoría automática, soft-delete.*

#### Domain — Entidades Relacionadas (CST / DOC)
**Rutas:** `src/AgoraHub360.ERP.Domain/Entities/CST/` · `...DOC/`

| Archivo | Descripción |
|---|---|
| `CST/CentroCosto.cs` | Código, Nombre, `CentroCostoPadreId` (jerarquía), `EmpresaId` |
| `CST/CostingRule.cs` | Reglas de costeo |
| `CST/LandedCostProfile.cs` | Perfil de costo aterrizado |
| `DOC/ComprobanteDocumento.cs` | Enlace entre asiento contable y documento físico |

#### Application — Servicios
**Ruta:** `src/AgoraHub360.ERP.Application/Services/`

| Servicio | Tamaño aprox. | Responsabilidad |
|---|---|---|
| `AsientoContableService.cs` | ~33 KB | CRUD asientos, validación cuadratura (Debe == Haber), contabilización manual, anulación |
| `CuentaContableService.cs` | ~27 KB | Árbol jerárquico, códigos, saldos, búsqueda |
| `PlantillaContableService.cs` | ~16 KB | CRUD plantillas, resolución de montos variables |
| `EstadoFinancieroService.cs` | ~15 KB | 5 reportes financieros con acumulación recursiva de árbol |
| `ContabilizacionService.cs` | ~11 KB | Motor automático: busca plantilla, resuelve montos, valida cuadratura, ajuste ±0.01 por redondeo, crea asiento contabilizado |
| `CentroCostoService.cs` | ~8.9 KB | CRUD centros de costo |
| `PeriodoContableService.cs` | ~6.7 KB | Apertura/cierre de períodos, bloqueo de contabilización |
| `CierreContableService.cs` | — | Cierre anual |
| `ComprobanteDocumentoService.cs` | — | Vinculación asiento ↔ documento físico |

**Interfaces definidas** (`Application/Interfaces/`): `IAsientoContableService`, `ICuentaContableService`, `IPeriodoContableService`, `IPlantillaContableService`, `IContabilizacionService`, `IEstadoFinancieroService`, `ICentroCostoService`, `ICierreContableService` (entre las 46 interfaces totales).

#### API — Endpoints
**Ruta:** `src/AgoraHub360.ERP.Api/Controllers/V1/`

```
AsientosContablesController.cs
  GET/POST/PUT/DELETE  /api/v1/asientos-contables
  POST                 /api/v1/asientos-contables/{id}/post      ← contabilizar
  POST                 /api/v1/asientos-contables/{id}/anular    ← anular

CuentasContablesController.cs
  GET/POST/PUT/DELETE  /api/v1/cuentas-contables

PeriodosContablesController.cs
  GET/POST/PUT/DELETE  /api/v1/periodos-contables
  POST                 /api/v1/periodos-contables/{id}/cerrar

PlantillasContablesController.cs
  GET/POST/PUT/DELETE  /api/v1/plantillas-contables

CentrosCostoController.cs
  GET/POST/PUT/DELETE  /api/v1/centros-costo

EstadosFinancierosController.cs
  GET  /api/v1/estados-financieros/balance-general?fechaCorte=
  GET  /api/v1/estados-financieros/estado-resultados?desde=&hasta=
  GET  /api/v1/estados-financieros/libro-diario?desde=&hasta=&estado=
  GET  /api/v1/estados-financieros/sumas-saldos?desde=&hasta=
  GET  /api/v1/estados-financieros/flujo-efectivo?...    ← nuevo

ContabilidadController.cs
  GET  /api/v1/contabilidad/...   (endpoints generales / dashboard)
```

#### Persistence — Configuraciones EF Core
**Ruta:** `src/AgoraHub360.ERP.Persistence/Configurations/ACC/`

```
AsientoContableConfiguration.cs
AsientoContableLineaConfiguration.cs
CierreContableConfiguration.cs
CuentaContableConfiguration.cs
PeriodoContableConfiguration.cs
PlantillaContableConfiguration.cs
PlantillaContableLineaConfiguration.cs
TipoCambioConfiguration.cs
TipoComprobanteConfiguration.cs
TipoPagoConfiguration.cs
```

#### Persistence — Tablas SQL Server

| Tabla | Descripción |
|---|---|
| `ACC_AsientosContables` | Cabecera de comprobantes, multi-tenant (`EmpresaId`) |
| `ACC_AsientosContablesLineas` | Líneas Debe/Haber con FK a cuenta contable |
| `ACC_CuentasContables` | Plan de cuentas con auto-referencia para jerarquía |
| `ACC_PeriodosContables` | Períodos mensuales con estado y firma de cierre |
| `ACC_CierresContables` | Cierres anuales _(nueva, migración 20260410)_ |
| `ACC_PlantillasContables` | Plantillas de asientos |
| `ACC_PlantillasContablesLineas` | Líneas de plantilla con `CampoMonto` y `Factor` |
| `ACC_TiposCambio` | Tasas de cambio de divisas |
| `ACC_TiposComprobante` | Configuración de tipos de comprobante |
| `ACC_TiposPago` | Medios de pago |
| `CST_CentrosCosto` | Centros de costo (jerarquía) |

#### Persistence — Migraciones ACC (9 migraciones)
**Ruta:** `src/AgoraHub360.ERP.Persistence/Migrations/`

| Migración | Fecha | Contenido |
|---|---|---|
| `20260226064451_ACC_CuentasContables` | 26-feb | Tabla plan de cuentas |
| `20260226065319_ACC_AsientosContables` | 26-feb | Tablas asientos y líneas |
| `20260226070740_ACC_PeriodosContables` | 26-feb | Tabla períodos |
| `20260226072414_ACC_PlantillasContables` | 26-feb | Tablas plantillas y líneas |
| `20260226081631_ACC_ComprobantesContables` | 26-feb | Tipos comprobante, pago, cambio |
| `20260306134001_AddCentrosCostoAndComprobanteDocumentos` | 06-mar | CST + DOC |
| `20260311103919_ACC_AsientoContable_NuevosCampos` | 11-mar | `OrigenTipo`, `RegistradoPorNombre`, etc. |
| `20260409134456_ACC_CuentaContable_ClasificacionFlujo` | 09-abr | Enum `ClasificacionFlujoEfectivo` |
| `20260410232130_AddCierreContable` | 10-abr | Tabla `ACC_CierresContables` _(más reciente)_ |

#### Shared — DTOs
**Ruta:** `src/AgoraHub360.ERP.Shared/DTOs/Contabilidad/`

| DTO | Contenido |
|---|---|
| `AsientoContableDto.cs` | Cabecera completa + `List<AsientoContableLineaDto>` |
| `CuentaContableDto.cs` | Plan de cuentas con jerarquía |
| `PeriodoContableDto.cs` | Año, mes, estado, fechas |
| `CierreContableDto.cs` | Gestion, fecha cierre |
| `PlantillaContableDto.cs` | Plantilla + `List<PlantillaContableLineaDto>` |
| `CentroCostoDto.cs` | Jerarquía de centros |
| `ComprobanteDocumentoDto.cs` | Enlace asiento-documento |
| `EstadosFinancierosDto.cs` | `BalanceGeneralDto` · `EstadoResultadosDto` · `LibroDiarioDto` · `SumasYSaldosDto` |

#### Web — Páginas Blazor
**Ruta:** `src/AgoraHub360.ERP.Web/Pages/Contabilidad/`

| Componente | Tamaño | Funcionalidad |
|---|---|---|
| `AsientosContables.razor` | ~74 KB | UI principal CRUD asientos; la página más completa del módulo |
| `PlanCuentas.razor` | ~24 KB | Árbol de cuentas, gestión |
| `PlantillasContables.razor` | ~19 KB | Editor de plantillas |
| `CentrosCosto.razor` | ~16 KB | Gestión de centros de costo |
| `LibroDiario.razor` | ~11 KB | Libro diario con filtros |
| `PeriodosContables.razor` | ~9 KB | Apertura/cierre de períodos |
| `SumasYSaldos.razor` | ~9 KB | Reporte sumas y saldos |
| `EstadoResultados.razor` | ~8.3 KB | Estado de resultados P&G |
| `BalanceGeneral.razor` | ~8.1 KB | Balance general |
| `FlujoDEfectivo.razor` | _(nuevo)_ | Estado de Flujo de Efectivo |
| `DashboardContable.razor` | — | Dashboard contable |

**HTTP Services Web** (`src/AgoraHub360.ERP.Web/Services/`): `CuentaContableHttpService.cs`, `AsientoContableHttpService.cs`, `PeriodoContableHttpService.cs`, `CierreContableHttpService.cs`, `PlantillaContableHttpService.cs`, `CentroCostoHttpService.cs`, `EstadoFinancieroHttpService.cs`, `HttpLibroMayorService.cs`, `HttpFlujoDEfectivoService.cs`, `HttpExportEstadosFinancierosService.cs`

### 4.3 Flujos Clave

#### Registro Manual de Asiento
```
Usuario → AsientosContables.razor
    → POST /api/v1/asientos-contables
    → AsientoContableService.CreateAsync()
        → Valida período abierto
        → Valida cuadratura (TotalDebe == TotalHaber)
        → Genera número secuencial por tipo+gestión
        → Estado inicial: "Borrador"
    → POST /api/v1/asientos-contables/{id}/post
        → Estado: "Contabilizado"
        → Actualiza SaldoActual en cada CuentaContable
```

#### Contabilización Automática desde Compras
```
RecepcionCompra / HojaImportacion confirmada
    → ContabilizacionService.ContabilizarDocumentoAsync(tipoDocumento, montos, ...)
        → Busca PlantillaContable por TipoDocumento
        → Valida período no cerrado
        → Resuelve montos por CampoMonto (ej: "Total", "Costo", "IGV")
        → Aplica Factor por línea
        → Valida cuadratura (ajuste automático ±0.01 por redondeo)
        → Construye glosa desde plantilla ({Numero}, {Proveedor}, {Fecha})
        → TipoComprobante: TRA (Traspaso)
        → Crea AsientoContable con TipoRegistro="Automático", Estado="Contabilizado"
        → Actualiza SaldoActual de cuentas afectadas
```

#### Generación de Reportes Financieros
```
EstadoFinancieroService
    → CalcularSaldosAlAsync(fechaCorte)       ← solo asientos "Contabilizado"
    → BuildTree(cuentas, saldos, TipoCuenta)  ← árbol jerárquico por tipo
    → SumarSaldoTree(grupos)                  ← acumulación recursiva
    → BalanceGeneralDto { Cuadrado = Activos == Pasivos + Patrimonio }
```

### 4.4 Funcionalidades Existentes vs. Faltantes

#### ✅ Implementado y funcional

- Plan de cuentas jerárquico N-niveles con tipos (Activo/Pasivo/Patrimonio/Ingreso/Gasto/Costo) y naturaleza (Deudora/Acreedora)
- Asientos contables por partida doble con validación de cuadratura; estados Borrador → Contabilizado → Anulado
- Tipos de comprobante (CI/CE/TRA) con numeración secuencial por tipo y gestión
- Períodos contables con apertura/cierre y firma histórica (`CerradoPorNombre`)
- **Cierre contable anual** (`CierreContable` — migración 20260410)
- **Motor de contabilización automática** (`ContabilizacionService`): genera asientos desde compras/recepciones/importaciones usando plantillas parametrizables
- Plantillas contables con campos de monto variables (`CampoMonto`) y factor multiplicador
- Tipos de cambio (USD, UFV) con valor capturado al registrar (`ValorTipoCambio`)
- Tipos de pago (Efectivo, Cheque, QR, S/D)
- Trazabilidad de origen en asientos automáticos (`OrigenTipo`, `OrigenId`, `OrigenReferencia`)
- **5 reportes financieros**: Balance General, Estado de Resultados, Libro Diario, Sumas y Saldos, Flujo de Efectivo _(nuevo)_
- Centros de costo con jerarquía
- **Clasificación de flujo de efectivo por cuenta** (`ClasificacionFlujoEfectivo` — migración 20260409)
- Exportación Excel de estados financieros (`docs/EXPORTAR_EXCEL_ASIENTOS.md`)
- Multi-divisa con valor de tipo de cambio capturado al registrar
- Multi-tenancy completo (todos los datos filtrados por `EmpresaId`)
- Auditoría automática en todos los registros

#### ⚠️ Parcial o Faltante (inferido)

| Funcionalidad | Estado | Observación |
|---|---|---|
| **Contabilización de ventas** | 🔴 No conectado | Motor `ContabilizacionService` listo; falta módulo Ventas |
| **Asiento automático de apertura de período** | 🟡 Parcial | Apertura manual sí; generación de asiento de apertura no documentada |
| **Conciliación bancaria** | 🔴 No implementado | Requeriría submódulo de extractos bancarios |
| **Presupuesto (Budget)** | 🔴 No implementado | Extensión natural sobre el plan de cuentas existente |
| **Retenciones e impuestos automáticos** | 🔴 No implementado | `TipoPago` cubre medios de pago; sin cálculo de IVA/IT/retenciones |
| **Declaraciones fiscales / SII** | 🔴 No evidenciado | Sin integración con autoridades tributarias |
| **UI para CostingRules / LandedCostProfiles** | 🟡 Backend listo | Entidades y servicios disponibles; sin página Blazor |

### 4.5 Dependencias con Otros Módulos

```
Contabilidad (ACC)
    ← Core/Empresas      → EmpresaId (multi-tenant), MonedaBaseId (moneda funcional)
    ← Core/Usuarios      → RegistradoPorNombre (firma inmutable), CreadoPor/ModificadoPor (auditoría)
    ← Core/Monedas       → TipoCambio.DivisaId, soporte multi-divisa
    ← Core/Numeración    → NumeracionDocumento para series de asientos por TipoComprobante y gestión
    ← CMP/Compras        → RecepcionCompra, HojaImportacion → disparan ContabilizacionService
                           PagoOrdenCompra → asiento de pago
    ← CST/Centros Costo  → CentroCostoId en líneas de asientos
    ← DOC/Documentos     → ComprobanteDocumento vincula asiento ↔ documento físico
    ← Workflow           → Tareas ligadas a OrdenPedido (flujo compras→ACC)
    → MDM/Productos      → usados en OC/recepciones que generan asientos (vía CMP)
    → Ventas (futuro)    → cuando se implemente, conectar al ContabilizacionService
```

### 4.6 Capacidades Transversales

| Capacidad | Implementación |
|---|---|
| **Multi-tenancy** | `EmpresaId` en todas las tablas ACC; filtrado automático por tenant |
| **Multi-divisa** | `TipoCambio` con valor capturado al registrar (`ValorTipoCambio`) |
| **Auditoría** | `CreadoEn`, `ModificadoEn`, `CreadoPor`, `ModificadoPor` automáticos |
| **Firma histórica** | `RegistradoPorNombre` / `CerradoPorNombre` inmutables en impresiones |
| **Trazabilidad** | `OrigenTipo` + `OrigenId` + `OrigenReferencia` en asientos automáticos |
| **Período seguro** | Bloqueo de contabilización en períodos cerrados |

---

## 5. Recomendaciones y Próximos Pasos

### 5.1 PRs y Ramas con Acción Inmediata

| PR | Acción recomendada | Razón |
|---|---|---|
| **#38** `[WIP] Create new repository` (Draft) | 🔴 **Cerrar** | Artefacto de agente IA; no aporta valor al ERP |
| **#27, #28, #29** (Drafts Workflow) | 🟡 **Revisar y decidir merge** | Implementan módulo Workflow completo; requieren revisión de conflictos con `main` actual |

### 5.2 Issues a Crear (Backlog Sugerido)

| Issue sugerido | Módulo | Prioridad |
|---|---|---|
| Implementar módulo Ventas (Facturas + Pedidos) con integración a Inventario | Ventas | 🔴 Alta |
| Conectar `ContabilizacionService` con facturas de venta | ACC + Ventas | 🔴 Alta |
| Evaluar y mergear rama `GestionTareasYProcesos` (PRs #27–#29) | Workflow | 🟡 Media |
| Crear UI Blazor para `Industries` / `ProductIndustryRules` | MDM/RUL | 🟡 Media |
| Crear UI para historial de versiones de entidades (`EntityVersions`) | VER | 🟡 Media |
| Implementar storage externo para módulo DOC | DOC | 🟡 Media |
| Crear UI para `CostingRules` y `LandedCostProfiles` | CST | 🟡 Media |
| Implementar asiento automático de apertura de período contable | ACC | 🟡 Media |
| Implementar cálculo automático de retenciones e impuestos (IVA/IT) | ACC | 🟡 Media |
| Implementar conciliación bancaria | ACC | 🔵 Baja |
| Implementar módulo de Presupuesto (Budget) | ACC | 🔵 Baja |
| Configurar pipeline CI/CD GitHub Actions | DevOps | 🔵 Baja |
| Ampliar cobertura de tests (servicios Application) | Tests | 🔵 Baja |
| Crear UI para `UbicacionesAlmacen` | MDM/INV | 🔵 Baja |

### 5.3 Revisiones Técnicas Prioritarias

1. **Autenticación en producción**: el `StubAuthHandler` está activo para desarrollo. Verificar que JWT real esté configurado y `StubAuthHandler` deshabilitado antes de cualquier despliegue en producción.

2. **Migración `AddCierreContable`** (2026-04-10, la más reciente): verificar que `AgoraDbContextModelSnapshot.cs` esté actualizado y que la migración no genere conflictos en entornos existentes con datos previos.

3. **Módulo Ventas vs. `OrdenPedido`**: existe la entidad `CMP/OrdenPedido.cs` que representa el pedido de cliente dentro del flujo de compras. Definir si este concepto se reutiliza o si se creará un módulo de Ventas independiente con sus propias entidades (Factura, PedidoVenta) antes de comenzar el desarrollo.

4. **Exportación Excel**: los documentos `EXPORTAR_EXCEL_ASIENTOS.md`, `DEPLOY_EXPORTAR_EXCEL.md`, `TESTING_EXPORTAR_EXCEL.md` indican que esta funcionalidad fue implementada en el sprint de abril 2026. Ejecutar los tests documentados para validar el funcionamiento correcto.

5. **Workflow (PRs #27–#29)**: los 3 drafts están apuntando a la rama `GestionTareasYProcesos`, no a `main`. Antes de mergear, verificar que esta rama esté actualizada con `main` y que no haya conflictos con las migraciones recientes de ACC.

---

*Documento generado mediante análisis estático del código fuente del repositorio en rama `main`.*  
*Última revisión: 2026-04-11.*
