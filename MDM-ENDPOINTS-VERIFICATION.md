# ?? Verificación de Endpoints MDM - API

## ?? Objetivo
Verificar que todos los endpoints MDM (Master Data Management) estén correctamente implementados y accesibles en la API.

---

## ? Controladores Implementados

### 1?? UnidadesMedida ?
**Archivo:** `src/AgoraHub360.ERP.Api/Controllers/V1/UnidadesMedidaController.cs`

**Ruta Base:** `api/v1/unidades-medida` ? (Ruta explícita - CORRECTO)

| Método | Endpoint | Descripción | Estado |
|--------|----------|-------------|--------|
| GET | `/api/v1/unidades-medida` | Listar todas | ? |
| GET | `/api/v1/unidades-medida/{id}` | Obtener por ID | ? |
| POST | `/api/v1/unidades-medida` | Crear nueva | ? |
| PUT | `/api/v1/unidades-medida/{id}` | Actualizar | ? |
| DELETE | `/api/v1/unidades-medida/{id}` | Eliminar | ? |

**Características:**
- ? Autorización requerida
- ? Multi-tenant (filtra por EmpresaId)
- ? Validación de unicidad (Nombre y Abreviatura)
- ? DTOs específicos: UnidadMedidaDto, CreateUnidadMedidaDto, UpdateUnidadMedidaDto

---

### 2?? Almacenes ?
**Archivo:** `src/AgoraHub360.ERP.Api/Controllers/V1/AlmacenesController.cs`

**Ruta Base:** `api/v1/almacenes` ? (Ruta explícita - CORRECTO)

| Método | Endpoint | Descripción | Estado |
|--------|----------|-------------|--------|
| GET | `/api/v1/almacenes` | Listar todos | ? |
| GET | `/api/v1/almacenes/{id}` | Obtener por ID | ? |
| POST | `/api/v1/almacenes` | Crear nuevo | ? |
| PUT | `/api/v1/almacenes/{id}` | Actualizar | ? |
| DELETE | `/api/v1/almacenes/{id}` | Eliminar | ? |

**Características:**
- ? Autorización requerida
- ? Multi-tenant (filtra por EmpresaId)
- ? DTOs en `Shared.DTOs.MDM` (namespace antiguo)

---

### 3?? CategoríasProducto ?
**Estado:** NO IMPLEMENTADO

**Ruta esperada:** `api/v1/categorias`

| Método | Endpoint | Descripción | Estado |
|--------|----------|-------------|--------|
| GET | `/api/v1/categorias` | Listar todas | ? Pendiente |
| GET | `/api/v1/categorias/{id}` | Obtener por ID | ? Pendiente |
| POST | `/api/v1/categorias` | Crear nueva | ? Pendiente |
| PUT | `/api/v1/categorias/{id}` | Actualizar | ? Pendiente |
| DELETE | `/api/v1/categorias/{id}` | Eliminar | ? Pendiente |

**Notas:**
- Servicio registrado en DI: ? `ICategoriaProductoService`
- DTOs existen en: `Shared.DTOs.MDM`
- ?? Falta implementar controlador

---

### 4?? Clientes ?
**Estado:** NO IMPLEMENTADO

**Ruta esperada:** `api/v1/clientes`

| Método | Endpoint | Descripción | Estado |
|--------|----------|-------------|--------|
| GET | `/api/v1/clientes` | Listar todos | ? Pendiente |
| GET | `/api/v1/clientes/{id}` | Obtener por ID | ? Pendiente |
| POST | `/api/v1/clientes` | Crear nuevo | ? Pendiente |
| PUT | `/api/v1/clientes/{id}` | Actualizar | ? Pendiente |
| DELETE | `/api/v1/clientes/{id}` | Eliminar | ? Pendiente |

**Notas:**
- Servicio registrado en DI: ? `IClienteService`
- DTOs existen en: `Shared.DTOs.MDM`
- ?? Falta implementar controlador

---

### 5?? Proveedores ?
**Estado:** NO IMPLEMENTADO

**Ruta esperada:** `api/v1/proveedores`

