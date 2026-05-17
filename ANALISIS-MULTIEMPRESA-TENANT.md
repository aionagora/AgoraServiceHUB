# Análisis de Aislamiento Multiempresa / Tenant — AgoraHUB360 ERP

> Fecha: 2026-05-17  
> Tipo: Investigación arquitectónica (sin cambios de código)

---

## Resumen ejecutivo

El repositorio **sí tiene una base multiempresa real**, pero el aislamiento está **implementado de forma inconsistente**.

**Lo sólido:**
- El dominio define `TenantEntity` con `EmpresaId` para la mayoría de entidades tenant-aware (`src/AgoraHub360.ERP.Domain/Common/TenantEntity.cs`).
- `AgoraDbContext` captura `ICurrentUserService.EmpresaId` y aplica **query filters globales** a todas las entidades que heredan de `TenantEntity`.
- Varios servicios validan explícitamente la empresa activa antes de leer/escribir (clientes, proveedores, contactos, perfiles, numeraciones y parámetros).

**Lo problemático:**
1. El tenant efectivo del backend sale solo del **claim JWT `EmpresaId`**, calculado en login como `Usuario.EmpresaActivaId ?? primeraEmpresaAsignada ?? primeraEmpresaActivaDelSistema` (`AuthService.cs:48-59`).
2. El **selector de empresa del frontend no actualiza ese claim**: solo guarda la empresa elegida en `localStorage` y el layout nunca renueva token ni llama a un endpoint para cambiar empresa activa (`MainLayout.razor:158-186`).
3. **Hay endpoints/servicios que bypassan el scoping**: especialmente sucursales, empresas, usuarios y auditoría.

---

## 1. Mapa estructural del repositorio

### Capas de la solución

| Proyecto | Responsabilidad |
|---|---|
| `AgoraHub360.ERP.Domain` | Entidades, objetos de valor, contratos de dominio |
| `AgoraHub360.ERP.Application` | Servicios de aplicación, interfaces, DTOs internos |
| `AgoraHub360.ERP.Persistence` | DbContext EF Core, repositorios, interceptores, migraciones |
| `AgoraHub360.ERP.Infrastructure` | Servicios transversales (email, almacenamiento, etc.) |
| `AgoraHub360.ERP.Api` | ASP.NET Core Web API, controladores, middleware, auth JWT |
| `AgoraHub360.ERP.Web` | Frontend Blazor WebAssembly |
| `AgoraHub360.ERP.Shared` | DTOs compartidos entre API y Web |

### Responsabilidades relevantes para multiempresa

**Domain**
- Entidades base: `AuditableEntity` (`Domain/Common/AuditableEntity.cs`), `TenantEntity` (`Domain/Common/TenantEntity.cs`).
- Núcleo multiempresa: `Empresa`, `Usuario`, `UsuarioEmpresa`, `Sucursal`, `Rol`, `UsuarioSucursalAcceso`, `PerfilAcceso`, `PerfilPermiso`, `UsuarioPerfil` (`Domain/Entities/Core/`).
- Entidades tenant-aware de negocio: `Cliente`, `ClienteSucursal`, `Contacto`, `Proveedor`, `Almacen`, `Product`, `CompanyProduct`, `PedidoVenta`, `OrdenCompra`, `PeriodoContable`, etc.

**Application**
- Servicios de negocio y scoping por tenant.
- Servicios bien scoped: clientes, proveedores, contactos, cliente sucursales, seguridad dinámica, usuario sucursal acceso.
- Servicio mal scoped: sucursales.

**Persistence**
- `AgoraDbContext` + query filters globales por `EmpresaId` (`Persistence/Context/AgoraDbContext.cs`).
- `Repository<T>` con métodos normales y métodos que **bypassean filtros** (`Persistence/Repositories/Repository.cs`).
- Interceptores: auditoría automática + auto-asignación de `EmpresaId` al crear `TenantEntity` (`Persistence/Interceptors/AuditableEntityInterceptor.cs`).

**API**
- Login/JWT (`AuthController`, `AuthService`).
- Middleware tenant (`TenantRequiredMiddleware`).
- CRUD de core/MDM/ACC/etc.

