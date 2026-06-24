# 05 — DTOs Compartidos (Shared)

> **Proyecto:** AgoraHub360.ERP.Shared  
> **Última actualización:** 2026-06-11  
> **Propósito:** DTOs, ApiResponse, constantes, configuración y utilerías compartidas entre capas.

---

## 1. Estructura

```
src/AgoraHub360.ERP.Shared/
├── Configuration/
├── Constants/
├── DTOs/              ← ~300 DTOs en 25+ subdirectorios
├── Extensions/
└── Utils/
```

---

## 2. Organización de DTOs por Módulo

| Carpeta | Contenido |
|---|---|
| `DTOs/ActivosFijos/` | `ActivoFijoDtos.cs` |
| `DTOs/AuditLog/` | `AuditLogDto.cs`, `AuditLogFilterDto.cs` |
| `DTOs/Auth/` | `LoginRequestDto.cs`, `CambiarEmpresaResponseDto.cs`, `EmpresaSesionDto.cs`, `SeleccionarEmpresaRequestDto.cs` |
| `DTOs/Bancario/` | `ConciliacionDtos.cs` |
| `DTOs/Cliente/` | `ClienteDto.cs`, `CreateClienteDto.cs`, `UpdateClienteDto.cs` |
| `DTOs/Compras/` | DTOs de OrdenPedido, OrdenCompra, Recepción, Importación |
| `DTOs/Contabilidad/` | Asientos, Cuentas, Periodos, Cierres, Estados Financieros, Plantillas |
| `DTOs/Contabilidad/Importacion/` | DTOs específicos para importación de asientos |
| `DTOs/CxC/` | `CuentaPorCobrarDtos.cs`, `ClienteCreditoConfiguracionDtos.cs` |
| `DTOs/Empresa/` | DTOs de empresa, creación, demo |
| **`DTOs/FacturacionElectronica/`** | **DTOs FE: Proveedor, Ambiente, Configuración, Emisión, Anulación, Estado, Auditoría** |
| `DTOs/Inventario/` | Movimientos, Kardex, Stock |
| `DTOs/MDM/` | Clientes, Productos, Proveedores, Marcas, UoM, Atributos, Variantes, Precios |
| `DTOs/Numeracion/` | Numeración de documentos |
| `DTOs/Parametro/` | Parámetros de sistema |
| `DTOs/Seguridad/` | Módulos, Formularios, Permisos, Roles, Usuarios |
| `DTOs/Sucursal/` | Sucursales |
| `DTOs/Tributario/` | Impuestos |
| `DTOs/Usuario/` | Usuarios, roles |
| **`DTOs/Ventas/`** | **VentaDtos.cs, FacturaVentaDtos.cs, PedidoVentaDto.cs, SiatMetodoPagoDtos.cs** |
| `DTOs/Workflow/` | Tareas |

---

## 3. DTOs de Ventas

### `FacturaVentaDtos.cs`

| DTO | Propósito |
|---|---|
| `FacturaVentaDto` | Detalle completo de factura (incluye CxC fields) |
| `FacturaVentaResumenDto` | Lista resumida para grillas |
| `FacturaVentaDetalleDto` | Línea de detalle de factura |
| `FacturaVentaFilterDto` | Filtros: NumeroFactura, ClienteId, EstadoFactura, Fecha, Busqueda, Moneda, Top, Pagina |
| `GenerarFacturaVentaRequestDto` | Request para generar factura desde venta |
| `AnularFacturaVentaRequestDto` | Request para anular factura |

### `VentaDtos.cs`

| DTO | Propósito |
|---|---|
| `VentaDto` | Detalle completo de venta (incluye pagos) |
| `VentaResumenDto` | Lista resumida para grillas |
| `VentaDetalleDto` | Línea de detalle de venta |
| `VentaPagoDto` | Pago registrado en una venta |
| `VentaFacturacionDatosDto` | Datos fiscales de la venta |
| `VentaFilterDto` | Filtros: NumeroVenta, Cliente, Estado, Fecha, Busqueda |
| `VentaPagoFilterDto` | Filtros: Buscar, NroVenta, NroFactura, Cliente, TipoPago, EstadoPago, Fecha |
| `CrearVentaRequestDto` | Creación de venta |
| `RegistrarPagoVentaRequestDto` | Registro de pago |
| `AnularPagoVentaRequestDto` | Anulación de pago |
| `GenerarVentaDesdePedidoRequestDto` | Generar venta desde pedido |

