# 00 — Guía Técnica Consolidada — AgoraHUB360 ERP

> **Proyecto:** AgoraHub360 ERP  
> **Última actualización:** 2026-06-11  
> **Propósito:** Guía técnica integral para desarrolladores que se incorporan al proyecto.

---

## 1. Resumen Ejecutivo Técnico

**AgoraHUB360 ERP** es un sistema ERP multi-empresa construido con Clean Architecture en .NET 8.

| Aspecto | Detalle |
|---|---|
| **Backend** | .NET 8, ASP.NET Core Web API |
| **Frontend** | Blazor WebAssembly |
| **Base de datos** | SQL Server + EF Core 8 |
| **Arquitectura** | Clean Architecture (Domain, Application, Persistence, Infrastructure, API, Web) |
| **Multiempresa** | Por `EmpresaId` con QueryFilter global |
| **Autenticación** | JWT + policies + StubAuthHandler (dev) |
| **Menú** | Estático (NavMenu) + dinámico (SeguridadDinamicaService) |
| **Facturación Electrónica** | Multi-proveedor con patrón Provider + Cirrus |
| **Tests** | xUnit 2.4.2, fakes manuales |
| **Total entidades** | ~162 |
| **Total servicios** | ~76 |
| **Total controllers** | ~58 |
| **Total tests** | 125 |

---

## 2. Mapa de Capas

```
┌─────────────────────────────────────────────────────────────────────┐
│                      AgoraHub360.ERP.Web                            │
│              Blazor WebAssembly / Servicios HTTP                    │
├─────────────────────────────────────────────────────────────────────┤
│                      AgoraHub360.ERP.Api                            │
│              REST Controllers / Middleware / JWT / Swagger           │
├─────────────────────────────────────────────────────────────────────┤
│  AgoraHub360.ERP.Application        │  AgoraHub360.ERP.Shared      │
│  Interfaces / Services / Result     │  DTOs / ApiResponse / Const   │
├─────────────────────────────────────────────────────────────────────┤
│  AgoraHub360.ERP.Persistence        │  AgoraHub360.ERP.Infrastructure│
│  DbContext / Migrations / Repos     │  Cifrado / Providers FE      │
├─────────────────────────────────────────────────────────────────────┤
│                   AgoraHub360.ERP.Domain                            │
│        Entities / Enums / Repo Interfaces / Common                  │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 3. Mapa de Módulos

| Módulo | Abrev. | Capa Domain | Capa API | Capa Web |
|---|---|---|---|---|
| Contabilidad | ACC | 12 entidades | 6 controllers | 18 páginas |
| Ventas | VTA | 9 entidades | 3 controllers | 8 páginas |
| Compras | CMP | 14 entidades | 3 controllers | 6 páginas |
| Datos Maestros | MDM | ~28 entidades | 14 controllers | 15 páginas |
| Inventario | INV | 2 entidades | 1 controller | 4 páginas |
| CxC | CXC | 2 entidades | 2 controllers | 2 páginas |
| FE | FE | 4 entidades | 1 controller | 2 páginas |
| Seguridad | Core | 10 entidades | 3 controllers | 6 páginas |
| Activos Fijos | ACT | 2 entidades | — | — |
| Bancario | BNC | 2 entidades | 1 controller | — |

---

## 4. Flujo Multiempresa

```
1. Usuario se loguea → obtiene JWT con PlatformRole
2. Usuario selecciona empresa → se renueva JWT con:
   - EmpresaId (TenantId)
   - TenantRole (AdminEmpresa, Operador, Viewer)
   - TenantStatus = "selected"
3. Cada request HTTP incluye JWT en Header
4. TenantRequiredMiddleware valida EmpresaId en rutas protegidas
5. ICurrentUserService.EmpresaId provee el tenant a los servicios
6. AgoraDbContext.ApplyTenantFilter filtra automáticamente por EmpresaId
```

---

## 5. Flujo de Autenticación

```
Login (Web) → POST /api/v1/auth/login
  → AuthService.LoginAsync()
  → Valida credenciales (hash manual)
  → Genera JWT con claims (UserId, Email, PlatformRole)
  → Retorna token

Seleccionar Empresa
  → POST /api/v1/auth/seleccionar-empresa
  → AuthService.CambiarEmpresaActivaAsync()
  → Renueva JWT con EmpresaId + TenantRole
  → Retorna nuevo token

Cada request:
  → AuthMessageHandler (Web) agrega "Authorization: Bearer {token}"
  → JwtBearerHandler (API) valida token
  → CurrentUserService provee claims a servicios
```

---

## 6. Flujo Ventas / Facturación / Pagos

```
PedidoVenta (Borrador → Confirmado)
  → ConfirmarAsync → reserva stock
  → GenerarVentaAsync → crea Venta desde Pedido

Venta (Borrador → Confirmada)
  → ConfirmarAsync → descuenta inventario
  → FacturaVentaService.GenerarDesdeVentaAsync → crea FacturaVenta
    → Asigna NumeroFactura, ActivityCode, ItemCode
    → Crea CuentaPorCobrar

Registrar Pago
  → VentaService.RegistrarPagoAsync
  → Ejecuta en transacción: guarda pago + actualiza CxC

