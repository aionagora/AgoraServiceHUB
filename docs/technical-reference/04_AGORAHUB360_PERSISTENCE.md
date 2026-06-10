# AGORAHUB360 ERP — CAPA DE PERSISTENCIA

> **Proyecto:** `src/AgoraHub360.ERP.Persistence/`
> **Propósito:** Acceso a datos con Entity Framework Core, migraciones, repositorios y configuración de base de datos SQL Server.

---

## 1. Estructura de Carpetas

```
AgoraHub360.ERP.Persistence/
├── Configurations/    ← Fluent API EF Core (configuración de tablas)
│   ├── ACC/           ← Contabilidad
│   ├── ACT/           ← Activos Fijos
│   ├── BNC/           ← Bancos
│   ├── CMP/           ← Compras
│   ├── CST/           ← Costos
│   ├── CXC/           ← Cuentas por Cobrar
│   ├── DOC/           ← Documentos
│   ├── INV/           ← Inventario
│   ├── LOG/           ← Logística
│   ├── MDM/           ← Maestro de Datos
│   ├── PRC/           ← Precios
│   ├── RUL/           ← Reglas
│   ├── TRB/           ← Tributario
│   ├── VER/           ← Versionado
│   ├── VTA/           ← Ventas
│   ├── WF/            ← Workflow
│   └── + archivos sueltos (Core)
├── Context/           ← AgoraDbContext + Factory
├── Interceptors/      ← AuditableEntityInterceptor, EntityVersioningInterceptor
├── Migrations/        ← Migraciones EF Core (~55+ migraciones)
├── Repositories/      ← Repository<T>, WorkflowRepository
├── Services/          ← AuditLogService, EmpresaSeedService
└── DependencyInjection.cs ← Registro de persistencia
```

---

## 2. DbContext

**Archivo:** `Context/AgoraDbContext.cs`
**Namespace:** `AgoraHub360.ERP.Persistence.Context`
**Implementa:** `IUnitOfWork`

### Constructores

```csharp
// Constructor principal (runtime) — recibe ICurrentUserService para tenant filter
public AgoraDbContext(DbContextOptions<AgoraDbContext> options, ICurrentUserService currentUserService)

// Constructor para migraciones (design-time) — sin tenant
public AgoraDbContext(DbContextOptions<AgoraDbContext> options)
```

### DbSets (~80 DbSets)

| Categoría | DbSets |
|---|---|
| Core (21) | `Empresas`, `Sucursales`, `Usuarios`, `UsuarioEmpresas`, `Roles`, `Monedas`, `Paises`, `Departamentos`, `Provincias`, `Ciudades`, `Zonas`, `ParametrosSistema`, `NumeracionesDocumento`, `AuditLogs`, `PerfilesAcceso`, `PerfilesPermisos`, `ModulosSistema`, `FormulariosSistema`, `AccionesSistema`, `UsuariosPerfiles`, `UsuariosSucursalesAccesos` |
| MDM (22) | `Clientes`, `ClienteSucursales`, `ClientePerfilesFiscales`, `Contactos`, `ContactosUsuariosAccesos`, `Proveedores`, `Almacenes`, `UbicacionesAlmacen`, `Catalogs`, `Brands`, `Manufacturers`, `ProductStatuses`, `Uoms`, `Products`, `CompanyProducts`, `ProductCodes`, `Categories`, `ProductCategories`, `AttributeDefinitions`, `AttributeOptions`, `ProductAttributes`, `ProductVariants`, `VariantAttributeValues`, `ProductUoms`, `CompanyProductFeatures` |
| VTA (8) | `Ventas`, `VentaDetalles`, `VentaFacturacionDatos`, `VentaPagos`, `SiatMetodosPago`, `FacturasVenta`, `FacturaVentaDetalles` |
| INV (2) | `MovimientosInventario`, `StockProductos` |
| CXC (2) | `CuentasPorCobrar`, `ClienteCreditoConfiguraciones` |
| CMP (12) | `OrdenesPedido`, `OrdenPedidoLineas`, `OrdenesCompra`, `OrdenCompraLineas`, `ConfirmacionesProveedor`, `PagosOrdenCompra`, `ExpedientesImportacion`, `HitosExpediente`, `RecepcionesCompra`, `RecepcionCompraLineas`, `HojasImportacion`, `GastosImportacion`, `ImportacionLineas` |
| ACC (12) | `CuentasContables`, `CierresContables`, `AsientosContables`, `AsientoContableLineas`, `PeriodosContables`, `PlantillasContables`, `PlantillaContableLineas`, `TiposComprobante`, `TiposCambio`, `TiposPago`, `PresupuestosContables`, `PresupuestosContablesLineas` |
| Otros | `ActivosFijos`, `DepreciacionesMensuales`, `ExtractosBancarios`, `ConciliacionesBancarias`, `RegistrosImpuesto`, `PriceLists`, `PriceListItems`, `CostingRules`, `LandedCostProfiles`, `CentrosCosto`, `Industries`, `ProductIndustryRules`, `EntityVersions`, `Documents`, `ProductDocuments`, `ComprobanteDocumentos`, `HojasRuta`, `HojaRutaHistorial`, `Tareas`, `PlantillasTareas` |

