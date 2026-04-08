# 📊 ESTADO ACTUAL Y DOCUMENTACIÓN COMPLETA
## AgoraHub360 ERP - Sistema ERP Empresarial

---

## 📋 INFORMACIÓN GENERAL

| Campo | Valor |
|-------|-------|
| **Nombre del Proyecto** | AgoraHub360 ERP |
| **Versión Actual** | v1.0.0 MVP |
| **Fecha de Gestión** | 2026 |
| **Repositorio** | https://github.com/abelcalvimontes/AgoraHUB360-ERP |
| **Rama Principal** | `main` |
| **Framework** | .NET 8.0 |
| **Lenguaje** | C# 12.0 |
| **Arquitectura** | Clean Architecture + Blazor WebAssembly |
| **Base de Datos** | SQL Server 2019+ |
| **IDE** | Visual Studio 2026 (18.4.3) |
| **Estado Actual** | ✅ MVP Operativo - En Desarrollo Activo |

---

## 🎯 VISIÓN DEL PROYECTO

**AgoraHub360 ERP** es un sistema ERP modular empresarial desarrollado bajo el **Modelo Ágora (MAPE)**, diseñado para entornos mixtos de importación y comercialización, con arquitectura limpia, escalable y **multiempresa desde el núcleo**.

### Objetivos Estratégicos
- ✅ Control operativo integral
- ✅ Gestión multiempresa y multimoneda
- ✅ Integración: Compras → Inventario → Ventas
- ✅ Costeo de importaciones con prorrateo
- ✅ Escalabilidad modular
- 🔄 Evolución futura hacia BI, automatización e integraciones externas

---

## 🏗️ ARQUITECTURA TÉCNICA

### Stack Tecnológico Completo

```
┌──────────────────────────────────────────────────────────┐
│         Frontend - Blazor WebAssembly (.NET 8)           │
│                                                          │
│  ┌────────────────────┐    ┌──────────────────────┐    │
│  │ AgoraHub360.ERP.Web│    │ AgoraHub360.ERP.Shared│    │
│  │  Blazor WASM       │◄──►│  DTOs y Contratos    │    │
│  └────────────────────┘    └──────────────────────┘    │
└──────────────────────────────────────────────────────────┘
                         │
                         │ HTTPS (JSON API)
                         ▼
┌──────────────────────────────────────────────────────────┐
│            Backend - ASP.NET Core API                    │
│                                                          │
│  ┌──────────────────────────────────────────────────┐  │
│  │        AgoraHub360.ERP.Api                        │  │
│  │  Controllers (v1) + Middleware + JWT Auth         │  │
│  └──────────────────────────────────────────────────┘  │
│                         │                                │
│  ┌──────────────────────┴────────────────────────────┐  │
│  │                                                    │  │
│  ▼                      ▼                      ▼     │  │
│ ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  │  │
│ │ Application │  │Infrastructure│  │ Persistence │  │  │
│ │  Services   │  │   Externos   │  │  EF Core    │  │  │
│ └─────────────┘  └─────────────┘  └─────────────┘  │  │
│       │                                   │          │  │
│       └─────────────┬─────────────────────┘          │  │
│                     ▼                                │  │
│            ┌─────────────────┐                       │  │
│            │     Domain      │                       │  │
│            │ Entidades Core  │                       │  │
│            └─────────────────┘                       │  │
└──────────────────────────────────────────────────────────┘
                         │
                         ▼
              ┌────────────────────┐
              │   SQL Server 2019+ │
              │ db_AgoraERP_Core   │
              └────────────────────┘
```

### Principios Arquitectónicos

✅ **Clean Architecture**
- Separación clara de responsabilidades
- Independencia de frameworks
- Testeable y mantenible

✅ **Domain-Driven Design (DDD)**
- Entidades ricas en comportamiento
- Agregados y value objects
- Lógica de negocio en el dominio

✅ **SOLID Principles**
- Single Responsibility
- Open/Closed
- Liskov Substitution
- Interface Segregation
- Dependency Inversion

---

## 📦 ESTRUCTURA DE CAPAS

| Capa | Proyecto | Responsabilidad | Dependencias | Tecnologías |
|------|----------|----------------|--------------|-------------|
| **Domain** | `AgoraHub360.ERP.Domain` | Entidades, interfaces core, reglas de negocio | ❌ Ninguna | C# 12, .NET 8 |
| **Application** | `AgoraHub360.ERP.Application` | Casos de uso, servicios de aplicación | → Domain | MediatR (futuro), AutoMapper |
| **Persistence** | `AgoraHub360.ERP.Persistence` | EF Core, migraciones, repositorios | → Domain, Application | EF Core 8.0, SQL Server |
| **Infrastructure** | `AgoraHub360.ERP.Infrastructure` | Servicios externos, integraciones | → Domain, Application | Email, SMS, Storage |
| **API** | `AgoraHub360.ERP.Api` | Controllers, middleware, autenticación | → Todas | ASP.NET Core 8, JWT, Swagger |
| **Web** | `AgoraHub360.ERP.Web` | Blazor WASM, componentes UI | → Shared | Blazor WASM 8.0, Bootstrap 5 |
| **Shared** | `AgoraHub360.ERP.Shared` | DTOs, contratos compartidos | ❌ Ninguna | C# 12, Data Annotations |
| **Tests** | `AgoraHub360.ERP.Tests` | Pruebas unitarias e integración | → Domain, Application | xUnit, Moq |

---

## 📦 MÓDULOS IMPLEMENTADOS (MVP v1.0)

### ✅ 1. CORE - Sistema Base
**Estado:** 🟢 Completo y Operativo (100%)

| Feature | Estado | Descripción | Tecnología |
|---------|--------|-------------|------------|
| Seguridad RBAC | ✅ | Roles y permisos basados en JWT | JWT Bearer, ASP.NET Identity |
| Multiempresa | ✅ | Aislamiento de datos por `EmpresaId` | Query Filters EF Core |
| Multimoneda | ✅ | Soporte para múltiples monedas | Domain Entities |
| Parámetros del Sistema | ✅ | Configuración general | Key-Value Store |
| Numeración Documentos | ✅ | Generación automática de correlativos | Secuencias transaccionales |
| Auditoría Automática | ✅ | Tracking de cambios | EF Core Interceptors |
| Versionamiento Entidades | ✅ | Control de versiones | EntityVersion Pattern |

**Entidades Core:**
```
├── Empresa
├── Usuario
├── Rol
├── Permiso
├── Moneda
├── TipoCambio
├── ParametroSistema
└── NumeracionDocumento
```

---

### ✅ 2. MDM - Maestros de Datos
**Estado:** 🟢 Completo y Operativo (100%)

| Maestro | Estado | CRUD | API Endpoint | UI Blazor | Características |
|---------|--------|------|--------------|-----------|-----------------|
| **Clientes** | ✅ | ✅ | `/api/v1/clientes` | ✅ | RUC, NIT, Razón Social, Multiempresa |
| **Proveedores** | ✅ | ✅ | `/api/v1/proveedores` | ✅ | Local/Internacional, Multimoneda |
| **Productos** | ✅ | ✅ | `/api/v1/productos` | ✅ | MP/PT/Servicio, SKU, Barcode |
| **Categorías** | ✅ | ✅ | `/api/v1/categorias-producto` | ✅ | Jerárquicas |
| **Unidades de Medida** | ✅ | ✅ | `/api/v1/unidades-medida` | ✅ | Conversiones |
| **Almacenes** | ✅ | ✅ | `/api/v1/almacenes` | ✅ | Multi-almacén |
| **Empresas** | ✅ | ✅ | `/api/v1/empresas` | ✅ | Multiempresa |
| **Monedas** | ✅ | ✅ | `/api/v1/monedas` | ✅ | ISO 4217 |

