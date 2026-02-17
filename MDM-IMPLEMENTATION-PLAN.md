# ?? Plan de Implementación MDM - AgoraHub360 ERP

## ?? Objetivo
Implementar un sistema MDM (Master Data Management) completo y funcional con:
- ? Clientes
- ? Proveedores  
- ? Productos
- ? Almacenes
- ? Catálogos (Categorías, Unidades de Medida)

## ?? Estado Actual

### ? **YA EXISTE**
| Componente | Estado | Ubicación |
|------------|--------|-----------|
| Entidades de Dominio | ? Completas | `src/AgoraHub360.ERP.Domain/Entities/MDM/` |
| Configuraciones EF Core | ? Completas | `src/AgoraHub360.ERP.Persistence/Configurations/` |
| Migraciones DB | ? Aplicadas | `AddMDMEntities`, `MDM_Enhanced` |
| Enums | ? Completos | `TipoProducto`, `TipoProveedor` |

### ? **FALTA IMPLEMENTAR**
| Componente | Estado | Prioridad |
|------------|--------|-----------|
| DTOs | ? No existen | ?? CRÍTICO |
| Servicios de Aplicación | ? No existen | ?? CRÍTICO |
| Controladores API | ? Eliminados | ?? CRÍTICO |
| Servicios HTTP (Web) | ? No existen | ?? CRÍTICO |
| Páginas Razor | ? Eliminadas | ?? ALTA |
| Registro DI | ? Falta | ?? CRÍTICO |

---

## ??? Estructura de Archivos a Crear

### 1?? **DTOs** (src/AgoraHub360.ERP.Shared/DTOs/)

```
Cliente/
??? ClienteDto.cs
??? CreateClienteDto.cs
??? UpdateClienteDto.cs

Proveedor/
??? ProveedorDto.cs
??? CreateProveedorDto.cs
??? UpdateProveedorDto.cs

Producto/
??? ProductoDto.cs
??? CreateProductoDto.cs
??? UpdateProductoDto.cs

CategoriaProducto/
??? CategoriaProductoDto.cs
??? CreateCategoriaProductoDto.cs
??? UpdateCategoriaProductoDto.cs

UnidadMedida/
??? UnidadMedidaDto.cs
??? CreateUnidadMedidaDto.cs
??? UpdateUnidadMedidaDto.cs

Almacen/
??? AlmacenDto.cs
??? CreateAlmacenDto.cs
??? UpdateAlmacenDto.cs
```

### 2?? **Interfaces de Servicio** (src/AgoraHub360.ERP.Application/Interfaces/)

```
IClienteService.cs
IProveedorService.cs
IProductoService.cs
ICategoriaProductoService.cs
IUnidadMedidaService.cs
IAlmacenService.cs
```

### 3?? **Servicios** (src/AgoraHub360.ERP.Application/Services/)

```
ClienteService.cs
ProveedorService.cs
ProductoService.cs
CategoriaProductoService.cs
UnidadMedidaService.cs
AlmacenService.cs
```

### 4?? **Controladores API** (src/AgoraHub360.ERP.Api/Controllers/V1/)

```
ClientesController.cs
ProveedoresController.cs
ProductosController.cs
CategoriasProductoController.cs
UnidadesMedidaController.cs
AlmacenesController.cs
```

### 5?? **Servicios HTTP (Blazor)** (src/AgoraHub360.ERP.Web/Services/)

```
ClienteHttpService.cs
ProveedorHttpService.cs
ProductoHttpService.cs
CategoriaProductoHttpService.cs
UnidadMedidaHttpService.cs
AlmacenHttpService.cs
```

### 6?? **Páginas Razor** (src/AgoraHub360.ERP.Web/Pages/MDM/)

```
Clientes.razor
Proveedores.razor
Productos.razor
Categorias.razor
UnidadesMedida.razor
Almacenes.razor
```

---

## ?? UX Mínimo Requerido

Cada página debe incluir:

### **Listado**
- ? Search box (búsqueda en tiempo real)
- ? Tabla responsive con datos
- ? Paginación (15 items por página)
- ? Badge de estado (Activo/Inactivo)
- ? Botón "Nuevo [Entidad]"

### **Formulario (Modal)**
- ? Validaciones visibles
- ? Campos obligatorios marcados con *
- ? InputText, InputSelect, InputNumber según tipo
- ? Botones: Cancelar / Guardar
- ? Loading state en guardado

### **Acciones**
- ? Editar (icono lápiz)
- ? Eliminar (icono basura)
- ? Confirmación de eliminación

---

## ?? Plantilla de Implementación

### **DTO Ejemplo (ClienteDto.cs)**
```csharp
namespace AgoraHub360.ERP.Shared.DTOs.Cliente;

public class ClienteDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NIT { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? NombreContacto { get; set; }
    public string TipoCliente { get; set; } = "General";
    public bool Activo { get; set; }
    public int EmpresaId { get; set; }
}
```

### **CreateDto Ejemplo**
```csharp
using System.ComponentModel.DataAnnotations;

public class CreateClienteDto
{
    [Required(ErrorMessage = "El código es requerido.")]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razón social es requerida.")]
    [MaxLength(300)]
    public string RazonSocial { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? NIT { get; set; }

    [MaxLength(500)]
    public string? Direccion { get; set; }

    [MaxLength(50)]
    public string? Telefono { get; set; }

    [MaxLength(200)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? NombreContacto { get; set; }

    [Required]
    [MaxLength(50)]
    public string TipoCliente { get; set; } = "General";
}
```

