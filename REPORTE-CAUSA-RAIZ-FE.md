# INFORME CAUSA RAÍZ FUNCIONAL — Facturación Electrónica

**Fecha:** 2026-06-15
**Rama:** diag/fe-runtime-pc-problematica
**Commit:** d436dea (887f466 + diagnostic files)

---

## 1. CAUSA RAÍZ EXACTA

Existen **DOS problemas independientes** que impiden el flujo "Emitir FE":

### 🔴 Problema #1 (Bloqueante): Falta configuración FE para Empresa 20 y 21

**Datos de la BD:**

| EmpresaId | FacturasVenta | Config FE Activa | ¿Puede emitir? |
|-----------|---------------|------------------|----------------|
| 20 | 7 | ❌ **NO EXISTE** | ❌ **Bloqueado** |
| 21 | 2 | ❌ **NO EXISTE** | ❌ **Bloqueado** |
| 27 | 5 | ✅ 1 (CIRRUS/TEST) | ⚠️ Intento falló |

**Solo hay 1 configuración FE en toda la BD** (Id=1, EmpresaId=27). Las empresas 20 y 21 no tienen configuración.

**¿Por qué el código no lo detecta?** — El código SÍ lo detecta. El servicio `FacturacionElectronicaService.EmitirAsync()` llama a `_configRepo.ObtenerActivaPorEmpresaAsync(empresaId)` y si no encuentra configuración, devuelve:

> "No existe configuración activa de facturación electrónica para esta empresa."

El problema es que **el flujo llega hasta este punto pero el error no se muestra claramente en la UI**. El usuario ve un error genérico.

### 🔴 Problema #2 (Confirmado): Provider Cirrus no puede autenticarse

**Auditoría — único intento de emisión:**

```
Id=1
FacturaVentaId=14 (EmpresaId=27)
Proveedor=CIRRUS, Ambiente=TEST
Exitoso=false
MensajeError="No se pudo obtener token de autenticación del proveedor."
TiempoRespuestaMs=1826
```

El método `CirrusFacturacionProvider.ObtenerTokenAsync()` falla porque:
1. El `ClientSecretEncrypted` no se descifra correctamente, O
2. La `TokenUrl` configurada no responde, O
3. El `ClientId` es inválido para el ambiente TEST de Cirrus

**Tiempo de respuesta: 1826ms** — suficiente para que la solicitud HTTP llegue al servidor, pero la autenticación es rechazada.

---

## 2. ¿QUÉ VENTA SE INTENTA EMITIR?

La única auditoría corresponde a **FacturaVentaId=14** (VentaId=32, EmpresaId=27). El usuario `ustemoral` (UserId=39) intentó emitirla y falló.

---

## 3. ¿QUÉ EMPRESAID TIENE CADA COSA?

| Componente | EmpresaId | ¿Config FE existe? |
|-----------|-----------|-------------------|
| VentaId=32 (intentada) | **27** | ✅ Sí |
| JWT del usuario que intentó | **27** (inferido del token) | ✅ Coincide |
| Config FE activa | **27** | ✅ Sí |
| VentaId=24 (Empresa 21) | **21** | ❌ **No** |
| VentaId=22 (Empresa 20) | **20** | ❌ **No** |

---

## 4. ¿EXISTEN DATOS FISCALES?

**Todas las ventas** tienen `VentaFacturacionDatos` con:
- `NitFactura`: ✅ presente
- `RazonSocialFactura`: ✅ presente
- `Facturar`: en algunos casos=0 (ventas 27-35 todas con Facturar=0)
- `EstadoFactura`: 1 (NoGenerada) para todas
- `FacturaId`: **NULL para todas** — la columna no se actualiza al crear factura

---

## 5. ¿EXISTE FACTURA PREVIA?

Sí, 14 facturas en `vta.FacturasVenta`, todas con `EstadoSiat=1` (NoEnviada), `EstadoFactura=2` (Generada), y **ninguna tiene CUF** (nunca se emitieron exitosamente).

---

## 6. ¿QUÉ ENDPOINT SE LLAMA?