**Características Avanzadas:**
- ✅ Búsqueda y filtrado
- ✅ Paginación
- ✅ Ordenamiento
- ✅ Exportación a Excel (EPPlus)
- ✅ Validaciones de negocio
- ✅ Soft delete
- ✅ Auditoría completa

---

### ✅ 3. INVENTARIO - Gestión Avanzada
**Estado:** 🟢 Completo y Operativo (100%)

| Feature | Estado | Descripción | Características |
|---------|--------|-------------|-----------------|
| Kardex Multi-almacén | ✅ | Control de entradas/salidas | FIFO/LIFO/Promedio |
| Costo Promedio | ✅ | Cálculo automático | Actualización en tiempo real |
| Ajustes de Inventario | ✅ | Ajustes por diferencia, merma | Multiempresa |
| Transferencias | ✅ | Movimiento entre almacenes | Trazabilidad completa |
| Trazabilidad | ✅ | Seguimiento de movimientos | Auditoría detallada |
| Consultas Kardex | ✅ | Reportes detallados | Filtros avanzados |

**Tipos de Movimiento:**
```
├── Entrada por Compra
├── Entrada por Ajuste
├── Entrada por Transferencia
├── Salida por Venta
├── Salida por Ajuste
├── Salida por Transferencia
└── Salida por Merma
```

**API Endpoints:**
```
GET    /api/v1/inventario/kardex
GET    /api/v1/inventario/kardex/{productoId}
GET    /api/v1/inventario/stock
POST   /api/v1/inventario/ajuste
POST   /api/v1/inventario/transferencia
GET    /api/v1/inventario/movimientos
```

---

### ✅ 4. COMPRAS + IMPORTACIÓN
**Estado:** 🟢 Completo y Operativo (100%)

| Feature | Estado | Descripción | Flujo |
|---------|--------|-------------|-------|
| Orden de Compra | ✅ | Creación y gestión de OC | Borrador → Aprobada → Recibida |
| Recepción Parcial | ✅ | Recepción por lotes | Control de cantidades |
| Registro de Gastos | ✅ | Gastos de importación | Asociación a OC |
| **Prorrateo de Costos** | ✅ | Distribución automática | Por cantidad/valor/peso |
| Flujo Importación Completo | ✅ | DUA, embarque, despacho | Trazabilidad end-to-end |
| Proveedores Internacionales | ✅ | Gestión multimoneda | Conversión automática |

**Flujo de Importación:**
```
1. Orden de Compra Internacional
   ├── Proveedor internacional
   ├── Moneda extranjera
   └── INCOTERMS
   
2. Embarque
   ├── Bill of Lading (BL)
   ├── Fecha de zarpe
   └── Fecha estimada de llegada
   
3. Despacho Aduanero
   ├── DUA (Declaración Única Aduanera)
   ├── Gastos aduanales
   └── Almacenaje
   
4. Prorrateo de Costos
   ├── FOB
   ├── Flete
   ├── Seguro
   ├── Aduanas
   └── Otros gastos
   
5. Recepción Final
   ├── Ingreso a almacén
   ├── Actualización de inventario
   └── Costo promedio actualizado
```

**API Endpoints:**
```
GET    /api/v1/ordenes-compra
POST   /api/v1/ordenes-compra
PUT    /api/v1/ordenes-compra/{id}
POST   /api/v1/compras/recepcion
POST   /api/v1/compras/prorrateo
GET    /api/v1/importacion/dua
POST   /api/v1/importacion/embarque
```

---

### ✅ 5. VENTAS
**Estado:** 🟢 Completo y Operativo (95%)

| Feature | Estado | Descripción | Observaciones |
|---------|--------|-------------|---------------|
| Cotizaciones | ✅ | Generación de cotizaciones | Conversión a pedido |
| Pedidos | ✅ | Gestión de pedidos | Control de stock |
| Facturación Interna | ✅ | Facturas sin timbrado | En desarrollo: SIN (v1.1) |
| Integración Inventario | ✅ | Descuento automático de stock | Transaccional |
| Control de Margen | ✅ | Cálculo de rentabilidad | % Margen, Markup |
| Gestión de Clientes | ✅ | CRM básico | Historial de compras |

**Flujo de Ventas:**
```
Cotización → Pedido → Factura → Entrega
     ↓          ↓         ↓         ↓
  (Vigencia) (Reserva) (Descuento) (Kardex)
```

**Pendiente v1.1:**
- 🔄 Integración con SIN (Servicio de Impuestos Nacionales - Bolivia)
- 🔄 Facturación electrónica
- 🔄 Timbrado digital

**API Endpoints:**
```
GET    /api/v1/cotizaciones
POST   /api/v1/cotizaciones
PUT    /api/v1/cotizaciones/{id}/convertir-a-pedido
GET    /api/v1/pedidos
POST   /api/v1/pedidos
PUT    /api/v1/pedidos/{id}/convertir-a-factura
GET    /api/v1/facturas
POST   /api/v1/facturas
```

---

### 🟡 6. CONTABILIDAD
**Estado:** 🟡 En Desarrollo Activo (60%)

| Feature | Estado | Descripción | Próximos Pasos |
|---------|--------|-------------|----------------|
| Plan de Cuentas | ✅ | Gestión de cuentas contables | Completo |
| Asientos Contables | 🟡 | En desarrollo | Validaciones de balance |
| Centro de Costos | 🔄 | Planificado | Q2 2026 |
| Libros Contables | 🔄 | Planificado | Q2 2026 |

**Estructura del Plan de Cuentas:**
```
├── 1. ACTIVO
│   ├── 1.1 Activo Corriente
│   └── 1.2 Activo No Corriente
├── 2. PASIVO
│   ├── 2.1 Pasivo Corriente
│   └── 2.2 Pasivo No Corriente
├── 3. PATRIMONIO
├── 4. INGRESOS
└── 5. GASTOS
```

**API Endpoints:**
```
GET    /api/v1/plan-cuentas
POST   /api/v1/plan-cuentas
GET    /api/v1/asientos-contables
POST   /api/v1/asientos-contables
```

---

### 🟡 7. LOGÍSTICA
**Estado:** 🟡 En Desarrollo (40%)

| Feature | Estado | Descripción |
|---------|--------|-------------|
| Hoja de Ruta | 🟡 | Gestión de entregas |
| Tracking Envíos | 🔄 | Planificado |
| Gestión de Transportistas | 🔄 | Planificado |

---

## 🗂️ ESTRUCTURA DE ARCHIVOS DETALLADA

