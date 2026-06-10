# AGORAHUB360 ERP — GUÍA MAESTRA DE ARQUITECTURA

> **Versión:** 1.0
> **Fecha:** 2026-06-10
> **Propósito:** Documento maestro de referencia arquitectónica y funcional del sistema AgoraHUB360 ERP.

---

## 1. Resumen Ejecutivo

### ¿Qué es AgoraHUB360?

**AgoraHUB360** es un sistema ERP (Enterprise Resource Planning) modular, multiempresa y multitenant, construido sobre **.NET 8** con **Clean Architecture** como principio rector. Está diseñado para la gestión integral de PyMEs en Bolivia, cubriendo desde la gestión de productos (MDM) hasta la contabilidad completa, facturación electrónica (SIAT), inventarios, ventas, compras, CxC y activos fijos.

### Objetivos

| Objetivo | Descripción |
|---|---|
| Modularidad | Cada módulo funcional es autocontenido y desacoplado |
| Multiempresa | Un solo deployment atiende múltiples empresas con aislamiento total de datos |
| Facturación Electrónica | Integración nativa con SIAT Bolivia |
| Escalabilidad | Clean Architecture permite escalar por capas |
| Mantenibilidad | Código limpio, principios SOLID, pruebas unitarias |

### Estado Actual

| Aspecto | Estado |
|---|---|
| Arquitectura | Clean Architecture 100% implementada |
| Domain | ~85 entidades en 15 submódulos |
| Application | ~76 servicios con interfaces |
| API | ~55+ controllers REST versionados |
| Web (Blazor) | ~45+ páginas interactivas |
| Persistencia | EF Core + SQL Server + migraciones |
| Tests | ~22 clases de prueba |
| Facturación SIAT | Integración implementada |
| Multiempresa | Filtro global por tenant (EmpresaId) |

---

## 2. Arquitectura General

### Clean Architecture

```
┌─────────────────────────────────────────────────────┐
│                   UI / API Layer                     │
│   (Blazor Server · REST API · Swagger)              │
├─────────────────────────────────────────────────────┤
│              Application Layer                        │
│   (Services · Interfaces · Validators · DTOs)       │
├─────────────────────────────────────────────────────┤
│              Domain Layer                             │
│   (Entities · Enums · Value Objects · Interfaces)   │
├─────────────────────────────────────────────────────┤
│              Persistence Layer                        │
│   (EF Core · DbContext · Repositories · Migrations) │
├─────────────────────────────────────────────────────┤
│           Infrastructure Layer                        │
│   (Servicios externos · Email · Archivos)           │
└─────────────────────────────────────────────────────┘
```

### Proyectos de la solución

| Proyecto | Rol | Dependencias |
|---|---|---|
| `AgoraHub360.ERP.Domain` | Capa de dominio (entidades, enums, interfaces de repositorio) | — |
| `AgoraHub360.ERP.Application` | Casos de uso, servicios de aplicación, interfaces de servicio | Domain, Shared |
| `AgoraHub360.ERP.Persistence` | EF Core DbContext, configuraciones, repositorios, migraciones | Domain, Application |
| `AgoraHub360.ERP.Infrastructure` | Servicios externos (email, archivos, etc.) | Application |
| `AgoraHub360.ERP.Shared` | DTOs, constantes, utilerías compartidas | — |
| `AgoraHub360.ERP.Api` | REST API, controllers, middleware, JWT | Application, Persistence, Infrastructure |
| `AgoraHub360.ERP.Web` | Blazor Server, páginas Razor, servicios HTTP | Shared |
| `AgoraHub360.ERP.Tests` | Pruebas unitarias y de seguridad | Application, Domain |

### Multiempresa y Tenant

El ERP implementa un modelo **tenant-aware** donde cada `Empresa` actúa como tenant. El aislamiento se logra mediante:

1. **`TenantEntity`** — Clase base abstracta con propiedad `EmpresaId`. Todas las entidades que pertenecen a una empresa heredan de ella.
2. **Query Filter Global** — En `AgoraDbContext.OnModelCreating()`, se aplica automáticamente un filtro `WHERE EmpresaId = @currentUser.EmpresaId` a todas las entidades `TenantEntity`.
3. **`CurrentUserService`** — Extrae el `EmpresaId` del claim JWT `ClaimTypesCustom.EmpresaId`.
4. **`TenantRequiredMiddleware`** — Middleware HTTP que valida que las peticiones incluyan el `EmpresaId` en rutas tenant-aware.

