s# 📊 ESTADO DEL PROYECTO — AgoraHUB360 ERP

**Generado**: 23-Jun-2026

---

## 1. RESUMEN EJECUTIVO

El proyecto **AgoraHUB360 ERP** es un sistema ERP completo basado en **.NET 8** con **Clean Architecture**, multiempresa, y módulos funcionales para Core, MDM, Inventario, Compras, Contabilidad, Ventas y CxC.

- **Build**: ✅ 0 errores, 37 warnings (pre-existentes, no bloqueantes)
- **Tests**: ✅ 125/125 pasando
- **Migraciones**: ✅ 117 aplicadas
- **API**: ✅ Inicia correctamente en `https://localhost:5002`
- **Arquitectura**: 8 proyectos (Domain, Application, Persistence, Infrastructure, Api, Web, Shared, Tests)

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

### Principios de dise�o

| Principio | Implementaci�n |
|-----------|---------------|
| **Multi-tenant** | `TenantEntity` con `EmpresaId`. Query filters globales en EF Core a�slan datos por empresa |
| **Auditor�a autom�tica** | `AuditableEntity` + `AuditableEntityInterceptor` registra FechaCreacion/Modificacion/Usuario |
| **Versionado de entidades** | `EntityVersioningInterceptor` genera snapshots JSON en tabla `EntityVersions` |
| **Clean Architecture** | Domain no depende de nada; Application solo de Domain; Persistence/API solo de Application |
| **API Versionada** | URL Segment (`/api/v1/`) + Header (`X-Api-Version`) via `Asp.Versioning` |
| **Autenticaci�n** | JWT Bearer + StubAuthHandler para desarrollo |

---

## 2. Proyectos de la Soluci�n

| Proyecto | Tipo | Descripci�n |
|----------|------|-------------|
| `AgoraHub360.ERP.Domain` | Class Library .NET 8 | Entidades, interfaces de repositorio, enums, clases base |
| `AgoraHub360.ERP.Application` | Class Library .NET 8 | Servicios de aplicaci�n, interfaces, Result pattern |
| `AgoraHub360.ERP.Persistence` | Class Library .NET 8 | EF Core DbContext, Repositorios, Migraciones, Interceptores |
| `AgoraHub360.ERP.Infrastructure` | Class Library .NET 8 | Servicios externos (email, storage, etc.) |
| `AgoraHub360.ERP.Api` | ASP.NET Core Web API .NET 8 | Controllers REST versionados, Middleware, Auth, Swagger |
| `AgoraHub360.ERP.Shared` | Class Library .NET 8 | DTOs compartidos entre API y Web |
| `AgoraHub360.ERP.Web` | Blazor WebAssembly .NET 8 | SPA Frontend, HTTP Services, P�ginas Razor |
| `AgoraHub360.ERP.Tests` | xUnit .NET 8 | Pruebas unitarias/integraci�n |

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

#### `AuditableEntity` � Todas las entidades
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `FechaCreacion` | `DateTime` | Fecha de creaci�n (auto) |
| `CreadoPor` | `string?` | Usuario creador (auto) |
| `FechaModificacion` | `DateTime?` | Fecha �ltima modificaci�n (auto) |
| `ModificadoPor` | `string?` | Usuario que modific� (auto) |
| `Activo` | `bool` | Soft-delete l�gico |

#### `TenantEntity : AuditableEntity` � Entidades multi-empresa
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| Todos los de `AuditableEntity` | � | � |
| `EmpresaId` | `int` | FK a Empresa (filtro global autom�tico) |

---

### 3.2 M�dulo CORE

#### `Empresas`
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `Nombre` | `string(200)` | Nombre legal |
| `NIT` | `string?` | NIT/RUC fiscal |
| `Direccion` | `string?` | Direcci�n |
| `Telefono` | `string?` | Tel�fono |
| `Email` | `string?` | Email corporativo |
| `MonedaBaseId` | `string?` | FK ? Monedas |
| + campos AuditableEntity | | |

#### `Usuarios`
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `NombreUsuario` | `string` | Login �nico |
| `Email` | `string` | Email �nico |
| `PasswordHash` | `string` | Hash BCrypt |
| `NombreCompleto` | `string` | Nombre display |
| `EmpresaActivaId` | `int?` | Empresa activa en sesi�n |
| + campos AuditableEntity | | |

#### `UsuarioEmpresas` (N:N)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `UsuarioId` | `int` PK,FK | FK ? Usuarios |
| `EmpresaId` | `int` PK,FK | FK ? Empresas |
| `RolId` | `int` | FK ? Roles |
| `EsAdministrador` | `bool` | Flag admin |

#### `Roles`
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `Nombre` | `string(100)` | Nombre del rol |
| `Descripcion` | `string?` | Descripci�n |
| + campos AuditableEntity | | |

#### `ParametrosSistema` (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Clave` | `string(100)` | Clave �nica por empresa |
| `Valor` | `string(500)` | Valor |
| `Descripcion` | `string?` | Descripci�n |
| `Categoria` | `string(50)` | Agrupador UI |
| `TipoDato` | `string(20)` | String/Integer/Decimal/Boolean/Select |

#### `NumeracionesDocumento` (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `TipoDocumento` | `string` | "OC", "REC", "FAC", "PED", "NC" |
| `Descripcion` | `string` | Nombre legible |
| `Prefijo` | `string` | "OC-", "FAC-" |
| `SiguienteNumero` | `int` | Correlativo |
| `Digitos` | `int` | Padding ceros |

#### `Monedas`
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `Id` | `string(3)` PK | C�digo ISO (BOB, USD, EUR) |
| `Nombre` | `string` | Nombre |
| `Simbolo` | `string` | S�mbolo (Bs., $) |
| `TasaCambio` | `decimal` | Tasa respecto a moneda base |

#### `AuditLogs`
| Campo | Tipo | Descripci�n |
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

### 3.3 M�dulo MDM � Master Data Management

