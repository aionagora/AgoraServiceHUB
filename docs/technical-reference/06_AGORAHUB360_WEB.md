# AGORAHUB360 ERP — CAPA WEB (BLAZOR SERVER)

> **Proyecto:** `src/AgoraHub360.ERP.Web/`
> **Propósito:** Interfaz de usuario Blazor Server con páginas interactivas, componentes reutilizables, servicios HTTP y navegación.

---

## 1. Estructura de Carpetas

```
AgoraHub360.ERP.Web/
├── Components/        ← Componentes reutilizables
│   └── Shared/        ← ExportButtons
├── Helpers/           ← Clases auxiliares
├── Layout/            ← MainLayout, LoginLayout, NavMenu
├── Pages/             ← ~45+ páginas Razor
│   ├── Compras/       ← OrdenesCompra, OrdenesPedido, Recepciones, Importaciones
│   ├── Config/        ← EmpresaDemo
│   ├── Configuracion/ ← Auditoria, Empresas, Roles, Sucursales, Usuarios
│   ├── Contabilidad/  ← Asientos, BalanceGeneral, CierreContable, Comprobantes
│   ├── Dashboards/    ← DashboardCompras, DashboardContabilidad, DashboardInventario
│   ├── Inventario/    ← Almacenes, Kardex, Movimientos, Stock
│   ├── MDM/           ← Almacenes, Atributos, Categorias, Clientes, Productos...
│   └── Ventas/        ← AntiguedadSaldosCxC, CuentasPorCobrar, Facturas, Pagos...
├── Services/          ← ~50+ servicios HTTP (HttpClient wrappers)
├── Shared/            ← Componentes compartidos (ModalRegistrarPago, etc.)
├── wwwroot/           ← Archivos estáticos, CSS, JS
├── _Imports.razor     ← Importaciones globales
├── App.razor          ← Punto de entrada Blazor
└── Program.cs         ← Configuración Blazor Server
```

---

## 2. Navegación

### NavMenu.razor

**Archivo:** `Layout/NavMenu.razor`

Estructura del menú lateral:

```
Dashboard
├── Dashboard Principal
├── Dashboard Compras
├── Dashboard Contabilidad
└── Dashboard Inventario

Ventas
├── Ventas Comercial
├── Nueva Venta
├── Pedidos de Venta
├── Facturas de Venta
├── Cuentas por Cobrar
└── Registrar Pago

MDM (Maestro de Datos)
├── Productos
├── Productos por Empresa
├── Productos Globales
├── Clientes
├── Proveedores
├── Marcas
├── Fabricantes
├── Categorías
├── Unidades de Medida
├── Variantes de Producto
├── Atributos de Producto
├── Almacenes
└── Listas de Precios

Inventario
├── Movimientos
├── Stock / Kardex
└── Almacenes

Compras
├── Órdenes de Pedido
├── Órdenes de Compra
├── Recepciones de Compra
├── Importaciones
└── Expedientes de Importación

Contabilidad
├── Dashboard Contable
├── Asientos Contables
├── Plan de Cuentas
├── Periodos Contables
├── Cierre Contable
├── Plantillas Contables
├── Comprobantes
├── Balance General
├── Estado de Resultados
├── Libro Diario
├── Libro Mayor
├── Sumas y Saldos
├── Flujo de Efectivo
├── Centros de Costo
└── Presupuestos

Configuración
├── Empresas
├── Sucursales
├── Usuarios
├── Roles
├── Numeraciones de Documento
├── Parámetros
├── Auditoría
├── Demo Data
└── Workflow
```

---

## 3. Páginas por Módulo

### 3.1. Ventas (8 páginas)

| Página | Ruta | Servicios inyectados | Funcionalidad |
|---|---|---|---|
| `VentasComercial.razor` | `/ventas/comercial` | `VentaHttpService`, `ClienteHttpService`, `FacturaVentaHttpService` | Lista paginada con filtros, botón nueva venta, confirmar/anular, registrar pago, generar factura |
| `VentaComercialForm.razor` | `/ventas/comercial/nueva` | `VentaHttpService`, `ClienteHttpService`, `ProductoHttpService` | Formulario creación/edición con selección de cliente, sucursal, productos, totales |
| `Pedidos.razor` | `/ventas/pedidos` | `PedidoVentaHttpService` | Lista filtrada de pedidos |
| `PedidoForm.razor` | `/ventas/pedidos/nueva` | `PedidoVentaHttpService` | Formulario de pedido |
| `Facturas.razor` | `/ventas/facturas` | `FacturaVentaHttpService`, `ClienteHttpService` | Lista filtrada de facturas, botón generar factura desde venta |
| `PagoVenta.razor` | `/ventas/pagos` | `VentaHttpService`, `ClienteHttpService` | Registrar pago con selección de venta |
| `CuentasPorCobrar.razor` | `/ventas/cuentas-por-cobrar` | `CuentasPorCobrarHttpService`, `ClienteHttpService` | Cards de resumen, tabla paginada con filtros, enlace a antigüedad |
| `AntiguedadSaldosCxC.razor` | `/ventas/cuentas-por-cobrar/antiguedad-saldos` | `CuentasPorCobrarHttpService` | Reporte de antigüedad con rangos (NoVencido, 1-30, 31-60, 61-90, 90+) |

