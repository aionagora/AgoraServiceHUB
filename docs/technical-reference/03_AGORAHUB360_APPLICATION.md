# AGORAHUB360 ERP — CAPA DE APLICACIÓN

> **Proyecto:** `src/AgoraHub360.ERP.Application/`
> **Propósito:** Casos de uso, servicios de aplicación, interfaces de servicio, validadores y configuración de DI.

---

## 1. Estructura de Carpetas

```
AgoraHub360.ERP.Application/
├── Common/           ← Result<T> (resultado genérico)
├── Interfaces/       ← Interfaces de servicio (~76 archivos)
├── Services/         ← Implementaciones de servicios (~76 archivos)
├── Validators/       ← Validadores (FluentValidation)
└── DependencyInjection.cs ← Registro de dependencias
```

---

## 2. Interfaces de Servicio

> **Total: ~76 interfaces** en `src/AgoraHub360.ERP.Application/Interfaces/`

### Auth y Seguridad

| Interfaz | Métodos principales |
|---|---|
| `IAuthService` | `LoginAsync`, `RefreshTokenAsync`, `CambiarEmpresaActivaAsync` |
| `ICurrentUserService` | `UserId`, `UserIdInt`, `UserName`, `EmpresaId`, `TenantId`, `PlatformRole`, `TenantRole`, `TenantStatus`, `IsInRole` |
| `ISeguridadDinamicaService` | Gestión de permisos dinámicos |
| `IAuditLogService` | Registro de auditoría |

### MDM — Productos

| Interfaz | Métodos principales |
|---|---|
| `IProductService` | CRUD de producto global |
| `ICompanyProductService` | CRUD de producto por empresa |
| `IProductCodeService` | Gestión de códigos alternativos |
| `ICategoryService` | Categorías |
| `ICategoriaProductoService` | Categorización de productos |
| `IBrandService` | Marcas |
| `IManufacturerService` | Fabricantes |
| `IAttributeDefinitionService` | Atributos dinámicos |
| `IProductVariantService` | Variantes de producto |
| `IProductUomService` | Presentaciones/UOM |
| `IUomService` | Unidades de medida |
| `IUnidadMedidaService` | Unidad de medida (endpoints) |
| `ICatalogService` | Catálogos |

### MDM — Clientes, Proveedores, Almacenes

| Interfaz | Métodos principales |
|---|---|
| `IClienteService` | CRUD de clientes |
| `IClienteSucursalService` | Sucursales de cliente |
| `IClientePerfilFiscalService` | Perfiles fiscales |
| `IClienteCreditoConfiguracionService` | Configuración de crédito por cliente |
| `IProveedorService` | CRUD de proveedores |
| `IAlmacenService` | CRUD de almacenes |
| `IContactoService` | Contactos |
| `IContactoUsuarioAccesoService` | Acceso de usuarios a contactos |

### Ventas

| Interfaz | Métodos principales |
|---|---|
| `IPedidoVentaService` | CRUD de pedidos, `ConfirmarAsync`, `AnularAsync` |
| `IVentaService` | CRUD de ventas, `ConfirmarAsync`, `CrearDesdePedidoAsync`, `RegistrarPagoAsync`, `AnularAsync`, `GetPagosPagedAsync`, `AnularPagoAsync` |
| `IFacturaVentaService` | CRUD de facturas, `GenerarDesdeVentaAsync`, `AnularAsync` |
| `ISiatMetodoPagoService` | Catálogo SIAT de métodos de pago |

### Cuentas por Cobrar

| Interfaz | Métodos principales |
|---|---|
| `ICuentasPorCobrarService` | `GetAllAsync`, `GetByIdAsync`, `GenerarDesdeFacturaAsync`, `ActualizarPorPagoAsync`, `ActualizarPorPagoVentaAsync`, `AnularAsync`, `GetSaldoTotalPendienteAsync`, `GetAntiguedadSaldosAsync` |

### Inventario

| Interfaz | Métodos principales |
|---|---|
| `IMovimientoInventarioService` | CRUD de movimientos, `GetAllAsync`, `GetByIdAsync`, `RegistrarAsync` |

### Compras

