# Plan de Implementación P0 — Consistencia Multiempresa / Empresa / Sucursal

**Proyecto:** AgoraHUB360 ERP  
**Stack:** .NET 8 · C# 12 · ASP.NET Core Web API · Blazor WebAssembly · EF Core 8 · SQL Server  
**Herramienta objetivo:** GitHub Copilot en Visual Studio 2026  
**Objetivo:** corregir la inconsistencia de manejo independiente multiempresa, especialmente la divergencia entre empresa activa del frontend, claim `EmpresaId` del JWT, scoping backend, sucursales, usuarios, empresas y auditoría.

---

## 1. Diagnóstico base

El proyecto ya posee infraestructura multiempresa real: entidades `TenantEntity` con `EmpresaId`, query filters globales en EF Core y servicios que validan empresa activa. El problema es que el aislamiento tenant está implementado de forma inconsistente: el backend toma el tenant efectivo desde el claim `EmpresaId` del JWT, mientras que el selector de empresa en Blazor solo cambia estado local/localStorage y no renueva el token. Además, sucursales, empresas, usuarios y auditoría presentan bypass o exposición global.

### Problema principal

```text
Frontend muestra empresa B
        ↓
localStorage guarda empresa B
        ↓
JWT sigue con EmpresaId = empresa A
        ↓
CurrentUserService devuelve empresa A
        ↓
AgoraDbContext filtra por empresa A
        ↓
El usuario cree operar en B, pero backend opera en A
```

### Objetivo técnico final

```text
Empresa activa UI = Empresa activa JWT = Empresa activa backend = Empresa activa EF Core = Empresa activa permisos = Empresa activa sucursal
```

---

## 2. Regla de implementación

Este plan sigue MAPE: primero arquitectura, luego desarrollo por capas, luego validación controlada.

No implementar todo de una vez. Trabajar por fases pequeñas y verificables.

---

## 3. Rama de trabajo recomendada

```bash
git checkout main
git pull
git checkout -b fix/p0-consistencia-multiempresa-sucursal
```

Commits sugeridos:

```text
fix(auth): enforce tenant selection and remove unsafe company fallback
fix(auth): add empresa selection endpoint and token regeneration
fix(web): synchronize company switch with backend jwt refresh
fix(sucursales): remove tenant bypass and scope branch management
fix(security): restrict global company user role audit endpoints
fix(tests): add tenant isolation regression tests
```

---

# FASE 0 — Auditoría inicial sin cambios

## Objetivo

Confirmar el estado real del código antes de modificarlo.

## Archivos a revisar

```text
src/AgoraHub360.ERP.Api/Services/AuthService.cs
src/AgoraHub360.ERP.Api/Controllers/V1/AuthController.cs
src/AgoraHub360.ERP.Api/Services/CurrentUserService.cs
src/AgoraHub360.ERP.Persistence/Context/AgoraDbContext.cs
src/AgoraHub360.ERP.Persistence/Repositories/Repository.cs
src/AgoraHub360.ERP.Application/Services/SucursalService.cs
src/AgoraHub360.ERP.Api/Controllers/V1/SucursalesController.cs
src/AgoraHub360.ERP.Application/Services/UsuarioService.cs
src/AgoraHub360.ERP.Domain/Entities/Core/Usuario.cs
src/AgoraHub360.ERP.Web/Layout/MainLayout.razor
src/AgoraHub360.ERP.Web/Services/EmpresaStateService.cs
src/AgoraHub360.ERP.Web/Services/JwtAuthStateProvider.cs
src/AgoraHub360.ERP.Web/Services/SesionUsuarioStateService.cs
src/AgoraHub360.ERP.Persistence/Services/AuditLogService.cs
```

## Comandos de búsqueda

```powershell
rg "IgnoreQueryFilters" src/
rg "FindIgnoreQueryFilters|GetByIdIgnoreQueryFilters" src/
rg "dto\.EmpresaId" src/Application src/AgoraHub360.ERP.Application
rg "EmpresaActivaId" src/
rg "mis-empresas" src/
rg "localStorage.*empresa|empresa_activa|EmpresaState" src/AgoraHub360.ERP.Web
```

## Prompt Copilot 0.1 — Auditoría de contexto