### JWT y Autenticación

- **Autenticación**: JWT Bearer con emisor único (`AgoraHub360.ERP`)
- **Claims personalizados**: `PlatformRole`, `TenantRole`, `EmpresaId`, `TenantStatus` (definidos en `Shared/Constants/ClaimTypesCustom.cs`)
- **StubAuthHandler**: Esquema de autenticación alternativo para desarrollo
- **Roles**: `PlatformSuperAdmin`, `PlatformAdmin`, `TenantAdmin`, `TenantUser`
- **Políticas**: Definidas en `Shared/Constants/PolicyNames.cs`
- **Handlers de autorización**: `TenantMembershipHandler`, `BranchAccessHandler`

### Esquema de Capas y Flujo de Datos

```
[Cliente Web/API]
       │
       ▼
┌──────────────────┐
│  Controller/Page  │ ← Recibe request, llama al servicio
│  (API/Web)       │
└──────┬───────────┘
       │ Inyección de dependencia
       ▼
┌──────────────────┐
│  Application     │ ← Valida reglas, orquesta repositorios
│  Service         │
└──────┬───────────┘
       │
       ▼
┌──────────────────┐
│  Domain Entity   │ ← Modelo de negocio (POCO)
└──────┬───────────┘
       │
       ▼
┌──────────────────┐
│  Repository      │ ← Persistencia (EF Core)
│  (Persistence)   │
└──────┬───────────┘
       │
       ▼
   [SQL Server]
```

---

## 3. Módulos del Sistema

### 3.1. Core (`Entities/Core/`)

Gestión de la empresa, usuarios, roles, sucursales, monedas, geografía y parámetros del sistema.

| Entidad | Descripción |
|---|---|
| `Empresa` | Entidad principal tenant |
| `Sucursal` | Sucursales por empresa |
| `Usuario` | Usuarios del sistema (cross-tenant) |
| `UsuarioEmpresa` | Asignación usuario ↔ empresa |
| `Rol` | Roles de acceso |
| `PerfilAcceso`, `PerfilPermiso` | Permisos detallados |
| `ModuloSistema`, `FormularioSistema`, `AccionSistema` | Catálogo de módulos y acciones |
| `Moneda`, `Pais`, `Departamento`, `Provincia`, `Ciudad`, `Zona` | Geografía maestra |
| `ParametroSistema` | Parámetros configurables |
| `NumeracionDocumento` | Control de numeración por tipo de documento |

### 3.2. MDM — Maestro de Datos (`Entities/MDM/`)

Gestión de productos, clientes, proveedores, catálogos, marcas, fabricantes, atributos dinámicos y variantes.

| Entidad | Descripción |
|---|---|
| `Product` | Producto global (matriz) |
| `CompanyProduct` | Producto por empresa (precios, códigos) |
| `Cliente` | Cliente con clasificación |
| `ClienteSucursal` | Sucursales del cliente |
| `ClientePerfilFiscal` | Perfil fiscal del cliente (SIAT) |
| `Proveedor` | Proveedores |
| `Almacen`, `UbicacionAlmacen` | Almacenes y ubicaciones |
| `Brand`, `Manufacturer`, `Catalog` | Marcas, fabricantes, catálogos |
| `Category`, `ProductCategory` | Categorías |
| `AttributeDefinition`, `AttributeOption` | Atributos dinámicos |
| `ProductVariant`, `ProductUom` | Variantes y presentaciones |
| `Uom` | Unidades de medida |
| `Contacto`, `ContactoUsuarioAcceso` | Contactos |

### 3.3. Inventario — INV (`Entities/INV/`)

| Entidad | Descripción |
|---|---|
| `MovimientoInventario` | Movimientos de entrada/salida/ajuste |
| `StockProducto` | Stock actual por producto y ubicación |

### 3.4. Ventas — VTA (`Entities/VTA/`)