| Interfaz | Métodos principales |
|---|---|
| `IOrdenPedidoService` | Órdenes de pedido |
| `IOrdenCompraService` | Órdenes de compra |
| `IRecepcionCompraService` | Recepciones de compra |
| `IExpedienteImportacionService` | Expedientes de importación |
| `IHojaImportacionService` | Hojas de importación |

### Contabilidad

| Interfaz | Métodos principales |
|---|---|
| `ICuentaContableService` | Plan de cuentas |
| `IAsientoContableService` | Asientos contables |
| `IPeriodoContableService` | Periodos fiscales |
| `ICierreContableService` | Cierre contable |
| `IPlantillaContableService` | Plantillas de asientos |
| `IContabilizacionService` | Contabilización automática |
| `IPresupuestoService` | Presupuestos |
| `IEstadoFinancieroService` | Estados financieros |
| `IGestionContableContextService` | Contexto contable |
| `IComprobanteDocumentoService` | Comprobantes con documentos |

### Geografía y Empresa

| Interfaz | Métodos principales |
|---|---|
| `IGeografiaService` | Países, departamentos, provincias, ciudades, zonas |
| `IEmpresaService` | CRUD de empresas |
| `IEmpresaDemoService` | Creación de empresa demo |
| `IEmpresaSeedService` | Seed inicial de empresa |
| `IConfiguracionInicialEmpresaService` | Configuración inicial |
| `ISucursalService` | CRUD de sucursales |
| `IUsuarioService` | CRUD de usuarios |
| `IUsuarioSucursalAccesoService` | Acceso a sucursales |
| `IRolService` | Roles |
| `INumeracionDocumentoService` | Control de numeración |
| `IParametroSistemaService` | Parámetros del sistema |

### Otros

| Interfaz | Propósito |
|---|---|
| `IActivoFijoService` | Activos fijos y depreciaciones |
| `IConciliacionBancariaService` | Conciliación bancaria |
| `IImpuestoService` | Registro de impuestos |
| `IPriceListService` | Listas de precios |
| `IIndustryService` | Industrias y reglas |
| `IVersioningService` | Versionado de entidades |
| `IWorkflowService` | Workflow |
| `INotificacionService` | Notificaciones |
| `IExportService` | Exportación de datos |
| `IAsientoExportService` | Exportación de asientos |
| `IAsientoImportService` | Importación de asientos |
| `IExtractoImportService` | Importación de extractos |
| `IOrdenPedidoService` | Órdenes de pedido |
| `IHojaRutaService` | Hojas de ruta logística |
| `IPeriodoContableNotificacionService` | Notificaciones de período |

---

## 3. Servicios Críticos

### 3.1. AuthService

**Archivo:** `src/AgoraHub360.ERP.Api/Services/AuthService.cs` (en API, no en Application)
**Namespace:** `AgoraHub360.ERP.Api.Services`

**Dependencias:**
- `IRepository<Usuario>`
- `IRepository<UsuarioEmpresa>`
- `IRepository<Empresa>`
- `IConfiguration`

**Métodos:**

| Método | Descripción |
|---|---|
| `LoginAsync(LoginRequestDto, CancellationToken)` | Valida credenciales (email + password hash) y genera JWT con claims |
| `RefreshTokenAsync(string, CancellationToken)` | Renueva token JWT |
| `CambiarEmpresaActivaAsync(int usuarioId, int empresaId, CancellationToken)` | Cambia empresa activa y regenera token |

**Flujo de Login:**
1. Buscar usuario por email
2. Validar password hash
3. Resolver PlatformRole (SuperAdmin si es platform admin)
4. Obtener empresas asignadas activas
5. Generar claims (PlatformRole, TenantRole, EmpresaId, TenantStatus)
6. Generar JWT con expiración configurable

### 3.2. VentaService

**Archivo:** `src/AgoraHub360.ERP.Application/Services/VentaService.cs`
**Namespace:** `AgoraHub360.ERP.Application.Services`

**Dependencias:** 16 repositorios + 3 servicios