```text
@workspace Analiza el flujo multiempresa actual del proyecto AgoraHUB360 ERP sin modificar código.

Revisa especialmente:
- AuthService.cs
- AuthController.cs
- CurrentUserService.cs
- AgoraDbContext.cs
- Repository.cs
- SucursalService.cs
- SucursalesController.cs
- UsuarioService.cs
- Usuario.cs
- MainLayout.razor
- EmpresaStateService.cs
- JwtAuthStateProvider.cs
- SesionUsuarioStateService.cs
- AuditLogService.cs

Devuélveme:
1. Cómo se resuelve actualmente EmpresaId.
2. Dónde se genera el JWT.
3. Dónde se lee EmpresaId desde claims.
4. Dónde se cambia la empresa en frontend.
5. Dónde se usa IgnoreQueryFilters.
6. Qué endpoints exponen datos globales.
7. Qué cambios mínimos recomiendas para que UI, JWT, backend y EF Core usen la misma empresa activa.

No hagas cambios de código todavía.
```

## Resultado esperado

Un resumen dentro de Copilot/IDE confirmando las rutas y métodos reales antes de tocar código.

---

# FASE 1 — Contratos Shared para empresa activa y sesión

## Objetivo

Crear contratos compartidos para seleccionar/cambiar empresa y devolver un contexto de autenticación consistente.

## Decisión arquitectónica

Agregar DTOs en `AgoraHub360.ERP.Shared` para no acoplar Web y API a entidades de dominio.

## DTOs sugeridos

```text
Shared/DTOs/Auth/SeleccionarEmpresaRequestDto.cs
Shared/DTOs/Auth/EmpresaSesionDto.cs
Shared/DTOs/Auth/SucursalSesionDto.cs
Shared/DTOs/Auth/CambiarEmpresaResponseDto.cs
```

### Estructura sugerida

```csharp
public sealed class SeleccionarEmpresaRequestDto
{
    public int EmpresaId { get; set; }
}

public sealed class EmpresaSesionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Nit { get; set; }
    public string Rol { get; set; } = string.Empty;
    public bool EsActiva { get; set; }
}

public sealed class SucursalSesionDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsCentral { get; set; }
    public bool EsPredeterminada { get; set; }
}

public sealed class CambiarEmpresaResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public EmpresaSesionDto EmpresaActiva { get; set; } = default!;
    public IReadOnlyList<EmpresaSesionDto> EmpresasDisponibles { get; set; } = Array.Empty<EmpresaSesionDto>();
    public IReadOnlyList<SucursalSesionDto> SucursalesDisponibles { get; set; } = Array.Empty<SucursalSesionDto>();
}
```

## Prompt Copilot 1.1 — Crear DTOs

```text
Crea DTOs compartidos para el flujo de selección y cambio de empresa activa en AgoraHub360.ERP.Shared.

Ubicación sugerida:
src/AgoraHub360.ERP.Shared/DTOs/Auth/

Crear:
1. SeleccionarEmpresaRequestDto con EmpresaId:int.
2. EmpresaSesionDto con Id, Nombre, Nit nullable, Rol, EsActiva.
3. SucursalSesionDto con Id, Codigo, Nombre, EsCentral, EsPredeterminada.
4. CambiarEmpresaResponseDto con Token, Expiration, EmpresaActiva, EmpresasDisponibles y SucursalesDisponibles.

Reglas:
- No usar entidades Domain.
- Mantener clases simples, serializables y compatibles con Blazor WASM.
- Usar propiedades inicializadas para evitar null warnings.
- No modificar todavía AuthService ni controladores.
```

## Validación

```bash
dotnet build
```

---

# FASE 2 — Backend Auth: eliminar fallback inseguro y generar token por empresa validada

## Objetivo

Hacer que el login y cambio de empresa nunca asignen una empresa global arbitraria.

## Reglas

1. Nunca usar “primera empresa activa del sistema” como fallback.
2. El usuario solo puede operar empresas existentes en `UsuarioEmpresa`.
3. El JWT debe incluir la empresa seleccionada.
4. El rol del JWT debe corresponder a la empresa seleccionada.
5. `Usuario.EmpresaActivaId` debe persistirse cuando se cambia empresa.

## Lógica esperada de login

