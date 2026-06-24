# FE-S01 — DOMAIN LAYER RESULTADO

> **Proyecto:** AgoraHub360 ERP
> **Fecha:** 2026-06-10
> **Sprint:** FE-S01 — Domain Layer para Facturación Electrónica
> **Estado:** COMPLETADO

---

## 1. Resumen Ejecutivo

Este sprint implementó solo **Domain Layer** para el módulo de Facturación Electrónica.

No se implementó Persistence, Application, Infrastructure, API ni Web.

### Decisión arquitectónica clave

Se eliminaron los enums `ProveedorFacturacionElectronica` y `AmbienteFacturacion` del diseño original. En su lugar, ambos son **catálogos en base de datos** (entidades `AuditableEntity`) sin hardcoding de proveedor ni ambiente en el código. La selección, activación y configuración se resuelven desde BD.

---

## 2. Archivos Creados (8 archivos)

### Entidades de dominio (4 archivos)

| Archivo | Tipo | Herencia |
|---|---|---|
| `src/AgoraHub360.ERP.Domain/Entities/FE/ProveedorFacturacionElectronica.cs` | Catálogo global | `AuditableEntity` |
| `src/AgoraHub360.ERP.Domain/Entities/FE/AmbienteFacturacionElectronica.cs` | Catálogo global | `AuditableEntity` |
| `src/AgoraHub360.ERP.Domain/Entities/FE/ConfiguracionFacturacionElectronica.cs` | Tenant-aware | `TenantEntity` |
| `src/AgoraHub360.ERP.Domain/Entities/FE/AuditoriaFacturacion.cs` | Tenant-aware | `TenantEntity` |

### Interfaces de repositorio (4 archivos)

| Archivo | Propósito |
|---|---|
| `src/AgoraHub360.ERP.Domain/Interfaces/IConfiguracionFERepository.cs` | CRUD configuración FE por empresa |
| `src/AgoraHub360.ERP.Domain/Interfaces/IAuditoriaFERepository.cs` | Solo escritura + consulta histórica |
| `src/AgoraHub360.ERP.Domain/Interfaces/IProveedorFERepository.cs` | Catálogo global de proveedores |
| `src/AgoraHub360.ERP.Domain/Interfaces/IAmbienteFERepository.cs` | Catálogo global de ambientes |

---

## 3. Archivos Modificados

```
No se modificaron archivos existentes.
```

Ninguna entidad, enum, servicio, controlador, DTO o configuración existente fue modificada.

---

## 4. Entidades Creadas

### ProveedorFacturacionElectronica

| Atributo | Valor |
|---|---|
| Tipo | Catálogo global |
| Herencia | `AuditableEntity` |
| Namespace | `AgoraHub360.ERP.Domain.Entities.FE` |
| Propósito | Catálogo de proveedores FE (Cirrus, AgoraFC, SIATDirecto) |
| PK | `int Id` |
| Campos clave | `Codigo`, `Nombre`, `RequierePosToken`, `SoportaAnulacion`, `SoportaConsultaEstado`, `SoportaModoOffline`, `ClaseProvider` |
| Riesgos | Ninguno — no hay FK desde empresa, no hay seed data |

### AmbienteFacturacionElectronica

| Atributo | Valor |
|---|---|
| Tipo | Catálogo global |
| Herencia | `AuditableEntity` |
| Namespace | `AgoraHub360.ERP.Domain.Entities.FE` |
| Propósito | Catálogo de ambientes (TEST, PRODUCCION) |
| PK | `int Id` |
| Campos clave | `Codigo`, `Nombre`, `EsProduccion` |
| Riesgos | Ninguno — no hay seed data |

### ConfiguracionFacturacionElectronica

| Atributo | Valor |
|---|---|
| Tipo | Tenant-aware |
| Herencia | `TenantEntity` |
| Namespace | `AgoraHub360.ERP.Domain.Entities.FE` |
| Propósito | Configuración FE por empresa con credenciales cifradas |
| PK | `int Id` |
| FKs | `ProveedorFacturacionElectronicaId`, `AmbienteFacturacionElectronicaId` |
| Campos clave | `ClientSecretEncrypted` (cifrado), `PosTokenEncrypted` (cifrado), `TokenUrl`, `ApiManagementUrl`, `ApiBillingUrl`, `ActivityCode`, `NitEmisor`, `SucursalFiscal`, `PuntoVentaFiscal`, `EsConfiguracionActiva`, `TimeoutSegundos` |
| Riesgos | Alto — si `EsConfiguracionActiva` no se controla bien, pueden quedar dos activas. La lógica de unicidad se implementará en Application. |

### AuditoriaFacturacion