### **Servicio Ejemplo**
```csharp
public class ClienteService : IClienteService
{
    private readonly IRepository<Cliente> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public async Task<Result<IReadOnlyList<ClienteDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<ClienteDto>>.Failure("No se pudo determinar la empresa activa.");

        var items = await _repository.FindAsync(c => c.EmpresaId == empresaId.Value, ct);
        return Result<IReadOnlyList<ClienteDto>>.Success(
            items.Select(MapToDto).ToList().AsReadOnly());
    }

    // ... CRUD completo
}
```

### **Controlador Ejemplo**
```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<ClienteDto>>.Ok(result.Value!));
    }

    // ... CRUD completo
}
```

### **Página Razor Ejemplo**
```razor
@page "/mdm/clientes"
@attribute [Authorize]
@inject ClienteHttpService ClienteSvc

<!-- Search box + Tabla + Paginación + Modal -->
```

---

## ?? Registro de Dependencias (Program.cs)

Agregar en `src/AgoraHub360.ERP.Api/Program.cs`:

```csharp
// Servicios MDM
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICategoriaProductoService, CategoriaProductoService>();
builder.Services.AddScoped<IUnidadMedidaService, UnidadMedidaService>();
builder.Services.AddScoped<IAlmacenService, AlmacenService>();
```

Agregar en `src/AgoraHub360.ERP.Web/Program.cs`:

```csharp
// HTTP Services MDM
builder.Services.AddScoped<ClienteHttpService>();
builder.Services.AddScoped<ProveedorHttpService>();
builder.Services.AddScoped<ProductoHttpService>();
builder.Services.AddScoped<CategoriaProductoHttpService>();
builder.Services.AddScoped<UnidadMedidaHttpService>();
builder.Services.AddScoped<AlmacenHttpService>();
```

---

## ?? Orden de Implementación Recomendado

### **Fase 1: Catálogos** (2-3 horas)
1. ? UnidadMedida (CRUD completo)
2. ? CategoriaProducto (CRUD completo)

### **Fase 2: Clientes** (2-3 horas)
3. ? ClienteDto + Service + Controller + HTTP + Razor

### **Fase 3: Proveedores** (2-3 horas)
4. ? ProveedorDto + Service + Controller + HTTP + Razor
5. ? Campo adicional: TipoProveedor (Local/Internacional)

### **Fase 4: Productos** (3-4 horas)
6. ? ProductoDto + Service + Controller + HTTP + Razor
7. ? Relaciones: Categoría + UnidadMedida
8. ? Enum: TipoProducto (MP/PT/Servicio)

### **Fase 5: Almacenes** (2-3 horas)
9. ? AlmacenDto + Service + Controller + HTTP + Razor

---

## ? Checklist de Validación

### **Por cada entidad:**
- [ ] DTO completo (Get, Create, Update)
- [ ] Interface de servicio
- [ ] Servicio implementado
- [ ] Controlador API
- [ ] HTTP Service (Blazor)
- [ ] Página Razor funcional
- [ ] Registro en DI
- [ ] Validaciones visibles
- [ ] Search funcional
- [ ] Paginación operativa
- [ ] Modal de confirmación al eliminar

---

## ?? Estimación de Tiempo

| Componente | Tiempo Estimado |
|------------|-----------------|
| DTOs (6 entidades × 3 archivos) | 2 horas |
| Interfaces de Servicio | 1 hora |
| Servicios | 4 horas |
| Controladores API | 3 horas |
| HTTP Services | 2 horas |
| Páginas Razor | 6 horas |
| Registro DI + Testing | 1 hora |
| **TOTAL** | **~19 horas** |

---

## ?? Resultado Esperado

Al finalizar, el usuario podrá:

1. ? Ver listado de Clientes con búsqueda y paginación
2. ? Crear/Editar/Eliminar clientes
3. ? Ver listado de Proveedores (Local/Internacional)
4. ? Crear/Editar/Eliminar proveedores
5. ? Ver listado de Productos con categoría y unidad
6. ? Crear/Editar/Eliminar productos
7. ? Ver listado de Almacenes
8. ? Crear/Editar/Eliminar almacenes
9. ? Gestionar Categorías y Unidades de Medida

Todas las operaciones son **multi-tenant** (filtradas por `EmpresaId`).

---

## ?? Siguientes Pasos

1. **Iniciar con Catálogos:** Más simples y necesarios para Productos
2. **Continuar con Clientes:** Funcionalidad crítica
3. **Implementar Proveedores:** Similar a Clientes
4. **Finalizar con Productos:** Más complejo por relaciones
5. **Completar Almacenes:** Preparación para Inventario

---

**¿Quieres que empiece a implementar alguna entidad específica?**

Puedo comenzar con:
- ?? **UnidadMedida** (más simple, CRUD básico)
- ?? **CategoriaProducto** (CRUD básico)
- ?? **Cliente** (CRUD completo con búsqueda)
- ?? **Proveedor** (CRUD + tipo Local/Internacional)
- ?? **Producto** (CRUD + relaciones + tipos)
- ?? **Almacen** (CRUD + ubicaciones)
