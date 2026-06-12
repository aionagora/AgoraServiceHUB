# 04 — API REST (Api)

> **Proyecto:** AgoraHub360.ERP.Api  
> **Última actualización:** 2026-06-11  
> **Propósito:** Punto de entrada REST API con JWT, versionado, middleware tenant, Swagger.

---

## 1. Controllers (49 total)

```
src/AgoraHub360.ERP.Api/Controllers/V1/
├── AlmacenesController.cs
├── AsientosContablesController.cs
├── AuditLogsController.cs
├── AuthController.cs
├── CategoriasProductoController.cs
├── CentrosCostoController.cs
├── ClientePerfilesFiscalesController.cs
├── ClientesController.cs
├── ClientesCreditoController.cs
├── ClienteSucursalesController.cs
├── ConciliacionBancariaController.cs
├── ContabilidadController.cs
├── ContactosController.cs
├── CuentasContablesController.cs
├── CuentasPorCobrarController.cs
├── DiagnosticsController.cs
├── DocumentosController.cs
├── EmpresasController.cs
├── EstadosFinancierosController.cs
├── ExpedientesImportacionController.cs
├── FacturacionElectronicaController.cs
├── FacturasVentaController.cs
├── GeografiaController.cs
├── HojasRutaController.cs
├── ImportacionesController.cs
├── MdmAttributesController.cs
├── MdmBrandsController.cs
├── MdmCatalogsController.cs
├── MdmCategoriesController.cs
├── MdmCompanyProductsController.cs
├── MdmManufacturersController.cs
├── MdmProductCodesController.cs
├── MdmProductsController.cs
├── MdmProductUomsController.cs
├── MdmVariantsController.cs
├── MovimientosInventarioController.cs
├── NumeracionesController.cs
├── OrdenesCompraController.cs
├── OrdenesPedidoController.cs
├── ParametrosController.cs
├── PedidosVentaController.cs
├── PeriodosContablesController.cs
├── PlantillasContablesController.cs
├── PrcPriceListsController.cs
├── PresupuestoController.cs
├── ProductosController.cs
├── ProveedoresController.cs
├── RecepcionesCompraController.cs
├── RolesController.cs
├── RulIndustriesController.cs
├── SeguridadDinamicaController.cs
├── SiatMetodosPagoController.cs
├── SucursalesController.cs
├── TributarioController.cs
├── UnidadesMedidaController.cs
├── UsuariosController.cs
├── UsuarioSucursalAccesosController.cs
├── VentasController.cs
└── WorkflowController.cs
```

**Total: ~58 controllers** (algunos archivos pueden ser partials).

---

## 2. Rutas por Módulo

| Módulo | Ruta base | Controller(s) |
|---|---|---|
| **Auth** | `/api/v1/auth` | `AuthController` |
| **Ventas** | `/api/v1/ventas`, `/api/v1/facturas-venta`, `/api/v1/ventas/pedidos` | `VentasController`, `FacturasVentaController`, `PedidosVentaController` |
| **CxC** | `/api/v1/cuentas-por-cobrar`, `/api/v1/clientes-credito` | `CuentasPorCobrarController`, `ClientesCreditoController` |
| **FE** | `/api/v1/facturacion-electronica` | `FacturacionElectronicaController` |
| **MDM** | `/api/v1/productos`, `/api/v1/clientes`, `/api/v1/proveedores` | `ProductosController`, `ClientesController`, `ProveedoresController`, `Mdm*Controllers` |
| **Contabilidad** | `/api/v1/asientos-contables`, `/api/v1/cuentas-contables`, `/api/v1/contabilidad` | `AsientosContablesController`, `CuentasContablesController`, `ContabilidadController` |
| **Compras** | `/api/v1/compras/ordenes-pedido`, `/api/v1/compras/ordenes-compra` | `OrdenesPedidoController`, `OrdenesCompraController` |
| **Inventario** | `/api/v1/movimientos-inventario` | `MovimientosInventarioController` |
| **Seguridad** | `/api/v1/seguridad-dinamica`, `/api/v1/roles` | `SeguridadDinamicaController`, `RolesController` |
| **Empresa** | `/api/v1/empresas` | `EmpresasController` |
| **Workflow** | `/api/v1/workflow` | `WorkflowController` |

---

## 3. Rutas de Facturación Electrónica