```
AgoraHUB360-ERP/
│
├── 📁 src/
│   │
│   ├── 📦 AgoraHub360.ERP.Domain/           # ← CAPA DE DOMINIO (No depende de nadie)
│   │   ├── 📁 Entities/
│   │   │   ├── 📁 Core/                     # Sistema base
│   │   │   │   ├── Empresa.cs
│   │   │   │   ├── Usuario.cs
│   │   │   │   ├── Rol.cs
│   │   │   │   ├── Permiso.cs
│   │   │   │   ├── Moneda.cs
│   │   │   │   ├── TipoCambio.cs
│   │   │   │   ├── ParametroSistema.cs
│   │   │   │   └── NumeracionDocumento.cs
│   │   │   │
│   │   │   ├── 📁 MDM/                      # Maestros
│   │   │   │   ├── Cliente.cs
│   │   │   │   ├── Proveedor.cs
│   │   │   │   ├── Producto.cs
│   │   │   │   ├── CategoriaProducto.cs
│   │   │   │   ├── UnidadMedida.cs
│   │   │   │   └── Almacen.cs
│   │   │   │
│   │   │   ├── 📁 INV/                      # Inventario
│   │   │   │   ├── Inventario.cs
│   │   │   │   ├── MovimientoInventario.cs
│   │   │   │   ├── Kardex.cs
│   │   │   │   ├── AjusteInventario.cs
│   │   │   │   └── TransferenciaInventario.cs
│   │   │   │
│   │   │   ├── 📁 CMP/                      # Compras
│   │   │   │   ├── OrdenCompra.cs
│   │   │   │   ├── OrdenCompraDetalle.cs
│   │   │   │   ├── RecepcionCompra.cs
│   │   │   │   ├── GastoImportacion.cs
│   │   │   │   └── ProrrateoImportacion.cs
│   │   │   │
│   │   │   ├── 📁 VEN/                      # Ventas
│   │   │   │   ├── Cotizacion.cs
│   │   │   │   ├── CotizacionDetalle.cs
│   │   │   │   ├── Pedido.cs
│   │   │   │   ├── PedidoDetalle.cs
│   │   │   │   ├── Factura.cs
│   │   │   │   └── FacturaDetalle.cs
│   │   │   │
│   │   │   ├── 📁 CON/                      # Contabilidad
│   │   │   │   ├── CuentaContable.cs
│   │   │   │   ├── AsientoContable.cs
│   │   │   │   ├── AsientoContableDetalle.cs
│   │   │   │   └── CentroCosto.cs
│   │   │   │
│   │   │   ├── 📁 LOG/                      # Logística
│   │   │   │   ├── HojaRuta.cs
│   │   │   │   └── HojaRutaDetalle.cs
│   │   │   │
│   │   │   └── 📁 VER/                      # Versionamiento
│   │   │       └── EntityVersion.cs
│   │   │
│   │   ├── 📁 Interfaces/
│   │   │   ├── IEntity.cs
│   │   │   ├── IAuditable.cs
│   │   │   ├── IMultiempresa.cs
│   │   │   └── ISoftDelete.cs
│   │   │
│   │   └── 📁 Common/
│   │       ├── BaseEntity.cs
│   │       └── ValueObjects/
│   │
│   ├── 📦 AgoraHub360.ERP.Application/      # ← CAPA DE APLICACIÓN
│   │   ├── 📁 Services/
│   │   │   ├── ClienteService.cs
│   │   │   ├── ProveedorService.cs
│   │   │   ├── ProductoService.cs
│   │   │   ├── InventarioService.cs
│   │   │   ├── CompraService.cs
│   │   │   ├── VentaService.cs
│   │   │   ├── ContabilidadService.cs
│   │   │   └── VersioningService.cs
│   │   │
│   │   ├── 📁 Interfaces/
│   │   │   ├── IClienteService.cs
│   │   │   ├── IProveedorService.cs
│   │   │   └── ... (más interfaces)
│   │   │
│   │   └── DependencyInjection.cs
│   │
│   ├── 📦 AgoraHub360.ERP.Persistence/      # ← CAPA DE PERSISTENCIA
│   │   ├── 📁 Context/
│   │   │   ├── AgoraDbContext.cs
│   │   │   └── AgoraDbContextFactory.cs
│   │   │
│   │   ├── 📁 Configurations/               # Fluent API
│   │   │   ├── EmpresaConfiguration.cs
│   │   │   ├── ClienteConfiguration.cs
│   │   │   ├── ProductoConfiguration.cs
│   │   │   └── ... (más configuraciones)
│   │   │
│   │   ├── 📁 Migrations/                   # Migraciones EF Core
│   │   │   ├── 20260101000000_InitialCreate.cs
│   │   │   ├── 20260305065107_CMP_FlujoImportacionCompleto.cs
│   │   │   └── ... (15+ migraciones)
│   │   │
│   │   ├── 📁 Repositories/
│   │   │   └── GenericRepository.cs
│   │   │
│   │   ├── 📁 Interceptors/
│   │   │   ├── AuditInterceptor.cs
│   │   │   └── EntityVersioningInterceptor.cs
│   │   │
│   │   ├── 📁 Seeders/                      # Datos iniciales
│   │   │   ├── EmpresaSeed.cs
│   │   │   ├── UsuarioSeed.cs
│   │   │   └── MonedaSeed.cs
│   │   │
│   │   └── DependencyInjection.cs
│   │
│   ├── 📦 AgoraHub360.ERP.Infrastructure/   # ← SERVICIOS EXTERNOS
│   │   ├── 📁 Services/
│   │   │   ├── EmailService.cs
│   │   │   └── StorageService.cs
│   │   │
│   │   └── DependencyInjection.cs
│   │
│   ├── 📦 AgoraHub360.ERP.Api/              # ← API REST
│   │   ├── 📁 Controllers/
│   │   │   └── 📁 V1/                       # API versionada
│   │   │       ├── AuthController.cs
│   │   │       ├── ClientesController.cs
│   │   │       ├── ProveedoresController.cs
│   │   │       ├── ProductosController.cs
│   │   │       ├── InventarioController.cs
│   │   │       ├── ComprasController.cs
│   │   │       ├── VentasController.cs
│   │   │       ├── ContabilidadController.cs
│   │   │       └── DiagnosticsController.cs
│   │   │
│   │   ├── 📁 Middleware/
│   │   │   ├── ErrorHandlingMiddleware.cs
│   │   │   └── TenantValidationMiddleware.cs
│   │   │
│   │   ├── 📁 Auth/
│   │   │   └── JwtTokenGenerator.cs
│   │   │
│   │   ├── 📁 Services/
│   │   │   └── CurrentUserService.cs
│   │   │
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   │
│   ├── 📦 AgoraHub360.ERP.Web/              # ← BLAZOR WEBASSEMBLY
│   │   ├── 📁 Pages/
│   │   │   ├── Index.razor                  # Dashboard
│   │   │   ├── Login.razor                  # ← ARCHIVO ACTUAL
│   │   │   │
│   │   │   ├── 📁 Maestros/
│   │   │   │   ├── Clientes.razor
│   │   │   │   ├── Proveedores.razor
│   │   │   │   ├── Productos.razor
│   │   │   │   ├── Categorias.razor
│   │   │   │   ├── Almacenes.razor
│   │   │   │   └── UnidadesMedida.razor
│   │   │   │
│   │   │   ├── 📁 Inventario/
│   │   │   │   ├── Kardex.razor
│   │   │   │   ├── Movimientos.razor
│   │   │   │   ├── Ajustes.razor
│   │   │   │   └── Transferencias.razor
│   │   │   │
│   │   │   ├── 📁 Compras/
│   │   │   │   ├── OrdenesCompra.razor
│   │   │   │   ├── Recepcion.razor
│   │   │   │   └── Importacion.razor
│   │   │   │
│   │   │   ├── 📁 Ventas/
│   │   │   │   ├── Cotizaciones.razor
│   │   │   │   ├── Pedidos.razor
│   │   │   │   └── Facturas.razor
│   │   │   │
│   │   │   ├── 📁 Contabilidad/
│   │   │   │   ├── PlanCuentas.razor
│   │   │   │   └── AsientosContables.razor
│   │   │   │
│   │   │   └── 📁 Config/
│   │   │       ├── Empresas.razor
│   │   │       ├── Monedas.razor
│   │   │       ├── Numeracion.razor
│   │   │       └── Parametros.razor
│   │   │
│   │   ├── 📁 Components/
│   │   │   ├── NavMenu.razor
│   │   │   └── LoginLayout.razor
│   │   │
│   │   ├── 📁 Services/
│   │   │   └── AuthHttpService.cs
│   │   │
│   │   ├── 📁 Auth/
│   │   │   └── JwtAuthStateProvider.cs
│   │   │
│   │   ├── 📁 wwwroot/
│   │   │   ├── css/
│   │   │   │   ├── app.css
│   │   │   │   └── bootstrap/
│   │   │   ├── js/
│   │   │   └── index.html
│   │   │
│   │   ├── Program.cs
│   │   └── App.razor
│   │
│   ├── 📦 AgoraHub360.ERP.Shared/           # ← DTOs COMPARTIDOS
│   │   └── 📁 DTOs/
│   │       ├── Auth/
│   │       │   ├── LoginRequestDto.cs
│   │       │   └── LoginResponseDto.cs
│   │       ├── Core/
│   │       ├── MDM/
│   │       ├── Inventario/
│   │       ├── Compras/
│   │       ├── Ventas/
│   │       └── Contabilidad/
│   │
│   └── 📦 AgoraHub360.ERP.Tests/            # ← PRUEBAS
│       ├── 📁 Domain.Tests/
│       ├── 📁 Application.Tests/
│       └── 📁 Integration.Tests/
│
├── 📁 docs/                                 # ← DOCUMENTACIÓN
│   ├── DEPLOYMENT_GUIDE.md
│   ├── COMMIT_CONVENTION.md
│   ├── GIT_WORKFLOW.md
│   ├── PROJECT_BOARD_SETUP.md
│   ├── DATABASE_TEST_GUIDE.md
│   ├── DATABASE_TEST_SUMMARY.md
│   ├── DATABASE_TEST_REPORT.md
│   ├── DATABASE_CONNECTION_FIX.md
│   ├── LOGIN-ERROR-FIX.md
│   ├── RESET-ADMIN-GUIDE.md
│   ├── RESET-ADMIN-QUICKSTART.md
│   └── CREDENCIALES-DEFAULT.md
│
├── 📁 scripts/                              # ← SCRIPTS DE AUTOMATIZACIÓN
│   ├── start-system.ps1
│   ├── reset-admin-user.ps1
│   ├── reset-admin-simple.ps1
│   ├── quick-db-test.ps1
│   └── test-database-connection.ps1
│
├── 📄 README.md
├── 📄 ESTADO_PROYECTO_COMPLETO.md           # ← ESTE ARCHIVO
├── 📄 AgoraHub360.ERP.sln
├── 📄 .gitignore
└── 📄 .editorconfig
```