#### `Catalogs` (Global)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `CatalogId` | `long` PK | Identificador |
| `Name` | `string(100)` | Nombre de la cat�logo |
| `Description` | `string?` | Descripci�n |
| + campos AuditableEntity | | |

#### `Brands` (Global)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `BrandId` | `long` PK | Identificador |
| `Name` | `string(100)` | Nombre de la marca |
| `Country` | `string?` | Pa�s de origen |
| `Website` | `string?` | Sitio web |

#### `Manufacturers` (Global)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `ManufacturerId` | `long` PK | Identificador |
| `Name` | `string(200)` | Nombre del fabricante |
| `Country` | `string?` | Pa�s |
| `Website` | `string?` | Sitio web |
| `ContactEmail` | `string?` | Email de contacto |

#### `Uoms` � Unidades de Medida (Global)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `UomId` | `int` PK | Identificador |
| `Code` | `string(20)` | C�digo (UND, KG, LT, CJ) |
| `Name` | `string(100)` | Nombre |
| `Category` | `string?` | Masa / Volumen / Unidades |

#### `ProductStatuses` � Ciclo de Vida (Global Seed)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `StatusId` | `int` PK | 1=Borrador, 2=Activo, 3=Bloqueado, 4=Descontinuado |
| `Code` | `string` | "Draft", "Active", "Blocked", "Discontinued" |
| `Name` | `string` | Nombre display |

#### `Products` � Producto Global (No tenant)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `ProductId` | `long` PK | Identificador |
| `CatalogId` | `long` FK | Cat�logo |
| `ProductKind` | `byte` | 1=Bien, 2=Servicio, 3=Kit, 4=MateriaPrima, 5=Embalaje |
| `GenericName` | `string(300)` | Nombre gen�rico (DCI) |
| `CommercialName` | `string(300)` | Nombre comercial |
| `ShortDescription` | `string?` | Descripci�n corta |
| `LongDescription` | `string?` | Descripci�n larga |
| `BrandId` | `long?` FK | Marca |
| `ManufacturerId` | `long?` FK | Fabricante |
| `DefaultUomId` | `int` FK | UdM base |
| `IsStockable` | `bool` | Controla stock |
| `IsSellable` | `bool` | Se puede vender |
| `IsPurchasable` | `bool` | Se puede comprar |
| `LifecycleStatusId` | `int` FK | Estado de ciclo |
| + campos AuditableEntity | | |

**Relaciones de Product:**
- 1:N ? `CompanyProducts` (activaci�n por empresa)
- 1:N ? `ProductCodes` (c�digos de barras, SKU, etc.)
- N:N ? `Categories` (v�a `ProductCategories`)
- 1:N ? `ProductAttributes` (atributos EAV)
- 1:N ? `ProductUoms` (UdM alternativas)
- 1:N ? `ProductVariants` (SKUs hijos)
- N:N ? `ProductClassifications` (v�a `ProductClassificationLinks`)

#### `CompanyProducts` � Extensi�n por Empresa (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `CompanyProductId` | `long` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `ProductId` | `long` FK | Producto global |
| `Sku` | `string(100)` | SKU propio empresa |
| `CodigoInterno` | `string?` | C�digo interno |
| `ImpuestoProfileId` | `int?` | Perfil impuesto |
| `MonedaBaseId` | `string?` | Moneda |
| `IsVisiblePOS` | `bool` | Canal POS |
| `IsVisibleEcommerce` | `bool` | Canal Ecommerce |
| `IsVisibleB2B` | `bool` | Canal B2B |
| `AllowReturns` | `bool` | Permite devoluciones |
| `WarrantyDays` | `int?` | D�as de garant�a |
| `MinStock` | `decimal?` | Stock m�nimo |
| `MaxStock` | `decimal?` | Stock m�ximo |
| `ReorderPoint` | `decimal?` | Punto de reorden |
| `CostingMethod` | `byte?` | Override costeo. 1=Promedio, 2=FIFO, 3=Est�ndar |
| + campos TenantEntity | | |

#### `CompanyProductFeatures` � Features Activables
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `EmpresaId` | `int` PK,FK | Empresa |
| `CompanyProductId` | `long` PK,FK | CompanyProduct |
| `FeatureCode` | `string` PK | LOT, SERIAL, FEFO, HAZMAT, QC, BOM, EXPIRY, ALLERGEN |
| `IsEnabled` | `bool` | Activado |

#### `ProductCodes` � Multi-Identificadores
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `ProductCodeId` | `long` PK | Identificador |
| `EmpresaId` | `int?` | Null = global |
| `ProductId` | `long` FK | Producto |
| `CodeType` | `byte` | 1=SKU, 2=Barra, 3=QR, 4=ExtProveedor, 5=ExtCliente, 6=Canal, 7=InternoAlt |
| `Valor` | `string(200)` | Valor del c�digo |
| `ProviderId` | `long?` | Proveedor asociado |
| `CustomerId` | `long?` | Cliente asociado |
| `ChannelId` | `int?` | Canal |
| `ValidFrom` | `DateOnly?` | Vigencia desde |
| `ValidTo` | `DateOnly?` | Vigencia hasta |
| `IsPrimary` | `bool` | C�digo principal |
| `IsActive` | `bool` | Activo |

#### `ProductUoms` � UdM Alternativas
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `ProductUomId` | `long` PK | Identificador |
| `ProductId` | `long` FK | Producto |
| `UomId` | `int` FK | Unidad de medida |
| `IsBase` | `bool` | Es UdM base |
| `FactorToBase` | `decimal` | Factor conversi�n (1 CJ = 12 UND ? 12) |
| `Barcode` | `string?` | C�digo de barras propio |

#### `ProductVariants` � SKUs Hijos (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `VariantId` | `long` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `ParentProductId` | `long` FK | Producto padre |
| `Sku` | `string(100)` | SKU variante |
| `Barcode` | `string?` | C�digo de barras |
| `VariantName` | `string?` | "Talla M / Color Negro" |

