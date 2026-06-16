# CODE REVIEW — Rama `diag/fe-runtime-pc-problematica`

**Fecha:** 2026-06-15
**Build:** ✅ 0 errores
**Estado:** Aprobado con 1 fix aplicado

---

## Archivos revisados (8 archivos, +229 líneas / -620 líneas)

| Archivo | Tipo | Líneas |
|---------|------|--------|
| `Shared/DTOs/.../TestConexionResultDto.cs` | **Nuevo** | +14 |
| `Application/Interfaces/IFacturacionElectronicaProvider.cs` | Modificado | +8 |
| `Application/Interfaces/IConfiguracionFEService.cs` | Modificado | +8 |
| `Application/Services/ConfiguracionFEService.cs` | Modificado | +52 |
| `Infrastructure/.../CirrusFacturacionProvider.cs` | Modificado | +67 |
| `Api/Controllers/V1/FacturacionElectronicaController.cs` | Modificado | +23 |
| `Web/Services/FacturacionFEHttpService.cs` | Modificado | +6 |
| `Web/Pages/Configuracion/ConfiguracionFE.razor` | Modificado | +44 |
| `Api/Controllers/V1/DiagnosticoController.cs` | **Eliminado** | -416 |
| Archivos temporales movidos a `docs/diagnostics/` y `tools/dev/` | Movidos | -201 |

---

## Validación contra criterios de aceptación

| # | Criterio | Estado | Evidencia |
|---|----------|--------|-----------|
| 1 | No se modificaron entidades FE | ✅ | `git diff` no muestra cambios en `Domain/Entities/FE/` ni `VTA/` |
| 2 | No se modificó DbContext | ✅ | `git diff` no muestra cambios en `Persistence/Context/` |
| 3 | No se modificaron mappings EF | ✅ | `git diff` no muestra cambios en `Persistence/Configurations/` |
| 4 | No se crearon migraciones | ✅ | `git diff` no muestra cambios en `Migrations/` |
| 5 | No se exponen secretos en DTOs | ✅ | `TestConexionResultDto` no tiene campos de secretos. `ConfiguracionFEDto` existente solo expone `TieneClientSecret` (bool) |
| 6 | TestConexionAsync no modifica datos | ✅ | Solo llama `ObtenerTokenAsync` (GET) y `ObtenerCufdAsync` (GET). No hay `SaveChangesAsync` ni escritura |
| 7 | Endpoint valida tenant EmpresaId desde JWT | ✅ | `ConfiguracionFEService.TestConexionAsync` llama a `ObtenerEmpresaId()` y valida `config.EmpresaId != empresaId` |
| 8 | Config de otra empresa no puede probarse | ✅ | Validación explícita: si `config.EmpresaId != empresaId` devuelve `Result.Failure("No tiene permisos...")` |
| 9 | Errores de Cirrus devuelven mensaje controlado | ✅ | Toda excepción capturada con try/catch. `DetalleTecnico` solo contiene `ex.GetType().Name`, no stack trace completo |
| 10 | DiagnosticoController responde 404 fuera de Dev | ✅ | Eliminado completamente. No más endpoints `/api/v1/diagnostico/*` |
| 11 | dotnet build 0 errores | ✅ | `Compilación correcto con 0 errores` |

---

## Riesgos detectados

### 🔴 Riesgo menor (corregido durante review)

**Bug: Stopwatch.Stop() prematuro en CirrusFacturacionProvider.TestConexionAsync**

El `sw.Stop()` se llamaba después de `ObtenerTokenAsync`, antes de `ObtenerCufdAsync`. Esto causaba que el tiempo de respuesta (`TiempoRespuestaMs`) solo midiera el token, no el CUFD.

**Fix aplicado:** Se movió `sw.Stop()` después de `ObtenerCufdAsync`. En el path de error del token, se llama `sw.Stop()` antes del return.

### 🟡 Riesgo bajo (observación)

**1. `IEnumerable<IFacturacionElectronicaProvider>` en ConfiguracionFEService**

Si en el futuro se registran múltiples providers (ej: AgoraFC, SIAT_DIRECTO), la resolución por `FirstOrDefault` con `CodigoProveedor` funciona correctamente. Pero si el provider Cirrus no es el único, habrá múltiples instancias en memoria. Esto es esperado y aceptable.

**Acción:** Ninguna. El patrón es correcto.

**2. `HttpClientFactory` usa named client "CirrusFacturacion"**

El provider usa `_httpClientFactory.CreateClient("CirrusFacturacion")`. Esto está registrado en `DependencyInjection.cs` como `services.AddHttpClient("CirrusFacturacion")`. Correcto.

**3. `DiagnosticoController` eliminado**

Se eliminó completamente el controller de diagnóstico. Los endpoints `/api/v1/diagnostico/*` ya no existen. Los archivos temporales (`DIAG-RUNTIME-PC-BASE.json`, `DIAG-FE-PC-BASE.json`, `COMPARAR-RUNTIME-FE.ps1`, `REPORTE-FORENSE-FE.md`) fueron movidos a `docs/diagnostics/` y `tools/dev/`. No hay riesgo de exposición en producción.

---

## Resumen de cambios netos

```
+229 líneas (funcionalidad productiva)
-620 líneas (diagnóstico temporal eliminado/movido)
= -391 líneas netas
```

---

## Recomendación final

**✅ APROBADO para merge**

La rama está limpia. Todos los cambios son seguros para producción:

1. `TestConexionAsync` es readonly (no escribe en BD)
2. Valida tenant por JWT
3. No expone secretos
4. No modifica entidades, DbContext, mappings ni migraciones
5. Build 0 errores
6. DiagnosticoController eliminado — no queda exposición de runtime/DB

**Instrucciones de merge:**
```bash
git checkout FE_Agora
git merge diag/fe-runtime-pc-problematica
git push origin FE_Agora
```