---

## 🔌 API ENDPOINTS COMPLETOS

### 🔐 Autenticación
```http
POST   /api/v1/auth/login           # Login de usuario
POST   /api/v1/auth/register         # Registro (admin only)
POST   /api/v1/auth/refresh          # Refresh token
GET    /api/v1/auth/me               # Usuario actual
POST   /api/v1/auth/logout           # Logout
```

### 🏢 Empresas
```http
GET    /api/v1/empresas              # Listar empresas
GET    /api/v1/empresas/{id}         # Obtener empresa
POST   /api/v1/empresas              # Crear empresa
PUT    /api/v1/empresas/{id}         # Actualizar empresa
DELETE /api/v1/empresas/{id}         # Eliminar empresa (soft)
```

### 👥 Clientes
```http
GET    /api/v1/clientes              # Listar clientes
GET    /api/v1/clientes/{id}         # Obtener cliente
POST   /api/v1/clientes              # Crear cliente
PUT    /api/v1/clientes/{id}         # Actualizar cliente
DELETE /api/v1/clientes/{id}         # Eliminar cliente
GET    /api/v1/clientes/search       # Búsqueda avanzada
GET    /api/v1/clientes/export       # Exportar a Excel
```

### 🏭 Proveedores
```http
GET    /api/v1/proveedores           # Listar proveedores
GET    /api/v1/proveedores/{id}      # Obtener proveedor
POST   /api/v1/proveedores           # Crear proveedor
PUT    /api/v1/proveedores/{id}      # Actualizar proveedor
DELETE /api/v1/proveedores/{id}      # Eliminar proveedor
```

### 📦 Productos
```http
GET    /api/v1/productos             # Listar productos
GET    /api/v1/productos/{id}        # Obtener producto
POST   /api/v1/productos             # Crear producto
PUT    /api/v1/productos/{id}        # Actualizar producto
DELETE /api/v1/productos/{id}        # Eliminar producto
GET    /api/v1/productos/stock       # Consultar stock
GET    /api/v1/productos/barcode/{code} # Buscar por barcode
```

### 📂 Categorías
```http
GET    /api/v1/categorias-producto   # Listar categorías
GET    /api/v1/categorias-producto/{id} # Obtener categoría
POST   /api/v1/categorias-producto   # Crear categoría
PUT    /api/v1/categorias-producto/{id} # Actualizar categoría
DELETE /api/v1/categorias-producto/{id} # Eliminar categoría
```

### 📏 Unidades de Medida
```http
GET    /api/v1/unidades-medida       # Listar unidades
GET    /api/v1/unidades-medida/{id}  # Obtener unidad
POST   /api/v1/unidades-medida       # Crear unidad
PUT    /api/v1/unidades-medida/{id}  # Actualizar unidad
DELETE /api/v1/unidades-medida/{id}  # Eliminar unidad
```

### 🏪 Almacenes
```http
GET    /api/v1/almacenes             # Listar almacenes
GET    /api/v1/almacenes/{id}        # Obtener almacén
POST   /api/v1/almacenes             # Crear almacén
PUT    /api/v1/almacenes/{id}        # Actualizar almacén
DELETE /api/v1/almacenes/{id}        # Eliminar almacén
```

### 💰 Monedas
```http
GET    /api/v1/monedas               # Listar monedas
GET    /api/v1/monedas/{id}          # Obtener moneda
POST   /api/v1/monedas               # Crear moneda
PUT    /api/v1/monedas/{id}          # Actualizar moneda
GET    /api/v1/monedas/tipo-cambio   # Tipo de cambio
```

### 📊 Inventario
```http
GET    /api/v1/inventario/kardex                    # Kardex general
GET    /api/v1/inventario/kardex/{productoId}       # Kardex por producto
GET    /api/v1/inventario/stock                     # Stock actual
GET    /api/v1/inventario/stock/{productoId}        # Stock de producto
GET    /api/v1/inventario/movimientos               # Movimientos
POST   /api/v1/inventario/ajuste                    # Ajuste de inventario
POST   /api/v1/inventario/transferencia             # Transferencia
GET    /api/v1/inventario/valorizado                # Inventario valorizado
```

### 🛒 Compras
```http
GET    /api/v1/ordenes-compra                       # Listar órdenes
GET    /api/v1/ordenes-compra/{id}                  # Obtener orden
POST   /api/v1/ordenes-compra                       # Crear orden
PUT    /api/v1/ordenes-compra/{id}                  # Actualizar orden
POST   /api/v1/ordenes-compra/{id}/aprobar          # Aprobar orden
POST   /api/v1/compras/recepcion                    # Recepción
POST   /api/v1/compras/recepcion-parcial            # Recepción parcial
POST   /api/v1/compras/gastos-importacion           # Registrar gastos
POST   /api/v1/compras/prorrateo                    # Prorratear costos
```

### 💵 Ventas
```http
GET    /api/v1/cotizaciones                         # Listar cotizaciones
GET    /api/v1/cotizaciones/{id}                    # Obtener cotización
POST   /api/v1/cotizaciones                         # Crear cotización
PUT    /api/v1/cotizaciones/{id}                    # Actualizar cotización
POST   /api/v1/cotizaciones/{id}/convertir-pedido   # Convertir a pedido

GET    /api/v1/pedidos                              # Listar pedidos
GET    /api/v1/pedidos/{id}                         # Obtener pedido
POST   /api/v1/pedidos                              # Crear pedido
PUT    /api/v1/pedidos/{id}                         # Actualizar pedido
POST   /api/v1/pedidos/{id}/convertir-factura       # Convertir a factura

GET    /api/v1/facturas                             # Listar facturas
GET    /api/v1/facturas/{id}                        # Obtener factura
POST   /api/v1/facturas                             # Crear factura
PUT    /api/v1/facturas/{id}/anular                 # Anular factura
```

