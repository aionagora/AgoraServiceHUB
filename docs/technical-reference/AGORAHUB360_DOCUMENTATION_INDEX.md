# AGORAHUB360 ERP — ÍNDICE DE DOCUMENTACIÓN TÉCNICA

> **Versión:** 1.0
> **Fecha:** 2026-06-10
> **Propósito:** Índice maestro de toda la documentación técnica del ERP.

---

## Resumen General

**AgoraHUB360 ERP** es un sistema ERP modular, multiempresa y multitenant construido sobre .NET 8 con Clean Architecture. Compuesto por **8 proyectos** en una solución .NET, abarca módulos de Core, MDM, Ventas, Inventario, Compras, Contabilidad, CxC, Facturación Electrónica (SIAT), Activos Fijos, Bancos, Logística y Workflow.

### Stack Tecnológico

- **Backend:** .NET 8, ASP.NET Core, Entity Framework Core 8
- **Frontend:** Blazor Server
- **Base de datos:** SQL Server
- **Autenticación:** JWT Bearer con claims personalizados
- **API:** REST versionada (v1) con Swagger
- **Pruebas:** xUnit + FluentAssertions

---

## Tabla de Documentos

| # | Documento | Descripción | Archivo |
|---|---|---|---|
| 01 | **Guía Maestra** | Resumen ejecutivo, arquitectura, módulos, roadmap, ADRs | `01_AGORAHUB360_MASTER_GUIDE.md` |
| 02 | **Capa de Dominio** | Entidades (~85), enums (17), clases base, reglas de negocio | `02_AGORAHUB360_DOMAIN.md` |
| 03 | **Capa de Aplicación** | Interfaces (~76), servicios, casos de uso, DI | `03_AGORAHUB360_APPLICATION.md` |
| 04 | **Capa de Persistencia** | DbContext, DbSets (~80), configuraciones, migraciones (~55) | `04_AGORAHUB360_PERSISTENCE.md` |
| 05 | **Capa de API** | Controllers (~55), endpoints, middleware JWT, Swagger | `05_AGORAHUB360_API.md` |
| 06 | **Capa Web (Blazor)** | Páginas (~45), componentes, layouts, servicios HTTP (~50) | `06_AGORAHUB360_WEB.md` |
| 07 | **Capa Shared** | DTOs, constantes, configuraciones, utilerías | `07_AGORAHUB360_SHARED.md` |
| 08 | **Pruebas** | Tests (~22 clases), cobertura, matriz de seguridad | `08_AGORAHUB360_TESTS.md` |

---

## Mapa de Arquitectura