**Web**
- Login Blazor + almacenamiento del JWT (`Login.razor`, `JwtAuthStateProvider.cs`).
- Estado local de empresa activa (`EmpresaStateService.cs`).
- Estado local de contexto/sucursal (`SesionUsuarioStateService.cs`).
- Páginas que todavía consumen catálogos globales.

### Entidades por tipo de herencia

```
AuditableEntity (solo auditoría, sin EmpresaId)
  └─ Empresa
  └─ Usuario
  └─ Rol
  └─ FormularioSistema, ModuloSistema, AccionSistema
  └─ DepreciacionMensual, CentroCosto, CostingRule, Industry, ...

TenantEntity : AuditableEntity (tiene EmpresaId → aplica query filter)
  └─ Sucursal
  └─ ParametroSistema, NumeracionDocumento, PerfilAcceso, PerfilPermiso, UsuarioPerfil, UsuarioSucursalAcceso
  └─ Cliente, ClienteSucursal, Contacto, Proveedor, Almacen
  └─ Product, CompanyProduct, Brand, Manufacturer, Catalog, Uom, ProductStatus, ...
  └─ PedidoVenta, OrdenCompra, RecepcionCompra, HojaImportacion, ...
  └─ AsientoContable, CuentaContable, PeriodoContable, PriceList, ...

AuditLog (entidad separada, no hereda de TenantEntity; tiene EmpresaId nullable)
```

---

## 2. Implementación actual del aislamiento tenant

### Base técnica

**Entidad tenant-aware**
- `TenantEntity` agrega `EmpresaId` a todas las entidades de negocio.

**Contexto actual**
- `CurrentUserService` lee `UserId`, `UserName`, `EmpresaId` desde claims del JWT (`Api/Services/CurrentUserService.cs:34-41`).

**Filtro global EF Core**
- `AgoraDbContext` guarda `_empresaId = currentUserService.EmpresaId` y aplica:
  ```csharp
  HasQueryFilter(e => _empresaId == null || e.EmpresaId == _empresaId)
  ```
  a **todas** las entidades `TenantEntity` (`Persistence/Context/AgoraDbContext.cs`).

**Repositorio genérico**
- `GetAllAsync` y `FindAsync` usan el `DbSet` con filtro global activo (`Persistence/Repositories/Repository.cs:31-38`).
- También existen `GetByIdIgnoreQueryFiltersAsync` y `FindIgnoreQueryFiltersAsync` que **saltean** el filtro.

**Auto-asignación de EmpresaId en writes**
- El interceptor pone `EmpresaId` automáticamente al crear un `TenantEntity` si viene `0` y existe claim (`Persistence/Interceptors/AuditableEntityInterceptor.cs:77-83`).

### Qué entidades quedan fuera del filtro global

`Empresa`, `Usuario`, `Rol`, `FormularioSistema`, `ModuloSistema`, `AccionSistema`, `AuditLog` no heredan de `TenantEntity`. Su aislamiento depende de políticas/servicios/endpoints, **no del DbContext**. Ahí existen las principales brechas.

---

## 3. Flujo de autenticación y construcción del contexto usuario/empresa

### Login

```
[Browser/UI]  POST /api/v1/auth/login  {email, password}
     │
     ▼
[AuthService]
  1. Busca usuario activo por email
  2. Verifica hash de contraseña
  3. Lee UsuarioEmpresa (asignaciones)
  4. Resuelve empresa activa:
       empresaId = user.EmpresaActivaId
                ?? primeraEmpresa?.EmpresaId
                ?? primeraEmpresaActivaDelSistema   ← FALLBACK PELIGROSO
  5. Resuelve rol:
       rol = primeraEmpresa?.Rol ?? "Viewer"
  6. Genera JWT con claims:
       nameid, name, email, role, EmpresaId
     │
     ▼
[Frontend]
  7. JwtAuthStateProvider guarda token en localStorage
  8. Inyecta "Authorization: Bearer <token>" en cada request
  9. SesionUsuarioStateService.InitializeAsync(forceReload: true)
 10. Llama GET /api/v1/seguridad-dinamica/contexto-sesion
 11. SeguridadDinamicaService arma: perfiles, menú, sucursales, sucursal predeterminada
```