### 📒 Contabilidad
```http
GET    /api/v1/plan-cuentas                         # Plan de cuentas
GET    /api/v1/plan-cuentas/{id}                    # Obtener cuenta
POST   /api/v1/plan-cuentas                         # Crear cuenta
PUT    /api/v1/plan-cuentas/{id}                    # Actualizar cuenta

GET    /api/v1/asientos-contables                   # Listar asientos
GET    /api/v1/asientos-contables/{id}              # Obtener asiento
POST   /api/v1/asientos-contables                   # Crear asiento
PUT    /api/v1/asientos-contables/{id}              # Actualizar asiento
```

### 🔍 Diagnósticos
```http
GET    /api/v1/diagnostics/ping                     # Test de API
GET    /api/v1/diagnostics/database-test            # Test de BD
GET    /health                                      # Health check
GET    /swagger                                     # Documentación Swagger
```

---

## 🖥️ PÁGINAS WEB BLAZOR

### 🔐 Autenticación
| Ruta | Descripción | Estado |
|------|-------------|--------|
| `/login` | Página de inicio de sesión | ✅ |

### 📊 Dashboard
| Ruta | Descripción | Estado |
|------|-------------|--------|
| `/` | Dashboard principal con KPIs | ✅ |

### 📋 Maestros
| Ruta | Descripción | Estado |
|------|-------------|--------|
| `/maestros/clientes` | Gestión de clientes | ✅ |
| `/maestros/proveedores` | Gestión de proveedores | ✅ |
| `/maestros/productos` | Gestión de productos | ✅ |
| `/maestros/categorias` | Categorías de productos | ✅ |
| `/maestros/almacenes` | Gestión de almacenes | ✅ |
| `/maestros/unidades-medida` | Unidades de medida | ✅ |

### 📦 Inventario
| Ruta | Descripción | Estado |
|------|-------------|--------|
| `/inventario/kardex` | Kardex valorizado | ✅ |
| `/inventario/movimientos` | Movimientos de inventario | ✅ |
| `/inventario/ajustes` | Ajustes de inventario | ✅ |
| `/inventario/transferencias` | Transferencias entre almacenes | ✅ |

### 🛒 Compras
| Ruta | Descripción | Estado |
|------|-------------|--------|
| `/compras/ordenes` | Órdenes de compra | ✅ |
| `/compras/recepcion` | Recepción de mercadería | ✅ |
| `/compras/importacion` | Flujo de importación | ✅ |

### 💰 Ventas
| Ruta | Descripción | Estado |
|------|-------------|--------|
| `/ventas/cotizaciones` | Cotizaciones | ✅ |
| `/ventas/pedidos` | Pedidos de venta | ✅ |
| `/ventas/facturas` | Facturas | ✅ |

### 📊 Contabilidad
| Ruta | Descripción | Estado |
|------|-------------|--------|
| `/contabilidad/plan-cuentas` | Plan de cuentas | 🟡 |
| `/contabilidad/asientos` | Asientos contables | 🟡 |

### ⚙️ Configuración
| Ruta | Descripción | Estado |
|------|-------------|--------|
| `/config/empresas` | Gestión de empresas | ✅ |
| `/config/monedas` | Gestión de monedas | ✅ |
| `/config/numeracion` | Numeración de documentos | ✅ |
| `/config/parametros` | Parámetros del sistema | ✅ |

---

## 🚀 GUÍA DE INICIO RÁPIDO

### Requisitos Previos

| Software | Versión | Descarga |
|----------|---------|----------|
| .NET SDK | 8.0+ | https://dotnet.microsoft.com/download |
| SQL Server | 2019+ | https://www.microsoft.com/sql-server |
| Visual Studio | 2022/2026 | https://visualstudio.microsoft.com/ |
| Git | Latest | https://git-scm.com/ |

### Paso 1: Clonar Repositorio

```powershell
git clone https://github.com/abelcalvimontes/AgoraHUB360-ERP
cd AgoraHUB360-ERP
```

### Paso 2: Configurar Base de Datos

Editar `src/AgoraHub360.ERP.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SERVIDOR,PUERTO;Database=db_AgoraERP_Core;User Id=USUARIO;Password=PASSWORD;TrustServerCertificate=true;MultipleActiveResultSets=true;Encrypt=false"
  },
  "Jwt": {
    "Key": "AgoraHub360-ERP-Dev-Secret-Key-2026-MinLength32!",
    "Issuer": "AgoraHub360.ERP",
    "Audience": "AgoraHub360.ERP.Web",
    "ExpirationMinutes": 480
  }
}
```

### Paso 3: Crear Base de Datos

```powershell
# Desde la raíz del proyecto
dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api
```

### Paso 4: Iniciar el Sistema

#### Opción A: Script Automatizado (Recomendado) 🚀

```powershell
.\start-system.ps1
```

El script automáticamente:
- ✅ Detiene procesos previos
- ✅ Inicia la API (puerto 7001)
- ✅ Inicia la Web (puerto 5002)
- ✅ Abre el navegador en el login
- ✅ Muestra las credenciales

#### Opción B: Inicio Manual

**Terminal 1 - API:**
```powershell
cd src\AgoraHub360.ERP.Api
dotnet run --launch-profile https
```

**Terminal 2 - Web:**
```powershell
cd src\AgoraHub360.ERP.Web
dotnet run --launch-profile https
```

### Paso 5: Acceder al Sistema

| Componente | URL | Estado |
|------------|-----|--------|
| **Aplicación Web** | https://localhost:5002 | ✅ |
| **Login** | https://localhost:5002/login | ✅ |
| **API** | https://localhost:7001 | ✅ |
| **Swagger** | https://localhost:7001/swagger | ✅ |

### 🔐 Credenciales de Acceso

El sistema crea automáticamente un usuario administrador:

| Campo | Valor |
|-------|-------|
| **Email** | `admin@agorahub360.com` |
| **Contraseña** | `Admin123` |
| **Rol** | Admin |

⚠️ **Importante:** Cambia estas credenciales antes de pasar a producción.

---

## 🧪 SCRIPTS DE TESTING

### Test de Conexión a Base de Datos

#### Test Rápido (sin iniciar la API)
```powershell
.\quick-db-test.ps1
```

#### Test Completo (con API corriendo)
```powershell
# Terminal 1: Inicia la API
cd src\AgoraHub360.ERP.Api
dotnet run

# Terminal 2: Ejecuta el test
.\test-database-connection.ps1
```

### Resetear Usuario Administrador

```powershell
# Método rápido
.\reset-admin-user.ps1

# Método alternativo (genera SQL)
.\reset-admin-simple.ps1
```

---

## 📈 ROADMAP Y VERSIONES

### v1.0 (Actual) - MVP Operativo ✅
**Fecha:** Febrero - Abril 2026  
**Estado:** Completo

**Módulos:**
- ✅ CORE (Seguridad, Multiempresa, Auditoría)
- ✅ MDM (Maestros de Datos)
- ✅ Inventario Avanzado
- ✅ Compras + Importación con Prorrateo
- ✅ Ventas (sin facturación electrónica)
- 🟡 Contabilidad Básica (60%)
- 🟡 Logística Básica (40%)

**Tecnologías:**
- .NET 8.0
- Blazor WebAssembly
- SQL Server 2019+
- EF Core 8.0
- JWT Authentication

---

### v1.1 (Próximo) - Facturación Electrónica
**Fecha Estimada:** Q2 2026 (Mayo - Junio)  
**Estado:** Planificado