```text
Login(email,password)
    ↓
Validar usuario y password
    ↓
Cargar empresas asignadas activas
    ↓
Si no tiene empresas asignadas:
    devolver error claro
    ↓
Si EmpresaActivaId existe y pertenece al usuario:
    usar EmpresaActivaId
Sino:
    usar primera empresa asignada activa SOLO del usuario
    persistirla como EmpresaActivaId
    ↓
Generar JWT con EmpresaId y Rol de ESA empresa
```

> Nota: una versión más estricta puede requerir empresa explícita cuando el usuario tenga varias empresas. Para una corrección incremental segura, basta eliminar el fallback global y agregar cambio explícito de empresa.

## Prompt Copilot 2.1 — Refactor AuthService

```text
Modifica AuthService.cs para corregir la resolución de empresa activa en login.

Objetivo:
- Eliminar completamente el fallback a la primera empresa activa del sistema.
- La empresa activa debe salir únicamente de Usuario.EmpresaActivaId si pertenece al usuario, o de una empresa asignada al usuario mediante UsuarioEmpresa.
- Si el usuario no tiene empresas asignadas activas, devolver error claro usando el patrón Result existente del proyecto.
- El rol del JWT debe corresponder a la empresa realmente seleccionada.
- Si Usuario.EmpresaActivaId es null o inválida, usar la primera empresa asignada activa del propio usuario y persistirla como EmpresaActivaId.
- No modificar todavía el frontend.

Revisa los nombres reales de métodos, repositorios y DTOs antes de escribir código.
Mantén Clean Architecture y no introduzcas dependencias desde Domain hacia Infrastructure.
```

## Prompt Copilot 2.2 — Extraer método GenerateJwt por empresa

```text
En AuthService.cs, refactoriza la generación del JWT para que reciba explícitamente:
- Usuario usuario
- int empresaId
- string rol

El token debe incluir claims existentes y además EmpresaId con la empresa seleccionada.
Asegúrate de que el rol usado sea el rol de UsuarioEmpresa para esa empresa, no el primer rol encontrado.

Si ya existe un método GenerateJwtToken, ajusta su firma con el menor cambio posible y actualiza llamadas internas.
No cambies contratos públicos innecesariamente.
```

## Prompt Copilot 2.3 — Persistir EmpresaActivaId

```text
Ajusta AuthService.cs y, si corresponde, UsuarioService.cs para asegurar que Usuario.EmpresaActivaId quede persistida cuando:
1. El login detecta que no hay empresa activa pero sí hay empresas asignadas al usuario.
2. El usuario cambia explícitamente de empresa.

Reglas:
- Validar siempre que EmpresaActivaId pertenezca a UsuarioEmpresa.
- No asignar empresas globales.
- No aceptar EmpresaId desde cliente sin validar relación usuario-empresa.
- Usar repositorios/persistencia existentes del proyecto.
```

## Validación backend

```bash
dotnet build
dotnet test
```

---

# FASE 3 — Endpoint explícito para seleccionar/cambiar empresa

## Objetivo

Agregar endpoint backend para cambiar empresa activa y devolver nuevo JWT.

## Endpoint recomendado

```http
POST /api/v1/auth/seleccionar-empresa
Authorization: Bearer <token_actual>
Content-Type: application/json

{
  "empresaId": 2
}
```

## Respuesta esperada

```json
{
  "token": "jwt_nuevo",
  "expiration": "2026-05-17T23:59:00Z",
  "empresaActiva": { "id": 2, "nombre": "Empresa B", "rol": "Admin", "esActiva": true },
  "empresasDisponibles": [],
  "sucursalesDisponibles": []
}
```

## Prompt Copilot 3.1 — Método de servicio CambiarEmpresaAsync

```text
Agrega en AuthService.cs un método de aplicación para cambiar empresa activa del usuario autenticado.

Nombre sugerido:
CambiarEmpresaActivaAsync(int usuarioId, int empresaId, CancellationToken ct)

o usa la convención real del proyecto.

Debe:
1. Cargar el usuario autenticado.
2. Validar que empresaId pertenezca al usuario en UsuarioEmpresa.
3. Validar que la empresa esté activa.
4. Obtener el rol correspondiente a esa empresa.
5. Actualizar Usuario.EmpresaActivaId.
6. Generar un nuevo JWT con EmpresaId y rol correctos.
7. Devolver CambiarEmpresaResponseDto con token, expiration, empresa activa, empresas disponibles y sucursales disponibles si existen servicios disponibles.

No usar IgnoreQueryFilters salvo que ya sea estrictamente necesario para Empresa/Usuario por no heredar TenantEntity, y en ese caso validar manualmente la relación usuario-empresa.
```

