# 01 — Capa de Dominio (Domain)

> **Proyecto:** AgoraHUB360 ERP  
> **Última actualización:** 2026-06-11  
> **Propósito:** Definir las entidades del negocio, enums, interfaces de repositorio y Common base.

---

## 1. Jerarquía de Clases Base

### `AuditableEntity` — Raíz de todas las entidades

```csharp
namespace AgoraHub360.ERP.Domain.Common;

public abstract class AuditableEntity
{
    public DateTime FechaCreacion { get; set; }
    public string? CreadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? ModificadoPor { get; set; }
    public bool Activo { get; set; } = true;  // Soft-delete
}
```

**Todas las entidades** del sistema heredan directa o indirectamente de `AuditableEntity`.

### `TenantEntity` — Entidades multiempresa

```csharp
public abstract class TenantEntity : AuditableEntity
{
    public int EmpresaId { get; set; }
}
```

Toda entidad que pertenece a una empresa específica hereda de `TenantEntity`.  
El `EmpresaId` se asigna automáticamente desde `ICurrentUserService.EmpresaId` en los servicios de aplicación.

### `Result<T>` — Patrón de resultado

```csharp
public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
}
```

Existe tanto en `Domain.Common.Result` como en `Application.Common.Result<T>`.  
El de Application agrega `Value` genérico.

---

## 2. Entidades por Módulo

### ACC — Contabilidad (12 entidades)

| Entidad | Archivo | Tenant? |
|---|---|---|
| `AsientoContable` | `Entities/ACC/AsientoContable.cs` | ✅ |
| `AsientoContableLinea` | `Entities/ACC/AsientoContableLinea.cs` | ✅ |
| `CierreContable` | `Entities/ACC/CierreContable.cs` | ✅ |
| `CuentaContable` | `Entities/ACC/CuentaContable.cs` | ✅ |
| `PeriodoContable` | `Entities/ACC/PeriodoContable.cs` | ✅ |
| `PlantillaContable` | `Entities/ACC/PlantillaContable.cs` | ✅ |
| `PlantillaContableLinea` | `Entities/ACC/PlantillaContableLinea.cs` | ✅ |
| `PresupuestoContable` | `Entities/ACC/PresupuestoContable.cs` | ✅ |
| `PresupuestoContableLinea` | `Entities/ACC/PresupuestoContableLinea.cs` | ✅ |
| `TipoCambio` | `Entities/ACC/TipoCambio.cs` | ❌ Global |
| `TipoComprobante` | `Entities/ACC/TipoComprobante.cs` | ❌ Global |
| `TipoPago` | `Entities/ACC/TipoPago.cs` | ❌ Global |

### ACT — Activos Fijos (2 entidades)

| Entidad | Archivo | Tenant? |
|---|---|---|
| `ActivoFijo` | `Entities/ACT/ActivoFijo.cs` | ✅ |
| `DepreciacionMensual` | `Entities/ACT/DepreciacionMensual.cs` | ✅ |

### BNC — Bancario (2 entidades)

| Entidad | Archivo | Tenant? |
|---|---|---|
| `ConciliacionBancaria` | `Entities/BNC/ConciliacionBancaria.cs` | ✅ |
| `ExtractoBancario` | `Entities/BNC/ExtractoBancario.cs` | ✅ |

### CMP — Compras (14 entidades)

