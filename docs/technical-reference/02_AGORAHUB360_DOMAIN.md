# AGORAHUB360 ERP — CAPA DE DOMINIO

> **Proyecto:** `src/AgoraHub360.ERP.Domain/`
> **Propósito:** Contiene las entidades del negocio, enums, interfaces de repositorio, excepciones y clases base.

---

## 1. Estructura de Carpetas

```
AgoraHub360.ERP.Domain/
├── Common/           ← Clases base (AuditableEntity, TenantEntity, Result)
├── Entities/         ← Entidades del dominio agrupadas por módulo
│   ├── Core/         ← Empresa, Usuario, Sucursal, Rol, Geografía, etc.
│   ├── MDM/          ← Producto, Cliente, Proveedor, Almacén, Catálogos
│   ├── VTA/          ← Venta, FacturaVenta, PedidoVenta, Pagos
│   ├── INV/          ← MovimientoInventario, StockProducto
│   ├── CXC/          ← CuentaPorCobrar, ClienteCreditoConfiguracion
│   ├── CMP/          ← OrdenCompra, OrdenPedido, RecepcionCompra, Importaciones
│   ├── ACC/          ← CuentaContable, AsientoContable, PeriodoContable
│   ├── ACT/          ← ActivoFijo, DepreciacionMensual
│   ├── BNC/          ← ExtractoBancario, ConciliacionBancaria
│   ├── CST/          ← CentroCosto, CostingRule, LandedCostProfile
│   ├── TRB/          ← RegistroImpuesto
│   ├── PRC/          ← PriceList, PriceListItem
│   ├── RUL/          ← Industry, ProductIndustryRule
│   ├── VER/          ← EntityVersion
│   ├── DOC/          ← Document, ComprobanteDocumento, ProductDocument
│   ├── LOG/          ← HojaRuta, HojaRutaHistorial
│   └── Workflow/     ← Tarea, PlantillaTarea
├── Enums/            ← 17 enumeraciones del sistema
├── Exceptions/       ← DomainException
└── Interfaces/       ← IRepository, IUnitOfWork, IWorkflowRepository
```

---

## 2. Clases Base

### AuditableEntity

**Archivo:** `Common/AuditableEntity.cs`
**Namespace:** `AgoraHub360.ERP.Domain.Common`

```csharp
public abstract class AuditableEntity
{
    public DateTime FechaCreacion { get; set; }
    public string? CreadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? ModificadoPor { get; set; }
    public bool Activo { get; set; } = true;
}
```

Todas las entidades del sistema heredan de esta clase. Los campos se asignan automáticamente mediante `AuditableEntityInterceptor`.

### TenantEntity

**Archivo:** `Common/TenantEntity.cs`
**Namespace:** `AgoraHub360.ERP.Domain.Common`

```csharp
public abstract class TenantEntity : AuditableEntity
{
    public int EmpresaId { get; set; }
}
```

Entidad base tenant-aware. Hereda de `AuditableEntity` y agrega `EmpresaId`. Todas las entidades que pertenecen a una empresa heredan de esta clase. El query filter global en `AgoraDbContext` garantiza el aislamiento automático.

### Result (no genérico)

**Archivo:** `Common/Result.cs`
**Namespace:** `AgoraHub360.ERP.Domain.Common`

Resultado simple para operaciones sin valor de retorno. Con `IsSuccess`, `Error`, `Success()`, `Failure()`.

---

## 3. Catálogo Completo de Entidades

> **Total: ~85 entidades distribuidas en 15 submódulos**

### 3.1. Core (21 entidades)

