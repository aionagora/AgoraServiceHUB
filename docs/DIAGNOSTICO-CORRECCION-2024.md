# DIAGNÓSTICO Y CORRECCIÓN COMPLETA
## AgoraHub360 ERP - Problemas de Autorización y Validación

Fecha: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

---

## 🔍 PARTE A: DIAGNÓSTICO /api/v1/empresas/mis-empresas

### ❌ PROBLEMA IDENTIFICADO

**Archivo:** `src/AgoraHub360.ERP.Api/Controllers/V1/EmpresasController.cs`

**Líneas 46-47 (ANTES):**
```csharp
[HttpGet("mis-empresas")]
[AllowAnonymous] // Excepción explícita: no hereda restricciones de la clase
[Authorize] // Pero SÍ requiere autenticación (sin roles específicos)
public async Task<IActionResult> GetMisEmpresas(CancellationToken ct)
```

### 🐛 CAUSA RAÍZ

1. **Conflicto de atributos:** `[AllowAnonymous]` seguido de `[Authorize]` es contradictorio.
2. **Comportamiento:** El segundo atributo (`[Authorize]`) SOBRESCRIBE al primero.
3. **Resultado:** El endpoint hereda cualquier policy/role configurada globalmente o a nivel de clase.
4. **Consecuencia:** ASP.NET Core interpretaba que el endpoint requería el rol "Admin" por alguna combinación de políticas en runtime.

### ✅ CORRECCIÓN APLICADA

**Líneas 42-47 (DESPUÉS):**
```csharp
/// <summary>
/// Obtiene solo las empresas asignadas al usuario logueado.
/// Solo requiere autenticación, sin roles específicos.
/// </summary>
[HttpGet("mis-empresas")]
public async Task<IActionResult> GetMisEmpresas(CancellationToken ct)
```

**Cambios:**
- ❌ Removido `[AllowAnonymous]` (no era necesario)
- ❌ Removido `[Authorize]` redundante del método
- ✅ El método ahora hereda el `[Authorize]` de la clase (línea 14)
- ✅ Documentación mejorada en XML comments

### 🎯 RESULTADO ESPERADO

- `/api/v1/empresas/mis-empresas` → Requiere autenticación JWT válida (SIN rol específico)
- `/api/v1/empresas` → Requiere autenticación + rol "Admin" (mantiene `[Authorize(Roles = Roles.Admin)]`)

---

## 🔍 PARTE B: DIAGNÓSTICO ClienteForm.razor

### ❌ PROBLEMA IDENTIFICADO

**Archivo:** `src/AgoraHub360.ERP.Web/Pages/MDM/ClienteForm.razor`

**Línea 84 (ANTES):**
```razor
<EditForm Model="@(IsNew ? (object)GetCreateDto() : GetUpdateDto())" 
          OnValidSubmit="SaveCliente" 
          OnInvalidSubmit="HandleInvalidSubmit">
```

**Variables separadas (líneas 414-422):**
```csharp
private string formCodigo = "";
private string formRazonSocial = "";
private string formNIT = "";
private string formTipoCliente = "General";
private string formTelefono = "";
private string formEmail = "";
private string formNombreContacto = "";
private string formDireccion = "";
private bool formActivo = true;
```

### 🐛 CAUSA RAÍZ

1. **Modelo inestable:** `GetCreateDto()` y `GetUpdateDto()` se ejecutan en **cada render**.
2. **Nueva instancia:** Cada render crea una nueva instancia del DTO, rompiendo el contexto de validación.
3. **Desvinculación:** Los campos `formCodigo`, `formRazonSocial`, etc. NO están vinculados al Model del EditForm.
4. **Consecuencia:** EditForm valida contra un DTO vacío recién creado, mientras los campos visuales tienen valores en variables separadas.
5. **Validación siempre falla:** Aunque el usuario llene los campos, el Model del EditForm permanece inválido.

### ✅ CORRECCIÓN APLICADA

