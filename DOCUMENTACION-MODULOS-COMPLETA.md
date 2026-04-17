# AgoraHUB360-ERP — Documentación Completa de Módulos

> **Estado actual:** Rama `main` · Fecha de revisión: 2026-04-17  
> Generada automáticamente a partir del análisis del código fuente.

---

## Tabla de Contenidos

1. [Visión General del Proyecto](#1-visión-general-del-proyecto)
2. [Arquitectura del Sistema](#2-arquitectura-del-sistema)
3. [Stack Tecnológico](#3-stack-tecnológico)
4. [Estructura de la Solución](#4-estructura-de-la-solución)
5. [Módulo CORE — Núcleo del Sistema](#5-módulo-core--núcleo-del-sistema)
6. [Módulo MDM — Maestros de Datos](#6-módulo-mdm--maestros-de-datos)
7. [Módulo ACC — Contabilidad](#7-módulo-acc--contabilidad)
8. [Módulo CMP — Compras e Importación](#8-módulo-cmp--compras-e-importación)
9. [Módulo INV — Inventario](#9-módulo-inv--inventario)
10. [Módulo LOG — Logística](#10-módulo-log--logística)
11. [Módulo BNC — Bancario y Conciliación](#11-módulo-bnc--bancario-y-conciliación)
12. [Módulo ACT — Activos Fijos](#12-módulo-act--activos-fijos)
13. [Módulo TRB — Tributario](#13-módulo-trb--tributario)
14. [Módulo PRC — Precios](#14-módulo-prc--precios)
15. [Módulo CST — Costos](#15-módulo-cst--costos)
16. [Módulo RUL — Reglas por Industria](#16-módulo-rul--reglas-por-industria)
17. [Módulo VER — Versionado de Entidades](#17-módulo-ver--versionado-de-entidades)
18. [Módulo WF — Workflow](#18-módulo-wf--workflow)
19. [Módulo DOC — Documentos Adjuntos](#19-módulo-doc--documentos-adjuntos)
20. [Módulo Ventas (en construcción)](#20-módulo-ventas-en-construcción)
21. [API REST — Resumen de Endpoints](#21-api-rest--resumen-de-endpoints)
22. [Frontend Web — Páginas Blazor](#22-frontend-web--páginas-blazor)
23. [Servicios en Background](#23-servicios-en-background)
24. [Capa de Infraestructura](#24-capa-de-infraestructura)
25. [Capa Shared — DTOs y Contratos](#25-capa-shared--dtos-y-contratos)
26. [Pruebas Automatizadas](#26-pruebas-automatizadas)
27. [Configuración e Instalación](#27-configuración-e-instalación)
28. [Multiempresa (Multi-Tenant)](#28-multiempresa-multi-tenant)
29. [Auditoría Automática](#29-auditoría-automática)
30. [Enumeraciones del Dominio](#30-enumeraciones-del-dominio)
31. [Roadmap y Estado del Proyecto](#31-roadmap-y-estado-del-proyecto)

---

## 1. Visión General del Proyecto

**AgoraHUB 360 – ERP** es un sistema ERP modular empresarial desarrollado bajo el **Modelo Ágora (MAPE)**. Está diseñado para empresas en entornos mixtos de importación y comercialización.

### Objetivo Principal
Proporcionar un sistema empresarial moderno, estructurado y escalable que permita:
- Control operativo integral
- Gestión multiempresa y multimoneda
- Integración completa: Compras → Inventario → Ventas
- Costeo de importaciones con prorrateo
- Escalabilidad modular
- Evolución futura hacia BI, automatización e integraciones externas

### Flujo Operativo Central
```
Proveedor → Orden de Pedido → Orden de Compra → Confirmación Proveedor
→ Pago Anticipo → Expediente Importación → Embarque → Aduana
→ Recepción Mercadería → Hoja Importación (Landed Cost)
→ Inventario Valorizado → Venta → Margen
```

---

## 2. Arquitectura del Sistema

El proyecto implementa **Clean Architecture** con separación estricta de responsabilidades:

```
┌─────────────────────────────────────────────────────────────────┐
│                      Presentación                                │
│   AgoraHub360.ERP.Web (Blazor WASM) ← API REST                  │
│   AgoraHub360.ERP.Api (ASP.NET Core Web API)                     │
├─────────────────────────────────────────────────────────────────┤
│                      Aplicación                                  │
│   AgoraHub360.ERP.Application (Servicios + Interfaces)           │
├─────────────────────────────────────────────────────────────────┤
│                      Dominio (núcleo)                            │
│   AgoraHub360.ERP.Domain (Entidades + Enums + Excepciones)       │
├─────────────────────────────────────────────────────────────────┤
│                  Infraestructura / Persistencia                  │
│   AgoraHub360.ERP.Persistence (EF Core + SQL Server)            │
│   AgoraHub360.ERP.Infrastructure (Servicios externos)            │
├─────────────────────────────────────────────────────────────────┤
│                      Contratos Compartidos                       │
│   AgoraHub360.ERP.Shared (DTOs + Constantes + Utils)            │
│   AgoraHub360.ERP.Tests (xUnit)                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Principios Aplicados
| Principio | Implementación |
|-----------|----------------|
| **Clean Architecture** | Domain → Application → Infrastructure → API |
| **SOLID** | Interfaces para todos los servicios, inyección de dependencias |
| **DDD (Domain-Driven Design)** | Entidades con lógica de dominio, Value Objects, Domain Exceptions |
| **CQRS parcial** | Separación Read/Write en servicios de aplicación |
| **Multi-Tenant** | Filtros globales en EF Core, `TenantEntity` base |
| **Repository Pattern** | `IRepository<T>` + `IUnitOfWork` |
| **Result Pattern** | `Result<T>` para manejo de errores sin excepciones |

---

## 3. Stack Tecnológico

| Componente | Tecnología | Versión |
|------------|-----------|---------|
| **Runtime** | .NET | 8.0 |
| **API Backend** | ASP.NET Core Web API | 8.0 |
| **Frontend** | Blazor WebAssembly | 8.0 |
| **ORM** | Entity Framework Core | 8.0.12 |
| **Base de Datos** | SQL Server | 2019+ |
| **Autenticación** | JWT (JSON Web Tokens) | — |
| **Versionado API** | Asp.Versioning.Mvc | 8.1.0 |
| **Documentación API** | Swashbuckle (Swagger) | 6.6.2 |
| **Excel/Export** | ClosedXML + EPPlus | 0.105.0 / 8.5.1 |
| **PDF** | QuestPDF | 2026.2.4 |
| **Validaciones** | FluentValidation | — |
| **Health Checks** | EF Core Health Checks | 8.0.12 |
| **Tests** | xUnit | — |

---

## 4. Estructura de la Solución

```
AgoraHub360.ERP.sln
├── src/
│   ├── AgoraHub360.ERP.Domain/          # Núcleo: entidades, enums, excepciones
│   │   ├── Common/                       # AuditableEntity, TenantEntity
│   │   ├── Entities/
│   │   │   ├── Core/                     # Empresa, Usuario, Rol, ParametroSistema, etc.
│   │   │   ├── MDM/                      # Clientes, Proveedores, Productos, Almacenes
│   │   │   ├── ACC/                      # Contabilidad
│   │   │   ├── CMP/                      # Compras e Importación
│   │   │   ├── INV/                      # Inventario
│   │   │   ├── LOG/                      # Logística
│   │   │   ├── BNC/                      # Bancario
│   │   │   ├── ACT/                      # Activos Fijos
│   │   │   ├── TRB/                      # Tributario
│   │   │   ├── PRC/                      # Precios
│   │   │   ├── CST/                      # Costos
│   │   │   ├── RUL/                      # Reglas Industria
│   │   │   ├── VER/                      # Versionado
│   │   │   ├── DOC/                      # Documentos
│   │   │   └── Workflow/                 # Tareas y Workflow
│   │   ├── Enums/                        # EstadoDocumento, TipoCuenta, etc.
│   │   ├── Exceptions/                   # DomainException
│   │   └── Interfaces/                   # IRepository, IUnitOfWork
│   │
│   ├── AgoraHub360.ERP.Application/      # Capa de aplicación
│   │   ├── Common/                       # Result<T>
│   │   ├── Interfaces/                   # IXxxService (50+ interfaces)
│   │   ├── Services/                     # Implementaciones de servicios
│   │   └── Validators/                   # FluentValidation
│   │
│   ├── AgoraHub360.ERP.Persistence/      # EF Core + SQL Server
│   │   ├── Context/                      # AgoraDbContext
│   │   ├── Configurations/               # Fluent API (por módulo)
│   │   ├── Repositories/                 # Implementación IRepository
│   │   ├── Migrations/                   # Migraciones EF Core
│   │   ├── Services/                     # Seed, Factory
│   │   └── Interceptors/                 # AuditInterceptor
│   │
│   ├── AgoraHub360.ERP.Infrastructure/   # Servicios externos
│   │   └── Services/                     # Email, archivos, etc.
│   │
│   ├── AgoraHub360.ERP.Api/              # ASP.NET Core Web API
│   │   ├── Auth/                         # StubAuthHandler
│   │   ├── BackgroundServices/           # PeriodoContableNotificador
│   │   ├── Controllers/V1/               # 45 controladores REST
│   │   ├── Middleware/                   # TenantMiddleware
│   │   ├── Properties/                   # launchSettings
│   │   └── Services/                     # CurrentUserService
│   │
│   ├── AgoraHub360.ERP.Web/              # Blazor WebAssembly
│   │   ├── Pages/                        # Páginas por módulo
│   │   ├── Components/                   # Componentes compartidos
│   │   ├── Services/                     # HTTP Clients
│   │   └── Layout/                       # MainLayout, NavMenu
│   │
│   └── AgoraHub360.ERP.Shared/           # Contratos compartidos
│       ├── DTOs/                         # DTOs por módulo
│       ├── Constants/                    # Constantes del sistema
│       ├── Extensions/                   # Extension methods
│       └── Utils/                        # Utilidades
│
└── tests/
    └── AgoraHub360.ERP.Tests/            # Pruebas unitarias (xUnit)
```

---

## 5. Módulo CORE — Núcleo del Sistema

El módulo CORE provee las entidades fundamentales del sistema y funcionalidades transversales.

### 5.1 Entidades de Dominio

#### `Empresa` — Tenant principal
Representa una empresa/tenant en el sistema multiempresa.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | PK autoincremental |
| `Nombre` | string | Nombre de la empresa |
| `NIT` | string? | Número de identificación tributaria |
| `Direccion` | string? | Dirección fiscal |
| `Telefono` | string? | Teléfono principal |
| `Email` | string? | Correo electrónico |
| `MonedaBaseId` | string? | ISO 4217 de la moneda base (ej: "BOB") |
| `IndustriaId` | byte | Industria: 1=Retail, 2=Alimentos, 3=Farmacia, 4=Ferretería, 5=Textil, 6=Tecnología, 7=General |
| `MetodoCosteoDefault` | byte | 1=Promedio, 2=FIFO, 3=LIFO, 4=Estándar |
| `PermiteVariantes` | bool | Habilita variantes de producto |
| `PermiteLotes` | bool | Habilita lotes y números de serie |
| `PermiteServicios` | bool | Habilita productos de tipo servicio |
| `AutoGeneraSku` | bool | Genera SKU automático |
| `PrefijoSku` | string? | Prefijo para SKU (ej: "PRD-") |

Hereda de `AuditableEntity` (FechaCreacion, CreadoPor, FechaModificacion, ModificadoPor, Activo).

---

#### `Usuario` — Usuario del sistema
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | PK |
| `NombreUsuario` | string | Login único |
| `Email` | string | Email (usado para login) |
| `PasswordHash` | string | Hash bcrypt de la contraseña |
| `NombreCompleto` | string | Nombre para mostrar |
| `EmpresaActivaId` | int? | Empresa activa en la sesión actual |
| `Empresas` | collection | Relación N:M con empresas (`UsuarioEmpresa`) |

---

#### `Rol` — Roles RBAC
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | PK |
| `Nombre` | string | Nombre del rol (Admin, Operador, Contador, etc.) |
| `Descripcion` | string? | Descripción del rol |

---

#### `UsuarioEmpresa` — Relación usuario-empresa con rol
Tabla pivote que asocia un usuario a una empresa con un rol específico. Permite que un usuario tenga distintos roles en distintas empresas.

---

#### `ParametroSistema` — Configuración clave-valor por empresa
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | PK |
| `Clave` | string | Nombre del parámetro (ej: "MonedaBase") |
| `Valor` | string | Valor del parámetro (ej: "BOB") |
| `Descripcion` | string? | Descripción legible |
| `Categoria` | string | Agrupación UI: "General", "Facturación", "Inventario" |
| `TipoDato` | string | String, Integer, Decimal, Boolean, Select |

Hereda de `TenantEntity` (incluye `EmpresaId`).

---

#### `NumeracionDocumento` — Correlativo de documentos por empresa
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | PK |
| `TipoDocumento` | string | Código: "OC", "REC", "FAC", "PED", "OP", "HR", etc. |
| `Descripcion` | string | Nombre legible ("Orden de Compra") |
| `Prefijo` | string | Ej: "OC-" |
| `SiguienteNumero` | int | Siguiente número a asignar (inicio: 1) |
| `Digitos` | int | Ceros a la izquierda (ej: 6 → "000001") |

**Método de dominio:**
- `GenerarSiguiente()`: retorna el número formateado (ej: "OC-000001") e incrementa el contador.

---

#### `Moneda` — Catálogo de monedas
Entidad de catálogo global (ISO 4217).

---

#### `AuditLog` — Registro de auditoría
Captura cada INSERT, UPDATE y DELETE del sistema.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | long | PK |
| `Entidad` | string | Nombre de la entidad modificada |
| `EntidadId` | string | PK de la entidad (como string) |
| `Accion` | string | "Insert", "Update" o "Delete" |
| `ValoresAnteriores` | string? | JSON con valores previos |
| `ValoresNuevos` | string? | JSON con valores nuevos |
| `CamposModificados` | string? | Lista de campos modificados (solo Update) |
| `EmpresaId` | int? | Empresa del contexto al momento del cambio |
| `Usuario` | string? | Usuario que realizó el cambio |
| `FechaHora` | DateTime | Timestamp UTC |

No hereda de `AuditableEntity` para evitar recursión en el interceptor.

---

### 5.2 Clases Base del Dominio

#### `AuditableEntity`
```
FechaCreacion  : DateTime
CreadoPor      : string?
FechaModificacion : DateTime?
ModificadoPor  : string?
Activo         : bool = true
```

#### `TenantEntity` (hereda de `AuditableEntity`)
Agrega:
```
EmpresaId : int
```
Todas las entidades que pertenecen a una empresa heredan de `TenantEntity`.

---

### 5.3 Endpoints API — CORE

#### Autenticación (`/api/v1/auth`)
| Método | Ruta | Descripción | Autorización |
|--------|------|-------------|--------------|
| POST | `/login` | Autenticar usuario, retorna JWT | Anónimo |
| GET | `/me` | Info del usuario autenticado | Bearer JWT |
| GET | `/mis-empresas` | Empresas asignadas al usuario | Bearer JWT |

**Request Login:**
```json
{ "email": "admin@agorahub360.com", "password": "Admin123" }
```
**Response Login:**
```json
{ "token": "eyJ...", "usuarioId": 1, "nombreCompleto": "...", "rol": "Admin", "empresaId": 1 }
```

#### Empresas (`/api/v1/empresas`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista todas las empresas |
| GET | `/mis-empresas` | Empresas del usuario autenticado |
| GET | `/{id}` | Obtener empresa por ID |
| POST | `/` | Crear nueva empresa |
| PUT | `/{id}` | Actualizar empresa |
| DELETE | `/{id}` | Eliminar empresa (soft-delete) |
| POST | `/{id}/seed` | Generar datos base MDM para la empresa |
| POST | `/mi-empresa/seed` | Seed para la empresa activa del usuario |

#### Usuarios (`/api/v1/usuarios`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista todos los usuarios |
| GET | `/{id}` | Usuario por ID |
| POST | `/` | Crear usuario |
| PUT | `/{id}` | Actualizar usuario |
| DELETE | `/{id}` | Eliminar usuario |
| POST | `/{id}/roles` | Asignar rol a usuario en empresa |
| DELETE | `/{id}/empresas/{empresaId}` | Remover usuario de empresa |
| PUT | `/{id}/reset-password` | Resetear contraseña (solo Admin) |

#### Roles (`/api/v1/roles`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista todos los roles |
| POST | `/` | Crear rol |
| PUT | `/{id}` | Actualizar rol |
| DELETE | `/{id}` | Eliminar rol |

#### Parámetros (`/api/v1/parametros`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista parámetros de la empresa activa |
| GET | `/{id}` | Parámetro por ID |
| POST | `/` | Crear parámetro |
| PUT | `/{id}` | Actualizar parámetro |
| DELETE | `/{id}` | Eliminar parámetro |

#### Numeraciones (`/api/v1/numeraciones`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista numeraciones de la empresa |
| GET | `/{id}` | Numeración por ID |
| POST | `/` | Crear numeración |
| PUT | `/{id}` | Actualizar numeración |
| DELETE | `/{id}` | Eliminar numeración |

#### Auditoría (`/api/v1/auditlogs`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista logs de auditoría (con filtros) |

#### Diagnóstico (`/api/v1/diagnostics`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/ping` | Verifica que la API esté activa |
| GET | `/database-test` | Test completo de conexión a BD |

---

## 6. Módulo MDM — Maestros de Datos

El módulo MDM (Master Data Management) gestiona todos los datos maestros del sistema, incluyendo clientes, proveedores, productos, almacenes y catálogos.

### 6.1 Entidades de Dominio MDM

#### `Cliente` — Clientes del sistema
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | PK |
| `Codigo` | string | Código único del cliente |
| `RazonSocial` | string | Nombre/razón social |
| `NIT` | string? | NIT o cédula |
| `Direccion` | string? | Dirección |
| `Telefono` | string? | Teléfono |
| `Email` | string? | Email |
| `NombreContacto` | string? | Persona de contacto |
| `TipoCliente` | string | "General" (extensible) |

---

#### `Proveedor` — Proveedores del sistema
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | PK |
| `Codigo` | string | Código único |
| `RazonSocial` | string | Nombre/razón social |
| `NIT` | string? | NIT fiscal |
| `Direccion` | string? | Dirección |
| `Telefono` | string? | Teléfono |
| `Email` | string? | Email |
| `NombreContacto` | string? | Persona de contacto |
| `TipoProveedor` | string | "Local" o "Internacional" |
| `Pais` | string? | País (relevante si es Internacional) |
| `CondicionPago` | string? | Contado, 30 días, 60 días, etc. |

---

#### `Almacen` — Bodegas/almacenes
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | PK |
| `Codigo` | string | Código único |
| `Nombre` | string | Nombre descriptivo |
| `Direccion` | string? | Dirección física |
| `Responsable` | string? | Persona responsable |
| `Telefono` | string? | Teléfono |

---

#### `UbicacionAlmacen` — Ubicaciones dentro de un almacén
Permite gestionar ubicaciones (estantes, niveles, posiciones) dentro de cada almacén.

---

#### `Uom` — Unidad de Medida (Unit of Measure)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `UomId` | int | PK |
| `Code` | string | Código ISO (ej: "KG", "UN", "L") |
| `Name` | string | Nombre descriptivo |

---

#### `Product` — Producto maestro global
El producto maestro es independiente de la empresa (catálogo global). Cada empresa lo instancia con `CompanyProduct`.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ProductId` | long | PK |
| `CatalogId` | long | FK al catálogo al que pertenece |
| `ProductKind` | byte | 1=Item, 2=Service, 3=Kit, 4=RawMaterial, 5=Packaging |
| `GenericName` | string | Nombre genérico (DCI en farmacia, nombre técnico) |
| `CommercialName` | string | Nombre comercial |
| `ShortDescription` | string? | Descripción corta |
| `LongDescription` | string? | Descripción extensa |
| `BrandId` | long? | FK a la marca |
| `ManufacturerId` | long? | FK al fabricante |
| `DefaultUomId` | int | UoM base del producto |
| `IsStockable` | bool | Si se lleva inventario |
| `IsSellable` | bool | Si se puede vender |
| `IsPurchasable` | bool | Si se puede comprar |
| `LifecycleStatusId` | int | Estado del ciclo de vida |

**Relaciones:**
- `CompanyProducts` — instancias del producto por empresa (SKU, precio, costo)
- `ProductCodes` — códigos alternativos (EAN, UPC, código interno)
- `ProductCategories` — categorías asignadas
- `ClassificationLinks` — clasificaciones temáticas
- `ProductAttributes` — atributos dinámicos
- `ProductUoms` — unidades de medida alternativas con factor de conversión
- `Variants` — variantes (talla, color, etc.)

---

#### `CompanyProduct` — Instancia de producto por empresa
Vincula un `Product` global con una empresa específica. Aquí se gestionan SKU, precio de venta, costo promedio, estado de activación, etc.

**Campos clave:**
- `CompanyProductId` (long): PK
- `ProductId` (long): FK al producto global
- `EmpresaId` (int): FK a la empresa
- `SKU` (string): código único dentro de la empresa
- `DefaultSalePrice` (decimal): precio de venta por defecto
- `AverageCost` (decimal): costo promedio actual

---

#### `Catalog` — Catálogo de productos
Agrupador de nivel superior para productos. Una empresa puede tener múltiples catálogos (ej: "Catálogo Bolivia 2026", "Importaciones").

---

#### `Brand` — Marca
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `BrandId` | long | PK |
| `Name` | string | Nombre de la marca |
| `LogoUrl` | string? | URL del logo |

---

#### `Manufacturer` — Fabricante
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ManufacturerId` | long | PK |
| `Name` | string | Nombre del fabricante |
| `Country` | string? | País de origen |

---

#### `Category` — Categoría de producto
Estructura jerárquica de categorías (árbol N-niveles).

---

#### `ProductClassification` — Clasificación temática
Clasificaciones adicionales para el producto (por uso, por sector, etc.).

---

#### `AttributeDefinition` — Definición de atributos dinámicos
Define los atributos configurables para los productos (ej: "Color", "Talla", "Material").

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `AttributeDefinitionId` | long | PK |
| `Name` | string | Nombre del atributo |
| `DataType` | string | Text, Number, Boolean, Select |
| `IsRequired` | bool | Si es obligatorio |

---

#### `AttributeOption` — Opciones de atributos
Para atributos de tipo "Select", define las opciones disponibles.

---

#### `ProductAttribute` — Valor de atributo en producto
Vincula un producto con el valor de un atributo definido.

---

#### `ProductVariant` — Variante de producto
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ProductVariantId` | long | PK |
| `ProductId` | long | FK al producto base |
| `SKU` | string | SKU específico de la variante |
| `Barcode` | string? | Código de barras |

---

#### `VariantAttributeValue` — Valor de atributo para variante
Vincula una variante con los valores de atributos que la identifican (ej: Color=Rojo, Talla=M).

---

#### `ProductUom` — UoM alternativa del producto
Permite definir múltiples unidades de medida para un producto con factores de conversión. (ej: UN → CAJA factor 12).

---

#### `ProductCode` — Códigos adicionales del producto
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ProductCodeId` | long | PK |
| `ProductId` | long | FK al producto |
| `CodeType` | string | EAN13, UPC, DUN14, Interno, Proveedor |
| `Code` | string | El código en sí |

---

#### `ProductStatus` — Estado de ciclo de vida
Catálogo de estados: Activo, Discontinuado, En Desarrollo, Agotado, etc.

---

#### `CompanyProductFeature` — Funcionalidades activables por empresa
Permite activar funcionalidades opcionales de un producto por empresa (lotes, trazabilidad, etc.).

---

### 6.2 Endpoints API — MDM

#### Clientes (`/api/v1/clientes`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista clientes de la empresa activa |
| GET | `/{id}` | Cliente por ID |
| POST | `/` | Crear cliente |
| PUT | `/{id}` | Actualizar cliente |
| DELETE | `/{id}` | Eliminar cliente |

#### Proveedores (`/api/v1/proveedores`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista proveedores |
| GET | `/{id}` | Proveedor por ID |
| POST | `/` | Crear proveedor |
| PUT | `/{id}` | Actualizar proveedor |
| DELETE | `/{id}` | Eliminar proveedor |

#### Almacenes (`/api/v1/almacenes`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista almacenes |
| GET | `/{id}` | Almacén por ID |
| POST | `/` | Crear almacén |
| PUT | `/{id}` | Actualizar almacén |
| DELETE | `/{id}` | Eliminar almacén |

#### Unidades de Medida (`/api/v1/unidadesmedida`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista UoMs de la empresa |
| GET | `/{id}` | UoM por ID |
| POST | `/` | Crear UoM |
| PUT | `/{id}` | Actualizar UoM |
| DELETE | `/{id}` | Eliminar UoM |

#### Categorías de Producto (`/api/v1/categoriasproducto`)
CRUD estándar sobre el árbol de categorías.

#### Productos Clásicos (`/api/v1/productos`)
Gestión de productos en el modelo clásico (legacy/compatibilidad).

#### MDM — Catálogos (`/api/v1/mdm/catalogs`)
CRUD de catálogos de productos.

#### MDM — Marcas (`/api/v1/mdm/brands`)
CRUD de marcas.

#### MDM — Fabricantes (`/api/v1/mdm/manufacturers`)
CRUD de fabricantes.

#### MDM — Categorías MDM (`/api/v1/mdm/categories`)
CRUD de categorías en el nuevo modelo MDM.

#### MDM — Productos (`/api/v1/mdm/products`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista productos (filtro por catálogo) |
| GET | `/{id}` | Producto por ID |
| POST | `/` | Crear producto |
| PUT | `/{id}` | Actualizar producto |
| DELETE | `/{id}` | Eliminar producto |
| GET | `/{id}/codes` | Códigos del producto |
| POST | `/{id}/codes` | Agregar código |
| DELETE | `/{id}/codes/{codeId}` | Eliminar código |
| GET | `/{id}/attributes` | Atributos del producto |

#### MDM — Variantes (`/api/v1/mdm/variants`)
CRUD de variantes de producto.

#### MDM — UoM de Productos (`/api/v1/mdm/product-uoms`)
Gestión de unidades alternativas con factores de conversión.

#### MDM — Atributos (`/api/v1/mdm/attributes`)
CRUD de definiciones de atributos dinámicos.

#### MDM — Productos Empresa (`/api/v1/mdm/company-products`)
Gestión de la instancia empresa-producto (SKU, precios, activación).

#### MDM — Códigos Producto (`/api/v1/mdm/product-codes`)
Gestión de códigos alternativos (EAN, UPC, internos).

---

## 7. Módulo ACC — Contabilidad

El módulo de Contabilidad implementa el plan de cuentas, asientos contables en partida doble, períodos, cierre contable, presupuestos, estados financieros y conciliación.

### 7.1 Entidades de Dominio ACC

#### `CuentaContable` — Plan de Cuentas
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `CuentaContableId` | int | PK |
| `Codigo` | string | Código estructurado: "1", "1.1", "1.1.3.01" |
| `Nombre` | string | Nombre descriptivo |
| `Tipo` | TipoCuenta | Activo, Pasivo, Patrimonio, Ingreso, Gasto, Costo |
| `Naturaleza` | NaturalezaCuenta | Deudora o Acreedora |
| `Nivel` | int | Nivel jerárquico (1=grupo, 2=subgrupo, 3=cuenta, 4=subcuenta) |
| `CuentaPadreId` | int? | FK a la cuenta padre (null = nivel raíz) |
| `PermiteMovimientos` | bool | True si acepta asientos (cuenta hoja) |
| `Descripcion` | string? | Notas adicionales |
| `SaldoActual` | decimal | Saldo acumulado actual |
| `ClasificacionFlujo` | enum | NoAplica, Operacional, Inversion, Financiacion |

**Método de dominio:**
- `ActualizarClasificacionFlujo(clasificacion)`: cambia la clasificación para el flujo de efectivo.

---

#### `AsientoContable` — Comprobante contable (Journal Entry)
Implementa partida doble. La suma de Debe debe igualar la suma de Haber.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `AsientoContableId` | long | PK |
| `TipoComprobanteId` | int? | FK a tipo (Ingreso=CI, Egreso=CE, Traspaso=CT) |
| `Numero` | string | Número único por período/tipo (ej: CI-001) |
| `Fecha` | DateTime | Fecha contable |
| `Gestion` | int | Año fiscal (ej: 2026) |
| `TipoRegistro` | string | Manual, Automático, Ajuste |
| `Estado` | string | Borrador, Contabilizado, Anulado |
| `Concepto` | string? | Concepto del comprobante |
| `Glosa` | string | Descripción/glosa general |
| `TipoCambioId` | int? | FK al tipo de cambio (Dólar, UFV) |
| `ValorTipoCambio` | decimal? | Valor vigente al momento del registro |
| `TipoPagoId` | int? | FK a tipo de pago (Cheque, Efectivo, QR) |
| `NumeroDocumentoPago` | string? | Nro. cheque, referencia QR, etc. |
| `RegistradoPorId` | int? | FK al usuario que registró |
| `RegistradoPorNombre` | string? | Nombre capturado (firma histórica) |
| `OrigenTipo` | string? | Tipo de documento origen (Recepción, Importación, Venta) |
| `OrigenId` | long? | ID del documento origen |
| `OrigenReferencia` | string? | Referencia legible (ej: REC-000001) |
| `TotalDebe` | decimal | Suma de débitos (calculado) |
| `TotalHaber` | decimal | Suma de créditos (calculado) |

**Métodos de dominio:**
- `AgregarLinea(linea)`: agrega una línea y recalcula totales (solo en Borrador)
- `EliminarLinea(lineaId)`: elimina una línea (solo en Borrador)
- `ValidarCuadratura()`: retorna `Result` indicando si Debe == Haber
- `Contabilizar(nombre?)`: cambia estado a Contabilizado (validando cuadratura)
- `Anular(motivo)`: cambia estado a Anulado y agrega nota en la Glosa
- `EstablecerTotales(debe, haber)`: establece totales explícitamente
- `EstaCuadrado()`: bool — verifica partida doble

---

#### `AsientoContableLinea` — Línea de comprobante
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `AsientoContableLineaId` | long | PK |
| `AsientoContableId` | long | FK al comprobante |
| `CuentaContableId` | int | FK a la cuenta contable |
| `Debe` | decimal | Monto débito |
| `Haber` | decimal | Monto crédito |
| `Glosa` | string | Descripción de la línea |
| `CentroCostoId` | int? | FK al centro de costo |

---

#### `PeriodoContable` — Período mensual
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `PeriodoContableId` | int | PK |
| `Anio` | int | Año fiscal (ej: 2026) |
| `Mes` | int | Mes 1-12 |
| `Nombre` | string | Nombre (ej: "Enero 2026") |
| `Estado` | string | "Abierto" o "Cerrado" |
| `FechaCierre` | DateTime? | Fecha en que se cerró |
| `CerradoPorId` | int? | FK al usuario que cerró |
| `CerradoPorNombre` | string? | Firma histórica |

---

#### `CierreContable` — Cierre anual/mensual
Registra el proceso de cierre de gestión con generación del asiento de cierre y apertura del siguiente período.

---

#### `PlantillaContable` — Plantilla de asiento
Permite guardar modelos de asientos frecuentes para ser reutilizados.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `PlantillaContableId` | int | PK |
| `Nombre` | string | Nombre de la plantilla |
| `Descripcion` | string? | Descripción |
| `Lineas` | collection | Líneas de la plantilla |

---

#### `TipoComprobante` — Tipos de comprobante
Catálogo: Comprobante de Ingreso (CI), Comprobante de Egreso (CE), Comprobante de Traspaso (CT), Comprobante de Ajuste (CA).

---

#### `TipoCambio` — Tipo de cambio vigente
Registros históricos del tipo de cambio oficial (USD/BOB, UFV/BOB, etc.).

---

#### `TipoPago` — Medios de pago
Catálogo: Efectivo, Cheque, Transferencia, QR, Tarjeta.

---

#### `PresupuestoContable` — Presupuesto contable anual
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `PresupuestoContableId` | int | PK |
| `Gestion` | int | Año del presupuesto |
| `Nombre` | string | Nombre/versión del presupuesto |
| `Estado` | string | Borrador → Aprobado → Cerrado |
| `Observaciones` | string? | Notas |

**Métodos de dominio:**
- `Aprobar()`: pasa de Borrador a Aprobado
- `Cerrar()`: pasa de Aprobado a Cerrado
- `AgregarLinea(linea)`: agrega línea (solo en Borrador)

---

#### `PresupuestoContableLinea` — Línea de presupuesto
Monto presupuestado por cuenta y mes.

---

### 7.2 Endpoints API — ACC

#### Cuentas Contables (`/api/v1/cuentascontables`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista plan de cuentas (filtros: tipo, nivel) |
| GET | `/{id}` | Cuenta por ID |
| POST | `/` | Crear cuenta |
| PUT | `/{id}` | Actualizar cuenta |
| DELETE | `/{id}` | Eliminar cuenta |

#### Asientos Contables (`/api/v1/contabilidad/asientos`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista asientos (filtros: desde, hasta, estado, tipo, búsqueda) |
| GET | `/{id}` | Asiento por ID con líneas |
| POST | `/` | Crear asiento en Borrador |
| PUT | `/{id}` | Actualizar asiento |
| POST | `/{id}/copiar` | Clonar un asiento existente |
| POST | `/{id}/contabilizar` | Cambiar estado a Contabilizado |
| POST | `/{id}/anular` | Anular asiento Contabilizado |
| DELETE | `/{id}` | Eliminar asiento en Borrador |
| GET | `/tipos-comprobante` | Catálogo tipos de comprobante |
| GET | `/tipos-cambio` | Catálogo tipos de cambio |
| GET | `/tipos-pago` | Catálogo medios de pago |
| POST | `/seed-catalogos` | Seed de catálogos contables |
| GET | `/exportar` | Exportar asientos (Excel/CSV/JSON/XML) |
| GET | `/{id}/exportar-excel` | Exportar comprobante individual a Excel |
| GET | `/exportar-excel-plano` | Exportar detalle plano para migración |
| GET | `/{id}/documentos` | Documentos adjuntos al comprobante |
| POST | `/{id}/documentos` | Adjuntar documento |
| DELETE | `/{id}/documentos/{docId}` | Remover documento adjunto |
| POST | `/importar` | Importación masiva desde Excel |
| GET | `/plantilla-importacion` | Descargar plantilla Excel vacía |

#### Períodos Contables (`/api/v1/periodoscontables`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista períodos |
| GET | `/{id}` | Período por ID |
| POST | `/` | Crear período |
| POST | `/{id}/cerrar` | Cerrar período |
| GET | `/actual` | Período contable activo actual |

#### Plantillas Contables (`/api/v1/plantillascontables`)
CRUD de plantillas de asientos contables frecuentes.

#### Cierre Contable (`/api/v1/contabilidad`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/cierre-contable/anual` | Ejecutar cierre anual de gestión |
| POST | `/cierre-contable` | Ejecutar cierre de período |
| GET | `/cierre-contable/{gestion}` | Obtener cierre de una gestión |

#### Estados Financieros (`/api/v1/contabilidad/estados-financieros`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/balance-general` | Balance General a fecha de corte |
| GET | `/balance-general/export` | Exportar Balance (xlsx/pdf) |
| GET | `/estado-resultados` | Estado de Resultados |
| GET | `/estado-resultados/export` | Exportar ER (xlsx/pdf) |
| GET | `/flujo-efectivo` | Flujo de Efectivo |
| GET | `/sumas-saldos` | Balance de Sumas y Saldos |
| GET | `/libro-diario` | Libro Diario (asientos cronológico) |
| GET | `/libro-mayor` | Libro Mayor (movimientos por cuenta) |
| GET | `/ratios` | Ratios financieros (liquidez, endeudamiento, etc.) |

#### Presupuesto (`/api/v1/presupuesto`)
CRUD de presupuestos con aprobación y gestión de líneas.

#### Centros de Costo (`/api/v1/centroscosto`)
CRUD de centros de costo con estructura jerárquica.

---

## 8. Módulo CMP — Compras e Importación

El módulo CMP cubre el ciclo completo de compras, desde la necesidad interna (Orden de Pedido) hasta la recepción y el proceso de importación internacional con hoja de costos de importación.

### 8.1 Flujo Completo del Proceso de Compras

```
[1] Orden de Pedido (OP)           → Borrador → EnRevision → (Stock / Aprobado)
[2] Orden de Compra (OC)           → Borrador → Confirmado → PendienteAprobacion
[3] Aprobación Interna             → Aprobado / Rechazado
[4] Confirmación Proveedor (PI)    → EnviadaProveedor → EnNegociacion → ConfirmadaProveedor
[5] Pago Anticipo/Saldo            → PagoProgramado → (Ejecutado)
[6] Embarque                       → EnTransito (Expediente de Importación)
[7] Despacho Aduanero              → Arribado → EnAduana → (ObservacionAduana) → Liberado
[8] Levante Aduanero               → Liberado
[9] Recepción Mercadería           → RecepcionParcial / RecepcionConDiferencias → Cerrado
[10] Hoja de Importación (LC)      → Cálculo y prorrateo de landed costs → Inventario
```

### 8.2 Entidades de Dominio CMP

#### `OrdenPedido` (OP) — Solicitud interna de compra
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `OrdenPedidoId` | long | PK |
| `Numero` | string | Correlativo (ej: OP-000001) |
| `FechaEmision` | DateTime | Fecha de emisión |
| `FechaRequerida` | DateTime? | Fecha en que se necesita |
| `SolicitanteId` | int | FK al usuario solicitante |
| `CentroCosto` | string? | Centro de costo que imputa |
| `Urgencia` | NivelUrgencia | Normal, Urgente, Crítico |
| `AlmacenDestinoId` | int | Almacén donde se requiere el stock |
| `Estado` | EstadoDocumento | Flujo completo de estados |
| `StockCubre` | bool? | True si stock/tránsito cubre (→ cierre sin OC) |
| `ObservacionesRevisionStock` | string? | Notas de revisión de stock |
| `MotivoRechazo` | string? | Motivo de rechazo/anulación |

**Relaciones:** Líneas (`OrdenPedidoLinea`), OCs generadas (`OrdenesCompra`)

---

#### `OrdenPedidoLinea` — Línea de Orden de Pedido
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ProductoId` | int/long | FK al producto |
| `Cantidad` | decimal | Cantidad requerida |
| `UomId` | int | Unidad de medida |
| `Observaciones` | string? | Notas de la línea |

---

#### `OrdenCompra` (OC) — Orden de Compra
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `OrdenCompraId` | long | PK |
| `Numero` | string | Correlativo (ej: OC-000001) |
| `FechaEmision` | DateTime | Fecha de emisión |
| `FechaEntregaEstimada` | DateTime? | ETD estimado del proveedor |
| `ProveedorId` | int | FK al proveedor |
| `AlmacenDestinoId` | int | Almacén de recepción |
| `MonedaId` | string | ISO 4217 (ej: "USD", "BOB") |
| `TasaCambio` | decimal | Tasa vs moneda base |
| `Estado` | EstadoDocumento | Flujo completo |
| `CondicionPago` | string? | Contado, 30d, 60d, etc. |
| `Incoterm` | string? | FOB, CIF, EXW, DDP, etc. |
| `ReferenciaExterna` | string? | Nro. cotización del proveedor |
| `MotivoRechazo` | string? | Motivo de rechazo |
| `Subtotal` | decimal | Suma de líneas |
| `Descuento` | decimal | Descuentos totales |
| `Impuesto` | decimal | IVA y otros |
| `Total` | decimal | Total final |
| `OrdenPedidoId` | long? | OP que originó la OC |
| `ExpedienteImportacionId` | long? | Expediente al que pertenece |

**Método de dominio:** `RecalcularTotales()` — suma líneas y actualiza totales.

---

#### `OrdenCompraLinea` — Línea de Orden de Compra
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ProductoId` | int/long | FK al producto |
| `Descripcion` | string | Descripción del ítem |
| `Cantidad` | decimal | Cantidad pedida |
| `PrecioUnitario` | decimal | Precio unitario en moneda de OC |
| `Descuento` | decimal | Porcentaje de descuento |
| `MontoDescuento` | decimal | Monto calculado |
| `Impuesto` | decimal | Porcentaje de impuesto |
| `MontoImpuesto` | decimal | Monto calculado |
| `Subtotal` | decimal | Cantidad × Precio - Descuento |
| `CantidadRecibida` | decimal | Acumulado de recepciones parciales |

---

#### `ConfirmacionProveedor` — PI / Confirmación del proveedor
Registro histórico de cada confirmación o negociación del proveedor sobre una OC.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Tipo` | string | "Confirmacion" o "Negociacion" |
| `FechaConfirmacion` | DateTime | Fecha del evento |
| `NumeroPi` | string? | Número de Proforma Invoice |
| `PrecioConfirmado` | decimal? | Precio final confirmado |
| `FechaEntregaConfirmada` | DateTime? | ETD confirmado por proveedor |
| `Observaciones` | string? | Notas |

---

#### `PagoOrdenCompra` — Pago programado/ejecutado
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `TipoPago` | string | "Anticipo" o "Saldo" |
| `Porcentaje` | decimal | % del total de la OC |
| `Monto` | decimal | Monto a pagar |
| `FechaProgramada` | DateTime | Fecha planificada |
| `FechaEjecucion` | DateTime? | Fecha real de pago |
| `Estado` | string | Programado, Ejecutado, Cancelado |
| `ReferenciaBancaria` | string? | Número de transferencia/cheque |

---

#### `ExpedienteImportacion` — Expediente de embarque internacional
Agrupa 1..N Órdenes de Compra en un mismo embarque.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ExpedienteImportacionId` | long | PK |
| `Numero` | string | Correlativo (ej: EXP-000001) |
| `Estado` | EstadoDocumento | Borrador → EnTransito → Arribado → EnAduana → Liberado → Cerrado |
| `Incoterm` | string? | FOB, CIF, EXW, DDP |
| `ModalidadTransporte` | string? | Marítimo, Aéreo, Terrestre, Multimodal |
| `PaisOrigen` | string? | País de origen |
| `PuertoOrigen` | string? | Puerto/aeropuerto origen |
| `PuertoDestino` | string? | Puerto/aeropuerto destino |
| `Forwarder` | string? | Agente de carga |
| `Aseguradora` | string? | Compañía aseguradora |
| `NumeroPólizaSeguro` | string? | Número de póliza |
| `NumeroBLAWB` | string? | Bill of Lading / Air Waybill |
| `ETD` | DateTime? | Estimated Time of Departure |
| `ATD` | DateTime? | Actual Time of Departure |
| `ETA` | DateTime? | Estimated Time of Arrival |
| `ATA` | DateTime? | Actual Time of Arrival |
| `NumeroDUIDIM` | string? | Número declaración aduanera (DUI/DIM) |
| `Despachante` | string? | Despachante de aduana |
| `FechaPresentacionAduana` | DateTime? | Presentación de documentos |
| `TotalTributos` | decimal? | Monto total de aranceles/tributos |
| `TuvoObservacionAduana` | bool | Si hubo observación/aforo |
| `DetalleObservacionAduana` | string? | Descripción del aforo |
| `FechaLevante` | DateTime? | Fecha de liberación/levante |

---

#### `HitoExpediente` — Hito de tracking del expediente
Registro de eventos importantes del tránsito (puerto, aduana, etc.).

---

#### `RecepcionCompra` — Recepción de mercadería
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `RecepcionCompraId` | long | PK |
| `Numero` | string | Correlativo (ej: REC-000001) |
| `OrdenCompraId` | long | FK a la OC |
| `FechaRecepcion` | DateTime | Fecha efectiva |
| `AlmacenId` | int | Almacén de recepción |
| `DocumentoProveedor` | string? | Guía/factura/remisión del proveedor |
| `Confirmada` | bool | True si ya se generaron movimientos de inventario |
| `TieneDiferencias` | bool | Si hubo faltantes/daños/sobrantes |
| `TipoDiferencia` | string? | Faltante, Daño, Sobrante, Mixto |
| `ActaDiferencias` | string? | Descripción del acta levantada |
| `NumeroReclamo` | string? | Número de reclamo al proveedor |
| `EnCuarentena` | bool | Si unidades con diferencias están en cuarentena |
| `UbicacionCuarentena` | string? | Ubicación de cuarentena |
| `ResultadoControlCalidad` | string? | Aprobado, Rechazado, Observado |

---

#### `RecepcionCompraLinea` — Línea de recepción
Registra la cantidad recibida por línea de OC.

---

#### `HojaImportacion` — Hoja de Costos de Importación (Landed Cost)
Calcula y prorratea los costos de importación (flete, seguro, aduana, etc.) sobre los productos recibidos.

---

#### `GastoImportacion` — Gasto de importación
Registra cada gasto asociado a la importación (flete, seguro, tributos, almacenaje, etc.) con método de prorrateo.

---

#### `ImportacionLinea` — Línea de hoja de importación
Registra el costo asignado a cada producto/línea después del prorrateo.

---

#### `TipoPagoImportacion` — Tipos de pago para importación
Catálogo de los tipos de pago utilizados en importaciones (Carta de Crédito, Transferencia, etc.).

---

### 8.3 Endpoints API — CMP

#### Órdenes de Pedido (`/api/v1/compras/pedidos`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista OPs con filtros |
| GET | `/{id}` | OP por ID con líneas |
| POST | `/` | Crear OP |
| PUT | `/{id}` | Actualizar OP |
| POST | `/{id}/lineas` | Agregar línea |
| PUT | `/{id}/lineas/{lineaId}` | Actualizar línea |
| DELETE | `/{id}/lineas/{lineaId}` | Eliminar línea |
| POST | `/{id}/estado` | Cambiar estado |
| DELETE | `/{id}` | Eliminar OP en Borrador |

#### Órdenes de Compra (`/api/v1/compras/ordenes`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista OCs (filtros: proveedor, estado, fechas) |
| GET | `/{id}` | OC por ID con líneas |
| POST | `/` | Crear OC |
| PUT | `/{id}` | Actualizar OC |
| POST | `/{id}/lineas` | Agregar línea |
| PUT | `/{id}/lineas/{lineaId}` | Actualizar línea |
| DELETE | `/{id}/lineas/{lineaId}` | Eliminar línea |
| POST | `/{id}/estado` | Cambiar estado (Confirmar, Anular) |
| POST | `/{id}/aprobar` | Aprobar o rechazar (Gate Finanzas) |
| POST | `/{id}/confirmacion-proveedor` | Registrar confirmación/PI del proveedor |
| GET | `/{id}/confirmaciones` | Historial de confirmaciones |
| POST | `/{id}/pagos` | Programar pago (anticipo/saldo) |
| POST | `/{id}/pagos/{pagoId}/ejecutar` | Ejecutar pago programado |
| GET | `/{id}/pagos` | Lista de pagos de la OC |
| DELETE | `/{id}` | Eliminar OC en Borrador |

#### Recepciones de Compra (`/api/v1/compras/recepciones`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista recepciones |
| GET | `/{id}` | Recepción por ID |
| POST | `/` | Crear recepción |
| POST | `/{id}/confirmar` | Confirmar y generar movimientos de inventario |
| DELETE | `/{id}` | Eliminar recepción no confirmada |

#### Expedientes de Importación (`/api/v1/compras/expedientes`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista expedientes |
| GET | `/{id}` | Expediente por ID |
| POST | `/` | Crear expediente |
| PUT | `/{id}` | Actualizar expediente |
| POST | `/{id}/estado` | Cambiar estado (tracking) |
| DELETE | `/{id}` | Eliminar expediente |

#### Importaciones - Hoja de Costos (`/api/v1/importaciones`)
Gestión de hojas de importación con cálculo de landed cost y prorrateo.

---

## 9. Módulo INV — Inventario

El módulo INV gestiona el inventario en tiempo real con soporte multi-almacén, kardex, costo promedio ponderado y trazabilidad de movimientos.

### 9.1 Entidades de Dominio INV

#### `MovimientoInventario` — Movimiento de inventario
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | PK |
| `Number` | string | Número único (ej: MOV-2026-00001) |
| `MovementType` | string | Receipt, Issue, Adjustment, Transfer |
| `MovementDate` | DateTime | Fecha del movimiento |
| `CompanyProductId` | long | FK al producto-empresa |
| `WarehouseId` | int | Almacén origen |
| `DestinationWarehouseId` | int? | Almacén destino (solo Transfer) |
| `Quantity` | decimal | Cantidad (siempre positiva) |
| `UnitCost` | decimal | Costo unitario del movimiento |
| `TotalCost` | decimal | UnitCost × Quantity |
| `Reference` | string? | Referencia al documento origen (PO-001, etc.) |
| `Notes` | string? | Notas adicionales |

---

#### `StockProducto` — Saldo de inventario actual
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | PK |
| `CompanyProductId` | long | FK al producto-empresa |
| `AlmacenId` | int | Almacén |
| `CurrentStock` | decimal | Saldo actual disponible |
| `AverageCost` | decimal | Costo promedio ponderado (WAC) vigente |
| `LastUpdated` | DateTime | Fecha del último movimiento |

El costo promedio se recalcula automáticamente con cada recepción usando la fórmula WAC:
```
NuevoCostoPromedio = (StockAnterior × CostoAnterior + CantidadEntrante × CostoEntrante)
                     / (StockAnterior + CantidadEntrante)
```

---

### 9.2 Endpoints API — INV

#### Inventario (`/api/v1/inventario`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/movimientos` | Lista movimientos (filtros: producto, almacén, tipo, fechas) |
| GET | `/movimientos/{id}` | Movimiento por ID |
| POST | `/movimientos` | Registrar nuevo movimiento |
| GET | `/kardex/{productoId}` | Kardex del producto (con saldo acumulado) |
| GET | `/stock` | Stock actual por producto/almacén |

---

## 10. Módulo LOG — Logística

El módulo LOG gestiona las hojas de ruta para el seguimiento de operaciones logísticas, tanto internas (importación, traspasos) como de servicio al cliente (entregas, recojos, devoluciones).

### 10.1 Entidades de Dominio LOG

#### `HojaRuta` — Hoja de Ruta logística
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `HojaRutaId` | long | PK |
| `NumeroHojaRuta` | string | Correlativo (ej: HR-000001) |
| `TipoOP` | string | "INTERNA" o "CLIENTE" |
| `SubTipo` | string | INTERNA: IMPORTACION/TRASPASO_INTERNO/DEVOLUCION/AJUSTE — CLIENTE: ENTREGA/RECOJO/DEVOLUCION_CLI |
| `AlmacenOrigenId` | int | Almacén de origen (obligatorio para INTERNA) |
| `AlmacenDestinoId` | int? | Almacén destino (TRASPASO_INTERNO) |
| `ProveedorCliente` | string? | Proveedor (INTERNA) o Cliente (CLIENTE) |
| `DireccionEntrega` | string? | Dirección de entrega (CLIENTE) |
| `ContactoCliente` | string? | Teléfono/email de contacto |
| `ResponsableUsuario` | string | Usuario responsable |
| `FechaRegistro` | DateTime | Fecha de registro automática |
| `FechaDocumento` | DateTime | Fecha del documento |
| `ETA` | DateTime? | ETA de arribo (INTERNA) o fecha estimada de entrega (CLIENTE) |
| `Estado` | string | BORRADOR, EN_PROCESO, COMPLETADO, ANULADO |
| `SubEstado` | string | INTERNA: INICIADO→EMBARCADO→EN_ADUANA→AFORO→LEVANTE→EN_RECEPCION→CERRADO / CLIENTE: INICIADO→PREPARANDO→EN_RUTA→ENTREGADO/NO_ENTREGADO→CERRADO |
| `OrdenPedidoId` | long? | OP origen (opcional) |

---

#### `HojaRutaHistorial` — Historial de estados de la hoja de ruta
Registro cronológico de cada cambio de estado con usuario, fecha y observaciones.

---

### 10.2 Endpoints API — LOG

#### Hojas de Ruta (`/api/v1/hojasruta`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista hojas de ruta |
| GET | `/{id}` | Hoja de ruta por ID con historial |
| POST | `/` | Crear hoja de ruta |
| PUT | `/{id}` | Actualizar hoja de ruta |
| POST | `/{id}/estado` | Cambiar estado/sub-estado |
| DELETE | `/{id}` | Eliminar (solo BORRADOR) |

---

## 11. Módulo BNC — Bancario y Conciliación

El módulo BNC gestiona la conciliación bancaria: importación de extractos bancarios y match automático con los asientos contables.

### 11.1 Entidades de Dominio BNC

#### `ConciliacionBancaria`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ConciliacionBancariaId` | int | PK |
| `CuentaContableId` | int | FK a cuenta bancaria en plan de cuentas |
| `PeriodoContableId` | int | FK al período |
| `SaldoExtracto` | decimal | Saldo según extracto bancario |
| `SaldoContable` | decimal | Saldo según libros contables |
| `Diferencia` | decimal | SaldoExtracto - SaldoContable (calculado) |
| `EstaConciliado` | bool | True si Diferencia <= 0.01 |
| `Estado` | string | EnProceso, Conciliado, Aprobado |

**Métodos de dominio:**
- `ActualizarSaldos(saldoExtracto, saldoContable)`: actualiza saldos y recalcula estado
- `Aprobar()`: aprueba la conciliación (solo si está conciliada)

---

#### `ExtractoBancario`
Registra cada línea del extracto bancario importado para el proceso de conciliación.

---

### 11.2 Endpoints API — BNC

#### Conciliación Bancaria (`/api/v1/conciliacion-bancaria`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/importar` | Importar extracto bancario (CSV/Excel) |
| GET | `/sugerencias` | Sugerencias de match automático |
| POST | `/confirmar-match` | Confirmar match manual/automático |
| GET | `/resumen` | Resumen de conciliación por cuenta/período |
| POST | `/aprobar` | Aprobar conciliación conciliada |

---

## 12. Módulo ACT — Activos Fijos

El módulo ACT gestiona el registro y depreciación automática de activos fijos conforme al DS 24051 (Bolivia).

### 12.1 Entidades de Dominio ACT

#### `ActivoFijo`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ActivoFijoId` | long | PK |
| `Codigo` | string | Código único del activo |
| `Descripcion` | string | Descripción del bien |
| `CategoriaActivo` | string | Edificio, Vehículo, Maquinaria, EquipoComp, Mobiliario |
| `CuentaContableId` | int | FK a cuenta de activo |
| `CuentaDepreciacionId` | int | FK a cuenta de depreciación acumulada |
| `CuentaGastoDepreciacionId` | int | FK a cuenta de gasto depreciación |
| `FechaAdquisicion` | DateTime | Fecha de compra |
| `CostoAdquisicion` | decimal | Costo histórico |
| `ValorResidual` | decimal | Valor residual al final de la vida útil |
| `TasaAnualDS24051` | decimal | Tasa de depreciación anual (según DS 24051) |
| `VidaUtilAnios` | int | Vida útil en años |
| `DepreciacionAcumulada` | decimal | Depreciación acumulada total |
| `ValorEnLibros` | decimal | CostoAdquisicion - DepreciacionAcumulada - ValorResidual (calculado) |
| `DepreciacionCompleta` | bool | True si ya se depreció completamente |
| `Estado` | string | Activo, Dado de Baja, Vendido |

**Métodos de dominio:**
- `CalcularCuotaMensual()`: retorna la cuota mensual = (Costo - ValorResidual) × Tasa / 12
- `AplicarDepreciacionMensual(periodoId, fecha)`: registra depreciación del período
- `DarDeBaja(motivo)`: cambia estado a "Dado de Baja"

---

#### `DepreciacionMensual`
Registra la cuota de depreciación aplicada en cada período contable.

---

### 12.2 Endpoints API — ACT
Gestión completa de activos fijos con cálculo y registro automático de depreciación mensual mediante servicio de aplicación.

---

## 13. Módulo TRB — Tributario

El módulo TRB gestiona el cálculo, declaración y pago de impuestos (Bolivia: IVA, IT, IUE, RC-IVA).

### 13.1 Entidades de Dominio TRB

#### `RegistroImpuesto`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `RegistroImpuestoId` | long | PK |
| `PeriodoContableId` | int | FK al período |
| `TipoImpuesto` | string | IVA, IT, IUE, RC-IVA |
| `BaseImponible` | decimal | Base imponible calculada |
| `Tasa` | decimal | Alícuota del impuesto |
| `MontoCalculado` | decimal | Base × Tasa |
| `CreditoFiscal` | decimal | Crédito fiscal (solo IVA) |
| `DebitoFiscal` | decimal | Débito fiscal (solo IVA) |
| `SaldoAFavor` | decimal | Arrastre de saldo a favor IVA |
| `MontoAPagar` | decimal | Neto a pagar |
| `Estado` | string | Calculado → Declarado → Pagado |
| `NumeroCertificado` | string? | Número de certificado/formulario |
| `FechaDeclaracion` | DateTime? | Fecha de presentación |
| `AsientoContableId` | long? | FK al asiento de pago |

**Métodos de dominio:**
- `MarcarDeclarado(numero, fecha)`: registra la declaración
- `MarcarPagado(asientoId)`: vincula con el asiento de pago

---

### 13.2 Endpoints API — TRB

#### Tributario (`/api/v1/tributario`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista registros de impuestos |
| GET | `/{id}` | Registro por ID |
| POST | `/calcular` | Calcular impuestos del período |
| POST | `/{id}/declarar` | Marcar como declarado |
| POST | `/{id}/pagar` | Marcar como pagado |

---

## 14. Módulo PRC — Precios

El módulo PRC gestiona listas de precios por empresa, moneda y canal de venta.

### 14.1 Entidades de Dominio PRC

#### `PriceList` — Lista de precios
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `PriceListId` | long | PK |
| `Code` | string | Código único |
| `Name` | string | Nombre descriptivo |
| `CurrencyId` | string | ISO 4217 |
| `ChannelId` | int? | Canal de venta (Retail, Mayorista, etc.) |
| `ValidFrom` | DateOnly? | Vigencia desde |
| `ValidTo` | DateOnly? | Vigencia hasta |
| `IsDefault` | bool | Si es la lista por defecto |

---

#### `PriceListItem` — Ítem de lista de precios
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `PriceListItemId` | long | PK |
| `PriceListId` | long | FK a la lista |
| `CompanyProductId` | long | FK al producto-empresa |
| `UomId` | int | Unidad de medida de referencia |
| `Price` | decimal | Precio en la moneda de la lista |
| `MinimumQuantity` | decimal | Cantidad mínima para aplicar el precio |

---

### 14.2 Endpoints API — PRC

#### Listas de Precios (`/api/v1/prc/price-lists`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Lista todas las listas de precios |
| GET | `/{id}` | Lista de precios por ID |
| POST | `/` | Crear lista de precios |
| PUT | `/{id}` | Actualizar lista |
| DELETE | `/{id}` | Eliminar lista |
| GET | `/{id}/items` | Ítems de la lista |
| POST | `/{id}/items` | Agregar ítem |
| DELETE | `/{id}/items/{itemId}` | Eliminar ítem |

---

## 15. Módulo CST — Costos

El módulo CST gestiona el costeo de productos, reglas de costeo por empresa y perfiles de landed cost (costos de importación).

### 15.1 Entidades de Dominio CST

#### `CentroCosto` — Centro de Costo
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | PK |
| `Codigo` | string | Código único (ej: CC-VEN-01) |
| `Nombre` | string | Nombre descriptivo |
| `Descripcion` | string? | Notas |
| `ParentId` | int? | Centro padre (jerarquía) |
| `EmpresaId` | int | Empresa propietaria |

Soporta estructura jerárquica N-niveles (árbol de centros de costo).

---

#### `CostingRule` — Regla de costeo
Define el método de costeo para un producto o categoría: Promedio Ponderado (WAC), FIFO, LIFO, Costo Estándar.

---

#### `LandedCostProfile` — Perfil de costos de importación
Define una plantilla de conceptos de gastos de importación (flete, seguro, tributos, etc.) con sus métodos de prorrateo (por peso, volumen, valor, cantidad).

---

### 15.2 Endpoints API — CST

#### Centros de Costo (`/api/v1/centroscosto`)
CRUD completo con soporte de jerarquía.

---

## 16. Módulo RUL — Reglas por Industria

El módulo RUL define catálogos de industrias y reglas específicas de productos según el sector industrial de la empresa.

### 16.1 Entidades de Dominio RUL

#### `Industry` — Industria
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IndustryId` | int | PK |
| `Name` | string | Nombre (Retail, Farmacia, Alimentos, etc.) |
| `Code` | string | Código corto |

---

#### `ProductIndustryRule` — Regla de producto por industria
Define restricciones o comportamientos especiales de un producto según la industria: lotes obligatorios, cadena de frío, vencimiento obligatorio, etc.

---

### 16.2 Endpoints API — RUL

#### Industrias (`/api/v1/rul/industries`)
CRUD de catálogo de industrias.

---

## 17. Módulo VER — Versionado de Entidades

El módulo VER provee versionado histórico de entidades críticas del sistema (productos, precios, configuraciones), permitiendo auditar cambios y restaurar versiones anteriores.

### 17.1 Entidades de Dominio VER

#### `EntityVersion`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `EntityVersionId` | long | PK |
| `EntityType` | string | Tipo de entidad ("Product", "PriceList", etc.) |
| `EntityId` | string | PK de la entidad como string |
| `Version` | int | Número de versión (incrementa) |
| `JsonSnapshot` | string | Snapshot JSON de la entidad en esa versión |
| `ChangedBy` | string? | Usuario que generó la versión |
| `ChangedAt` | DateTime | Timestamp UTC |
| `ChangeReason` | string? | Motivo del cambio |

---

## 18. Módulo WF — Workflow

El módulo WF implementa un sistema de workflow polimórfico que permite gestionar tareas de hitos sobre cualquier documento del ERP.

### 18.1 Entidades de Dominio WF

#### `Tarea` — Tarea de Workflow
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | PK |
| `EntityType` | string | Tipo de documento padre: "OrdenPedido", "OrdenCompra", "OrdenVenta", "Embarque" |
| `EntityId` | int | PK del documento padre |
| `Orden` | int | Secuencia de ejecución |
| `Codigo` | string | Código funcional: ETD, ETA, DUI, RECEP, PAGO, etc. |
| `Descripcion` | string | Descripción legible para UI |
| `FechaPlan` | DateOnly? | Fecha planificada |
| `FechaReal` | DateOnly? | Fecha real de ejecución |
| `Estado` | string | PENDIENTE, EN_PROCESO, COMPLETADO, BLOQUEADO |
| `Completado` | bool | Flag de rendimiento (duplica Estado=="COMPLETADO") |
| `Responsable` | string? | Nombre/área responsable |
| `Observaciones` | string? | Notas al completar o bloquear |
| `MetadataJson` | string? | Metadata extendida en JSON libre |

**Método de dominio:**
- `GetMetadata<T>()`: deserializa MetadataJson a tipo T

---

#### `PlantillaTarea` — Plantilla de tarea workflow
Define plantillas reutilizables para los hitos de workflow de cada tipo de documento.

---

### 18.2 Endpoints API — WF

#### Workflow (`/api/v1/workflow`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/tareas` | Lista tareas (filtros: entityType, entityId, estado) |
| GET | `/tareas/{id}` | Tarea por ID |
| POST | `/tareas` | Crear tarea |
| PUT | `/tareas/{id}` | Actualizar tarea |
| POST | `/tareas/{id}/completar` | Marcar tarea como completada |
| POST | `/tareas/{id}/bloquear` | Bloquear tarea |
| DELETE | `/tareas/{id}` | Eliminar tarea |
| GET | `/plantillas` | Lista plantillas de workflow |
| POST | `/plantillas` | Crear plantilla |

#### Configuración Workflow (`/api/v1/configuracion/workflow`)
Interfaz visual para configurar plantillas de tareas por tipo de documento.

---

## 19. Módulo DOC — Documentos Adjuntos

El módulo DOC gestiona documentos digitales (PDF, imágenes, Excel) adjuntos a entidades del sistema, especialmente comprobantes contables.

### 19.1 Entidades de Dominio DOC

#### `Document` — Documento digital
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `DocumentId` | int | PK |
| `FileName` | string | Nombre original del archivo |
| `FileType` | string | MIME type (application/pdf, image/jpeg, etc.) |
| `FileSize` | long | Tamaño en bytes |
| `FilePath` | string | Ruta en el servidor (wwwroot/uploads) |
| `UploadedAt` | DateTime | Timestamp de carga |
| `EmpresaId` | int | Empresa propietaria |

**Tipos de archivo permitidos:** PDF, JPEG, PNG, XLSX, XLS (máximo 10 MB)

---

#### `ComprobanteDocumento` — Vínculo comprobante-documento
Tabla de relación N:M entre `AsientoContable` y `Document`.

---

#### `ProductDocument` — Vínculo producto-documento
Tabla de relación N:M entre `Product` y `Document` (fichas técnicas, certificados, etc.).

---

### 19.2 Endpoints API — DOC

#### Documentos (`/api/v1/documentos`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/` | Subir archivo y registrar (multipart/form-data) |
| GET | `/{id}` | Obtener metadatos del documento |
| GET | `/{id}/download` | Descargar el archivo |
| DELETE | `/{id}` | Eliminar documento (y archivo físico) |

---

## 20. Módulo Ventas (en construcción)

El módulo de Ventas está parcialmente implementado en la capa Web con páginas Blazor de Facturas y Pedidos de Venta, y en el servidor con el endpoint de Órdenes de Pedido de Venta. Su implementación completa corresponde al roadmap v1.0.

### Páginas Web implementadas:
- `/ventas/facturas` — Listado de facturas de venta
- `/ventas/pedidos` — Listado de pedidos de venta

---

## 21. API REST — Resumen de Endpoints

Base URL: `https://localhost:7001/api/v1/`

### Resumen completo de controladores

| Controlador | Ruta Base | Módulo |
|-------------|-----------|--------|
| AuthController | `/auth` | CORE |
| EmpresasController | `/empresas` | CORE |
| UsuariosController | `/usuarios` | CORE |
| RolesController | `/roles` | CORE |
| ParametrosController | `/parametros` | CORE |
| NumeracionesController | `/numeraciones` | CORE |
| AuditLogsController | `/auditlogs` | CORE |
| DiagnosticsController | `/diagnostics` | CORE |
| ClientesController | `/clientes` | MDM |
| ProveedoresController | `/proveedores` | MDM |
| AlmacenesController | `/almacenes` | MDM |
| UnidadesMedidaController | `/unidadesmedida` | MDM |
| CategoriasProductoController | `/categoriasproducto` | MDM |
| ProductosController | `/productos` | MDM |
| MdmProductsController | `/mdm/products` | MDM |
| MdmBrandsController | `/mdm/brands` | MDM |
| MdmManufacturersController | `/mdm/manufacturers` | MDM |
| MdmCatalogsController | `/mdm/catalogs` | MDM |
| MdmCategoriesController | `/mdm/categories` | MDM |
| MdmVariantsController | `/mdm/variants` | MDM |
| MdmProductUomsController | `/mdm/product-uoms` | MDM |
| MdmAttributesController | `/mdm/attributes` | MDM |
| MdmCompanyProductsController | `/mdm/company-products` | MDM |
| MdmProductCodesController | `/mdm/product-codes` | MDM |
| CuentasContablesController | `/cuentascontables` | ACC |
| AsientosContablesController | `/contabilidad/asientos` | ACC |
| PeriodosContablesController | `/periodoscontables` | ACC |
| PlantillasContablesController | `/plantillascontables` | ACC |
| ContabilidadController | `/contabilidad` | ACC |
| EstadosFinancierosController | `/contabilidad/estados-financieros` | ACC |
| PresupuestoController | `/presupuesto` | ACC |
| CentrosCostoController | `/centroscosto` | ACC/CST |
| OrdenesPedidoController | `/compras/pedidos` | CMP |
| OrdenesCompraController | `/compras/ordenes` | CMP |
| RecepcionesCompraController | `/compras/recepciones` | CMP |
| ExpedientesImportacionController | `/compras/expedientes` | CMP |
| ImportacionesController | `/importaciones` | CMP |
| MovimientosInventarioController | `/inventario` | INV |
| HojasRutaController | `/hojasruta` | LOG |
| ConciliacionBancariaController | `/conciliacion-bancaria` | BNC |
| TributarioController | `/tributario` | TRB |
| PrcPriceListsController | `/prc/price-lists` | PRC |
| RulIndustriesController | `/rul/industries` | RUL |
| WorkflowController | `/workflow` | WF |
| DocumentosController | `/documentos` | DOC |

**Total: 45 controladores**

### Convenciones de la API
- **Versionado:** Todas las rutas incluyen `/v{version}` (actualmente v1)
- **Autenticación:** Bearer JWT en todos los endpoints excepto `/auth/login`
- **Formato de respuesta:**
```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": { ... },
  "error": null
}
```
- **Filtrado global de tenant:** Todos los datos se filtran automáticamente por `EmpresaId` del token JWT
- **Soft Delete:** La mayoría de entidades usan eliminación lógica (campo `Activo = false`)

---

## 22. Frontend Web — Páginas Blazor

Aplicación Blazor WebAssembly disponible en `https://localhost:5002`.

### 22.1 Páginas por Módulo

#### Autenticación
| Ruta | Página | Descripción |
|------|--------|-------------|
| `/login` | Login.razor | Formulario de login con JWT |

#### Dashboard
| Ruta | Página | Descripción |
|------|--------|-------------|
| `/` | Dashboard.razor | Dashboard principal |
| `/accesos-directos` | AccesosDirectos.razor | Accesos rápidos por módulo |
| `/dashboards/compras` | DashboardCompras.razor | KPIs del módulo de compras |
| `/dashboards/contabilidad` | DashboardContabilidad.razor | KPIs contables |
| `/dashboards/inventario` | DashboardInventario.razor | KPIs de inventario |

#### Configuración del Sistema (`/config/`)
| Ruta | Página | Descripción |
|------|--------|-------------|
| `/config/empresas` | Empresas.razor | CRUD empresas |
| `/config/usuarios` | Usuarios.razor | CRUD usuarios y roles |
| `/config/roles` | Roles.razor | CRUD roles RBAC |
| `/config/parametros-numeracion` | ParametrosNumeracion.razor | Configuración de correlativos |
| `/config/auditoria` | Auditoria.razor | Visualización del log de auditoría |
| `/config/empresa-demo` | EmpresaDemo.razor | Seed de datos demo |

#### Configuración de Workflow
| Ruta | Página | Descripción |
|------|--------|-------------|
| `/configuracion/workflow` | ConfiguracionWorkflow.razor | Configuración visual de workflow |

#### Módulo MDM (`/mdm/`)
| Ruta | Página | Descripción |
|------|--------|-------------|
| `/mdm/clientes` | Clientes.razor | CRUD clientes |
| `/mdm/proveedores` | Proveedores.razor | CRUD proveedores |
| `/mdm/almacenes` | Almacenes.razor | CRUD almacenes |
| `/mdm/unidades-medida` | UnidadesMedida.razor | CRUD UoM |
| `/mdm/categorias` | Categorias.razor | Árbol de categorías |
| `/mdm/marcas` | Marcas.razor | CRUD marcas |
| `/mdm/fabricantes` | Fabricantes.razor | CRUD fabricantes |
| `/mdm/productos` | Productos.razor | Productos clásicos |
| `/mdm/productos-globales` | ProductosGlobales.razor | Catálogo global MDM |
| `/mdm/productos-empresa` | ProductosEmpresa.razor | Instancias empresa-producto |
| `/mdm/variantes` | VariantesProducto.razor | Variantes de producto |
| `/mdm/atributos` | AtributosProducto.razor | Definición de atributos dinámicos |
| `/mdm/listas-precios` | ListasPrecios.razor | Gestión de listas de precios |

#### Módulo Contabilidad (`/contabilidad/`)
| Ruta | Página | Descripción |
|------|--------|-------------|
| `/contabilidad/dashboard` | DashboardContable.razor | Dashboard contable con KPIs |
| `/contabilidad/plan-cuentas` | PlanCuentas.razor | Plan de cuentas jerárquico |
| `/contabilidad/asientos` | AsientosContables.razor | Gestión de comprobantes |
| `/contabilidad/importar-asientos` | AsientosImportar.razor | Importación masiva desde Excel |
| `/contabilidad/comprobantes` | Comprobantes.razor | Vista de comprobantes |
| `/contabilidad/periodos` | PeriodosContables.razor | Gestión de períodos |
| `/contabilidad/plantillas` | PlantillasContables.razor | Plantillas de asientos |
| `/contabilidad/cierre` | CierreContable.razor | Proceso de cierre contable |
| `/contabilidad/centros-costo` | CentrosCosto.razor | Árbol de centros de costo |
| `/contabilidad/balance-general` | BalanceGeneral.razor | Balance General |
| `/contabilidad/estado-resultados` | EstadoResultados.razor | Estado de Resultados |
| `/contabilidad/flujo-efectivo` | FlujoDEfectivo.razor | Flujo de Efectivo |
| `/contabilidad/libro-diario` | LibroDiario.razor | Libro Diario |
| `/contabilidad/libro-mayor` | LibroMayor.razor | Libro Mayor |
| `/contabilidad/sumas-saldos` | SumasYSaldos.razor | Balance de Sumas y Saldos |

#### Módulo Compras (`/compras/`)
| Ruta | Página | Descripción |
|------|--------|-------------|
| `/compras/ordenes-pedido` | OrdenesPedido.razor | Gestión de Órdenes de Pedido |
| `/compras/ordenes-compra` | OrdenesCompra.razor | Gestión de Órdenes de Compra |
| `/compras/recepciones` | RecepcionesCompra.razor | Recepciones de mercadería |
| `/compras/expedientes` | ExpedientesImportacion.razor | Expedientes de importación |
| `/compras/importaciones` | Importaciones.razor | Hojas de importación (landed cost) |

#### Módulo Inventario (`/inventario/`)
| Ruta | Página | Descripción |
|------|--------|-------------|
| `/inventario/almacenes` | Almacenes.razor | Vista de almacenes (inventario) |
| `/inventario/movimientos` | Movimientos.razor | Movimientos de inventario |
| `/inventario/stock` | Stock.razor | Stock actual por producto/almacén |
| `/inventario/kardex` | Kardex.razor | Kardex por producto |

#### Módulo Ventas (`/ventas/`)
| Ruta | Página | Descripción |
|------|--------|-------------|
| `/ventas/pedidos` | Pedidos.razor | Pedidos de venta |
| `/ventas/facturas` | Facturas.razor | Facturas de venta |

---

## 23. Servicios en Background

### `PeriodoContableNotificadorService`
Servicio `IHostedService` (Background Service) que ejecuta diariamente a las **08:00 UTC**.

**Funcionalidad:**
1. Recorre todas las empresas activas en el sistema
2. Para cada empresa, invoca `IPeriodoContableNotificacionService.VerificarPeriodosProximosAsync`
3. Si hay períodos próximos a cerrar, emite notificaciones

**Configuración:**
- Hora de ejecución: 08:00 UTC (ajustable vía `HoraEjecucion`)
- Tolerancia de error: los errores de empresas individuales se loguean sin detener el servicio
- Resiliencia: maneja `OperationCanceledException` para apagado limpio

---

## 24. Capa de Infraestructura

### `AgoraHub360.ERP.Infrastructure`

Provee implementaciones de servicios externos:
- **Email**: servicio de envío de correos electrónicos (notificaciones de período)
- **Archivos**: gestión del sistema de archivos para documentos subidos

### `AgoraHub360.ERP.Persistence`

#### `AgoraDbContext`
DbContext principal con soporte multiempresa mediante **filtros globales de EF Core**:
- Todos los `DbSet<TenantEntity>` tienen filtro automático: `e.EmpresaId == _empresaId`
- El `_empresaId` se obtiene del `ICurrentUserService` (extraído del claim JWT)
- En modo design-time (migraciones), el filtro se omite

#### `AuditInterceptor`
Interceptor de SaveChanges que:
1. Captura entidades modificadas antes de guardar
2. Genera registros `AuditLog` con valores anteriores y nuevos en JSON
3. Se ejecuta automáticamente sin código adicional en los servicios

#### Repositorio genérico
```csharp
IRepository<T> where T : class
{
    GetByIdAsync(id)
    GetAllAsync()
    FindAsync(predicate)
    AddAsync(entity)
    Update(entity)
    Delete(entity)
    SaveChangesAsync()
}
```

---

## 25. Capa Shared — DTOs y Contratos

### `AgoraHub360.ERP.Shared`

Contiene todos los contratos de datos compartidos entre la API y el cliente Web.

#### DTOs por Módulo

| Carpeta | Descripción |
|---------|-------------|
| `DTOs/Empresa/` | EmpresaDto, CreateEmpresaDto, UpdateEmpresaDto |
| `DTOs/Usuario/` | UsuarioDto, CreateUsuarioDto, UpdateUsuarioDto, AsignarRolDto, ResetPasswordDto |
| `DTOs/Rol/` | RolDto, CreateRolDto |
| `DTOs/MDM/` | ProductDto2, CreateProductDto2, CompanyProductDto, BrandDto, etc. |
| `DTOs/Cliente/` | ClienteDto, CreateClienteDto, UpdateClienteDto |
| `DTOs/Proveedor/` | ProveedorDto, CreateProveedorDto, UpdateProveedorDto |
| `DTOs/Contabilidad/` | AsientoContableDto, CuentaContableDto, PeriodoContableDto, etc. |
| `DTOs/Contabilidad/Importacion/` | ImportResultDto, ImportValidacionDto (importación masiva) |
| `DTOs/Compras/` | OrdenCompraDto, OrdenPedidoDto, RecepcionCompraDto, etc. |
| `DTOs/Inventario/` | MovimientoInventarioDto, StockProductoDto, KardexDto |
| `DTOs/Logistica/` | HojaRutaDto, CreateHojaRutaDto |
| `DTOs/Bancario/` | ConciliacionBancariaDto, ExtractoBancarioDto, SugerenciaConciliacionDto |
| `DTOs/ActivosFijos/` | ActivoFijoDto, DepreciacionMensualDto |
| `DTOs/Tributario/` | RegistroImpuestoDto |
| `DTOs/PRC/` | PriceListDto, PriceListItemDto |
| `DTOs/RUL/` | IndustryDto |
| `DTOs/VER/` | EntityVersionDto |
| `DTOs/Workflow/` | TareaDto, CreateTareaDto, UpdateTareaDto, PlantillaTareaDto |
| `DTOs/DOC/` | DocumentDto, ComprobanteDocumentoDto, AdjuntarDocumentoDto |
| `DTOs/Parametro/` | ParametroSistemaDto, CreateParametroDto |
| `DTOs/Numeracion/` | NumeracionDocumentoDto |
| `DTOs/Notificacion/` | NotificacionDto |
| `DTOs/Presupuestos/` | PresupuestoContableDto |

#### `ApiResponse<T>` — Envelope de respuesta estándar
```csharp
class ApiResponse<T>
{
    bool Success;
    string? Message;
    T? Data;
    string? Error;

    static ApiResponse<T> Ok(T data, string? msg = null)
    static ApiResponse<T> Fail(string error)
}
```

#### `Result<T>` — Pattern de resultado (Application Layer)
```csharp
class Result<T>
{
    bool IsSuccess;
    T? Value;
    string? Error;

    static Result<T> Success(T value)
    static Result<T> Failure(string error)
}
```

---

## 26. Pruebas Automatizadas

### Proyecto de Tests: `AgoraHub360.ERP.Tests`

Framework: **xUnit**

#### Tests de Dominio
| Archivo | Cobertura |
|---------|-----------|
| `Domain/AuditLogTests.cs` | Creación y campos del AuditLog |
| `Domain/AuditableEntityTests.cs` | Campos de auditoría automática |
| `Domain/EmpresaTests.cs` | Creación y propiedades de Empresa |
| `Domain/NumeracionDocumentoTests.cs` | GenerarSiguiente() — correlativo |
| `Domain/ParametroSistemaTests.cs` | Creación y validación de parámetros |
| `Domain/RolTests.cs` | Creación de roles |
| `Domain/TenantEntityTests.cs` | EmpresaId en TenantEntity |

#### Tests de Aplicación
| Archivo | Cobertura |
|---------|-----------|
| `Application/AuditLogDtoTests.cs` | Mapping de AuditLog a DTO |
| `Application/EmpresaServiceTests.cs` | CRUD de empresas con mock |
| `Application/NumeracionDocumentoServiceTests.cs` | Generación de correlativos |
| `Application/ParametroSistemaServiceTests.cs` | CRUD de parámetros |
| `Application/ResultTests.cs` | Comportamiento del Result<T> |
| `Application/RolServiceTests.cs` | CRUD de roles |

---

## 27. Configuración e Instalación

### Requisitos previos
- .NET 8 SDK
- SQL Server 2019+
- Visual Studio 2022/2026 o VS Code

### Paso 1: Clonar el repositorio
```bash
git clone https://github.com/abelcalvimontes/AgoraHUB360-ERP.git
```

### Paso 2: Configurar la cadena de conexión
Editar `src/AgoraHub360.ERP.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR,PUERTO;Database=TU_BD;User Id=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=true;MultipleActiveResultSets=true;Encrypt=false"
  },
  "JwtSettings": {
    "Secret": "TU_CLAVE_SECRETA_JWT_256_BITS",
    "Issuer": "AgoraHub360.ERP",
    "Audience": "AgoraHub360.ERP.Client",
    "ExpirationMinutes": 480
  }
}
```

### Paso 3: Ejecutar migraciones
```bash
dotnet ef database update --project src/AgoraHub360.ERP.Persistence --startup-project src/AgoraHub360.ERP.Api
```

### Paso 4: Iniciar el sistema

**Opción A — Script automático (recomendado):**
```powershell
.\start-system.ps1
```
El script:
- Detiene procesos previos
- Inicia la API (puerto 7001)
- Inicia la Web (puerto 5002)
- Abre el navegador en la pantalla de login
- Muestra las credenciales

**Opción B — Manual:**
```bash
# Terminal 1 - API
cd src\AgoraHub360.ERP.Api
dotnet run --launch-profile https

# Terminal 2 - Web
cd src\AgoraHub360.ERP.Web
dotnet run --launch-profile https
```

### URLs de acceso
| Servicio | URL |
|----------|-----|
| Aplicación Web | `https://localhost:5002` |
| Login | `https://localhost:5002/login` |
| API REST | `https://localhost:7001` |
| Swagger UI | `https://localhost:7001/swagger` |
| Health Check | `https://localhost:7001/health` |

### Credenciales por defecto
| Campo | Valor |
|-------|-------|
| Email | `admin@agorahub360.com` |
| Contraseña | `Admin123` |
| Rol | `Admin` |

> ⚠️ Cambiar las credenciales antes de pasar a producción.

### Resetear usuario administrador
```powershell
.\reset-admin-user.ps1
```

### Scripts de diagnóstico
| Script | Descripción |
|--------|-------------|
| `quick-db-test.ps1` | Test rápido de conexión a BD (sin API) |
| `test-database-connection.ps1` | Test completo con API corriendo |
| `test-mdm-endpoints.ps1` | Verificación de endpoints MDM |
| `verify-mdm-routes.ps1` | Verificar rutas MDM |
| `verify-admin-user.ps1` | Verificar existencia del usuario admin |

---

## 28. Multiempresa (Multi-Tenant)

El sistema implementa multiempresa desde el núcleo del diseño.

### Mecanismo de aislamiento

1. **Token JWT**: incluye el claim `EmpresaId` al autenticarse
2. **Middleware de Tenant**: extrae el `EmpresaId` del token y lo inyecta en el contexto
3. **`ICurrentUserService`**: provee `EmpresaId`, `UserId`, `UserName`, `Rol` al resto de los servicios
4. **Filtros globales EF Core**: todo query sobre `TenantEntity` incluye automáticamente `WHERE EmpresaId = @empresaId`
5. **Selección de empresa**: al login, el usuario puede seleccionar entre sus empresas asignadas

### Tabla `UsuarioEmpresa`
Un usuario puede pertenecer a múltiples empresas con diferentes roles:
```
Usuario 1 → Empresa A (Rol: Admin)
Usuario 1 → Empresa B (Rol: Operador)
Usuario 2 → Empresa A (Rol: Contador)
```

### Seed de datos por empresa
Cada empresa puede inicializar sus datos base (catálogos, UoMs, estados de producto) usando el endpoint `/empresas/{id}/seed`.

---

## 29. Auditoría Automática

### Interceptor de Auditoría
El `AuditInterceptor` se registra en el `AgoraDbContext` y captura automáticamente:
- **INSERT**: graba el JSON con todos los valores nuevos
- **UPDATE**: graba JSON con valores anteriores, valores nuevos y lista de campos modificados
- **DELETE**: graba el JSON con los últimos valores conocidos

Los registros se almacenan en la tabla `AuditLogs` con:
- Entidad y PK afectada
- Tipo de operación
- Usuario y empresa del contexto
- Timestamp UTC preciso

### Exclusiones
- La entidad `AuditLog` en sí no se audita (evita recursión)
- Entidades de configuración del sistema

---

## 30. Enumeraciones del Dominio

### `EstadoDocumento`
| Valor | Código | Descripción |
|-------|--------|-------------|
| 1 | Borrador | Estado inicial |
| 2 | Confirmado | Confirmado internamente |
| 3 | Aprobado | Aprobado por autoridad |
| 4 | Anulado | Anulado |
| 5 | RecepcionParcial | Recepción parcial de OC |
| 6 | Cerrado | Proceso cerrado |
| 10 | EnRevision | OP enviada a Compras Central |
| 11 | AbastecidoConStock | OP atendida con stock existente |
| 20 | PendienteAprobacion | OC pendiente de aprobación |
| 21 | EnviadaProveedor | OC enviada al proveedor |
| 22 | EnNegociacion | Proveedor en negociación |
| 23 | ConfirmadaProveedor | PI aceptada por proveedor |
| 24 | PagoProgramado | Pago(s) programados/ejecutados |
| 30 | EnTransito | Expediente en tránsito |
| 31 | Arribado | Mercadería arribada al puerto |
| 32 | EnAduana | En proceso aduanero |
| 33 | ObservacionAduana | Aforo/observación en aduana |
| 34 | Liberado | Levante obtenido |
| 35 | RecepcionConDiferencias | Recepción con faltantes/daños |
| 40 | Rechazado | Rechazado (aprobación denegada) |

### `TipoCuenta`
| Valor | Nombre | Descripción |
|-------|--------|-------------|
| 1 | Activo | Activos del balance |
| 2 | Pasivo | Pasivos del balance |
| 3 | Patrimonio | Patrimonio neto |
| 4 | Ingreso | Ingresos del estado de resultados |
| 5 | Gasto | Gastos del estado de resultados |
| 6 | Costo | Costo de ventas |

### `NaturalezaCuenta`
| Valor | Nombre |
|-------|--------|
| 1 | Deudora |
| 2 | Acreedora |

### `NivelUrgencia`
| Valor | Nombre |
|-------|--------|
| 1 | Normal |
| 2 | Urgente |
| 3 | Crítico |

### `TipoProducto`
| Valor | Nombre |
|-------|--------|
| 1 | MateriaPrima |
| 2 | ProductoTerminado |
| 3 | Servicio |

### `ClasificacionFlujoEfectivo`
| Valor | Nombre |
|-------|--------|
| 0 | NoAplica |
| 1 | Operacional |
| 2 | Inversion |
| 3 | Financiacion |

---

## 31. Roadmap y Estado del Proyecto

### Estado Actual (rama `main`, Abril 2026)

| Módulo | Estado | Notas |
|--------|--------|-------|
| CORE (Seguridad, Roles, Empresas) | ✅ Completo | Multiempresa, JWT, RBAC |
| CORE (Parámetros, Numeraciones) | ✅ Completo | Configuración flexible |
| CORE (Auditoría) | ✅ Completo | Interceptor automático |
| MDM (Clientes, Proveedores) | ✅ Completo | CRUD full |
| MDM (Almacenes, UoM) | ✅ Completo | CRUD full |
| MDM (Productos clásicos) | ✅ Completo | Modelo simplificado |
| MDM (Productos avanzado v2) | ✅ Completo | Catálogo, Brand, Variantes, Atributos |
| MDM (Listas de Precios) | ✅ Completo | Por canal y moneda |
| ACC (Plan de Cuentas) | ✅ Completo | Jerárquico N-niveles |
| ACC (Asientos Contables) | ✅ Completo | Partida doble, exportación múltiple |
| ACC (Importación masiva Excel) | ✅ Completo | Validación + importación |
| ACC (Períodos Contables) | ✅ Completo | Con notificaciones automáticas |
| ACC (Cierre Contable) | ✅ Completo | Mensual y anual |
| ACC (Estados Financieros) | ✅ Completo | BG, ER, FE, Ratios |
| ACC (Presupuesto) | ✅ Completo | Con ciclo de aprobación |
| CMP (Orden de Pedido) | ✅ Completo | Flujo completo |
| CMP (Orden de Compra) | ✅ Completo | Flujo con aprobación y pagos |
| CMP (Expediente Importación) | ✅ Completo | Tracking completo |
| CMP (Recepción Mercadería) | ✅ Completo | Con control de diferencias |
| CMP (Hoja Importación/Landed Cost) | ✅ Completo | Prorrateo automático |
| INV (Movimientos/Kardex) | ✅ Completo | WAC automático |
| INV (Stock multi-almacén) | ✅ Completo | Actualización automática |
| LOG (Hojas de Ruta) | ✅ Completo | INTERNA y CLIENTE |
| BNC (Conciliación Bancaria) | ✅ Completo | Importación extracto + match |
| ACT (Activos Fijos) | ✅ Completo | Depreciación DS 24051 |
| TRB (Tributario) | ✅ Completo | IVA, IT, IUE, RC-IVA |
| PRC (Listas de Precios) | ✅ Completo | Por canal, moneda, vigencia |
| CST (Centros de Costo) | ✅ Completo | Árbol jerárquico |
| CST (Reglas de Costeo) | ✅ Completo | WAC/FIFO/LIFO/Estándar |
| RUL (Industrias) | ✅ Completo | Catálogo + reglas |
| VER (Versionado) | ✅ Completo | Snapshots históricos |
| WF (Workflow) | ✅ Completo | Tareas polimórficas |
| DOC (Documentos Adjuntos) | ✅ Completo | Upload + vínculo comprobantes |
| Ventas | 🚧 Parcial | Páginas Blazor skeleton |
| Facturación Electrónica SIN | ⏳ Planificado | v1.1 |

### Roadmap

| Versión | Alcance | Estado |
|---------|---------|--------|
| **v1.0 MVP** | CORE + MDM + ACC + CMP + INV + LOG | ✅ En producción |
| **v1.1** | Facturación electrónica SIN + Ventas completo | ⏳ Planificado |
| **v1.2** | BI básico + dashboards analíticos | ⏳ Planificado |
| **v2.0** | MRP + Producción | ⏳ Futuro |
| **v2.x** | Automatización avanzada + Integraciones externas | ⏳ Futuro |

### Metodología de Desarrollo — Modelo Ágora (MAPE)
1. **Diagnóstico** — Análisis de requerimientos
2. **Diseño Arquitectónico** — Clean Architecture + DDD
3. **Desarrollo por Capas** — Domain → Application → Persistence → API → Web
4. **Validación Controlada** — Tests + revisión de estructura
5. **Implementación Guiada** — Deploy con monitoreo
6. **Evolución Continua** — Roadmap incremental

> **Principio fundamental:** No se libera ningún módulo sin validación estructural completa.

---

*Documentación generada automáticamente a partir del análisis del código fuente de la rama `main`.*  
*Proyecto: AgoraHUB 360 – ERP | Dirección Ágora Tech*
