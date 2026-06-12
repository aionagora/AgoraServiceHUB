# 06 — Cliente Blazor WebAssembly (Web)

> **Proyecto:** AgoraHub360.ERP.Web  
> **Última actualización:** 2026-06-11  
> **Propósito:** UI Blazor WebAssembly, servicios HTTP, autenticación JWT, menú dinámico.

---

## 1. Estructura del Proyecto

```
src/AgoraHub360.ERP.Web/
├── Layout/
│   ├── MainLayout.razor         ← Layout principal con sidebar
│   ├── MainLayout.razor.css
│   └── NavMenu.razor             ← Menú de navegación (estático + dinámico)
├── Pages/
│   ├── Login.razor
│   ├── Dashboard.razor
│   ├── AccesosDirectos.razor
│   ├── Config/                   ← Configuración global y de empresa
│   ├── Configuracion/            ← Configuración FE, Workflow
│   ├── Ventas/                   ← Ventas, Facturas, Pedidos, Pagos, CxC
│   ├── MDM/                      ← Maestros: Clientes, Productos, Proveedores
│   ├── Inventario/               ← Stock, Kardex, Movimientos
│   ├── Compras/                  ← Órdenes, Recepciones, Importaciones
│   ├── Contabilidad/             ← Asientos, Plan Cuentas, Estados Financieros
│   └── Dashboards/               ← Dashboards por módulo
├── Services/                     ← Servicios HTTP (~50 servicios)
├── Shared/                       ← Componentes compartidos (modales, dropdowns)
├── wwwroot/                      ← Estáticos
└── Program.cs                    ← Punto de entrada Blazor WASM
```

---

## 2. Program.cs

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// HttpClient con AuthMessageHandler para JWT
builder.Services.AddTransient<AuthMessageHandler>();
builder.Services.AddScoped(sp => new HttpClient(handler)
{
    BaseAddress = new Uri(apiBaseUrl)
});

// Autenticación
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());

// ~50 Servicios HTTP registrados como Scoped
builder.Services.AddScoped<AuthHttpService>();
builder.Services.AddScoped<FacturacionFEHttpService>();
// ... otros servicios ...
```

---

## 3. Servicios HTTP (~50)

| Servicio | Ruta base |
|---|---|
| `AuthHttpService` | `api/v1/auth` |
| `FacturacionFEHttpService` | `api/v1/facturacion-electronica` |
| `VentaHttpService` | `api/v1/ventas` |
| `FacturaVentaHttpService` | `api/v1/facturas-venta` |
| `PedidoVentaHttpService` | `api/v1/ventas/pedidos` |
| `CuentasPorCobrarHttpService` | `api/v1/cuentas-por-cobrar` |
| `ClienteHttpService` | `api/v1/clientes` |
| `ProductoHttpService` | `api/v1/productos` |
| `AsientoContableHttpService` | `api/v1/asientos-contables` |
| `SeguridadDinamicaHttpService` | `api/v1/seguridad-dinamica` |
| ... | ... |

**Características comunes:**
- Usan `HttpClient` inyectado (con `AuthMessageHandler`)
- Deserializan `ApiResponse<T>` con `System.Text.Json`
- Encapsulan URLs y construcción de query strings
- No exponen secretos

---

## 4. AuthMessageHandler

```csharp
// Intercepta cada petición HTTP y agrega el Bearer token JWT
public class AuthMessageHandler : DelegatingHandler
{
    private readonly JwtAuthStateProvider _authProvider;
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _authProvider.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        return await base.SendAsync(request, cancellationToken);
    }
}
```

---

## 5. JwtAuthStateProvider

```csharp
public class JwtAuthStateProvider : AuthenticationStateProvider
{
    // Almacena el token JWT en memoria (no localStorage)
    // Provee AuthenticationState con los claims del token
    // Método CambiarEmpresaActivaAsync renueva token con nuevo EmpresaId
}
```

**Flujo:**
1. Login → API devuelve JWT → `JwtAuthStateProvider` almacena token
2. Cada request HTTP → `AuthMessageHandler` agrega `Authorization: Bearer {token}`
3. `AuthenticationStateProvider` expone claims al sistema de autorización Blazor

---

## 6. Menú Dinámico vs Estático

El menú usa una **combinación** de ambos:

### Menú Estático (NavMenu.razor)

Controlado por `UiAuthorizationService`:

| Sección | Visible cuando |
|---|---|
| Dashboard | Siempre (logueado) |
| Módulos operativos | `HasTenantSelected` |
| Administración Global | `IsPlatformAdmin` |
| Administración de Empresa | `IsTenantAdmin && HasTenantSelected` |

### Menú Dinámico (SeguridadDinamicaService)

El sistema tiene tablas `ModuloSistema`, `FormularioSistema`, `PerfilPermiso` que permiten menú configurable por BD.  
El servicio `SeguridadDinamicaService.GetMenuUsuarioAsync()` retorna el menú filtrado por permisos del usuario.

**Actualmente, los links principales están en NavMenu.razor estático. El menú dinámico está disponible para personalización avanzada.**

---

## 7. UiAuthorizationService

```csharp
public class UiAuthorizationService
{
    public bool IsSuperAdmin => _sesionState.PlatformRole == "SuperAdmin";
    public bool IsPlatformAdmin => IsSuperAdmin || _sesionState.PlatformRole == "SystemAdmin";
    public bool IsTenantAdmin => _sesionState.IsTenantAdmin;
    public bool HasTenantSelected => _sesionState.HasTenantSelected;
    