---

## 3. Query Filter Global (Tenant)

```csharp
private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
{
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (!typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
            continue;

        var method = typeof(AgoraDbContext)
            .GetMethod(nameof(ApplyTenantFilter),
                BindingFlags.NonPublic | BindingFlags.Static)
            ?.MakeGenericMethod(entityType.ClrType);

        var filter = method?.Invoke(null, new object[] { _empresaId });
        entityType.SetQueryFilter((LambdaExpression)filter!);
    }
}
```

Aplica automáticamente `WHERE EmpresaId = @empresaId` a todas las entidades `TenantEntity`. Esto garantiza aislamiento total entre empresas sin intervención manual en las consultas.

---

## 4. Configuraciones (Fluent API)

> **Total: ~40+ configuraciones** en `Configurations/`

### Esquemas de Base de Datos

| Schema | Módulo | Ejemplo de tablas |
|---|---|---|
| `core` | Core | `Empresas`, `Usuarios`, `Roles`, `Sucursales` |
| `mdm` | MDM | `Products`, `Clientes`, `Proveedores`, `Almacenes` |
| `vta` | Ventas | `Ventas`, `FacturasVenta`, `PedidosVenta` |
| `inv` | Inventario | `MovimientosInventario`, `StockProducto` |
| `cxc` | CxC | `CuentasPorCobrar`, `ClienteCreditoConfiguraciones` |
| `cmp` | Compras | `OrdenesCompra`, `RecepcionesCompra`, `OrdenesPedido` |
| `acc` | Contabilidad | `CuentasContables`, `AsientosContables`, `PeriodosContables` |
| `act` | Activos Fijos | `ActivosFijos`, `DepreciacionesMensuales` |
| `bnc` | Bancos | `ExtractosBancarios`, `ConciliacionesBancarias` |
| `trb` | Tributario | `RegistrosImpuesto` |
| `prc` | Precios | `PriceLists`, `PriceListItems` |
| `rul` | Reglas | `Industries`, `ProductIndustryRules` |
| `ver` | Versionado | `EntityVersions` |
| `doc` | Documentos | `Documents`, `ComprobanteDocumentos` |
| `log` | Logística | `HojasRuta`, `HojaRutaHistorial` |
| `wf` | Workflow | `Tareas`, `PlantillasTareas` |
| `geo` | Geografía | `Paises`, `Departamentos`, `Provincias`, `Ciudades`, `Zonas` |

### Ejemplo de Configuración

```csharp
// Configurations/CXC/CuentaPorCobrarConfiguration.cs
builder.ToTable("CuentasPorCobrar", "cxc");
builder.HasKey(x => x.Id);
builder.Property(x => x.Id).UseIdentityColumn();

builder.HasOne(x => x.FacturaVenta)
    .WithMany()
    .HasForeignKey(x => x.FacturaVentaId)
    .OnDelete(DeleteBehavior.Restrict);

// SaldoPendiente es computed column
builder.Property(x => x.SaldoPendiente)
    .HasPrecision(18, 2)
    .HasComputedColumnSql("[TotalFactura] - [TotalPagado]", stored: true);

// Índices
builder.HasIndex(x => new { x.EmpresaId, x.FacturaVentaId }).IsUnique();
builder.HasIndex(x => new { x.EmpresaId, x.Estado });
builder.HasIndex(x => new { x.EmpresaId, x.ClienteId });
builder.HasIndex(x => new { x.EmpresaId, x.FechaVencimiento });
```

---

## 5. Catálogo de Tablas