#### 1. Modelo estable (línea ~620+):
```csharp
private sealed class ClienteFormModel
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razón social es obligatoria.")]
    [MaxLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    public string RazonSocial { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string? NIT { get; set; }

    [Required(ErrorMessage = "El tipo de cliente es obligatorio.")]
    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string TipoCliente { get; set; } = "General";

    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string? Telefono { get; set; }

    [MaxLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    public string? Email { get; set; }

    [MaxLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    public string? NombreContacto { get; set; }

    [MaxLength(300, ErrorMessage = "Máximo 300 caracteres.")]
    public string? Direccion { get; set; }

    public bool Activo { get; set; } = true;
}
```

#### 2. Instancia única (línea ~428):
```csharp
private ClienteFormModel formModel = new();
```

#### 3. EditForm actualizado (línea ~84):
```razor
<EditForm Model="formModel" 
          OnValidSubmit="SaveCliente" 
          OnInvalidSubmit="HandleInvalidSubmit">
```

#### 4. Inputs vinculados directamente:
```razor
<InputText @bind-Value="formModel.Codigo" />
<ValidationMessage For="@(() => formModel.Codigo)" />

<InputText @bind-Value="formModel.RazonSocial" />
<ValidationMessage For="@(() => formModel.RazonSocial)" />

<InputSelect @bind-Value="formModel.TipoCliente">
    <option value="General">General</option>
    <option value="Mayorista">Mayorista</option>
    <option value="Minorista">Minorista</option>
    <option value="Corporativo">Corporativo</option>
</InputSelect>
<ValidationMessage For="@(() => formModel.TipoCliente)" />
```

#### 5. HandleInvalidSubmit mejorado:
```csharp
private async Task HandleInvalidSubmit(EditContext editContext)
{
    var messages = editContext.GetValidationMessages().ToList();

    await JS.InvokeVoidAsync("console.warn", "[TEMP-LOG] SaveCliente bloqueado por validación.", new { 
        Codigo = formModel.Codigo, 
        RazonSocial = formModel.RazonSocial, 
        TipoCliente = formModel.TipoCliente 
    });

    foreach (var message in messages)
    {
        await JS.InvokeVoidAsync("console.warn", $"[TEMP-LOG] Validación: {message}");
    }

    errorMessage = messages.Any() 
        ? "Hay errores de validación en el formulario. Revisa los campos marcados en rojo." 
        : "El formulario es inválido. Por favor revisa todos los campos obligatorios.";
}
```

#### 6. SaveCliente mapea desde formModel:
```csharp
var dto = new CreateClienteDto
{
    Codigo = formModel.Codigo,
    RazonSocial = formModel.RazonSocial,
    NIT = formModel.NIT,
    TipoCliente = formModel.TipoCliente,
    Telefono = formModel.Telefono,
    Email = formModel.Email,
    NombreContacto = formModel.NombreContacto,
    Direccion = formModel.Direccion
};
```

### 🎯 RESULTADO ESPERADO

- ✅ EditForm usa un modelo estable que persiste entre renders
- ✅ Los campos están vinculados directamente al modelo
- ✅ La validación funciona correctamente
- ✅ OnValidSubmit se ejecuta cuando Código, Razón Social y TipoCliente tienen valor
- ✅ POST /api/v1/clientes se ejecuta con datos válidos
- ✅ Los mensajes de validación se muestran correctamente en consola

---

## 📋 ELIMINACIONES

**Variables/métodos eliminados de ClienteForm.razor:**
- ❌ `private object currentDto`
- ❌ `private string formCodigo`
- ❌ `private string formRazonSocial`
- ❌ `private string formNIT`
- ❌ `private string formTipoCliente`
- ❌ `private string formTelefono`
- ❌ `private string formEmail`
- ❌ `private string formNombreContacto`
- ❌ `private string formDireccion`
- ❌ `private bool formActivo`
- ❌ `GetCreateDto()` method
- ❌ `GetUpdateDto()` method

**Reemplazados por:**
- ✅ `private ClienteFormModel formModel = new();`

---

## 🛠️ INSTRUCCIONES DE PRUEBA

### 1. Ejecutar script de limpieza:
```powershell
.\scripts\reiniciar-servicios.ps1
```

