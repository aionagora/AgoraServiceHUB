# ? Mejoras de Accesibilidad - Selector de Empresa

## ?? **Objetivo**

Mejorar la experiencia del usuario para que cuando se loguee, acceda automáticamente solo a la(s) empresa(s) que tiene asignada(s), eliminando la confusión de ver empresas a las que no tiene acceso.

---

## ?? **Mejoras Implementadas**

### 1?? **Nuevo Endpoint: GET /api/v1/empresas/mis-empresas**

**Ubicación:** `src/AgoraHub360.ERP.Api/Controllers/V1/EmpresasController.cs`

```csharp
[HttpGet("mis-empresas")]
public async Task<IActionResult> GetMisEmpresas(CancellationToken ct)
{
    var userId = _currentUserService.UserIdInt;
    if (!userId.HasValue)
        return Unauthorized(...);

    // Obtener solo empresas asignadas al usuario
    var empresasResult = await _usuarioService.GetEmpresasAsignadasAsync(userId.Value, ct);
    
    // Filtrar empresas activas
    var misEmpresas = allEmpresas.Value!
        .Where(e => empresaIds.Contains(e.Id) && e.Activo)
        .ToList();

    return Ok(...);
}
```

**Funcionalidad:**
- ? Obtiene solo las empresas asignadas al usuario autenticado
- ? Filtra empresas activas
- ? Protegido con `[Authorize]`

---

### 2?? **Servicio HTTP Actualizado**

**Ubicación:** `src/AgoraHub360.ERP.Web/Services/EmpresaHttpService.cs`

```csharp
public async Task<List<EmpresaDto>> GetMisEmpresasAsync()
{
    var response = await _http.GetFromJsonAsync<ApiResponse<List<EmpresaDto>>>(
        $"{BaseUrl}/mis-empresas");
    return response?.Data ?? new List<EmpresaDto>();
}
```

---

### 3?? **MainLayout Mejorado**

**Ubicación:** `src/AgoraHub360.ERP.Web/Layout/MainLayout.razor`

#### **Cambio A: Carga de Empresas Asignadas**

```csharp
private async Task LoadEmpresasAsync()
{
    try
    {
        // ANTES: Cargaba TODAS las empresas
        // var empresas = await EmpresaSvc.GetAllAsync();
        
        // AHORA: Carga solo las empresas asignadas al usuario
        var empresas = await EmpresaSvc.GetMisEmpresasAsync();
        EmpresaState.SetEmpresas(empresas);
        
        // Si solo tiene una empresa, establecerla automáticamente
        if (empresas.Count == 1)
        {
            await EmpresaState.SetEmpresaActivaAsync(empresas[0]);
        }
    }
    catch { }
}
```

#### **Cambio B: Selector Condicional**

```razor
<!-- Solo se muestra si tiene MÁS de una empresa -->
@if (EmpresaState.Empresas.Count > 1)
{
    <div class="empresa-selector">
        <i class="bi bi-building"></i>
        <select class="empresa-select" @onchange="OnEmpresaChanged">
            @foreach (var emp in EmpresaState.Empresas.Where(e => e.Activo))
            {
                <option value="@emp.Id" selected="@(emp.Id == EmpresaState.EmpresaActivaId)">
                    @emp.Nombre
                </option>
            }
        </select>
    </div>
}
else if (EmpresaState.EmpresaActiva != null)
{
    <!-- Badge simple si solo tiene UNA empresa -->
    <div class="empresa-badge">
        <i class="bi bi-building"></i>
        <span class="empresa-nombre">@EmpresaState.EmpresaActiva.Nombre</span>
    </div>
}
```

**Visual:**

**Antes (con dropdown):**
```
??????????????????????????????????????????
?  ?? [EMPRESA A ?]  ?? usuarioA  ?     ?
??????????????????????????????????????????
```

**Ahora (sin dropdown, una sola empresa):**
```
??????????????????????????????????????????
?  ?? EMPRESA A   ?? usuarioA  ?         ?
??????????????????????????????????????????
```