| Dependencia | Tipo |
|---|---|
| `IRepository<Venta>` | Principal |
| `IRepository<VentaDetalle>` | Detalles |
| `IRepository<VentaFacturacionDatos>` | Datos de facturación |
| `IRepository<VentaPago>` | Pagos |
| `IRepository<Sucursal>` | Validación sucursal |
| `IRepository<Almacen>` | Validación almacén |
| `IRepository<Cliente>` | Validación cliente |
| `IRepository<ClientePerfilFiscal>` | Perfil fiscal |
| `IRepository<PedidoVenta>` | Pedido origen |
| `IRepository<PedidoVentaDetalle>` | Detalle del pedido |
| `IRepository<CompanyProduct>` | Producto empresa |
| `IRepository<Product>` | Producto global |
| `IRepository<Empresa>` | Empresa |
| `INumeracionDocumentoService` | Numeración automática |
| `ICurrentUserService` | Usuario actual |
| `IUnitOfWork` | Transacciones |
| `ICuentasPorCobrarService` | Generación de CxC |
| `IRepository<FacturaVenta>` | Factura asociada |

**Métodos:**

| Método | Descripción |
|---|---|
| `GetAllAsync(VentaFilterDto?, CancellationToken)` | Lista paginada con filtros |
| `GetByIdAsync(long, CancellationToken)` | Detalle completo con cliente, sucursal, detalles, pagos |
| `CreateAsync(CrearVentaRequestDto, CancellationToken)` | Crear venta con validaciones de stock, crédito y precios |
| `UpdateAsync(long, ActualizarVentaRequestDto, CancellationToken)` | Actualizar cabecera y detalles |
| `ConfirmarAsync(long, ConfirmarVentaRequestDto?, CancellationToken)` | Confirmar: descuenta inventario, genera CxC |
| `CrearDesdePedidoAsync(GenerarVentaDesdePedidoRequestDto, CancellationToken)` | Convierte pedido en venta |
| `RegistrarPagoAsync(long, RegistrarPagoVentaRequestDto, CancellationToken)` | Registra pago, actualiza CxC |
| `AnularAsync(long, AnularVentaRequestDto, CancellationToken)` | Anula venta, reversa inventario y CxC |
| `AnularPagoAsync(long, AnularPagoVentaRequestDto, CancellationToken)` | Anula pago individual |
| `GetPagosPagedAsync(VentaPagoFilterDto, CancellationToken)` | Paginación de pagos |

**Reglas de negocio:**
1. Validar existencia de: sucursal, almacén, cliente, productos
2. Validar límite de crédito del cliente si aplica
3. Numeración automática desde `NumeracionDocumento`
4. Al confirmar: descuenta inventario (vía `MovimientoInventario`)
5. Al confirmar: genera `CuentaPorCobrar` (si no es contado)
6. Al anular: reversa inventario y CxC

### 3.3. FacturaVentaService

**Archivo:** `src/AgoraHub360.ERP.Application/Services/FacturaVentaService.cs`
**Namespace:** `AgoraHub360.ERP.Application.Services`

**Dependencias:** 12 repositorios + 2 servicios

| Método | Descripción |
|---|---|
| `GetAllAsync(FacturaVentaFilterDto?, CancellationToken)` | Lista paginada con filtros |
| `GetByIdAsync(long, CancellationToken)` | Detalle completo con líneas |
| `GetByVentaIdAsync(long, CancellationToken)` | Factura por venta |
| `GenerarDesdeVentaAsync(GenerarFacturaVentaRequestDto, CancellationToken)` | Genera factura desde venta confirmada |
| `AnularAsync(long, AnularFacturaVentaRequestDto, CancellationToken)` | Anula factura |

### 3.4. CuentasPorCobrarService

**Archivo:** `src/AgoraHub360.ERP.Application/Services/CuentasPorCobrarService.cs`
**Namespace:** `AgoraHub360.ERP.Application.Services`

**Dependencias:** 5 repositorios + 2 servicios