Anular Venta
  → VentaService.AnularAsync
  → Anula factura asociada si existe
  → Revierte inventario si aplica
```

---

## 7. Flujo Facturación Electrónica

```
1. Configurar FE desde UI (/config/facturacion-electronica)
   → Seleccionar proveedor (CIRRUS)
   → Seleccionar ambiente (TEST / PRODUCCION)
   → Ingresar credenciales (se cifran al guardar)
   → Activar configuración

2. Emitir factura
   → POST /api/v1/facturacion-electronica/emitir/{facturaVentaId}
   → FacturacionElectronicaService.EmitirAsync()
   → Obtiene config activa de la empresa
   → Resuelve provider por CodigoProveedor
   → Obtiene token OAuth2 (cacheado)
   → Envía factura al proveedor
   → Guarda CUF/CUFD/BillUuid en FacturaVenta
   → Registra auditoría

3. Anular factura
   → POST /api/v1/facturacion-electronica/anular/{facturaVentaId}
   → Anula contra proveedor
   → Actualiza estado en BD
   → Registra auditoría

4. Consultar estado
   → GET /api/v1/facturacion-electronica/estado/{facturaVentaId}
```

---

## 8. Reglas de Arquitectura

| Regla | Descripción |
|---|---|
| **Clean Architecture** | Domain → Application → Persistence/Infrastructure → API → Web |
| **Dependencias** | Solo inward: Web→API, API→Application, Application→Domain |
| **Result Pattern** | Todos los servicios retornan `Result<T>` (no excepciones) |
| **Tenant Isolation** | `EmpresaId` desde JWT, filtro global en EF |
| **Soft Delete** | `Activo` bool en todas las entidades |
| **No enums de proveedor FE** | Proveedores FE son catálogo en BD, no enum |
| **Secretos cifrados** | AES-256-CBC con formato `v1:Base64` |
| **Provider Pattern** | `IFacturacionElectronicaProvider` resuelto por `CodigoProveedor` |
| **No Moq** | Tests usan fakes manuales |
| **Blazor WASM** | Cliente WebAssembly, no Server |

---

## 9. Riesgos Conocidos

| Riesgo | Impacto | Estado |
|---|---|---|
| Dos enums EstadoFactura | Confusión en mapeos | ⚠️ Documentado |
| Sin Moq en tests | Fake manual más código | ⚠️ Aceptado |
| Sin WebApplicationFactory | Sin tests integración HTTP | ⚠️ Aceptado |
| ConnectionString en texto plano | Exposición en appsettings.json | ⚠️ Mejorable |
| Cobertura de tests baja | Solo ~125 tests | ⚠️ Aceptado |

---

## 10. Checklist para Nuevos Desarrolladores

- [ ] Leer los 9 documentos técnicos en `/docs/technical/`
- [ ] Entender Clean Architecture y dependencias inward
- [ ] Comprender multiempresa: `TenantEntity`, `ICurrentUserService`, QueryFilter
- [ ] Conocer el patrón `Result<T>` (sin excepciones)
- [ ] Revisar el flujo FE: Provider Pattern, cifrado, OAuth2
- [ ] Revisar estructura de tests: fakes manuales, xUnit
- [ ] Configurar entorno local (ver sección 11)

---

## 11. Cómo Levantar el Sistema

### Requisitos

- .NET 8 SDK
- SQL Server (local o Docker)
- Visual Studio 2022 / VS Code

### Pasos

```bash
# Restaurar dependencias
dotnet restore

# Aplicar migraciones (crear/actualizar BD local)
dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api

# Ejecutar API (backend)
dotnet run --project src/AgoraHub360.ERP.Api --urls https://localhost:5001

# Ejecutar Web (frontend Blazor)
dotnet run --project src/AgoraHub360.ERP.Web --urls https://localhost:5002
```

### Configuración necesaria

- `appsettings.Development.json` en API:
  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "Server=.;Database=AgoraHub360_ERP;Trusted_Connection=true;TrustServerCertificate=true;"
    },
    "Encryption:Key": "tu-clave-base64-de-32-bytes=="
  }
  ```

---

## 12. Cómo Validar Build/Tests

```bash
# Build completo
dotnet build AgoraHub360.ERP.sln

# Ejecutar tests
dotnet test AgoraHub360.ERP.sln

# Verificar resultado: 0 errores, 125+ tests passed
```

---

## 13. Cómo Probar FE

1. Iniciar API (`dotnet run --project src/AgoraHub360.ERP.Api`)
2. Iniciar Web (`dotnet run --project src/AgoraHub360.ERP.Web`)
3. Login con credenciales de desarrollo
4. Seleccionar empresa
5. Navegar a Configuración → Facturación Electrónica
6. Crear configuración con datos dummy
7. Ir a Ventas → Facturas
8. Crear/emitir una factura de prueba

---

## 14. Archivos de Documentación Generados

```
docs/technical/
├── 00_GUIA_TECNICA_AGORAHUB360_ERP.md   ← Este archivo
├── 01_DOMAIN.md
├── 02_PERSISTENCE.md
├── 03_APPLICATION.md
├── 04_API.md
├── 05_SHARED_DTOS.md
├── 06_WEB_BLAZOR_WASM.md
├── 07_INFRASTRUCTURE.md
└── 08_TESTS.md
```