## Prompt Copilot 3.2 — Endpoint en AuthController

```text
Agrega en AuthController.cs un endpoint versionado y autenticado:
POST /api/v1/auth/seleccionar-empresa

Debe recibir SeleccionarEmpresaRequestDto y llamar al método de AuthService para cambiar empresa activa.
Debe tomar el usuario autenticado desde claims, no desde el body.
Debe devolver 200 con CambiarEmpresaResponseDto si todo es correcto.
Debe devolver 400/403 si la empresa no pertenece al usuario.
Debe mantener el patrón de respuestas existente del proyecto.

No crear un controlador nuevo si AuthController ya maneja autenticación.
```

## Prompt Copilot 3.3 — Unificar mis-empresas

```text
Revisa la duplicación de endpoints:
- GET /api/v1/auth/mis-empresas
- GET /api/v1/empresas/mis-empresas

Propón y aplica una consolidación mínima sin romper el frontend:
- Mantén uno como endpoint canónico.
- Si decides conservar ambos temporalmente, ambos deben usar la misma lógica interna.
- Usuario normal solo ve sus empresas asignadas.
- Admin global solo ve todas si existe una policy explícita o regla ya definida.

No expongas empresas globales a usuarios normales.
```

---

# FASE 4 — Frontend: sincronizar selector de empresa con backend/JWT

## Objetivo

El cambio de empresa en Blazor debe llamar al backend, recibir token nuevo y refrescar contexto completo.

## Flujo esperado

```text
Usuario selecciona empresa
    ↓
EmpresaStateService.CambiarEmpresaAsync(empresaId)
    ↓
POST /api/v1/auth/seleccionar-empresa
    ↓
Recibe token nuevo
    ↓
JwtAuthStateProvider.ReplaceTokenAsync(token)
    ↓
HttpClient Authorization header actualizado
    ↓
SesionUsuarioStateService.InitializeAsync(forceReload: true)
    ↓
Menú, permisos, sucursales y datos cargan para nueva empresa
```

## Prompt Copilot 4.1 — JwtAuthStateProvider ReplaceTokenAsync

```text
Modifica JwtAuthStateProvider.cs para soportar reemplazo seguro del JWT cuando el backend devuelve un token nuevo por cambio de empresa.

Crear o ajustar método:
ReplaceTokenAsync(string token)

Debe:
1. Guardar el token nuevo en localStorage con la clave ya usada por el proyecto.
2. Actualizar el AuthenticationState.
3. Actualizar el Authorization header del HttpClient si este provider lo maneja actualmente.
4. Notificar NotifyAuthenticationStateChanged.
5. No cerrar sesión.

Respeta los nombres reales de métodos existentes como MarkUserAsAuthenticated, NotifyUserLogout, etc.
```

## Prompt Copilot 4.2 — EmpresaStateService conectado al backend

```text
Refactoriza EmpresaStateService.cs para que el cambio de empresa no sea solo localStorage.

Objetivo:
- Agregar método CambiarEmpresaActivaAsync(int empresaId).
- Este método debe llamar a POST /api/v1/auth/seleccionar-empresa.
- Al recibir respuesta, debe usar JwtAuthStateProvider.ReplaceTokenAsync(token).
- Debe actualizar la empresa activa local solo después de éxito del backend.
- Debe disparar OnChange solo después de actualizar token y empresa activa.
- Mantener compatibilidad con el estado actual si hay métodos usados por MainLayout.

No permitir que la UI crea que cambió de empresa si backend devuelve error.
```

## Prompt Copilot 4.3 — MainLayout selector de empresa