    public bool CanSeeGlobalAdminMenu() => IsPlatformAdmin;
    public bool CanSeeTenantAdminMenu() => IsTenantAdmin && HasTenantSelected;
    public bool CanSeeOperationalMenu() => HasTenantSelected;
}
```

---

## 8. Páginas de Ventas

| Página | Ruta | Propósito |
|---|---|---|
| `Ventas/VentasComercial.razor` | `/ventas/comercial` | Lista de ventas con filtros y resumen |
| `Ventas/VentaComercialForm.razor` | `/ventas/comercial/nuevo`, `/ventas/comercial/{id}` | Crear/editar venta |
| `Ventas/Facturas.razor` | `/ventas/facturas` | Lista de facturas con filtros |
| `Ventas/Pedidos.razor` | `/ventas/pedidos` | Lista de pedidos con filtros y paginación |
| `Ventas/PedidoForm.razor` | `/ventas/pedidos/nuevo`, `/ventas/pedidos/editar/{id}` | Crear/editar pedido |
| `Ventas/PagoVenta.razor` | `/ventas/pagos` | Registro de pagos e historial |
| `Ventas/CuentasPorCobrar.razor` | `/ventas/cuentas-por-cobrar` | Cartera de CxC |
| `Ventas/AntiguedadSaldosCxC.razor` | `/ventas/cuentas-por-cobrar/antiguedad-saldos` | Reporte de antigüedad |

---

## 9. Páginas de Configuración FE

| Página | Ruta | Propósito |
|---|---|---|
| `Configuracion/ConfiguracionFE.razor` | `/config/facturacion-electronica` | CRUD de configuraciones FE |
| `ConfiguracionFE.razor (Auditoría)` | `/config/facturacion-electronica/auditoria` | Consulta read-only de auditoría FE |

---

## 10. Rutas del Menú (NavMenu.razor)

Ubicadas en `CanSeeTenantAdminMenu()`:

```
/config/facturacion-electronica       → Facturación Electrónica
/config/facturacion-electronica/auditoria  → Auditoría FE
```

---

## 11. Problemas Frecuentes

| Problema | Causa | Solución |
|---|---|---|
| Menú FE no visible | Usuario sin tenant seleccionado | Seleccionar empresa primero |
| Menú FE no visible | `NavMenu.razor` sin enlace | Agregar `NavLink` en sección correcta |
| Página FE no carga | Servicio `FacturacionFEHttpService` no registrado | Agregar `AddScoped` en `Program.cs` |
| Error 401 en API | Token expirado o sin EmpresaId | Re-login o cambiar empresa |
| Error CORS | Blazor en puerto no configurado | Agregar origen en CORS policy |
| Caché de Blazor | DLLs viejos en caché del navegador | `Ctrl+F5` o limpiar Application Cache |
