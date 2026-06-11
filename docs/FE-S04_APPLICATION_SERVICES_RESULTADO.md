# FE-S04 — APPLICATION SERVICES RESULTADO

> **Proyecto:** AgoraHUB360 ERP
> **Fecha:** 2026-06-10
> **Sprint:** FE-S04 — Application Services para Configuración y Emisión de Facturación Electrónica
> **Estado:** COMPLETADO

---

## 1. Resumen Ejecutivo

Este sprint implementó los servicios de aplicación del módulo de Facturación Electrónica.

```
ConfiguracionFEService  — CRUD, cifrado, activación única, validaciones
FacturacionElectronicaService — Emisión, anulación, consulta de estado, auditoría, idempotencia
```

No se creó API, UI ni provider Cirrus.

---

## 2. Archivos Creados (2 archivos)

| Archivo | Propósito |
|---|---|
| `src/AgoraHub360.ERP.Application/Services/ConfiguracionFEService.cs` | Gestión de configuración FE por empresa |
| `src/AgoraHub360.ERP.Application/Services/FacturacionElectronicaService.cs` | Emisión, anulación y consulta de estado FE |

---

## 3. Archivos Modificados (1 archivo)

| Archivo | Cambio |
|---|---|
| `src/AgoraHub360.ERP.Application/DependencyInjection.cs` | +3 líneas: registros `ConfiguracionFEService` y `FacturacionElectronicaService` |

---

## 4. Servicios Implementados

### ConfiguracionFEService

**Dependencias:**
- `IConfiguracionFERepository` — repositorio configuración
- `IProveedorFERepository` — validar proveedor existe
- `IAmbienteFERepository` — validar ambiente existe
- `ICifradoService` — cifrar secretos
- `ICurrentUserService` — obtener EmpresaId
- `IUnitOfWork` — guardar cambios

**Métodos:**

| Método | Descripción |
|---|---|
| `ListarPorEmpresaAsync` | Lista configuraciones de la empresa con proveedor/ambiente |
| `ObtenerPorIdAsync` | Obtiene una configuración validando tenant |
| `CrearAsync` | Crea configuración cifrando secretos, activando/desactivando según corresponda |
| `ActualizarAsync` | Actualiza configuración, cifra secretos solo si vienen nuevos |
| `ActivarAsync` | Activa una config y desactiva las demás de la misma empresa |
| `DesactivarAsync` | Desactiva una configuración |
| `ListarProveedoresAsync` | Lista proveedores activos del catálogo global |
| `ListarAmbientesAsync` | Lista ambientes activos del catálogo global |

### FacturacionElectronicaService

**Dependencias:**
- `IConfiguracionFERepository` — obtener configuración activa
- `IAuditoriaFERepository` — registrar auditoría
- `IEnumerable<IFacturacionElectronicaProvider>` — resolución dinámica de provider
- `IRepository<FacturaVenta>` — cargar/actualizar factura
- `IRepository<FacturaVentaDetalle>` — cargar detalles
- `ICurrentUserService` — obtener EmpresaId
- `IUnitOfWork` — guardar cambios
- `ILogger<FacturacionElectronicaService>` — logging

**Métodos:**

| Método | Descripción |
|---|---|
| `EmitirAsync` | Emisión completa con validaciones, idempotencia, resolución de provider, auditoría |
| `AnularAsync` | Anulación con validación de CUF, motivo, resolución de provider, auditoría |
| `VerificarEstadoAsync` | Consulta de estado con actualización de EstadoSiat, auditoría |

---

## 5. Seguridad de Secretos

| Aspecto | Cumplido |
|---|---|
| `ClientSecret` se cifra con `ICifradoService` en creación | ✅ |
| `PosToken` se cifra con `ICifradoService` en creación | ✅ |
| En actualización, si `ClientSecret` viene null/vacío se conserva el cifrado actual | ✅ |
| En actualización, si `PosToken` viene null/vacío se conserva el cifrado actual | ✅ |
| `ConfiguracionFEDto` no expone `ClientSecretEncrypted` ni `PosTokenEncrypted` | ✅ |
| Solo se expone `TieneClientSecret` (bool) y `TienePosToken` (bool) | ✅ |
| No se loguean secretos | ✅ |

