# AgoraHub360 ERP — Documento de Estado del Proyecto

> **Versión del Sistema:** v1.0.0  
> **Gestión:** 2026  
> **Fecha del Documento:** Febrero 2026  
> **Plataforma:** .NET 8.0 | Blazor WebAssembly | SQL Server  
> **Repositorio:** `https://github.com/abelcalvimontes/AgoraHUB360-ERP`  
> **Rama:** `main`

---

## Índice

1. [Descripción General](#1-descripción-general)
2. [Arquitectura del Sistema](#2-arquitectura-del-sistema)
3. [Estructura de la Solución](#3-estructura-de-la-solución)
4. [Módulos Funcionales](#4-módulos-funcionales)
5. [Modelo de Datos — Entidades por Dominio](#5-modelo-de-datos--entidades-por-dominio)
6. [Capa de Aplicación — Servicios e Interfaces](#6-capa-de-aplicación--servicios-e-interfaces)
7. [API REST — Endpoints](#7-api-rest--endpoints)
8. [Frontend — Páginas Blazor](#8-frontend--páginas-blazor)
9. [Persistencia — Base de Datos](#9-persistencia--base-de-datos)
10. [Patrones y Convenciones](#10-patrones-y-convenciones)
11. [Seguridad y Autenticación](#11-seguridad-y-autenticación)
12. [Estado de Migraciones](#12-estado-de-migraciones)
13. [Dependencias y Paquetes NuGet](#13-dependencias-y-paquetes-nuget)
14. [Métricas del Proyecto](#14-métricas-del-proyecto)
15. [Roadmap y Módulos Pendientes](#15-roadmap-y-módulos-pendientes)

---

## 1. Descripción General

**AgoraHub360 ERP** es un sistema ERP modular empresarial desarrollado bajo el **Modelo Ágora (MAPE)**, diseñado para entornos mixtos de importación y comercialización, con arquitectura limpia, escalable y **multiempresa desde el núcleo**.

### Objetivos del Sistema

- Control operativo integral (compras, inventario, ventas, contabilidad)
- Gestión multiempresa y multimoneda
- Integración compras ? inventario ? ventas ? contabilidad
- Costeo de importaciones con prorrateo de gastos
- Escalabilidad modular
- Evolución futura hacia BI, automatización e integraciones externas

### Stack Tecnológico

| Componente | Tecnología |
|------------|-----------|
| **Runtime** | .NET 8.0 (LTS) |
| **Backend API** | ASP.NET Core Web API |
| **Frontend** | Blazor WebAssembly (SPA) |
| **ORM** | Entity Framework Core 8.0 |
| **Base de Datos** | SQL Server 2019+ |
| **Autenticación** | JWT Bearer Tokens |
| **Documentación API** | Swagger / OpenAPI (Swashbuckle) |
| **Versionamiento API** | Asp.Versioning (URL Segment + Header) |
| **Reportes** | EPPlus (exportación Excel) |
| **UI Framework** | Bootstrap 5 + Bootstrap Icons |

---

## 2. Arquitectura del Sistema

### 2.1 Clean Architecture

El proyecto implementa **Clean Architecture** con separación estricta de responsabilidades en capas concéntricas:

```
                    ???????????????????????????????????
                    ?          Presentación            ?
                    ?   API (Controllers) + Web (WASM) ?
                    ???????????????????????????????????
                                   ?
                    ???????????????????????????????????
                    ?         Aplicación               ?
                    ?   Services + Interfaces + DTOs   ?
                    ???????????????????????????????????
                                   ?
                    ???????????????????????????????????
                    ?           Dominio                ?
                    ?  Entities + Enums + Interfaces   ?
                    ???????????????????????????????????
                                   ?
                    ???????????????????????????????????
                    ?       Infraestructura            ?
                    ?  Persistence (EF) + External Svc ?
                    ???????????????????????????????????
```

### 2.2 Flujo de una Petición

```
Browser (Blazor WASM)
  ?
  ??? HttpService (ej: AsientoContableHttpService)
  ?     ??? HTTP GET/POST/PUT/DELETE ? API
  ?
  ?
API Backend
  ?
  ??? GlobalExceptionMiddleware     ? Captura excepciones
  ??? JWT Authentication            ? Valida token
  ??? TenantRequiredMiddleware      ? Valida EmpresaId en claims
  ??? Controller (V1)               ? Ruteo y validación HTTP
  ?     ??? IService.Method()       ? Lógica de negocio
  ?           ??? IRepository<T>    ? Acceso a datos
  ?                 ??? DbContext   ? EF Core + Tenant Filter
  ?                       ??? SQL Server
  ?
  ??? ApiResponse<T>               ? Respuesta estandarizada
```

### 2.3 Arquitectura Multi-Tenant

```
                    ????????????????????????????????
                    ?        JWT Token Claims       ?
                    ?  UserId, UserName, EmpresaId  ?
                    ????????????????????????????????
                                   ?
                    ????????????????????????????????
                    ?  TenantRequiredMiddleware     ?
                    ?  Valida EmpresaId en claims   ?
                    ????????????????????????????????
                                   ?
                    ????????????????????????????????
                    ?    ICurrentUserService        ?
                    ?  Expone: UserId, EmpresaId    ?
                    ????????????????????????????????
                                   ?
        ???????????????????????????????????????????????????????
        ?                          ?                          ?
        ?                          ?                          ?
 ????????????????    ?????????????????????    ????????????????????
 ?   Services   ?    ?   DbContext       ?    ?  Global Query    ?
 ? Filtran por  ?    ? Inyecta EmpresaId ?    ?  Filter          ?
 ?  EmpresaId   ?    ?  en interceptors  ?    ? WHERE EmpresaId= ?
 ????????????????    ?????????????????????    ????????????????????
```

**Todas las entidades** que heredan de `TenantEntity` tienen filtro automático por `EmpresaId` a nivel de DbContext.

---

## 3. Estructura de la Solución

### 3.1 Proyectos (8 proyectos + 1 tests)

```
AgoraHUB360-ERP/
??? AgoraHub360.ERP.sln
?
??? src/
?   ??? AgoraHub360.ERP.Domain/              ? Entidades, Enums, Interfaces base
?   ?   ??? Common/                          ? AuditableEntity, TenantEntity
?   ?   ??? Entities/                        ? 65 entidades en 8 dominios
?   ?   ??? Enums/                           ? TipoCuenta, NaturalezaCuenta, EstadoDocumento, TipoProducto
?   ?   ??? Interfaces/                      ? IRepository<T>, IUnitOfWork
?   ?
?   ??? AgoraHub360.ERP.Application/         ? Lógica de negocio
?   ?   ??? Common/                          ? Result<T>
?   ?   ??? Interfaces/                      ? 32 interfaces de servicio
?   ?   ??? Services/                        ? 32 implementaciones
?   ?   ??? DependencyInjection.cs           ? Registro IoC
?   ?
?   ??? AgoraHub360.ERP.Shared/              ? DTOs compartidos (API ? Web)
?   ?   ??? DTOs/                            ? ~70 DTOs agrupados por módulo
?   ?   ??? Constants/                       ? Roles
?   ?
?   ??? AgoraHub360.ERP.Persistence/         ? EF Core, Migraciones, Repositorios
?   ?   ??? Context/                         ? AgoraDbContext (multi-tenant)
?   ?   ??? Configurations/                  ? ~50 Fluent API configs
?   ?   ??? Interceptors/                    ? Auditoría + Versionado automático
?   ?   ??? Migrations/                      ? 15 migraciones
?   ?   ??? Repositories/                    ? Repository<T> genérico
?   ?   ??? Services/                        ? AuditLogService
?   ?   ??? DependencyInjection.cs
?   ?
?   ??? AgoraHub360.ERP.Infrastructure/      ? Servicios externos (futuro)
?   ?   ??? DependencyInjection.cs
?   ?
?   ??? AgoraHub360.ERP.Api/                 ? API REST Backend
?   ?   ??? Controllers/V1/                  ? 30 controllers
?   ?   ??? Middleware/                       ? GlobalException, TenantRequired
?   ?   ??? Auth/                            ? StubAuthHandler (dev)
?   ?   ??? Services/                        ? AuthService, CurrentUserService
?   ?   ??? Program.cs                       ? Entry point + pipeline
?   ?
?   ??? AgoraHub360.ERP.Web/                 ? Frontend Blazor WASM
?       ??? Layout/                          ? MainLayout, NavMenu, LoginLayout
?       ??? Pages/                           ? 37 páginas Razor
?       ??? Services/                        ? 34 HttpServices
?       ??? Shared/                          ? RedirectToLogin
?       ??? wwwroot/                         ? Estáticos + appsettings.json
?
??? tests/
?   ??? AgoraHub360.ERP.Tests/               ? Tests unitarios
?
??? docs/
    ??? DEPLOYMENT_GUIDE.md                  ? Guía de despliegue
    ??? COMMIT_CONVENTION.md                 ? Convención de commits
    ??? PROJECT_STATUS.md                    ? Este documento
```

### 3.2 Dependencias entre Proyectos

```
Domain ? (sin dependencias externas)
  ?
  ?
Application ? Domain
  ?
  ?
Shared ? (sin dependencias — solo DTOs puros)
  ?
  ?
Persistence ? Domain, Application
  ?
  ?
Infrastructure ? (reservado para servicios externos)
  ?
  ?
Api ? Application, Persistence, Infrastructure
  ?
Web ? Shared (solo DTOs, no necesita backend)
```

---

## 4. Módulos Funcionales

### 4.1 Resumen de Módulos

| Módulo | Schema BD | Estado | Descripción |
|--------|-----------|--------|-------------|
| **Core** | `core` | ? Completo | Empresas, Usuarios, Roles, Parámetros, Numeración, Auditoría |
| **MDM** | `mdm` | ? Completo | Productos, Categorías, Clientes, Proveedores, Almacenes, Atributos, Marcas, Fabricantes, Variantes, UOM, Códigos, Listas de Precios |
| **INV** | `inv` | ? Completo | Movimientos de Inventario, Stock por ubicación, Kardex |
| **CMP** | `cmp` | ? Completo | Órdenes de Compra, Recepciones, Importaciones con prorrateo |
| **ACC** | `acc` | ? Completo | Plan de Cuentas, Comprobantes Contables, Períodos, Plantillas, Estados Financieros |
| **PRC** | `prc` | ? Completo | Listas de Precios, Ítems de Precio |
| **CST** | `cst` | ?? Estructura | Reglas de Costeo, Perfiles de Landed Cost |
| **RUL** | `rul` | ?? Estructura | Industrias, Reglas por Industria |
| **VER** | `ver` | ? Completo | Versionado de entidades (historial de cambios) |
| **DOC** | `doc` | ?? Estructura | Documentos multimedia |
| **VTA** | — | ?? Pendiente | Ventas, Pedidos, Facturación |

### 4.2 Detalle por Módulo

#### Core — Sistema Base
- Gestión multi-empresa con asignación usuario ? empresa
- Roles y permisos (Admin, Gerente, Contador, Almacenero, Vendedor, Operador)
- Parámetros configurables por empresa (clave-valor)
- Numeración automática de documentos (configurable por tipo)
- Auditoría completa con log de operaciones
- Autenticación JWT con login por email/contraseña

#### MDM — Master Data Management
- **Productos**: Catálogo global + productos por empresa
- **Categorías**: Jerarquía multinivel (padre-hijo)
- **Clasificaciones**: Etiquetas cruzadas por producto
- **Atributos dinámicos**: Definiciones + opciones + valores
- **Variantes**: Producto ? Variantes con atributos
- **UOM**: Unidades de medida con factores de conversión
- **Códigos**: Múltiples códigos por producto (SKU, EAN, UPC)
- **Marcas y Fabricantes**: Catálogos independientes
- **Clientes y Proveedores**: Con datos fiscales (NIT, NIF)
- **Almacenes**: Con ubicaciones jerárquicas
- **Listas de Precios**: Múltiples listas con ítems y vigencia
- **Status de Producto**: Activo, Descontinuado, etc.

#### INV — Inventario
- Movimientos: Entrada, Salida, Transferencia, Ajuste
- Stock por producto × almacén × ubicación
- Kardex (trazabilidad de movimientos por producto)
- Integración automática con recepciones de compra

#### CMP — Compras
- **Órdenes de Compra**: Con líneas de detalle, estados (Borrador ? Aprobada ? Recibida)
- **Recepciones de Compra**: Vinculada a OC, genera movimiento de inventario automático
- **Hojas de Importación**: Gastos de importación con prorrateo automático por línea

#### ACC — Contabilidad
- **Plan de Cuentas**: Jerárquico multinivel (ej: 1.1.3.01)
- **Comprobantes Contables**: Ingreso / Egreso / Traspaso
  - Numeración por tipo y gestión (CI-0001, CE-0001, CT-0001)
  - Tipo de cambio (USD, UFV) con valor al momento del registro
  - Tipo de pago (Efectivo, Cheque, QR, Transferencia, S/D)
  - Validación de partida doble (Debe = Haber)
  - Estados: Borrador ? Contabilizado ? Anulado
  - Copiar comprobante existente
- **Períodos Contables**: Apertura/Cierre mensual
- **Plantillas de Contabilización**: Asientos automáticos por tipo de documento
- **Estados Financieros**: Balance General, Estado de Resultados, Sumas y Saldos, Libro Diario
- **Motor de Contabilización**: Genera asientos automáticos desde plantillas

---

## 5. Modelo de Datos — Entidades por Dominio

### 5.1 Core (6 entidades)

| Entidad | Hereda de | Descripción |
|---------|-----------|-------------|
| `Empresa` | `AuditableEntity` | Empresa del sistema |
| `Usuario` | `AuditableEntity` | Usuario con hash de contraseña |
| `UsuarioEmpresa` | — | Relación N:N usuario ? empresa con rol |
| `Rol` | `AuditableEntity` | Rol del sistema |
| `ParametroSistema` | `TenantEntity` | Configuración clave-valor por empresa |
| `NumeracionDocumento` | `TenantEntity` | Secuencias automáticas por tipo de doc |
| `AuditLog` | — | Log de auditoría |
| `Moneda` | `TenantEntity` | Monedas configurables |

### 5.2 MDM (22 entidades)

| Entidad | Descripción |
|---------|-------------|
| `Product` | Producto global (catálogo base) |
| `CompanyProduct` | Producto habilitado por empresa |
| `CompanyProductFeature` | Features activables por empresa |
| `ProductVariant` | Variante de producto |
| `VariantAttributeValue` | Valores de atributo por variante |
| `ProductCode` | Códigos alternativos (SKU, EAN) |
| `ProductUom` | UOM por producto con factores |
| `ProductAttribute` | Valor de atributo en producto |
| `AttributeDefinition` | Definición de atributo dinámico |
| `AttributeOption` | Opciones para atributos tipo Select |
| `Category` | Categoría jerárquica |
| `ProductCategory` | Relación N:N producto ? categoría |
| `ProductClassification` | Clasificación / etiqueta |
| `ProductClassificationLink` | Relación N:N producto ? clasificación |
| `Catalog` | Catálogo agrupador |
| `Brand` | Marca |
| `Manufacturer` | Fabricante |
| `Uom` | Unidad de medida base |
| `ProductStatus` | Estado del producto |
| `Cliente` | Cliente con datos fiscales |
| `Proveedor` | Proveedor con datos fiscales |
| `Almacen` | Almacén |
| `UbicacionAlmacen` | Ubicación dentro de almacén |

### 5.3 INV (2 entidades)

| Entidad | Descripción |
|---------|-------------|
| `MovimientoInventario` | Movimiento de stock (Entrada/Salida/Ajuste/Transferencia) |
| `StockProducto` | Stock actual por producto × almacén × ubicación |

### 5.4 CMP (6 entidades)

| Entidad | Descripción |
|---------|-------------|
| `OrdenCompra` | Cabecera de orden de compra |
| `OrdenCompraLinea` | Línea de detalle de OC |
| `RecepcionCompra` | Cabecera de recepción |
| `RecepcionCompraLinea` | Línea de recepción |
| `HojaImportacion` | Hoja de importación con prorrateo |
| `ImportacionLinea` | Línea de importación |
| `GastoImportacion` | Gasto prorrateado |

### 5.5 ACC (8 entidades)

| Entidad | Descripción |
|---------|-------------|
| `CuentaContable` | Cuenta del plan contable (jerárquica) |
| `AsientoContable` | Cabecera de comprobante contable |
| `AsientoContableLinea` | Línea Debe/Haber |
| `PeriodoContable` | Período mensual (Abierto/Cerrado) |
| `PlantillaContable` | Plantilla para asientos automáticos |
| `PlantillaContableLinea` | Línea de plantilla |
| `TipoComprobante` | Tipo: Ingreso, Egreso, Traspaso |
| `TipoCambio` | Tipo de cambio (USD, UFV) |
| `TipoPago` | Forma de pago (Efectivo, Cheque, QR, etc.) |

### 5.6 PRC (2 entidades)

| Entidad | Descripción |
|---------|-------------|
| `PriceList` | Lista de precios |
| `PriceListItem` | Ítem de precio por producto |

### 5.7 Soporte (5 entidades)

| Entidad | Dominio | Descripción |
|---------|---------|-------------|
| `CostingRule` | CST | Regla de costeo |
| `LandedCostProfile` | CST | Perfil de landed cost |
| `Industry` | RUL | Industria |
| `ProductIndustryRule` | RUL | Regla por industria |
| `EntityVersion` | VER | Historial de versiones |
| `Document` | DOC | Documento multimedia |
| `ProductDocument` | DOC | Relación producto ? documento |

### 5.8 Enumeraciones

| Enum | Valores |
|------|---------|
| `TipoCuenta` | Activo, Pasivo, Patrimonio, Ingreso, Gasto, Costo |
| `NaturalezaCuenta` | Deudora, Acreedora |
| `EstadoDocumento` | Borrador, Aprobado, Procesado, Anulado, Cerrado |
| `TipoProducto` | Simple, Configurable, Kit, Servicio |

### 5.9 Clases Base

```
AuditableEntity (abstracta)
??? FechaCreacion      : DateTime
??? CreadoPor          : string?
??? FechaModificacion  : DateTime?
??? ModificadoPor      : string?
??? Activo             : bool (soft delete)
    ?
    ??? TenantEntity (abstracta)
        ??? EmpresaId  : int (filtro multi-tenant automático)
```

---

## 6. Capa de Aplicación — Servicios e Interfaces

### 6.1 Interfaces de Servicio (32)

| Interface | Módulo | Descripción |
|-----------|--------|-------------|
| `IAuthService` | Core | Autenticación y generación JWT |
| `ICurrentUserService` | Core | Usuario actual (claims del JWT) |
| `IEmpresaService` | Core | CRUD de empresas |
| `IUsuarioService` | Core | CRUD de usuarios + asignación empresas |
| `IRolService` | Core | CRUD de roles |
| `IParametroSistemaService` | Core | Parámetros por empresa |
| `INumeracionDocumentoService` | Core | Secuencias automáticas |
| `IAuditLogService` | Core | Log de auditoría |
| `IProductService` | MDM | Productos globales |
| `ICompanyProductService` | MDM | Productos por empresa |
| `IProductVariantService` | MDM | Variantes |
| `IAttributeDefinitionService` | MDM | Atributos dinámicos |
| `ICatalogService` | MDM | Catálogos |
| `ICategoryService` | MDM | Categorías jerárquicas |
| `ICategoriaProductoService` | MDM | Categorías (legacy) |
| `IBrandService` | MDM | Marcas |
| `IManufacturerService` | MDM | Fabricantes |
| `IUomService` | MDM | Unidades de medida |
| `IProductUomService` | MDM | UOM por producto |
| `IProductCodeService` | MDM | Códigos de producto |
| `IPriceListService` | PRC | Listas de precios |
| `IProductoService` | MDM | Productos (legacy) |
| `IClienteService` | MDM | Clientes |
| `IProveedorService` | MDM | Proveedores |
| `IAlmacenService` | MDM | Almacenes |
| `IIndustryService` | RUL | Industrias |
| `IVersioningService` | VER | Versionado |
| `IMovimientoInventarioService` | INV | Inventario |
| `IOrdenCompraService` | CMP | Órdenes de compra |
| `IRecepcionCompraService` | CMP | Recepciones |
| `IHojaImportacionService` | CMP | Importaciones |
| `ICuentaContableService` | ACC | Plan de cuentas |
| `IAsientoContableService` | ACC | Comprobantes contables |
| `IPeriodoContableService` | ACC | Períodos contables |
| `IPlantillaContableService` | ACC | Plantillas de asientos |
| `IContabilizacionService` | ACC | Motor de contabilización automática |
| `IEstadoFinancieroService` | ACC | Estados financieros |

### 6.2 Patrón Result

Todos los servicios retornan `Result<T>` para comunicar éxito/error sin excepciones:

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    public static Result<T> Success(T value);
    public static Result<T> Failure(string error);
}
```

---

## 7. API REST — Endpoints

### 7.1 Controllers (30)

| Controller | Ruta Base | Módulo | Operaciones |
|------------|-----------|--------|-------------|
| `AuthController` | `/api/v1/auth` | Core | Login |
| `EmpresasController` | `/api/v1/empresas` | Core | CRUD + mis empresas |
| `UsuariosController` | `/api/v1/usuarios` | Core | CRUD + asignar empresa/rol |
| `RolesController` | `/api/v1/roles` | Core | CRUD |
| `ParametrosController` | `/api/v1/parametros` | Core | CRUD por empresa |
| `NumeracionesController` | `/api/v1/numeraciones` | Core | CRUD |
| `AuditLogsController` | `/api/v1/audit-logs` | Core | Consulta paginada |
| `DiagnosticsController` | `/api/v1/diagnostics` | Core | Info del sistema |
| `MdmCatalogsController` | `/api/v1/mdm/catalogs` | MDM | CRUD catálogos |
| `MdmCategoriesController` | `/api/v1/mdm/categories` | MDM | CRUD categorías |
| `MdmProductsController` | `/api/v1/mdm/products` | MDM | CRUD productos globales |
| `MdmCompanyProductsController` | `/api/v1/mdm/company-products` | MDM | CRUD productos empresa |
| `MdmVariantsController` | `/api/v1/mdm/variants` | MDM | CRUD variantes |
| `MdmAttributesController` | `/api/v1/mdm/attributes` | MDM | CRUD atributos |
| `MdmBrandsController` | `/api/v1/mdm/brands` | MDM | CRUD marcas |
| `MdmManufacturersController` | `/api/v1/mdm/manufacturers` | MDM | CRUD fabricantes |
| `MdmProductUomsController` | `/api/v1/mdm/product-uoms` | MDM | CRUD UOM producto |
| `MdmProductCodesController` | `/api/v1/mdm/product-codes` | MDM | CRUD códigos |
| `PrcPriceListsController` | `/api/v1/prc/price-lists` | PRC | CRUD listas precios |
| `RulIndustriesController` | `/api/v1/rul/industries` | RUL | CRUD industrias |
| `CategoriasProductoController` | `/api/v1/categorias-producto` | MDM | Legacy |
| `ProductosController` | `/api/v1/productos` | MDM | Legacy |
| `ClientesController` | `/api/v1/clientes` | MDM | CRUD clientes |
| `ProveedoresController` | `/api/v1/proveedores` | MDM | CRUD proveedores |
| `AlmacenesController` | `/api/v1/almacenes` | MDM | CRUD almacenes |
| `UnidadesMedidaController` | `/api/v1/unidades-medida` | MDM | CRUD UOM base |
| `MovimientosInventarioController` | `/api/v1/inventario` | INV | CRUD + stock + kardex |
| `OrdenesCompraController` | `/api/v1/compras/ordenes` | CMP | CRUD + aprobar + recibir |
| `RecepcionesCompraController` | `/api/v1/compras/recepciones` | CMP | CRUD + confirmar |
| `ImportacionesController` | `/api/v1/compras/importaciones` | CMP | CRUD + liquidar + prorrateo |
| `CuentasContablesController` | `/api/v1/contabilidad/cuentas` | ACC | CRUD + seed plan cuentas |
| `AsientosContablesController` | `/api/v1/contabilidad/asientos` | ACC | CRUD + contabilizar + anular + copiar + catálogos |
| `PeriodosContablesController` | `/api/v1/contabilidad/periodos` | ACC | CRUD + abrir + cerrar |
| `PlantillasContablesController` | `/api/v1/contabilidad/plantillas` | ACC | CRUD + seed |
| `EstadosFinancierosController` | `/api/v1/contabilidad/estados-financieros` | ACC | Balance, EERR, Sumas, Diario |

### 7.2 Respuesta Estandarizada

Todas las respuestas siguen el formato `ApiResponse<T>`:

```json
{
  "success": true,
  "message": "Operación exitosa.",
  "data": { ... }
}
```

### 7.3 Health Check

```
GET /health ? 200 Healthy | 503 Unhealthy
```

---

## 8. Frontend — Páginas Blazor

### 8.1 Páginas (37 total)

| Módulo | Página | Ruta | Funcionalidad |
|--------|--------|------|---------------|
| **General** | Dashboard | `/` | Panel principal |
| **General** | Login | `/login` | Autenticación |
| **MDM** | Clientes | `/mdm/clientes` | CRUD clientes |
| **MDM** | Proveedores | `/mdm/proveedores` | CRUD proveedores |
| **MDM** | Productos | `/mdm/productos` | CRUD productos legacy |
| **MDM** | Categorías | `/mdm/categorias` | CRUD categorías |
| **MDM** | Unid. Medida | `/mdm/unidades-medida` | CRUD UOM |
| **MDM** | Almacenes (MDM) | `/mdm/almacenes` | CRUD almacenes |
| **MDM** | Catálogo Global | `/mdm/productos-globales` | Productos globales MDM |
| **MDM** | Prod. Empresa | `/mdm/productos-empresa` | Productos por empresa |
| **MDM** | Variantes | `/mdm/variantes` | Variantes de producto |
| **MDM** | Atributos | `/mdm/atributos` | Atributos dinámicos |
| **MDM** | Marcas | `/mdm/marcas` | CRUD marcas |
| **MDM** | Fabricantes | `/mdm/fabricantes` | CRUD fabricantes |
| **MDM** | Listas Precios | `/mdm/listas-precios` | Listas de precios |
| **INV** | Almacenes | `/inventario/almacenes` | Vista inventario |
| **INV** | Movimientos | `/inventario/movimientos` | Entradas/Salidas |
| **INV** | Kardex | `/inventario/kardex` | Trazabilidad |
| **INV** | Stock | `/inventario/stock` | Stock actual |
| **CMP** | Órdenes | `/compras/ordenes` | Órdenes de compra |
| **CMP** | Recepciones | `/compras/recepciones` | Recepciones |
| **CMP** | Importaciones | `/compras/importaciones` | Hojas de importación |
| **ACC** | Plan Cuentas | `/contabilidad/plan-cuentas` | Plan de cuentas jerárquico |
| **ACC** | Comprobantes | `/contabilidad/asientos` | Registro contable completo |
| **ACC** | Períodos | `/contabilidad/periodos` | Períodos contables |
| **ACC** | Plantillas | `/contabilidad/plantillas` | Plantillas de asientos |
| **ACC** | Balance Gral. | `/contabilidad/balance-general` | Balance general |
| **ACC** | Estado Resultados | `/contabilidad/estado-resultados` | Estado de resultados |
| **ACC** | Sumas y Saldos | `/contabilidad/sumas-saldos` | Balance de comprobación |
| **ACC** | Libro Diario | `/contabilidad/libro-diario` | Libro diario |
| **VTA** | Pedidos | `/ventas/pedidos` | ?? Pendiente |
| **VTA** | Facturas | `/ventas/facturas` | ?? Pendiente |
| **Config** | Empresas | `/config/empresas` | Gestión empresas |
| **Config** | Usuarios | `/config/usuarios` | Gestión usuarios |
| **Config** | Roles | `/config/roles` | Gestión roles |
| **Config** | Parámetros | `/config/parametros-numeracion` | Parámetros y numeración |
| **Config** | Auditoría | `/config/auditoria` | Log de auditoría |

### 8.2 HTTP Services (34)

Cada servicio encapsula las llamadas HTTP a la API con tipado fuerte:

| Service | API Base |
|---------|----------|
| `AuthHttpService` | `/api/v1/auth` |
| `EmpresaHttpService` | `/api/v1/empresas` |
| `UsuarioHttpService` | `/api/v1/usuarios` |
| `RolHttpService` | `/api/v1/roles` |
| `ParametroHttpService` | `/api/v1/parametros` |
| `NumeracionHttpService` | `/api/v1/numeraciones` |
| `AuditLogHttpService` | `/api/v1/audit-logs` |
| `CategoriaProductoHttpService` | `/api/v1/categorias-producto` |
| `CatalogoHttpService` | `/api/v1/mdm/catalogs` |
| `UnidadMedidaHttpService` | `/api/v1/unidades-medida` |
| `ProductoHttpService` | `/api/v1/productos` |
| `ClienteHttpService` | `/api/v1/clientes` |
| `ProveedorHttpService` | `/api/v1/proveedores` |
| `AlmacenHttpService` | `/api/v1/almacenes` |
| `MdmProductoGlobalHttpService` | `/api/v1/mdm/products` |
| `MdmCompanyProductHttpService` | `/api/v1/mdm/company-products` |
| `MdmVariantHttpService` | `/api/v1/mdm/variants` |
| `MdmAttributeHttpService` | `/api/v1/mdm/attributes` |
| `BrandHttpService` | `/api/v1/mdm/brands` |
| `ManufacturerHttpService` | `/api/v1/mdm/manufacturers` |
| `ProductUomHttpService` | `/api/v1/mdm/product-uoms` |
| `ProductCodeHttpService` | `/api/v1/mdm/product-codes` |
| `PriceListHttpService` | `/api/v1/prc/price-lists` |
| `MovimientoInventarioHttpService` | `/api/v1/inventario` |
| `OrdenCompraHttpService` | `/api/v1/compras/ordenes` |
| `RecepcionCompraHttpService` | `/api/v1/compras/recepciones` |
| `HojaImportacionHttpService` | `/api/v1/compras/importaciones` |
| `CuentaContableHttpService` | `/api/v1/contabilidad/cuentas` |
| `AsientoContableHttpService` | `/api/v1/contabilidad/asientos` |
| `PeriodoContableHttpService` | `/api/v1/contabilidad/periodos` |
| `PlantillaContableHttpService` | `/api/v1/contabilidad/plantillas` |
| `EstadoFinancieroHttpService` | `/api/v1/contabilidad/estados-financieros` |
| `EmpresaStateService` | (estado local — empresa activa) |
| `JwtAuthStateProvider` | (gestión de autenticación WASM) |

### 8.3 Layout del Frontend

```
???????????????????????????????????????????????????????????????
? [A] AgoraHub360  v1.0.0      ?  [Empresa]  [Usuario ?]    ? ? Topbar
???????????????????????????????????????????????????????????????
? Dashboard      ?                                            ?
? ? Datos Maes.  ?                                            ?
?   Clientes     ?              Área de Contenido             ?
?   Proveedores  ?                  (@Body)                   ?
?   Productos    ?                                            ?
?   ...          ?                                            ?
? ? Inventario   ?                                            ?
? ? Compras      ?                                            ?
? ? Contabilidad ?                                            ?
? ? Reportes     ?                                            ?
? ? Ventas       ?                                            ?
? ? Sistema      ?                                            ?
?                ??????????????????????????????????????????????
?                ? AgoraHub360 ERP v1.0.0 © 2026 — Gestión   ? ? Footer
???????????????????????????????????????????????????????????????
```

---

## 9. Persistencia — Base de Datos

### 9.1 Motor

- **SQL Server** 2019+ (compatible con Express, Standard, Developer, Azure SQL)
- **EF Core 8.0** con Code-First y migraciones
- **Soft Delete** automático (`Activo = false`, nunca DELETE físico)

### 9.2 Schemas de Base de Datos

| Schema | Módulo | Tablas |
|--------|--------|--------|
| `core` | Core | Empresas, Usuarios, UsuarioEmpresas, Roles, ParametrosSistema, NumeracionesDocumento, AuditLogs, Monedas |
| `mdm` | MDM | Products, CompanyProducts, ProductVariants, Categories, ProductCategories, Catalogs, Brands, Manufacturers, Uoms, ProductUoms, ProductCodes, ProductStatuses, AttributeDefinitions, AttributeOptions, ProductAttributes, VariantAttributeValues, ProductClassifications, ProductClassificationLinks, CompanyProductFeatures, Clientes, Proveedores, Almacenes, UbicacionesAlmacen |
| `inv` | INV | MovimientosInventario, StockProductos |
| `cmp` | CMP | OrdenesCompra, OrdenCompraLineas, RecepcionesCompra, RecepcionCompraLineas, HojasImportacion, GastosImportacion, ImportacionLineas |
| `acc` | ACC | CuentasContables, AsientosContables, AsientoContableLineas, PeriodosContables, PlantillasContables, PlantillaContableLineas, TiposComprobante, TiposCambio, TiposPago |
| `prc` | PRC | PriceLists, PriceListItems |
| `cst` | CST | CostingRules, LandedCostProfiles |
| `rul` | RUL | Industries, ProductIndustryRules |
| `ver` | VER | EntityVersions |
| `doc` | DOC | Documents, ProductDocuments |

### 9.3 Interceptors

| Interceptor | Función |
|-------------|---------|
| `AuditableEntityInterceptor` | Establece automáticamente `FechaCreacion`, `CreadoPor`, `FechaModificacion`, `ModificadoPor` |
| `EntityVersioningInterceptor` | Crea snapshot JSON en `EntityVersions` al modificar entidades versionadas |

### 9.4 Filtro Multi-Tenant Global

Aplicado automáticamente en `OnModelCreating` a **todas** las entidades que heredan `TenantEntity`:

```csharp
modelBuilder.Entity<T>().HasQueryFilter(e => _empresaId == null || e.EmpresaId == _empresaId);
```

---

## 10. Patrones y Convenciones

### 10.1 Patrones Implementados

| Patrón | Uso |
|--------|-----|
| **Clean Architecture** | Separación en capas Domain ? Application ? Infrastructure ? Presentation |
| **Repository Pattern** | `IRepository<T>` genérico con `Repository<T>` en Persistence |
| **Unit of Work** | `IUnitOfWork` ? `SaveChangesAsync()` transaccional |
| **Result Pattern** | `Result<T>` para comunicar éxito/error sin excepciones |
| **CQRS Lite** | Separación implícita entre lecturas (Get/Find) y escrituras (Create/Update) |
| **Multi-Tenant** | Filtro automático por `EmpresaId` a nivel de DbContext |
| **Soft Delete** | `Activo = false` en lugar de DELETE |
| **Audit Trail** | Interceptors automáticos para campos de auditoría |
| **API Versioning** | URL segment (`/api/v1/`) + Header (`X-Api-Version`) |
| **DTO Pattern** | DTOs en proyecto Shared (compartido entre API y Web) |

### 10.2 Convenciones de Código

| Aspecto | Convención |
|---------|-----------|
| **Idioma código** | Inglés para nombres técnicos, español para dominio de negocio |
| **Namespaces** | `AgoraHub360.ERP.{Capa}.{Módulo}` |
| **Entidades** | Nombre singular en español o inglés (ej: `AsientoContable`, `Product`) |
| **DTOs** | `{Entidad}Dto`, `Create{Entidad}Dto`, `Update{Entidad}Dto` |
| **Servicios** | `I{Entidad}Service` ? `{Entidad}Service` |
| **Controllers** | Plural (ej: `AsientosContablesController`) |
| **HttpServices** | `{Entidad}HttpService` (en Web) |
| **Tablas BD** | Plural en español (ej: `AsientosContables`) |
| **Schemas BD** | Abreviatura de 3 letras (ej: `acc`, `cmp`, `inv`) |

### 10.3 Convención de Commits

```
<tipo>(<alcance>): <descripción>

Tipos: feat, fix, refactor, docs, style, test, chore
Alcance: core, mdm, inv, cmp, acc, web, api, persistence
```

---

## 11. Seguridad y Autenticación

### 11.1 Flujo de Autenticación

```
1. POST /api/v1/auth/login { email, password }
2. AuthService valida credenciales (BCrypt hash)
3. Genera JWT con claims: UserId, UserName, Email, Role, EmpresaId
4. Frontend almacena token en localStorage
5. JwtAuthStateProvider parsea token ? AuthenticationState
6. HttpClient inyecta header Authorization: Bearer {token}
7. API valida JWT ? extrae claims ? ICurrentUserService
```

### 11.2 Claims del Token

| Claim | Descripción |
|-------|-------------|
| `NameIdentifier` | ID del usuario |
| `Name` | Nombre de usuario |
| `Email` | Email |
| `Role` | Rol asignado |
| `EmpresaId` | Empresa activa (inyectada al seleccionar) |

### 11.3 Middleware de Seguridad

```
Request ? GlobalExceptionMiddleware
        ? UseAuthentication (JWT)
        ? UseAuthorization
        ? TenantRequiredMiddleware
        ? Controller
```

### 11.4 CORS

Configurado para permitir solo la URL del frontend (`BlazorBaseUrl`).

---

## 12. Estado de Migraciones

### 15 migraciones aplicadas (al 26/02/2026)

| # | Nombre | Fecha | Módulo | Descripción |
|---|--------|-------|--------|-------------|
| 1 | `BaseCore` | 17/02 | Core | Empresas, Usuarios, Monedas |
| 2 | `AddRolesTable` | 17/02 | Core | Tabla de Roles |
| 3 | `AddAuditLogTable` | 17/02 | Core | Log de auditoría |
| 4 | `AddParametroSistemaNumeracionDocumento` | 17/02 | Core | Parámetros y numeración |
| 5 | `AddDefaultAdminUser` | 17/02 | Core | Seed usuario admin |
| 6 | `AddMDMEntities` | 17/02 | MDM | Entidades maestras MDM |
| 7 | `MDM_Enhanced` | 17/02 | MDM | Mejoras MDM |
| 8 | `MDM_Refinements` | 18/02 | MDM | Refinamientos MDM |
| 9 | `INV_MovimientosInventario` | 19/02 | INV | Inventario y Stock |
| 10 | `SeedProductStatus` | 23/02 | MDM | Seed estados de producto |
| 11 | `SeedDefaultCatalog` | 23/02 | MDM | Seed catálogo por defecto |
| 12 | `CMP_OrdenesCompra` | 26/02 | CMP | Órdenes de compra |
| 13 | `CMP_RecepcionesCompra` | 26/02 | CMP | Recepciones |
| 14 | `CMP_Importaciones` | 26/02 | CMP | Importaciones |
| 15 | `ACC_CuentasContables` | 26/02 | ACC | Plan de cuentas |
| 16 | `ACC_AsientosContables` | 26/02 | ACC | Asientos contables |
| 17 | `ACC_PeriodosContables` | 26/02 | ACC | Períodos |
| 18 | `ACC_PlantillasContables` | 26/02 | ACC | Plantillas |
| 19 | `ACC_ComprobantesContables` | 26/02 | ACC | Tipos comprobante, cambio, pago |

---

## 13. Dependencias y Paquetes NuGet

### API Backend

| Paquete | Versión | Uso |
|---------|---------|-----|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.12 | Autenticación JWT |
| `Asp.Versioning.Mvc` | 8.1.0 | Versionado de API |
| `Asp.Versioning.Mvc.ApiExplorer` | 8.1.0 | Swagger + versiones |
| `EPPlus` | 7.5.2 | Exportación Excel |
| `Microsoft.AspNetCore.OpenApi` | 8.0.24 | OpenAPI |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.12 | Migraciones |
| `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` | 8.0.12 | Health checks |
| `Swashbuckle.AspNetCore` | 6.6.2 | Swagger UI |

### Persistence

| Paquete | Versión | Uso |
|---------|---------|-----|
| `Microsoft.EntityFrameworkCore` | 8.0.12 | ORM |
| `Microsoft.EntityFrameworkCore.SqlServer` | 8.0.12 | Provider SQL Server |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.12 | Migraciones |

### Frontend Web

| Paquete | Versión | Uso |
|---------|---------|-----|
| `Microsoft.AspNetCore.Components.Authorization` | 8.0.24 | Auth en Blazor |
| `Microsoft.AspNetCore.Components.WebAssembly` | 8.0.24 | Blazor WASM |
| `Microsoft.AspNetCore.Components.WebAssembly.DevServer` | 8.0.24 | Dev server |

---

## 14. Métricas del Proyecto

### Conteo de Artefactos

| Categoría | Cantidad |
|-----------|----------|
| **Proyectos en solución** | 8 + 1 tests |
| **Entidades de dominio** | ~65 |
| **Enumeraciones** | 4 (TipoCuenta, NaturalezaCuenta, EstadoDocumento, TipoProducto) |
| **Interfaces de servicio** | 32 |
| **Servicios de aplicación** | 32 |
| **Controllers API** | 30 |
| **Endpoints REST** | ~150+ |
| **Páginas Blazor** | 37 |
| **HTTP Services (Web)** | 34 |
| **DTOs** | ~70 |
| **Configuraciones EF** | ~50 |
| **Migraciones** | 19 |
| **Schemas de BD** | 10 |
| **Tablas en BD** | ~55 |

### Líneas de Código Estimadas

| Capa | Archivos .cs/.razor | LOC aprox. |
|------|---------------------|------------|
| Domain | ~65 | ~2,500 |
| Application | ~65 | ~8,000 |
| Shared | ~70 | ~2,000 |
| Persistence | ~100 | ~12,000 |
| Api | ~35 | ~3,000 |
| Web | ~75 | ~10,000 |
| **Total** | **~410** | **~37,500** |

---

## 15. Roadmap y Módulos Pendientes

### Fase Actual (v1.0.0) — Completada

- [x] Core: Multi-empresa, Usuarios, Roles, Auditoría
- [x] MDM: Productos, Categorías, Clientes, Proveedores, Almacenes
- [x] MDM Avanzado: Variantes, Atributos, Marcas, UOM, Precios
- [x] Inventario: Movimientos, Stock, Kardex
- [x] Compras: OC, Recepciones, Importaciones con prorrateo
- [x] Contabilidad: Plan de Cuentas, Comprobantes, Períodos, Plantillas, EEFF
- [x] Despliegue: Documentación, Health Checks

### Fase 2 (v1.1.0) — Próxima

- [ ] Ventas: Pedidos, Facturas, Notas de crédito/débito
- [ ] Facturación electrónica (SIAT Bolivia)
- [ ] Cuentas por Cobrar
- [ ] Cuentas por Pagar
- [ ] Reportes PDF (estados financieros, facturas)

### Fase 3 (v2.0.0) — Futura

- [ ] Costeo real de importaciones (Landed Cost completo)
- [ ] Reglas de negocio por industria
- [ ] Documentos multimedia por producto
- [ ] Dashboard con KPIs y gráficos
- [ ] Notificaciones en tiempo real (SignalR)
- [ ] Exportación masiva a Excel mejorada
- [ ] Multi-idioma (i18n)
- [ ] App móvil (MAUI/Blazor Hybrid)
- [ ] Integración con servicios externos (bancos, SIN)

### Fase 4 (v3.0.0) — Estratégica

- [ ] Business Intelligence (reportes avanzados, cubos OLAP)
- [ ] Automatización de procesos (workflows)
- [ ] API pública para integraciones
- [ ] Modelo SaaS multi-tenant con aislamiento por BD
- [ ] Migración a Azure (App Service + Azure SQL + Blob Storage)

---

> **Documento generado:** Febrero 2026  
> **Desarrollado por:** Ágora HUB 360 — Dirección Ágora Tech  
> **Contacto:** soporte@agorahub360.com