| Entidad | Archivo | Tenant? |
|---|---|---|
| `ConfirmacionProveedor` | `Entities/CMP/ConfirmacionProveedor.cs` | ✅ |
| `ExpedienteImportacion` | `Entities/CMP/ExpedienteImportacion.cs` | ✅ |
| `GastoImportacion` | `Entities/CMP/GastoImportacion.cs` | ✅ |
| `HitoExpediente` | `Entities/CMP/HitoExpediente.cs` | ✅ |
| `HojaImportacion` | `Entities/CMP/HojaImportacion.cs` | ✅ |
| `ImportacionLinea` | `Entities/CMP/ImportacionLinea.cs` | ✅ |
| `OrdenCompra` | `Entities/CMP/OrdenCompra.cs` | ✅ |
| `OrdenCompraLinea` | `Entities/CMP/OrdenCompraLinea.cs` | ✅ |
| `OrdenPedido` | `Entities/CMP/OrdenPedido.cs` | ✅ |
| `OrdenPedidoLinea` | `Entities/CMP/OrdenPedidoLinea.cs` | ✅ |
| `PagoOrdenCompra` | `Entities/CMP/PagoOrdenCompra.cs` | ✅ |
| `RecepcionCompra` | `Entities/CMP/RecepcionCompra.cs` | ✅ |
| `RecepcionCompraLinea` | `Entities/CMP/RecepcionCompraLinea.cs` | ✅ |
| `TipoPagoImportacion` | `Entities/CMP/TipoPagoImportacion.cs` | ❌ Global |

### Core — Núcleo del Sistema (21 entidades)

| Entidad | Archivo | Tenant? |
|---|---|---|
| `AccionSistema` | `Entities/Core/AccionSistema.cs` | ❌ Global |
| `AuditLog` | `Entities/Core/AuditLog.cs` | ✅ |
| `Ciudad` | `Entities/Core/Ciudad.cs` | ❌ Global |
| `Departamento` | `Entities/Core/Departamento.cs` | ❌ Global |
| `Empresa` | `Entities/Core/Empresa.cs` | ❌ Global |
| `FormularioSistema` | `Entities/Core/FormularioSistema.cs` | ❌ Global |
| `ModuloSistema` | `Entities/Core/ModuloSistema.cs` | ❌ Global |
| `Moneda` | `Entities/Core/Moneda.cs` | ❌ Global |
| `NumeracionDocumento` | `Entities/Core/NumeracionDocumento.cs` | ✅ |
| `Pais` | `Entities/Core/Pais.cs` | ❌ Global |
| `ParametroSistema` | `Entities/Core/ParametroSistema.cs` | ✅ |
| `PerfilAcceso` | `Entities/Core/PerfilAcceso.cs` | ❌ Global |
| `PerfilPermiso` | `Entities/Core/PerfilPermiso.cs` | ❌ Global |
| `Provincia` | `Entities/Core/Provincia.cs` | ❌ Global |
| `Rol` | `Entities/Core/Rol.cs` | ❌ Global |
| `Sucursal` | `Entities/Core/Sucursal.cs` | ✅ |
| `Usuario` | `Entities/Core/Usuario.cs` | ❌ Global |
| `UsuarioEmpresa` | `Entities/Core/UsuarioEmpresa.cs` | ❌ (relacional) |
| `UsuarioPerfil` | `Entities/Core/UsuarioPerfil.cs` | ❌ (relacional) |
| `UsuarioSucursalAcceso` | `Entities/Core/UsuarioSucursalAcceso.cs` | ✅ |
| `Zona` | `Entities/Core/Zona.cs` | ❌ Global |

### CXC — Cuentas por Cobrar (2 entidades)

| Entidad | Archivo | Tenant? |
|---|---|---|
| `ClienteCreditoConfiguracion` | `Entities/CXC/ClienteCreditoConfiguracion.cs` | ✅ |
| `CuentaPorCobrar` | `Entities/CXC/CuentaPorCobrar.cs` | ✅ |

### FE — Facturación Electrónica (4 entidades)

| Entidad | Archivo | Tenant? |
|---|---|---|
| `AmbienteFacturacionElectronica` | `Entities/FE/AmbienteFacturacionElectronica.cs` | ❌ Global (catálogo) |
| `AuditoriaFacturacion` | `Entities/FE/AuditoriaFacturacion.cs` | ✅ |
| `ConfiguracionFacturacionElectronica` | `Entities/FE/ConfiguracionFacturacionElectronica.cs` | ✅ |
| `ProveedorFacturacionElectronica` | `Entities/FE/ProveedorFacturacionElectronica.cs` | ❌ Global (catálogo) |

### INV — Inventario (2 entidades)

| Entidad | Archivo | Tenant? |
|---|---|---|
| `MovimientoInventario` | `Entities/INV/MovimientoInventario.cs` | ✅ |
| `StockProducto` | `Entities/INV/StockProducto.cs` | ✅ |