#### `VariantAttributeValues` � Valores de Ejes de Variante
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `VariantId` | `long` PK,FK | Variante |
| `AttributeId` | `long` PK,FK | Atributo (eje) |
| `OptionId` | `long?` | Opci�n seleccionada |
| `ValueString` | `string?` | Valor libre |

#### `AttributeDefinitions` � Atributos Din�micos por Industria
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `AttributeId` | `long` PK | Identificador |
| `IndustryId` | `int` FK | Industria |
| `Code` | `string(50)` | C�digo �nico por industria |
| `Name` | `string(100)` | Nombre |
| `DataType` | `byte` | 1=String, 2=Int, 3=Decimal, 4=Bool, 5=Date, 6=Json, 7=Select |
| `IsRequired` | `bool` | Obligatorio |
| `IsSearchable` | `bool` | Indexable en b�squeda |
| `IsVariantAxis` | `bool` | Eje de variante (color, talla) |
| `ValidationRegex` | `string?` | Regex de validaci�n |
| `MinValue` | `decimal?` | M�nimo num�rico |
| `MaxValue` | `decimal?` | M�ximo num�rico |
| `UnitHint` | `string?` | Hint de unidad (kg, cm) |

#### `AttributeOptions` � Opciones de Select
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `OptionId` | `long` PK | Identificador |
| `AttributeId` | `long` FK | Atributo padre |
| `Value` | `string(200)` | Valor opci�n |
| `SortOrder` | `int` | Orden |

#### `ProductAttributes` � Valores EAV por Producto
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `ProductAttributeId` | `long` PK | Identificador |
| `ProductId` | `long` FK | Producto |
| `AttributeId` | `long` FK | Definici�n del atributo |
| `ValueString` | `string?` | Valor texto |
| `ValueDecimal` | `decimal?` | Valor decimal |
| `ValueInt` | `int?` | Valor entero |
| `ValueBool` | `bool?` | Valor booleano |
| `ValueDate` | `DateOnly?` | Valor fecha |
| `ValueJson` | `string?` | Valor JSON |
| `OptionId` | `long?` | Opci�n seleccionada |
| `ValidFrom` | `DateOnly?` | Vigencia desde |
| `ValidTo` | `DateOnly?` | Vigencia hasta |

#### `Categories` � Categor�as Jer�rquicas
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `CategoryId` | `long` PK | Identificador |
| `CatalogId` | `long` FK | Cat�logo |
| `ParentCategoryId` | `long?` FK | Padre (self-ref, N niveles) |
| `Name` | `string(200)` | Nombre |
| `Path` | `string?` | Ruta materializada "/Electronics/Phones" |
| `SortOrder` | `int` | Orden |

#### `ProductCategories` (N:N)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `ProductId` | `long` PK,FK | Producto |
| `CategoryId` | `long` PK,FK | Categor�a |

#### `ProductClassifications` � Clasificaciones Adicionales
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `ClassificationId` | `long` PK | Identificador |
| `CatalogId` | `long` FK | Cat�logo |
| `Type` | `string` | Tipo de clasificaci�n |
| `Name` | `string` | Nombre |

#### `ProductClassificationLinks` (N:N)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `ProductId` | `long` PK,FK | Producto |
| `ClassificationId` | `long` PK,FK | Clasificaci�n |

#### `Clientes` (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Codigo` | `string(50)` | C�digo cliente |
| `RazonSocial` | `string(200)` | Raz�n social |
| `NIT` | `string?` | NIT/RUC |
| `Direccion` | `string?` | Direcci�n |
| `Telefono` | `string?` | Tel�fono |
| `Email` | `string?` | Email |
| `NombreContacto` | `string?` | Contacto |
| `TipoCliente` | `string(50)` | General, VIP, etc. |

#### `Proveedores` (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Codigo` | `string(50)` | C�digo proveedor |
| `RazonSocial` | `string(200)` | Raz�n social |
| `NIT` | `string?` | NIT/RUC |
| `Direccion` | `string?` | Direcci�n |
| `Telefono` | `string?` | Tel�fono |
| `Email` | `string?` | Email |
| `NombreContacto` | `string?` | Contacto |
| `TipoProveedor` | `string(50)` | Local, Internacional |
| `Pais` | `string?` | Pa�s |
| `CondicionPago` | `string?` | Condici�n de pago |

#### `Almacenes` (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Codigo` | `string(20)` | C�digo |
| `Nombre` | `string(200)` | Nombre |
| `Direccion` | `string?` | Direcci�n |
| `Responsable` | `string?` | Responsable |
| `Telefono` | `string?` | Tel�fono |

#### `UbicacionesAlmacen`
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `AlmacenId` | `int` FK | Almac�n |
| `Codigo` | `string` | C�digo ubicaci�n (A1-B2-C3) |
| `Nombre` | `string?` | Nombre legible |

---

### 3.4 M�dulo INV � Inventario

#### `MovimientosInventario` (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Number` | `string` | N�mero �nico "MOV-2026-00001" |
| `MovementType` | `string` | Receipt / Issue / Adjustment / Transfer |
| `MovementDate` | `DateTime` | Fecha del movimiento |
| `CompanyProductId` | `long` FK | Producto empresa |
| `WarehouseId` | `int` FK | Almac�n origen |
| `DestinationWarehouseId` | `int?` FK | Almac�n destino (Transfer) |
| `Quantity` | `decimal` | Cantidad (siempre positivo) |
| `UnitCost` | `decimal` | Costo unitario |
| `TotalCost` | `decimal` | Costo total |
| `Reference` | `string?` | Documento origen (OC-001) |
| `Notes` | `string?` | Notas |

#### `StockProductos` (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `CompanyProductId` | `long` FK | Producto empresa |
| `AlmacenId` | `int` FK | Almac�n |
| `CurrentStock` | `decimal` | Stock disponible actual |
| `AverageCost` | `decimal` | Costo promedio ponderado (WAC) |
| `LastUpdated` | `DateTime` | �ltima actualizaci�n |