| Método | Descripción |
|---|---|
| `GetAllAsync(CuentaPorCobrarFilterDto?, CancellationToken)` | Lista paginada con filtros |
| `GetByIdAsync(long, CancellationToken)` | Detalle con pagos aplicados |
| `GenerarDesdeFacturaAsync(long, CancellationToken)` | Crea CxC al facturar |
| `ActualizarPorPagoAsync(long, decimal, CancellationToken)` | Actualiza saldo por pago recibido |
| `ActualizarPorPagoVentaAsync(long, CancellationToken)` | Actualiza saldo por ventaId (busca factura asociada) |
| `AnularAsync(long, CancellationToken)` | Anula CxC |
| `GetSaldoTotalPendienteAsync(CancellationToken)` | Suma total pendiente |
| `GetAntiguedadSaldosAsync(CancellationToken)` | Reporte de antigüedad |

### 3.5. PedidoVentaService

**Archivo:** `src/AgoraHub360.ERP.Application/Services/PedidoVentaService.cs`
**Namespace:** `AgoraHub360.ERP.Application.Services`

**Dependencias:** 8 repositorios

Funcionalidad: CRUD de pedidos de venta (solicitudes de cliente previas a la venta). Incluye confirmación y anulación con verificación de stock disponible.

### 3.6. MovimientoInventarioService

**Archivo:** `src/AgoraHub360.ERP.Application/Services/MovimientoInventarioService.cs`
**Namespace:** `AgoraHub360.ERP.Application.Services`

**Dependencias:** 4 repositorios

| Método | Descripción |
|---|---|
| `GetAllAsync(int?, int?, string?, DateTime?, DateTime?)` | Lista filtrada |
| `GetByIdAsync(int, CancellationToken)` | Detalle |
| `RegistrarAsync(CrearMovimientoInventarioDto, CancellationToken)` | Registra movimiento y actualiza stock |

**Regla de negocio:** Al registrar un movimiento, actualiza `StockProducto` (incrementa o decrementa según `MovementType`).

### 3.7. ProductService

**Archivo:** `src/AgoraHub360.ERP.Application/Services/ProductService.cs`
**Namespace:** `AgoraHub360.ERP.Application.Services`

**Dependencias:** 6 repositorios

| Método | Descripción |
|---|---|
| `GetAllAsync(long?, CancellationToken)` | Productos por catálogo (filtro tenant automático) |
| `GetByIdAsync(long, CancellationToken)` | Detalle con navegaciones |
| `CreateAsync(CreateProductRequestDto, CancellationToken)` | Creación validada |
| `UpdateAsync(long, UpdateProductRequestDto, CancellationToken)` | Actualización |
| `DeleteAsync(long, CancellationToken)` | Eliminación lógica |

### 3.8. EmpresaService

**Archivo:** `src/AgoraHub360.ERP.Application/Services/EmpresaService.cs`
**Namespace:** `AgoraHub360.ERP.Application.Services`

**Dependencias:** 4 repositorios + 2 servicios de semilla

| Método | Descripción |
|---|---|
| `GetAllAsync(CancellationToken)` | Todas las empresas |
| `GetByIdAsync(int, CancellationToken)` | Detalle |
| `CreateAsync(CrearEmpresaRequestDto, CancellationToken)` | Creación con seed de datos iniciales |
| `UpdateAsync(int, ActualizarEmpresaRequestDto, CancellationToken)` | Actualización |
| `DeleteAsync(int, CancellationToken)` | Eliminación lógica |

---

## 4. Dependency Injection

**Archivo:** `src/AgoraHub360.ERP.Application/DependencyInjection.cs`

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registro de todos los servicios de aplicación
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IVentaService, VentaService>();
        // ... ~76 registros

        return services;
    }
}
```

---

## 5. Reglas de Negocio Globales

| Regla | Aplicación |
|---|---|
| Filtro tenant automático | Todos los servicios — vía `TenantEntity` + query filter |
| Transaccionalidad | Servicios críticos usan `IUnitOfWork` para commit atómico |
| Numeración automática | Ventas, facturas, compras — vía `NumeracionDocumento` |
| Descuento de inventario | Al confirmar venta (si `IsStockable`) |
| Generación de CxC | Al facturar venta con saldo pendiente |
| Control de crédito | Validación de límite antes de confirmar venta a crédito |
| Auditoría automática | `AuditableEntityInterceptor` registra fechas y usuarios |

---
