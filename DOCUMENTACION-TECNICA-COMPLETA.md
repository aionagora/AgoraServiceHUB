# ?? DOCUMENTACIÓN TÉCNICA COMPLETA - AGORAHUB360 ERP

## ?? Índice

1. [Información General del Proyecto](#información-general-del-proyecto)
2. [Arquitectura del Sistema](#arquitectura-del-sistema)
3. [Estructura de Capas (Clean Architecture)](#estructura-de-capas-clean-architecture)
4. [Módulos Implementados](#módulos-implementados)
5. [Tecnologías y Frameworks](#tecnologías-y-frameworks)
6. [Patrones de Diseño](#patrones-de-diseño)
7. [Entidades del Dominio](#entidades-del-dominio)
8. [Sistema Multi-Tenant](#sistema-multi-tenant)
9. [Auditoría Automática](#auditoría-automática)
10. [Autenticación y Autorización](#autenticación-y-autorización)
11. [API REST](#api-rest)
12. [Frontend Blazor WebAssembly](#frontend-blazor-webassembly)
13. [Base de Datos](#base-de-datos)
14. [Configuración y Deployment](#configuración-y-deployment)
15. [Scripts y Herramientas](#scripts-y-herramientas)
16. [Testing](#testing)
17. [Convenciones de Código](#convenciones-de-código)
18. [Estado Actual del Desarrollo](#estado-actual-del-desarrollo)
19. [Roadmap y Próximas Funcionalidades](#roadmap-y-próximas-funcionalidades)

---

## ?? Información General del Proyecto

### Descripción
**AgoraHub360 ERP** es un sistema ERP (Enterprise Resource Planning) modular empresarial desarrollado bajo el **Modelo Ágora (MAPE)**, diseñado específicamente para entornos mixtos de importación y comercialización.

### Características Principales
- ? **Multi-empresa (Multi-tenant)** desde el núcleo
- ? **Clean Architecture** con separación clara de responsabilidades
- ? **Auditoría automática** de todas las operaciones
- ? **Multimoneda** con soporte BOB, USD, EUR
- ? **RBAC** (Role-Based Access Control)
- ? **API REST** con versionado
- ? **Frontend moderno** con Blazor WebAssembly
- ? **Entity Framework Core** con SQL Server
- ? **JWT Authentication**

### Repositorio
- **Git:** https://github.com/abelcalvimontes/AgoraHUB360-ERP
- **Branch actual:** `GestionProductos`
- **Ubicación:** `D:\AgoraCORE\AgoraHUB360-ERP`

---

## ??? Arquitectura del Sistema

### Tipo de Arquitectura
**Clean Architecture (Arquitectura Limpia)** con separación en capas concéntricas:

```
???????????????????????????????????????????????????????????
?                    PRESENTATION                         ?
?           (Blazor WebAssembly + API)                    ?
?  ????????????????????????????????????????????????????? ?
?  ?              APPLICATION                          ? ?
?  ?         (Business Logic / Services)               ? ?
?  ?  ??????????????????????????????????????????????? ? ?
?  ?  ?           DOMAIN                            ? ? ?
?  ?  ?    (Entities, Enums, Interfaces)            ? ? ?
?  ?  ??????????????????????????????????????????????? ? ?
?  ????????????????????????????????????????????????????? ?
???????????????????????????????????????????????????????????
         ?                        ?
         ?                        ?
   ??????????????         ?????????????????
   ?PERSISTENCE ?         ?INFRASTRUCTURE ?
   ?(EF Core)   ?         ?(External)     ?
   ??????????????         ?????????????????
```

### Principios Aplicados
- **SOLID Principles**
- **DRY** (Don't Repeat Yourself)
- **Separation of Concerns**
- **Dependency Inversion**
- **Repository Pattern**
- **Unit of Work Pattern**

---

## ?? Estructura de Capas (Clean Architecture)

### 1. **Domain Layer** (`AgoraHub360.ERP.Domain`)
**Responsabilidad:** Entidades del negocio, reglas de dominio, interfaces

```
src/AgoraHub360.ERP.Domain/
??? Common/
?   ??? AuditableEntity.cs          // Base para auditoría
?   ??? TenantEntity.cs              // Base para multi-tenant
??? Entities/
?   ??? Core/                        // Entidades del núcleo
?   ?   ??? Empresa.cs
?   ?   ??? Usuario.cs
?   ?   ??? UsuarioEmpresa.cs
?   ?   ??? Rol.cs
?   ?   ??? Moneda.cs
?   ?   ??? AuditLog.cs
?   ?   ??? ParametroSistema.cs
?   ?   ??? NumeracionDocumento.cs
?   ??? MDM/                         // Master Data Management
?       ??? CategoriaProducto.cs
?       ??? UnidadMedida.cs
?       ??? Cliente.cs
?       ??? Proveedor.cs
?       ??? Almacen.cs
?       ??? UbicacionAlmacen.cs
?       ??? Producto.cs
??? Enums/
?   ??? TipoProducto.cs
?   ??? EstadoDocumento.cs
??? Interfaces/
    ??? IRepository.cs
    ??? IUnitOfWork.cs
```

**Características:**
- ? **NO depende de ninguna otra capa**
- ? Define contratos (interfaces)
- ? Contiene lógica de negocio pura
- ? Entidades con anotaciones mínimas

---

### 2. **Application Layer** (`AgoraHub360.ERP.Application`)
**Responsabilidad:** Casos de uso, servicios de aplicación, DTOs

```
src/AgoraHub360.ERP.Application/
??? Common/
?   ??? Result.cs                    // Patrón Result para operaciones
??? Interfaces/
?   ??? ICurrentUserService.cs
?   ??? IAuthService.cs
?   ??? IEmpresaService.cs
?   ??? IUsuarioService.cs
?   ??? IRolService.cs
?   ??? IParametroService.cs
?   ??? INumeracionService.cs
?   ??? IAuditLogService.cs
?   ??? ICategoriaProductoService.cs
?   ??? IUnidadMedidaService.cs
?   ??? IClienteService.cs
?   ??? IProveedorService.cs
?   ??? IAlmacenService.cs
?   ??? IProductoService.cs
??? Services/
    ??? EmpresaService.cs
    ??? UsuarioService.cs
    ??? RolService.cs
    ??? ParametroService.cs
    ??? NumeracionDocumentoService.cs
    ??? AuditLogService.cs
    ??? CategoriaProductoService.cs
    ??? UnidadMedidaService.cs
    ??? ClienteService.cs
    ??? ProveedorService.cs
    ??? AlmacenService.cs
    ??? ProductoService.cs
```

**Características:**
- ? Implementa casos de uso del negocio
- ? Orquesta flujos entre dominio y persistencia
- ? Valida reglas de negocio
- ? Retorna `Result<T>` para manejo de errores
- ? **Multi-tenant aware** en todos los servicios

---

### 3. **Persistence Layer** (`AgoraHub360.ERP.Persistence`)
**Responsabilidad:** Acceso a datos, Entity Framework Core

```
src/AgoraHub360.ERP.Persistence/
??? Context/
?   ??? AgoraDbContext.cs
?   ??? AgoraDbContextFactory.cs     // Para migraciones
??? Configurations/                   // Fluent API
?   ??? EmpresaConfiguration.cs
?   ??? UsuarioConfiguration.cs
?   ??? ProductoConfiguration.cs
?   ??? ...
??? Interceptors/
?   ??? AuditableEntityInterceptor.cs // Auditoría automática
??? Migrations/
?   ??? [EF Core Migrations]
??? Repositories/
    ??? Repository.cs                 // Implementación genérica
    ??? UnitOfWork.cs
```

**Características:**
- ? Entity Framework Core 8.0
- ? Fluent API para configuraciones
- ? Interceptors para auditoría automática
- ? Migraciones Code-First
- ? Repository Pattern + Unit of Work

---

### 4. **Infrastructure Layer** (`AgoraHub360.ERP.Infrastructure`)
**Responsabilidad:** Servicios externos, implementaciones de infraestructura

```
src/AgoraHub360.ERP.Infrastructure/
??? Services/
?   ??? CurrentUserService.cs        // Obtiene contexto del usuario
??? [Futuras integraciones externas]
```

---

### 5. **API Layer** (`AgoraHub360.ERP.Api`)
**Responsabilidad:** Endpoints REST, autenticación, middleware

```
src/AgoraHub360.ERP.Api/
??? Controllers/V1/
?   ??? AuthController.cs
?   ??? DiagnosticsController.cs
?   ??? EmpresasController.cs
?   ??? UsuariosController.cs
?   ??? RolesController.cs
?   ??? ParametrosController.cs
?   ??? NumeracionesController.cs
?   ??? AuditLogsController.cs
?   ??? CategoriasController.cs
?   ??? UnidadesMedidaController.cs
?   ??? ClientesController.cs
?   ??? ProveedoresController.cs
?   ??? AlmacenesController.cs
?   ??? ProductosController.cs
??? Middleware/
?   ??? TenantRequiredMiddleware.cs  // Validación multi-tenant
??? Services/
?   ??? AuthService.cs               // JWT generation
??? Program.cs                        // Configuración DI
```

**Características:**
- ? API versionada (`/api/v1/...`)
- ? JWT Authentication
- ? Swagger/OpenAPI
- ? CORS configurado
- ? Middleware de validación tenant
- ? Authorization por roles

---

### 6. **Shared Layer** (`AgoraHub360.ERP.Shared`)
**Responsabilidad:** DTOs compartidos entre API y Web

```
src/AgoraHub360.ERP.Shared/
??? Constants/
?   ??? Roles.cs                     // Constantes de roles
??? DTOs/
    ??? ApiResponse.cs
    ??? LoginRequestDto.cs
    ??? AuthResponseDto.cs
    ??? PaginatedResultDto.cs
    ??? Empresa/
    ??? Usuario/
    ??? Rol/
    ??? Parametro/
    ??? Numeracion/
    ??? AuditLog/
    ??? CategoriaProducto/
    ??? UnidadMedida/
    ??? Cliente/
    ??? Proveedor/
    ??? Almacen/
    ??? Producto/
```

---

### 7. **Web Layer** (`AgoraHub360.ERP.Web`)
**Responsabilidad:** Frontend Blazor WebAssembly

```
src/AgoraHub360.ERP.Web/
??? Pages/
?   ??? Login.razor
?   ??? Index.razor
?   ??? Sistema/
?   ?   ??? Empresas.razor
?   ?   ??? Usuarios.razor
?   ?   ??? Roles.razor
?   ?   ??? AuditLogs.razor
?   ??? Config/
?   ?   ??? ParametrosNumeracion.razor
?   ?   ??? ...
?   ??? MDM/
?       ??? CategoriaProducto.razor
?       ??? UnidadesMedida.razor
?       ??? Clientes.razor
?       ??? Proveedores.razor
?       ??? Almacenes.razor
?       ??? Productos.razor
??? Layout/
?   ??? MainLayout.razor
?   ??? NavMenu.razor
?   ??? Header.razor
?   ??? LoginLayout.razor
??? Services/
?   ??? AuthHttpService.cs
?   ??? EmpresaHttpService.cs
?   ??? [Todos los servicios HTTP]
?   ??? JwtAuthStateProvider.cs
??? wwwroot/
?   ??? css/
?   ??? index.html
??? Program.cs
```

---

### 8. **Tests Layer** (`AgoraHub360.ERP.Tests`)
**Responsabilidad:** Unit Tests, Integration Tests

```
tests/AgoraHub360.ERP.Tests/
??? Domain/
?   ??? AuditableEntityTests.cs
?   ??? TenantEntityTests.cs
?   ??? EmpresaTests.cs
??? Application/
    ??? RolServiceTests.cs
    ??? NumeracionDocumentoServiceTests.cs
```

---

## ?? Módulos Implementados

### **1. CORE - Módulos Centrales** ? 100%

#### 1.1 **Seguridad y Autenticación**
- ? Login con JWT
- ? Gestión de Usuarios
- ? Gestión de Roles (Admin, Manager, User, Viewer)
- ? RBAC (Role-Based Access Control)
- ? Usuario-Empresa (Many-to-Many)

#### 1.2 **Multi-Empresa (Multi-Tenant)**
- ? Gestión de Empresas
- ? Contexto de empresa activa por usuario
- ? Aislamiento de datos por empresa
- ? Middleware de validación tenant

#### 1.3 **Multimoneda**
- ? Soporte BOB, USD, EUR
- ? Configuración de moneda base por empresa
- ? Seed data con monedas principales

#### 1.4 **Parámetros del Sistema**
- ? Configuración clave-valor por empresa
- ? Categorización de parámetros
- ? Tipos de datos (String, Integer, Decimal, Boolean, Select)
- ? Interfaz inline edit

#### 1.5 **Numeración de Documentos**
- ? Series de numeración por tipo de documento
- ? Formato personalizable (Prefijo + Número + Dígitos)
- ? Preview en tiempo real
- ? Inicialización de series base

#### 1.6 **Auditoría Automática**
- ? Log de todas las operaciones (INSERT, UPDATE, DELETE)
- ? Capture de valores anteriores y nuevos
- ? Campos modificados
- ? Usuario y timestamp
- ? Búsqueda y filtrado de logs

---

### **2. MDM - Master Data Management** ? 100%

#### 2.1 **Categorías de Producto**
- ? CRUD completo
- ? Validación de nombre único por empresa
- ? Interfaz Blazor con modales

#### 2.2 **Unidades de Medida**
- ? CRUD completo
- ? Abreviatura + Nombre
- ? Validación de unicidad

#### 2.3 **Clientes**
- ? CRUD completo
- ? Código, Razón Social, NIT
- ? Tipo de cliente (Nacional/Internacional)
- ? Datos de contacto

#### 2.4 **Proveedores**
- ? CRUD completo
- ? Tipo (Local/Internacional)
- ? Condiciones de pago
- ? País de origen

#### 2.5 **Almacenes**
- ? CRUD completo
- ? Ubicaciones dentro del almacén
- ? Responsable
- ? Dirección y contacto

#### 2.6 **Productos** ? **ÚLTIMO IMPLEMENTADO**
- ? CRUD completo
- ? Relación con Categoría y Unidad de Medida
- ? Tipos: Materia Prima, Producto Terminado, Servicio
- ? Control de stock opcional
- ? Múltiples campos de precio (Compra, Venta, Costo Base)
- ? SKU opcional
- ? Stock mínimo
- ? Interfaz con selectores dropdown
- ? Validación de datos previos

---

### **3. INVENTARIO** ? Pendiente

- ? Movimientos de Inventario
- ? Kardex
- ? Ajustes y Transferencias
- ? Valorización (Costo Promedio)
- ? Trazabilidad

---

### **4. COMPRAS** ? Pendiente

- ? Orden de Compra
- ? Recepción de Mercadería
- ? Gastos Asociados
- ? Prorrateo de Costos
- ? Integración con Inventario

---

### **5. VENTAS** ? Pendiente

- ? Cotización
- ? Pedido de Venta
- ? Facturación Interna
- ? Control de Margen
- ? Integración con Inventario

---

## ?? Tecnologías y Frameworks

### **Backend**
| Tecnología | Versión | Uso |
|------------|---------|-----|
| .NET | 8.0 | Framework principal |
| ASP.NET Core | 8.0 | Web API |
| Entity Framework Core | 8.0.12 | ORM |
| SQL Server | 2019+ | Base de datos |
| Swagger/OpenAPI | - | Documentación API |
| JWT | - | Autenticación |
| Asp.Versioning | - | Versionado API |

### **Frontend**
| Tecnología | Versión | Uso |
|------------|---------|-----|
| Blazor WebAssembly | .NET 8 | SPA Framework |
| Bootstrap | 5.x | UI Framework |
| Bootstrap Icons | - | Iconografía |
| JavaScript Interop | - | Funciones JS |

### **Testing**
| Tecnología | Versión | Uso |
|------------|---------|-----|
| xUnit | - | Framework de testing |
| Moq | - | Mocking (futuro) |

### **Herramientas de Desarrollo**
- Visual Studio 2022/2026
- Git / GitHub
- SQL Server Management Studio
- Azure Data Studio
- PowerShell (Scripts de automatización)

---

## ?? Patrones de Diseño

### **1. Repository Pattern**
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
}
```

### **2. Unit of Work Pattern**
```csharp
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

### **3. Result Pattern**
```csharp
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? Error { get; private set; }
    
    public static Result<T> Success(T value);
    public static Result<T> Failure(string error);
}
```

### **4. Service Layer Pattern**
Todos los servicios implementan interfaces y siguen la estructura:
```csharp
public interface IEntityService
{
    Task<Result<IReadOnlyList<EntityDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<EntityDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<EntityDto>> CreateAsync(CreateEntityDto dto, CancellationToken ct = default);
    Task<Result<EntityDto>> UpdateAsync(int id, UpdateEntityDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
```

---

## ?? Entidades del Dominio

### **Jerarquía de Entidades Base**

```csharp
// Base para todas las entidades
public abstract class AuditableEntity
{
    public DateTime FechaCreacion { get; set; }
    public string? CreadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? ModificadoPor { get; set; }
    public bool Activo { get; set; } = true;
}

// Base para entidades multi-tenant
public abstract class TenantEntity : AuditableEntity
{
    public int EmpresaId { get; set; }
}
```

### **Entidades Core**

#### **Empresa** (Root del Tenant)
```csharp
public class Empresa : AuditableEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string? NIT { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? MonedaBaseId { get; set; }
    public Moneda? MonedaBase { get; set; }
}
```

#### **Usuario**
```csharp
public class Usuario : AuditableEntity
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }  // SHA256
    public string NombreCompleto { get; set; }
    public int? EmpresaActivaId { get; set; }
    public Empresa? EmpresaActiva { get; set; }
    public ICollection<UsuarioEmpresa> Empresas { get; set; }
}
```

#### **Producto** (Ejemplo de Tenant Entity)
```csharp
public class Producto : TenantEntity
{
    public int Id { get; set; }
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public int CategoriaProductoId { get; set; }
    public int UnidadMedidaId { get; set; }
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal StockMinimo { get; set; }
    public string? Sku { get; set; }
    public string TipoProducto { get; set; }  // MateriaPrima/ProductoTerminado/Servicio
    public bool ControlStock { get; set; } = true;
    public decimal CostoBase { get; set; }
}
```

---

## ?? Sistema Multi-Tenant

### **Conceptos Clave**

1. **Empresa** = Tenant (Raíz del aislamiento)
2. **TenantEntity** = Base para todas las entidades por empresa
3. **EmpresaId** = FK presente en todas las tablas tenant-aware
4. **Usuario-Empresa** = Many-to-Many con roles

### **Flujo Multi-Tenant**

```
1. Usuario hace login
   ?
2. Sistema determina empresa activa (EmpresaActivaId)
   ?
3. JWT incluye claim "EmpresaId"
   ?
4. Todos los servicios filtran por EmpresaId
   ?
5. Auditoría captura EmpresaId automáticamente
   ?
6. Datos completamente aislados por empresa
```

### **Validación Tenant en API**

```csharp
// TenantRequiredMiddleware
// Valida que operaciones de escritura incluyan EmpresaId
if (isWriteOperation && !hasEmpresaId)
{
    return 403 Forbidden;
}
```

### **Auto-Set EmpresaId**

```csharp
// AuditableEntityInterceptor
if (entry.Entity is TenantEntity tenantEntity
    && tenantEntity.EmpresaId == 0
    && empresaId.HasValue)
{
    tenantEntity.EmpresaId = empresaId.Value;
}
```

---

## ?? Auditoría Automática

### **Interceptor de Auditoría**

El sistema captura automáticamente:
- ? **Entidad afectada**
- ? **ID de la entidad**
- ? **Acción** (Insert/Update/Delete)
- ? **Valores anteriores** (JSON)
- ? **Valores nuevos** (JSON)
- ? **Campos modificados**
- ? **Usuario** que realizó la acción
- ? **EmpresaId** del contexto
- ? **Timestamp** UTC

### **Tabla AuditLog**

```sql
CREATE TABLE [core].[AuditLogs] (
    Id BIGINT IDENTITY PRIMARY KEY,
    Entidad NVARCHAR(128) NOT NULL,
    EntidadId NVARCHAR(128) NOT NULL,
    Accion NVARCHAR(20) NOT NULL,
    ValoresAnteriores NVARCHAR(MAX),
    ValoresNuevos NVARCHAR(MAX),
    CamposModificados NVARCHAR(1000),
    EmpresaId INT,
    Usuario NVARCHAR(256),
    FechaHora DATETIME2 NOT NULL
);
```

### **Interfaz de Búsqueda**

Filtros disponibles:
- ?? Rango de fechas
- ?? Entidad
- ?? Usuario
- ?? Empresa
- ? Acción (Insert/Update/Delete)

---

## ?? Autenticación y Autorización

### **Autenticación JWT**

```csharp
// AuthService genera JWT
var claims = new List<Claim>
{
    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new(ClaimTypes.Name, user.NombreUsuario),
    new(ClaimTypes.Email, user.Email),
    new(ClaimTypes.Role, rol),
    new("EmpresaId", empresaId.Value.ToString())
};
```

### **Roles del Sistema**

| Rol | Descripción |
|-----|-------------|
| **Admin** | Acceso total |
| **Manager** | Reportes y aprobaciones |
| **User** | Operaciones CRUD |
| **Viewer** | Solo lectura |

### **Hash de Contraseñas**

```csharp
// SHA256 (Desarrollo - Cambiar a BCrypt/Argon2 en producción)
using var sha = SHA256.Create();
var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
return Convert.ToBase64String(bytes);
```

### **Credenciales Por Defecto**

```
Email:      admin@agorahub360.com
Usuario:    admin
Contraseña: Admin123  (Cambiar en producción)
Rol:        Admin
```

---

## ?? API REST

### **Estructura de Endpoints**

```
/api/v1/
??? auth/
?   ??? login (POST)
??? diagnostics/
?   ??? ping (GET)
?   ??? database-test (GET)
??? empresas/ (GET, POST, PUT, DELETE)
??? usuarios/ (GET, POST, PUT, DELETE)
??? roles/ (GET, POST, PUT, DELETE)
??? parametros/ (GET, POST, PUT, DELETE)
??? numeraciones/ (GET, POST, PUT, DELETE)
??? audit-logs/ (GET, search)
??? categorias/ (GET, POST, PUT, DELETE)
??? unidades-medida/ (GET, POST, PUT, DELETE)
??? clientes/ (GET, POST, PUT, DELETE)
??? proveedores/ (GET, POST, PUT, DELETE)
??? almacenes/ (GET, POST, PUT, DELETE)
??? productos/ (GET, POST, PUT, DELETE)
```

### **Formato de Respuesta**

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    
    public static ApiResponse<T> Ok(T data, string? message = null);
    public static ApiResponse<T> Fail(string message);
}
```

### **Ejemplo de Request/Response**

**POST /api/v1/productos**
```json
{
  "codigo": "PROD-001",
  "nombre": "Laptop Dell",
  "categoriaProductoId": 1,
  "unidadMedidaId": 2,
  "precioCompra": 500.00,
  "precioVenta": 750.00,
  "tipoProducto": "ProductoTerminado",
  "controlStock": true
}
```

**Response 201 Created**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "codigo": "PROD-001",
    "nombre": "Laptop Dell",
    "categoriaNombre": "Electrónica",
    "unidadMedidaNombre": "Unidad",
    "precioVenta": 750.00,
    "activo": true
  },
  "message": "Producto creado exitosamente"
}
```

---

## ?? Frontend Blazor WebAssembly

### **Características**

- ? SPA (Single Page Application)
- ? Componentes Razor reutilizables
- ? State Management local
- ? HTTP Services con manejo de errores
- ? JWT Storage en localStorage
- ? Authorization por roles
- ? Responsive design (Bootstrap)

### **Estructura de Páginas**

```
/login         ? Login
/              ? Dashboard
/sistema/
  /empresas    ? Gestión Empresas
  /usuarios    ? Gestión Usuarios
  /roles       ? Gestión Roles
  /audit-logs  ? Logs de Auditoría
/config/
  /parametros  ? Parámetros y Numeración
/mdm/
  /categorias          ? Categorías Producto
  /unidades-medida     ? Unidades de Medida
  /clientes            ? Clientes
  /proveedores         ? Proveedores
  /almacenes           ? Almacenes
  /productos           ? Productos
```

### **Patrón de Componentes**

```razor
@page "/mdm/productos"
@attribute [Authorize]
@inject ProductoHttpService ProductoService
@inject IJSRuntime JS

<PageTitle>Productos</PageTitle>

<!-- UI Markup -->

@code {
    private List<ProductoDto> productos = new();
    private bool loading = true;
    private string? errorMessage;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }
    
    private async Task LoadData()
    {
        // ...
    }
}
```

---

## ??? Base de Datos

### **SQL Server**

- **Versión:** 2019+
- **Collation:** Latin1_General_CI_AS
- **Nombre BD:** `AgoraHub360ERP`

### **Esquemas**

```sql
[core]     -- Entidades del núcleo
[mdm]      -- Master Data Management
[inv]      -- Inventario (futuro)
[com]      -- Compras (futuro)
[ven]      -- Ventas (futuro)
```

### **Tablas Core**

| Tabla | Schema | Descripción |
|-------|--------|-------------|
| Empresas | core | Tenants del sistema |
| Usuarios | core | Usuarios del sistema |
| UsuarioEmpresas | core | Relación Usuario-Empresa-Rol |
| Roles | core | Catálogo de roles |
| Monedas | core | Monedas del sistema |
| AuditLogs | core | Log de auditoría |
| ParametrosSistema | core | Configuración |
| NumeracionesDocumento | core | Series de numeración |

### **Tablas MDM**

| Tabla | Schema | Descripción |
|-------|--------|-------------|
| CategoriasProducto | mdm | Categorías |
| UnidadesMedida | mdm | Unidades de medida |
| Clientes | mdm | Clientes |
| Proveedores | mdm | Proveedores |
| Almacenes | mdm | Almacenes |
| UbicacionesAlmacen | mdm | Ubicaciones en almacén |
| Productos | mdm | Productos |

### **Índices Únicos**

Todas las entidades tenant-aware tienen índice único:
```sql
CREATE UNIQUE INDEX IX_Producto_Empresa_Codigo
ON [mdm].[Productos] (EmpresaId, Codigo);
```

### **Migraciones**

```powershell
# Crear migración
dotnet ef migrations add NombreMigracion --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api

# Aplicar migraciones
dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api
```

---

## ?? Configuración y Deployment

### **appsettings.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=AgoraHub360ERP;User Id=sa;Password=...;TrustServerCertificate=true;MultipleActiveResultSets=true;Encrypt=false"
  },
  "Jwt": {
    "Key": "AgoraHub360-ERP-Dev-Secret-Key-2024-MinLength32!",
    "Issuer": "AgoraHub360.ERP",
    "Audience": "AgoraHub360.ERP.Web",
    "ExpirationHours": 8
  }
}
```

### **Puertos**

- **API:** `https://localhost:7001`
- **Web:** `https://localhost:5002`

### **Inicio del Sistema**

```powershell
# Script automatizado
.\start-system.ps1

# O manualmente
# Terminal 1 - API
cd src\AgoraHub360.ERP.Api
dotnet run --launch-profile https

# Terminal 2 - Web
cd src\AgoraHub360.ERP.Web
dotnet run --launch-profile https
```

---

## ??? Scripts y Herramientas

### **Scripts PowerShell Disponibles**

| Script | Descripción |
|--------|-------------|
| `start-system.ps1` | Inicia API + Web automáticamente |
| `quick-db-test.ps1` | Test rápido de conexión BD |
| `test-database-connection.ps1` | Test completo con API |
| `reset-admin-user.ps1` | Reset usuario administrador (automático) |
| `reset-admin-simple.ps1` | Genera SQL para reset admin |
| `reset-password-custom.ps1` | Reset con contraseña personalizada |

### **Documentación de Scripts**

- `CREDENCIALES-DEFAULT.md` - Credenciales por defecto
- `DATABASE-TEST-GUIDE.md` - Guía de testing BD
- `RESET-ADMIN-GUIDE.md` - Guía de reseteo admin
- `RESET-ADMIN-QUICKSTART.md` - Quick start reset
- `CUSTOM-PASSWORD-SETUP.md` - Setup contraseña custom

---

## ?? Testing

### **Unit Tests**

```
tests/AgoraHub360.ERP.Tests/
??? Domain/
?   ??? AuditableEntityTests.cs
?   ??? TenantEntityTests.cs
?   ??? EmpresaTests.cs
??? Application/
    ??? RolServiceTests.cs
    ??? NumeracionDocumentoServiceTests.cs
```

### **Ejecutar Tests**

```powershell
dotnet test
```

### **Coverage**

- Domain: ? Parcial
- Application: ? Parcial
- Integration: ? Pendiente

---

## ?? Convenciones de Código

### **Naming Conventions**

```csharp
// Clases: PascalCase
public class ProductoService { }

// Interfaces: IPascalCase
public interface IProductoService { }

// Métodos: PascalCase
public async Task<Result<ProductoDto>> GetByIdAsync(int id) { }

// Parámetros: camelCase
private readonly IRepository<Producto> _repository;

// Constantes: PascalCase
public const string Admin = "Admin";
```

### **Commits**

Seguir convención de [Conventional Commits](COMMIT_CONVENTION.md):

```
feat(mdm): añadir gestión de productos
fix(auth): corregir validación de token
docs(readme): actualizar documentación
refactor(services): optimizar queries
```

### **Tipos de Commit**

- `feat`: Nueva característica
- `fix`: Corrección de bugs
- `docs`: Documentación
- `refactor`: Refactorización
- `test`: Tests
- `chore`: Tareas menores

---

## ?? Estado Actual del Desarrollo

### **Completado (?)**

#### Backend API
- ? Clean Architecture implementada
- ? 12/12 Controllers Core + MDM
- ? Multi-tenant funcional
- ? Auditoría automática
- ? JWT Authentication
- ? Repository Pattern
- ? Unit of Work
- ? Result Pattern

#### Frontend Blazor
- ? 12/12 Páginas Core + MDM
- ? HTTP Services completos
- ? AuthStateProvider
- ? NavMenu organizado
- ? Modales para CRUD
- ? Manejo de errores inline

#### Base de Datos
- ? Migraciones completas
- ? Seed data (Monedas, Roles, Admin)
- ? Índices únicos
- ? FK Constraints
- ? Multi-tenant tables

#### Documentación
- ? README completo
- ? Guías de scripts
- ? Documentación técnica (este archivo)
- ? Commit conventions

---

### **En Desarrollo (??)**

- ?? Testing completo
- ?? Validaciones adicionales
- ?? Manejo de errores mejorado

---

### **Pendiente (?)**

#### Módulos
- ? Inventario (Movimientos, Kardex)
- ? Compras (Órdenes, Recepciones)
- ? Ventas (Cotizaciones, Pedidos, Facturas)
- ? Contabilidad
- ? Finanzas
- ? Reportes y Dashboards

#### Funcionalidades
- ? Facturación Electrónica SIN (v1.1)
- ? Paginación en listados
- ? Filtros avanzados
- ? Exportación a Excel
- ? Importación masiva
- ? Cambio de contraseña desde UI
- ? Gestión de permisos granulares
- ? Notificaciones
- ? Logs de sistema

---

## ??? Roadmap y Próximas Funcionalidades

### **v1.0 - MVP (Actual)** ?
- ? Core + MDM completos
- ? Multi-tenant operativo
- ? Auditoría funcional

### **v1.1 - Inventario Básico** ?
- ? Movimientos de Inventario
- ? Kardex por producto
- ? Ajustes y Transferencias
- ? Valorización (Costo Promedio)
- ? Reportes de stock

### **v1.2 - Compras + Importación** ?
- ? Orden de Compra
- ? Recepción parcial/total
- ? Gastos asociados
- ? Prorrateo de costos
- ? Integración con Inventario

### **v1.3 - Ventas Básicas** ?
- ? Cotización
- ? Pedido de Venta
- ? Facturación interna
- ? Control de margen
- ? Integración con Inventario

### **v1.4 - Facturación Electrónica SIN** ?
- ? Integración con API SIN (Bolivia)
- ? Emisión de facturas electrónicas
- ? Anulación de facturas
- ? Reporte de ventas al SIN

### **v2.0 - BI y Reportes** ??
- ? Dashboards
- ? Reportes configurables
- ? Gráficos y estadísticas
- ? Exportación a PDF/Excel

### **v2.x - Módulos Avanzados** ??
- ? Producción / MRP
- ? Contabilidad
- ? Finanzas
- ? CRM
- ? RRHH
- ? Automatización con workflows

---

## ?? Contexto para Desarrollo Continuo

### **Al Continuar el Desarrollo, Considera:**

1. **Mantener Clean Architecture**
   - Domain no debe depender de nada
   - Application orquesta el negocio
   - Persistence solo para datos

2. **Multi-Tenant Siempre**
   - Todas las entidades transaccionales heredan de `TenantEntity`
   - Servicios SIEMPRE filtran por `EmpresaId`
   - Validar pertenencia al tenant en operaciones

3. **Auditoría Automática**
   - No es necesario código adicional
   - El interceptor captura todo automáticamente

4. **Patrón Consistente**
   - Usar `Result<T>` en servicios
   - DTOs separados (Create/Update/Read)
   - HTTP Services en Web
   - Modales para CRUD

5. **Validaciones**
   - DataAnnotations en DTOs
   - Validaciones de negocio en Services
   - Mensajes inline en Blazor

6. **Testing**
   - Agregar tests para nueva funcionalidad
   - Mock de repositorios
   - Tests de servicios

---

## ?? Información de Contacto

**Proyecto:** AgoraHub360 ERP  
**Organización:** Ágora HUB 360 - Dirección Ágora Tech  
**Repositorio:** https://github.com/abelcalvimontes/AgoraHUB360-ERP  
**Metodología:** Modelo Ágora (MAPE)

---

## ?? Notas Finales

Este documento proporciona una visión completa del estado actual del proyecto **AgoraHub360 ERP**. 

**Usa esta documentación como contexto para:**
- ? Entender la arquitectura completa
- ? Continuar el desarrollo coherentemente
- ? Mantener los estándares establecidos
- ? Implementar nuevas funcionalidades
- ? Onboarding de nuevos desarrolladores

**Mantén esta documentación actualizada** conforme avance el proyecto.

---

**Última Actualización:** 2026-02-17  
**Versión del Sistema:** 1.0  
**Estado:** ? **MDM 100% Completado - Listo para Módulo Inventario**
