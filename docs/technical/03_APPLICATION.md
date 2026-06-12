# 03 — Capa de Aplicación (Application)

> **Proyecto:** AgoraHub360.ERP.Application  
> **Última actualización:** 2026-06-11  
> **Propósito:** Casos de uso del sistema, orquestación de repositorios, validaciones de negocio.

---

## 1. Estructura

```
src/AgoraHub360.ERP.Application/
├── Common/
│   ├── Result.cs              ← Result<T> con Value + Error + IsSuccess
│   └── (otros utilitarios)
├── Interfaces/
│   └── 77 interfaces de servicio (IService)
├── Services/
│   └── 61+ implementaciones de servicio
├── DependencyInjection.cs
└── (validadores FluentValidation si aplican)
```

---

## 2. Interfaces (77 total)

Todas las interfaces están en `Interfaces/` sin subdirectorios.

| Grupo | Interfaces |
|---|---|
| **Ventas** | `IFacturaVentaService`, `IVentaService`, `IPedidoVentaService`, `ICuentasPorCobrarService`, `IClienteCreditoConfiguracionService`, `IClienteService`, `IClienteSucursalService`, `IClientePerfilFiscalService` |
| **FE** | `IFacturacionElectronicaService`, `IConfiguracionFEService`, `IAuditoriaFEService`, `IFacturacionElectronicaProvider` |
| **Auth** | `IAuthService`, `ICurrentUserService` |
| **Seguridad** | `ISeguridadDinamicaService`, `IRolService`, `IUsuarioService`, `IUsuarioSucursalAccesoService` |
| **MDM** | `IProductService`, `IProductoService`, `ICompanyProductService`, `ICategoryService`, `IBrandService`, `IManufacturerService`, `IUnidadMedidaService`, `IProductUomService`, `IProductCodeService`, `IProductVariantService`, `IAttributeDefinitionService`, `ICatalogService`, `IPriceListService`, `IProveedorService`, `IAlmacenService` |
| **Contabilidad** | `IAsientoContableService`, `ICuentaContableService`, `IPeriodoContableService`, `IPlantillaContableService`, `ICierreContableService`, `ICentroCostoService`, `IEstadoFinancieroService`, `IConciliacionBancariaService`, `IContabilizacionService`, `IImpuestoService` |
| **Compras** | `IOrdenPedidoService`, `IOrdenCompraService`, `IRecepcionCompraService`, `IHojaImportacionService`, `IExpedienteImportacionService` |
| **Inventario** | `IMovimientoInventarioService` |
| **Infraestructura** | `IExportService`, `ICifradoService`, `IAsientoExportService`, `IAsientoImportService` |
| **Workflow** | `IWorkflowService` |
| **Core** | `IEmpresaService`, `ISucursalService`, `IParametroSistemaService`, `INumeracionDocumentoService`, `IAuditLogService`, `IEmpresaDemoService`, `IEmpresaSeedService`, `IConfiguracionInicialEmpresaService`, `IVersioningService` |

---

## 3. Servicios Principales

### 3.1 `FacturaVentaService`

**Archivo:** `Services/FacturaVentaService.cs`
**Dependencias:** 12 repositorios + 2 servicios

| Método | Descripción |
|---|---|
| `GetAllAsync(FacturaVentaFilterDto?, CancellationToken)` | Lista paginada con filtros (NroFactura, Cliente, Estado, Fecha, Moneda, Búsqueda) |
| `GetByIdAsync(long, CancellationToken)` | Detalle completo con líneas + CxC |
| `GetByVentaIdAsync(long, CancellationToken)` | Factura por venta |
| `GenerarDesdeVentaAsync(GenerarFacturaVentaRequestDto, CancellationToken)` | Genera factura desde venta confirmada. Asigna ItemCode, ActivityCode, NumeroFactura |
| `AnularAsync(long, AnularFacturaVentaRequestDto, CancellationToken)` | Anula factura, actualiza CxC |

### 3.2 `VentaService`

**Archivo:** `Services/VentaService.cs`
**Dependencias:** 13 repositorios + 4 servicios