### MDM — Datos Maestros (~28 entidades)

| Entidad | Archivo | Tenant? |
|---|---|---|
| `Almacen` | `Entities/MDM/Almacen.cs` | ✅ |
| `AttributeDefinition` | `Entities/MDM/AttributeDefinition.cs` | ❌ Global |
| `Brand` | `Entities/MDM/Brand.cs` | ❌ Global |
| `Catalog` | `Entities/MDM/Catalog.cs` | ❌ Global |
| `Category` | `Entities/MDM/Category.cs` | ❌ Global |
| `Cliente` | `Entities/MDM/Cliente.cs` | ✅ |
| `ClientePerfilFiscal` | `Entities/MDM/ClientePerfilFiscal.cs` | ✅ |
| `ClienteSucursal` | `Entities/MDM/ClienteSucursal.cs` | ✅ |
| `CompanyProduct` | `Entities/MDM/CompanyProduct.cs` | ✅ |
| `Contacto` | `Entities/MDM/Contacto.cs` | ✅ |
| `Manufacturer` | `Entities/MDM/Manufacturer.cs` | ❌ Global |
| `Product` | `Entities/MDM/Product.cs` | ❌ Global |
| `ProductVariant` | `Entities/MDM/ProductVariant.cs` | ❌ Global |
| `Proveedor` | `Entities/MDM/Proveedor.cs` | ✅ |
| `Uom` | `Entities/MDM/Uom.cs` | ❌ Global |
| ... | (más entidades de atributos, códigos, clasificaciones) | |

### VTA — Ventas (9 entidades)

| Entidad | Archivo | Tenant? |
|---|---|---|
| `FacturaVenta` | `Entities/VTA/FacturaVenta.cs` | ✅ |
| `FacturaVentaDetalle` | `Entities/VTA/FacturaVentaDetalle.cs` | ✅ |
| `PedidoVenta` | `Entities/VTA/PedidoVenta.cs` | ✅ |
| `PedidoVentaDetalle` | `Entities/VTA/PedidoVentaDetalle.cs` | ✅ |
| `SiatMetodoPago` | `Entities/VTA/SiatMetodoPago.cs` | ❌ Global |
| `Venta` | `Entities/VTA/Venta.cs` | ✅ |
| `VentaDetalle` | `Entities/VTA/VentaDetalle.cs` | ✅ |
| `VentaFacturacionDatos` | `Entities/VTA/VentaFacturacionDatos.cs` | ✅ |
| `VentaPago` | `Entities/VTA/VentaPago.cs` | ✅ |

---

## 3. Enums Existentes (17)

| Enum | Valores |
|---|---|
| `EstadoCuentaPorCobrar` | `Pendiente, Pagada, Vencida, Anulada` |
| `EstadoDocumento` | `Borrador, Emitido, Recibido, Anulado` |
| `EstadoFacturaVenta` | `NoGenerada, Pendiente, Generada, Anulada` |
| `EstadoFacturaVentaComercial` | `Borrador, Generada, Anulada` |
| `EstadoPagoVenta` | `Pendiente, Pagado, Anulado` |
| `EstadoSiatFactura` | `NoEnviada, Pendiente, Validada, Rechazada, Anulada` |
| `EstadoVenta` | `Borrador, Confirmada, Facturada, Despachada, Pagada, Anulada` |
| `ModoPago` | (valores según SIAT) |
| `NivelUrgencia` | `Baja, Normal, Alta, Urgente` |
| `PlatformRole` | `SystemAdmin, Admin, Operador, Viewer, None` |
| `TenantRole` | `Admin, Operador, Viewer, None` |
| `TipoCuenta` | (tipos contables) |
| `TipoDocumentoFactura` | (tipos fiscales) |
| `TipoItemVenta` | `Producto, Servicio, Cargo` |
| `TipoPago` | `Efectivo, TarjetaCredito, TarjetaDebito, Transferencia, Cheque, Deposito, Otro` |
| `TipoProducto` | (clasificación de producto) |
| `TipoVenta` | `Directa, DesdePedido` |