```
┌─────────────────────────────────────────────────────────────────────┐
│                        PRESENTACIÓN                                  │
│  ┌─────────────────────┐  ┌──────────────────────────────────────┐  │
│  │  AgoraHub360.ERP.Web│  │      AgoraHub360.ERP.Api             │  │
│  │  (Blazor Server)    │  │  (REST API + Swagger + JWT)          │  │
│  │  ~45 Razor Pages    │  │  ~55 Controllers V1                  │  │
│  └─────────┬───────────┘  └────────────────┬─────────────────────┘  │
└────────────┼───────────────────────────────┼─────────────────────────┘
             │                               │
┌────────────┼───────────────────────────────┼─────────────────────────┐
│            ▼                               ▼                         │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │              AgoraHub360.ERP.Application                      │   │
│  │  ~76 Interfaces + ~76 Services + Validators + DI             │   │
│  │  AuthService · VentaService · FacturaVentaService            │   │
│  │  CuentasPorCobrarService · MovimientoInventarioService       │   │
│  └──────────────────────────┬───────────────────────────────────┘   │
│                             │                                       │
│  ┌──────────────────────────▼───────────────────────────────────┐   │
│  │              AgoraHub360.ERP.Domain                            │   │
│  │  ~85 Entidades · 17 Enums · 3 Clases Base                    │   │
│  │  TenantEntity · AuditableEntity · Result                      │   │
│  │  IRepository<T> · IUnitOfWork                                 │   │
│  └──────────────────────────┬───────────────────────────────────┘   │
│                             │                                       │
│  ┌──────────────────────────▼───────────────────────────────────┐   │
│  │            AgoraHub360.ERP.Persistence                         │   │
│  │  EF Core DbContext · ~55 Migraciones · ~40 Configurations    │   │
│  │  Query Filter Global (Tenant) · Interceptors                 │   │
│  │  Repository<T> · AuditLogService · EmpresaSeedService        │   │
│  └──────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │           AgoraHub360.ERP.Infrastructure                      │   │
│  │  Servicios externos (Email, archivos, etc.)                  │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                            │
                            ▼
                    ┌──────────────┐
                    │  SQL Server  │
                    │  ~80 tablas  │
                    │  16 schemas  │
                    └──────────────┘

     ┌─────────────────────────────────────────────────────────┐
     │              AgoraHub360.ERP.Shared                       │
     │  DTOs (~20 subdirs) · Constants · PaginatedResultDto     │
     │  ApiResponse<T> · AppFormattingOptions                   │
     └─────────────────────────────────────────────────────────┘

     ┌─────────────────────────────────────────────────────────┐
     │              AgoraHub360.ERP.Tests                        │
     │  ~22 clases · Unitarias + Seguridad                      │
     │  Domain (7) · Application (10) · Security (9) · Auth (2) │
     └─────────────────────────────────────────────────────────┘
```

---

## Dependencias entre Proyectos

```
                    ┌──────────────┐
                    │    Shared    │
                    └──────┬───────┘
                           │
              ┌────────────┼────────────┐
              ▼            ▼            ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│   Domain     │  │  Application │  │  Persistence  │
│  (sin dep)   │  │ (Domain,     │  │ (Domain,      │
│              │  │  Shared)     │  │  Application) │
└──────────────┘  └──────┬───────┘  └──────────────┘
                         │
              ┌──────────┼──────────┐
              ▼          ▼          ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│ Infrastructure│ │     API      │ │     Web      │
│ (Application)│ │(App, Persist,│ │   (Shared)   │
│              │ │  Infra)      │ │              │
└──────────────┘ └──────────────┘ └──────────────┘
                                  │
                                  ▼
                         ┌──────────────┐
                         │    Tests     │
                         │ (Application,│
                         │   Domain)    │
                         └──────────────┘
```

---

## Lectura Recomendada

1. **01 — Guía Maestra**: Comience aquí para entender la arquitectura general, objetivos y módulos.
2. **02 — Dominio**: Para conocer todas las entidades, enums y reglas de negocio.
3. **03 — Aplicación**: Para entender los casos de uso, servicios críticos y flujos de negocio.
4. **04 — Persistencia**: Para el modelo de base de datos, esquemas y migraciones.
5. **05 — API**: Para los endpoints REST, autenticación y contratos de API.
6. **06 — Web**: Para la interfaz de usuario Blazor y servicios HTTP.
7. **07 — Shared**: Para los DTOs y constantes compartidas.
8. **08 — Tests**: Para la cobertura de pruebas y seguridad.

---

## Estadísticas del Proyecto

| Métrica | Cantidad |
|---|---|
| Proyectos en solución | 8 |
| Entidades de dominio | ~85 |
| Enumeraciones | 17 |
| Interfaces de servicio | ~76 |
| Servicios implementados | ~76 |
| Controllers API | ~55 |
| Razor Pages | ~45 |
| Servicios HTTP Web | ~50 |
| DTOs (subdirectorios) | ~20 |
| Configuraciones EF | ~40 |
| Migraciones | ~55 |
| Clases de prueba | ~22 |
| Tablas en BD | ~80 |
| Schemas | ~16 |
| Líneas de código estimadas | ~100,000+ |

---