```text
Modifica MainLayout.razor para que el selector de empresa use el nuevo flujo backend.

Cuando el usuario seleccione otra empresa:
1. Mostrar estado de carga o bloquear selector temporalmente.
2. Llamar EmpresaStateService.CambiarEmpresaActivaAsync(empresaId).
3. Si hay éxito, recargar SesionUsuarioStateService.InitializeAsync(forceReload: true).
4. Limpiar o recargar datos dependientes de empresa si existen.
5. Redirigir al dashboard o refrescar menú según patrón actual.
6. Si falla, mostrar mensaje y mantener empresa anterior.

No guardar solo localStorage como fuente de verdad.
```

## Prompt Copilot 4.4 — SesionUsuarioStateService reload post-token

```text
Revisa SesionUsuarioStateService.cs y ajusta el método InitializeAsync(forceReload: true) para que, después de reemplazar el token por cambio de empresa, recargue:
- contexto de usuario,
- perfiles,
- permisos,
- menú,
- sucursales disponibles,
- sucursal predeterminada.

Debe limpiar el contexto anterior antes de aplicar el nuevo para evitar mezclar datos de empresas.
No uses datos cacheados de la empresa anterior si forceReload es true.
```

---

# FASE 5 — Sucursales tenant-safe

## Objetivo

Cerrar la brecha más crítica: sucursales no deben aceptar empresa arbitraria ni usar bypass de filtros globales para usuarios normales.

## Cambios clave

### Antes

```text
GetAllByEmpresaAsync(int empresaId)
FindIgnoreQueryFiltersAsync(s => s.EmpresaId == empresaId)
CrearSucursalDto.EmpresaId desde cliente
GetByIdIgnoreQueryFiltersAsync(id)
```

### Después

```text
GetMisSucursalesAsync()
Usar empresa actual desde ICurrentUserService
Usar query filters normales
Crear sucursal con EmpresaId del contexto actual
Validar permisos para admin global si se crea para otra empresa
```

## Prompt Copilot 5.1 — Refactor SucursalService tenant-aware

```text
Refactoriza SucursalService.cs para corregir aislamiento multiempresa.

Reglas obligatorias:
1. No usar GetByIdIgnoreQueryFiltersAsync ni FindIgnoreQueryFiltersAsync para operaciones normales de usuario.
2. Inferir EmpresaId desde ICurrentUserService.EmpresaId.
3. Si no hay EmpresaId en sesión, devolver error claro.
4. GetAll debe devolver sucursales de la empresa activa usando filtros globales EF Core.
5. GetById, Update, Delete, CambiarEstado y EstablecerCentral deben validar que la sucursal pertenece a la empresa activa.
6. Create no debe confiar en dto.EmpresaId para usuario normal; debe usar EmpresaId del contexto actual.
7. Si existe rol Admin global y se requiere crear sucursal para otra empresa, encapsularlo en método separado con policy explícita, no en el flujo normal.
8. Mantener DTOs existentes en lo posible para no romper UI, pero ignorar o validar EmpresaId del DTO.

Devuelve errores con el patrón Result existente.
```

## Prompt Copilot 5.2 — SucursalesController scoped

```text
Modifica SucursalesController.cs para exponer endpoints tenant-safe.

Recomendado:
- GET /api/v1/sucursales -> sucursales de empresa activa.
- GET /api/v1/sucursales/{id} -> sucursal solo si pertenece a empresa activa.
- POST /api/v1/sucursales -> crear sucursal para empresa activa.
- PUT /api/v1/sucursales/{id} -> actualizar solo dentro de empresa activa.
- DELETE /api/v1/sucursales/{id} -> eliminar/desactivar solo dentro de empresa activa.

Evitar endpoint público que reciba empresaId arbitrario.
Si hay endpoints por empresaId existentes, marcarlos como admin-only o mantenerlos temporalmente con validación estricta de pertenencia/policy.
```

## Prompt Copilot 5.3 — DTO CrearSucursalDto

```text
Revisa CrearSucursalDto.cs y ActualizarSucursalDto.cs.

Objetivo:
- Evitar que EmpresaId enviado desde cliente controle el tenant de la sucursal para usuarios normales.
- Si no se puede eliminar EmpresaId por compatibilidad, documentar en comentarios que será ignorado en flujo normal y validado solo para Admin global.
- Ajustar mapeos de SucursalService para asignar EmpresaId desde ICurrentUserService.
- No romper las páginas actuales sin ajustar frontend.
```