**Archivos clave:**
- `Api/Services/AuthService.cs` — resolución de empresa en login
- `Api/Controllers/V1/AuthController.cs` — endpoint de login
- `Web/Pages/Login.razor` — UI de login
- `Web/Services/JwtAuthStateProvider.cs` — estado de autenticación Blazor
- `Web/Services/SesionUsuarioStateService.cs` — contexto de sesión

### Cambio de empresa en UI

```
[Usuario selecciona empresa en selector]
     │
     ▼
[MainLayout.razor]
  EmpresaState.SetEmpresaActivaAsync(empresa)
     │
     ▼
[EmpresaStateService]
  _empresaActiva = empresa
  localStorage["agorahub360_empresa_activa"] = empresa.Id  ← SOLO LOCAL
  OnChange?.Invoke()
     │
     ▼
[Backend / JWT]
  ❌ No se actualiza
  CurrentUserService.EmpresaId sigue leyendo el claim original del token
```

**Resultado:** backend y frontend tienen empresas distintas.

---

## 4. Cómo se resuelve la empresa activa y dónde se filtra

### Fuente efectiva del tenant en backend

La empresa activa real para API/Application/Persistence es el **claim JWT `EmpresaId`**:

| Capa | Archivo | Líneas |
|---|---|---|
| CurrentUserService | `Api/Services/CurrentUserService.cs` | 34–41 |
| AgoraDbContext (filter) | `Persistence/Context/AgoraDbContext.cs` | 33–39 |
| Servicios | `ClienteService.cs`, `ProveedorService.cs`, etc. | — |

### Problemas en la resolución del claim

- `Usuario.EmpresaActivaId` existe en el dominio (`Domain/Entities/Core/Usuario.cs`).
- Pero `UsuarioService.CreateAsync` y `UpdateAsync` **nunca la setean** (`Application/Services/UsuarioService.cs:69-76`, `119-125`).
- No existe endpoint para **cambiar empresa activa del usuario** ni para **regenerar token** con otra empresa.
- El DTO de login no devuelve contexto multiempresa; solo token + datos básicos (`Shared/DTOs/AuthResponseDto.cs`).

---

## 5. Entidades y consultas relevantes

### Núcleo multiempresa

| Entidad | Archivo | Tipo | Notas |
|---|---|---|---|
| `Empresa` | `Domain/Entities/Core/Empresa.cs` | `AuditableEntity` | Catálogo global; sin EmpresaId |
| `Usuario` | `Domain/Entities/Core/Usuario.cs` | `AuditableEntity` | Tiene `EmpresaActivaId` y `Empresas` |
| `UsuarioEmpresa` | `Domain/Entities/Core/UsuarioEmpresa.cs` | join | `UsuarioId`, `EmpresaId`, `Rol` |
| `Sucursal` | `Domain/Entities/Core/Sucursal.cs` | `TenantEntity` | Filtrada por EF |
| `UsuarioSucursalAcceso` | `Domain/Entities/Core/UsuarioSucursalAcceso.cs` | `TenantEntity` | Acceso por sucursal |
| `PerfilAcceso` | `Domain/Entities/Core/PerfilAcceso.cs` | `TenantEntity` | Seguridad dinámica |
| `PerfilPermiso` | `Domain/Entities/Core/PerfilPermiso.cs` | `TenantEntity` | — |
| `UsuarioPerfil` | `Domain/Entities/Core/UsuarioPerfil.cs` | `TenantEntity` | — |

### MDM / terceros

| Entidad | Servicio | Estado scoping |
|---|---|---|
| `Cliente` | `ClienteService` | ✅ Bien scoped |
| `ClienteSucursal` | `ClienteSucursalService` | ✅ Bien scoped |
| `Contacto` | `ContactoService` | ✅ Bien scoped |
| `Proveedor` | `ProveedorService` | ✅ Bien scoped |
| `Almacen` | `AlmacenService` | ⚠️ Crea con tenant; sin validación extra en Get/Update/Delete |
| `Sucursal` | `SucursalService` | ❌ Usa `IgnoreQueryFilters`; acepta `empresaId` externo |
| `Product` / `CompanyProduct` | `ProductService` / `CompanyProductService` | ✅ Bien scoped |