**Features:**
- 🔄 Integración con SIN (Servicio de Impuestos Nacionales - Bolivia)
- 🔄 Facturación electrónica
- 🔄 Timbrado digital
- 🔄 CUIS y CUFD
- 🔄 Modalidades de facturación (en línea/contingencia)
- 🔄 Códigos de actividad y productos

---

### v1.2 - Business Intelligence
**Fecha Estimada:** Q3 2026 (Julio - Septiembre)  
**Estado:** Planificado

**Features:**
- 🔄 Dashboards interactivos
- 🔄 Reportes avanzados (PDF, Excel)
- 🔄 KPIs y métricas personalizables
- 🔄 Gráficos dinámicos
- 🔄 Análisis de tendencias
- 🔄 Reportes financieros

---

### v2.0 - Producción y MRP
**Fecha Estimada:** Q4 2026 (Octubre - Diciembre)  
**Estado:** Planificado

**Features:**
- 🔄 Módulo de Producción
- 🔄 MRP (Material Requirements Planning)
- 🔄 BOM (Bill of Materials)
- 🔄 Órdenes de producción
- 🔄 Control de calidad
- 🔄 Costos de producción

---

### v2.x - Automatización y Escalabilidad
**Fecha Estimada:** 2027  
**Estado:** Planificado

**Features:**
- 🔄 Workflows automatizados
- 🔄 Integraciones con sistemas externos
- 🔄 API pública para terceros
- 🔄 Webhooks
- 🔄 Notificaciones push
- 🔄 Mobile app (iOS/Android)
- 🔄 Migración a microservicios

---

## 🔐 SEGURIDAD Y AUDITORÍA

### Autenticación

**JWT (JSON Web Tokens)**
```csharp
// Configuración en Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "AgoraHub360.ERP",
            ValidAudience = "AgoraHub360.ERP.Web",
            IssuerSigningKey = new SymmetricSecurityKey(...)
        };
    });
```

**Características:**
- ✅ Tokens con expiración (8 horas por defecto)
- ✅ Refresh tokens
- ✅ Revocación de tokens
- ✅ Claims personalizados (EmpresaId, Rol, UserId)

### Autorización

**RBAC (Role-Based Access Control)**
```csharp
[Authorize(Roles = "Admin")]
[Authorize(Roles = "Admin,Gerente")]
[Authorize(Policy = "RequireAdminRole")]
```

**Roles predefinidos:**
- `Admin` - Acceso total
- `Gerente` - Acceso a módulos operativos
- `Vendedor` - Acceso a ventas
- `Almacenero` - Acceso a inventario
- `Contador` - Acceso a contabilidad

### Multiempresa (Tenant Isolation)

**Implementación:**
```csharp
// Todas las entidades incluyen
public int EmpresaId { get; set; }
public Empresa Empresa { get; set; } = null!;

// Filtro automático en EF Core
modelBuilder.Entity<Producto>()
    .HasQueryFilter(p => p.EmpresaId == CurrentEmpresaId);
```

**Características:**
- ✅ Aislamiento total de datos por empresa
- ✅ Filtros automáticos en todas las consultas
- ✅ Validación de empresa en cada request
- ✅ Seguridad a nivel de base de datos

### Auditoría Automática

**Interceptor de EF Core:**
```csharp
public class AuditInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(...)
    {
        // Automáticamente agrega:
        // - CreatedBy
        // - CreatedAt
        // - ModifiedBy
        // - ModifiedAt
        // - IsDeleted (soft delete)
    }
}
```

**Campos de auditoría:**
```csharp
public abstract class AuditableEntity : BaseEntity
{
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
}
```

### Versionamiento de Entidades

**EntityVersion Pattern:**
```csharp
public class EntityVersion
{
    public int Id { get; set; }
    public string EntityName { get; set; } = null!;
    public int EntityId { get; set; }
    public int Version { get; set; }
    public string Changes { get; set; } = null!;  // JSON
    public string ChangedBy { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
}
```

**Características:**
- ✅ Historial completo de cambios
- ✅ Rollback de versiones
- ✅ Auditoría detallada

---

## 🗄️ BASE DE DATOS

### Esquema de Tablas

#### Core (Sistema Base)
```sql
-- Multiempresa
[core].[Empresas]
[core].[Usuarios]
[core].[Roles]
[core].[Permisos]
[core].[UsuarioRol]
[core].[RolPermiso]

-- Parámetros
[core].[Monedas]
[core].[TiposCambio]
[core].[ParametrosSistema]
[core].[NumeracionDocumentos]
```

#### MDM (Maestros)
```sql
[mdm].[Clientes]
[mdm].[Proveedores]
[mdm].[Productos]
[mdm].[CategoriasProducto]
[mdm].[UnidadesMedida]
[mdm].[Almacenes]
```

#### Inventario
```sql
[inv].[Inventarios]
[inv].[MovimientosInventario]
[inv].[Kardex]
[inv].[AjustesInventario]
[inv].[TransferenciasInventario]
```

#### Compras
```sql
[cmp].[OrdenesCompra]
[cmp].[OrdenesCompraDetalle]
[cmp].[RecepcionesCompra]
[cmp].[GastosImportacion]
[cmp].[ProrrateosImportacion]
```

#### Ventas
```sql
[ven].[Cotizaciones]
[ven].[CotizacionesDetalle]
[ven].[Pedidos]
[ven].[PedidosDetalle]
[ven].[Facturas]
[ven].[FacturasDetalle]
```

#### Contabilidad
```sql
[con].[CuentasContables]
[con].[AsientosContables]
[con].[AsientosContablesDetalle]
[con].[CentrosCosto]
```

#### Versionamiento
```sql
[ver].[EntityVersions]
```

### Migraciones

**Historial de Migraciones:**
```
20260101000000_InitialCreate
20260115000000_AddAuditing
20260120000000_AddMultiempresa
20260125000000_AddInventory
20260201000000_AddSales
20260210000000_AddPurchases
20260220000000_AddAccounting
20260305065107_CMP_FlujoImportacionCompleto
... (15+ migraciones)
```

**Comandos útiles:**
```powershell
# Crear nueva migración
dotnet ef migrations add NombreMigracion --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api

# Aplicar migraciones
dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api

# Ver historial
dotnet ef migrations list --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api

# Rollback a migración específica
dotnet ef database update NombreMigracion --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api

# Generar script SQL
dotnet ef migrations script --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api --output migration.sql
```

---

## 📚 DOCUMENTACIÓN DISPONIBLE

| Documento | Descripción | Ubicación |
|-----------|-------------|-----------|
| **README.md** | Guía principal del proyecto | `/README.md` |
| **DEPLOYMENT_GUIDE.md** | Guía completa de despliegue | `/docs/DEPLOYMENT_GUIDE.md` |
| **COMMIT_CONVENTION.md** | Convenciones de commits | `/docs/COMMIT_CONVENTION.md` |
| **GIT_WORKFLOW.md** | Flujo de trabajo con Git | `/docs/GIT_WORKFLOW.md` |
| **PROJECT_BOARD_SETUP.md** | Configuración del tablero | `/docs/PROJECT_BOARD_SETUP.md` |
| **DATABASE_TEST_GUIDE.md** | Guía de testing de BD | `/docs/DATABASE_TEST_GUIDE.md` |
| **DATABASE_TEST_SUMMARY.md** | Resumen de testing | `/docs/DATABASE_TEST_SUMMARY.md` |
| **DATABASE_TEST_REPORT.md** | Reporte técnico de tests | `/docs/DATABASE_TEST_REPORT.md` |
| **DATABASE_CONNECTION_FIX.md** | Solución de problemas BD | `/docs/DATABASE_CONNECTION_FIX.md` |
| **LOGIN-ERROR-FIX.md** | Solución error de login | `/docs/LOGIN-ERROR-FIX.md` |
| **RESET-ADMIN-GUIDE.md** | Guía completa reseteo admin | `/docs/RESET-ADMIN-GUIDE.md` |
| **RESET-ADMIN-QUICKSTART.md** | Guía rápida reseteo admin | `/docs/RESET-ADMIN-QUICKSTART.md` |
| **CREDENCIALES-DEFAULT.md** | Credenciales por defecto | `/docs/CREDENCIALES-DEFAULT.md` |
| **ESTADO_PROYECTO_COMPLETO.md** | Este documento | `/ESTADO_PROYECTO_COMPLETO.md` |

