# 02 — Capa de Persistencia (Persistence)

> **Proyecto:** AgoraHub360.ERP.Persistence  
> **Última actualización:** 2026-06-11  
> **Propósito:** Acceso a datos con EF Core 8, SQL Server, migraciones, repositorios y configuraciones.

---

## 1. AgoraDbContext

**Archivo:** `Persistence/Context/AgoraDbContext.cs`

```csharp
namespace AgoraHub360.ERP.Persistence.Context;

public class AgoraDbContext : DbContext
```

Es el DbContext principal del sistema. Registra ~65 DbSets, uno por cada entidad.

### DbSets principales por esquema

| Esquema | DbSets clave |
|---|---|
| `acc` | `AsientosContables`, `AsientosContablesLineas`, `CuentasContables`, `PeriodosContables`, `PlantillasContables` |
| `act` | `ActivosFijos`, `DepreciacionesMensuales` |
| `bnc` | `ConciliacionesBancarias`, `ExtractosBancarios` |
| `cfg` | `ProveedoresFacturacionElectronica`, `AmbientesFacturacionElectronica`, `ConfiguracionesFacturacionElectronica`, `AuditoriasFacturacion` |
| `cmp` | `OrdenesPedido`, `OrdenesCompra`, `RecepcionesCompra`, `Importaciones`, `ExpedientesImportacion` |
| `core` | `Empresas`, `Sucursales`, `Usuarios`, `Roles`, `ModulosSistema`, `FormulariosSistema`, `ParametrosSistema`, `NumeracionesDocumento`, `AuditLogs` |
| `cxc` | `CuentasPorCobrar`, `ConfiguracionesCreditoCliente` |
| `inv` | `MovimientosInventario`, `StockProductos` |
| `mdm` | `Productos`, `ProductosEmpresa`, `Clientes`, `Proveedores`, `Almacenes`, `UnidadesMedida`, `Marcas`, `Fabricantes`, `Atributos`, `Variantes` |
| `vta` | `Ventas`, `FacturasVenta`, `PedidosVenta`, `PagosVenta`, `MetodosPagoSiat` |
| `Workflow` | `Tareas`, `PlantillasTarea` |

---

## 2. Query Filters (Tenant Isolation)

En `OnModelCreating`, se aplica un filtro global para todas las entidades que implementan `TenantEntity`:

```csharp
modelBuilder.Entity<TenantEntity>().HasQueryFilter(e => e.EmpresaId == _currentUser.EmpresaId);
```

Esto asegura que **todas las consultas** a entidades tenant-aware filtren automáticamente por `EmpresaId`.

### IgnoreQueryFilters

Solo se usa en:
- `EmpresaDemoService` — para semilla de datos demo.
- `Repository.FindIgnoreQueryFiltersAsync` — para casos excepcionales donde se necesita acceso global.

---

## 3. Interceptores

| Interceptor | Propósito |
|---|---|
| `AuditableEntityInterceptor` | Asigna `FechaCreacion`, `CreadoPor`, `FechaModificacion`, `ModificadoPor` automáticamente al guardar cambios. |

---

## 4. Configuraciones EF por Módulo

Ubicadas en `Persistence/Configurations/` organizadas por esquema:

| Carpeta | Contenido |
|---|---|
| `ACC/` | `AsientoContableConfiguration`, `CuentaContableConfiguration`, `PeriodoContableConfiguration` |
| `CMP/` | `OrdenPedidoConfiguration`, `OrdenCompraConfiguration`, `RecepcionCompraConfiguration` |
| `Core/` | `EmpresaConfiguration`, `SucursalConfiguration`, `UsuarioConfiguration`, `RolConfiguration`, `ParametroSistemaConfiguration` |
| `CXC/` | `CuentaPorCobrarConfiguration`, `ClienteCreditoConfiguracionConfiguration` |
| `FE/` | `ConfiguracionFEConfiguration`, `AuditoriaFEConfiguration`, `ProveedorFEConfiguration`, `AmbienteFEConfiguration` |
| `INV/` | `MovimientoInventarioConfiguration`, `StockProductoConfiguration` |
| `MDM/` | `ClienteConfiguration`, `ProductConfiguration`, `CompanyProductConfiguration`, `AlmacenConfiguration`, `UomConfiguration` |
| `VTA/` | `VentaConfiguration`, `VentaDetalleConfiguration`, `FacturaVentaConfiguration`, `FacturaVentaDetalleConfiguration`, `PedidoVentaConfiguration`, `PedidoVentaDetalleConfiguration`, `VentaPagoConfiguration` |

Cada configuración define:
- Nombre de tabla y esquema (`ToTable("FacturasVenta", "vta")`)
- Llave primaria
- Propiedades requeridas, longitud máxima, tipos
- Relaciones y FK
- Índices

### Convenciones detectadas

