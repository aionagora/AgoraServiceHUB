# AGORAHUB360 ERP — CAPA DE API REST

> **Proyecto:** `src/AgoraHub360.ERP.Api/`
> **Propósito:** API REST versionada con controllers, middleware JWT, autorización basada en claims y Swagger.

---

## 1. Estructura de Carpetas

```
AgoraHub360.ERP.Api/
├── Auth/                  ← StubAuthHandler (desarrollo)
├── Authorization/         
│   ├── Handlers/          ← TenantMembershipHandler, BranchAccessHandler
│   └── Requirements/      ← Custom authorization requirements
├── BackgroundServices/    ← PeriodoContableNotificadorService
├── Controllers/
│   └── V1/                ← ~55+ controllers versionados
├── Middleware/             ← GlobalExceptionMiddleware, TenantRequiredMiddleware
├── Services/               ← AuthService, CurrentUserService
├── Properties/             ← launchSettings
├── wwwroot/                ← Archivos estáticos
├── Program.cs              ← Punto de entrada (JWT, DI, middleware pipeline)
└── appsettings.json        ← Configuración (connection strings, JWT)
```

---

## 2. Program.cs — Pipeline de Middleware

**Archivo:** `Program.cs`

### Orden del pipeline:

1. Configuración de cultura (numérica y UI)
2. `IHttpContextAccessor` + `ICurrentUserService`
3. `AddApplication()`, `AddPersistence()`, `AddInfrastructure()`
4. Autenticación JWT + StubAuthHandler (desarrollo)
5. Autorización con políticas (PlatformSuperAdmin, TenantAdmin, etc.)
6. Swagger/OpenAPI
7. API Versioning (Asp.Versioning)
8. CORS
9. Middleware pipeline: `TenantRequiredMiddleware` → `GlobalExceptionMiddleware` → Routing → Auth → Endpoints

### Configuración JWT

```csharp
// appsettings.json:
{
  "Jwt": {
    "Key": "AgoraHub360-ERP-Dev-Secret-Key-2026-MinLength32!",
    "Issuer": "AgoraHub360.ERP",
    "Audience": "AgoraHub360.ERP.Web"
  }
}
```

### Políticas de Autorización

Definidas en `Shared/Constants/PolicyNames.cs`:

| Política | Requisito |
|---|---|
| `RequireAuthenticated` | Usuario autenticado |
| `RequirePlatformSuperAdmin` | `PlatformRole` = `SuperAdmin` |
| `RequireTenantAdmin` | `TenantRole` = `Admin` |
| `RequireTenantActive` | `TenantStatus` = `Active` |
| + handlers personalizados | `TenantMembershipHandler`, `BranchAccessHandler` |

---

## 3. Catálogo de Controllers

> **Total: ~55 controllers** en `Controllers/V1/`