| Entidad | Archivo | TenantEntity | Descripción |
|---|---|---|---|
| `Empresa` | `Entities/Core/Empresa.cs` | No (hereda de AuditableEntity) | Entidad principal — el tenant. Configuración global |
| `Sucursal` | `Entities/Core/Sucursal.cs` | Sí | Sucursal/tienda de la empresa |
| `Usuario` | `Entities/Core/Usuario.cs` | No | Usuario del sistema (cross-tenant) |
| `UsuarioEmpresa` | `Entities/Core/UsuarioEmpresa.cs` | No | Asignación usuario ↔ empresa |
| `UsuarioSucursalAcceso` | `Entities/Core/UsuarioSucursalAcceso.cs` | Sí | Acceso a sucursal por usuario |
| `UsuarioPerfil` | `Entities/Core/UsuarioPerfil.cs` | Sí | Perfiles asignados al usuario |
| `Rol` | `Entities/Core/Rol.cs` | Sí | Roles del sistema por empresa |
| `PerfilAcceso` | `Entities/Core/PerfilAcceso.cs` | Sí | Perfiles de acceso |
| `PerfilPermiso` | `Entities/Core/PerfilPermiso.cs` | Sí | Permisos detallados por perfil |
| `ModuloSistema` | `Entities/Core/ModuloSistema.cs` | No | Catálogo de módulos del sistema |
| `FormularioSistema` | `Entities/Core/FormularioSistema.cs` | No | Formularios dentro de cada módulo |
| `AccionSistema` | `Entities/Core/AccionSistema.cs` | No | Acciones (CRUD) sobre formularios |
| `Moneda` | `Entities/Core/Moneda.cs` | No | Catálogo de monedas |
| `Pais` | `Entities/Core/Pais.cs` | No | Catálogo de países |
| `Departamento` | `Entities/Core/Departamento.cs` | No | Departamentos/estados |
| `Provincia` | `Entities/Core/Provincia.cs` | No | Provincias |
| `Ciudad` | `Entities/Core/Ciudad.cs` | No | Ciudades |
| `Zona` | `Entities/Core/Zona.cs` | No | Zonas/barrios |
| `ParametroSistema` | `Entities/Core/ParametroSistema.cs` | Sí | Parámetros configurables por empresa |
| `NumeracionDocumento` | `Entities/Core/NumeracionDocumento.cs` | Sí | Control de secuencias numéricas |
| `AuditLog` | `Entities/Core/AuditLog.cs` | Sí | Log de auditoría |

### 3.2. MDM — Maestro de Datos (24 entidades)

| Entidad | Archivo | TenantEntity | Descripción |
|---|---|---|---|
| `Cliente` | `Entities/MDM/Cliente.cs` | Sí | Cliente con código, RSO, NIT, contacto |
| `ClienteSucursal` | `Entities/MDM/ClienteSucursal.cs` | Sí | Sucursal/dirección del cliente |
| `ClientePerfilFiscal` | `Entities/MDM/ClientePerfilFiscal.cs` | Sí | Perfil fiscal para facturación SIAT |
| `Contacto` | `Entities/MDM/Contacto.cs` | Sí | Contacto de cliente/proveedor |
| `ContactoUsuarioAcceso` | `Entities/MDM/ContactoUsuarioAcceso.cs` | Sí | Acceso de usuario a contacto |
| `Proveedor` | `Entities/MDM/Proveedor.cs` | Sí | Proveedor |
| `Almacen` | `Entities/MDM/Almacen.cs` | Sí | Almacén/bodega |
| `UbicacionAlmacen` | `Entities/MDM/UbicacionAlmacen.cs` | Sí | Ubicación dentro del almacén |
| `Catalog` | `Entities/MDM/Catalog.cs` | Sí | Catálogo de productos |
| `Brand` | `Entities/MDM/Brand.cs` | Sí | Marca |
| `Manufacturer` | `Entities/MDM/Manufacturer.cs` | Sí | Fabricante |
| `ProductStatus` | `Entities/MDM/ProductStatus.cs` | Sí | Estado del ciclo de vida del producto |
| `Uom` | `Entities/MDM/Uom.cs` | No (AuditableEntity) | Unidad de medida (catálogo global) |
| `Product` | `Entities/MDM/Product.cs` | Sí | Producto maestro (item/servicio/kit) |
| `CompanyProduct` | `Entities/MDM/CompanyProduct.cs` | Sí | Producto por empresa (precios, SKU) |
| `ProductCode` | `Entities/MDM/ProductCode.cs` | Sí | Códigos alternativos (EAN, UPC, etc.) |
| `Category` | `Entities/MDM/Category.cs` | Sí | Categoría |
| `ProductCategory` | `Entities/MDM/ProductCategory.cs` | Sí | Asignación producto ↔ categoría |
| `ProductClassification` | `Entities/MDM/ProductClassification.cs` | Sí | Clasificación de producto |
| `ProductClassificationLink` | `Entities/MDM/ProductClassificationLink.cs` | Sí | Enlace producto ↔ clasificación |
| `AttributeDefinition` | `Entities/MDM/AttributeDefinition.cs` | Sí | Definición de atributo dinámico |
| `AttributeOption` | `Entities/MDM/AttributeOption.cs` | Sí | Opción de atributo (ej: talla S, M, L) |
| `ProductAttribute` | `Entities/MDM/ProductAttribute.cs` | Sí | Valor de atributo por producto |
| `ProductVariant` | `Entities/MDM/ProductVariant.cs` | Sí | Variante de producto (SKU específico) |
| `VariantAttributeValue` | `Entities/MDM/VariantAttributeValue.cs` | Sí | Valor de atributo por variante |
| `ProductUom` | `Entities/MDM/ProductUom.cs` | Sí | Presentación/UOM por producto |
| `CompanyProductFeature` | `Entities/MDM/CompanyProductFeature.cs` | Sí | Features activables por empresa |

