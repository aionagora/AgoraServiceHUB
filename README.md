# AgoraHUB360-ERP

Sistema ERP modular empresarial desarrollado bajo el Modelo Ágora (MAPE), diseñado para entornos mixtos de importación y comercialización, con arquitectura limpia, escalable y multiempresa desde el núcleo.

## Estado actual

- **Repositorio:** `abelcalvimontes/AgoraHUB360-ERP`
- **Rama principal:** `main`
- **Última actualización documentada:** 2026-05-17
- **Stack base:** .NET 8 · C# 12 · Blazor WebAssembly · ASP.NET Core Web API · EF Core 8 · SQL Server
- **Arquitectura:** Clean Architecture + multiempresa (tenant-aware) + auditoría transversal
- **Estado general:** MVP operativo en desarrollo activo

## Resumen ejecutivo

AgoraHUB360 ERP es la base tecnológica del ecosistema Ágora HUB 360. Actualmente el proyecto tiene una base funcional sólida en **Core, MDM, Inventario, Compras e importante cobertura de Contabilidad**, mientras que **Ventas** continúa en desarrollo activo y existen capacidades adicionales preparadas a nivel de backend o estructura para futuras iteraciones.

Durante abril y mayo de 2026 se consolidaron entregas importantes en:

- Gestión de **empresas y sucursales**
- Flujo de **clientes por sucursal**
- Documentación de **estado actual del proyecto**
- Análisis arquitectónico del **aislamiento multiempresa / tenant**

## Módulos y estado funcional

| Área | Estado | Completitud estimada | Comentarios |
|------|--------|----------------------|-------------|
| Core (usuarios, roles, empresas, sucursales) | ✅ Completo | 100% | Multiempresa, seguridad JWT, parámetros y numeración |
| MDM (clientes, proveedores, productos, almacenes, categorías, UDM) | ✅ Completo | 100% | Incluye productos globales, variantes, listas de precios y catálogos |
| Inventario | ✅ Completo | 100% | Kardex, movimientos, stock y costo promedio |
| Compras e importación | ✅ Completo | 100% | Órdenes, recepciones, landed cost y prorrateo |
| Contabilidad | ✅ Operativo | 95% | Reportes principales y cierre de gestión disponibles |
| Ventas | 🔄 En desarrollo | 5–15% | Existen ramas activas y avance reciente, pero no está consolidado como módulo completo en `main` |
| CxC / CxP | ⬜ Pendiente | 0% | Roadmap |
| Facturación electrónica SIN/SIAT | ⬜ Pendiente | 0% | Roadmap v1.1 |
| Reportes PDF / BI / automatización | 🔄 Parcial / pendiente | — | Hay piezas preparadas, pero no cerradas end-to-end |
| Tests automatizados | ⬜ Bajo avance | ~5% | Estructura disponible, cobertura aún baja |

## Arquitectura de la solución

La solución está organizada en los siguientes proyectos dentro de `src/`:

- `AgoraHub360.ERP.Domain`
- `AgoraHub360.ERP.Application`
- `AgoraHub360.ERP.Persistence`
- `AgoraHub360.ERP.Infrastructure`
- `AgoraHub360.ERP.Api`
- `AgoraHub360.ERP.Web`
- `AgoraHub360.ERP.Shared`
- `AgoraHub360.ERP.Tests`

### Principios estructurales

- **Domain** no depende de otras capas.
- **Application** concentra servicios y reglas de negocio.
- **Persistence** implementa EF Core, migraciones, repositorios e interceptores.
- **Infrastructure** contiene servicios externos e integraciones futuras.
- **API** expone endpoints REST versionados.
- **Web** consume la API desde Blazor WebAssembly.
- **Shared** agrupa DTOs y contratos comunes.
- **Tests** centraliza pruebas unitarias e integración.

## Funcionalidades transversales relevantes

- Multiempresa con `EmpresaId` y filtros globales de tenant
- Auditoría automática
- Versionado de entidades
- Soft delete por `Activo = false`
- API versionada bajo `/api/v1/`
- Autenticación JWT
- Swagger y health checks

## Acceso local y arranque

### Requisitos

- .NET 8 SDK
- SQL Server 2019+
- Visual Studio 2022/2026

### Configuración de base de datos