## Prompt Copilot 5.4 — Frontend sucursales

```text
Actualiza las páginas y servicios Blazor de sucursales para consumir endpoints scoped.

Archivos a revisar:
- Web/Services/HttpSucursalService.cs
- Web/Pages/Config/Sucursales.razor
- Web/Pages/Config/SucursalForm.razor

Cambios:
1. No cargar todas las empresas para filtrar sucursales en cliente.
2. Usuario normal no debe ver selector de empresa en formulario de sucursal.
3. La sucursal se crea para la empresa activa del token.
4. Si existe rol Admin global, mostrar selector de empresa solo bajo policy/rol explícito.
5. Después de cambiar empresa, recargar lista de sucursales desde API.
```

---

# FASE 6 — Endpoints globales: empresas, usuarios, roles, auditoría

## Objetivo

Evitar exposición cross-tenant de metadatos administrativos.

## Reglas

| Recurso | Usuario normal | Admin empresa | Admin global |
|---|---|---|---|
| Empresas | Solo asignadas | Solo asignadas/administra | Todas |
| Usuarios | Solo empresa activa si tiene permiso | Empresa activa | Todas |
| Roles | Roles de empresa o sistema según policy | Gestiona dentro de empresa | Global |
| Auditoría | Solo empresa activa | Empresa activa | Global |

## Prompt Copilot 6.1 — EmpresasController/EmpresaService

```text
Refactoriza EmpresasController.cs y EmpresaService.cs para evitar exposición global de empresas.

Reglas:
1. GET /api/v1/empresas para usuario normal debe devolver solo empresas asignadas al usuario o requerir policy AdminGlobal.
2. GET /api/v1/empresas/mis-empresas debe ser el endpoint recomendado para selector de empresa.
3. Si existe acceso global, protegerlo con policy/rol explícito.
4. No devolver todas las empresas a usuarios autenticados comunes.
5. Mantener compatibilidad mínima con frontend, ajustando llamadas si corresponde.
```

## Prompt Copilot 6.2 — UsuariosController/UsuarioService

```text
Refactoriza UsuariosController.cs y UsuarioService.cs para que usuarios normales o admins de empresa no reciban usuarios de otros tenants.

Reglas:
1. El listado normal de usuarios debe estar filtrado por empresa activa.
2. Usuarios globales solo con policy AdminGlobal.
3. Al crear usuario, sus empresas asignadas deben validarse según permisos del actor.
4. Usuario.EmpresaActivaId debe setearse si se asigna una empresa inicial válida.
5. No filtrar en frontend lo que backend no debió enviar.
```

## Prompt Copilot 6.3 — RolesController policy

```text
Revisa RolesController.cs y aplica policy explícita para gestión de roles.

Objetivo:
- No exponer catálogo global de roles a cualquier usuario autenticado.
- Si los roles son globales del sistema, solo AdminGlobal puede administrarlos.
- Si existen perfiles por empresa, usuario debe consultar perfiles/roles de empresa activa.
- Mantener endpoints necesarios para menú/permisos sin exponer administración global.
```

## Prompt Copilot 6.4 — Auditoría tenant-aware

```text
Refactoriza AuditLogService.cs y AuditLogsController.cs para aplicar scoping por empresa.

Reglas:
1. Inyectar ICurrentUserService en AuditLogService o capa correspondiente.
2. Usuario normal/admin empresa solo puede consultar logs de EmpresaId del JWT.
3. No aceptar EmpresaId arbitrario desde query para usuarios no globales.
4. AdminGlobal puede consultar todas o filtrar por empresa explícita.
5. Mantener paginación/filtros existentes.
6. No romper estructura de DTOs actuales si no es necesario.
```

---

# FASE 7 — Middleware tenant y defensa en profundidad

## Objetivo

El middleware no debe ser la única barrera, pero debe reforzar lecturas/escrituras críticas.

## Prompt Copilot 7.1 — TenantRequiredMiddleware

```text
Revisa TenantRequiredMiddleware.cs y mejora su rol como defensa en profundidad.

Objetivo:
- No depender solo del middleware para tenant isolation.
- Asegurar que rutas tenant-aware autenticadas tengan EmpresaId en claims.
- Evaluar protección también para GET en rutas de negocio, no solo POST/PUT/DELETE.
- Mantener exenciones claras para login, health, swagger y endpoints globales protegidos.
- No bloquear endpoints públicos necesarios.

Devuelve primero propuesta y luego aplica cambios mínimos seguros.
```