### 3.3. Ventas — VTA (8 entidades)

| Entidad | Archivo | TenantEntity | Descripción |
|---|---|---|---|
| `PedidoVenta` | `Entities/VTA/PedidoVenta.cs` | Sí | Pedido/solicitud de cliente |
| `PedidoVentaDetalle` | `Entities/VTA/PedidoVentaDetalle.cs` | Sí | Línea de pedido |
| `Venta` | `Entities/VTA/Venta.cs` | Sí | Venta comercial confirmada |
| `VentaDetalle` | `Entities/VTA/VentaDetalle.cs` | Sí | Línea de venta |
| `VentaPago` | `Entities/VTA/VentaPago.cs` | Sí | Pago recibido contra una venta |
| `VentaFacturacionDatos` | `Entities/VTA/VentaFacturacionDatos.cs` | Sí | Datos de facturación asociados |
| `FacturaVenta` | `Entities/VTA/FacturaVenta.cs` | Sí | Factura electrónica (SIAT) |
| `FacturaVentaDetalle` | `Entities/VTA/FacturaVentaDetalle.cs` | Sí | Línea de factura |
| `SiatMetodoPago` | `Entities/VTA/SiatMetodoPago.cs` | No | Catálogo SIAT de métodos de pago |

### 3.4. Inventario — INV (2 entidades)

| Entidad | Archivo | TenantEntity | Descripción |
|---|---|---|---|
| `MovimientoInventario` | `Entities/INV/MovimientoInventario.cs` | Sí | Movimiento de entrada/salida/ajuste/transferencia |
| `StockProducto` | `Entities/INV/StockProducto.cs` | Sí | Stock actual por producto-almacén |

### 3.5. Cuentas por Cobrar — CXC (2 entidades)

| Entidad | Archivo | TenantEntity | Descripción |
|---|---|---|---|
| `CuentaPorCobrar` | `Entities/CXC/CuentaPorCobrar.cs` | Sí | Saldo pendiente de factura |
| `ClienteCreditoConfiguracion` | `Entities/CXC/ClienteCreditoConfiguracion.cs` | Sí | Configuración de crédito por cliente |

### 3.6. Compras — CMP (12 entidades)

| Entidad | Archivo | TenantEntity | Descripción |
|---|---|---|---|
| `OrdenPedido` | `Entities/CMP/OrdenPedido.cs` | Sí | Solicitud de compra interna |
| `OrdenPedidoLinea` | `Entities/CMP/OrdenPedidoLinea.cs` | Sí | Línea de solicitud |
| `OrdenCompra` | `Entities/CMP/OrdenCompra.cs` | Sí | Orden de compra a proveedor |
| `OrdenCompraLinea` | `Entities/CMP/OrdenCompraLinea.cs` | Sí | Línea de OC |
| `RecepcionCompra` | `Entities/CMP/RecepcionCompra.cs` | Sí | Recepción de mercadería |
| `RecepcionCompraLinea` | `Entities/CMP/RecepcionCompraLinea.cs` | Sí | Línea de recepción |
| `ConfirmacionProveedor` | `Entities/CMP/ConfirmacionProveedor.cs` | Sí | Confirmación del proveedor |
| `PagoOrdenCompra` | `Entities/CMP/PagoOrdenCompra.cs` | Sí | Pagos a proveedores |
| `ExpedienteImportacion` | `Entities/CMP/ExpedienteImportacion.cs` | Sí | Expediente de importación |
| `HitoExpediente` | `Entities/CMP/HitoExpediente.cs` | Sí | Hito dentro del expediente |
| `HojaImportacion` | `Entities/CMP/HojaImportacion.cs` | Sí | Hoja de importación |
| `GastoImportacion` | `Entities/CMP/GastoImportacion.cs` | Sí | Gasto asociado a importación |
| `ImportacionLinea` | `Entities/CMP/ImportacionLinea.cs` | Sí | Línea de importación |
| `TipoPagoImportacion` | `Entities/CMP/TipoPagoImportacion.cs` | No | Tipo de pago en importación |