| # | Controller | Ruta base | Authorize |
|---|---|---|---|
| 1 | `AuthController` | `/api/v1/auth` | Mixto (login es AllowAnonymous) |
| 2 | `EmpresasController` | `/api/v1/empresas` | Sí |
| 3 | `SucursalesController` | `/api/v1/sucursales` | Sí |
| 4 | `UsuariosController` | `/api/v1/usuarios` | Sí |
| 5 | `RolesController` | `/api/v1/roles` | Sí |
| 6 | `ParametrosController` | `/api/v1/parametros` | Sí |
| 7 | `NumeracionesController` | `/api/v1/numeraciones` | Sí |
| 8 | `ProductosController` | `/api/v1/productos` | Sí |
| 9 | `ClientesController` | `/api/v1/clientes` | Sí |
| 10 | `ClientesCreditoController` | `/api/v1/clientes` | Sí |
| 11 | `ClientesPerfilesFiscalesController` | `/api/v1/clientes-perfiles-fiscales` | Sí |
| 12 | `ClienteSucursalesController` | `/api/v1/clientes/{id}/sucursales` | Sí |
| 13 | `ProveedoresController` | `/api/v1/proveedores` | Sí |
| 14 | `ContactosController` | `/api/v1/contactos` | Sí |
| 15 | `AlmacenesController` | `/api/v1/almacenes` | Sí |
| 16 | `CategoriasProductoController` | `/api/v1/categorias` | Sí |
| 17 | `UnidadesMedidaController` | `/api/v1/unidades-medida` | Sí |
| 18 | `PedidosVentaController` | `/api/v1/ventas/pedidos` | Sí |
| 19 | `VentasController` | `/api/v1/ventas` | Sí |
| 20 | `FacturasVentaController` | `/api/v1/facturas-venta` | Sí |
| 21 | `CuentasPorCobrarController` | `/api/v1/cuentas-por-cobrar` | Sí |
| 22 | `SiatMetodosPagoController` | `/api/v1/siat-metodos-pago` | Sí |
| 23 | `MovimientosInventarioController` | `/api/v1/inventario` | Sí |
| 24 | `MdmProductsController` | `/api/v1/mdm/products` | Sí |
| 25 | `MdmCompanyProductsController` | `/api/v1/mdm/company-products` | Sí |
| 26 | `MdmProductCodesController` | `/api/v1/mdm/product-codes` | Sí |
| 27 | `MdmCatalogsController` | `/api/v1/mdm/catalogs` | Sí |
| 28 | `MdmBrandsController` | `/api/v1/mdm/brands` | Sí |
| 29 | `MdmManufacturersController` | `/api/v1/mdm/manufacturers` | Sí |
| 30 | `MdmCategoriesController` | `/api/v1/mdm/categories` | Sí |
| 31 | `MdmAttributesController` | `/api/v1/mdm/attributes` | Sí |
| 32 | `MdmVariantsController` | `/api/v1/mdm/variants` | Sí |
| 33 | `MdmProductUomsController` | `/api/v1/mdm/product-uoms` | Sí |
| 34 | `OrdenesCompraController` | `/api/v1/compras/ordenes-compra` | Sí |
| 35 | `OrdenesPedidoController` | `/api/v1/compras/ordenes-pedido` | Sí |
| 36 | `RecepcionesCompraController` | `/api/v1/compras/recepciones` | Sí |
| 37 | `ImportacionesController` | `/api/v1/compras/importaciones` | Sí |
| 38 | `ExpedientesImportacionController` | `/api/v1/compras/expedientes-importacion` | Sí |
| 39 | `CuentasContablesController` | `/api/v1/contabilidad/cuentas` | Sí |
| 40 | `AsientosContablesController` | `/api/v1/contabilidad/asientos` | Sí |
| 41 | `PeriodosContablesController` | `/api/v1/contabilidad/periodos` | Sí |
| 42 | `CierresContablesController` | (pendiente) | Sí |
| 43 | `PlantillasContablesController` | `/api/v1/contabilidad/plantillas` | Sí |
| 44 | `PresupuestoController` | `/api/v1/contabilidad/presupuestos` | Sí |
| 45 | `ContabilidadController` | `/api/v1/contabilidad` | Sí |
| 46 | `EstadosFinancierosController` | `/api/v1/contabilidad/estados-financieros` | Sí |
| 47 | `CentrosCostoController` | `/api/v1/contabilidad/centros-costo` | Sí |
| 48 | `ConciliacionBancariaController` | `/api/v1/bancos/conciliaciones` | Sí |
| 49 | `TributarioController` | `/api/v1/tributario` | Sí |
| 50 | `GeografíaController` | `/api/v1/geografia` | Sí |
| 51 | `SeguridadDinamicaController` | `/api/v1/seguridad-dinamica` | Sí |
| 52 | `AuditLogsController` | `/api/v1/audit-logs` | Sí |
| 53 | `HojasRutaController` | `/api/v1/logistica/hojas-ruta` | Sí |
| 54 | `DocumentosController` | `/api/v1/documentos` | Sí |
| 55 | `WorkflowController` | `/api/v1/workflow` | Sí |
| 56 | `PrcPriceListsController` | `/api/v1/prc/price-lists` | Sí |
| 57 | `RulIndustriesController` | `/api/v1/rul/industries` | Sí |
| 58 | `DiagnosticsController` | `/api/v1/diagnostics` | No (AllowAnonymous) |
| 59 | `UsuarioSucursalAccesosController` | `/api/v1/usuarios-sucursales-accesos` | Sí |

---

## 4. Endpoints por Controller (detalle crítico)

### AuthController (`/api/v1/auth`)