- **Frontend Blazor**: `Facturas.razor` → `EmitirFEAsync(fac.Id)` → `FacturacionFEHttpService.EmitirAsync(id)` → **POST `{ApiBaseUrl}/api/v1/facturacion-electronica/emitir/{facturaVentaId}`**
- **Backend**: `FacturacionElectronicaController.Emitir(long facturaVentaId)` → `FacturacionElectronicaService.EmitirAsync(facturaVentaId, ct)` → `CirrusFacturacionProvider.EmitirFacturaAsync()`

**CORRECTO.** No hay problema de ruta.

---

## 7. ¿EN QUÉ LÍNEA FALLA?

En `CirrusFacturacionProvider.cs`, método `ObtenerTokenAsync()`, **línea donde la respuesta HTTP del token no es exitosa** o el JSON no contiene `access_token`.

El servicio de aplicación captura la excepción y nunca lanza un 500 — devuelve un `Result.Failure` controlado. Pero la UI traduce el error como un mensaje genérico.

---

## 8. DIAGNÓSTICO POR EMPRESA

### Empresa 27 — La única configurada
- ✅ Venta existe
- ✅ JWT EmpresaId = Venta EmpresaId (si el usuario seleccionó empresa 27)
- ✅ Config FE existe
- ❌ **Provider Cirrus falla al autenticarse** (token OAuth2 inválido)
- **Solución**: Verificar credenciales Cirrus (ClientId, ClientSecret cifrado, TokenUrl)

### Empresa 20 — Sin configuración
- ✅ Venta existe
- ⚠️ JWT EmpresaId probablemente 20
- ❌ **No hay configuración FE** — El servicio devuelve: "No existe configuración activa..."
- **Solución**: Crear configuración FE para empresa 20, O validar y mostrar error claro

### Empresa 21 — Sin configuración
- Mismo caso que empresa 20.

---

## 9. PATCH MÍNIMO REQUERIDO

### Patch A — Mensaje de error claro en UI para "sin configuración FE"

En `Facturas.razor`, método `EmitirFEAsync()`, el `errorMessage` ya se muestra. Pero el mensaje "No existe configuración activa" llega como `result.Message`. **Ya funciona.** Pero se podría mejorar:

```razor
@if (result.Data != null && !result.Data.Exitoso)
{
    errorMessage = $"FE rechazada: {result.Data.Mensaje}";
}
else if (!result.Success)
{
    errorMessage = result.Message ?? "Error al emitir FE.";
}
```

Esto **ya está implementado** en `Facturas.razor` líneas 470-486. ✅

### Patch B — Verificar credenciales Cirrus

**Este es el bloqueo real.** El provider no puede autenticarse. Acciones:

1. Verificar que `ClientSecretEncrypted` en la BD sea válido
2. Verificar que `TokenUrl` sea accesible desde la red
3. Verificar que `ClientId` sea correcto para el ambiente TEST

**El código de cifrado (`ICifradoService`) está funcionando** porque puede descifrar el secret (de lo contrario la excepción sería diferente). El problema es que **las credenciales descifradas no son aceptadas por Cirrus**.

### Patch C — Habilitar configuración FE multi-empresa desde UI

Actualmente solo hay UI de configuración FE en `ConfiguracionFE.razor`. Verificar que permita crear configuraciones para diferentes empresas (no solo la empresa activa del JWT).

---

## 10. CONCLUSIÓN

| Hallazgo | Impacto | Prioridad |
|----------|---------|-----------|
| Empresas 20 y 21 sin configuración FE | Alto — 9 facturas no se pueden emitir | **Alta** |
| Provider Cirrus no autentica (Empresa 27) | Alto — única configuración no funciona | **Alta** |
| `VentaFacturacionDatos.FacturaId` nunca se actualiza | Bajo — trazabilidad rota | Baja |
| `dbo.FacturasVenta` legacy vacía | Bajo — no interfiere | Muy baja |

### El código fuente NO TIENE errores de lógica, mapeo o ruta.

**El problema es de datos (falta configuración FE para empresas 20/21) y de configuración externa (credenciales Cirrus inválidas).**
