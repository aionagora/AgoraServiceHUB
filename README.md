# AgoraHUB360-ERP
AgoraHUB 360 – ERP  Sistema ERP modular empresarial desarrollado bajo el Modelo Ágora (MAPE), diseñado para entornos mixtos de importación y comercialización, con arquitectura limpia, escalable y multiempresa.

🚀 AgoraHUB 360 – ERP

Sistema ERP modular empresarial desarrollado bajo el Modelo Ágora (MAPE), diseñado para entornos mixtos de importación y comercialización, con arquitectura limpia, escalable y multiempresa desde el núcleo.

📌 Visión del Proyecto

AgoraHUB 360 – ERP es la base tecnológica del ecosistema Ágora HUB 360.
Su objetivo es proporcionar un sistema empresarial moderno, estructurado y escalable que permita:

Control operativo integral

Gestión multiempresa y multimoneda

Integración compras → inventario → ventas

Costeo de importaciones con prorrateo

Escalabilidad modular

Evolución futura hacia BI, automatización e integraciones externas

🧠 Arquitectura

El proyecto está construido bajo:

Clean Architecture

Separación por dominios

Principios SOLID

Multiempresa (tenant-aware)

Auditoría transversal

Roadmap gobernado por arquitectura (MAPE)

Stack Tecnológico

.NET 8

Blazor WebAssembly

ASP.NET Core Web API

SQL Server 2019+

Entity Framework Core

JWT Authentication

🏗 Estructura de la Solución
AgoraHub360.ERP.Domain
AgoraHub360.ERP.Application
AgoraHub360.ERP.Persistence
AgoraHub360.ERP.Infrastructure
AgoraHub360.ERP.Api
AgoraHub360.ERP.Web
AgoraHub360.ERP.Shared
AgoraHub360.ERP.Tests

Principios estructurales

Domain no depende de ninguna capa.

Application contiene reglas de negocio.

Persistence maneja EF Core.

Infrastructure maneja servicios externos.

API expone endpoints.

Web (Blazor WASM) consume la API.

Shared contiene contratos y DTOs.

Tests valida dominio y aplicación.

📦 MVP v1.0 – Alcance

Módulos incluidos en la primera versión:

1️⃣ CORE

Seguridad y roles (RBAC)

Multiempresa

Multimoneda

Parámetros del sistema

Numeración de documentos

Auditoría automática

2️⃣ MDM (Maestros)

Clientes

Proveedores (local e internacional)

Productos (MP / PT / Servicio)

Almacenes

Categorías

Unidades de medida

3️⃣ Inventario Avanzado Base

Kardex

Multi-almacén

Ajustes y transferencias

Costo promedio

Trazabilidad básica

4️⃣ Compras + Importación Básica

Orden de compra

Recepción parcial

Registro de gastos asociados

Prorrateo de costos a inventario

5️⃣ Ventas

Cotización

Pedido

Facturación interna

Integración con inventario

Control de margen básico

Facturación electrónica SIN será incorporada en la versión v1.1.

🔄 Flujo Operativo MVP

Proveedor → Orden de Compra → Recepción + Costos → Inventario Valorizado → Venta → Margen

🔐 Multiempresa

El sistema es multiempresa desde el diseño:

Todas las entidades relevantes incluyen EmpresaId

Seguridad basada en roles

Contexto activo por sesión

Datos aislados por tenant

🛠 Configuración Inicial

## Requisitos

.NET 8 SDK

SQL Server 2019+

Visual Studio 2022/2026

## Pasos

1. **Clonar repositorio**

2. **Configurar cadena de conexión** en `src/AgoraHub360.ERP.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR,PUERTO;Database=TU_BD;User Id=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=true;MultipleActiveResultSets=true;Encrypt=false"
  }
}
```

**Nota importante:** La cadena de conexión en `appsettings.json` es leída automáticamente por el `AgoraDbContextFactory` para las migraciones. No es necesario configurar nada adicional.

3. **Ejecutar migraciones:**

```bash
dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api
```

4. **Iniciar el sistema:**

### Opción A: Script Automatizado (Recomendado) 🚀

```powershell
.\start-system.ps1
```

El script automáticamente:
- ✅ Detiene procesos previos
- ✅ Inicia la API (puerto 7001)
- ✅ Inicia la Web (puerto 5002)
- ✅ Abre el navegador en el login
- ✅ Muestra las credenciales

### Opción B: Inicio Manual

**Terminal 1 - API:**
```bash
cd src\AgoraHub360.ERP.Api
dotnet run --launch-profile https
```

**Terminal 2 - Web:**
```bash
cd src\AgoraHub360.ERP.Web
dotnet run --launch-profile https
```

5. **Acceder al sistema:**
   - Aplicación Web: `https://localhost:5002`
   - Login: `https://localhost:5002/login`
   - API: `https://localhost:7001`
   - Swagger: `https://localhost:7001/swagger`

## 🔐 Credenciales de Acceso

El sistema crea automáticamente un usuario administrador:

| Campo | Valor |
|-------|-------|
| **Email** | `admin@agorahub360.com` |
| **Contraseña** | `Admin123` |
| **Rol** | Admin |

📚 **Documentación Completa:** [CREDENCIALES-DEFAULT.md](CREDENCIALES-DEFAULT.md)

⚠️ **Importante:** Cambia estas credenciales antes de pasar a producción.

🔍 Test de Conexión a Base de Datos

### Test Rápido (sin iniciar la API)
```powershell
.\quick-db-test.ps1
```

### Test Completo (con API corriendo)
```powershell
# Terminal 1: Inicia la API
cd src\AgoraHub360.ERP.Api
dotnet run

# Terminal 2: Ejecuta el test
.\test-database-connection.ps1
```

### Endpoints de Diagnóstico
- **GET** `/api/v1/diagnostics/ping` - Verifica que la API esté activa
- **GET** `/api/v1/diagnostics/database-test` - Test completo de BD
- **GET** `/health` - Health check general

📚 **Documentación Completa:**
- [Script de Inicio](start-system.ps1) - Inicio automatizado
- [Credenciales por Defecto](CREDENCIALES-DEFAULT.md) - Usuario y contraseña
- [Solución Error de Login](LOGIN-ERROR-FIX.md) - Troubleshooting de conexión
- [Guía de Testing](DATABASE-TEST-GUIDE.md) - Guía detallada
- [Resumen Ejecutivo](DATABASE-TEST-SUMMARY.md) - Resumen rápido
- [Reporte Técnico](DATABASE-TEST-REPORT.md) - Detalles técnicos
- [Solución de Problemas de Conexión](DATABASE-CONNECTION-FIX.md) - Troubleshooting BD

📈 Roadmap

v1.0 → MVP Mixto Operativo

v1.1 → Integración SIN (Facturación electrónica)

v1.2 → BI básico + dashboards

v2.0 → Producción / MRP

v2.x → Automatización avanzada + integraciones externas

🧪 Testing

Unit Tests en Domain y Application

Integration Tests en Persistence

Pruebas funcionales previas a release

Database Testing con scripts automatizados

📜 Metodología

Este proyecto sigue el Modelo Ágora (MAPE):

Diagnóstico

Diseño Arquitectónico

Desarrollo por Capas

Validación Controlada

Implementación Guiada

Evolución Continua

No se libera sin validación estructural.

🎯 Objetivo Estratégico

Convertirse en la plataforma ERP central del ecosistema Ágora HUB 360 y escalar regionalmente como solución SaaS empresarial.

📩 Contacto

Proyecto desarrollado por Ágora HUB 360 – Dirección Ágora Tech.