Editar `src/AgoraHub360.ERP.Api/appsettings.json` con una cadena de conexión válida:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR,PUERTO;Database=TU_BD;User Id=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=true;MultipleActiveResultSets=true;Encrypt=false"
  }
}
```

### Aplicar migraciones

```bash
dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api
```

### Inicio rápido

```powershell
.\start-system.ps1
```

El script automatiza:

- detención de procesos previos
- inicio de API en puerto 7001
- inicio de Web en puerto 5002
- apertura del navegador
- visualización de credenciales por defecto

### URLs locales

- Web: `https://localhost:5002`
- Login: `https://localhost:5002/login`
- API: `https://localhost:7001`
- Swagger: `https://localhost:7001/swagger`
- Health: `https://localhost:7001/health`

## Credenciales por defecto

| Campo | Valor |
|-------|-------|
| Email | `admin@agorahub360.com` |
| Contraseña | `Admin123` |
| Rol | `Admin` |

> **Importante:** cambiar estas credenciales antes de cualquier despliegue productivo.

## Scripts útiles

- `start-system.ps1` — inicia API y Web
- `quick-db-test.ps1` — test rápido de base de datos
- `test-database-connection.ps1` — validación completa de conexión
- `reset-admin-user.ps1` — reseteo del usuario administrador
- `verify-admin-user.ps1` — verificación del usuario admin
- `verify-mdm-routes.ps1` — validación de rutas MDM

## Documentación recomendada

### Estado y contexto general

- `docs/ESTADO_ACTUAL_2026.md` — resumen ejecutivo del estado actual del proyecto
- `ESTADO_PROYECTO_COMPLETO.md` — visión consolidada de alto nivel
- `DOCUMENTACION-MODULOS-COMPLETA.md` — documentación técnica y funcional amplia por módulos
- `docs/ANALISIS_MODULOS_ERP.md` — análisis transversal de módulos y arquitectura
- `ANALISIS-MULTIEMPRESA-TENANT.md` — análisis actualizado del aislamiento multiempresa / tenant

### Operación y despliegue

- `docs/DEPLOYMENT_GUIDE.md` — guía de despliegue
- `CREDENCIALES-DEFAULT.md` — acceso inicial
- `RESET-ADMIN-GUIDE.md` y `RESET-ADMIN-QUICKSTART.md` — recuperación de usuario admin

### Documentación específica y técnica

- `docs/MODULO-FINANCIERO-CONTABLE.md` — estado del módulo financiero-contable
- `docs/CHECKLIST_DESARROLLO.md` — checklist por fases y validación pre-producción
- `docs/GIT_WORKFLOW.md` — flujo de ramas y trabajo con Git
- `docs/COMMIT_CONVENTION.md` — convención de commits
- `docs/PROJECT_BOARD_SETUP.md` — estructura de tablero de proyecto

## Ramas visibles y contexto de evolución

Además de `main`, el repositorio mantiene ramas de trabajo y referencia que ayudan a contextualizar el estado del proyecto, por ejemplo:

- `Venta-Pedido`
- `PedidoSucursal`
- `VentasGestion`
- `feature/pedidos-venta`
- `ClienteGestion_Sucursal`
- `ClienteSucursal`
- `codex/CorreccionMultiempresa`

Estas ramas muestran que el proyecto sigue avanzando especialmente en ventas, sucursales y mejoras multiempresa.

## Actividad reciente relevante

Entre los cambios recientes visibles en el repositorio destacan:

- **2026-05-17:** incorporación de `ANALISIS-MULTIEMPRESA-TENANT.md`
- **2026-05-11:** actualización de `docs/ESTADO_ACTUAL_2026.md` y checklist de desarrollo
- **abril 2026:** consolidación de empresas, sucursales y cliente por sucursal
- **abril 2026:** generación de documentación amplia de módulos

## Roadmap resumido

### v1.1

- Ventas completas
- Facturación electrónica SIAT / SIN
- Cuentas por cobrar y por pagar
- Reportes PDF

### v1.2

- Dashboards y KPIs reales
- Exportaciones masivas Excel/PDF
- UI para módulos parcialmente estructurados
- Mejor cobertura de pruebas
- CI/CD

### v2.0+

- Workflows y automatización
- BI
- API pública
- SaaS multi-tenant más robusto
- Integraciones externas

## Nota sobre la documentación

La documentación del repositorio fue creciendo por hitos, incidencias, fixes, análisis y entregas funcionales. Por eso existen varios `.md` históricos. Este README pasa a funcionar como **punto de entrada principal** y dirige a los documentos vigentes más útiles para entender el estado real del proyecto al **17 de mayo de 2026**.