### 3.2. MDM (15 páginas)

| Página | Ruta | Servicios inyectados |
|---|---|---|
| `Productos.razor` | `/mdm/productos` | `ProductoHttpService`, `CatalogoHttpService`, `UnidadMedidaHttpService`, `BrandHttpService`, `ManufacturerHttpService`, `VariantService`, `ProductUomService`, `ProductCodeService`, `AttributeService`, `CompanyProductService`, `EmpresaSvc` |
| `ProductosEmpresa.razor` | `/mdm/productos-empresa` | `MdmCompanyProductHttpService` |
| `ProductosGlobales.razor` | `/mdm/productos-globales` | `MdmProductoGlobalHttpService` |
| `Clientes.razor` | `/mdm/clientes` | `ClienteHttpService`, `EmpresaStateService` |
| `ClienteForm.razor` | `/mdm/clientes/nuevo` | `ClienteHttpService` |
| `Proveedores.razor` | `/mdm/proveedores` | `ProveedorHttpService` |
| `Marcas.razor` | `/mdm/marcas` | `BrandHttpService` |
| `Fabricantes.razor` | `/mdm/fabricantes` | `ManufacturerHttpService` |
| `Categorias.razor` | `/mdm/categorias` | — |
| `UnidadesMedida.razor` | `/mdm/unidades-medida` | `UnidadMedidaHttpService` |
| `AtributosProducto.razor` | `/mdm/atributos` | `MdmAttributeHttpService` |
| `VariantesProducto.razor` | `/mdm/variantes` | `MdmVariantHttpService` |
| `Almacenes.razor` | `/mdm/almacenes` | — |
| `ListasPrecios.razor` | `/mdm/listas-precios` | `PriceListHttpService` |

### 3.3. Inventario (4 páginas)

| Página | Ruta | Funcionalidad |
|---|---|---|
| `Movimientos.razor` | `/inventario/movimientos` | Lista de movimientos con filtros |
| `Stock.razor` | `/inventario/stock` | Stock actual por producto |
| `Kardex.razor` | `/inventario/kardex` | Kardex valorado |
| `Almacenes.razor` | `/inventario/almacenes` | Gestión de almacenes |

### 3.4. Compras (5 páginas)

| Página | Ruta |
|---|---|
| `OrdenesPedido.razor` | `/compras/ordenes-pedido` |
| `OrdenPedidoForm.razor` | `/compras/ordenes-pedido/nueva` |
| `OrdenesCompra.razor` | `/compras/ordenes-compra` |
| `RecepcionesCompra.razor` | `/compras/recepciones` |
| `Importaciones.razor` | `/compras/importaciones` |
| `ExpedientesImportacion.razor` | `/compras/expedientes-importacion` |

### 3.5. Contabilidad (17 páginas)

| Página | Ruta | Funcionalidad |
|---|---|---|
| `DashboardContable.razor` | `/contabilidad` | Dashboard con indicadores |
| `AsientosContables.razor` | `/contabilidad/asientos` | CRUD de asientos, importar/exportar |
| `AsientosContables.razor.cs` | (code-behind) | Lógica de importación/exportación |
| `AsientosImportar.razor` | `/contabilidad/asientos/importar` | Importación masiva |
| `PlanCuentas.razor` | `/contabilidad/plan-cuentas` | Árbol jerárquico de cuentas |
| `PeriodosContables.razor` | `/contabilidad/periodos` | Gestión de periodos fiscales |
| `CierreContable.razor` | `/contabilidad/cierre` | Cierre de periodo |
| `CierreContable.razor.cs` | (code-behind) | Lógica de cierre |
| `PlantillasContables.razor` | `/contabilidad/plantillas` | Plantillas de asientos |
| `Comprobantes.razor` | `/contabilidad/comprobantes` | Comprobantes con documentos |
| `BalanceGeneral.razor` | `/contabilidad/balance-general` | Reporte de balance |
| `EstadoResultados.razor` | `/contabilidad/estado-resultados` | Reporte de resultados |
| `LibroDiario.razor` | `/contabilidad/libro-diario` | Reporte |
| `LibroMayor.razor` | `/contabilidad/libro-mayor` | Reporte |
| `SumasYSaldos.razor` | `/contabilidad/sumas-saldos` | Reporte |
| `FlujoDEfectivo.razor` | `/contabilidad/flujo-efectivo` | Reporte |
| `CentrosCosto.razor` | `/contabilidad/centros-costo` | Gestión de centros de costo |
| `Presupuestos.razor` | `/contabilidad/presupuestos` | Gestión de presupuestos |

