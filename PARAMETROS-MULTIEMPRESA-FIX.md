# ? Corrección: Creación de Parámetros y Numeración por Empresa

## ?? Problema Identificado

Al intentar crear un parámetro o numeración desde la interfaz web, el sistema fallaba porque:
1. ? No se estaba asignando el `EmpresaId` al crear nuevos registros
2. ? No se estaba filtrando por empresa al listar registros
3. ? El sistema es **multiempresa** pero los servicios no estaban tenant-aware

---

## ?? Diagnóstico

### Arquitectura del Sistema

El sistema AgoraHub360 ERP es **multiempresa desde el núcleo**:
- Todas las entidades heredan de `TenantEntity` que incluye `EmpresaId`
- El JWT contiene un claim `EmpresaId` del usuario autenticado
- Existe un `ICurrentUserService` para obtener la empresa activa

### Problema Encontrado

Los servicios `ParametroSistemaService` y `NumeracionDocumentoService` **NO** estaban utilizando el `ICurrentUserService`, por lo tanto:
- Al crear registros, el `EmpresaId` quedaba en **0** (valor por defecto)
- Al listar registros, devolvía **todos** sin filtrar por empresa
- Violación del principio de **aislamiento de datos por tenant**

---

## ? Solución Implementada

### 1?? **ParametroSistemaService**

**Antes:**
```csharp
public ParametroSistemaService(
    IRepository<ParametroSistema> repository, 
    IUnitOfWork unitOfWork)
{
    _repository = repository;
    _unitOfWork = unitOfWork;
}
```

**Después:**
```csharp
public ParametroSistemaService(
    IRepository<ParametroSistema> repository, 
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)  // ? NUEVO
{
    _repository = repository;
    _unitOfWork = unitOfWork;
    _currentUserService = currentUserService;  // ? NUEVO
}
```

**Cambios en los métodos:**

#### `GetAllAsync()`
```csharp
// ANTES: Devolvía todos los parámetros sin filtro
var items = await _repository.GetAllAsync(ct);

// DESPUÉS: Filtra por empresa
var empresaId = _currentUserService.EmpresaId;
if (!empresaId.HasValue)
    return Result<...>.Failure("No se pudo determinar la empresa activa del usuario.");

var items = await _repository.FindAsync(p => p.EmpresaId == empresaId.Value, ct);
```

#### `UpsertAsync()`
```csharp
// ANTES: No asignaba EmpresaId
existing = new ParametroSistema
{
    Clave = dto.Clave,
    Valor = dto.Valor,
    // EmpresaId no se asignaba ?
};

// DESPUÉS: Asigna EmpresaId del usuario autenticado
var empresaId = _currentUserService.EmpresaId;
if (!empresaId.HasValue)
    return Result<...>.Failure("No se pudo determinar la empresa activa del usuario.");

existing = new ParametroSistema
{
    Clave = dto.Clave,
    Valor = dto.Valor,
    EmpresaId = empresaId.Value,  // ? Asigna empresa
    Activo = true
};
```

---

### 2?? **NumeracionDocumentoService**

**Mismos cambios aplicados:**

1. ? Inyección de `ICurrentUserService`
2. ? Filtrado por empresa en `GetAllAsync()`
3. ? Asignación de `EmpresaId` en `CreateAsync()`
4. ? Validación de permisos en `Update` y `Delete`

**Ejemplo de validación de seguridad:**
```csharp
public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
{
    var empresaId = _currentUserService.EmpresaId;
    if (!empresaId.HasValue)
        return Result<bool>.Failure("No se pudo determinar la empresa activa del usuario.");

    var entity = await _repository.GetByIdAsync(id, ct);
    if (entity is null)
        return Result<bool>.Failure("Numeración no encontrada.");

    // Verificar que pertenece a la empresa del usuario
    if (entity.EmpresaId != empresaId.Value)
        return Result<bool>.Failure("No tiene permisos para eliminar esta numeración.");

    await _repository.DeleteAsync(entity, ct);
    await _unitOfWork.SaveChangesAsync(ct);
    return Result<bool>.Success(true);
}
```

---

### 3?? **Tests Corregidos**

**Antes:**
```csharp
public ParametroSistemaServiceTests()
{
    _repo = new FakeParamRepo();
    _uow = new FakeUow();
    _sut = new ParametroSistemaService(_repo, _uow);  // ? Faltaba parámetro
}
```

**Después:**
```csharp
public ParametroSistemaServiceTests()
{
    _repo = new FakeParamRepo();
    _uow = new FakeUow();
    _currentUserService = new FakeCurrentUserService { EmpresaId = 1 };  // ? Mock
    _sut = new ParametroSistemaService(_repo, _uow, _currentUserService);
}

// Fake para testing
private class FakeCurrentUserService : ICurrentUserService
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public int? EmpresaId { get; set; }
}
```

---

## ?? Archivos Modificados