| Entidad | Descripción |
|---|---|
| `PedidoVenta` | Pedido de cliente (pre-venta) |
| `PedidoVentaDetalle` | Líneas del pedido |
| `Venta` | Venta comercial confirmada |
| `VentaDetalle` | Líneas de venta |
| `VentaPago` | Pagos recibidos contra la venta |
| `VentaFacturacionDatos` | Datos de facturación asociados |
| `FacturaVenta` | Factura electrónica (SIAT) |
| `FacturaVentaDetalle` | Líneas de factura |
| `SiatMetodoPago` | Catálogo SIAT de métodos de pago |

### 3.5. Facturación Electrónica (SIAT)

Integración con SIAT Bolivia para:
- Facturación electrónica (FacturaVenta)
- Catálogos SIAT (SiatMetodoPago)
- Estados de factura: `Borrador`, `Emitida`, `Anulada`, `Rechazada`

### 3.6. Cuentas por Cobrar — CXC (`Entities/CXC/`)

| Entidad | Descripción |
|---|---|
| `CuentaPorCobrar` | Cartera de facturas pendientes |
| `ClienteCreditoConfiguracion` | Límites y condiciones de crédito por cliente |

### 3.7. Pagos (Ventas)

Los pagos se registran contra las ventas (`VentaPago`) con tipos definidos en `TipoPago` (Contado, Crédito, Transferencia, Cheque, QR, etc.). Los pagos actualizan automáticamente el saldo de la `CuentaPorCobrar`.

### 3.8. Compras — CMP (`Entities/CMP/`)

| Entidad | Descripción |
|---|---|
| `OrdenPedido`, `OrdenPedidoLinea` | Solicitud de compra interna |
| `OrdenCompra`, `OrdenCompraLinea` | Orden de compra a proveedor |
| `RecepcionCompra`, `RecepcionCompraLinea` | Recepción de mercancía |
| `ExpedienteImportacion` | Gestión de importaciones |
| `HojaImportacion`, `GastoImportacion`, `ImportacionLinea` | Costos de importación |
| `PagoOrdenCompra` | Pagos a proveedores |

### 3.9. Contabilidad — ACC (`Entities/ACC/`)

| Entidad | Descripción |
|---|---|
| `CuentaContable` | Plan de cuentas |
| `AsientoContable`, `AsientoContableLinea` | Asientos contables |
| `PeriodoContable` | Periodos fiscales |
| `CierreContable` | Cierre de periodos |
| `PlantillaContable`, `PlantillaContableLinea` | Plantillas de asientos |
| `TipoComprobante` | Tipos de comprobante |
| `TipoCambio` | Tipos de cambio |
| `PresupuestoContable`, `PresupuestoContableLinea` | Presupuestos |

### 3.10. Módulos Adicionales

| Módulo | Carpeta | Descripción |
|---|---|---|
| Activos Fijos | `ACT/` | `ActivoFijo`, `DepreciacionMensual` |
| Bancos | `BNC/` | `ExtractoBancario`, `ConciliacionBancaria` |
| Costos | `CST/` | `CentroCosto`, `CostingRule`, `LandedCostProfile` |
| Tributario | `TRB/` | `RegistroImpuesto` |
| Precios | `PRC/` | `PriceList`, `PriceListItem` |
| Reglas | `RUL/` | `Industry`, `ProductIndustryRule` |
| Versionado | `VER/` | `EntityVersion` |
| Documentos | `DOC/` | `Document`, `ComprobanteDocumento`, `ProductDocument` |
| Logística | `LOG/` | `HojaRuta`, `HojaRutaHistorial` |
| Workflow | `Workflow/` | `Tarea`, `PlantillaTarea` |

---

## 4. Roadmap

| Fase | Estado | Descripción |
|---|---|---|
| Fase 1 — Core + MDM | ✅ Completo | Empresa, usuarios, roles, productos, clientes, proveedores |
| Fase 2 — Ventas + CxC | ✅ Completo | Pedidos, ventas, facturas, pagos, cuentas por cobrar |
| Fase 3 — Inventario | ✅ Completo | Movimientos, stock, kardex |
| Fase 4 — Compras | ✅ Completo | Órdenes de pedido, compras, recepciones, importaciones |
| Fase 5 — Contabilidad | ✅ Completo | Plan de cuentas, asientos, periodos, cierre, presupuestos |
| Fase 6 — Facturación SIAT | ✅ Completo | Factura electrónica, métodos de pago SIAT |
| Fase 7 — Módulos especializados | ✅ Completo | Activos fijos, bancos, costos, logística, workflow |
| Fase 8 — UI Web (Blazor) | ✅ Completo | ~45 páginas interactivas |
| Fase 9 — Pruebas y seguridad | 🔄 En progreso | ~22 tests, handlers de autorización |
| Fase 10 — Documentación | 🔄 En progreso | Presente documento |