| Método | Endpoint | Descripción |
|---|---|---|
| POST | `/login` | Autenticación de usuario (AllowAnonymous) |
| GET | `/me` | Información del usuario autenticado |
| GET | `/mis-empresas` | Empresas asignadas al usuario |
| POST | `/seleccionar-empresa` | Cambiar empresa activa |
| POST | `/refresh` | Renovar token |

### VentasController (`/api/v1/ventas`)

| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/` | Lista paginada con filtros (VentaFilterDto) |
| GET | `/pagos` | Pagos paginados (VentaPagoFilterDto) |
| GET | `/{id:long}` | Detalle de venta |
| POST | `/` | Crear venta |
| PUT | `/{id:long}` | Actualizar venta |
| POST | `/{id:long}/confirmar` | Confirmar venta (descuenta inventario + genera CxC) |
| POST | `/desde-pedido` | Crear venta desde pedido |
| POST | `/{id:long}/pagos` | Registrar pago |
| POST | `/{id:long}/anular` | Anular venta |
| POST | `/pagos/{id:long}/anular` | Anular pago |

### FacturasVentaController (`/api/v1/facturas-venta`)

| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/` | Lista paginada (FacturaVentaFilterDto) |
| GET | `/{id:long}` | Detalle de factura |
| GET | `/por-venta/{ventaId:long}` | Factura por ID de venta |
| POST | `/generar-desde-venta` | Generar factura desde venta |
| POST | `/{id:long}/anular` | Anular factura |

### CuentasPorCobrarController (`/api/v1/cuentas-por-cobrar`)

| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/` | Lista paginada (CuentaPorCobrarFilterDto) |
| GET | `/{id:long}` | Detalle con pagos |
| POST | `/generar-desde-factura/{facturaVentaId:long}` | Generar CxC desde factura |
| PUT | `/actualizar-por-pago/{facturaVentaId:long}` | Actualizar saldo por pago |
| PUT | `/{id:long}/anular` | Anular CxC |
| GET | `/saldo-pendiente` | Saldo total pendiente |
| GET | `/antiguedad-saldos` | Reporte de antigüedad |

### ClientesCreditoController

| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/{clienteId:int}/credito-configuracion` | Obtener config. de crédito |
| PUT | `/{clienteId:int}/credito-configuracion` | Guardar config. de crédito |

---

## 5. Middleware

### GlobalExceptionMiddleware

**Archivo:** `Middleware/GlobalExceptionMiddleware.cs`

Manejo global de excepciones no capturadas. Retorna `500 Internal Server Error` con detalle en desarrollo.

### TenantRequiredMiddleware

**Archivo:** `Middleware/TenantRequiredMiddleware.cs`

Valida que las peticiones a rutas tenant-aware incluyan el claim `EmpresaId`. 
- Las operaciones de escritura (POST/PUT/DELETE) **requieren** EmpresaId
- Las operaciones de lectura (GET) **no requieren** EmpresaId pero aplican filtro si existe
- **Rutas exentas:** `/auth/login`, `/auth/seleccionar-empresa`, `/auth/mis-empresas`, `/auth/me`, `/empresas`, `/health`, `/swagger`, `/diagnostics`

---

## 6. Servicios de API

### CurrentUserService

**Archivo:** `Services/CurrentUserService.cs`
**Namespace:** `AgoraHub360.ERP.Api.Services`

Implementa `ICurrentUserService` extrayendo claims JWT del `HttpContext`:
- `UserId` → `ClaimTypes.NameIdentifier`
- `UserName` → `ClaimTypes.Name`
- `EmpresaId` → `ClaimTypesCustom.EmpresaId`
- `PlatformRole` → `ClaimTypesCustom.PlatformRole`
- `TenantRole` → `ClaimTypesCustom.TenantRole`
- `TenantStatus` → `ClaimTypesCustom.TenantStatus`

### AuthService

**Archivo:** `Services/AuthService.cs`

Implementa `IAuthService` con lógica de login JWT, refresh token y cambio de empresa activa.

---

## 7. DTOs de Respuesta

### ApiResponse<T>

```csharp
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}
```

Todos los endpoints retornan `ApiResponse<T>` como envoltura estándar.

### PaginatedResultDto<T>

```csharp
public class PaginatedResultDto<T>
{
    public List<T> Items { get; set; }
    public int TotalItems { get; set; }
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalPaginas { get; }
}
```

---