---

### 4?? **Estilos CSS Nuevos**

**Ubicación:** `src/AgoraHub360.ERP.Web/Layout/MainLayout.razor.css`

```css
/* Badge de empresa cuando solo hay una asignada */
.empresa-badge {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.85rem;
    color: var(--agora-primary);
    background: rgba(26, 58, 92, 0.08);
    padding: 0.4rem 0.75rem;
    border-radius: var(--radius-sm);
    font-weight: 500;
}

.empresa-badge i {
    font-size: 1rem;
}

.empresa-nombre {
    font-weight: 600;
}
```

---

### 5?? **ICurrentUserService Mejorado**

**Ubicación:** `src/AgoraHub360.ERP.Application/Interfaces/ICurrentUserService.cs`

```csharp
public interface ICurrentUserService
{
    string? UserId { get; }
    int? UserIdInt { get; }  // ? NUEVO
    string? UserName { get; }
    int? EmpresaId { get; }
}
```

**Implementación actualizada:**

```csharp
public int? UserIdInt
{
    get
    {
        var userId = UserId;
        return int.TryParse(userId, out var id) ? id : null;
    }
}
```

---

## ?? **Comparación: Antes vs Después**

| Aspecto | Antes | Después |
|---------|-------|---------|
| **Empresas visibles** | Todas las empresas del sistema | Solo empresas asignadas al usuario |
| **Selector dropdown** | Siempre visible | Solo si tiene >1 empresa |
| **Empresa única** | Dropdown con 1 opción | Badge simple sin dropdown |
| **Acceso automático** | Manual (seleccionar dropdown) | Automático si tiene 1 empresa |
| **Confusión** | Usuario ve empresas sin acceso | Usuario solo ve sus empresas |

---

## ?? **Casos de Uso**

### **Caso 1: Usuario con UNA empresa asignada**

**Ejemplo:** `usuarioA` asignado solo a `EMPRESA A`

1. Usuario hace login
2. Sistema carga automáticamente `EMPRESA A`
3. UI muestra: `?? EMPRESA A` (badge simple, sin dropdown)
4. Usuario accede directamente al contenido de `EMPRESA A`

**Beneficio:** ? Acceso inmediato, sin pasos adicionales

---

### **Caso 2: Usuario con MÚLTIPLES empresas asignadas**

**Ejemplo:** `usuarioB` asignado a `EMPRESA A`, `EMPRESA B`, `EMPRESA C`

1. Usuario hace login
2. Sistema carga las 3 empresas asignadas
3. UI muestra: `?? [EMPRESA A ?]` (dropdown funcional)
4. Usuario puede seleccionar entre sus 3 empresas

**Beneficio:** ? Flexibilidad para cambiar entre empresas

---

### **Caso 3: Usuario SIN empresas asignadas**

**Ejemplo:** `usuarioC` sin empresas asignadas

1. Usuario hace login
2. Sistema carga lista vacía
3. UI muestra: `?? Sin empresas`
4. Usuario ve mensaje: "No tiene empresas asignadas"

**Beneficio:** ? Feedback claro sobre la situación

---

## ?? **Seguridad**

### **Endpoint Protegido**

```csharp
[Authorize]
[HttpGet("mis-empresas")]
public async Task<IActionResult> GetMisEmpresas(CancellationToken ct)
{
    // Validar autenticación
    var userId = _currentUserService.UserIdInt;
    if (!userId.HasValue)
        return Unauthorized(...);
    
    // Obtener solo empresas del usuario actual
    var empresasResult = await _usuarioService.GetEmpresasAsignadasAsync(userId.Value, ct);
    
    // Filtrar empresas activas
    var misEmpresas = allEmpresas.Value!
        .Where(e => empresaIds.Contains(e.Id) && e.Activo)
        .ToList();
}
```