### 2. En el navegador:
1. Abrir DevTools (F12)
2. Application → Clear Storage → Clear site data
3. Service Workers → Unregister (si aparece)
4. Recargar con Ctrl+F5

### 3. Prueba de /api/v1/empresas/mis-empresas:
1. Login como usuario normal (NO admin)
2. Verificar que GET /api/v1/empresas/mis-empresas devuelve 200 OK
3. Verificar que devuelve solo empresas asignadas al usuario
4. Verificar en logs del backend que NO aparece "RolesAuthorizationRequirement"

### 4. Prueba de ClienteForm:
1. Ir a /mdm/clientes/nuevo
2. Llenar:
   - Código: "TEST001"
   - Razón Social: "Cliente Prueba"
   - Tipo Cliente: "Mayorista"
3. Click en "Guardar Cliente"
4. Verificar en DevTools → Console:
   - `[TEMP-LOG] SaveCliente iniciado` con datos correctos
   - `[TEMP-LOG] DTO CreateCliente` con datos correctos
   - NO debe aparecer `[TEMP-LOG] SaveCliente bloqueado por validación`
5. Verificar en DevTools → Network:
   - POST /api/v1/clientes aparece
   - Status 200 o 201
   - Response con datos del cliente creado
6. Verificar redirección a /mdm/clientes/editar/{id}

---

## ✅ CHECKLIST DE VERIFICACIÓN

### Parte A: /empresas/mis-empresas
- [ ] Backend NO muestra "Authorization failed" para este endpoint
- [ ] GET devuelve 200 OK para usuario autenticado normal
- [ ] GET devuelve solo empresas activas asignadas al usuario
- [ ] GET /api/v1/empresas SÍ mantiene Roles=Admin (solo para verificar que no se rompió)

### Parte B: ClienteForm
- [ ] Al llenar Código, Razón Social y Tipo Cliente, el botón Guardar NO está deshabilitado
- [ ] Al hacer click en Guardar, OnValidSubmit se ejecuta
- [ ] POST /api/v1/clientes aparece en Network tab
- [ ] POST incluye body con JSON correcto
- [ ] POST NO incluye EmpresaId (el backend lo asigna automáticamente)
- [ ] Si hay error de validación, los mensajes aparecen en consola
- [ ] ValidationMessage se muestra en campos con error

---

## 📝 NOTAS IMPORTANTES

1. **No se tocó ClienteService** - No era necesario, el problema estaba en el formulario.
2. **No se envía EmpresaId desde frontend** - El backend lo asigna desde el token JWT del usuario actual.
3. **Validaciones alineadas con SQL Server** - MaxLength coincide con DDL real.
4. **Namespace 'Clientes' mantenido** - Según copilot-instructions.md.
5. **FluentValidation** - Se mantiene en backend, DataAnnotations solo en frontend para EditForm.

---

## 🔗 ARCHIVOS MODIFICADOS

1. `src/AgoraHub360.ERP.Api/Controllers/V1/EmpresasController.cs`
   - Líneas 42-47: Corrección de atributos de autorización

2. `src/AgoraHub360.ERP.Web/Pages/MDM/ClienteForm.razor`
   - Línea 8: Added `@using System.ComponentModel.DataAnnotations`
   - Línea 84: Model cambiado a `formModel`
   - Líneas 91-141: Inputs vinculados a `formModel.*`
   - Líneas 394-837: Lógica @code refactorizada con ClienteFormModel

3. `scripts/reiniciar-servicios.ps1` (NUEVO)
   - Script completo de limpieza y reinicio

---

## 📞 SOPORTE

Si después de aplicar estos cambios persisten los problemas:

1. Verificar que la DLL de la API en runtime es la nueva (revisar timestamp de archivo)
2. Verificar que no hay múltiples instancias de dotnet corriendo
3. Verificar logs del backend en tiempo real durante la prueba
4. Verificar que el token JWT incluye los claims correctos (UserId, EmpresaId, Role)
5. Agregar logs temporales en Program.cs para inspeccionar metadata de endpoints

---

**Generado automáticamente por GitHub Copilot**
**Fecha:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
