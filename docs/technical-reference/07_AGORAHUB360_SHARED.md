# AGORAHUB360 ERP — CAPA DE SHARED

> **Proyecto:** `src/AgoraHub360.ERP.Shared/`
> **Propósito:** DTOs compartidos, constantes de seguridad, configuraciones y utilerías consumidas por todas las capas.

---

## 1. Estructura de Carpetas

```
AgoraHub360.ERP.Shared/
├── Configuration/      ← AppFormattingOptions
├── Constants/          ← ClaimTypesCustom, PolicyNames, Roles, TenantStatus
├── DTOs/               ← ~20 subdirectorios de DTOs
│   ├── ActivosFijos/
│   ├── AuditLog/
│   ├── Auth/           ← LoginRequestDto, AuthResponseDto, CambiarEmpresaResponseDto
│   ├── Bancario/
│   ├── Cliente/
│   ├── Common/
│   ├── Compras/
│   ├── Contabilidad/
│   ├── CxC/            ← CuentaPorCobrarDtos, ClienteCreditoConfiguracionDtos
│   ├── DOC/
│   ├── Empresa/
│   ├── Inventario/
│   ├── Logistica/
│   ├── MDM/
│   ├── Notificacion/
│   ├── Numeracion/
│   ├── PRC/
│   ├── Presupuestos/
│   ├── Proveedor/
│   ├── Rol/
│   ├── RUL/
│   ├── Seguridad/
│   ├── Sucursal/
│   ├── Tributario/
│   ├── Usuario/
│   ├── Ventas/         ← VentaDtos, FacturaVentaDtos, PedidoVentaDtos, PagoDtos
│   ├── VER/
│   └── Workflow/
├── Extensions/         ← EstadoDocumentoExtensions
├── Utils/              ← ExportFormatHelper
└── PaginatedResultDto.cs  (raíz de DTOs)
```

---

## 2. DTOs por Módulo

### Auth

| DTO | Propiedades | Uso |
|---|---|---|
| `LoginRequestDto` | Email, Password | AuthController.Login |
| `AuthResponseDto` | Token, RefreshToken, Usuario, Empresas | Respuesta login |
| `CambiarEmpresaResponseDto` | Token, EmpresaId, EmpresaNombre | Cambio de empresa activa |

### Ventas

| DTO | Propósito |
|---|---|
| `VentaDto` | Detalle completo de venta con cliente, sucursal, detalles, pagos |
| `VentaResumenDto` | Resumen para listado paginado |
| `CrearVentaRequestDto` | Request de creación (clienteId, sucursalId, detalles, etc.) |
| `ActualizarVentaRequestDto` | Request de actualización |
| `ConfirmarVentaRequestDto` | Request de confirmación |
| `AnularVentaRequestDto` | Request de anulación |
| `GenerarVentaDesdePedidoRequestDto` | Request para crear venta desde pedido |
| `RegistrarPagoVentaRequestDto` | Request de registro de pago (monto, tipo, referencia) |
| `AnularPagoVentaRequestDto` | Request de anulación de pago (motivo) |
| `VentaPagoDto` | DTO de pago de venta |
| `VentaFilterDto` | Filtro de búsqueda (NumeroVenta, ClienteId, Estado, Fechas, Paginación) |
| `VentaPagoFilterDto` | Filtro de pagos (Busqueda, NumeroVenta, NumeroFactura, ClienteId, TipoPago, EstadoPago, Fechas, Paginación) |
| `PedidoVentaDto` | DTO de pedido de venta |
| `PedidoVentaFilterDto` | Filtro de pedidos |
| `FacturaVentaDto` | Detalle de factura con líneas |
| `FacturaVentaResumenDto` | Resumen para listado |
| `FacturaVentaFilterDto` | Filtro (NumeroFactura, ClienteId, Estado, Fechas, Moneda, Paginación) |
| `GenerarFacturaVentaRequestDto` | Request de generación de factura |
| `AnularFacturaVentaRequestDto` | Request de anulación |

### CxC

| DTO | Propósito |
|---|---|
| `CuentaPorCobrarResumenDto` | Resumen para listado paginado |
| `CuentaPorCobrarDetalleDto` | Detalle con pagos aplicados |
| `CuentaPorCobrarFilterDto` | Filtro (ClienteId, Estado, Fechas, NumeroFactura, Paginación) |
| `AntiguedadSaldosResumenDto` | Reporte de antigüedad |
| `AntiguedadSaldosClienteDto` | Desglose por cliente con rangos |
| `ClienteCreditoConfiguracionDto` | Configuración de crédito (habilitado, días, límite) |
| `GuardarClienteCreditoConfiguracionRequestDto` | Request de guardado |

### MDM

| DTO | Propósito |
|---|---|
| `ProductDto`, `ProductDto2` | Producto global |
| `CompanyProductDto` | Producto por empresa |
| `ClienteDto` | Cliente |
| `ProveedorDto` | Proveedor |
| `BrandDto` | Marca |
| `ManufacturerDto` | Fabricante |
| `UomDto` | Unidad de medida |
| `AlmacenDto` | Almacén |
| `CategoryDto` | Categoría |
| Y ~20+ más... | Atributos, variantes, códigos |

### Contabilidad

| DTO | Propósito |
|---|---|
| `CuentaContableDto` | Cuenta del plan |
| `AsientoContableDto` | Asiento con líneas |
| `AsientoContableLineaDto` | Línea Debe/Haber |
| `PeriodoContableDto` | Periodo fiscal |
| `PlantillaContableDto` | Plantilla de asiento |
| `CierreContableDto` | Cierre de periodo |
| `BalanceGeneralDto` | Reporte de balance |
| `EstadoResultadoDto` | Reporte de resultados |
| Y ~30+ más... | |

### Compras

| DTO | Propósito |
|---|---|
| `OrdenCompraDto` | Orden de compra |
| `OrdenPedidoDto` | Orden de pedido |
| `RecepcionCompraDto` | Recepción de mercadería |
| `ExpedienteImportacionDto` | Expediente de importación |
| Y ~15+ más... | |

### Comunes

| DTO | Propósito |
|---|---|
| `PaginatedResultDto<T>` | Resultado paginado genérico |
| `ApiResponse<T>` | Envoltura estándar de respuesta API |

---

## 3. Constantes

| Archivo | Contenido |
|---|---|
| `Constants/ClaimTypesCustom.cs` | `PlatformRole`, `TenantRole`, `EmpresaId`, `TenantStatus` |
| `Constants/PolicyNames.cs` | `RequireAuthenticated`, `RequirePlatformSuperAdmin`, `RequireTenantAdmin`, `RequireTenantActive` |
| `Constants/Roles.cs` | Roles del sistema |
| `Constants/TenantStatus.cs` | Estados del tenant |

### ClaimTypesCustom

```csharp
public static class ClaimTypesCustom
{
    public const string PlatformRole = "PlatformRole";
    public const string TenantRole = "TenantRole";
    public const string EmpresaId = "EmpresaId";
    public const string TenantStatus = "TenantStatus";
}
```

---

## 4. Configuración

| Archivo | Propósito |
|---|---|
| `Configuration/AppFormattingOptions.cs` | Configuración de cultura (formato numérico boliviano: separador miles `.`, decimal `,`) |

---

## 5. Utilidades

| Archivo | Propósito |
|---|---|
| `Utils/ExportFormatHelper.cs` | Helper para formatos de exportación (Excel, PDF) |
| `Extensions/EstadoDocumentoExtensions.cs` | Extension methods para `EstadoDocumento` |

---
