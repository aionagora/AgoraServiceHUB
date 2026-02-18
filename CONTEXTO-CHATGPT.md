# ?? CONTEXTO RÁPIDO PARA CHATGPT - AGORAHUB360 ERP

## ?? Resumen Ejecutivo

**AgoraHub360 ERP** es un sistema ERP empresarial multi-tenant desarrollado en **.NET 8** con **Blazor WebAssembly** y **Clean Architecture**.

**Branch actual:** `GestionProductos`  
**Estado:** ? **Core + MDM 100% Completado**  
**Próximo:** Módulo Inventario

---

## ??? Arquitectura

### Stack Tecnológico
- **Backend:** .NET 8, ASP.NET Core Web API, EF Core 8.0.12, SQL Server 2019+
- **Frontend:** Blazor WebAssembly, Bootstrap 5
- **Auth:** JWT con SHA256 (cambiar a BCrypt en producción)
- **API:** Versionada (`/api/v1/...`), Swagger/OpenAPI

### Estructura (Clean Architecture)
```
Domain (Entities, Interfaces) 
  ?
Application (Services, Business Logic)
  ?
Persistence (EF Core, Repositories) + Infrastructure (External)
  ?
API (Controllers, Middleware) + Web (Blazor WASM)
```

**Principios:** SOLID, Repository Pattern, Unit of Work, Result Pattern

---

## ?? Módulos Completados

### ? CORE (100%)
- Multi-empresa (Multi-tenant con `EmpresaId`)
- Autenticación JWT (4 roles: Admin, Manager, User, Viewer)
- Usuarios y Empresas (Many-to-Many con roles)
- Multimoneda (BOB, USD, EUR)
- Parámetros del sistema (clave-valor por empresa)
- Numeración de documentos (series personalizables)
- Auditoría automática (INSERT/UPDATE/DELETE con interceptor)

### ? MDM - Master Data (100%)
- Categorías de Producto
- Unidades de Medida
- Clientes (Nacional/Internacional)
- Proveedores (Local/Internacional)
- Almacenes + Ubicaciones
- **Productos** ? **ÚLTIMO IMPLEMENTADO**
  - Tipos: Materia Prima, Producto Terminado, Servicio
  - Relación con Categoría y Unidad
  - Precios: Compra, Venta, Costo Base
  - Control de stock opcional
  - SKU, Stock mínimo

---

## ?? Entidades Clave

### Jerarquía Base
```csharp
// Base para todas las entidades
public abstract class AuditableEntity
{
    DateTime FechaCreacion, FechaModificacion
    string? CreadoPor, ModificadoPor
    bool Activo = true
}

// Base para entidades multi-tenant
public abstract class TenantEntity : AuditableEntity
{
    int EmpresaId  // ? FK a Empresa (Tenant)
}
```

### Ejemplo: Producto
```csharp
public class Producto : TenantEntity
{
    int Id, EmpresaId
    string Codigo, Nombre, TipoProducto
    int CategoriaProductoId, UnidadMedidaId
    decimal PrecioCompra, PrecioVenta, CostoBase, StockMinimo
    bool ControlStock = true
    string? Sku, Descripcion
}
```

### Multi-Tenant Flow
```
Usuario ? Login ? JWT con claim "EmpresaId"
  ?
Servicios filtran automáticamente por EmpresaId
  ?
Interceptor auto-asigna EmpresaId en nuevas entidades
  ?
Middleware valida EmpresaId en operaciones de escritura
  ?
Datos 100% aislados por empresa
```

---

## ??? Base de Datos

### Esquemas
- `[core]` - Empresas, Usuarios, Roles, Monedas, AuditLogs, Parámetros, Numeraciones
- `[mdm]` - Categorías, Unidades, Clientes, Proveedores, Almacenes, Productos

### Índices Únicos
Todas las entidades tenant-aware:
```sql
CREATE UNIQUE INDEX IX_Producto_Empresa_Codigo
ON [mdm].[Productos] (EmpresaId, Codigo);
```

### Auditoría
Tabla `[core].[AuditLogs]` captura automáticamente:
- Entidad, EntidadId, Acción (Insert/Update/Delete)
- ValoresAnteriores, ValoresNuevos (JSON)
- CamposModificados, Usuario, EmpresaId, FechaHora

---

## ?? Patrones Implementados

### 1. Result Pattern
```csharp
public class Result<T>
{
    bool IsSuccess
    T? Value
    string? Error
    
    static Success(T value)
    static Failure(string error)
}
```

### 2. Repository Pattern
```csharp
IRepository<T> : GetById, GetAll, Find, Add, Update, Delete
```

### 3. Service Layer
```csharp
IEntityService : GetAllAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync
// Todos retornan Result<T>
```

---

## ?? API REST

### Endpoints Disponibles
```
/api/v1/
??? auth/login
??? empresas, usuarios, roles
??? parametros, numeraciones, audit-logs
??? categorias, unidades-medida, clientes, proveedores, almacenes, productos
```

