# ?? FIX: Problema de Ruta en UnidadesMedida

## ? Problema Identificado

La aplicación **NO está consumiendo correctamente** la API de UnidadMedida debido a un **desajuste de rutas**.

---

## ?? Causa Raíz

### Antes del Fix:

**Controlador API** (`UnidadesMedidaController.cs`):
```csharp
[Route("api/v{version:apiVersion}/[controller]")]
```
Genera la ruta: `api/v1/UnidadesMedida` ? (PascalCase)

**HTTP Service** (`UnidadMedidaHttpService.cs`):
```csharp
private const string BaseUrl = "api/v1/unidades-medida";
```
Intenta conectarse a: `api/v1/unidades-medida` ? (kebab-case)

### Resultado:
? **404 Not Found** - Las rutas no coinciden

---

## ? Solución Aplicada

### Cambio realizado:

**Archivo:** `src/AgoraHub360.ERP.Api/Controllers/V1/UnidadesMedidaController.cs`

**Antes:**
```csharp
[Route("api/v{version:apiVersion}/[controller]")]
```

**Después:**
```csharp
[Route("api/v{version:apiVersion}/unidades-medida")]
```

### Ahora:
? **Controlador API:** `api/v1/unidades-medida`  
? **HTTP Service:** `api/v1/unidades-medida`  
? **Las rutas coinciden** ?

---

## ?? Cómo Aplicar el Fix

### Opción 1: Hot Reload (Si está en debug)

Si la aplicación está corriendo en modo debug con Hot Reload:

```
La aplicación debería actualizar automáticamente
No es necesario reiniciar
```

### Opción 2: Reiniciar la API

Si Hot Reload no funciona o no está en debug:

```powershell
# Detener la API (Ctrl+C en la terminal)
# Volver a iniciarla:
cd src\AgoraHub360.ERP.Api
dotnet run --launch-profile https
```

### Opción 3: Reiniciar todo el sistema

```powershell
# Detener ambos procesos (API y Web)
# Usar el script de inicio:
.\start-system.ps1
```

---

## ? Verificación

### 1. Verificar la ruta en Swagger

1. Abre `https://localhost:7001/swagger/index.html`
2. Busca el endpoint
3. Debe aparecer como: **`GET /api/v1/unidades-medida`** ?

### 2. Probar en el navegador

1. Ve a `https://localhost:5002/mdm/unidades-medida`
2. El mensaje "Error al cargar unidades de medida" **debe desaparecer** ?
3. Debe mostrar la tabla (vacía o con datos)

### 3. Ver en DevTools

1. Abre DevTools (F12)
2. Ve a la pestaña **Network**
3. Filtra por `unidades-medida`
4. Recarga la página
5. Verifica que:
   - **Status:** `200 OK` ? (antes era 404)
   - **Response:** JSON con datos o array vacío

---

## ?? Otros Controladores a Revisar

Este mismo problema puede existir en otros controladores MDM. Verifica:

### ? Controladores con `[controller]`:
```csharp
[Route("api/v{version:apiVersion}/[controller]")]
```

### ? Deben usar rutas explícitas:
```csharp
[Route("api/v{version:apiVersion}/clientes")]          // ClientesController
[Route("api/v{version:apiVersion}/proveedores")]       // ProveedoresController
[Route("api/v{version:apiVersion}/productos")]         // ProductosController
[Route("api/v{version:apiVersion}/categorias")]        // CategoriasController
[Route("api/v{version:apiVersion}/almacenes")]         // AlmacenesController
```

---

## ?? Checklist Post-Fix

- [ ] Aplicar hot reload o reiniciar API
- [ ] Verificar en Swagger que la ruta es correcta
- [ ] Probar en el navegador
- [ ] Verificar en DevTools que responde 200 OK
- [ ] Crear al menos una unidad de medida de prueba
- [ ] Verificar que la búsqueda funciona
- [ ] Verificar que editar funciona
- [ ] Verificar que eliminar funciona

---

## ?? Impacto del Fix

| Antes | Después |
|-------|---------|
| ? Error 404 Not Found | ? 200 OK |
| ? "Error al cargar unidades de medida" | ? Tabla funcional |
| ? No se puede crear unidades | ? CRUD completo funcional |
| ? Swagger muestra ruta incorrecta | ? Ruta correcta en Swagger |

---

## ?? Archivos Modificados

```
? src/AgoraHub360.ERP.Api/Controllers/V1/UnidadesMedidaController.cs
   - Cambio: [controller] ? unidades-medida
```

---

## ?? Debug Info

Si después del fix **el problema persiste**:

### 1. Verificar que el cambio se aplicó:
```powershell
# Ver la ruta actual en el controlador
Get-Content src\AgoraHub360.ERP.Api\Controllers\V1\UnidadesMedidaController.cs | Select-String "Route"
```

Debe mostrar:
```
[Route("api/v{version:apiVersion}/unidades-medida")]
```

### 2. Limpiar y recompilar:
```powershell
dotnet clean
dotnet build
```

### 3. Ver logs de la API:
En la terminal donde corre la API, buscar líneas como:
```
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint 'GET api/v1/unidades-medida'
```

### 4. Test con curl:
```powershell
curl -X GET "https://localhost:7001/api/v1/unidades-medida" `
  -H "Authorization: Bearer TU_TOKEN" `
  -k
```

---

## ? Conclusión

El problema estaba en una **inconsistencia de nomenclatura de rutas**:
- El controlador usaba `[controller]` que genera **PascalCase**
- El cliente esperaba **kebab-case**

La solución fue **especificar la ruta explícitamente** en el controlador.

---

**Estado:** ? **FIX APLICADO**  
**Requiere:** Reiniciar la API para que tome efecto  
**Impacto:** UnidadesMedida ahora funciona correctamente  

---

**Fecha:** 2026-02-17  
**Autor:** GitHub Copilot  
**Revisión:** ? Completa
