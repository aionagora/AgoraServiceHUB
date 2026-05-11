# AgoraHub360 ERP — Estado Actual del Proyecto

> **Fecha:** 11 de mayo 2026  
> **Versión del sistema:** v1.0.0 (main)  
> **Stack:** .NET 8 · C# 12 · Blazor WebAssembly · ASP.NET Core API · EF Core 8 · SQL Server  
> **Arquitectura:** Clean Architecture (Domain → Application → Persistence/Infrastructure → API → Web)  
> **Repositorio:** `https://github.com/abelcalvimontes/AgoraHUB360-ERP`

---

## Índice

1. [Resumen ejecutivo](#1-resumen-ejecutivo)
2. [Arquitectura y estructura](#2-arquitectura-y-estructura)
3. [Estado funcional por módulo](#3-estado-funcional-por-módulo)
4. [Métricas del proyecto](#4-métricas-del-proyecto)
5. [Historial de migraciones](#5-historial-de-migraciones)
6. [Actividad reciente (git)](#6-actividad-reciente-git)
7. [Ramas activas](#7-ramas-activas)
8. [Deuda técnica y pendientes](#8-deuda-técnica-y-pendientes)
9. [Roadmap](#9-roadmap)
10. [Acceso y configuración](#10-acceso-y-configuración)

---

## 1. Resumen ejecutivo

**AgoraHub360 ERP** es un sistema ERP modular, multiempresa y multisucursal, diseñado para entornos de importación y comercialización. Implementa Clean Architecture sobre .NET 8 con frontend Blazor WebAssembly.

El sistema cubre los módulos **Core, MDM, Inventario, Compras y Contabilidad** en estado operativo completo. El módulo de **Ventas** está en desarrollo activo. La gestión de **Empresas y Sucursales** acaba de completarse (PR #53–55, mayo 2026).

### Resumen de estado por área

| Área | Estado | Completitud |
|------|--------|-------------|
| Core (usuarios, empresas, sucursales) | ✅ Completo | 100% |
| MDM (productos, clientes, proveedores) | ✅ Completo | 100% |
| Inventario | ✅ Completo | 100% |
| Compras | ✅ Completo | 100% |
| Contabilidad | ✅ Completo | 95% |
| Ventas | 🔄 Placeholder | 5% |
| CxC / CxP | ⬜ Pendiente | 0% |
| Facturación electrónica SIN/SIAT | ⬜ Pendiente | 0% |
| Reportes PDF | ⬜ Pendiente | 0% |
| Tests automatizados | ⬜ Pendiente | ~5% |

---

## 2. Arquitectura y estructura

### Proyectos de la solución

| Proyecto | Tipo | Descripción |
|----------|------|-------------|
| `AgoraHub360.ERP.Domain` | Class Library | Entidades, enums, interfaces base |
| `AgoraHub360.ERP.Application` | Class Library | 56 servicios, interfaces, Result pattern |
| `AgoraHub360.ERP.Shared` | Class Library | ~70 DTOs compartidos API ↔ Web |
| `AgoraHub360.ERP.Persistence` | Class Library | EF Core DbContext (180+ DbSets), migraciones, interceptors |
| `AgoraHub360.ERP.Infrastructure` | Class Library | Servicios externos (reservado futuro) |
| `AgoraHub360.ERP.Api` | ASP.NET Core API | 51 controllers REST versionados |
| `AgoraHub360.ERP.Web` | Blazor WASM | 68+ páginas y componentes Razor |
| `AgoraHub360.ERP.Tests` | xUnit | Tests unitarios/integración |

### Puertos y URLs

| Componente | URL |
|------------|-----|
| API (HTTPS) | https://localhost:7001 |
| Web (HTTPS) | https://localhost:5002 |
| Swagger UI | https://localhost:7001/swagger |
| Health Check | https://localhost:7001/health |
| Base de Datos | 192.168.88.14:56885 → `db_AgoraERP_Core` |

### Principios transversales

| Principio | Implementación |
|-----------|---------------|
| Multi-tenant | `TenantEntity.EmpresaId` + Global Query Filter en DbContext |
| Auditoría automática | `AuditableEntityInterceptor` → tabla `AuditLogs` |
| Versionado de entidades | `EntityVersioningInterceptor` → tabla `EntityVersions` |
| Soft delete | `Activo = false` (nunca DELETE físico) |
| Result pattern | `Result<T>.Success / .Failure` en todos los servicios |
| API versionada | URL segment `/api/v1/` + Header `X-Api-Version` |
| Respuesta uniforme | `ApiResponse<T> { success, message, data, errors }` |

---

## 3. Estado funcional por módulo

### CORE — Sistema Base ✅

| Funcionalidad | API | Frontend | Notas |
|---------------|-----|----------|-------|
| Autenticación JWT | ✅ | ✅ | BCrypt + token 8h |
| Gestión de Empresas | ✅ | ✅ | CRUD completo |
| Gestión de Sucursales | ✅ | ✅ | Completado PR #53–55 (mayo 2026) |
| Gestión de Usuarios | ✅ | ✅ | CRUD + asignación empresas/roles |
| Roles | ✅ | ✅ | CRUD completo |
| Parámetros de sistema | ✅ | ✅ | Clave-valor por empresa |
| Numeración de documentos | ✅ | ✅ | Auto-incremento configurable |
| Auditoría | ✅ | ✅ | Log paginado, automático |
| Geografía | ✅ | — | Endpoint disponible |
| Monedas | ✅ | — | Seed inicial |

### MDM — Master Data Management ✅

| Funcionalidad | API | Frontend | Notas |
|---------------|-----|----------|-------|
| Catálogos | ✅ | ✅ | |
| Productos globales | ✅ | ✅ | Wizard 6 pestañas |
| Productos por empresa (CompanyProduct) | ✅ | ✅ | SKU, canales, stock mín/máx |
| Variantes de producto | ✅ | ✅ | Con ejes de variante |
| Atributos dinámicos (EAV) | ✅ | ✅ | Por industria, 7 tipos de dato |
| Códigos de producto | ✅ | ✅ | SKU, EAN, proveedor, etc. |
| UdM alternativas | ✅ | ✅ | Con factores de conversión |
| Categorías jerárquicas | ✅ | ✅ | N niveles |
| Marcas | ✅ | ✅ | |
| Fabricantes | ✅ | ✅ | |
| Clientes | ✅ | ✅ | Con sucursales (PR #54–55) |
| Proveedores | ✅ | ✅ | |
| Almacenes | ✅ | ✅ | |
| Listas de precios | ✅ | ✅ | Con ítems y vigencia |
| Features activables (LOT, SERIAL…) | ✅ | ⬜ | Sin UI |
| Industrias/Reglas | ✅ | ⬜ | Sin UI |
| Documentos multimedia | ✅ estructura | ⬜ | Sin UI ni storage |

### INV — Inventario ✅

| Funcionalidad | API | Frontend | Notas |
|---------------|-----|----------|-------|
| Movimientos (Entrada/Salida/Ajuste/Transferencia) | ✅ | ✅ | |
| Stock por almacén | ✅ | ✅ | Costo promedio WAC |
| Kardex | ✅ | ✅ | |
| Integración automática con Recepciones | ✅ | — | Automático |

### CMP — Compras ✅

| Funcionalidad | API | Frontend | Notas |
|---------------|-----|----------|-------|
| Órdenes de Compra (cabecera + líneas) | ✅ | ✅ | Flujo de estados completo |
| Flujo de estados OC | ✅ | ✅ | Borrador→Confirmado→Aprobado→RecepciónParcial→Cerrado/Anulado |
| Recepciones de Compra | ✅ | ✅ | Parcial/total, genera movimiento INV |
| Importaciones / Landed Cost | ✅ | ✅ | Prorrateo por Peso/Volumen/Valor/Unidades + ajuste WAC |
| Expedientes de importación | ✅ | ✅ | |

### ACC — Contabilidad ✅ (95%)

| Funcionalidad | API | Frontend | Notas |
|---------------|-----|----------|-------|
| Plan de Cuentas jerárquico | ✅ | ✅ | |
| Comprobantes contables (Ingreso/Egreso/Traspaso) | ✅ | ✅ | Partida doble, tipo cambio |
| Períodos contables | ✅ | ✅ | Apertura/Cierre mensual |
| Cierre de gestión | ✅ | ✅ | PR #49 — asientos cierre y apertura |
| Plantillas de contabilización | ✅ | ✅ | Asientos automáticos |
| Balance General | ✅ | ✅ | |
| Estado de Resultados | ✅ | ✅ | |
| Sumas y Saldos | ✅ | ✅ | |
| Libro Diario | ✅ | ✅ | |
| Libro Mayor | ✅ | ✅ | |
| Flujo de Efectivo | ✅ estructura | 🔄 | Parcial |
| Notificaciones períodos próximos a cerrar | ✅ | ✅ | PR #47 (S-06) |
| Conciliación bancaria | ✅ estructura | ⬜ | Sin UI |
| Activos fijos | ✅ estructura | ⬜ | Sin depreciación |
| Centros de costo | ✅ estructura | ⬜ | Sin UI |
| Presupuestos | ⬜ TODO | ⬜ | Pendiente |

### VTA — Ventas 🔄 (5%)

| Funcionalidad | API | Frontend | Notas |
|---------------|-----|----------|-------|
| Pedidos de venta | ⬜ | ⬜ Placeholder | Rama `Venta-Pedido` activa |
| Facturas de venta | ⬜ | ⬜ Placeholder | |
| Notas de crédito/débito | ⬜ | ⬜ | |

---

## 4. Métricas del proyecto

| Categoría | Cantidad |
|-----------|----------|
| Proyectos en solución | 8 + 1 tests |
| Entidades de dominio | 95 |
| Servicios de aplicación | 56 |
| Interfaces de servicio | 56 |
| Controllers API | 51 |
| Endpoints REST | ~200 |
| Páginas Blazor | 68+ |
| HTTP Services (Web) | 34 |
| DTOs (Shared) | ~70 |
| Configuraciones EF Fluent | ~50 |
| Migraciones aplicadas | 20+ |
| Schemas de BD | 10 |
| Tablas en BD | ~55 |
| LOC estimadas | ~37,500+ |

---

## 5. Historial de migraciones

| # | Migración | Módulo | Descripción |
|---|-----------|--------|-------------|
| 1 | `BaseCore` | Core | Empresas, Usuarios, Monedas |
| 2 | `AddRolesTable` | Core | Roles |
| 3 | `AddAuditLogTable` | Core | Log auditoría |
| 4 | `AddParametroSistemaNumeracionDocumento` | Core | Parámetros y numeración |
| 5 | `AddDefaultAdminUser` | Core | Seed admin |
| 6 | `AddMDMEntities` | MDM | Entidades maestras MDM inicial |
| 7 | `MDM_Enhanced` | MDM | Productos globales, variantes, atributos |
| 8 | `MDM_Refinements` | MDM | PriceLists, Categorías, Códigos, UoM |
| 9 | `INV_MovimientosInventario` | INV | Stock y movimientos |
| 10 | `SeedProductStatus` | MDM | Estados de ciclo de vida |
| 11 | `SeedDefaultCatalog` | MDM | Catálogo por defecto |
| 12 | `CMP_OrdenesCompra` | CMP | Órdenes de compra |
| 13 | `CMP_RecepcionesCompra` | CMP | Recepciones |
| 14 | `CMP_Importaciones` | CMP | Importaciones / Landed Cost |
| 15 | `ACC_CuentasContables` | ACC | Plan de cuentas |
| 16 | `ACC_AsientosContables` | ACC | Comprobantes contables |
| 17 | `ACC_PeriodosContables` | ACC | Períodos |
| 18 | `ACC_PlantillasContables` | ACC | Plantillas |
| 19 | `ACC_ComprobantesContables` | ACC | Tipos comprobante, cambio, pago |
| 20+ | Migraciones adicionales | Varios | Refinamientos ACC, Sucursales, etc. |

---

## 6. Actividad reciente (git)

| Commit | Descripción | Fecha aprox. |
|--------|-------------|--------------|
| `054afb4` | Merge PR #55 — Cliente Sucursal (cierre) | Mayo 2026 |
| `f70f5f5` | Cliente sucursal cerrado | Mayo 2026 |
| `a3a1010` | Merge PR #54 — ClienteGestion_Sucursal | Mayo 2026 |
| `9bf7643` | Sucursal (datos cliente por sucursal) | Mayo 2026 |
| `69fc4f9` | Merge PR #53 — EmpresaSucursal_ActualizacionDatos | Mayo 2026 |
| `993a147` | Cambios catálogos empresa | Mayo 2026 |
| `3feffb7` | Merge PR #50 — Documentación completa módulos | Abr 2026 |
| `857bd90` | PR #49 — Cierre de gestión, asientos cierre/apertura | Abr 2026 |
| `d935932` | PR #47 — S-06: Notificaciones períodos próximos a cerrar | Abr 2026 |

---

## 7. Ramas activas

| Rama | Estado | Descripción |
|------|--------|-------------|
| `main` | ✅ Producción | Rama principal |
| `Venta-Pedido` | 🔄 Desarrollo | Módulo de ventas (pedidos) |
| `PedidoSucursal` | 🔄 Desarrollo | Pedidos por sucursal |
| `DEPLOY` | 📦 Deploy | Artefactos de despliegue |
| `DeployWEB` | 📦 Deploy | Frontend desplegado |
| `CONT-002` | Cerrada/mergeada | Contabilidad avanzada |
| `S-07` | Cerrada/mergeada | Cierre de período contable |

---

## 8. Deuda técnica y pendientes

### TODOs en código

| Archivo | Descripción |
|---------|-------------|
| `AsientoContableHttpService.cs` | Manejo async pendiente |
| `EstadoFinancieroService.cs` | Generación de estados financieros incompleta |
| `PresupuestoService.cs` | Servicio de presupuestos no implementado |
| `ActivoFijoService.cs` | Cálculo de depreciaciones pendiente |
| `ImpuestoService.cs` | Motor de cálculo de impuestos pendiente |
| `Tarea.cs` | Entidad de workflow con notas pendientes |

### Módulos sin UI (backend listo, frontend pendiente)

- Industrias y reglas por industria
- Features activables (LOT, SERIAL, FEFO, HAZMAT, QC…)
- Historial de versiones de entidades
- Documentos multimedia adjuntos a productos
- Conciliación bancaria
- Activos fijos y depreciación
- Centros de costo
- Ubicaciones de almacén

### Infraestructura pendiente

- Tests: cobertura ~5% (solo estructura xUnit)
- CI/CD: pipeline GitHub Actions no configurado
- Exportación PDF (QuestPDF instalado, no integrado en flujos)
- Storage real para documentos (actualmente solo modelo)

---

## 9. Roadmap

### v1.1.0 — En desarrollo activo

- [ ] Módulo Ventas completo (pedidos, facturas, NC/ND)
- [ ] Facturación electrónica SIAT Bolivia
- [ ] Cuentas por Cobrar (CxC)
- [ ] Cuentas por Pagar (CxP)
- [ ] Reportes PDF (estados financieros, facturas)

### v1.2.0 — Próximo ciclo

- [ ] Dashboard con KPIs reales (stock mínimo, movimientos del día)
- [ ] Exportación masiva Excel/PDF en todos los listados
- [ ] UI para Industrias, Reglas, Activos Fijos, Conciliación Bancaria
- [ ] Tests unitarios — cobertura mínima 60%
- [ ] CI/CD GitHub Actions

### v2.0.0 — Futuro

- [ ] Costeo FIFO/Estándar (actualmente solo WAC)
- [ ] Notificaciones en tiempo real (SignalR)
- [ ] Multi-idioma (i18n)
- [ ] Integración con bancos / servicios externos

### v3.0.0 — Estratégico

- [ ] Business Intelligence (cubos OLAP)
- [ ] Automatización de procesos (workflow engine)
- [ ] API pública para integraciones
- [ ] Modelo SaaS multi-tenant con aislamiento por BD
- [ ] Migración Azure (App Service + Azure SQL + Blob Storage)
- [ ] App móvil (MAUI/Blazor Hybrid)

---

## 10. Acceso y configuración

### Credenciales por defecto

| Campo | Valor |
|-------|-------|
| Email | `admin@agorahub360.com` |
| Contraseña | `Admin123` (**cambiar en producción**) |

### Scripts de utilidad

| Script | Función |
|--------|---------|
| `start-system.ps1` | Inicia API y Web juntos |
| `test-database-connection.ps1` | Verifica conectividad BD |
| `reset-admin-user.ps1` | Resetea contraseña admin |
| `verify-admin-user.ps1` | Verifica estado del admin |
| `verify-mdm-routes.ps1` | Verifica endpoints MDM |

### Iniciar el sistema

```powershell
# Opción 1: Script automático
.\start-system.ps1

# Opción 2: Manual
cd src/AgoraHub360.ERP.Api && dotnet run
cd src/AgoraHub360.ERP.Web && dotnet run
```

---

> **Documento generado:** 11 de mayo 2026  
> **Desarrollado por:** Ágora HUB 360 — Abel Calvimontes  
> **Stack:** .NET 8 · Blazor WASM · SQL Server