1. ? `src/AgoraHub360.ERP.Application/Services/ParametroSistemaService.cs`
2. ? `src/AgoraHub360.ERP.Application/Services/NumeracionDocumentoService.cs`
3. ? `tests/AgoraHub360.ERP.Tests/Application/ParametroSistemaServiceTests.cs`
4. ? `tests/AgoraHub360.ERP.Tests/Application/NumeracionDocumentoServiceTests.cs`

---

## ?? Seguridad Mejorada

### Aislamiento de Datos por Empresa

Ahora el sistema garantiza que:
- ? Cada usuario solo ve los parámetros/numeraciones de **su empresa**
- ? No puede crear parámetros/numeraciones en **otras empresas**
- ? No puede modificar/eliminar parámetros/numeraciones de **otras empresas**

### Flujo de Seguridad

```
1. Usuario inicia sesión
2. JWT incluye EmpresaId (ej: EmpresaId = 1)
3. Al crear parámetro:
   ??> Sistema obtiene EmpresaId del JWT
   ??> Asigna EmpresaId al nuevo parámetro
   ??> Guarda en BD con EmpresaId = 1

4. Al listar parámetros:
   ??> Sistema obtiene EmpresaId del JWT
   ??> Filtra: WHERE EmpresaId = 1
   ??> Usuario solo ve SUS parámetros
```

---

## ?? Testing

### Test 1: Crear Parámetro

```powershell
# Iniciar sistema
.\start-system.ps1

# Navegar a Parámetros
https://localhost:5002/config/parametros

# Crear parámetro
- Clave: "TEST_PARAM"
- Valor: "12345"
- Categoría: "General"
- Tipo: "String"

# Verificar en BD
SELECT * FROM core.ParametrosSistema WHERE Clave = 'TEST_PARAM';
-- Debe tener EmpresaId = 1 ?
```

### Test 2: Crear Numeración

```powershell
# Navegar a Numeración
https://localhost:5002/config/parametros

# Tab: "Numeración de Documentos"
# Crear serie:
- Tipo: "TEST"
- Descripción: "Serie de Test"
- Prefijo: "TEST-"

# Verificar en BD
SELECT * FROM core.NumeracionesDocumento WHERE TipoDocumento = 'TEST';
-- Debe tener EmpresaId = 1 ?
```

### Test 3: Verificar Aislamiento

```sql
-- Crear usuario en otra empresa (manualmente en BD)
INSERT INTO core.UsuarioEmpresas (UsuarioId, EmpresaId, Rol)
VALUES (1, 2, 'Admin');

-- Cambiar empresa activa en JWT (reiniciar sesión)
-- Al listar parámetros, solo debe ver los de EmpresaId = 2
```

---

## ? Checklist de Verificación

- [x] `ParametroSistemaService` usa `ICurrentUserService`
- [x] `NumeracionDocumentoService` usa `ICurrentUserService`
- [x] `GetAllAsync()` filtra por empresa
- [x] `CreateAsync()` asigna `EmpresaId`
- [x] `Update/Delete` validan permisos por empresa
- [x] Tests actualizados con mock de `ICurrentUserService`
- [x] Compilación exitosa
- [x] Tests pasan correctamente

---

## ?? Comparación Antes/Después

| Aspecto | Antes | Después |
|---------|-------|---------|
| **EmpresaId en creación** | ? No se asignaba (0) | ? Se asigna del JWT |
| **Filtrado por empresa** | ? Devolvía todos | ? Filtra por empresa |
| **Seguridad** | ? Sin validación | ? Valida permisos |
| **Tests** | ? Faltaba parámetro | ? Completos |

---

## ?? Resultado

? **Sistema Multiempresa Completo**
- Cada empresa tiene sus propios parámetros y numeraciones
- Aislamiento de datos garantizado
- Seguridad por tenant implementada
- Tests actualizados y funcionales

---

## ?? Mejores Prácticas Aplicadas

1. **Tenant-Aware Services:** Todos los servicios deben usar `ICurrentUserService`
2. **Validación de Seguridad:** Siempre verificar que el recurso pertenece a la empresa del usuario
3. **Mensajes de Error Claros:** "No se pudo determinar la empresa activa del usuario"
4. **Tests Completos:** Incluir mocks para todas las dependencias

---

## ?? Próximos Pasos (Opcional)

### 1. Implementar Filtro Global de Tenant

Considerar agregar un filtro global en EF Core:
```csharp
modelBuilder.Entity<ParametroSistema>()
    .HasQueryFilter(p => p.EmpresaId == _currentUserService.EmpresaId);
```

### 2. Auditoría Mejorada

Agregar logs cuando se intenta acceder a recursos de otra empresa:
```csharp
if (entity.EmpresaId != empresaId.Value)
{
    _logger.LogWarning("Usuario {UserId} intentó acceder a recurso de empresa {EmpresaId}", 
        _currentUserService.UserId, entity.EmpresaId);
    return Result<bool>.Failure("No tiene permisos...");
}
```

---

**? Problema Resuelto - Sistema Multiempresa Funcional**

_Fecha: 17/02/2026_
_Versión: v1.0_