| Método | Descripción |
|---|---|
| `GetAllAsync(VentaFilterDto?, CancellationToken)` | Lista paginada con filtros |
| `GetByIdAsync(long, CancellationToken)` | Detalle completo |
| `CreateAsync(CrearVentaRequestDto, CancellationToken)` | Crear venta con descuento por línea y total, descuenta inventario si aplica |
| `UpdateAsync(...)` | Actualizar venta |
| `ConfirmarAsync(...)` | Confirma venta, descuenta inventario |
| `RegistrarPagoAsync(...)` | Registra pago, actualiza CxC con transacción atómica |
| `CrearDesdePedidoAsync(...)` | Genera venta desde pedido confirmado |
| `AnularAsync(...)` | Anula venta |
| `GetPagosPagedAsync(...)` | Lista pagos paginados con filtros |
| `AnularPagoAsync(...)` | Anula pago individual |

### 3.3 `PedidoVentaService`

**Archivo:** `Services/PedidoVentaService.cs`
**Dependencias:** 9 repositorios

| Método | Descripción |
|---|---|
| `GetAllAsync(CancellationToken)` | Lista completa (sin paginar) |
| `GetPagedAsync(PedidoVentaFilterDto, CancellationToken)` | Lista paginada con filtros (Buscar, NroPedido, Cliente, Estado, Prioridad, Fecha, Top) |
| `GetByIdAsync(long, CancellationToken)` | Detalle con líneas |
| `CreateAsync(CreatePedidoVentaDto, CancellationToken)` | Crear pedido. Asigna Prioridad, Estado="Borrador" |
| `UpdateAsync(...)` | Actualizar pedido |
| `DeleteAsync(...)` | Borrado lógico |
| `ConfirmarAsync(...)` | Confirma pedido (reserva stock) |
| `GenerarVentaAsync(...)` | Genera venta desde pedido |

### 3.4 `FacturacionElectronicaService`

**Archivo:** `Services/FacturacionElectronicaService.cs`
**Dependencias:** `IConfiguracionFERepository`, `IAuditoriaFERepository`, `IEnumerable<IFacturacionElectronicaProvider>`, repositorios de ventas

| Método | Descripción |
|---|---|
| `EmitirAsync(long facturaVentaId, CancellationToken)` | Orquesta: obtiene config activa → resuelve provider → emite → guarda CUF/CUFD → registra auditoría |
| `AnularAsync(long facturaVentaId, string motivo, CancellationToken)` | Orquesta: obtiene config → resuelve provider → anula → registra auditoría |
| `VerificarEstadoAsync(long facturaVentaId, CancellationToken)` | Consulta estado contra el proveedor |

**Flujo de emisión:**
1. Obtiene `FacturaVenta` con detalles + datos fiscales del cliente
2. Valida que la factura esté en estado `Borrador` (no emitida aún)
3. Obtiene configuración FE activa de la empresa
4. Resuelve el proveedor por `CodigoProveedor` desde `IEnumerable<IFacturacionElectronicaProvider>`
5. Construye `EmitirFacturaRequestDto` con ItemCodes desde `CompanyProduct.Sku`
6. Llama al provider para emitir
7. Si éxito: guarda CUF, CUFD, BillUuid, actualiza estado de factura, registra auditoría
8. Si falla: registra auditoría con error, no marca factura como emitida

### 3.5 `ConfiguracionFEService`

**Archivo:** `Services/ConfiguracionFEService.cs`
**Dependencias:** `IConfiguracionFERepository`, `IProveedorFERepository`, `IAmbienteFERepository`, `ICifradoService`, `ICurrentUserService`

| Método | Descripción |
|---|---|
| `ListarProveedoresAsync` | Catálogo global de proveedores FE |
| `ListarAmbientesAsync` | Catálogo global de ambientes FE |
| `ListarPorEmpresaAsync` | Configuraciones FE de la empresa activa |
| `ObtenerPorIdAsync` | Config por Id (valida tenant) |
| `CrearAsync` | Crea config cifrando ClientSecret y PosToken |
| `ActualizarAsync` | Actualiza, recifra solo si se envía nuevo secreto |
| `ActivarAsync` | Activa una config, desactiva las demás de la misma empresa |
| `DesactivarAsync` | Desactiva una config |

### 3.6 `CuentasPorCobrarService`

