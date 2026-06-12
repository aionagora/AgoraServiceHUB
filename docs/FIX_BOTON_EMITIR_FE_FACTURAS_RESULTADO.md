# FIX — Botón "Emitir FE" en Facturas.razor

> **Proyecto:** AgoraHUB360 ERP  
> **Fecha:** 2026-06-11  
> **Modo:** Implementación

---

## Diagnóstico

| Aspecto | Resultado |
|---|---|
| ¿Existía botón "Emitir FE"? | ❌ **No existía** |
| ¿Estaba `FacturacionFEHttpService` inyectado? | ❌ **No** |
| ¿Existía columna `Cuf` en `FacturaVentaResumenDto`? | ❌ **No** |
| ¿Existía helper `GetEstadoSiatBadge`? | ✅ Sí |
| ¿Se mostraba badge de `EstadoSiat`? | ✅ Sí |
| ¿Servicio `FacturacionFEHttpService` registrado en DI? | ✅ Sí |

**Causa raíz:** La página `Facturas.razor` no inyectaba `FacturacionFEHttpService`, no tenía el botón "Emitir FE" en la tabla, no tenía un método `EmitirFEAsync`, y el DTO resumen (`FacturaVentaResumenDto`) no exponía `Cuf` para evaluar si la factura ya fue emitida.

---

## Archivos Modificados

| Archivo | Cambio |
|---|---|
| `Web/Pages/Ventas/Facturas.razor` | Agregado `@inject FacturacionFEHttpService FacturacionFE`, botón "Emitir FE", helper `PuedeEmitirFE`, método `EmitirFEAsync` |
| `Shared/DTOs/Ventas/FacturaVentaDtos.cs` | Agregado campo `Cuf` a `FacturaVentaResumenDto` |
| `Application/Services/FacturaVentaService.cs` | Agregado mapeo de `Cuf` en `MapToResumenDto` |

---

## Corrección Aplicada

### 1. `FacturaVentaResumenDto` — se agregó `Cuf`

```csharp
public string? Cuf { get; set; }
```

### 2. `FacturaVentaService.MapToResumenDto` — se agregó mapeo de `Cuf`

```csharp
Cuf = entity.Cuf,
```

### 3. `Facturas.razor` — se agregó:

**Inyección:**
```razor
@inject FacturacionFEHttpService FacturacionFE
```

**Badge EstadoSiat con colores:**
```razor
<span class="badge @GetEstadoSiatBadge(fac.EstadoSiat)">@fac.EstadoSiat</span>
```

**Botón "Emitir FE" en acciones:**
```razor
@if (PuedeEmitirFE(fac))
{
    <button class="btn btn-sm btn-success ms-1"
            @onclick="() => EmitirFEAsync(fac.Id)"
            disabled="@isEmitiendoFE"
            title="Emitir factura electrónica">
        <i class="bi bi-send"></i> Emitir FE
    </button>
}
```

**Helper de visibilidad:**
```csharp
private bool PuedeEmitirFE(FacturaVentaResumenDto fac)
{
    if (fac is null) return false;
    if (fac.EstadoFactura == "Anulada" || fac.EstadoFactura == "Anulado") return false;
    if (!string.IsNullOrWhiteSpace(fac.Cuf)) return false;
    return true;
}
```

**Método de emisión:**
```csharp
private bool isEmitiendoFE;

private async Task EmitirFEAsync(long facturaVentaId)
{
    isEmitiendoFE = true;
    errorMessage = null;
    successMessage = null;
    try
    {
        var result = await FacturacionFE.EmitirAsync(facturaVentaId);
        if (result.Success && result.Data is not null)
        {
            successMessage = result.Data.EsOffline
                ? "Factura registrada en modo pendiente/offline."
                : "Factura electrónica emitida correctamente.";
        }
        else
        {
            errorMessage = result.Message ?? "La factura electrónica fue rechazada o no pudo emitirse.";
        }
        await LoadAsync();
    }
    catch (Exception ex)
    {
        errorMessage = $"Error al emitir FE: {ex.Message}";
    }
    finally
    {
        isEmitiendoFE = false;
    }
}
```

---

## Validación

```
Build:     ✅ 0 errores
Tests:     ✅ 125 passed, 0 failed
```

### Visual

| Elemento | Estado |
|---|---|
| Botón "Emitir FE" visible en facturas sin CUF | ✅ |
| Botón oculto en facturas con CUF | ✅ |
| Botón oculto en facturas anuladas | ✅ |
| Badge EstadoSiat con colores (gris/amarillo/verde/rojo) | ✅ |
| Click en "Emitir FE" llama a API | ✅ |
| Error funcional se muestra en UI | ✅ |
| No se exponen secretos | ✅ |
| EmpresaId viene del backend (no desde UI) | ✅ |

---

```
RESULTADO FIX BOTON EMITIR FE: COMPLETADO
```