---

## 🎯 MODELO ÁGORA (MAPE)

El proyecto sigue el **Modelo Ágora de Planificación Estructurada (MAPE)**, una metodología propietaria que garantiza calidad y escalabilidad:

### 1. Diagnóstico
- Análisis profundo del problema
- Identificación de requisitos
- Evaluación de alternativas tecnológicas

### 2. Diseño Arquitectónico
- Clean Architecture
- Domain-Driven Design
- Principios SOLID
- Patrones de diseño

### 3. Desarrollo por Capas
- Domain → Application → Persistence → Infrastructure → API → Web
- Cada capa independiente y testeable
- Separación de responsabilidades

### 4. Validación Controlada
- Unit tests
- Integration tests
- Validación funcional
- Code review obligatorio

### 5. Implementación Guiada
- Despliegue planificado
- Documentación completa
- Scripts de automatización
- Monitoreo continuo

### 6. Evolución Continua
- Feedback constante
- Mejoras iterativas
- Roadmap estructurado
- Versiones planificadas

> ⚡ **Principio fundamental:** No se libera sin validación estructural

---

## 📊 MÉTRICAS DEL PROYECTO

### Código

| Métrica | Valor |
|---------|-------|
| **Proyectos en Solución** | 8 |
| **Líneas de Código (aprox.)** | 50,000+ |
| **Archivos de Código** | 300+ |
| **Entidades de Dominio** | 80+ |
| **Servicios de Aplicación** | 25+ |

### API

| Métrica | Valor |
|---------|-------|
| **Controllers** | 15+ |
| **Endpoints** | 120+ |
| **Versiones API** | 1 (v1) |
| **Autenticación** | JWT |

### Frontend

| Métrica | Valor |
|---------|-------|
| **Páginas Blazor** | 35+ |
| **Componentes** | 50+ |
| **Archivos CSS** | 10+ |
| **Tamaño WASM (aprox.)** | 8 MB |

### Base de Datos

| Métrica | Valor |
|---------|-------|
| **Tablas** | 60+ |
| **Migraciones** | 15+ |
| **Stored Procedures** | 0 (EF Core) |
| **Esquemas** | 7 (core, mdm, inv, cmp, ven, con, ver) |

---

## ✅ ESTADO DE MÓDULOS RESUMIDO

| Módulo | Estado | Completitud | Próximos Pasos |
|--------|--------|-------------|----------------|
| 🔐 **CORE** | 🟢 Completo | 100% | Mantenimiento y optimización |
| 📋 **MDM** | 🟢 Completo | 100% | Mantenimiento |
| 📦 **Inventario** | 🟢 Completo | 100% | Optimizaciones de performance |
| 🛒 **Compras** | 🟢 Completo | 100% | Optimizaciones |
| 💰 **Ventas** | 🟢 Completo | 95% | Facturación SIN (v1.1) |
| 📊 **Contabilidad** | 🟡 En desarrollo | 60% | Asientos, libros, reportes |
| 🚚 **Logística** | 🟡 En desarrollo | 40% | Tracking, transportistas |
| 📈 **BI/Reportes** | 🔄 Planificado | 0% | v1.2 (Q3 2026) |
| 🏭 **Producción** | 🔄 Planificado | 0% | v2.0 (Q4 2026) |

**Leyenda:**
- 🟢 Completo y Operativo
- 🟡 En Desarrollo Activo
- 🔄 Planificado

---

## 🛠️ TECNOLOGÍAS Y LIBRERÍAS

### Backend

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| **.NET** | 8.0 | Framework principal |
| **C#** | 12.0 | Lenguaje de programación |
| **ASP.NET Core** | 8.0 | Web API |
| **Entity Framework Core** | 8.0.12 | ORM |
| **SQL Server** | 2019+ | Base de datos |
| **JWT Bearer** | 8.0.12 | Autenticación |
| **Asp.Versioning** | 8.1.0 | Versionado de API |
| **Swashbuckle** | 6.6.2 | Documentación Swagger |
| **EPPlus** | 7.5.2 | Exportación a Excel |

### Frontend

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| **Blazor WebAssembly** | 8.0.24 | Framework SPA |
| **Bootstrap** | 5.3 | Framework CSS |
| **Bootstrap Icons** | 1.11 | Iconos |
| **C#** | 12.0 | Lenguaje (Blazor) |

### Herramientas de Desarrollo

| Herramienta | Versión | Propósito |
|-------------|---------|-----------|
| **Visual Studio** | 2026 (18.4.3) | IDE principal |
| **Git** | Latest | Control de versiones |
| **PowerShell** | 7+ | Scripts de automatización |
| **SQL Server Management Studio** | Latest | Gestión de BD |

---

## 🚦 HEALTH CHECKS Y MONITOREO

### Endpoints de Health Check

```http
GET /health                            # Health check general
GET /api/v1/diagnostics/ping           # Test de API
GET /api/v1/diagnostics/database-test  # Test de BD
```

### Respuestas

**Health Check Exitoso:**
```json
{
  "status": "Healthy",
  "duration": "00:00:00.123",
  "info": {
    "database": "Healthy"
  }
}
```

**Health Check con Problemas:**
```json
{
  "status": "Unhealthy",
  "duration": "00:00:05.000",
  "error": {
    "database": "Timeout connecting to database"
  }
}
```

---

## 🐛 TROUBLESHOOTING

### Problema: Error al conectar con la base de datos

**Síntomas:**
```
SqlException: A network-related or instance-specific error occurred
```

**Solución:**
1. Verificar que SQL Server esté corriendo
2. Verificar la cadena de conexión en `appsettings.json`
3. Ejecutar `.\quick-db-test.ps1`
4. Revisar el firewall y permisos

**Documentación:** `docs/DATABASE_CONNECTION_FIX.md`

---

### Problema: Error 401 Unauthorized al hacer login

**Síntomas:**
```
Error de conexión con el servidor
401 Unauthorized
```

**Solución:**
1. Verificar que la API esté corriendo en `https://localhost:7001`
2. Verificar configuración de CORS en `Program.cs`
3. Verificar que el JWT esté configurado correctamente
4. Resetear usuario admin: `.\reset-admin-user.ps1`

**Documentación:** `docs/LOGIN-ERROR-FIX.md`

---

### Problema: Migraciones no se aplican

**Síntomas:**
```
The entity type 'X' requires a primary key to be defined
```

**Solución:**
```powershell
# Eliminar base de datos
dotnet ef database drop --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api --force

# Recrear base de datos
dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api
```

---

### Problema: Blazor no carga (pantalla en blanco)

**Síntomas:**
- Pantalla blanca
- Consola de navegador muestra errores de CORS

**Solución:**
1. Verificar que la API esté corriendo
2. Verificar URL de la API en `Program.cs` del proyecto Web
3. Verificar configuración de CORS en la API
4. Limpiar caché del navegador (Ctrl+Shift+Del)

---

## 📞 SOPORTE Y CONTACTO

### Equipo de Desarrollo

**Proyecto desarrollado por:** Ágora HUB 360 – Dirección Ágora Tech

### Recursos