| Método | Endpoint | Descripción | Estado |
|--------|----------|-------------|--------|
| GET | `/api/v1/proveedores` | Listar todos | ? Pendiente |
| GET | `/api/v1/proveedores/{id}` | Obtener por ID | ? Pendiente |
| POST | `/api/v1/proveedores` | Crear nuevo | ? Pendiente |
| PUT | `/api/v1/proveedores/{id}` | Actualizar | ? Pendiente |
| DELETE | `/api/v1/proveedores/{id}` | Eliminar | ? Pendiente |

**Notas:**
- Servicio registrado en DI: ? `IProveedorService`
- DTOs existen en: `Shared.DTOs.MDM`
- ?? Falta implementar controlador

---

### 6?? Productos ?
**Estado:** NO IMPLEMENTADO

**Ruta esperada:** `api/v1/productos`

| Método | Endpoint | Descripción | Estado |
|--------|----------|-------------|--------|
| GET | `/api/v1/productos` | Listar todos | ? Pendiente |
| GET | `/api/v1/productos/{id}` | Obtener por ID | ? Pendiente |
| POST | `/api/v1/productos` | Crear nuevo | ? Pendiente |
| PUT | `/api/v1/productos/{id}` | Actualizar | ? Pendiente |
| DELETE | `/api/v1/productos/{id}` | Eliminar | ? Pendiente |

**Notas:**
- Servicio registrado en DI: ? `IProductoService`
- DTOs existen en: `Shared.DTOs.MDM`
- ?? Falta implementar controlador
- ?? Requiere relaciones con CategoríaProducto y UnidadMedida

---

## ?? Resumen de Estado

| Entidad MDM | Controlador | Servicio | DTOs | Estado |
|-------------|-------------|----------|------|--------|
| UnidadMedida | ? | ? | ? | **COMPLETO** |
| Almacen | ? | ? | ? | **COMPLETO** |
| CategoriaProducto | ? | ? | ? | 66% (Falta controlador) |
| Cliente | ? | ? | ? | 66% (Falta controlador) |
| Proveedor | ? | ? | ? | 66% (Falta controlador) |
| Producto | ? | ? | ? | 66% (Falta controlador) |

**Progreso Total:** 33% (2/6 controladores implementados)

---

## ?? Controladores No-MDM Verificados

### Core/Sistema

| Controlador | Ruta | Problema Detectado |
|-------------|------|-------------------|
| ? EmpresasController | `api/v1/[controller]` | ?? Usar ruta explícita: `empresas` |
| ? UsuariosController | `api/v1/[controller]` | ?? Usar ruta explícita: `usuarios` |
| ? RolesController | (no revisado) | - |
| ? ParametrosController | `api/v1/[controller]` | ?? Usar ruta explícita: `parametros` |
| ? NumeracionesController | `api/v1/[controller]` | ?? Usar ruta explícita: `numeraciones` |
| ? AuditLogsController | (no revisado) | - |
| ? AuthController | (no revisado) | - |

---

## ?? Problemas Detectados

### 1. Uso de `[controller]` en Rutas

**Problema:**
Varios controladores usan `[Route("api/v{version:apiVersion}/[controller]")]` que genera rutas en **PascalCase** (ej: `Empresas`, `Usuarios`).

**Impacto:**
- Inconsistencia con convenciones REST (kebab-case)
- Desajuste con HTTP Services del cliente
- Puede causar errores 404

**Solución:**
Usar rutas explícitas en minúsculas:
```csharp
[Route("api/v{version:apiVersion}/empresas")]
[Route("api/v{version:apiVersion}/usuarios")]
[Route("api/v{version:apiVersion}/parametros")]
[Route("api/v{version:apiVersion}/numeraciones")]
```

### 2. DTOs en Namespace Antiguo

**Problema:**
Almacenes usa DTOs en `Shared.DTOs.MDM` (namespace antiguo).

**Impacto:**
- Inconsistencia con UnidadMedida que usa `Shared.DTOs.UnidadMedida`
- Confusión en imports

**Solución:**
Migrar DTOs de Almacen a su propio namespace:
- `Shared.DTOs.Almacen.AlmacenDto`
- `Shared.DTOs.Almacen.CreateAlmacenDto`
- `Shared.DTOs.Almacen.UpdateAlmacenDto`