---

# FASE 8 — Pruebas de regresión multiempresa

## Objetivo

Probar que no vuelve a ocurrir fuga cross-tenant.

## Casos obligatorios

1. Usuario A con empresa 1 no ve sucursales de empresa 2.
2. Usuario multiempresa cambia de empresa y el JWT nuevo cambia `EmpresaId`.
3. Después de cambio de empresa, `contexto-sesion` devuelve sucursales de la empresa nueva.
4. Crear sucursal no permite enviar `EmpresaId` distinto al del JWT.
5. Auditoría no devuelve logs de otra empresa para usuario común.
6. `EmpresasController.GetAll` no devuelve todas las empresas a usuario normal.
7. Login de usuario sin empresa asignada falla con error claro.
8. Login no usa fallback a primera empresa del sistema.

## Prompt Copilot 8.1 — Tests Auth tenant

```text
Agrega pruebas automatizadas para el flujo de autenticación multiempresa.

Casos:
1. Login de usuario sin UsuarioEmpresa debe fallar con error claro.
2. Login no debe usar la primera empresa activa del sistema como fallback.
3. Login de usuario con empresa asignada genera JWT con EmpresaId correcto.
4. Cambiar empresa con empresa no asignada devuelve 403 o Result failure.
5. Cambiar empresa con empresa asignada devuelve token nuevo con claim EmpresaId actualizado.

Usa el proyecto AgoraHub360.ERP.Tests y el patrón de testing existente.
No introducir frameworks nuevos salvo que ya existan.
```

## Prompt Copilot 8.2 — Tests Sucursal tenant

```text
Agrega pruebas para SucursalService y/o SucursalesController.

Casos:
1. GetAll devuelve solo sucursales de EmpresaId del usuario actual.
2. GetById de sucursal de otra empresa devuelve NotFound o Forbidden.
3. Create ignora o rechaza dto.EmpresaId distinto al claim y crea con EmpresaId del contexto.
4. Update/Delete no pueden afectar sucursal de otra empresa.
5. No se usan métodos IgnoreQueryFilters en el flujo normal.

Mantén Clean Architecture y usa mocks/fakes según patrón existente del proyecto.
```

## Prompt Copilot 8.3 — Tests Frontend estado empresa

```text
Revisa si el proyecto tiene pruebas para Blazor/Web. Si existen, agrega pruebas para EmpresaStateService y JwtAuthStateProvider.

Casos:
1. CambiarEmpresaActivaAsync llama al backend.
2. Si backend responde éxito, reemplaza token y actualiza empresa activa.
3. Si backend falla, mantiene empresa anterior.
4. forceReload en SesionUsuarioStateService limpia contexto anterior.

Si no existe infraestructura de pruebas frontend, crea una nota TODO técnica documentada y no agregues dependencias nuevas sin aprobación.
```

---

# FASE 9 — Validación manual en Visual Studio 2026

## Escenario mínimo

Crear o usar datos:

```text
Empresa A
  Sucursal A1
  Cliente A1

Empresa B
  Sucursal B1
  Cliente B1

Usuario multiempresa
  Asignado a Empresa A y Empresa B
```

## Checklist manual

1. Login con usuario multiempresa.
2. Confirmar empresa activa inicial.
3. Ver sucursales: solo empresa activa.
4. Cambiar empresa desde selector.
5. Abrir DevTools o logs: confirmar llamada `POST /auth/seleccionar-empresa`.
6. Confirmar que token cambia.
7. Confirmar que `/contexto-sesion` devuelve nueva empresa/sucursales.
8. Ver sucursales: solo nueva empresa.
9. Intentar abrir URL con `sucursalId` de otra empresa: debe fallar.
10. Intentar crear sucursal con EmpresaId manipulado en request: backend debe ignorar/rechazar.
11. Revisar auditoría: usuario normal no ve logs globales.
12. Revisar usuarios/empresas: usuario normal no ve catálogo global.

---

# FASE 10 — Documentación y ADR

## Objetivo

Dejar decisión arquitectónica documentada.