---

## 4. Interfaces de Repositorio (7)

| Interfaz | Archivo | Propósito |
|---|---|---|
| `IRepository<T>` | `Interfaces/IRepository.cs` | CRUD genérico con Find, query filters |
| `IUnitOfWork` | `Interfaces/IUnitOfWork.cs` | Transacciones atómicas, SaveChanges |
| `IAuditoriaFERepository` | `Interfaces/IAuditoriaFERepository.cs` | Auditoría FE (write + read) |
| `IConfiguracionFERepository` | `Interfaces/IConfiguracionFERepository.cs` | Configuración FE por empresa |
| `IProveedorFERepository` | `Interfaces/IProveedorFERepository.cs` | Catálogo de proveedores FE |
| `IAmbienteFERepository` | `Interfaces/IAmbienteFERepository.cs` | Catálogo de ambientes FE |
| `IWorkflowRepository` | `Interfaces/IWorkflowRepository.cs` | Tareas y plantillas de workflow |

### `IRepository<T>` — Métodos

```csharp
Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
Task<T?> GetByIdAsync(long id, CancellationToken ct = default);
Task<T?> GetByIdIgnoreQueryFiltersAsync(int id, CancellationToken ct = default);
Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
Task<IReadOnlyList<T>> FindIgnoreQueryFiltersAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
Task<T> AddAsync(T entity, CancellationToken ct = default);
Task UpdateAsync(T entity, CancellationToken ct = default);
Task DeleteAsync(T entity, CancellationToken ct = default);
```

---

## 5. Reglas del Dominio

### Soft-Delete
Toda entidad usa `Activo` (bool) para borrado lógico. Ninguna entidad se elimina físicamente en operaciones normales.

### Multiempresa (Tenant Isolation)
- Entidades de negocio heredan de `TenantEntity` (tienen `EmpresaId`).
- Catálogos globales (países, monedas, marcas, etc.) heredan solo de `AuditableEntity`.
- El `EmpresaId` nunca viene del cliente; lo provee `ICurrentUserService` desde el JWT.

### Facturación Electrónica
- `ConfiguracionFacturacionElectronica` es tenant-aware (`TenantEntity`).
- `ProveedorFacturacionElectronica` y `AmbienteFacturacionElectronica` son catálogos globales.
- `AuditoriaFacturacion` es tenant-aware y guarda snapshots textuales del proveedor/ambiente (no FK a catálogos, para preservar histórico aunque los catálogos cambien).

---

## 6. Riesgos de Diseño detectados

| Riesgo | Descripción |
|---|---|
| **Dos enums de estado factura** | Existen `EstadoFacturaVenta` y `EstadoFacturaVentaComercial` con valores similares pero no idénticos. Puede causar confusión en mapeos. |
| **Entidades Core sin tenant** | `Usuario`, `Rol`, `PerfilPermiso` son globales — la seguridad es multi-empresa pero los usuarios son globales. |
| **Sin enum de proveedor FE** | Los proveedores FE viven en BD como catálogo, no como enum. Consistente con el patrón multi-proveedor, pero requiere validación en runtime. |

---

## 7. Archivos del Proyecto

```
src/AgoraHub360.ERP.Domain/
├── Common/
│   ├── AuditableEntity.cs
│   ├── Result.cs
│   └── TenantEntity.cs
├── Entities/
│   ├── ACC/  (12 entidades)
│   ├── ACT/  (2 entidades)
│   ├── BNC/  (2 entidades)
│   ├── CMP/  (14 entidades)
│   ├── Core/ (21 entidades)
│   ├── CST/  (costos)
│   ├── CXC/  (2 entidades)
│   ├── DOC/  (documentos)
│   ├── FE/   (4 entidades)
│   ├── INV/  (2 entidades)
│   ├── MDM/  (~28 entidades)
│   ├── VTA/  (9 entidades)
│   └── Workflow/ (2 entidades)
├── Enums/   (17 enums)
└── Interfaces/ (7 interfaces)
```

**Total: ~162 entidades, 17 enums, 7 interfaces de repositorio.**