### Configuración por empresa

| Entidad | Servicio | Estado |
|---|---|---|
| `ParametroSistema` | `ParametroSistemaService` | ✅ Bien scoped |
| `NumeracionDocumento` | `NumeracionDocumentoService` | ✅ Bien scoped |

### Auditoría

| Entidad | Servicio | Estado |
|---|---|---|
| `AuditLog` | `AuditLogService` | ❌ No aplica tenant implícito; solo filtra si cliente envía `EmpresaId` |

---

## 6. Hallazgos concretos

### Hallazgo A — El selector de empresa no cambia el tenant del backend

| Elemento | Archivo | Líneas |
|---|---|---|
| Cambio local de empresa | `Web/Layout/MainLayout.razor` | 178–186 |
| Estado solo en localStorage | `Web/Services/EmpresaStateService.cs` | 34–38 |
| Backend: lee claim JWT original | `Api/Services/CurrentUserService.cs` | 34–41 |
| Servicio de sesión también usa claim | `Application/Services/SeguridadDinamicaService.cs` | 342–363 |

**Impacto:** un usuario puede "ver" en la UI que cambió de empresa, pero las consultas al backend siguen corriendo sobre la empresa del token original.

---

### Hallazgo B — El login elige empresa/rol de forma incompleta y potencialmente incorrecta

```csharp
// Api/Services/AuthService.cs:48-59
var empresaId = user.EmpresaActivaId ?? primeraEmpresa?.EmpresaId;
// Fallback: si no tiene asignaciones, usa la primera empresa activa del sistema
if (empresaId == null)
    empresaId = (await _empresaRepository.FindAsync(e => e.Activo, ct)).FirstOrDefault()?.Id;
```

**Problemas:**
- `EmpresaActivaId` raramente está seteada.
- Si no hay asignaciones → se usa **cualquier empresa activa del sistema**.
- El `rol` del claim corresponde a la `primeraEmpresa`, que puede no coincidir con la activa elegida.

**Impacto:** tenant incorrecto desde el login; rol incorrecto para la empresa activa.

---

### Hallazgo C — Sucursales bypasea el aislamiento

```csharp
// Application/Services/SucursalService.cs
// GetAll: usa IgnoreQueryFilters + empresa por parámetro libre
public async Task<...> GetAllByEmpresaAsync(int empresaId, ...)
    => await _repo.FindIgnoreQueryFiltersAsync(s => s.EmpresaId == empresaId, ct);

// GetById/Update/Delete/CambiarEstado/EstablecerCentral: también IgnoreQueryFilters
var sucursal = await _repo.GetByIdIgnoreQueryFiltersAsync(id, ct);

// Create: acepta EmpresaId desde DTO del cliente
new Sucursal { EmpresaId = dto.EmpresaId, ... }
```

| Archivo | Líneas |
|---|---|
| `Application/Services/SucursalService.cs` | 29–31, 47–57, 59–76, 122–178, 181–241 |
| `Api/Controllers/V1/SucursalesController.cs` | 26–33 |
| `Shared/DTOs/Sucursal/CrearSucursalDto.cs` | 5–10 |

**Impacto:** cualquier usuario autenticado con un `empresaId` o `sucursalId` conocido puede consultar/manipular sucursales fuera de su tenant.

---

### Hallazgo D — Empresas y usuarios son globales y sin policy estricta

```csharp
// Api/Controllers/V1/EmpresasController.cs - solo [Authorize], sin policy
[HttpGet] public async Task<IActionResult> GetAll(...)   // devuelve TODAS

// Api/Controllers/V1/UsuariosController.cs - solo [Authorize]
[HttpGet] public async Task<IActionResult> GetAll(...)   // devuelve TODOS con sus asignaciones

// Api/Controllers/V1/RolesController.cs - solo [Authorize]
[HttpGet] public async Task<IActionResult> GetAll(...)   // catálogo global
```