---

## 5. Riesgos

| Riesgo | Impacto | Mitigación |
|---|---|---|
| Complejidad multiempresa | Alto | Filtro global por tenant + pruebas de aislamiento |
| Facturación SIAT cambiante | Medio | Abstracción mediante interfaces |
| Deuda técnica en UI Blazor | Medio | Refactorización planificada de componentes |
| Cobertura de tests insuficiente | Medio | Plan de aumento de cobertura |
| Migraciones de base de datos | Bajo | Migraciones automáticas con EF Core |
| Seguridad en JWT | Alto | Claims personalizados + handlers de autorización |

---

## 6. Decisiones Arquitectónicas (ADR)

### ADR-001: Clean Architecture

**Contexto**: Se requiere un sistema ERP mantenible y escalable.
**Decisión**: Adoptar Clean Architecture con 4 capas (Domain, Application, Persistence, Infrastructure) más las capas de presentación (API, Web).
**Consecuencias**: Separación clara de responsabilidades, testabilidad, independencia de frameworks.

### ADR-002: Multiempresa vía TenantEntity

**Contexto**: El sistema debe atender múltiples empresas con aislamiento de datos.
**Decisión**: Implementar modelo tenant-aware con `TenantEntity` como clase base y query filter global en EF Core.
**Consecuencias**: Aislamiento automático sin consultas manuales `WHERE EmpresaId = ...`.

### ADR-003: JWT con Claims Personalizados

**Contexto**: Se necesita autorización granular por empresa y rol.
**Decisión**: Usar JWT con claims `PlatformRole`, `TenantRole`, `EmpresaId`, `TenantStatus`.
**Consecuencias**: Stateless authentication, sin sesiones en servidor.

### ADR-004: Repositorio Genérico

**Contexto**: Operaciones CRUD repetitivas en todas las entidades.
**Decisión**: Implementar `IRepository<T>` genérico con métodos `GetById`, `FindAsync`, `Add`, `Update`, `Delete`.
**Consecuencias**: Reducción de código boilerplate, consistencia en acceso a datos.

### ADR-005: Blazor Server como UI

**Contexto**: Se requiere una interfaz web interactiva y familiar (Razor).
**Decisión**: Usar Blazor Server con SignalR para interactividad en tiempo real.
**Consecuencias**: Conexión persistente, baja latencia, pero requiere conexión de red estable.

### ADR-006: Versionado de API

**Contexto**: La API REST debe evolucionar sin romper clientes existentes.
**Decisión**: Usar `Asp.Versioning` con versionado por URL (`/api/v1/...`).
**Consecuencias**: Convivencia de múltiples versiones, migración gradual de clientes.

---

## 7. Stack Tecnológico

| Componente | Tecnología |
|---|---|
| Lenguaje | C# 12 (.NET 8) |
| Framework Web | ASP.NET Core 8 |
| UI | Blazor Server |
| ORM | Entity Framework Core 8 |
| Base de datos | SQL Server |
| Autenticación | JWT Bearer |
| Documentación API | Swagger / OpenAPI |
| Testing | xUnit + FluentAssertions |
| IDE | Visual Studio Code |

---

## 8. Proyectos y Dependencias

```
AgoraHub360.ERP.Domain (0)
     ↑
AgoraHub360.ERP.Shared (0)
     ↑
AgoraHub360.ERP.Application (Domain, Shared)
     ↑
AgoraHub360.ERP.Persistence (Domain, Application)
AgoraHub360.ERP.Infrastructure (Application)
     ↑
AgoraHub360.ERP.Api (Application, Persistence, Infrastructure)
AgoraHub360.ERP.Web (Shared)
     ↑
AgoraHub360.ERP.Tests (Application, Domain)
```

---
