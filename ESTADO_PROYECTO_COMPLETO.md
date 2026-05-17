# AgoraHub360 ERP — Estado Consolidado del Proyecto

> **Fecha de actualización:** 17 de mayo de 2026  
> **Rama principal:** `main`  
> **Repositorio:** `abelcalvimontes/AgoraHUB360-ERP`  
> **Stack:** .NET 8 · C# 12 · Blazor WebAssembly · ASP.NET Core Web API · EF Core 8 · SQL Server  
> **Arquitectura:** Clean Architecture + multiempresa + auditoría transversal

---

## 1. Resumen ejecutivo

**AgoraHub360 ERP** es un ERP modular empresarial diseñado para importación y comercialización, construido como base tecnológica del ecosistema Ágora HUB 360.

A la fecha, el proyecto tiene una base operativa sólida en:

- **Core**
- **MDM**
- **Inventario**
- **Compras / Importación**
- **Contabilidad** (muy avanzada y operativa, aunque con componentes aún pendientes de completar)

El módulo de **Ventas** presenta actividad reciente y ramas de trabajo activas, pero todavía no debe comunicarse como completamente consolidado en `main`.

Adicionalmente, el repositorio incorpora documentación reciente de alto valor sobre el aislamiento **multiempresa / tenant**, que revela que la infraestructura multi-tenant existe, pero aún hay brechas funcionales que deben corregirse a nivel de autenticación, cambio de empresa y algunos servicios.

---

## 2. Estado por área funcional

| Área | Estado | Completitud estimada | Comentarios |
|------|--------|----------------------|-------------|
| Core (usuarios, roles, empresas, sucursales) | ✅ Completo | 100% | Gestión multiempresa, JWT, parámetros, numeración y auditoría |
| MDM | ✅ Completo | 100% | Productos, clientes, proveedores, almacenes, categorías, UDM, listas de precios |
| Inventario | ✅ Completo | 100% | Kardex, movimientos, stock, costo promedio |
| Compras / Importación | ✅ Completo | 100% | Órdenes, recepciones, expediente de importación y landed cost |
| Contabilidad | ✅ Operativo | 95% | Plan de cuentas, asientos, períodos, cierres y reportes principales |
| Ventas | 🔄 En desarrollo | 5–15% | Hay avance reciente, pero el estado aún no es “completo” en `main` |
| CxC / CxP | ⬜ Pendiente | 0% | Roadmap |
| Facturación electrónica SIN / SIAT | ⬜ Pendiente | 0% | Roadmap v1.1 |
| Reportes PDF | 🔄 Parcial | — | Hay piezas y ramas relacionadas, no flujo consolidado completo |
| Tests automatizados | ⬜ Bajo avance | ~5% | Infraestructura existe, cobertura baja |

---

## 3. Arquitectura y estructura de la solución

### Proyectos de la solución

| Proyecto | Tipo | Descripción |
|----------|------|-------------|
| `AgoraHub360.ERP.Domain` | Class Library | Entidades, enums, interfaces y reglas base |
| `AgoraHub360.ERP.Application` | Class Library | Servicios de aplicación, contratos, patrón Result |
| `AgoraHub360.ERP.Persistence` | Class Library | DbContext, EF Core, migraciones, repositorios, interceptores |
| `AgoraHub360.ERP.Infrastructure` | Class Library | Integraciones y servicios externos |
| `AgoraHub360.ERP.Api` | ASP.NET Core Web API | Endpoints REST, auth JWT, middleware, Swagger |
| `AgoraHub360.ERP.Web` | Blazor WebAssembly | Frontend SPA |
| `AgoraHub360.ERP.Shared` | Class Library | DTOs y contratos compartidos |
| `AgoraHub360.ERP.Tests` | xUnit | Pruebas unitarias e integración |

### Principios transversales

- Multiempresa por `EmpresaId`
- Filtros globales tenant-aware en persistencia
- Auditoría automática
- Versionado de entidades
- Soft delete
- Result pattern
- API versionada bajo `/api/v1/`
- Autenticación JWT

---

## 4. Estado funcional por módulo

### CORE — Sistema Base ✅

Incluye:

- autenticación JWT,
- usuarios y roles,
- empresas,
- sucursales,
- parámetros de sistema,
- numeración de documentos,
- auditoría,
- soporte multiempresa.

Las entregas de **empresas y sucursales** quedaron reforzadas durante abril y mayo de 2026.

### MDM — Maestros ✅

Incluye:

- clientes,
- proveedores,
- productos,
- variantes,
- atributos dinámicos,
- unidades de medida,
- categorías,
- marcas,
- fabricantes,
- almacenes,
- listas de precios.

Existen además componentes estructurados sin UI completa, como features activables, reglas por industria y documentos multimedia.

### INVENTARIO ✅

Incluye:

- movimientos,
- stock por almacén,
- kardex,
- integración con recepciones,
- costo promedio.

### COMPRAS / IMPORTACIÓN ✅

Incluye:

- órdenes de compra,
- recepciones parciales o totales,
- expediente de importación,
- landed cost / prorrateo,
- integración con inventario.

### CONTABILIDAD ✅ (operativa)

Incluye:

- plan de cuentas jerárquico,
- comprobantes,
- períodos,
- cierre de gestión,
- plantillas contables,
- balance general,
- estado de resultados,
- sumas y saldos,
- libro diario,
- libro mayor.

Hay componentes aún parciales o sin UI completa, por ejemplo:

- flujo de efectivo,
- conciliación bancaria,
- activos fijos,
- centros de costo,
- presupuestos.

### VENTAS 🔄