**Impacto:** exposición cross-tenant de metadatos administrativos (lista de empresas, lista de usuarios, roles).

---

### Hallazgo E — La UI sigue cargando catálogos globales y filtrando en cliente

| Página | Catálogo cargado | Filtrado |
|---|---|---|
| `Config/Usuarios.razor:425-439` | Todos los usuarios | Cliente, por `_filtroEmpresaId` |
| `Config/Usuarios.razor:433-439` | Todas las empresas | Cliente, para el dropdown |
| `Config/Sucursales.razor:194-200` | Todas las empresas | Cliente, para el selector |
| `Config/SucursalForm.razor:446-448` | Todas las empresas | Cliente, para el combo |
| `Config/ParametrosNumeracion.razor:555-564` | Todas las empresas → `FirstOrDefault` activa | No hay |
| `Config/Empresas.razor:222-229` | Todas las empresas | No hay |

**Impacto:** aunque los módulos de negocio estén bien filtrados, la UI expone catálogos de otras empresas.

---

### Hallazgo F — Middleware tenant protege poco y tarde

```csharp
// Api/Middleware/TenantRequiredMiddleware.cs
// Solo actúa en rutas API
// Solo en operaciones POST/PUT/DELETE (no protege GET)
// Exime varias rutas globales
```

**Impacto:** la mayoría de las fugas de lectura no pasan por esta barrera.

---

### Hallazgo G — Auditoría no está scoped por tenant por defecto

```csharp
// Persistence/Services/AuditLogService.cs:17-41
// No inyecta ICurrentUserService
// Solo filtra si el cliente envía EmpresaId en el query
if (filter.EmpresaId.HasValue)
    query = query.Where(a => a.EmpresaId == filter.EmpresaId);
```

**Impacto:** un usuario puede pedir logs globales o de otra empresa.

---

### Hallazgo H — Duplicación de endpoint "mis empresas"

| Endpoint | Archivo | Notas |
|---|---|---|
| `GET /api/v1/auth/mis-empresas` | `Api/Controllers/V1/AuthController.cs:73-103` | Trato especial a Admin |
| `GET /api/v1/empresas/mis-empresas` | `Api/Controllers/V1/EmpresasController.cs:43-60` | Sin trato especial |

**Impacto:** comportamiento duplicado e inconsistente.

---

## 7. Hipótesis de causa raíz

La causa raíz no es un único bug, sino la combinación de tres factores:

### Factor 1 — Modelo de usuario multiempresa mal proyectado a JWT

El dominio permite muchas empresas por usuario (`Usuario.cs`, `UsuarioEmpresa.cs`), pero el token solo lleva **una** empresa y **un** rol (`AuthService.cs:92-101`). Esa empresa se obtiene por heurística, no por selección transaccional del usuario.

### Factor 2 — La empresa "activa" de la UI no se propaga al backend

El selector de empresa solo cambia `localStorage` (`EmpresaStateService.cs:34-38`). El backend sigue con el `EmpresaId` del token (`CurrentUserService.cs:34-41`).

### Factor 3 — Existen rutas que no usan el tenant actual, sino parámetros externos o datasets globales

- Sucursales por `empresaId` + `IgnoreQueryFilters`.
- Empresas/usuarios/auditoría sin policy estricta.
- UI consumiendo `GetAllAsync()` global.

### Conclusión

> Si hoy un usuario "ve datos de otras empresas" o el sistema "no discrimina bien empresa/clientes/sucursales al iniciar sesión", la explicación es:
> 1. Al iniciar sesión, el token puede quedar con una empresa distinta de la realmente deseada.
> 2. Después del login, la UI puede mostrar otra empresa "activa" sin que el backend cambie de tenant.
> 3. Además, algunos endpoints permiten leer recursos por empresa arbitraria o ver catálogos globales.

---

## 8. Prioridad de corrección recomendada

### 🔴 Prioridad 0 — Corregir el origen del tenant autenticado

**Objetivo:** que backend, JWT, sesión y UI hablen siempre de la misma empresa.

**Puntos exactos:**