---

### 3.5 M�dulo PRC � Precios

#### `PriceLists` (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `PriceListId` | `long` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Code` | `string(50)` | C�digo |
| `Name` | `string(200)` | Nombre |
| `CurrencyId` | `string(3)` | Moneda |
| `ChannelId` | `int?` | Canal |
| `ValidFrom` | `DateOnly?` | Vigencia desde |
| `ValidTo` | `DateOnly?` | Vigencia hasta |
| `IsDefault` | `bool` | Lista por defecto |

#### `PriceListItems`
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `ItemId` | `long` PK | Identificador |
| `PriceListId` | `long` FK | Lista de precios |
| `CompanyProductId` | `long?` FK | Por CompanyProduct (XOR) |
| `VariantId` | `long?` FK | Por Variante (XOR) |
| `Price` | `decimal` | Precio |
| `MinQty` | `decimal?` | Cantidad m�nima (precio por volumen) |
| `DiscountPercent` | `decimal?` | Descuento % |
| `ValidFrom` | `DateOnly?` | Vigencia desde |
| `ValidTo` | `DateOnly?` | Vigencia hasta |

---

### 3.6 M�dulo CST � Costeo

#### `CostingRules`
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `EmpresaId` | `int` PK | Empresa |
| `ProductKind` | `byte` PK | Tipo de producto |
| `DefaultMethod` | `byte` | 1=Promedio WAC, 2=FIFO, 3=Est�ndar |

#### `LandedCostProfiles` (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `ProfileId` | `long` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Name` | `string` | Nombre |
| `AllocationMethod` | `byte` | 1=Peso, 2=Volumen, 3=Valor, 4=Unidades |
| `IsDefault` | `bool` | Default |

---

### 3.7 M�dulo RUL � Reglas

#### `Industries` � Industrias/Segmentos
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `IndustryId` | `int` PK | Identificador |
| `Code` | `string` | "PHARMA", "HARDWARE", "FOOD" |
| `Name` | `string` | Nombre |

#### `ProductIndustryRules`
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `RuleId` | `long` PK | Identificador |
| `IndustryId` | `int` FK | Industria |
| `ConditionJson` | `string JSON` | Condici�n evaluable |
| `ActionsJson` | `string JSON` | Features a habilitar |
| `Priority` | `int` | Prioridad |

---

### 3.8 M�dulo VER � Versionado

#### `EntityVersions`
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `VersionId` | `long` PK | Identificador |
| `EntityName` | `string` | Nombre de la entidad |
| `EntityId` | `string` | PK serializada |
| `VersionNo` | `int` | N�mero de versi�n |
| `ChangeType` | `byte` | 1=Create, 2=Update, 3=Delete |
| `SnapshotJson` | `string JSON` | Estado completo serializado |
| `ChangedBy` | `string?` | Usuario |
| `ChangedAt` | `DateTime` | Fecha UTC |
| `EmpresaId` | `int?` | Empresa contexto |

---

### 3.9 M�dulo DOC � Documentos Multimedia

#### `Documents` (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `DocumentId` | `long` PK | Identificador |
| `StorageProvider` | `byte` | 1=DB, 2=FileSystem, 3=S3, 4=Azure |
| `FileName` | `string` | Nombre del archivo |
| `MimeType` | `string` | MIME type |
| `Url` | `string?` | URL de acceso |
| `Hash` | `string?` | Hash de integridad |
| `SizeBytes` | `long` | Tama�o en bytes |

#### `ProductDocuments` (N:N)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `ProductId` | `long` PK,FK | Producto |
| `DocumentId` | `long` PK,FK | Documento |
| `DocType` | `string?` | Ficha t�cnica, imagen, certificado |

---

### 3.10 M�dulo CMP � Compras

#### `OrdenesCompra` (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `OrdenCompraId` | `long` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Numero` | `string(30)` | N�mero �nico "OC-000001" |
| `FechaEmision` | `DateTime` | Fecha de emisi�n |
| `FechaEntregaEstimada` | `DateTime?` | Fecha estimada de entrega |
| `ProveedorId` | `int` FK | Proveedor |
| `AlmacenDestinoId` | `int` FK | Almac�n destino |
| `MonedaId` | `string(3)` | Moneda (ISO 4217) |
| `TasaCambio` | `decimal` | Tasa de cambio |
| `Estado` | `byte` (enum) | 1=Borrador, 2=Confirmado, 3=Aprobado, 4=Anulado, 5=RecepcionParcial, 6=Cerrado |
| `CondicionPago` | `string?` | Condici�n de pago |
| `Observaciones` | `string?` | Observaciones |
| `ReferenciaExterna` | `string?` | Nro. cotizaci�n proveedor |
| `Subtotal` | `decimal` | Suma subtotales l�neas |
| `Descuento` | `decimal` | Suma descuentos l�neas |
| `Impuesto` | `decimal` | Suma impuestos l�neas |
| `Total` | `decimal` | Subtotal - Descuento + Impuesto |

#### `OrdenCompraLineas`
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `OrdenCompraLineaId` | `long` PK | Identificador |
| `OrdenCompraId` | `long` FK | Orden de compra |
| `NumeroLinea` | `int` | N�mero secuencial |
| `CompanyProductId` | `long` FK | Producto empresa |
| `Descripcion` | `string(300)` | Descripci�n libre |
| `UnidadMedida` | `string(20)` | UdM de compra |
| `Cantidad` | `decimal` | Cantidad solicitada |
| `PrecioUnitario` | `decimal` | Precio unitario |
| `PorcentajeDescuento` | `decimal` | Descuento % (0�100) |
| `MontoDescuento` | `decimal` | Monto descuento calculado |
| `Subtotal` | `decimal` | (Cant � P/U) - Descuento |
| `PorcentajeImpuesto` | `decimal` | Impuesto % |
| `MontoImpuesto` | `decimal` | Monto impuesto calculado |
| `TotalLinea` | `decimal` | Subtotal + Impuesto |
| `CantidadRecepcionada` | `decimal` | Cantidad ya recibida |