| Entidad | Tabla | Schema | PK | FK clave |
|---|---|---|---|---|
| Empresa | Empresas | core | Id | — |
| Sucursal | Sucursales | core | Id | EmpresaId |
| Usuario | Usuarios | core | Id | — |
| UsuarioEmpresa | UsuarioEmpresas | core | (UsuarioId, EmpresaId) | UsuarioId, EmpresaId |
| Rol | Roles | core | Id | EmpresaId |
| Moneda | Monedas | core | Id | — |
| Pais | Paises | geo | Id | — |
| Departamento | Departamentos | geo | Id | PaisId |
| Provincia | Provincias | geo | Id | DepartamentoId |
| Ciudad | Ciudades | geo | Id | ProvinciaId |
| Zona | Zonas | geo | Id | CiudadId |
| ParametroSistema | ParametrosSistema | core | Id | EmpresaId |
| NumeracionDocumento | NumeracionesDocumento | core | Id | EmpresaId |
| Cliente | Clientes | mdm | Id | EmpresaId |
| ClienteSucursal | ClienteSucursales | mdm | Id | ClienteId |
| ClientePerfilFiscal | ClientePerfilesFiscales | mdm | Id | ClienteId |
| Product | Products | mdm | ProductId | EmpresaId, CatalogId, BrandId |
| CompanyProduct | CompanyProducts | mdm | Id | ProductId, EmpresaId |
| Almacen | Almacenes | mdm | Id | EmpresaId |
| Venta | Ventas | vta | Id | EmpresaId, SucursalId, ClienteId |
| VentaDetalle | VentaDetalles | vta | Id | VentaId, CompanyProductId |
| FacturaVenta | FacturasVenta | vta | Id | EmpresaId, VentaId |
| CuentaPorCobrar | CuentasPorCobrar | cxc | Id | EmpresaId, FacturaVentaId |
| MovimientoInventario | MovimientosInventario | inv | Id | EmpresaId, CompanyProductId |
| StockProducto | StockProducto | inv | Id | EmpresaId, CompanyProductId |
| OrdenCompra | OrdenesCompra | cmp | OrdenCompraId | EmpresaId, ProveedorId |
| CuentaContable | CuentasContables | acc | CuentaContableId | EmpresaId, CuentaPadreId |
| AsientoContable | AsientosContables | acc | AsientoContableId | EmpresaId, TipoComprobanteId |
| Y ~40 más... | | | | |

---

## 6. Repositorios

### Repository<T>

**Archivo:** `Repositories/Repository.cs`

Implementación genérica de `IRepository<T>` con métodos:
- `GetByIdAsync(int/long)` + overload `GetByIdIgnoreQueryFiltersAsync`
- `GetAllAsync`
- `FindAsync(Expression<Func<T, bool>>)` + overload `FindIgnoreQueryFiltersAsync`
- `AddAsync`
- `UpdateAsync`
- `DeleteAsync`

### WorkflowRepository

**Archivo:** `Repositories/WorkflowRepository.cs`

Repositorio especializado para tareas de workflow con consultas adicionales.

---

## 7. Interceptors

| Interceptor | Archivo | Función |
|---|---|---|
| `AuditableEntityInterceptor` | `Interceptors/AuditableEntityInterceptor.cs` | Asigna `FechaCreacion`/`CreadoPor` al insertar y `FechaModificacion`/`ModificadoPor` al actualizar automáticamente |
| `EntityVersioningInterceptor` | `Interceptors/EntityVersioningInterceptor.cs` | Registra versiones de entidades cuando se modifican |

---

## 8. Servicios de Persistencia

| Servicio | Archivo | Función |
|---|---|---|
| `AuditLogService` | `Services/AuditLogService.cs` | Registro centralizado de auditoría en base de datos |
| `EmpresaSeedService` | `Services/EmpresaSeedService.cs` | Seed de datos iniciales al crear una empresa (geografía, parámetros, etc.) |

---

## 9. Migraciones

> **Total: ~55+ migraciones** desde `20260217065412_BaseCore` hasta `20260610044131_VTA_PedidosVenta_Prioridad`

| Timeline | Migraciones clave |
|---|---|
| Feb 2026 | Base core, roles, auditoría, parámetros, admin seed |
| Feb-Mar 2026 | MDM (productos, clientes, proveedores, almacenes, atributos) |
| Mar 2026 | Ventas, facturas, CxC, inventario, CRUD contable |
| Mar-Abr 2026 | Compras (órdenes, recepciones, importaciones), contabilidad avanzada |
| May 2026 | Sucursales expandidas, geografía, workflow, logística |
| May-Jun 2026 | Módulo ventas comercial completo, perfiles fiscales, SIAT, CxC, crédito |

---