| Atributo | Valor |
|---|---|
| Tipo | Tenant-aware |
| Herencia | `TenantEntity` |
| Namespace | `AgoraHub360.ERP.Domain.Entities.FE` |
| Propósito | Auditoría de emisiones/anulaciones/consultas FE |
| PK | `long Id` |
| Campos clave | ProveedorCodigo (snapshot), AmbienteCodigo (snapshot), BillUuid, Cuf, EstadoSiat, MensajeError, UsuarioId, TiempoRespuestaMs, Exitoso |
| Riesgos | Bajo — solo escritura. Sin FK a catálogos (snapshot textual) |

---

## 5. Interfaces Creadas

| Interfaz | Métodos | Tenant-aware |
|---|---|---|
| `IConfiguracionFERepository` | ObtenerActivaPorEmpresaAsync, ListarPorEmpresaAsync, ObtenerPorIdAsync, AgregarAsync, ActualizarAsync | Sí |
| `IAuditoriaFERepository` | RegistrarAsync, ListarPorFacturaAsync, ListarPorEmpresaAsync | Sí |
| `IProveedorFERepository` | ListarActivosAsync, ObtenerPorIdAsync, ObtenerPorCodigoAsync | No (global) |
| `IAmbienteFERepository` | ListarActivosAsync, ObtenerPorIdAsync, ObtenerPorCodigoAsync | No (global) |

---

## 6. Validación de Entidades Existentes

| Entidad/Enum | Ruta | Estado | ¿Modificada? |
|---|---|---|---|
| `FacturaVenta` | `Domain/Entities/VTA/FacturaVenta.cs` | ✅ Existe con BillUuid, Cuf, Cufd, SiatQr, EnlacePdf, EnlaceXml, ActivityCode, EstadoFactura, EstadoSiat | No |
| `FacturaVentaDetalle` | `Domain/Entities/VTA/FacturaVentaDetalle.cs` | ✅ Existe con ItemCode | No |
| `EstadoSiatFactura` | `Domain/Enums/EstadoSiatFactura.cs` | ✅ Existe con valores: NoEnviada, Pendiente, Validada, Rechazada, Anulada | No |
| `EstadoFacturaVentaComercial` | `Domain/Enums/EstadoFacturaVentaComercial.cs` | ✅ Existe con valores: Borrador, Generada, Anulada | No |
| `EstadoFacturaVenta` | `Domain/Enums/EstadoFacturaVenta.cs` | ✅ Existe con valores: NoGenerada, Pendiente, Generada, Anulada | No |

### Brechas detectadas

| Brecha | Impacto | Acción |
|---|---|---|
| `FacturaVenta` no tiene campo `Cuf` de tipo específico (es string?) | Bajo | Validar formato Cirrus en FE-S01 |
| `EstadoSiatFactura` no tiene mapeo directo a estados Cirrus | Medio | Documentar mapeo en FE-S02 |

---

## 7. Validación Anti-Hardcoding

| Requisito | Cumplido |
|---|---|
| No se creó enum de proveedor | ✅ Cumplido |
| No se creó enum de ambiente | ✅ Cumplido |
| Proveedor será catálogo en BD | ✅ Diseñado como `ProveedorFacturacionElectronica : AuditableEntity` |
| Ambiente será catálogo en BD | ✅ Diseñado como `AmbienteFacturacionElectronica : AuditableEntity` |
| Configuración activa será por empresa | ✅ `ConfiguracionFacturacionElectronica : TenantEntity` con `EsConfiguracionActiva` |
| URLs y credenciales vendrán desde BD | ✅ Campos en `ConfiguracionFacturacionElectronica` |

---

## 8. Resultado Build

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Todos los proyectos compilaron correctamente.

---

## 9. Riesgos o Pendientes para FE-S02

| Pendiente | Sprint | Descripción |
|---|---|---|
| `ICifradoService` | FE-S02 | Crear interfaz en Application para cifrar/descifrar ClientSecret y PosToken |
| `AesCifradoService` | FE-S02 | Implementar AES-256-CBC + IV aleatorio en Infrastructure |
| Contratos Application | FE-S02 | Crear servicios de aplicación FE |
| DTOs Shared | FE-S02 | Crear DTOs para FE en `Shared/DTOs/FacturacionElectronica/` |
| Provider pattern | FE-S02 | Crear `IFacturacionElectronicaProvider` en Application |
| Persistence | FE-S03 | DbSets, configuraciones EF, migración esquema `cfg` |
| Seed inicial | FE-S03 | Sembrar proveedores (CIRRUS) y ambientes (TEST, PRODUCCION) |
| Validar unicidad EsConfiguracionActiva | FE-S03 | Solo una activa por empresa (lógica en Application) |
| TenantRequiredMiddleware | FE-S03 | Añadir ruta FE a exempt paths si es necesario |

---

## 10. Resultado Final

```
RESULTADO FE-S01: COMPLETADO
```

```
SIGUIENTE PROMPT RECOMENDADO:
FE-S02 — Contratos Application, DTOs Shared y Seguridad de Secretos para Facturación Electrónica
```

---