#### `RecepcionesCompra` (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `RecepcionCompraId` | `long` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Numero` | `string(30)` | N�mero �nico "REC-000001" |
| `OrdenCompraId` | `long` FK | OC asociada |
| `FechaRecepcion` | `DateTime` | Fecha de recepci�n |
| `AlmacenId` | `int` FK | Almac�n destino |
| `DocumentoProveedor` | `string?` | Gu�a/factura del proveedor |
| `Observaciones` | `string?` | Observaciones |
| `Confirmada` | `bool` | Ya gener� movimientos INV |

#### `RecepcionCompraLineas`
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `RecepcionCompraLineaId` | `long` PK | Identificador |
| `RecepcionCompraId` | `long` FK | Recepci�n |
| `OrdenCompraLineaId` | `long` FK | L�nea de OC |
| `CantidadRecibida` | `decimal` | Cantidad recibida |
| `CostoUnitario` | `decimal` | Costo unitario |
| `Notas` | `string?` | Notas |

#### `HojasImportacion` (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `EmpresaId` | `int` FK | Empresa |
| `Fecha` | `DateTime` | Fecha de la importaci�n |
| `ArchivoNombre` | `string` | Nombre del archivo |
| `ArchivoHash` | `string` | Hash de integridad |
| `ArchivoTamano` | `long` | Tama�o en bytes |
| `Estado` | `byte` | 1=EnCola, 2=EnProceso, 3=Completo, 4=Error |
| `MensajeError` | `string?` | Mensaje de error si falla |
| `RegistrosProcesados` | `int` | Cantidad de registros procesados |
| `RegistrosExitosos` | `int` | Cantidad de registros exitosos |
| `RegistrosErrores` | `int` | Cantidad de registros con errores |

#### `GastosImportacion` (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `HojaImportacionId` | `int` FK | Referencia a la hoja de importaci�n |
| `Linea` | `int` | L�nea en el archivo |
| `Descripcion` | `string` | Descripci�n del gasto |
| `Monto` | `decimal` | Monto del gasto |
| `MonedaId` | `string(3)` | Moneda del gasto |
| `ProveedorId` | `int?` | Proveedor asociado (opcional) |
| `Fecha` | `DateTime?` | Fecha del gasto (opcional) |
| `TipoGasto` | `string` | Tipo de gasto (fijo, variable) |
| `CentroCosto` | `string?` | Centro de costo (opcional) |

#### `ImportacionLineas` (TenantEntity)
| Campo | Tipo | Descripci�n |
|-------|------|-------------|
| `Id` | `int` PK | Identificador |
| `HojaImportacionId` | `int` FK | Referencia a la hoja de importaci�n |
| `Linea` | `int` | L�nea en el archivo |
| `ProveedorId` | `int` | Proveedor |
| `ProductoId` | `long` | Producto global |
| `Sku` | `string?` | SKU propio empresa |
| `Cantidad` | `decimal` | Cantidad ordenada |
| `PrecioUnitario` | `decimal` | Precio unitario |
| `MonedaId` | `string(3)` | Moneda del precio |
| `TasaCambio` | `decimal` | Tasa de cambio a moneda base |
| `Descuento` | `decimal?` | Descuento aplicado |
| `Impuesto` | `decimal?` | Impuesto aplicado |
| `TotalLinea` | `decimal` | Total l�nea (Cantidad x PrecioUnitario) |

---

### Mapa completo de tablas

```
CORE:        Empresas � Usuarios � UsuarioEmpresas � Roles � Monedas
             ParametrosSistema � NumeracionesDocumento � AuditLogs

MDM:         Catalogs � Brands � Manufacturers � Uoms � ProductStatuses � Products
             CompanyProducts � CompanyProductFeatures
             ProductCodes � ProductUoms � ProductVariants � VariantAttributeValues
             AttributeDefinitions � AttributeOptions � ProductAttributes
             Categories � ProductCategories
             ProductClassifications � ProductClassificationLinks
             Clientes � Proveedores � Almacenes � UbicacionesAlmacen

INV:         MovimientosInventario � StockProductos

PRC:         PriceLists � PriceListItems

CST:         CostingRules � LandedCostProfiles

RUL:         Industries � ProductIndustryRules

VER:         EntityVersions

DOC:         Documents � ProductDocuments

CMP:         OrdenesCompra � OrdenCompraLineas � RecepcionesCompra � RecepcionCompraLineas
             HojasImportacion � GastosImportacion � ImportacionLineas
```

**Total de tablas: 51**

---

## 4. Capa Application � Servicios e Interfaces

### 4.1 Patr�n Result

```csharp
Result<T>
  .Success(value)   // IsSuccess = true, Value = T
  .Failure(error)   // IsSuccess = false, Error = string
```

### 4.2 Servicios registrados