| Archivo | Acción |
|---|---|
| `Api/Services/AuthService.cs` | Eliminar fallback a primera empresa del sistema; exigir empresa explícita o error claro |
| `Api/Controllers/V1/AuthController.cs` | Agregar endpoint explícito `POST /auth/seleccionar-empresa` que renueve token |
| `Domain/Entities/Core/Usuario.cs` | Asegurar que `EmpresaActivaId` se setea al crear y al cambiar empresa |
| `Application/Services/UsuarioService.cs` | Sетear `EmpresaActivaId` en `CreateAsync` y al asignar empresa |
| `Web/Layout/MainLayout.razor` | Al cambiar empresa, llamar al endpoint de renovación de token y recargar contexto |
| `Web/Services/EmpresaStateService.cs` | Delegar a backend en lugar de solo guardar en localStorage |
| `Web/Services/JwtAuthStateProvider.cs` | Soportar refresh del token al cambiar empresa |
| `Web/Services/SesionUsuarioStateService.cs` | Recargar contexto completo cuando cambia el token |

**Estrategia segura:**
1. Introducir un flujo explícito de **selección/cambio de empresa activa**.
2. Validar contra `UsuarioEmpresa`.
3. Persistir `EmpresaActivaId`.
4. Regenerar JWT con `EmpresaId` y rol de **esa empresa específica**.
5. Al cambiar empresa en UI, refrescar contexto de sesión y sucursal.

---

### 🔴 Prioridad 0 — Cerrar la brecha de sucursales

| Archivo | Acción |
|---|---|
| `Application/Services/SucursalService.cs` | Dejar de usar `IgnoreQueryFilters`; inferir empresa desde contexto actual |
| `Api/Controllers/V1/SucursalesController.cs` | Si se mantiene `empresaId` por ruta, validar que el usuario pertenece a esa empresa |
| `Web/Services/HttpSucursalService.cs` | Cambiar a endpoint que devuelva sucursales del tenant actual |
| `Web/Pages/Config/Sucursales.razor` | Consumir endpoint scoped |
| `Web/Pages/Config/SucursalForm.razor` | No exponer selector de empresa arbitraria a usuario no admin |

---

### 🟡 Prioridad 1 — Restringir endpoints globales

| Archivo | Acción |
|---|---|
| `Api/Controllers/V1/EmpresasController.cs` + `Application/Services/EmpresaService.cs` | Agregar policy `Admin` para `GetAll`; usuario normal solo ve sus empresas |
| `Api/Controllers/V1/UsuariosController.cs` + `Application/Services/UsuarioService.cs` | `GetAll` solo para Admin; usuario normal ve solo usuarios de su empresa |
| `Api/Controllers/V1/RolesController.cs` | Policy explícita para gestión de roles |
| `Api/Controllers/V1/AuditLogsController.cs` + `Persistence/Services/AuditLogService.cs` | Aplicar filtro implícito por empresa actual; Admin puede ver todos |

---

### 🟡 Prioridad 1 — Dejar de filtrar en cliente con datasets globales

| Archivo | Acción |
|---|---|
| `Web/Pages/Config/Usuarios.razor` | Consumir `GET /api/v1/usuarios?empresaId={empresa_activa}` o endpoint scoped |
| `Web/Pages/Config/Sucursales.razor` | Consumir endpoint scoped, no cargar todas las empresas |
| `Web/Pages/Config/SucursalForm.razor` | No ofrecer combo de empresas a usuario no admin |
| `Web/Pages/Config/ParametrosNumeracion.razor` | No usar `EmpresaSvc.GetAllAsync()` para elegir empresa; usar la del contexto |
| `Web/Pages/Config/Empresas.razor` | Restringir a Admin o devolver solo las asignadas |

---

### 🟢 Prioridad 2 — Endurecer auditoría

| Archivo | Acción |
|---|---|
| `Persistence/Services/AuditLogService.cs` | Inyectar `ICurrentUserService`; aplicar filtro implícito por empresa salvo Admin |
| `Api/Controllers/V1/AuditLogsController.cs` | Policy: Admin para acceso global; no aceptar `EmpresaId` arbitrario |