### 3.7. Contabilidad — ACC (11 entidades)

| Entidad | Archivo | TenantEntity | Descripción |
|---|---|---|---|
| `CuentaContable` | `Entities/ACC/CuentaContable.cs` | Sí | Plan de cuentas jerárquico |
| `AsientoContable` | `Entities/ACC/AsientoContable.cs` | Sí | Comprobante contable |
| `AsientoContableLinea` | `Entities/ACC/AsientoContableLinea.cs` | Sí | Línea Debe/Haber |
| `PeriodoContable` | `Entities/ACC/PeriodoContable.cs` | Sí | Período fiscal |
| `CierreContable` | `Entities/ACC/CierreContable.cs` | Sí | Cierre de período |
| `PlantillaContable` | `Entities/ACC/PlantillaContable.cs` | Sí | Plantilla de asiento |
| `PlantillaContableLinea` | `Entities/ACC/PlantillaContableLinea.cs` | Sí | Línea de plantilla |
| `TipoComprobante` | `Entities/ACC/TipoComprobante.cs` | Sí | Tipo (Ingreso, Egreso, Traspaso) |
| `TipoCambio` | `Entities/ACC/TipoCambio.cs` | Sí | Tipo de cambio |
| `TipoPago` | `Entities/ACC/TipoPago.cs` | No | Catálogo de tipos de pago |
| `PresupuestoContable` | `Entities/ACC/PresupuestoContable.cs` | Sí | Presupuesto |
| `PresupuestoContableLinea` | `Entities/ACC/PresupuestoContableLinea.cs` | Sí | Línea de presupuesto |

### 3.8. Otros Módulos

| Módulo | Entidades |
|---|---|
| ACT — Activos Fijos | `ActivoFijo`, `DepreciacionMensual` |
| BNC — Bancos | `ExtractoBancario`, `ConciliacionBancaria` |
| CST — Costos | `CentroCosto`, `CostingRule`, `LandedCostProfile` |
| TRB — Tributario | `RegistroImpuesto` |
| PRC — Precios | `PriceList`, `PriceListItem` |
| RUL — Reglas | `Industry`, `ProductIndustryRule` |
| VER — Versionado | `EntityVersion` |
| DOC — Documentos | `Document`, `ComprobanteDocumento`, `ProductDocument` |
| LOG — Logística | `HojaRuta`, `HojaRutaHistorial` |
| Workflow | `Tarea`, `PlantillaTarea` |

---

## 4. Enumeraciones (17 enums)

| Enum | Archivo | Valores |
|---|---|---|
| `EstadoCuentaPorCobrar` | `Enums/EstadoCuentaPorCobrar.cs` | Pendiente=1, Parcial=2, Pagada=3, Vencida=4, Anulada=5 |
| `EstadoDocumento` | `Enums/EstadoDocumento.cs` | — |
| `EstadoFacturaVenta` | `Enums/EstadoFacturaVenta.cs` | — |
| `EstadoFacturaVentaComercial` | `Enums/EstadoFacturaVentaComercial.cs` | Borrador, Emitida, Anulada, Rechazada |
| `EstadoPagoVenta` | `Enums/EstadoPagoVenta.cs` | Pendiente, Pagado, Anulado |
| `EstadoSiatFactura` | `Enums/EstadoSiatFactura.cs` | NoEnviada, Enviada, Aceptada, Rechazada |
| `EstadoVenta` | `Enums/EstadoVenta.cs` | Borrador=1, Confirmada=2, Facturada=3, Despachada=4, Pagada=5, Anulada=6 |
| `ModoPago` | `Enums/ModoPago.cs` | — |
| `NivelUrgencia` | `Enums/NivelUrgencia.cs` | — |
| `PlatformRole` | `Enums/PlatformRole.cs` | SuperAdmin, Admin |
| `TenantRole` | `Enums/TenantRole.cs` | Admin, User |
| `TipoCuenta` | `Enums/TipoCuenta.cs` | Activo, Pasivo, Patrimonio, Ingreso, Gasto, Costo |
| `TipoDocumentoFactura` | `Enums/TipoDocumentoFactura.cs` | Factura, NotaCredito, NotaDebito |
| `TipoItemVenta` | `Enums/TipoItemVenta.cs` | — |
| `TipoPago` | `Enums/TipoPago.cs` | — |
| `TipoProducto` | `Enums/TipoProducto.cs` | — |
| `TipoVenta` | `Enums/TipoVenta.cs` | Directa=1, DesdePedido=2 |