| Interface | Implementaci�n | Descripci�n |
|-----------|---------------|-------------|
| `IAuthService` | `AuthService` (Api) | Login/JWT |
| `IEmpresaService` | `EmpresaService` | CRUD empresas |
| `IUsuarioService` | `UsuarioService` | CRUD usuarios, roles |
| `IRolService` | `RolService` | CRUD roles |
| `IAuditLogService` | `AuditLogService` (Persistence) | Consulta auditoria paginada |
| `IParametroSistemaService` | `ParametroSistemaService` | CRUD par�metros |
| `INumeracionDocumentoService` | `NumeracionDocumentoService` | Gesti�n numeraci�n |
| `IProductoService` (legacy) | `ProductoService` | Wrapper legacy |
| `IProductService` (MDM) | `ProductService` | CRUD producto global |
| `ICatalogService` | `CatalogService` | CRUD cat�logos |
| `IBrandService` | `BrandService` | CRUD marcas |
| `IManufacturerService` | `ManufacturerService` | CRUD fabricantes |
| `IUomService` | `UomService` | CRUD UdM globales |
| `IUnidadMedidaService` (legacy) | `UnidadMedidaService` | Wrapper legacy |
| `IProductUomService` | `ProductUomService` | UdM alternativas producto |
| `IProductCodeService` | `ProductCodeService` | C�digos producto |
| `ICompanyProductService` | `CompanyProductService` | Activaci�n empresa |
| `IProductVariantService` | `ProductVariantService` | Variantes SKU |
| `IAttributeDefinitionService` | `AttributeDefinitionService` | Atributos din�micos + Product Attributes |
| `ICategoryService` | `CategoryService` | Categor�as jer�rquicas |
| `ICategoriaProductoService` (legacy) | `CategoriaProductoService` | Wrapper legacy |
| `IClienteService` | `ClienteService` | CRUD clientes |
| `IProveedorService` | `ProveedorService` | CRUD proveedores |
| `IAlmacenService` | `AlmacenService` | CRUD almacenes |
| `IMovimientoInventarioService` | `MovimientoInventarioService` | Movimientos INV + Stock |
| `IPriceListService` | `PriceListService` | Listas de precios |
| `IIndustryService` | `IndustryService` | Industrias/Reglas |
| `IVersioningService` | `VersioningService` | Consulta versiones |
| `ICurrentUserService` | `CurrentUserService` (Api) | Contexto usuario/empresa |
| `IOrdenCompraService` | `OrdenCompraService` | CRUD �rdenes de compra |
| `IRecepcionCompraService` | `RecepcionCompraService` | Recepciones + movimientos INV autom�ticos |
| `IHojaImportacionService` | `HojaImportacionService` | Landed Cost: gastos + distribuci�n + ajuste WAC |

---

## 5. Capa API � Controladores y Endpoints

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
| `RecepcionesCompraController` | `/compras/recepciones` | GET all, GET id, POST, DELETE |
| `ImportacionesController` | `/compras/importaciones` | GET all, GET id, POST, DELETE; POST /{id}/gastos; DELETE /{id}/gastos/{gastoId}; POST /{id}/liquidar |

### Middleware

| Middleware | Funci�n |
|------------|---------|
| `GlobalExceptionMiddleware` | Captura excepciones no manejadas ? respuesta JSON uniforme |
| `TenantRequiredMiddleware` | Valida que el token lleve EmpresaId en Claims |

### Autenticaci�n

- **Producci�n:** JWT Bearer con validaci�n de Issuer/Audience/Key
- **Desarrollo:** `StubAuthHandler` � cualquier token activa autenticaci�n (sin validar firma)

---

## 6. Capa Shared � DTOs

### Patr�n de respuesta uniforme

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

### DTOs por m�dulo

#### Core
| DTO | Uso |
|-----|-----|
| `LoginRequestDto` | Login |
| `AuthResponseDto` | Token JWT + info usuario |
| `EmpresaDto / CreateEmpresaDto / UpdateEmpresaDto` | CRUD empresa |
| `UsuarioDto / CreateUsuarioDto / UpdateUsuarioDto / AsignarRolDto` | CRUD usuario |
| `RolDto / CreateRolDto / UpdateRolDto` | CRUD rol |
| `AuditLogDto / AuditLogFilterDto` | Auditor�a |
| `ParametroSistemaDto / UpsertParametroDto` | Par�metros |
| `NumeracionDocumentoDto / CreateNumeracionDto / UpdateNumeracionDto` | Numeraci�n |
| `PaginatedResultDto<T>` | Paginaci�n gen�rica |

#### MDM � Producto Global
| DTO | Uso |
|-----|-----|
| `ProductDto2` | Lectura producto completo con navegaciones |
| `CreateProductDto2` | Creaci�n |
| `UpdateProductDto2` | Actualizaci�n |
| `CatalogDto` | Cat�logo |
| `BrandDto` | Marca |
| `ManufacturerDto` | Fabricante |
| `UomDto` | Unidad de medida |

#### MDM � Producto Empresa
| DTO | Uso |
|-----|-----|
| `CompanyProductDto` | Lectura con SKU, canales, stock |
| `CreateCompanyProductDto` | Activar producto en empresa |
| `UpdateCompanyProductDto` | Actualizar |

#### MDM � Variantes
| DTO | Uso |
|-----|-----|
| `ProductVariantDto` | Variante con ejes |
| `VariantAxisValueDto` | Eje (atributo + valor) |
| `CreateProductVariantDto` | Crear variante |
| `UpdateProductVariantDto` | Actualizar |

#### MDM � Atributos
| DTO | Uso |
|-----|-----|
| `AttributeDefinitionDto` | Atributo con opciones |
| `AttributeOptionDto` | Opci�n |
| `CreateAttributeDefinitionDto` | Crear |
| `CreateAttributeOptionDto / UpdateAttributeOptionDto` | Opciones |
| `ProductAttributeDto` | Valor EAV asignado al producto |
| `UpsertProductAttributeDto` | Crear/actualizar valor |

#### MDM � C�digos y UdM
| DTO | Uso |
|-----|-----|
| `ProductCodeDto` | C�digo producto |
| `CreateProductCodeDto` | Crear c�digo |
| `ProductUomDto` | UdM alternativa |
| `CreateProductUomDto` | Crear UdM |

#### MDM � Terceros
| DTO | Uso |
|-----|-----|
| `ClienteDto / CreateClienteDto / UpdateClienteDto` | Clientes |
| `ProveedorDto / CreateProveedorDto / UpdateProveedorDto` | Proveedores |
| `AlmacenDto / CreateAlmacenDto / UpdateAlmacenDto` | Almacenes |
| `CategoriaProductoDto / CreateCategoriaProductoDto / UpdateCategoriaProductoDto` | Cats legacy |
| `CategoryDto` | Categor�as MDM |