### `PedidoVentaDto.cs`

| DTO | Propósito |
|---|---|
| `PedidoVentaDto` | Pedido de venta (incluye líneas) |
| `PedidoVentaDetalleDto` | Línea de detalle de pedido |
| `CreatePedidoVentaDto` | Creación de pedido (incluye Prioridad) |
| `UpdatePedidoVentaDto` | Actualización de pedido |
| `PedidoVentaFilterDto` | Filtros: Busqueda, NumeroPedido, Cliente, Estado, Prioridad, Fecha |

---

## 4. DTOs de Facturación Electrónica

| DTO | Propósito | ¿Expone secretos? |
|---|---|---|
| `ProveedorFEDto` | Catálogo de proveedor FE | ❌ No |
| `AmbienteFEDto` | Catálogo de ambiente FE | ❌ No |
| `ConfiguracionFEDto` | Config FE (solo `TieneClientSecret`, `TienePosToken`) | ❌ No expone valores |
| `CrearConfiguracionFERequestDto` | Crear config (recibe secretos) | N/A (request) |
| `ActualizarConfiguracionFERequestDto` | Actualizar config (secretos opcionales) | N/A (request) |
| `EmitirFacturaRequestDto` | Request de emisión | ❌ No |
| `FacturacionFEItemDto` | Item de línea para emisión | ❌ No |
| `EmisionFacturaResultDto` | Resultado de emisión | ❌ No |
| `AnularFacturaFERequestDto` | Request de anulación | ❌ No |
| `AnulacionFacturaResultDto` | Resultado de anulación | ❌ No |
| `EstadoFacturaResultDto` | Resultado de consulta de estado | ❌ No |
| `AuditoriaFEDto` | Registro de auditoría FE | ❌ No |
| `AuditoriaFEFiltroDto` | Filtros para consulta de auditoría | ❌ No |

---

## 5. DTOs de Auth

| DTO | Propósito |
|---|---|
| `LoginRequestDto` | Credenciales de login (Email, Password) |
| `SeleccionarEmpresaRequestDto` | Selección de empresa activa |
| `CambiarEmpresaResponseDto` | Respuesta con nuevo JWT |
| `EmpresaSesionDto` | Datos de empresa en sesión |
| `SucursalSesionDto` | Datos de sucursal en sesión |

---

## 6. ApiResponse

**Archivo:** `DTOs/ApiResponse.cs`

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}
```

Es el formato estándar de respuesta para **todos** los endpoints de la API.

---

## 7. PaginatedResultDto

**Archivo:** `DTOs/PaginatedResultDto.cs`

```csharp
public class PaginatedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalPaginas => (int)Math.Ceiling((double)TotalItems / TamanoPagina);
}
```

Usado en todos los endpoints de listado paginado.

---

## 8. Constantes

**Archivo:** `Constants/ClaimTypesCustom.cs`, `Constants/PolicyNames.cs`, `Constants/Roles.cs`

| Constante | Valores |
|---|---|
| `Roles.SuperAdmin` | SuperAdmin |
| `Roles.SystemAdmin` | SystemAdmin |
| `Roles.AdminEmpresa` | AdminEmpresa |
| `Roles.Operador` | Operador |
| `Roles.Viewer` | Viewer |
| `ClaimTypesCustom.EmpresaId` | Claim de empresa activa |
| `ClaimTypesCustom.PlatformRole` | Claim de rol global |
| `ClaimTypesCustom.TenantRole` | Claim de rol en empresa |
| `PolicyNames.RequireTenantAdmin` | Policy para admin de empresa |

---

## 9. Reglas de Exposición de Datos

| Regla | Aplicación |
|---|---|
| **Nunca exponer secretos** | `ConfiguracionFEDto` no incluye `ClientSecret` ni `PosToken` |
| **Nunca exponer tokens** | `EmisionFacturaResultDto` no incluye `access_token` |
| **Nunca exponer body completo** | `AuditoriaFEDto` no incluye request/response body |
| **Filtros siempre incluyen paginación** | Todos los filter DTOs tienen `Top`, `Pagina`, `TamanoPagina` |