Actualmente debe considerarse **en desarrollo activo**. Hay evidencia de trabajo reciente en ramas y PRs, pero no conviene documentarlo como módulo cerrado y completo dentro de `main`.

Estado recomendable de comunicación:

- pedidos / facturas / notas: en construcción,
- integración final de ventas en `main`: todavía no consolidada.

---

## 5. Multiempresa / tenant: estado real

Uno de los hallazgos más importantes recientes del repositorio es que el sistema **sí tiene infraestructura multiempresa real**, pero aún existen brechas entre diseño y comportamiento efectivo.

### Ya implementado

- `TenantEntity` y `EmpresaId`
- Query filters globales
- servicios scoped
- soporte de contexto por usuario autenticado

### Riesgos / hallazgos documentados

Según `ANALISIS-MULTIEMPRESA-TENANT.md`, existen brechas en:

- resolución del tenant al momento del login,
- cambio de empresa realizado solo en frontend,
- endpoints o servicios que pueden bypassear el aislamiento esperado.

### Documento clave

- `ANALISIS-MULTIEMPRESA-TENANT.md`

Este documento debe considerarse obligatorio para cualquier trabajo futuro sobre:

- autenticación,
- empresas,
- sucursales,
- selector de empresa,
- seguridad de datos.

---

## 6. Actividad reciente relevante

### Cambios recientes visibles

- **2026-05-17** — se agrega `ANALISIS-MULTIEMPRESA-TENANT.md`
- **2026-05-11** — actualización de documentación de estado y checklist
- **2026-04-20 a 2026-04-22** — consolidación de cambios en empresas, sucursales y cliente por sucursal
- **2026-04-17** — incorporación de documentación completa de módulos

### PRs recientes de referencia

- **PR #61** — análisis de aislamiento multiempresa / tenant
- **PR #57** — estado actual y checklist de desarrollo
- **PR #55** — cliente sucursal
- **PR #54** — gestión de sucursal / datos cliente por sucursal
- **PR #50** — documentación completa de módulos

---

## 7. Ramas de contexto

Ramas visibles y útiles como referencia del estado evolutivo:

- `main`
- `Venta-Pedido`
- `PedidoSucursal`
- `VentasGestion`
- `feature/pedidos-venta`
- `ClienteGestion_Sucursal`
- `ClienteSucursal`
- `codex/CorreccionMultiempresa`

Estas ramas ayudan a entender qué líneas de trabajo han estado activas recientemente, aunque no toda rama refleja funcionalidad ya consolidada en `main`.

---

## 8. Deuda técnica y pendientes

### Funcionalidades aún pendientes o parciales

- ventas cerradas end-to-end en `main`
- facturación electrónica SIN / SIAT
- cuentas por cobrar
- cuentas por pagar
- reportes PDF integrados en flujos operativos
- conciliación bancaria
- activos fijos con depreciación
- centros de costo con UI
- presupuestos

### Infraestructura pendiente

- cobertura de pruebas significativamente mayor
- CI/CD
- almacenamiento real de documentos
- cierre de brechas de tenant isolation

---

## 9. Roadmap resumido

### v1.1

- ventas completas,
- facturación electrónica,
- CxC,
- CxP,
- reportes PDF.

### v1.2

- dashboards y KPIs,
- exportaciones Excel/PDF más amplias,
- UI de módulos ya estructurados,
- mejora de pruebas,
- CI/CD.

### v2.0+

- BI,
- automatización,
- API pública,
- SaaS multi-tenant más robusto,
- integraciones externas.

---

## 10. Acceso, scripts y arranque

### Credenciales por defecto

| Campo | Valor |
|-------|-------|
| Email | `admin@agorahub360.com` |
| Contraseña | `Admin123` |

> Cambiar antes de producción.

### Scripts útiles

| Script | Función |
|--------|---------|
| `start-system.ps1` | Inicia API y Web |
| `quick-db-test.ps1` | Test rápido BD |
| `test-database-connection.ps1` | Test completo BD |
| `reset-admin-user.ps1` | Reset admin |
| `verify-admin-user.ps1` | Verifica admin |
| `verify-mdm-routes.ps1` | Verifica rutas MDM |

### URLs locales

- Web: `https://localhost:5002`
- Login: `https://localhost:5002/login`
- API: `https://localhost:7001`
- Swagger: `https://localhost:7001/swagger`
- Health: `https://localhost:7001/health`

---

## 11. Documentos de referencia recomendados

### Punto de entrada

- `README.md`

### Estado actual

- `docs/ESTADO_ACTUAL_2026.md`
- `ESTADO_PROYECTO_COMPLETO.md`

### Análisis amplio

- `DOCUMENTACION-MODULOS-COMPLETA.md`
- `docs/ANALISIS_MODULOS_ERP.md`

### Multiempresa y seguridad contextual

- `ANALISIS-MULTIEMPRESA-TENANT.md`

### Operación y soporte

- `docs/DEPLOYMENT_GUIDE.md`
- `CREDENCIALES-DEFAULT.md`
- `RESET-ADMIN-GUIDE.md`
- `RESET-ADMIN-QUICKSTART.md`

---

## 12. Nota de consolidación documental

Este documento reemplaza la visión anterior que mostraba inconsistencias entre el estado real de `main`, el avance de ventas y el nivel de madurez contable.

A partir de esta actualización, debe asumirse como referencia que:

- **Contabilidad está operativa y avanzada**, no en 60%,
- **Ventas sigue en construcción**, aunque con trabajo reciente,
- **multiempresa existe pero requiere correcciones funcionales adicionales**.