#### INV � Inventario
| DTO | Uso |
|-----|-----|
| `MovimientoInventarioDto` | Movimiento |
| `CreateMovimientoInventarioDto` | Crear movimiento |
| `StockProductoDto` | Stock por almac�n |
| `KardexItemDto` | L�nea de kardex |

#### PRC � Precios
| DTO | Uso |
|-----|-----|
| `PriceListDto` | Lista de precios con �tems |

#### RUL
| DTO | Uso |
|-----|-----|
| `IndustryDto` | Industria |

#### VER
| DTO | Uso |
|-----|-----|
| `EntityVersionDto` | Versi�n de entidad |

#### Constantes compartidas
```csharp
Roles.Admin = "Administrador"
Roles.User  = "Usuario"
```

---

## 7. Capa Web � Blazor WASM

### 7.1 Servicios HTTP

| Servicio | Base URL | Descripci�n |
|----------|----------|-------------|
| `JwtAuthStateProvider` | � | `AuthenticationStateProvider` JWT + localStorage |
| `AuthHttpService` | `/auth` | Login |
| `EmpresaHttpService` | `/empresas` | CRUD empresas |
| `EmpresaStateService` | � | Estado empresa activa (in-memory) |
| `UsuarioHttpService` | `/usuarios` | CRUD usuarios |
| `RolHttpService` | `/roles` | CRUD roles |
| `AuditLogHttpService` | `/auditlogs` | Consulta auditor�a |
| `ParametroHttpService` | `/parametros` | Par�metros |
| `NumeracionHttpService` | `/numeraciones` | Numeraci�n |
| `CategoriaProductoHttpService` (legacy) | `/categorias-producto` | Categor�as |
| `CatalogoHttpService` | `/mdm/catalogs` | Cat�logos |
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
| `ProductCodeHttpService` | `/mdm/product-codes` | C�digos producto |
| `PriceListHttpService` | `/prc/price-lists` | Listas de precios |
| `MovimientoInventarioHttpService` | `/inventario/movimientos` | Inventario |
| `OrdenCompraHttpService` | `/compras/ordenes` | �rdenes de compra |
| `RecepcionCompraHttpService` | `/compras/recepciones` | Recepciones de compra |
| `HojaImportacionHttpService` | `/compras/importaciones` | Hojas de importaci�n (Landed Cost) |

### 7.2 P�ginas Blazor

#### Layout
| Componente | Descripci�n |
|-----------|-------------|
| `MainLayout.razor` | Layout principal con sidebar y navbar |
| `NavMenu.razor` | Men� de navegaci�n multi-m�dulo |
| `LoginLayout.razor` | Layout para pantalla de login |

#### P�ginas

| Ruta | P�gina | Estado |
|------|--------|--------|
| `/login` | `Login.razor` | ? Completo |
| `/dashboard` | `Dashboard.razor` | ? B�sico |
| **Config** | | |
| `/config/empresas` | `Empresas.razor` | ? CRUD completo |
| `/config/usuarios` | `Usuarios.razor` | ? CRUD + roles |
| `/config/roles` | `Roles.razor` | ? CRUD completo |
| `/config/auditoria` | `Auditoria.razor` | ? Consulta paginada |
| `/config/parametros-numeracion` | `ParametrosNumeracion.razor` | ? Par�metros + Numeraci�n |
| **MDM Legacy** | | |
| `/mdm/categorias` | `Categorias.razor` | ? CRUD |
| `/mdm/unidades-medida` | `UnidadesMedida.razor` | ? CRUD |
| `/mdm/productos` | `Productos.razor` | ? **CRUD + Wizard 6 pesta�as** |
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
| `/mdm/listas-precios` | `ListasPrecios.razor` | ? CRUD + �tems |
| **Ventas** | | |
| `/ventas/pedidos` | `Pedidos.razor` | ?? Placeholder |
| `/ventas/facturas` | `Facturas.razor` | ?? Placeholder |
| **Compras** | | |
| `/compras/ordenes` | `OrdenesCompra.razor` | ? **CRUD completo + l�neas + flujo estado** |
| `/compras/recepciones` | `RecepcionesCompra.razor` | ? **Recepci�n parcial/total + INV autom�tico** |
| `/compras/importaciones` | `Importaciones.razor` | ? **Landed Cost: gastos + distribuci�n + liquidaci�n** |

### 7.3 Wizard "Nuevo Producto" (`/mdm/productos`)

El modal de creaci�n es un **wizard de 6 pesta�as progresivas**:

| # | Pesta�a | Funcionalidad |
|---|---------|--------------|
| 1 | **Datos B�sicos** | Formulario principal. `Guardar y Continuar` ? crea el producto y habilita el resto |
| 2 | **UdM** | Agregar/eliminar UdM alternativas inline |
| 3 | **C�digos** | Agregar/eliminar c�digos (barras, SKU, EAN) inline |
| 4 | **Atributos** | Asignar atributos EAV con campo din�mico seg�n DataType |
| 5 | **Variantes** | Crear SKUs hijos con ejes de variante |
| 6 | **Empresa** | Activar producto en empresa con SKU, canales, stock min/max |

### 7.4 Sub-paneles en `ProductosGlobales` y `Productos`

Filas expandibles con pesta�as de relaciones:

| Pesta�a | Datos mostrados | Acciones |
|---------|----------------|---------|
| Variantes | SKU, nombre, barcode, ejes | Ver / link a gesti�n |
| UdM | Unidad, factor, barcode | Agregar, Eliminar |
| C�digos | Tipo, valor, vigencia | Agregar, Eliminar |
| Atributos | Nombre, valor, vigencia | Asignar (din�mico), Ver |
| Prod. Empresa | SKU, canales, stock | Ver / link a gesti�n |

---

## 8. Persistencia � Migraciones y Configuraciones

### Historial de Migraciones

