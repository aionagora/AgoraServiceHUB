# AgoraHub360 ERP — Documentación de Estado del Proyecto

> **Rama activa:** `GestionImportacion`  
> **Stack:** .NET 8 · C# 12 · Blazor WebAssembly · ASP.NET Core API · Entity Framework Core · SQL Server  
> **Arquitectura:** Clean Architecture (Domain ? Application ? Infrastructure/Persistence ? API ? Web)  
> **Fecha de documento:** generado automáticamente desde el estado actual del workspace

---

## Índice

1. [Arquitectura General](#1-arquitectura-general)
2. [Proyectos de la Solución](#2-proyectos-de-la-solución)
3. [Modelo de Dominio y Tablas de Base de Datos](#3-modelo-de-dominio-y-tablas-de-base-de-datos)
4. [Capa Application — Servicios e Interfaces](#4-capa-application--servicios-e-interfaces)
5. [Capa API — Controladores y Endpoints](#5-capa-api--controladores-y-endpoints)
6. [Capa Shared — DTOs](#6-capa-shared--dtos)
7. [Capa Web — Blazor WASM](#7-capa-web--blazor-wasm)
8. [Persistencia — Migraciones y Configuraciones](#8-persistencia--migraciones-y-configuraciones)
9. [Patrones Transversales](#9-patrones-transversales)
10. [Estado Funcional por Módulo](#10-estado-funcional-por-módulo)
11. [Pendientes y Próximos Pasos](#11-pendientes-y-próximos-pasos)

---

## 1. Arquitectura General

```
???????????????????????????????????????????????????????????????????????
?                        AgoraHub360 ERP                              ?
?                                                                     ?
?  ???????????????     HTTP/REST      ???????????????????????????    ?
?  ?   Blazor    ? ?????????????????? ?   ASP.NET Core API      ?    ?
?  ?   WASM      ?   JWT Bearer       ?   (v1, Versionada)      ?    ?
?  ?   (Web)     ?                    ???????????????????????????    ?
?  ???????????????                                 ?                  ?
?                                       ????????????????????????     ?
?                                       ?  Application Layer   ?     ?
?                                       ?  (Services, IFaces)  ?     ?
?                                       ????????????????????????     ?
?                                    ????????????????????????????    ?
?                          ???????????   Domain Layer           ?    ?
?                          ?         ?   (Entities, Interfaces) ?    ?
?                          ?         ????????????????????????????    ?
?              ????????????????????  ??????????????????????????     ?
?              ?   Persistence    ?  ?   Infrastructure       ?     ?
?              ?  (EF Core, SQL)  ?  ?  (Email, Storage, etc) ?     ?
?              ????????????????????  ??????????????????????????     ?
?                          ?                                          ?
?                    ??????????????                                   ?
?                    ? SQL Server ?                                   ?
?                    ??????????????                                   ?
???????????????????????????????????????????????????????????????????????
```

### Principios de diseño

| Principio | Implementación |
|-----------|---------------|
| **Multi-tenant** | `TenantEntity` con `EmpresaId`. Query filters globales en EF Core aíslan datos por empresa |
| **Auditoría automática** | `AuditableEntity` + `AuditableEntityInterceptor` registra FechaCreacion/Modificacion/Usuario |
| **Versionado de entidades** | `EntityVersioningInterceptor` genera snapshots JSON en tabla `EntityVersions` |
| **Clean Architecture** | Domain no depende de nada; Application solo de Domain; Persistence/API solo de Application |
| **API Versionada** | URL Segment (`/api/v1/`) + Header (`X-Api-Version`) via `Asp.Versioning` |
| **Autenticación** | JWT Bearer + StubAuthHandler para desarrollo |

---

## 2. Proyectos de la Solución

| Proyecto | Tipo | Descripción |
|----------|------|-------------|
| `AgoraHub360.ERP.Domain` | Class Library .NET 8 | Entidades, interfaces de repositorio, enums, clases base |
| `AgoraHub360.ERP.Application` | Class Library .NET 8 | Servicios de aplicación, interfaces, Result pattern |
| `AgoraHub360.ERP.Persistence` | Class Library .NET 8 | EF Core DbContext, Repositorios, Migraciones, Interceptores |
| `AgoraHub360.ERP.Infrastructure` | Class Library .NET 8 | Servicios externos (email, storage, etc.) |
| `AgoraHub360.ERP.Api` | ASP.NET Core Web API .NET 8 | Controllers REST versionados, Middleware, Auth, Swagger |
| `AgoraHub360.ERP.Shared` | Class Library .NET 8 | DTOs compartidos entre API y Web |
| `AgoraHub360.ERP.Web` | Blazor WebAssembly .NET 8 | SPA Frontend, HTTP Services, Páginas Razor |
| `AgoraHub360.ERP.Tests` | xUnit .NET 8 | Pruebas unitarias/integración |

### Dependencias entre proyectos

```
Domain  ???  Application  ???  Persistence
                    ?                ?
                    ??????? Api ??????
                              ?
                           Shared
                              ?
                            Web
```

---

## 3. Modelo de Dominio y Tablas de Base de Datos

### 3.1 Clases Base

#### `AuditableEntity` — Todas las entidades
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `FechaCreacion` | `DateTime` | Fecha de creación (auto) |
| `CreadoPor` | `string?` | Usuario creador (auto) |
| `FechaModificacion` | `DateTime?` | Fecha última modificación (auto) |
| `ModificadoPor` | `string?` | Usuario que modificó (auto) |
| `Activo` | `bool` | Soft-delete lógico |

#### `TenantEntity : AuditableEntity` — Entidades multi-empresa
| Campo | Tipo | Descripción |
|-------|------|-------------|
| Todos los de `AuditableEntity` | — | — |
| `EmpresaId` | `int` | FK a Empresa (filtro global automático) |

---

### 3.2 Módulo CORE

#### `Empresas`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `Nombre` | `string(200)` | Nombre legal |
| `NIT` | `string?` | NIT/RUC fiscal |
| `Direccion` | `string?` | Dirección |
| `Telefono` | `string?` | Teléfono |
| `Email` | `string?` | Email corporativo |
| `MonedaBaseId` | `string?` | FK ? Monedas |
| + campos AuditableEntity | | |

#### `Usuarios`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `NombreUsuario` | `string` | Login único |
| `Email` | `string` | Email único |
| `PasswordHash` | `string` | Hash BCrypt |
| `NombreCompleto` | `string` | Nombre display |
| `EmpresaActivaId` | `int?` | Empresa activa en sesión |
| + campos AuditableEntity | | |

#### `UsuarioEmpresas` (N:N)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `UsuarioId` | `int` PK,FK | FK ? Usuarios |
| `EmpresaId` | `int` PK,FK | FK ? Empresas |
| `RolId` | `int` | FK ? Roles |
| `EsAdministrador` | `bool` | Flag admin |

#### `Roles`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `Nombre` | `string(100)` | Nombre del rol |
| `Descripcion` | `string?` | Descripción |
| + campos AuditableEntity | | |

#### `ParametrosSistema` (TenantEntity)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Clave` | `string(100)` | Clave única por empresa |
| `Valor` | `string(500)` | Valor |
| `Descripcion` | `string?` | Descripción |
| `Categoria` | `string(50)` | Agrupador UI |
| `TipoDato` | `string(20)` | String/Integer/Decimal/Boolean/Select |

#### `NumeracionesDocumento` (TenantEntity)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `TipoDocumento` | `string` | "OC", "REC", "FAC", "PED", "NC" |
| `Descripcion` | `string` | Nombre legible |
| `Prefijo` | `string` | "OC-", "FAC-" |
| `SiguienteNumero` | `int` | Correlativo |
| `Digitos` | `int` | Padding ceros |

#### `Monedas`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `string(3)` PK | Código ISO (BOB, USD, EUR) |
| `Nombre` | `string` | Nombre |
| `Simbolo` | `string` | Símbolo (Bs., $) |
| `TasaCambio` | `decimal` | Tasa respecto a moneda base |

#### `AuditLogs`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `long` PK | Identificador |
| `Entidad` | `string` | Nombre de la entidad |
| `EntidadId` | `string` | PK afectada |
| `Accion` | `string` | Insert / Update / Delete |
| `ValoresAnteriores` | `string? JSON` | Estado previo |
| `ValoresNuevos` | `string? JSON` | Estado nuevo |
| `CamposModificados` | `string?` | Lista de campos |
| `EmpresaId` | `int?` | Contexto empresa |
| `Usuario` | `string?` | Usuario responsable |
| `FechaHora` | `DateTime` | UTC |

---

### 3.3 Módulo MDM — Master Data Management

#### `Catalogs` (Global)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `CatalogId` | `long` PK | Identificador |
| `Name` | `string(100)` | Nombre de la catálogo |
| `Description` | `string?` | Descripción |
| + campos AuditableEntity | | |

#### `Brands` (Global)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `BrandId` | `long` PK | Identificador |
| `Name` | `string(100)` | Nombre de la marca |
| `Country` | `string?` | País de origen |
| `Website` | `string?` | Sitio web |

#### `Manufacturers` (Global)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ManufacturerId` | `long` PK | Identificador |
| `Name` | `string(200)` | Nombre del fabricante |
| `Country` | `string?` | País |
| `Website` | `string?` | Sitio web |
| `ContactEmail` | `string?` | Email de contacto |

#### `Uoms` — Unidades de Medida (Global)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `UomId` | `int` PK | Identificador |
| `Code` | `string(20)` | Código (UND, KG, LT, CJ) |
| `Name` | `string(100)` | Nombre |
| `Category` | `string?` | Masa / Volumen / Unidades |

#### `ProductStatuses` — Ciclo de Vida (Global Seed)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `StatusId` | `int` PK | 1=Borrador, 2=Activo, 3=Bloqueado, 4=Descontinuado |
| `Code` | `string` | "Draft", "Active", "Blocked", "Discontinued" |
| `Name` | `string` | Nombre display |

#### `Products` — Producto Global (No tenant)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ProductId` | `long` PK | Identificador |
| `CatalogId` | `long` FK | Catálogo |
| `ProductKind` | `byte` | 1=Bien, 2=Servicio, 3=Kit, 4=MateriaPrima, 5=Embalaje |
| `GenericName` | `string(300)` | Nombre genérico (DCI) |
| `CommercialName` | `string(300)` | Nombre comercial |
| `ShortDescription` | `string?` | Descripción corta |
| `LongDescription` | `string?` | Descripción larga |
| `BrandId` | `long?` FK | Marca |
| `ManufacturerId` | `long?` FK | Fabricante |
| `DefaultUomId` | `int` FK | UdM base |
| `IsStockable` | `bool` | Controla stock |
| `IsSellable` | `bool` | Se puede vender |
| `IsPurchasable` | `bool` | Se puede comprar |
| `LifecycleStatusId` | `int` FK | Estado de ciclo |
| + campos AuditableEntity | | |

**Relaciones de Product:**
- 1:N ? `CompanyProducts` (activación por empresa)
- 1:N ? `ProductCodes` (códigos de barras, SKU, etc.)
- N:N ? `Categories` (vía `ProductCategories`)
- 1:N ? `ProductAttributes` (atributos EAV)
- 1:N ? `ProductUoms` (UdM alternativas)
- 1:N ? `ProductVariants` (SKUs hijos)
- N:N ? `ProductClassifications` (vía `ProductClassificationLinks`)

#### `CompanyProducts` — Extensión por Empresa (TenantEntity)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `CompanyProductId` | `long` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `ProductId` | `long` FK | Producto global |
| `Sku` | `string(100)` | SKU propio empresa |
| `CodigoInterno` | `string?` | Código interno |
| `ImpuestoProfileId` | `int?` | Perfil impuesto |
| `MonedaBaseId` | `string?` | Moneda |
| `IsVisiblePOS` | `bool` | Canal POS |
| `IsVisibleEcommerce` | `bool` | Canal Ecommerce |
| `IsVisibleB2B` | `bool` | Canal B2B |
| `AllowReturns` | `bool` | Permite devoluciones |
| `WarrantyDays` | `int?` | Días de garantía |
| `MinStock` | `decimal?` | Stock mínimo |
| `MaxStock` | `decimal?` | Stock máximo |
| `ReorderPoint` | `decimal?` | Punto de reorden |
| `CostingMethod` | `byte?` | Override costeo. 1=Promedio, 2=FIFO, 3=Estándar |
| + campos TenantEntity | | |

#### `CompanyProductFeatures` — Features Activables
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `EmpresaId` | `int` PK,FK | Empresa |
| `CompanyProductId` | `long` PK,FK | CompanyProduct |
| `FeatureCode` | `string` PK | LOT, SERIAL, FEFO, HAZMAT, QC, BOM, EXPIRY, ALLERGEN |
| `IsEnabled` | `bool` | Activado |

#### `ProductCodes` — Multi-Identificadores
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ProductCodeId` | `long` PK | Identificador |
| `EmpresaId` | `int?` | Null = global |
| `ProductId` | `long` FK | Producto |
| `CodeType` | `byte` | 1=SKU, 2=Barra, 3=QR, 4=ExtProveedor, 5=ExtCliente, 6=Canal, 7=InternoAlt |
| `Valor` | `string(200)` | Valor del código |
| `ProviderId` | `long?` | Proveedor asociado |
| `CustomerId` | `long?` | Cliente asociado |
| `ChannelId` | `int?` | Canal |
| `ValidFrom` | `DateOnly?` | Vigencia desde |
| `ValidTo` | `DateOnly?` | Vigencia hasta |
| `IsPrimary` | `bool` | Código principal |
| `IsActive` | `bool` | Activo |

#### `ProductUoms` — UdM Alternativas
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ProductUomId` | `long` PK | Identificador |
| `ProductId` | `long` FK | Producto |
| `UomId` | `int` FK | Unidad de medida |
| `IsBase` | `bool` | Es UdM base |
| `FactorToBase` | `decimal` | Factor conversión (1 CJ = 12 UND ? 12) |
| `Barcode` | `string?` | Código de barras propio |

#### `ProductVariants` — SKUs Hijos (TenantEntity)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `VariantId` | `long` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `ParentProductId` | `long` FK | Producto padre |
| `Sku` | `string(100)` | SKU variante |
| `Barcode` | `string?` | Código de barras |
| `VariantName` | `string?` | "Talla M / Color Negro" |

#### `VariantAttributeValues` — Valores de Ejes de Variante
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `VariantId` | `long` PK,FK | Variante |
| `AttributeId` | `long` PK,FK | Atributo (eje) |
| `OptionId` | `long?` | Opción seleccionada |
| `ValueString` | `string?` | Valor libre |

#### `AttributeDefinitions` — Atributos Dinámicos por Industria
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `AttributeId` | `long` PK | Identificador |
| `IndustryId` | `int` FK | Industria |
| `Code` | `string(50)` | Código único por industria |
| `Name` | `string(100)` | Nombre |
| `DataType` | `byte` | 1=String, 2=Int, 3=Decimal, 4=Bool, 5=Date, 6=Json, 7=Select |
| `IsRequired` | `bool` | Obligatorio |
| `IsSearchable` | `bool` | Indexable en búsqueda |
| `IsVariantAxis` | `bool` | Eje de variante (color, talla) |
| `ValidationRegex` | `string?` | Regex de validación |
| `MinValue` | `decimal?` | Mínimo numérico |
| `MaxValue` | `decimal?` | Máximo numérico |
| `UnitHint` | `string?` | Hint de unidad (kg, cm) |

#### `AttributeOptions` — Opciones de Select
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `OptionId` | `long` PK | Identificador |
| `AttributeId` | `long` FK | Atributo padre |
| `Value` | `string(200)` | Valor opción |
| `SortOrder` | `int` | Orden |

#### `ProductAttributes` — Valores EAV por Producto
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ProductAttributeId` | `long` PK | Identificador |
| `ProductId` | `long` FK | Producto |
| `AttributeId` | `long` FK | Definición del atributo |
| `ValueString` | `string?` | Valor texto |
| `ValueDecimal` | `decimal?` | Valor decimal |
| `ValueInt` | `int?` | Valor entero |
| `ValueBool` | `bool?` | Valor booleano |
| `ValueDate` | `DateOnly?` | Valor fecha |
| `ValueJson` | `string?` | Valor JSON |
| `OptionId` | `long?` | Opción seleccionada |
| `ValidFrom` | `DateOnly?` | Vigencia desde |
| `ValidTo` | `DateOnly?` | Vigencia hasta |

#### `Categories` — Categorías Jerárquicas
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `CategoryId` | `long` PK | Identificador |
| `CatalogId` | `long` FK | Catálogo |
| `ParentCategoryId` | `long?` FK | Padre (self-ref, N niveles) |
| `Name` | `string(200)` | Nombre |
| `Path` | `string?` | Ruta materializada "/Electronics/Phones" |
| `SortOrder` | `int` | Orden |

#### `ProductCategories` (N:N)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ProductId` | `long` PK,FK | Producto |
| `CategoryId` | `long` PK,FK | Categoría |

#### `ProductClassifications` — Clasificaciones Adicionales
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ClassificationId` | `long` PK | Identificador |
| `CatalogId` | `long` FK | Catálogo |
| `Type` | `string` | Tipo de clasificación |
| `Name` | `string` | Nombre |

#### `ProductClassificationLinks` (N:N)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ProductId` | `long` PK,FK | Producto |
| `ClassificationId` | `long` PK,FK | Clasificación |

#### `Clientes` (TenantEntity)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Codigo` | `string(50)` | Código cliente |
| `RazonSocial` | `string(200)` | Razón social |
| `NIT` | `string?` | NIT/RUC |
| `Direccion` | `string?` | Dirección |
| `Telefono` | `string?` | Teléfono |
| `Email` | `string?` | Email |
| `NombreContacto` | `string?` | Contacto |
| `TipoCliente` | `string(50)` | General, VIP, etc. |

#### `Proveedores` (TenantEntity)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Codigo` | `string(50)` | Código proveedor |
| `RazonSocial` | `string(200)` | Razón social |
| `NIT` | `string?` | NIT/RUC |
| `Direccion` | `string?` | Dirección |
| `Telefono` | `string?` | Teléfono |
| `Email` | `string?` | Email |
| `NombreContacto` | `string?` | Contacto |
| `TipoProveedor` | `string(50)` | Local, Internacional |
| `Pais` | `string?` | País |
| `CondicionPago` | `string?` | Condición de pago |

#### `Almacenes` (TenantEntity)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Codigo` | `string(20)` | Código |
| `Nombre` | `string(200)` | Nombre |
| `Direccion` | `string?` | Dirección |
| `Responsable` | `string?` | Responsable |
| `Telefono` | `string?` | Teléfono |

#### `UbicacionesAlmacen`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `AlmacenId` | `int` FK | Almacén |
| `Codigo` | `string` | Código ubicación (A1-B2-C3) |
| `Nombre` | `string?` | Nombre legible |

---

### 3.4 Módulo INV — Inventario

#### `MovimientosInventario` (TenantEntity)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Number` | `string` | Número único "MOV-2026-00001" |
| `MovementType` | `string` | Receipt / Issue / Adjustment / Transfer |
| `MovementDate` | `DateTime` | Fecha del movimiento |
| `CompanyProductId` | `long` FK | Producto empresa |
| `WarehouseId` | `int` FK | Almacén origen |
| `DestinationWarehouseId` | `int?` FK | Almacén destino (Transfer) |
| `Quantity` | `decimal` | Cantidad (siempre positivo) |
| `UnitCost` | `decimal` | Costo unitario |
| `TotalCost` | `decimal` | Costo total |
| `Reference` | `string?` | Documento origen (OC-001) |
| `Notes` | `string?` | Notas |

#### `StockProductos` (TenantEntity)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `CompanyProductId` | `long` FK | Producto empresa |
| `AlmacenId` | `int` FK | Almacén |
| `CurrentStock` | `decimal` | Stock disponible actual |
| `AverageCost` | `decimal` | Costo promedio ponderado (WAC) |
| `LastUpdated` | `DateTime` | Última actualización |

---

### 3.5 Módulo PRC — Precios

#### `PriceLists` (TenantEntity)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `PriceListId` | `long` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Code` | `string(50)` | Código |
| `Name` | `string(200)` | Nombre |
| `CurrencyId` | `string(3)` | Moneda |
| `ChannelId` | `int?` | Canal |
| `ValidFrom` | `DateOnly?` | Vigencia desde |
| `ValidTo` | `DateOnly?` | Vigencia hasta |
| `IsDefault` | `bool` | Lista por defecto |

#### `PriceListItems`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ItemId` | `long` PK | Identificador |
| `PriceListId` | `long` FK | Lista de precios |
| `CompanyProductId` | `long?` FK | Por CompanyProduct (XOR) |
| `VariantId` | `long?` FK | Por Variante (XOR) |
| `Price` | `decimal` | Precio |
| `MinQty` | `decimal?` | Cantidad mínima (precio por volumen) |
| `DiscountPercent` | `decimal?` | Descuento % |
| `ValidFrom` | `DateOnly?` | Vigencia desde |
| `ValidTo` | `DateOnly?` | Vigencia hasta |

---

### 3.6 Módulo CST — Costeo

#### `CostingRules`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `EmpresaId` | `int` PK | Empresa |
| `ProductKind` | `byte` PK | Tipo de producto |
| `DefaultMethod` | `byte` | 1=Promedio WAC, 2=FIFO, 3=Estándar |

#### `LandedCostProfiles` (TenantEntity)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ProfileId` | `long` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Name` | `string` | Nombre |
| `AllocationMethod` | `byte` | 1=Peso, 2=Volumen, 3=Valor, 4=Unidades |
| `IsDefault` | `bool` | Default |

---

### 3.7 Módulo RUL — Reglas

#### `Industries` — Industrias/Segmentos
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IndustryId` | `int` PK | Identificador |
| `Code` | `string` | "PHARMA", "HARDWARE", "FOOD" |
| `Name` | `string` | Nombre |

#### `ProductIndustryRules`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `RuleId` | `long` PK | Identificador |
| `IndustryId` | `int` FK | Industria |
| `ConditionJson` | `string JSON` | Condición evaluable |
| `ActionsJson` | `string JSON` | Features a habilitar |
| `Priority` | `int` | Prioridad |

---

### 3.8 Módulo VER — Versionado

#### `EntityVersions`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `VersionId` | `long` PK | Identificador |
| `EntityName` | `string` | Nombre de la entidad |
| `EntityId` | `string` | PK serializada |
| `VersionNo` | `int` | Número de versión |
| `ChangeType` | `byte` | 1=Create, 2=Update, 3=Delete |
| `SnapshotJson` | `string JSON` | Estado completo serializado |
| `ChangedBy` | `string?` | Usuario |
| `ChangedAt` | `DateTime` | Fecha UTC |
| `EmpresaId` | `int?` | Empresa contexto |

---

### 3.9 Módulo DOC — Documentos Multimedia

#### `Documents` (TenantEntity)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `DocumentId` | `long` PK | Identificador |
| `StorageProvider` | `byte` | 1=DB, 2=FileSystem, 3=S3, 4=Azure |
| `FileName` | `string` | Nombre del archivo |
| `MimeType` | `string` | MIME type |
| `Url` | `string?` | URL de acceso |
| `Hash` | `string?` | Hash de integridad |
| `SizeBytes` | `long` | Tamaño en bytes |

#### `ProductDocuments` (N:N)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ProductId` | `long` PK,FK | Producto |
| `DocumentId` | `long` PK,FK | Documento |
| `DocType` | `string?` | Ficha técnica, imagen, certificado |

---

### 3.10 Módulo CMP — Compras

#### `OrdenesCompra` (TenantEntity)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `OrdenCompraId` | `long` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Numero` | `string(30)` | Número único "OC-000001" |
| `FechaEmision` | `DateTime` | Fecha de emisión |
| `FechaEntregaEstimada` | `DateTime?` | Fecha estimada de entrega |
| `ProveedorId` | `int` FK | Proveedor |
| `AlmacenDestinoId` | `int` FK | Almacén destino |
| `MonedaId` | `string(3)` | Moneda (ISO 4217) |
| `TasaCambio` | `decimal` | Tasa de cambio |
| `Estado` | `byte` (enum) | 1=Borrador, 2=Confirmado, 3=Aprobado, 4=Anulado |
| `CondicionPago` | `string?` | Condición de pago |
| `Observaciones` | `string?` | Observaciones |
| `ReferenciaExterna` | `string?` | Nro. cotización proveedor |
| `Subtotal` | `decimal` | Suma subtotales líneas |
| `Descuento` | `decimal` | Suma descuentos líneas |
| `Impuesto` | `decimal` | Suma impuestos líneas |
| `Total` | `decimal` | Subtotal - Descuento + Impuesto |

#### `OrdenCompraLineas`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `OrdenCompraLineaId` | `long` PK | Identificador |
| `OrdenCompraId` | `long` FK | Orden de compra |
| `NumeroLinea` | `int` | Número secuencial |
| `CompanyProductId` | `long` FK | Producto empresa |
| `Descripcion` | `string(300)` | Descripción libre |
| `UnidadMedida` | `string(20)` | UdM de compra |
| `Cantidad` | `decimal` | Cantidad solicitada |
| `PrecioUnitario` | `decimal` | Precio unitario |
| `PorcentajeDescuento` | `decimal` | Descuento % (0–100) |
| `MontoDescuento` | `decimal` | Monto descuento calculado |
| `Subtotal` | `decimal` | (Cant × P/U) - Descuento |
| `PorcentajeImpuesto` | `decimal` | Impuesto % |
| `MontoImpuesto` | `decimal` | Monto impuesto calculado |
| `TotalLinea` | `decimal` | Subtotal + Impuesto |
| `CantidadRecepcionada` | `decimal` | Cantidad ya recibida |

---

### Mapa completo de tablas

```
CORE:        Empresas · Usuarios · UsuarioEmpresas · Roles · Monedas
             ParametrosSistema · NumeracionesDocumento · AuditLogs

MDM:         Catalogs · Brands · Manufacturers · Uoms · ProductStatuses · Products
             CompanyProducts · CompanyProductFeatures
             ProductCodes · ProductUoms · ProductVariants · VariantAttributeValues
             AttributeDefinitions · AttributeOptions · ProductAttributes
             Categories · ProductCategories
             ProductClassifications · ProductClassificationLinks
             Clientes · Proveedores · Almacenes · UbicacionesAlmacen

INV:         MovimientosInventario · StockProductos

PRC:         PriceLists · PriceListItems

CST:         CostingRules · LandedCostProfiles

RUL:         Industries · ProductIndustryRules

VER:         EntityVersions

DOC:         Documents · ProductDocuments

CMP:         OrdenesCompra · OrdenCompraLineas
```

**Total de tablas: 45**

---

## 4. Capa Application — Servicios e Interfaces

### 4.1 Patrón Result

```csharp
Result<T>
  .Success(value)   // IsSuccess = true, Value = T
  .Failure(error)   // IsSuccess = false, Error = string
```

### 4.2 Servicios registrados

| Interface | Implementación | Descripción |
|-----------|---------------|-------------|
| `IAuthService` | `AuthService` (Api) | Login/JWT |
| `IEmpresaService` | `EmpresaService` | CRUD empresas |
| `IUsuarioService` | `UsuarioService` | CRUD usuarios, roles |
| `IRolService` | `RolService` | CRUD roles |
| `IAuditLogService` | `AuditLogService` (Persistence) | Consulta auditoria paginada |
| `IParametroSistemaService` | `ParametroSistemaService` | CRUD parámetros |
| `INumeracionDocumentoService` | `NumeracionDocumentoService` | Gestión numeración |
| `IProductoService` (legacy) | `ProductoService` | Wrapper legacy |
| `IProductService` (MDM) | `ProductService` | CRUD producto global |
| `ICatalogService` | `CatalogService` | CRUD catálogos |
| `IBrandService` | `BrandService` | CRUD marcas |
| `IManufacturerService` | `ManufacturerService` | CRUD fabricantes |
| `IUomService` | `UomService` | CRUD UdM globales |
| `IUnidadMedidaService` (legacy) | `UnidadMedidaService` | Wrapper legacy |
| `IProductUomService` | `ProductUomService` | UdM alternativas producto |
| `IProductCodeService` | `ProductCodeService` | Códigos producto |
| `ICompanyProductService` | `CompanyProductService` | Activación empresa |
| `IProductVariantService` | `ProductVariantService` | Variantes SKU |
| `IAttributeDefinitionService` | `AttributeDefinitionService` | Atributos dinámicos + Product Attributes |
| `ICategoryService` | `CategoryService` | Categorías jerárquicas |
| `ICategoriaProductoService` (legacy) | `CategoriaProductoService` | Wrapper legacy |
| `IClienteService` | `ClienteService` | CRUD clientes |
| `IProveedorService` | `ProveedorService` | CRUD proveedores |
| `IAlmacenService` | `AlmacenService` | CRUD almacenes |
| `IMovimientoInventarioService` | `MovimientoInventarioService` | Movimientos INV + Stock |
| `IPriceListService` | `PriceListService` | Listas de precios |
| `IIndustryService` | `IndustryService` | Industrias/Reglas |
| `IVersioningService` | `VersioningService` | Consulta versiones |
| `ICurrentUserService` | `CurrentUserService` (Api) | Contexto usuario/empresa |
| `IOrdenCompraService` | `OrdenCompraService` | CRUD órdenes de compra |

---

## 5. Capa API — Controladores y Endpoints

### Base URL: `/api/v1/`

| Controlador | Ruta | Endpoints principales |
|-------------|------|-----------------------|
| `AuthController` | `/auth` | `POST /login` |
| `EmpresasController` | `/empresas` | GET all, GET id, POST, PUT, DELETE |
| `UsuariosController` | `/usuarios` | GET all, GET id, POST, PUT, DELETE, POST /{id}/roles |
| `RolesController` | `/roles` | GET all, GET id, POST, PUT, DELETE |
| `AuditLogsController` | `/auditlogs` | GET (filtrado/paginado) |
| `ParametrosController` | `/parametros` | GET, PUT (upsert) |
| `NumeracionesController` | `/numeraciones` | GET, POST, PUT, DELETE, POST /{id}/siguiente |
| `ClientesController` | `/clientes` | GET all, GET id, POST, PUT, DELETE |
| `ProveedoresController` | `/proveedores` | GET all, GET id, POST, PUT, DELETE |
| `AlmacenesController` | `/almacenes` | GET all, GET id, POST, PUT, DELETE |
| `UnidadesMedidaController` (legacy) | `/unidades-medida` | GET all, GET id, POST, PUT, DELETE |
| `CategoriasProductoController` (legacy) | `/categorias-producto` | GET all, POST, PUT, DELETE |
| `ProductosController` (legacy) | `/productos` | GET all, GET id, POST, PUT, DELETE |
| `MdmCatalogsController` | `/mdm/catalogs` | GET all, GET id, POST, PUT, DELETE |
| `MdmProductsController` | `/mdm/products` | GET all, GET id, POST, PUT, DELETE |
| `MdmCompanyProductsController` | `/mdm/company-products` | GET all, GET id, POST, PUT, DELETE |
| `MdmVariantsController` | `/mdm/variants` | GET all, GET id (byParent), POST, PUT, DELETE |
| `MdmAttributesController` | `/mdm/attributes` | GET (byIndustry), GET id, POST, DELETE; OPTIONS /{id}/options; GET product/{productId}; POST product |
| `MdmProductCodesController` | `/mdm/product-codes` | GET (byProduct), GET id, POST, DELETE |
| `MdmProductUomsController` | `/mdm/product-uoms` | GET (byProduct), POST, DELETE |
| `MdmCategoriesController` | `/mdm/categories` | GET, POST, PUT, DELETE |
| `MdmBrandsController` | `/mdm/brands` | GET all, GET id, POST, PUT, DELETE |
| `MdmManufacturersController` | `/mdm/manufacturers` | GET all, GET id, POST, PUT, DELETE |
| `PrcPriceListsController` | `/prc/price-lists` | GET all, GET id, POST, PUT, DELETE; items |
| `MovimientosInventarioController` | `/inventario/movimientos` | GET all, GET id, POST; GET kardex; GET stock |
| `RulIndustriesController` | `/rul/industries` | GET all, POST, PUT, DELETE; rules |
| `DiagnosticsController` | `/diagnostics` | GET health/ping |
| `OrdenesCompraController` | `/compras/ordenes` | GET all, GET id, POST, PUT, DELETE; POST /{id}/lineas; PUT/DELETE /{id}/lineas/{lineaId}; POST /{id}/estado |

### Middleware

| Middleware | Función |
|------------|---------|
| `GlobalExceptionMiddleware` | Captura excepciones no manejadas ? respuesta JSON uniforme |
| `TenantRequiredMiddleware` | Valida que el token lleve EmpresaId en Claims |

### Autenticación

- **Producción:** JWT Bearer con validación de Issuer/Audience/Key
- **Desarrollo:** `StubAuthHandler` — cualquier token activa autenticación (sin validar firma)

---

## 6. Capa Shared — DTOs

### Patrón de respuesta uniforme

```csharp
ApiResponse<T> {
  bool Success
  string? Message
  T? Data
  List<string> Errors
  // ApiResponse<T>.Ok(data, msg)
  // ApiResponse<T>.Fail(error)
}
```

### DTOs por módulo

#### Core
| DTO | Uso |
|-----|-----|
| `LoginRequestDto` | Login |
| `AuthResponseDto` | Token JWT + info usuario |
| `EmpresaDto / CreateEmpresaDto / UpdateEmpresaDto` | CRUD empresa |
| `UsuarioDto / CreateUsuarioDto / UpdateUsuarioDto / AsignarRolDto` | CRUD usuario |
| `RolDto / CreateRolDto / UpdateRolDto` | CRUD rol |
| `AuditLogDto / AuditLogFilterDto` | Auditoría |
| `ParametroSistemaDto / UpsertParametroDto` | Parámetros |
| `NumeracionDocumentoDto / CreateNumeracionDto / UpdateNumeracionDto` | Numeración |
| `PaginatedResultDto<T>` | Paginación genérica |

#### MDM — Producto Global
| DTO | Uso |
|-----|-----|
| `ProductDto2` | Lectura producto completo con navegaciones |
| `CreateProductDto2` | Creación |
| `UpdateProductDto2` | Actualización |
| `CatalogDto` | Catálogo |
| `BrandDto` | Marca |
| `ManufacturerDto` | Fabricante |
| `UomDto` | Unidad de medida |

#### MDM — Producto Empresa
| DTO | Uso |
|-----|-----|
| `CompanyProductDto` | Lectura con SKU, canales, stock |
| `CreateCompanyProductDto` | Activar producto en empresa |
| `UpdateCompanyProductDto` | Actualizar |

#### MDM — Variantes
| DTO | Uso |
|-----|-----|
| `ProductVariantDto` | Variante con ejes |
| `VariantAxisValueDto` | Eje (atributo + valor) |
| `CreateProductVariantDto` | Crear variante |
| `UpdateProductVariantDto` | Actualizar |

#### MDM — Atributos
| DTO | Uso |
|-----|-----|
| `AttributeDefinitionDto` | Atributo con opciones |
| `AttributeOptionDto` | Opción |
| `CreateAttributeDefinitionDto` | Crear |
| `CreateAttributeOptionDto / UpdateAttributeOptionDto` | Opciones |
| `ProductAttributeDto` | Valor EAV asignado al producto |
| `UpsertProductAttributeDto` | Crear/actualizar valor |

#### MDM — Códigos y UdM
| DTO | Uso |
|-----|-----|
| `ProductCodeDto` | Código producto |
| `CreateProductCodeDto` | Crear código |
| `ProductUomDto` | UdM alternativa |
| `CreateProductUomDto` | Crear UdM |

#### MDM — Terceros
| DTO | Uso |
|-----|-----|
| `ClienteDto / CreateClienteDto / UpdateClienteDto` | Clientes |
| `ProveedorDto / CreateProveedorDto / UpdateProveedorDto` | Proveedores |
| `AlmacenDto / CreateAlmacenDto / UpdateAlmacenDto` | Almacenes |
| `CategoriaProductoDto / CreateCategoriaProductoDto / UpdateCategoriaProductoDto` | Cats legacy |
| `CategoryDto` | Categorías MDM |

#### INV — Inventario
| DTO | Uso |
|-----|-----|
| `MovimientoInventarioDto` | Movimiento |
| `CreateMovimientoInventarioDto` | Crear movimiento |
| `StockProductoDto` | Stock por almacén |
| `KardexItemDto` | Línea de kardex |

#### PRC — Precios
| DTO | Uso |
|-----|-----|
| `PriceListDto` | Lista de precios con ítems |

#### RUL
| DTO | Uso |
|-----|-----|
| `IndustryDto` | Industria |

#### VER
| DTO | Uso |
|-----|-----|
| `EntityVersionDto` | Versión de entidad |

#### Constantes compartidas
```csharp
Roles.Admin = "Administrador"
Roles.User  = "Usuario"
```

---

## 7. Capa Web — Blazor WASM

### 7.1 Servicios HTTP

| Servicio | Base URL | Descripción |
|----------|----------|-------------|
| `JwtAuthStateProvider` | — | `AuthenticationStateProvider` JWT + localStorage |
| `AuthHttpService` | `/auth` | Login |
| `EmpresaHttpService` | `/empresas` | CRUD empresas |
| `EmpresaStateService` | — | Estado empresa activa (in-memory) |
| `UsuarioHttpService` | `/usuarios` | CRUD usuarios |
| `RolHttpService` | `/roles` | CRUD roles |
| `AuditLogHttpService` | `/auditlogs` | Consulta auditoría |
| `ParametroHttpService` | `/parametros` | Parámetros |
| `NumeracionHttpService` | `/numeraciones` | Numeración |
| `CategoriaProductoHttpService` (legacy) | `/categorias-producto` | Categorías |
| `CatalogoHttpService` | `/mdm/catalogs` | Catálogos |
| `UnidadMedidaHttpService` | `/unidades-medida` | UdM |
| `ProductoHttpService` (legacy) | `/productos` | Productos legacy |
| `ClienteHttpService` | `/clientes` | Clientes |
| `ProveedorHttpService` | `/proveedores` | Proveedores |
| `AlmacenHttpService` | `/almacenes` | Almacenes |
| `MdmProductoGlobalHttpService` | `/mdm/products` | Productos globales MDM |
| `MdmCompanyProductHttpService` | `/mdm/company-products` | Productos empresa |
| `MdmVariantHttpService` | `/mdm/variants` | Variantes |
| `MdmAttributeHttpService` | `/mdm/attributes` | Atributos + Product Attributes |
| `BrandHttpService` | `/mdm/brands` | Marcas |
| `ManufacturerHttpService` | `/mdm/manufacturers` | Fabricantes |
| `ProductUomHttpService` | `/mdm/product-uoms` | UdM producto |
| `ProductCodeHttpService` | `/mdm/product-codes` | Códigos producto |
| `PriceListHttpService` | `/prc/price-lists` | Listas de precios |
| `MovimientoInventarioHttpService` | `/inventario/movimientos` | Inventario |
| `OrdenCompraHttpService` | `/compras/ordenes` | Órdenes de compra |

### 7.2 Páginas Blazor

#### Layout
| Componente | Descripción |
|-----------|-------------|
| `MainLayout.razor` | Layout principal con sidebar y navbar |
| `NavMenu.razor` | Menú de navegación multi-módulo |
| `LoginLayout.razor` | Layout para pantalla de login |

#### Páginas

| Ruta | Página | Estado |
|------|--------|--------|
| `/login` | `Login.razor` | ? Completo |
| `/dashboard` | `Dashboard.razor` | ? Básico |
| **Config** | | |
| `/config/empresas` | `Empresas.razor` | ? CRUD completo |
| `/config/usuarios` | `Usuarios.razor` | ? CRUD + roles |
| `/config/roles` | `Roles.razor` | ? CRUD completo |
| `/config/auditoria` | `Auditoria.razor` | ? Consulta paginada |
| `/config/parametros-numeracion` | `ParametrosNumeracion.razor` | ? Parámetros + Numeración |
| **MDM Legacy** | | |
| `/mdm/categorias` | `Categorias.razor` | ? CRUD |
| `/mdm/unidades-medida` | `UnidadesMedida.razor` | ? CRUD |
| `/mdm/productos` | `Productos.razor` | ? **CRUD + Wizard 6 pestañas** |
| `/mdm/clientes` | `Clientes.razor` | ? CRUD completo |
| `/mdm/proveedores` | `Proveedores.razor` | ? CRUD completo |
| `/mdm/almacenes` | `Almacenes.razor` | ? CRUD completo |
| **MDM Avanzado** | | |
| `/mdm/productos-globales` | `ProductosGlobales.razor` | ? **CRUD + 4 sub-paneles** |
| `/mdm/productos-empresa` | `ProductosEmpresa.razor` | ? CRUD |
| `/mdm/variantes` | `VariantesProducto.razor` | ? CRUD + ejes |
| `/mdm/atributos` | `AtributosProducto.razor` | ? CRUD + opciones |
| `/mdm/marcas` | `Marcas.razor` | ? CRUD |
| `/mdm/fabricantes` | `Fabricantes.razor` | ? CRUD |
| `/mdm/listas-precios` | `ListasPrecios.razor` | ? CRUD + ítems |
| **Inventario** | | |
| `/inventario/movimientos` | `Movimientos.razor` | ? CRUD + filtros |
| `/inventario/stock` | `Stock.razor` | ? Consulta |
| `/inventario/kardex` | `Kardex.razor` | ? Kardex producto |
| `/inventario/almacenes` | `Almacenes.razor` (INV) | ? Vista |
| **Ventas** | | |
| `/ventas/pedidos` | `Pedidos.razor` | ?? Placeholder |
| `/ventas/facturas` | `Facturas.razor` | ?? Placeholder |
| **Compras** | | |
| `/compras/ordenes` | `OrdenesCompra.razor` | ? **CRUD completo + líneas + flujo estado** |

### 7.3 Wizard "Nuevo Producto" (`/mdm/productos`)

El modal de creación es un **wizard de 6 pestañas progresivas**:

| # | Pestaña | Funcionalidad |
|---|---------|--------------|
| 1 | **Datos Básicos** | Formulario principal. `Guardar y Continuar` ? crea el producto y habilita el resto |
| 2 | **UdM** | Agregar/eliminar UdM alternativas inline |
| 3 | **Códigos** | Agregar/eliminar códigos (barras, SKU, EAN) inline |
| 4 | **Atributos** | Asignar atributos EAV con campo dinámico según DataType |
| 5 | **Variantes** | Crear SKUs hijos con ejes de variante |
| 6 | **Empresa** | Activar producto en empresa con SKU, canales, stock min/max |

### 7.4 Sub-paneles en `ProductosGlobales` y `Productos`

Filas expandibles con pestañas de relaciones:

| Pestaña | Datos mostrados | Acciones |
|---------|----------------|---------|
| Variantes | SKU, nombre, barcode, ejes | Ver / link a gestión |
| UdM | Unidad, factor, barcode | Agregar, Eliminar |
| Códigos | Tipo, valor, vigencia | Agregar, Eliminar |
| Atributos | Nombre, valor, vigencia | Asignar (dinámico), Ver |
| Prod. Empresa | SKU, canales, stock | Ver / link a gestión |

---

## 8. Persistencia — Migraciones y Configuraciones

### Historial de Migraciones

| Migración | Descripción |
|-----------|-------------|
| `20260217065412_BaseCore` | Core: Empresas, Usuarios, UsuarioEmpresas, Monedas |
| `20260217084352_AddRolesTable` | Roles, vínculo UsuarioEmpresa-Rol |
| `20260217090218_AddAuditLogTable` | Tabla AuditLogs |
| `20260217091857_AddParametroSistemaNumeracionDocumento` | ParametrosSistema, NumeracionesDocumento |
| `20260217153259_AddDefaultAdminUser` | Seed usuario admin por defecto |
| `20260217191851_AddMDMEntities` | MDM inicial: Clientes, Proveedores, Almacenes, UoM legacy |
| `20260217194759_MDM_Enhanced` | MDM avanzado: Products, Variants, Attributes, CompanyProduct |
| `20260218215253_MDM_Refinements` | Refinamientos: PriceLists, Categories, Codes, ProductUoms |
| `20260219000000_INV_MovimientosInventario` | Inventario: Movimientos, Stock |
| `20260223085825_SeedProductStatus` | Seed estados de ciclo de vida de producto |
| `20260223091722_SeedDefaultCatalog` | Seed catálogo general por defecto |

### Interceptores EF Core

| Interceptor | Función |
|-------------|---------|
| `AuditableEntityInterceptor` | Auto-rellena `FechaCreacion`, `CreadoPor`, `FechaModificacion`, `ModificadoPor`. Genera registros en `AuditLogs` |
| `EntityVersioningInterceptor` | Genera snapshots JSON en `EntityVersions` en cada INSERT/UPDATE/DELETE |

### Repositorio Genérico

```csharp
IRepository<T> where T : class
  GetByIdAsync(id)
  GetAllAsync()
  FindAsync(predicate)
  AddAsync(entity)
  UpdateAsync(entity)
  DeleteAsync(entity)
  // + IUnitOfWork.SaveChangesAsync()
```

---

## 9. Patrones Transversales

### 9.1 Multi-tenancy

- `TenantEntity` agrega `EmpresaId` en todas las entidades tenant-aware
- El `DbContext` aplica filtros globales `Where(e => e.EmpresaId == _empresaId)` automáticamente
- El `EmpresaId` se obtiene de `ICurrentUserService` que lo lee del JWT Claim

### 9.2 Result Pattern

```csharp
// En servicios
return Result<T>.Success(value);
return Result<T>.Failure("mensaje de error");

// En controllers
if (!result.IsSuccess) return BadRequest(ApiResponse<T>.Fail(result.Error!));
return Ok(ApiResponse<T>.Ok(result.Value!));
```

### 9.3 ApiResponse uniforme

Todas las respuestas de la API siguen:
```json
{
  "success": true,
  "message": "Producto creado.",
  "data": { ... },
  "errors": []
}
```

### 9.4 DTOs EAV (Entity-Attribute-Value)

El modelo EAV para `ProductAttribute` soporta:
- `ValueString` ? DataType 1 (String)
- `ValueInt` ? DataType 2 (Int) 
- `ValueDecimal` ? DataType 3 (Decimal)
- `ValueBool` ? DataType 4 (Bool)
- `ValueDate` ? DataType 5 (Date)
- `ValueJson` ? DataType 6 (Json)
- `OptionId` ? DataType 7 (Select)

---

## 10. Estado Funcional por Módulo

| Módulo | Backend API | Frontend Blazor | Observaciones |
|--------|------------|-----------------|---------------|
| **Autenticación** | ? Completo | ? Completo | JWT + Stub dev |
| **Empresas** | ? Completo | ? Completo | Multi-tenant |
| **Usuarios / Roles** | ? Completo | ? Completo | |
| **Auditoría** | ? Completo | ? Completo | Automática vía interceptor |
| **Parámetros** | ? Completo | ? Completo | |
| **Numeración** | ? Completo | ? Completo | Auto-incremento |
| **Clientes** | ? Completo | ? Completo | |
| **Proveedores** | ? Completo | ? Completo | |
| **Almacenes** | ? Completo | ? Completo | |
| **Catálogos** | ? Completo | ? (en formularios) | |
| **UdM** | ? Completo | ? Completo | |
| **Marcas** | ? Completo | ? Completo | |
| **Fabricantes** | ? Completo | ? Completo | |
| **Productos (legacy)** | ? Completo | ? **Wizard 6 tabs** | Wizard inline |
| **Productos Globales MDM** | ? Completo | ? **Sub-paneles** | 5 relaciones |
| **CompanyProducts** | ? Completo | ? Completo | |
| **ProductCodes** | ? Completo | ? Integrado | En wizard y sub-paneles |
| **ProductUoms** | ? Completo | ? Integrado | En wizard y sub-paneles |
| **ProductAttributes EAV** | ? Completo | ? Integrado | Campo dinámico por DataType |
| **Variantes** | ? Completo | ? Completo | Con ejes de variante |
| **Categorías** | ? Completo | ? Completo | Jerárquicas N-nivel |
| **Atributos Dinámicos** | ? Completo | ? Completo | Por industria |
| **Industrias/Reglas** | ? Completo | ?? Sin UI | Endpoint disponible |
| **Listas de Precios** | ? Completo | ? Completo | Con ítems |
| **Inventario Movimientos** | ? Completo | ? Completo | 4 tipos de movimiento |
| **Stock / Kardex** | ? Completo | ? Completo | WAC automático |
| **Versionado** | ? Automático | ?? Sin UI | Interceptor activo |
| **Documentos** | ? Entidad/Config | ?? Sin UI | Storage pendiente |
| **Costeo** | ? Entidad/Config | ?? Sin UI | CostingRules + LandedCost |
| **Ventas (Pedidos/Facturas)** | ?? Pendiente | ?? Placeholder | Rama activa |
| **Compras (OC)** | ? Completo | ? **CRUD + líneas + flujo** | Cabecera + líneas, Confirmar/Aprobar/Anular |

---

## 11. Pendientes y Próximos Pasos

### Alta prioridad (rama `GestionImportacion`)
- [x] **Módulo Compras** — Órdenes de Compra con líneas de productos, flujo aprobación
- [ ] **Recepción de OC** — Crear movimiento Receipt automático al recepcionar
- [ ] **Módulo Importación** — Landed Cost aplicado sobre OC, distribución por método
- [ ] **Módulo Ventas** — Pedidos y Facturas integradas con stock e inventario

### Media prioridad
- [ ] **UI Industrias/Reglas** — Página de gestión de `Industries` y `ProductIndustryRules`
- [ ] **UI Versiones** — Historial de cambios de entidades
- [ ] **UI Documents** — Gestión de documentos multimedia adjuntos a productos
- [ ] **Features Activables** — UI para `CompanyProductFeatures` (LOT, SERIAL, FEFO, etc.)
- [ ] **Costeo Avanzado** — UI para `CostingRules` y `LandedCostProfiles`

### Baja prioridad
- [ ] **Exportación Excel/PDF** en listados
- [ ] **Dashboard** con métricas reales (stock bajo mínimo, movimientos del día)
- [ ] **Tests** — Cobertura de servicios Application
- [ ] **CI/CD** pipeline GitHub Actions
- [ ] **Migración almacén** — UI para `UbicacionesAlmacen`

---

*Documento generado automáticamente desde el estado del workspace en D:\AgoraCORE\AgoraHUB360-ERP*