**Archivo:** `Services/CuentasPorCobrarService.cs`
**Dependencias:** 5 repositorios

| Método | Descripción |
|---|---|
| `GetAllAsync(CuentaPorCobrarFilterDto?, CancellationToken)` | Lista paginada con filtros (Cliente, Estado, Fecha, Moneda) |
| `GetByIdAsync(long, CancellationToken)` | Detalle con pagos aplicados |
| `GenerarDesdeFacturaAsync(long, CancellationToken)` | Crea CxC desde factura |
| `ActualizarPorPagoAsync(long, decimal, CancellationToken)` | Actualiza saldo por pago |
| `ActualizarPorPagoVentaAsync(long, CancellationToken)` | Recalcula CxC desde pagos de la venta |
| `AnularAsync(long, CancellationToken)` | Anula CxC |
| `GetSaldoTotalPendienteAsync(CancellationToken)` | Resumen de saldo |
| `GetAntiguedadSaldosAsync(CancellationToken)` | Reporte de antigüedad |

### 3.7 `AuthService`

**Archivo:** `Services/AuthService.cs`
**Dependencias:** repositorios de usuario, empresa, rol + JWT

| Método | Descripción |
|---|---|
| `LoginAsync(LoginRequestDto)` | Autentica, genera JWT con claims |
| `CambiarEmpresaActivaAsync(SeleccionarEmpresaRequestDto)` | Renueva JWT con nuevo EmpresaId |
| `GetCurrentUserPermissionsAsync` | Permisos del usuario actual |
| `GetMisEmpresasAsync` | Empresas a las que tiene acceso |

---

## 4. Patrón de Resultado

```csharp
namespace AgoraHub360.ERP.Application.Common;

public class Result<T> : Result
{
    public T? Value { get; }
    public static Result<T> Success(T value) => ...;
    public static new Result<T> Failure(string error) => ...;
}
```

**Reglas del patrón:**
- Todos los servicios retornan `Result<T>` o `Result` (sin valor).
- `IsSuccess = false` + `Error` con mensaje descriptivo para errores de negocio.
- Excepciones técnicas (DB, red) se capturan y envuelven en `Result.Failure()`.
- **No se lanzan excepciones** en flujo normal de negocio.

---

## 5. Uso de IUnitOfWork

```csharp
await _unitOfWork.ExecuteInTransactionAsync(async () =>
{
    // Operaciones atómicas
    await _repo1.AddAsync(entity1);
    await _repo2.UpdateAsync(entity2);
});
```

Usado en:
- `ConfirmarAsync` (VentaService) — descuenta inventario + actualiza estado
- `RegistrarPagoAsync` (VentaService) — guarda pago + actualiza CxC
- `GenerarDesdeVentaAsync` (FacturaVentaService) — crea factura + actualiza CxC

---

## 6. Uso de ICurrentUserService

```csharp
public interface ICurrentUserService
{
    int? EmpresaId { get; }
    string? UserId { get; }
    string? Email { get; }
    // ...
}
```

Se inyecta en **todos los servicios** para:
1. Obtener `EmpresaId` para filtrar datos tenant-aware
2. Obtener `UserId` para auditoría (`CreadoPor`, `ModificadoPor`)
3. Validar que el usuario tenga acceso a la empresa

El `EmpresaId` **nunca** se acepta desde el DTO de entrada. Siempre viene del JWT.

---

## 7. DependencyInjection (Application)

**Archivo:** `Application/DependencyInjection.cs`

```csharp
services.AddScoped<IFacturaVentaService, FacturaVentaService>();
services.AddScoped<IVentaService, VentaService>();
services.AddScoped<IPedidoVentaService, PedidoVentaService>();
// ... ~76 registros Scoped
```

---

## 8. Reglas de Tenant

| Regla | Aplicación |
|---|---|
| EmpresaId desde JWT | ✅ `ICurrentUserService.EmpresaId` |
| No aceptar EmpresaId desde DTO | ✅ Verificado en FacturaVentaService, VentaService, FE Service |
| Auditoría incluye EmpresaId | ✅ AuditoriaFacturacion hereda de TenantEntity |
| Config FE tenant-aware | ✅ ConfiguracionFacturacionElectronica hereda de TenantEntity |