---

## 5. Interfaces del Dominio

| Interfaz | Archivo | Propósito |
|---|---|---|
| `IRepository<T>` | `Interfaces/IRepository.cs` | CRUD genérico con `GetById`, `FindAsync`, `Add`, `Update`, `Delete` |
| `IUnitOfWork` | `Interfaces/IUnitOfWork.cs` | Unit of Work (Commit) |
| `IWorkflowRepository` | `Interfaces/IWorkflowRepository.cs` | Repositorio especializado para Workflow |

---

## 6. Excepciones

| Clase | Archivo | Propósito |
|---|---|---|
| `DomainException` | `Exceptions/DomainException.cs` | Excepción base para errores de dominio |

---

## 7. Reglas de Negocio por Entidad

### Empresa
- `IndustriaId`: 1=Retail, 2=Alimentos, 3=Farmacia, 4=Ferretería, 5=Textil, 6=Tecnología, 7=General
- `MetodoCosteoDefault`: 1=Promedio, 2=FIFO, 3=LIFO, 4=Estándar
- No hereda de TenantEntity (es el tenant mismo)

### Venta
- `EstadoVenta`: Borrador → Confirmada → Facturada → Pagada, o Anulada desde cualquier estado
- `TipoVenta`: Directa o DesdePedido
- `InventarioDescontado`: flag para control de doble descuento
- `FacturaGenerada`: flag para evitar doble facturación
- Relaciona con `Sucursal`, `Almacen`, `Cliente`, `Moneda`

### FacturaVenta
- `EstadoFacturaVentaComercial`: Borrador → Emitida → Anulada
- `EstadoSiatFactura`: NoEnviada → Enviada → Aceptada / Rechazada
- Campos específicos SIAT: `BillUuid`, `NumeroAutorizacion`, `SiatQr`, `EnlaceXml`, `EnlacePdf`
- `TipoDocumentoFactura`: Factura, NotaCredito, NotaDebito

### CuentaPorCobrar
- `SaldoPendiente` = `TotalFactura - TotalPagado` (cálculo automático)
- Estado derivado: Pendiente (saldo>0, no vencida), Parcial, Pagada, Vencida, Anulada
- Datos denormalizados de cliente y factura para consultas rápidas

### MovimientoInventario
- `MovementType`: Receipt, Issue, Adjustment, Transfer
- `Quantity`: siempre positiva (el tipo determina el signo)
- `UnitCost`: costo promedio en salida, costo de compra en entrada
- `TotalCost` = `UnitCost * Quantity`
- Transferencias tienen `WarehouseId` (origen) y `DestinationWarehouseId` (destino)

### OrdenCompra
- Flujo: Borrador → Confirmado → PendienteAprobacion → Aprobado → EnviadaProveedor → ConfirmadaProveedor → RecepcionParcial → Cerrado

### CuentaContable
- Estructura jerárquica N-nivel con `Codigo` y `CuentaPadreId`
- `TipoCuenta`: Activo, Pasivo, Patrimonio, Ingreso, Gasto, Costo
- `Naturaleza`: Deudora o Acreedora
- `Nivel`: 1=grupo, 2=subgrupo, 3=cuenta, 4=subcuenta

### Product
- `ProductKind`: 1=Item, 2=Service, 3=Kit, 4=RawMaterial, 5=Packaging
- `IsStockable`, `IsSellable`, `IsPurchasable`: flags de comportamiento

---