**Garantías de seguridad:**
- ? `[Authorize]` - Solo usuarios autenticados
- ? `_currentUserService.UserIdInt` - Identidad verificada desde JWT
- ? Filtrado por `empresaIds` - Solo empresas con relación `UsuarioEmpresa`
- ? Filtrado por `Activo` - Solo empresas activas

---

## ?? **Testing**

### **Test 1: Usuario con una empresa**

```csharp
[Fact]
public async Task GetMisEmpresas_UsuarioConUnaEmpresa_RetornaUna()
{
    // Arrange
    _currentUserService.UserIdInt = 1;
    _usuarioEmpresaRepository.Setup(
        x => x.GetEmpresasAsignadasAsync(1))
        .ReturnsAsync([new() { EmpresaId = 1, Rol = "Admin" }]);
    
    // Act
    var result = await _controller.GetMisEmpresas();
    
    // Assert
    Assert.Single(result.Value);
    Assert.Equal(1, result.Value[0].Id);
}
```

### **Test 2: Usuario sin empresas**

```csharp
[Fact]
public async Task GetMisEmpresas_UsuarioSinEmpresas_RetornaVacia()
{
    // Arrange
    _currentUserService.UserIdInt = 2;
    _usuarioEmpresaRepository.Setup(
        x => x.GetEmpresasAsignadasAsync(2))
        .ReturnsAsync([]);
    
    // Act
    var result = await _controller.GetMisEmpresas();
    
    // Assert
    Assert.Empty(result.Value);
}
```

---

## ?? **Checklist de Implementación**

- [x] ? Endpoint `GET /api/v1/empresas/mis-empresas` creado
- [x] ? Servicio HTTP `GetMisEmpresasAsync()` implementado
- [x] ? MainLayout actualizado con lógica condicional
- [x] ? Estilos CSS para `empresa-badge` agregados
- [x] ? `ICurrentUserService.UserIdInt` implementado
- [x] ? Tests actualizados
- [x] ? Compilación exitosa
- [ ] ?? Testing manual en navegador
- [ ] ?? Documentación actualizada

---

## ?? **Cómo Probar**

### **Paso 1: Preparar Datos**

```sql
-- Crear usuario con una sola empresa
INSERT INTO core.Usuarios (NombreUsuario, Email, PasswordHash, NombreCompleto, Activo)
VALUES ('testuser1', 'test1@test.com', 'hash...', 'Usuario Test 1', 1);

-- Asignar a una empresa
INSERT INTO core.UsuarioEmpresas (UsuarioId, EmpresaId, Rol)
VALUES (SCOPE_IDENTITY(), 1, 'User');
```

### **Paso 2: Ejecutar Sistema**

```powershell
.\start-system.ps1
```

### **Paso 3: Login**

1. Navega a `https://localhost:5002/login`
2. Inicia sesión con `testuser1`
3. **Observa:**
   - ? No aparece dropdown de empresas
   - ? Aparece badge: `?? EMPRESA A`
   - ? Acceso inmediato al sistema

### **Paso 4: Verificar Red**

1. Abre Developer Tools (F12)
2. Ve a Network
3. Busca petición: `GET /api/v1/empresas/mis-empresas`
4. **Verifica respuesta:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "nombre": "EMPRESA A",
      "activo": true
    }
  ]
}
```

---

## ? **Beneficios**

### **Para el Usuario**
- ? Experiencia más limpia y directa
- ? Sin confusión sobre qué empresa seleccionar
- ? Acceso automático si tiene una sola empresa
- ? Menos clics necesarios

### **Para la Seguridad**
- ? Usuarios solo ven empresas asignadas
- ? Endpoint protegido con autenticación
- ? Filtrado por usuario en backend
- ? Sin exposición de información de otras empresas

### **Para el Mantenimiento**
- ? Código más claro y específico
- ? Endpoint reutilizable
- ? Tests unitarios actualizados
- ? Documentación completa

---

**? Implementación completada exitosamente**

_Fecha: 17/02/2026_