| Recurso | Enlace |
|---------|--------|
| **Repositorio GitHub** | https://github.com/abelcalvimontes/AgoraHUB360-ERP |
| **Issues** | https://github.com/abelcalvimontes/AgoraHUB360-ERP/issues |
| **Documentación** | `/docs/` |

### Licencia

**Licencia:** Propietario  
**Copyright:** © 2026 Ágora HUB 360

---

## 📝 CONVENCIONES DE CÓDIGO

### Commits

Seguir la convención de Conventional Commits:

```
<tipo>(<ámbito>): <descripción>

[cuerpo opcional]

[pie opcional]
```

**Tipos:**
- `feat`: Nueva funcionalidad
- `fix`: Corrección de bugs
- `docs`: Cambios en documentación
- `style`: Cambios de formato
- `refactor`: Refactorización
- `test`: Añadir tests
- `chore`: Tareas de mantenimiento

**Ejemplos:**
```
feat(ventas): agregar módulo de cotizaciones
fix(api): corregir error en login
docs(readme): actualizar guía de instalación
```

**Documentación completa:** `docs/COMMIT_CONVENTION.md`

### Branches

```
main              # Producción
develop           # Desarrollo
feature/nombre    # Nueva funcionalidad
bugfix/nombre     # Corrección de bug
hotfix/nombre     # Corrección urgente en producción
release/v1.x      # Preparación de release
```

**Documentación completa:** `docs/GIT_WORKFLOW.md`

---

## 📅 CHANGELOG

### v1.0.0 (Abril 2026)

**Features:**
- ✅ Sistema Core completo (Multiempresa, Seguridad, Auditoría)
- ✅ Maestros de Datos (Clientes, Proveedores, Productos, etc.)
- ✅ Inventario Valorizado con Kardex
- ✅ Compras e Importación con Prorrateo
- ✅ Ventas (Cotizaciones, Pedidos, Facturas internas)
- 🟡 Contabilidad Básica (en desarrollo)
- 🟡 Logística Básica (en desarrollo)

**Tecnologías:**
- .NET 8.0
- Blazor WebAssembly
- SQL Server 2019+
- EF Core 8.0
- JWT Authentication

**Documentación:**
- 15+ documentos técnicos
- Scripts de automatización
- Guías de despliegue
- Troubleshooting

---

## 🎓 RECURSOS DE APRENDIZAJE

### Para Nuevos Desarrolladores

| Tema | Recurso | Prioridad |
|------|---------|-----------|
| **Clean Architecture** | README.md → Arquitectura | 🔴 Alta |
| **Entity Framework Core** | Microsoft Docs | 🔴 Alta |
| **Blazor WebAssembly** | Microsoft Docs | 🔴 Alta |
| **Convenciones de Commits** | docs/COMMIT_CONVENTION.md | 🟠 Media |
| **Git Workflow** | docs/GIT_WORKFLOW.md | 🟠 Media |
| **Despliegue** | docs/DEPLOYMENT_GUIDE.md | 🟢 Baja |

### Orden Recomendado de Lectura

1. `README.md` - Visión general
2. `ESTADO_PROYECTO_COMPLETO.md` - Este documento
3. `docs/GIT_WORKFLOW.md` - Flujo de trabajo
4. `docs/COMMIT_CONVENTION.md` - Convenciones
5. Explorar código en orden: Domain → Application → Persistence → API → Web

---

## 🎯 PRÓXIMOS HITOS

### Corto Plazo (1-2 meses)

- [ ] Completar módulo de Contabilidad (Asientos, Libros)
- [ ] Completar módulo de Logística (Tracking)
- [ ] Optimizaciones de performance
- [ ] Tests unitarios (cobertura >80%)
- [ ] Documentación de API (Swagger completo)

### Mediano Plazo (3-6 meses)

- [ ] Integración con SIN (Facturación Electrónica Bolivia)
- [ ] Dashboards y BI
- [ ] Reportes avanzados
- [ ] Mobile app (PWA)

### Largo Plazo (6-12 meses)

- [ ] Módulo de Producción
- [ ] MRP (Material Requirements Planning)
- [ ] Migración a microservicios
- [ ] API pública para terceros
- [ ] Integraciones con sistemas externos

---

## 🏆 LOGROS DEL PROYECTO

### Técnicos

✅ Arquitectura limpia y escalable  
✅ Multiempresa desde el núcleo  
✅ Auditoría automática completa  
✅ Versionamiento de entidades  
✅ Prorrateo automático de costos de importación  
✅ Blazor WebAssembly 100% funcional  
✅ API REST con versionado  
✅ JWT Authentication  
✅ Documentación extensa  

### Funcionales

✅ Sistema operativo end-to-end  
✅ Flujo completo: Compras → Inventario → Ventas  
✅ Gestión de importaciones completa  
✅ Control de inventario multi-almacén  
✅ Kardex valorizado  
✅ Contabilidad básica  

---

## 🔮 VISIÓN FUTURA

AgoraHub360 ERP aspira a convertirse en:

- 🎯 **Plataforma ERP Central** del ecosistema Ágora HUB 360
- 🌎 **Solución SaaS Regional** para empresas en Latinoamérica
- 🏭 **Sistema Integral** que cubra todo el ciclo empresarial
- 🤖 **Plataforma Inteligente** con IA y automatización
- 🔗 **Hub de Integración** con sistemas de terceros

---

## 📊 DASHBOARD DE ESTADO

```
┌────────────────────────────────────────────────────────────┐
│              AGORAHUB360 ERP - ESTADO ACTUAL               │
├────────────────────────────────────────────────────────────┤
│  Versión:        v1.0.0 MVP                                │
│  Estado:         🟢 Operativo                              │
│  Completitud:    85%                                       │
│  Tecnología:     .NET 8 + Blazor WASM                      │
│  Base de Datos:  SQL Server 2019+                          │
├────────────────────────────────────────────────────────────┤
│  MÓDULOS:                                                  │
│  ✅ Core          100%  🟢 Operativo                       │
│  ✅ MDM           100%  🟢 Operativo                       │
│  ✅ Inventario    100%  🟢 Operativo                       │
│  ✅ Compras       100%  🟢 Operativo                       │
│  ✅ Ventas         95%  🟢 Operativo (falta SIN)          │
│  🟡 Contabilidad   60%  🟡 En desarrollo                   │
│  🟡 Logística      40%  🟡 En desarrollo                   │
├────────────────────────────────────────────────────────────┤
│  PRÓXIMA VERSIÓN: v1.1 - Facturación Electrónica SIN      │
│  FECHA ESTIMADA:  Q2 2026 (Mayo-Junio)                    │
└────────────────────────────────────────────────────────────┘
```

---

## 🎉 CONCLUSIÓN

**AgoraHub360 ERP v1.0** es un sistema ERP empresarial completo y funcional que implementa:

✅ **Arquitectura moderna** (Clean Architecture + DDD)  
✅ **Multiempresa nativo** (aislamiento total de datos)  
✅ **Módulos operativos completos** (Compras, Inventario, Ventas)  
✅ **Prorrateo automático de importaciones** (feature única)  
✅ **Auditoría y versionamiento** (trazabilidad completa)  
✅ **Frontend moderno** (Blazor WebAssembly)  
✅ **API REST robusta** (versionada y documentada)  

El sistema está listo para **producción** en su alcance MVP, con un roadmap claro hacia funcionalidades avanzadas (Facturación Electrónica, BI, Producción).

---

**Documento generado:** Abril 2026  
**Versión del documento:** 1.0  
**Autor:** Ágora HUB 360 - Dirección Ágora Tech  
**Estado del proyecto:** ✅ MVP v1.0 Operativo

---

*Para más información, consultar la documentación en `/docs/` o el repositorio en GitHub.*