| Aspecto | Convención |
|---|---|
| Naming tablas | Plural PascalCase: `FacturasVenta`, `CuentasPorCobrar` |
| Esquemas | 3 letras minúsculas: `vta`, `mdm`, `acc`, `cxc`, `inv`, `cmp`, `cfg` |
| PK | `Id` autoincremental (Identity) |
| FK | `EntidadId` (ej: `FacturaVentaId`) |
| Soft delete | `Activo` bool en todas las entidades |
| Longitud strings | `nvarchar(50)` a `nvarchar(500)` según campo |

---

## 5. Esquemas SQL Usados

| Esquema | Módulo |
|---|---|
| `core` | Empresa, Sucursal, Usuario, Rol, Parametros, Numeración, Seguridad |
| `vta` | Ventas, Facturas, Pedidos, Pagos |
| `mdm` | Clientes, Productos, Proveedores, Almacenes, UOM, Marcas |
| `acc` | Contabilidad: asientos, cuentas, periodos |
| `cxc` | Cuentas por Cobrar |
| `inv` | Inventario, Stock |
| `cmp` | Compras: órdenes, recepciones, importaciones |
| `cfg` | **Facturación Electrónica** (creado en FE-S03) |
| `act` | Activos Fijos |
| `bnc` | Conciliación Bancaria |
| `Workflow` | Tareas y Workflow |

---

## 6. Migraciones Importantes

| Migración | Descripción |
|---|---|
| `20260419235537_UpdateSeguridadAndSucursales` | Seguridad dinámica + sucursales |
| `20260511235500_VTA_PedidosVent` | Creación de PedidosVenta |
| `20260518042528_AlmacenSucursalNavegacion` | Relación Almacén ↔ Sucursal |
| `20260522075005_AddSalesReservationFields` | Campos de reserva en ventas |
| `20260526094949_AddDetalleAdicionalVentaFactura` | Detalle adicional en ventas/facturas |
| `20260529174214_AddClientePerfilFiscalIdToFacturaVenta` | Perfil fiscal en factura |
| `20260531110128_AddSiatFieldsToFacturaVenta` | Campos SIAT (BillUuid, Cuf, Cufd, SiatQr) |
| `**20260610222150_FE_CfgFacturacionElectronica**` | **Esquema `cfg`** + tablas FE |
| `20260610044131_VTA_PedidosVenta_Prioridad` | Columna Prioridad en PedidosVenta |

---

## 7. Repositorios FE (Persistence/Repositories/FE/)

| Repositorio | Interfaz | Propósito |
|---|---|---|
| `ConfiguracionFERepository` | `IConfiguracionFERepository` | CRUD config FE + activa por empresa |
| `AuditoriaFERepository` | `IAuditoriaFERepository` | Write y read de auditoría FE |
| ProveedorFE (en repo genérico) | `IProveedorFERepository` | Catálogo de proveedores |
| AmbienteFE (en repo genérico) | `IAmbienteFERepository` | Catálogo de ambientes |

---

## 8. Esquema `cfg` (Facturación Electrónica)

Creado en la migración `FE_CfgFacturacionElectronica`:

### Tablas

| Tabla | Esquema | Tenant? | Propósito |
|---|---|---|---|
| `ProveedoresFacturacionElectronica` | `cfg` | ❌ Global | Catálogo de proveedores (CIRRUS, etc.) |
| `AmbientesFacturacionElectronica` | `cfg` | ❌ Global | Catálogo de ambientes (TEST, PRODUCCION) |
| `ConfiguracionFacturacionElectronica` | `cfg` | ✅ | Config FE por empresa (credenciales cifradas) |
| `AuditoriaFacturacion` | `cfg` | ✅ | Auditoría de operaciones FE |

---

## 9. Riesgos de Aislamiento Multiempresa

| Riesgo | Descripción | Mitigación |
|---|---|---|
| **IgnoreQueryFilters** | Solo en `EmpresaDemoService` y `FindIgnoreQueryFiltersAsync` | Bajo — no se usa en operaciones core |
| **EmpresaId desde DTO** | Algunos DTOs aceptan EmpresaId como parámetro | Medio — verificar que el servicio valide contra `ICurrentUserService` |
| **Config FE tenant-aware** | `ConfiguracionFacturacionElectronica` hereda de `TenantEntity` | ✅ Correcto |

---

## 10. DependencyInjection (Persistence)

**Archivo:** `Persistence/DependencyInjection.cs`

Registra:
- `AgoraDbContext` (Scoped)
- `IRepository<T>` → `Repository<T>` (Scoped)
- `IUnitOfWork` → `UnitOfWork` (Scoped)
- `IAuditoriaFERepository` → `AuditoriaFERepository` (Scoped)
- `IConfiguracionFERepository` → `ConfiguracionFERepository` (Scoped)
- `IProveedorFERepository` (Scoped)
- `IAmbienteFERepository` (Scoped)