---

### 🟢 Prioridad 2 — Revisión arquitectónica amplia

Buscar y revisar patrones similares:

```
# Usos de IgnoreQueryFilters
rg "IgnoreQueryFilters" src/

# Acceso directo al DbContext sin scoping explícito
rg "AgoraDbContext _context" src/

# Servicios que aceptan EmpresaId en DTO/ruta sin validación
rg "dto\.EmpresaId" src/Application/

# Entidades no-tenant consultadas directamente
rg "FindIgnoreQueryFilters\|GetByIdIgnoreQueryFilters" src/
```

---

## 9. Riesgos arquitectónicos amplios

| # | Riesgo | Archivos involucrados |
|---|---|---|
| R1 | **Inconsistencia empresa activa UI vs token** | `AuthService.cs`, `EmpresaStateService.cs`, `MainLayout.razor` |
| R2 | **Bypass deliberado de filtros globales** | `Repository.cs:25-38`, `SucursalService.cs:29-31` |
| R3 | **Exposición de catálogos globales a usuarios no admin** | `EmpresasController.cs`, `UsuariosController.cs`, `RolesController.cs`, `AuditLogsController.cs` |
| R4 | **Middleware tenant solo cubre writes, no reads** | `TenantRequiredMiddleware.cs:49-59` |
| R5 | **Modelo multiempresa comprimido a un claim único** | `AuthService.cs:92-101`, `UsuarioEmpresa.cs` |
| R6 | **`EmpresaActivaId` no se setea en alta/modificación de usuario** | `UsuarioService.cs:69-76`, `119-125` |
| R7 | **Fallback a "primera empresa del sistema" en login** | `AuthService.cs:54-59` |
| R8 | **Endpoint duplicado "mis empresas" con comportamiento inconsistente** | `AuthController.cs:73-103`, `EmpresasController.cs:43-60` |

---

## 10. Resumen de archivos a intervenir

### Correcciones críticas (Prioridad 0)

```
src/AgoraHub360.ERP.Api/Services/AuthService.cs
src/AgoraHub360.ERP.Api/Controllers/V1/AuthController.cs
src/AgoraHub360.ERP.Application/Services/SucursalService.cs
src/AgoraHub360.ERP.Application/Services/UsuarioService.cs
src/AgoraHub360.ERP.Api/Controllers/V1/SucursalesController.cs
src/AgoraHub360.ERP.Domain/Entities/Core/Usuario.cs
src/AgoraHub360.ERP.Web/Layout/MainLayout.razor
src/AgoraHub360.ERP.Web/Services/EmpresaStateService.cs
src/AgoraHub360.ERP.Web/Services/JwtAuthStateProvider.cs
src/AgoraHub360.ERP.Web/Services/SesionUsuarioStateService.cs
src/AgoraHub360.ERP.Web/Pages/Config/SucursalForm.razor
```

### Correcciones importantes (Prioridad 1)

```
src/AgoraHub360.ERP.Api/Controllers/V1/EmpresasController.cs
src/AgoraHub360.ERP.Api/Controllers/V1/UsuariosController.cs
src/AgoraHub360.ERP.Api/Controllers/V1/RolesController.cs
src/AgoraHub360.ERP.Api/Controllers/V1/AuditLogsController.cs
src/AgoraHub360.ERP.Application/Services/EmpresaService.cs
src/AgoraHub360.ERP.Web/Pages/Config/Usuarios.razor
src/AgoraHub360.ERP.Web/Pages/Config/Sucursales.razor
src/AgoraHub360.ERP.Web/Pages/Config/ParametrosNumeracion.razor
src/AgoraHub360.ERP.Web/Pages/Config/Empresas.razor
```

### Mejoras menores (Prioridad 2)

```
src/AgoraHub360.ERP.Persistence/Services/AuditLogService.cs
src/AgoraHub360.ERP.Persistence/Repositories/Repository.cs
src/AgoraHub360.ERP.Api/Middleware/TenantRequiredMiddleware.cs
```

---

*Análisis generado automáticamente a partir del código fuente del repositorio. No se realizaron cambios de código.*
