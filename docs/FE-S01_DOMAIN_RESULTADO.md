# FE-S01 — CAPA DE DOMINIO PARA FACTURACIÓN ELECTRÓNICA

> **Proyecto:** AgoraHUB360 ERP  
> **Fecha:** 2026-06-11  
> **Modo:** Implementación  
> **Sprint:** FE-S01

---

## 1. Resumen Ejecutivo

**ESTADO: COMPLETADO**

Se implementaron todas las entidades de dominio, enums e interfaces de repositorio necesarias para el módulo de Facturación Electrónica multi-proveedor. Se verificó que las entidades SIAT existentes (`FacturaVenta`, `FacturaVentaDetalle`, `EstadoSiatFactura`) ya contenían los campos fiscales necesarios.

---

## 2. Entidades Creadas

### Catálogos Globales (AuditableEntity)

| Entidad | Archivo | Propósito |
|---|---|---|
| `ProveedorFacturacionElectronica` | `Domain/Entities/FE/ProveedorFacturacionElectronica.cs` | Catálogo de proveedores FE (CIRRUS, etc.) |
| `AmbienteFacturacionElectronica` | `Domain/Entities/FE/AmbienteFacturacionElectronica.cs` | Catálogo de ambientes (TEST, PRODUCCION) |

### Entidades Tenant-aware (TenantEntity)

| Entidad | Archivo | Propósito |
|---|---|---|
| `ConfiguracionFacturacionElectronica` | `Domain/Entities/FE/ConfiguracionFacturacionElectronica.cs` | Configuración FE por empresa (credenciales cifradas, URLs, etc.) |
| `AuditoriaFacturacion` | `Domain/Entities/FE/AuditoriaFacturacion.cs` | Auditoría de operaciones FE (emisión, anulación, consulta) |

---

## 3. Interfaces de Repositorio Creadas

| Interfaz | Archivo | Métodos principales |
|---|---|---|
| `IProveedorFERepository` | `Domain/Interfaces/IProveedorFERepository.cs` | `ListarActivosAsync`, `ObtenerPorIdAsync`, `ObtenerPorCodigoAsync` |
| `IAmbienteFERepository` | `Domain/Interfaces/IAmbienteFERepository.cs` | `ListarActivosAsync`, `ObtenerPorIdAsync`, `ObtenerPorCodigoAsync` |
| `IConfiguracionFERepository` | `Domain/Interfaces/IConfiguracionFERepository.cs` | `ObtenerActivaPorEmpresaAsync`, `ListarPorEmpresaAsync`, CRUD completo |
| `IAuditoriaFERepository` | `Domain/Interfaces/IAuditoriaFERepository.cs` | `RegistrarAsync`, `ListarPorFacturaAsync`, `ListarPorEmpresaAsync` |

---

## 4. Entidades SIAT Verificadas (ya existían)

| Entidad | Campos FE | Estado |
|---|---|---|
| `FacturaVenta` | `BillUuid`, `Cuf`, `Cufd`, `SiatQr`, `EnlacePdf`, `EnlaceXml`, `ActivityCode`, `EstadoSiat` | ✅ Existentes |
| `FacturaVentaDetalle` | `ItemCode` | ✅ Existente |
| `EstadoSiatFactura` (enum) | `NoEnviada, Pendiente, Validada, Rechazada, Anulada` | ✅ Existente |
| `EstadoFacturaVenta` (enum) | `NoGenerada, Pendiente, Generada, Anulada` | ✅ Existente |

---

## 5. Detalle de Entidades

### ProveedorFacturacionElectronica

```csharp
public class ProveedorFacturacionElectronica : AuditableEntity
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;       // Ej: "CIRRUS"
    public string Nombre { get; set; } = string.Empty;        // Ej: "Cirrus"
    public string? Descripcion { get; set; }
    public bool RequierePosToken { get; set; }                 // Define si el proveedor necesita POS token
    public bool SoportaAnulacion { get; set; } = true;
    public bool SoportaConsultaEstado { get; set; } = true;
    public bool Activo { get; set; } = true;
    public int Orden { get; set; }
}
```

### AmbienteFacturacionElectronica

```csharp
public class AmbienteFacturacionElectronica : AuditableEntity
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;       // Ej: "TEST", "PRODUCCION"
    public string Nombre { get; set; } = string.Empty;        // Ej: "Pruebas", "Producción"
    public bool Activo { get; set; } = true;
}
```

### ConfiguracionFacturacionElectronica