---

## ?? Plan de Acción

### Fase 1: Correcciones Inmediatas
1. ? **UnidadMedida** - Ruta corregida
2. ? **Almacenes** - Migrar DTOs a namespace propio
3. ? **Controladores Core** - Cambiar a rutas explícitas

### Fase 2: Implementar Controladores Faltantes
1. **CategoriaProducto** (~30 min)
2. **Cliente** (~45 min)
3. **Proveedor** (~45 min)
4. **Producto** (~60 min - tiene relaciones)

### Fase 3: Testing y Documentación
1. Probar todos los endpoints en Swagger
2. Verificar autenticación y multi-tenant
3. Actualizar documentación de API

---

## ?? Cómo Verificar Endpoints

### Opción 1: Swagger UI

1. Iniciar API:
```powershell
cd src\AgoraHub360.ERP.Api
dotnet run --launch-profile https
```

2. Abrir Swagger:
```
https://localhost:7001/swagger/index.html
```

3. Buscar sección **MDM** o **V1**

4. Verificar endpoints disponibles

### Opción 2: Script PowerShell

Crear `test-mdm-endpoints.ps1`:
```powershell
$endpoints = @(
    "https://localhost:7001/api/v1/unidades-medida",
    "https://localhost:7001/api/v1/almacenes",
    "https://localhost:7001/api/v1/categorias",
    "https://localhost:7001/api/v1/clientes",
    "https://localhost:7001/api/v1/proveedores",
    "https://localhost:7001/api/v1/productos"
)

foreach ($endpoint in $endpoints) {
    try {
        $response = Invoke-WebRequest -Uri $endpoint -SkipCertificateCheck -ErrorAction Stop
        Write-Host "? $endpoint - 200 OK" -ForegroundColor Green
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 401) {
            Write-Host "? $endpoint - 401 (requiere auth - existe)" -ForegroundColor Yellow
        }
        elseif ($statusCode -eq 404) {
            Write-Host "? $endpoint - 404 (no implementado)" -ForegroundColor Red
        }
        else {
            Write-Host "??  $endpoint - $statusCode" -ForegroundColor DarkYellow
        }
    }
}
```

### Opción 3: curl

```bash
# UnidadesMedida
curl -X GET "https://localhost:7001/api/v1/unidades-medida" -k

# Almacenes
curl -X GET "https://localhost:7001/api/v1/almacenes" -k

# Categorías (debe dar 404)
curl -X GET "https://localhost:7001/api/v1/categorias" -k
```

---

## ?? Template para Nuevo Controlador MDM

```csharp
namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.{Entidad};
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/{entidad-kebab-case}")]
[Authorize]
public class {Entidad}Controller : ControllerBase
{
    private readonly I{Entidad}Service _service;

    public {Entidad}Controller(I{Entidad}Service service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<{Entidad}Dto>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<{Entidad}Dto>.Fail(result.Error!));
        return Ok(ApiResponse<{Entidad}Dto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Create{Entidad}Dto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<{Entidad}Dto>.Fail(result.Error!));
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<{Entidad}Dto>.Ok(result.Value!, "{Entidad} creada exitosamente."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Update{Entidad}Dto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrad"))
                return NotFound(ApiResponse<{Entidad}Dto>.Fail(result.Error!));
            return BadRequest(ApiResponse<{Entidad}Dto>.Fail(result.Error!));
        }
        return Ok(ApiResponse<{Entidad}Dto>.Ok(result.Value!, "{Entidad} actualizada exitosamente."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "{Entidad} eliminada exitosamente."));
    }
}
```

---

## ?? Referencias

- [ASP.NET Core API Versioning](https://github.com/dotnet/aspnet-api-versioning)
- [REST API Naming Conventions](https://restfulapi.net/resource-naming/)
- [Swagger/OpenAPI Specification](https://swagger.io/specification/)

---

**Fecha:** 2026-02-17  
**Estado:** 2/6 controladores MDM implementados (33%)  
**Próximo:** Implementar CategoriaProductoController