| Migraci�n | Descripci�n |
|-----------|-------------|
| `20260217065412_BaseCore` | Core: Empresas, Usuarios, UsuarioEmpresas, Monedas |
| `20260217084352_AddRolesTable` | Roles, v�nculo UsuarioEmpresa-Rol |
| `20260217090218_AddAuditLogTable` | Tabla AuditLogs |
| `20260217091857_AddParametroSistemaNumeracionDocumento` | ParametrosSistema, NumeracionesDocumento |
| `20260217153259_AddDefaultAdminUser` | Seed usuario admin por defecto |
| `20260217191851_AddMDMEntities` | MDM inicial: Clientes, Proveedores, Almacenes, UoM legacy |
| `20260217194759_MDM_Enhanced` | MDM avanzado: Products, Variants, Attributes, CompanyProduct |
| `20260218215253_MDM_Refinements` | Refinamientos: PriceLists, Categories, Codes, ProductUoms |
| `20260219000000_INV_MovimientosInventario` | Inventario: Movimientos, Stock |
| `20260223085825_SeedProductStatus` | Seed estados de ciclo de vida de producto |
| `20260223091722_SeedDefaultCatalog` | Seed cat�logo general por defecto |

### Interceptores EF Core

| Interceptor | Funci�n |
|-------------|---------|
| `AuditableEntityInterceptor` | Auto-rellena `FechaCreacion`, `CreadoPor`, `FechaModificacion`, `ModificadoPor`. Genera registros en `AuditLogs` |
| `EntityVersioningInterceptor` | Genera snapshots JSON en `EntityVersions` en cada INSERT/UPDATE/DELETE |

### Repositorio Gen�rico

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
- El `DbContext` aplica filtros globales `Where(e => e.EmpresaId == _empresaId)` autom�ticamente
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

## 10. Estado Funcional por M�dulo

| M�dulo | Backend API | Frontend Blazor | Observaciones |
|--------|------------|-----------------|---------------|
| **Autenticaci�n** | ? Completo | ? Completo | JWT + Stub dev |
| **Empresas** | ? Completo | ? Completo | Multi-tenant |
| **Usuarios / Roles** | ? Completo | ? Completo | |
| **Auditor�a** | ? Completo | ? Completo | Autom�tica v�a interceptor |
| **Par�metros** | ? Completo | ? Completo | |
| **Numeraci�n** | ? Completo | ? Completo | Auto-incremento |
| **Clientes** | ? Completo | ? Completo | |
| **Proveedores** | ? Completo | ? Completo | |
| **Almacenes** | ? Completo | ? Completo | |
| **Cat�logos** | ? Completo | ? (en formularios) | |
| **UdM** | ? Completo | ? Completo | |
| **Marcas** | ? Completo | ? Completo | |
| **Fabricantes** | ? Completo | ? Completo | |
| **Productos (legacy)** | ? Completo | ? **Wizard 6 tabs** | Wizard inline |
| **Productos Globales MDM** | ? Completo | ? **Sub-paneles** | 5 relaciones |
| **CompanyProducts** | ? Completo | ? Completo | |
| **ProductCodes** | ? Completo | ? Integrado | En wizard y sub-paneles |
| **ProductUoms** | ? Completo | ? Integrado | En wizard y sub-paneles |
| **ProductAttributes EAV** | ? Completo | ? Integrado | Campo din�mico por DataType |
| **Variantes** | ? Completo | ? Completo | Con ejes de variante |
| **Categor�as** | ? Completo | ? Completo | Jer�rquicas N-nivel |
| **Atributos Din�micos** | ? Completo | ? Completo | Por industria |
| **Industrias/Reglas** | ? Completo | ?? Sin UI | Endpoint disponible |
| **Listas de Precios** | ? Completo | ? Completo | Con �tems |
| **Inventario Movimientos** | ? Completo | ? Completo | 4 tipos de movimiento |
| **Versionado** | ? Autom�tico | ?? Sin UI | Interceptor activo |
| **Documentos** | ? Entidad/Config | ?? Sin UI | Storage pendiente |
| **Costeo** | ? Entidad/Config | ?? Sin UI | CostingRules + LandedCost |
| **Ventas (Pedidos/Facturas)** | ?? Pendiente | ?? Placeholder | Rama activa |
| **Compras** | ? Completo | ? **CRUD + l�neas + flujo** | Confirmar/Aprobar/Recepci�nParcial/Cerrado/Anular |
| **Recepci�n de Compra** | ? Completo | ? **Recepci�n parcial/total** | Genera Receipt INV + actualiza WAC autom�ticamente |
| **Importaci�n (Landed Cost)** | ? Completo | ? **Gastos + Distribuci�n + Liquidaci�n** | Por Valor/Unidades/Peso/Volumen + ajuste WAC |

---

## 11. Pendientes y Pr�ximos Pasos

### Alta prioridad (rama `GestionImportacion`)
- [x] **M�dulo Compras** � �rdenes de Compra con l�neas de productos, flujo aprobaci�n
- [x] **Recepci�n de OC** � Crear movimiento Receipt autom�tico al recepcionar
- [x] **M�dulo Importaci�n** � Landed Cost aplicado sobre OC, distribuci�n por m�todo
- [ ] **M�dulo Ventas** � Pedidos y Facturas integradas con stock e inventario

### Media prioridad
- [ ] **UI Industrias/Reglas** � P�gina de gesti�n de `Industries` y `ProductIndustryRules`
- [ ] **UI Versiones** � Historial de cambios de entidades
- [ ] **UI Documents** � Gesti�n de documentos multimedia adjuntos a productos
- [ ] **Features Activables** � UI para `CompanyProductFeatures` (LOT, SERIAL, FEFO, etc.)
- [ ] **Costeo Avanzado** � UI para `CostingRules` y `LandedCostProfiles`

### Baja prioridad
- [ ] **Exportaci�n Excel/PDF** en listados
- [ ] **Dashboard** con m�tricas reales (stock bajo m�nimo, movimientos del d�a)
- [ ] **Tests** � Cobertura de servicios Application
- [ ] **CI/CD** pipeline GitHub Actions
- [ ] **Migraci�n almac�n** � UI para `UbicacionesAlmacen`