```csharp
public class ConfiguracuracionFacturacionElectronica : TenantEntity
{
    public int Id { get; set; }
    public string NombreConfiguracion { get; set; } = string.Empty;
    
    // FK a catálogos
    public int ProveedorFacturacionElectronicaId { get; set; }
    public ProveedorFacturacionElectronica? ProveedorFacturacionElectronica { get; set; }
    
    public int AmbienteFacturacionElectronicaId { get; set; }
    public AmbienteFacturacionElectronica? AmbienteFacturacionElectronica { get; set; }
    
    // Credenciales (cifradas en BD)
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecretEncrypted { get; set; } = string.Empty;  // Cifrado con AES
    public string? PosTokenEncrypted { get; set; }                      // Cifrado con AES
    
    // URLs del proveedor
    public string TokenUrl { get; set; } = string.Empty;
    public string ApiManagementUrl { get; set; } = string.Empty;
    public string ApiBillingUrl { get; set; } = string.Empty;
    
    // Configuración fiscal
    public string ActivityCode { get; set; } = string.Empty;
    public string NitEmisor { get; set; } = string.Empty;
    public int TimeoutSegundos { get; set; } = 30;
    
    // Estado
    public bool EsConfiguracionActiva { get; set; }
}
```

### AuditoriaFacturacion

```csharp
public class AuditoriaFacturacion : TenantEntity
{
    public long Id { get; set; }
    public long FacturaVentaId { get; set; }
    public FacturaVenta? FacturaVenta { get; set; }
    
    // Snapshots textuales (no FK para preservar histórico)
    public string ProveedorCodigo { get; set; }
    public string ProveedorNombre { get; set; }
    public string AmbienteCodigo { get; set; }
    public string AmbienteNombre { get; set; }
    
    // Datos de operación
    public string BillUuid { get; set; }
    public string? Cuf { get; set; }
    public EstadoSiatFactura EstadoSiat { get; set; }
    public string? MensajeError { get; set; }
    public string UsuarioId { get; set; }
    public DateTime FechaHora { get; set; }
    public long TiempoRespuestaMs { get; set; }
    public string? CodigoRespuestaProveedor { get; set; }
    public string? DescripcionRespuestaProveedor { get; set; }
    public bool Exitoso { get; set; }
}
```

---

## 6. Estructura de Carpetas Creada

```
src/AgoraHub360.ERP.Domain/
├── Entities/FE/
│   ├── ProveedorFacturacionElectronica.cs   ← Nuevo
│   ├── AmbienteFacturacionElectronica.cs     ← Nuevo
│   ├── ConfiguracionFacturacionElectronica.cs ← Nuevo
│   └── AuditoriaFacturacion.cs              ← Nuevo
├── Interfaces/
│   ├── IProveedorFERepository.cs            ← Nuevo
│   ├── IAmbienteFERepository.cs             ← Nuevo
│   ├── IConfiguracionFERepository.cs        ← Nuevo
│   └── IAuditoriaFERepository.cs            ← Nuevo
├── Enums/                                   ← Sin cambios
└── Common/                                  ← Sin cambios
```

---

## 7. Entidades Verificadas (sin cambios)

```
Domain/Entities/VTA/FacturaVenta.cs          ← ✅ BillUuid, Cuf, Cufd, etc.
Domain/Entities/VTA/FacturaVentaDetalle.cs   ← ✅ ItemCode
Domain/Enums/EstadoSiatFactura.cs            ← ✅ 5 valores
Domain/Enums/EstadoFacturaVenta.cs           ← ✅ 4 valores
Domain/Enums/EstadoFacturaVentaComercial.cs  ← ✅ 3 valores
```

---

## 8. Reglas de Diseño Aplicadas

| Regla | Aplicación |
|---|---|
| Catálogos globales sin tenant | `ProveedorFacturacionElectronica`, `AmbienteFacturacionElectronica` → `AuditableEntity` |
| Config tenant-aware | `ConfiguracionFacturacionElectronica` → `TenantEntity` |
| Auditoría tenant-aware | `AuditoriaFacturacion` → `TenantEntity` |
| Historial preservado | Auditoría guarda snapshots textuales, no FK a catálogos |
| Secretos cifrados | `ClientSecretEncrypted`, `PosTokenEncrypted` (string, se cifran en Application) |
| Sin enums de proveedor/ambiente | Son catálogos en BD, no enums |
| Convención de esquema | Las tablas usarán esquema `cfg` (3 letras minúsculas) |

---

## 9. Validación

```
Build:     ✅ 0 errores
Tests:     ✅ 125 passed, 0 failed
```

---

## 10. Pendientes para FE-S02

| Pendiente | Prioridad |
|---|---|
| Crear `ICifradoService` en Application/Interfaces | Alta |
| Crear `AesCifradoService` en Infrastructure/Services | Alta |
| Crear `IFacturacionElectronicaProvider` en Application/Interfaces | Alta |
| Crear `CirrusFacturacionProvider` en Infrastructure | Alta |
| Definir DTOs de Facturación Electrónica en Shared | Alta |
| Registrar servicios en DI | Alta |

---

```
RESULTADO FE-S01: COMPLETADO
SIGUIENTE PROMPT RECOMENDADO:
FE-S02 — DTOs, contratos Application y cifrado
```