## Archivo recomendado

```text
docs/adr/ADR-0001-tenant-activo-jwt-selector-empresa.md
```

## Prompt Copilot 10.1 — ADR tenant activo

```text
Crea un ADR en docs/adr/ADR-0001-tenant-activo-jwt-selector-empresa.md.

Debe documentar:
1. Contexto: inconsistencia entre frontend localStorage y JWT EmpresaId.
2. Decisión: el tenant efectivo del backend es EmpresaId del JWT; el selector de empresa debe renovar token vía backend.
3. Alternativas consideradas: localStorage only, header X-EmpresaId, token por empresa.
4. Decisión adoptada: endpoint seleccionar-empresa, validación UsuarioEmpresa, persistencia EmpresaActivaId, regeneración de JWT.
5. Consecuencias positivas.
6. Riesgos.
7. Reglas de implementación.
8. Checklist de validación.

Usa lenguaje técnico claro y alineado a Clean Architecture y MAPE.
```

---

# Orden recomendado de ejecución con Copilot

1. Prompt 0.1 — auditoría.
2. Prompt 1.1 — DTOs.
3. Prompt 2.1 — login sin fallback global.
4. Prompt 2.2 — token por empresa validada.
5. Prompt 2.3 — persistir EmpresaActivaId.
6. Prompt 3.1 — método cambiar empresa.
7. Prompt 3.2 — endpoint seleccionar empresa.
8. Prompt 4.1 — ReplaceTokenAsync.
9. Prompt 4.2 — EmpresaStateService backend.
10. Prompt 4.3 — MainLayout selector.
11. Prompt 4.4 — recarga contexto sesión.
12. Prompt 5.1 — SucursalService scoped.
13. Prompt 5.2 — SucursalesController scoped.
14. Prompt 5.3 — DTO sucursal.
15. Prompt 5.4 — UI sucursales.
16. Prompt 6.1–6.4 — endpoints globales.
17. Prompt 8.1–8.3 — tests.
18. Prompt 10.1 — ADR.

---

# Definición de terminado P0

La corrección P0 se considera terminada cuando:

- El login no usa primera empresa global del sistema.
- El usuario solo recibe token para empresas asignadas.
- El cambio de empresa llama backend.
- El backend valida UsuarioEmpresa.
- El backend regenera JWT.
- Frontend reemplaza token.
- Contexto de sesión se recarga.
- Sucursales se filtran por empresa activa.
- No se puede manipular sucursal cross-tenant.
- Empresas/usuarios/roles/auditoría no exponen datos globales a usuarios comunes.
- Existen pruebas de regresión multiempresa.
- Existe ADR documentando la decisión.

---

# Prompt maestro para usar al inicio de cada sesión Copilot

```text
Contexto del proyecto AgoraHUB360 ERP:

Estoy corrigiendo el problema P0 de consistencia multiempresa/tenant.
El proyecto usa .NET 8, C# 12, ASP.NET Core Web API, Blazor WebAssembly, EF Core 8, SQL Server, Clean Architecture y DDD ligero.
La arquitectura tiene proyectos Domain, Application, Persistence, Infrastructure, Api, Web, Shared y Tests.

Problema:
El backend usa EmpresaId desde JWT como tenant efectivo, pero el frontend cambia empresa solo en localStorage. Esto causa divergencia entre empresa activa UI, JWT, backend y EF Core. Además, SucursalService usa IgnoreQueryFilters y hay endpoints globales de empresas/usuarios/roles/auditoría.

Reglas obligatorias:
1. No usar primera empresa activa del sistema como fallback de login.
2. Validar empresa contra UsuarioEmpresa.
3. Regenerar JWT al cambiar empresa.
4. Empresa activa debe estar sincronizada entre UI, JWT, backend, EF Core y permisos.
5. No aceptar EmpresaId desde DTO para definir tenant en flujo normal.
6. No usar IgnoreQueryFilters salvo admin global con validación explícita.
7. Mantener Clean Architecture.
8. No poner lógica de negocio en controllers.
9. Usar DTOs Shared para API/Web.
10. Agregar pruebas de regresión multiempresa.

Antes de modificar código, revisa los archivos reales y adapta nombres/métodos existentes. No inventes patrones que no existan en el proyecto.
```