### Formato de Respuesta
```csharp
ApiResponse<T> { bool Success, T? Data, string? Message }
```

### Autenticación
```
Authorization: Bearer <JWT>
Claims: NameIdentifier, Name, Email, Role, EmpresaId
```

---

## ?? Frontend Blazor

### Estructura
```
/login, /
/sistema/ ? Empresas, Usuarios, Roles, AuditLogs
/config/ ? Parámetros y Numeración
/mdm/ ? Categorías, Unidades, Clientes, Proveedores, Almacenes, Productos
```

### Patrón de Componentes
```razor
@page "/mdm/productos"
@inject ProductoHttpService Service

<PageTitle>Productos</PageTitle>

<!-- Lista + Modales CRUD -->

@code {
    List<ProductoDto> items;
    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }
}
```

---

## ?? Credenciales Por Defecto

```
Email:      admin@agorahub360.com
Usuario:    admin
Contraseña: Admin123
Rol:        Admin
Empresa:    AgoraHub360 - Empresa Demo (ID: 1)
```

**?? Cambiar en producción**

---

## ??? Comandos Útiles

### Iniciar Sistema
```powershell
.\start-system.ps1
# API: https://localhost:7001
# Web: https://localhost:5002
```

### Migraciones
```powershell
dotnet ef migrations add NombreMigracion --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api
dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api
```

### Tests
```powershell
dotnet test
```

### Reset Admin
```powershell
.\reset-admin-user.ps1
```

---

## ? Próximos Pasos (Roadmap)

### v1.1 - Inventario ? **SIGUIENTE**
- ? Movimientos de Inventario
- ? Kardex por producto
- ? Ajustes y Transferencias
- ? Valorización (Costo Promedio)

### v1.2 - Compras
- ? Orden de Compra
- ? Recepción de Mercadería
- ? Gastos asociados
- ? Prorrateo de costos

### v1.3 - Ventas
- ? Cotización, Pedido, Factura
- ? Control de margen

### v1.4 - Facturación SIN (Bolivia)
- ? Integración API SIN
- ? Facturación electrónica

---

## ?? Convenciones

### Commits (Conventional Commits)
```
feat(mdm): añadir gestión de productos
fix(auth): corregir validación JWT
docs(readme): actualizar documentación
refactor(services): optimizar queries
```

### Naming
- **Clases/Métodos:** PascalCase
- **Interfaces:** IPascalCase
- **Parámetros/Variables:** camelCase
- **Privados:** _camelCase

---

## ?? Al Desarrollar, Recuerda:

### ? SIEMPRE
1. **Heredar de `TenantEntity`** para entidades por empresa
2. **Filtrar por `EmpresaId`** en servicios
3. **Usar `Result<T>`** en servicios
4. **DTOs separados** (Create/Update/Read)
5. **Validar en Services** (no solo en DTOs)
6. **HTTP Services** en Web para cada controller
7. **Modales** para CRUD en Blazor
8. **Índice único** `(EmpresaId, Codigo)` en tablas tenant

### ? NUNCA
1. Dejar entidades sin `EmpresaId` si son tenant-aware
2. Olvidar validar unicidad por empresa
3. Hardcodear `EmpresaId` (viene del claim JWT)
4. Exponer entidades del dominio directamente (usar DTOs)

---

## ?? Archivos Importantes

### Documentación
- `DOCUMENTACION-TECNICA-COMPLETA.md` - Documentación detallada
- `README.md` - Overview del proyecto
- `COMMIT_CONVENTION.md` - Convenciones de commits

### Scripts
- `start-system.ps1` - Iniciar API + Web
- `reset-admin-user.ps1` - Reset usuario admin
- `quick-db-test.ps1` - Test conexión BD

### Configuración
- `src/AgoraHub360.ERP.Api/appsettings.json` - Connection string, JWT
- `src/AgoraHub360.ERP.Persistence/Context/AgoraDbContext.cs` - DbContext principal

---

## ?? Prompt Recomendado para ChatGPT

```
Contexto: Estoy desarrollando AgoraHub360 ERP, un sistema multi-tenant en .NET 8 
con Blazor WASM y Clean Architecture.

Estado actual:
- ? Core completo (Multi-tenant, Auth JWT, Auditoría automática)
- ? MDM completo (Categorías, Unidades, Clientes, Proveedores, Almacenes, Productos)
- ? Próximo: Módulo Inventario

Arquitectura:
- Domain ? Application ? Persistence/Infrastructure ? API/Web
- Multi-tenant con EmpresaId en todas las entidades transaccionales
- TenantEntity (base) hereda de AuditableEntity
- Repository Pattern + Unit of Work + Result Pattern
- Auditoría automática con interceptor

Necesito ayuda con: [DESCRIBE TU NECESIDAD]
```

---

**Versión:** 1.0  
**Fecha:** 2026-02-17  
**Estado:** ? **MDM Completado - Listo para Inventario**

---

?? **Ver documentación completa:** `DOCUMENTACION-TECNICA-COMPLETA.md`