| Método | Ruta | Acción |
|---|---|---|
| `GET` | `/api/v1/facturacion-electronica/proveedores` | Catálogo de proveedores FE |
| `GET` | `/api/v1/facturacion-electronica/ambientes` | Catálogo de ambientes FE |
| `GET` | `/api/v1/facturacion-electronica/configuraciones` | Configs FE de la empresa activa |
| `GET` | `/api/v1/facturacion-electronica/configuraciones/{id}` | Config FE por Id |
| `POST` | `/api/v1/facturacion-electronica/configuraciones` | Crear configuración FE |
| `PUT` | `/api/v1/facturacion-electronica/configuraciones/{id}` | Actualizar configuración FE |
| `POST` | `/api/v1/facturacion-electronica/configuraciones/{id}/activar` | Activar configuración |
| `POST` | `/api/v1/facturacion-electronica/configuraciones/{id}/desactivar` | Desactivar configuración |
| `POST` | `/api/v1/facturacion-electronica/emitir/{facturaVentaId}` | Emitir factura electrónica |
| `POST` | `/api/v1/facturacion-electronica/anular/{facturaVentaId}` | Anular factura electrónica |
| `GET` | `/api/v1/facturacion-electronica/estado/{facturaVentaId}` | Consultar estado |
| `GET` | `/api/v1/facturacion-electronica/auditoria` | Listar auditoría FE (con filtros) |
| `GET` | `/api/v1/facturacion-electronica/auditoria/factura/{facturaVentaId}` | Auditoría por factura |

---

## 4. Middleware Pipeline

Orden de ejecución:

```
1. GlobalExceptionMiddleware        → Captura excepciones no manejadas → 500
2. TenantRequiredMiddleware         → Valida EmpresaId en rutas protegidas
3. Authentication (JWT Bearer)      → Autentica usuario
4. Authorization (Policies)         → Valida roles/permisos
5. Controllers                      → Ejecutan la acción
```

---

## 5. JWT

- **Algoritmo:** HS256 (SymmetricSecurityKey)
- **Claims incluidos:**
  - `sub` — UserId
  - `email` — Email del usuario
  - `PlatformRole` — Rol global (SystemAdmin, SuperAdmin, etc.)
  - `TenantRole` — Rol en la empresa activa
  - `TenantId` / `EmpresaId` — Empresa activa
  - `TenantStatus` — `selected` o `none`
- **StubAuthHandler:** En desarrollo, cualquier token es aceptado (dev shortcut)

### Policies de Autorización

| Policy | Requisito |
|---|---|
| `RequireAuthenticated` | Usuario autenticado |
| `RequirePlatformSuperAdmin` | `PlatformRole = SuperAdmin` |
| `RequirePlatformAdmin` | `PlatformRole = SuperAdmin | SystemAdmin` |
| `RequireTenantSelected` | `TenantStatus = selected` + `EmpresaId > 0` |
| `RequireTenantAdmin` | `TenantRole = TenantOwner | AdminEmpresa` |
| `RequireTenantSupervisor` | Roles anteriores + `Supervisor` |
| `RequireTenantOperator` | Roles anteriores + `Operador` |
| `RequireBranchAccessRead` | Acceso a sucursal (solo lectura) |
| `RequireBranchAccessOperate` | Acceso a sucursal (operación) |
| `RequireAuditGlobalRead` | `PlatformRole = SuperAdmin | SystemAdmin | SecurityAuditor` |

---

## 6. ApiResponse

**Archivo:** `Shared/DTOs/ApiResponse.cs`

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null);
    public static ApiResponse<T> Fail(string error, string? message = null);
}
```

Usado en **todos los controllers** como envoltura estándar de respuesta.

---

## 7. Manejo de Errores

| Situación | Código HTTP | Respuesta |
|---|---|---|
| Éxito | 200 | `ApiResponse<T>.Ok(data)` |
| Creado | 201 | `CreatedAtAction` + `ApiResponse<T>.Ok` |
| Error de negocio | 400 | `ApiResponse<T>.Fail("mensaje")` |
| No encontrado | 404 | `ApiResponse<T>.Fail("no encontrado")` |
| No autorizado | 401 | JWT middleware |
| Prohibido | 403 | Policy middleware |
| Excepción no manejada | 500 | `GlobalExceptionMiddleware` |
| Rechazo Cirrus (negocio) | 200 | `ApiResponse.Fail("motivo")` — la comunicación con Cirrus fue exitosa |

---

## 8. Swagger

- URL: `https://localhost:5001/swagger`
- Título: `AgoraHub360 ERP API v1.0.0`
- Esquema JWT Bearer para autenticación
- Documentación de endpoints con `ProducesResponseType` en controllers seleccionados

---

## 9. CORS

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorDev", policy =>
    {
        policy.WithOrigins("http://localhost:5001", "https://localhost:5002")
              .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});
```

---

## 10. Program.cs Flujo

```
1. Configurar cultura (en-US numérica)
2. DI: HttpContextAccessor, CurrentUserService
3. Capas: AddApplication() + AddPersistence() + AddInfrastructure()
4. JWT Auth + StubAuthHandler
5. Policies de autorización
6. API Versioning (v1.0)
7. Controllers
8. Swagger
9. CORS
10. HealthChecks
11. Background Services
Build → Middleware pipeline → Run
```