---

## 6. Provider Pattern

| Aspecto | Cumplido |
|---|---|
| Provider se resuelve por `CodigoProveedor` string | ✅ |
| No se usa enum para proveedor | ✅ |
| No se hardcodea Cirrus | ✅ |
| Los providers se inyectan como `IEnumerable<IFacturacionElectronicaProvider>` | ✅ |
| Si no hay provider registrado para el código, se retorna error descriptivo | ✅ |

---

## 7. Idempotencia

| Aspecto | Cumplido |
|---|---|
| `BillUuid` se reutiliza si ya existe en `FacturaVenta.BillUuid` | ✅ |
| `BillUuid` se genera con `Guid.NewGuid()` si no existe (antes de llamar al provider) | ✅ |
| `BillUuid` se persiste incluso si el provider falla (reintento seguro) | ✅ |
| La factura no se marca como emitida si el provider rechaza | ✅ |

---

## 8. Auditoría

| Aspecto | Cumplido |
|---|---|
| Se registra auditoría en éxito | ✅ |
| Se registra auditoría en rechazo del proveedor | ✅ |
| Se registra auditoría en timeout/excepción | ✅ |
| No se guardan secretos en auditoría | ✅ |
| No se guarda response body completo | ✅ |
| Se incluye: ProveedorCodigo, AmbienteCodigo, BillUuid, Cuf, EstadoSiat, TiempoRespuestaMs | ✅ |

---

## 9. Validaciones de Negocio

| Validación | Implementada en |
|---|---|
| EmpresaId desde JWT (no desde request) | Todos los servicios |
| Validar que config pertenece a la empresa | `ObtenerPorIdAsync`, `ActualizarAsync`, `ActivarAsync`, `DesactivarAsync` |
| Validar estado emitible (solo Borrador + NoEnviada/Pendiente) | `EmitirAsync` |
| ItemCode obligatorio en todos los detalles | `EmitirAsync` |
| URL debe iniciar con https:// | `CrearAsync` |
| Timeout entre 5 y 60 segundos | `CrearAsync` |
| PosToken obligatorio si proveedor.RequierePosToken | `CrearAsync` |
| Motivo anulación mínimo 5 caracteres | `AnularAsync` |
| CUF obligatorio para anular/consultar | `AnularAsync`, `VerificarEstadoAsync` |
| Activación única por empresa | `ActivarAsync`, `CrearAsync`, `ActualizarAsync` |

---

## 10. Resultado Build

```
Build succeeded. 0 errors, 0 warnings.
```

---

## 11. Resultado Tests

```
Tests: total: 125; errors: 0; passed: 125; skipped: 0
```

(+6 tests nuevos de AesCifradoService desde FE-S02, todos pasan)

---

## 12. Pendientes para FE-S05

| Pendiente | Sprint |
|---|---|
| API Controller `FacturacionElectronicaController` | FE-S05 |
| Endpoint `GET /api/v1/facturacion-electronica/configuracion` | FE-S05 |
| Endpoint `POST /api/v1/facturacion-electronica/emitir/{facturaVentaId}` | FE-S05 |
| Endpoint `POST /api/v1/facturacion-electronica/anular/{facturaVentaId}` | FE-S05 |
| Endpoint `GET /api/v1/facturacion-electronica/estado/{facturaVentaId}` | FE-S05 |
| Endpoint `GET /api/v1/facturacion-electronica/proveedores` | FE-S05 |
| Endpoint `GET /api/v1/facturacion-electronica/ambientes` | FE-S05 |
| Swagger documentation | FE-S05 |
| Manejo HTTP de errores con ApiResponse<T> | FE-S05 |
| Registrar ruta FE en TenantRequiredMiddleware exempt paths | FE-S05 |

---

## 13. Resultado Final

```
RESULTADO FE-S04: COMPLETADO
```

```
SIGUIENTE PROMPT RECOMENDADO:
FE-S05 — API REST Facturación Electrónica
```

---