### 3.6. Configuración (10 páginas)

| Página | Ruta |
|---|---|
| `Empresas.razor` | `/config/empresas` |
| `EmpresaDemo.razor` | `/config/demo` |
| `Sucursales.razor` | `/config/sucursales` |
| `SucursalForm.razor` | `/config/sucursales/nueva` |
| `Usuarios.razor` | `/config/usuarios` |
| `Roles.razor` | `/config/roles` |
| `NumeracionesDocumento.razor` | `/config/numeraciones` |
| `ParametrosNumeracion.razor` | `/config/parametros-numeracion` |
| `Auditoria.razor` | `/config/auditoria` |
| `ConfiguracionWorkflow.razor` | `/config/workflow` |

### 3.7. Generales

| Página | Ruta | Descripción |
|---|---|---|
| `Dashboard.razor` | `/` | Dashboard principal |
| `Login.razor` | `/login` | Página de inicio de sesión |
| `AccesosDirectos.razor` | (componente) | Atajos del dashboard |

---

## 4. Componentes Compartidos

| Componente | Archivo | Propósito |
|---|---|---|
| `ExportButtons.razor` | `Components/Shared/ExportButtons.razor` | Botones de exportación a Excel/PDF |
| `ModalRegistrarPago.razor` | `Shared/ModalRegistrarPago.razor` | Modal para registro de pagos |
| `ModalRegistrarPago.razor.cs` | `Shared/ModalRegistrarPago.razor.cs` | Code-behind del modal |
| `PlantillaComprobanteModal.razor` | `Shared/PlantillaComprobanteModal.razor` | Modal de selección de plantilla |
| `AccountSearchDropdown.razor` | `Shared/AccountSearchDropdown.razor` | Selector de cuentas contables |
| `RedirectToLogin.razor` | `Shared/RedirectToLogin.razor` | Redirección cuando no autenticado |
| `TareasTab.razor` | `Shared/TareasTab.razor` | Tabla de tareas de workflow |
| `VistaPreviewComprobante.razor` | `Shared/VistaPreviewComprobante.razor` | Vista previa de comprobante |

---

## 5. Servicios HTTP (Clientes)

> **Total: ~50 servicios** en `Services/`

| Servicio | Propósito |
|---|---|
| `AuthHttpService` | Login, refresh, cambio de empresa |
| `AuthMessageHandler` | Interceptor HTTP que agrega JWT Bearer header |
| `JwtAuthStateProvider` | AuthenticationStateProvider para Blazor |
| `SesionUsuarioStateService` | Estado global del usuario |
| `UiAuthorizationService` | Autorización UI |
| `EmpresaStateService` | Estado de empresa activa |
| `VentaHttpService` | CRUD ventas |
| `FacturaVentaHttpService` | CRUD facturas |
| `PedidoVentaHttpService` | CRUD pedidos |
| `CuentasPorCobrarHttpService` | CxC endpoints |
| `ClienteHttpService` | CRUD clientes |
| `ProductoHttpService` | CRUD productos |
| `ProveedorHttpService` | CRUD proveedores |
| `AlmacenHttpService` | CRUD almacenes |
| `MovimientoInventarioHttpService` | Movimientos de inventario |
| `OrdenCompraHttpService` | Órdenes de compra |
| `RecepcionCompraHttpService` | Recepciones |
| `AsientoContableHttpService` | Asientos contables |
| `CuentaContableHttpService` | Plan de cuentas |
| `PeriodoContableHttpService` | Periodos |
| `PlantillaContableHttpService` | Plantillas |
| Y ~30 más... | |

---

## 6. Layouts

| Layout | Archivo | Descripción |
|---|---|---|
| `MainLayout.razor` | `Layout/MainLayout.razor` | Layout principal con sidebar (NavMenu) y header |
| `LoginLayout.razor` | `Layout/LoginLayout.razor` | Layout minimalista para login |
| `NavMenu.razor` | `Layout/NavMenu.razor` | Menú lateral con navegación por módulos |

---

## 7. Program.cs

**Archivo:** `Program.cs`

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001/") });

// Registro de ~50 servicios HTTP
builder.Services.AddScoped<IVentaHttpService, VentaHttpService>();
// ...
```

---
