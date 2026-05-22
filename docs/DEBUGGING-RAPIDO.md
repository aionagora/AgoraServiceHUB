# 🔧 COMANDOS DE DEBUGGING RÁPIDO

## 🚨 SI PERSISTE ERROR 401 EN /empresas/mis-empresas

### 1. Verificar metadata del endpoint en runtime
Agregar temporalmente en `Program.cs` después de `app.MapControllers();`:

```csharp
app.Use(async (context, next) =>
{
    var endpoint = context.GetEndpoint();
    if (endpoint != null && context.Request.Path.StartsWithSegments("/api/v1/empresas/mis-empresas"))
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("═══ ENDPOINT METADATA DEBUG ═══");
        logger.LogWarning($"Path: {context.Request.Path}");
        logger.LogWarning($"DisplayName: {endpoint.DisplayName}");

        var authorizeData = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
        foreach (var auth in authorizeData)
        {
            logger.LogWarning($"Policy: {auth.Policy ?? "(null)"}");
            logger.LogWarning($"Roles: {auth.Roles ?? "(null)"}");
            logger.LogWarning($"AuthenticationSchemes: {auth.AuthenticationSchemes ?? "(null)"}");
        }
        logger.LogWarning("═══════════════════════════════");
    }
    await next();
});
```

**Resultado esperado:**
```
Path: /api/v1/empresas/mis-empresas
DisplayName: AgoraHub360.ERP.Api.Controllers.V1.EmpresasController.GetMisEmpresas (AgoraHub360.ERP.Api)
Policy: (null)
Roles: (null)
AuthenticationSchemes: (null)
```

**Si aparece `Roles: Admin`** → hay un problema de caché o compilación.

---

## 🚨 SI PERSISTE VALIDACIÓN EN ClienteForm

### 1. Verificar estado del EditForm en runtime
En `ClienteForm.razor`, modificar temporalmente el EditForm:

```razor
<EditForm Model="formModel" OnValidSubmit="SaveCliente" OnInvalidSubmit="HandleInvalidSubmit">
    <DataAnnotationsValidator />

    <!-- DEBUG: Mostrar estado del modelo -->
    <div class="alert alert-info">
        <strong>DEBUG - Estado del Modelo:</strong><br />
        Código: [@formModel.Codigo] - Válido: @(!string.IsNullOrWhiteSpace(formModel.Codigo))<br />
        Razón Social: [@formModel.RazonSocial] - Válido: @(!string.IsNullOrWhiteSpace(formModel.RazonSocial))<br />
        Tipo Cliente: [@formModel.TipoCliente] - Válido: @(!string.IsNullOrWhiteSpace(formModel.TipoCliente))<br />
        Modelo válido: @(new ValidationContext(formModel).GetValidationErrors().Count() == 0)
    </div>

    <ValidationSummary class="alert alert-danger" />

    <!-- resto del formulario -->
</EditForm>
```

**Resultado esperado al llenar campos:**
```
Código: [TEST001] - Válido: True
Razón Social: [Cliente Prueba] - Válido: True
Tipo Cliente: [Mayorista] - Válido: True
Modelo válido: True
```

---

## 🚨 SI EL POST NO LLEGA AL BACKEND

### 1. Verificar ClienteHttpService
Revisar `src/AgoraHub360.ERP.Web/Services/ClienteHttpService.cs`:

```csharp
public async Task<ApiResponse<ClienteDto>> CreateAsync(CreateClienteDto dto)
{
    Console.WriteLine($"[DEBUG] ClienteHttpService.CreateAsync - DTO: {JsonSerializer.Serialize(dto)}");

    var response = await _httpClient.PostAsJsonAsync("clientes", dto);

    Console.WriteLine($"[DEBUG] Response Status: {response.StatusCode}");

    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>();
        Console.WriteLine($"[DEBUG] Response Data: {JsonSerializer.Serialize(result)}");
        return result!;
    }

    var errorContent = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"[DEBUG] Error Content: {errorContent}");

    return ApiResponse<ClienteDto>.Fail($"Error {response.StatusCode}: {errorContent}");
}
```

### 2. Verificar headers en Network tab
En DevTools → Network → POST /api/v1/clientes → Headers:

**Debe incluir:**
```
Authorization: Bearer eyJ...
Content-Type: application/json
X-Empresa-Id: {número}
```

**Si falta Authorization:**
→ Token JWT no está en LocalStorage o expiró

**Si falta X-Empresa-Id:**
→ Revisar EmpresaSelector.razor y LocalStorageService

---

## 🚨 SI EL BACKEND DEVUELVE 400 BAD REQUEST

### 1. Ver logs detallados del backend
En `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Debug",
      "AgoraHub360.ERP": "Debug"
    }
  }
}
```

### 2. Agregar logs en ClienteService backend
En `src/AgoraHub360.ERP.Application/Services/ClienteService.cs`:

```csharp
public async Task<Result<ClienteDto>> CreateAsync(CreateClienteDto dto, CancellationToken ct)
{
    _logger.LogInformation("═══ CREATE CLIENTE DEBUG ═══");
    _logger.LogInformation($"DTO: {JsonSerializer.Serialize(dto)}");
    _logger.LogInformation($"EmpresaId actual: {_currentUserService.EmpresaId}");
    _logger.LogInformation($"UserId actual: {_currentUserService.UserIdInt}");

    // Validación
    var validationResult = await _validator.ValidateAsync(dto, ct);
    if (!validationResult.IsValid)
    {
        _logger.LogWarning("Validación falló:");
        foreach (var error in validationResult.Errors)
        {
            _logger.LogWarning($"  - {error.PropertyName}: {error.ErrorMessage}");
        }
        return Result<ClienteDto>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
    }

    // resto del método...
}
```

---

## 🔍 VERIFICAR CACHÉ DE BLAZOR WASM

### 1. Limpiar caché del service worker
En DevTools → Application:

1. **Clear storage**
   - Uncheck "Cookies" si quieres mantener sesión
   - Check todo lo demás
   - Click "Clear site data"

2. **Service Workers**
   - Si aparece alguno registrado → Click "Unregister"

3. **Cache Storage**
   - Expandir "Cache Storage"
   - Borrar todas las entradas manualmente

### 2. Deshabilitar caché durante desarrollo
En `src/AgoraHub360.ERP.Web/Program.cs`:

```csharp
// Comentar o remover esta línea durante desarrollo:
// builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Reemplazar con:
builder.Services.AddScoped(sp =>
{
    var http = new HttpClient { BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!) };
    http.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
    {
        NoCache = true,
        NoStore = true
    };
    return http;
});
```

---

## 🔍 VERIFICAR QUE LA DLL CORRECTA ESTÁ CORRIENDO

### PowerShell:
```powershell
# Ver procesos dotnet
Get-Process dotnet | Select-Object Id, StartTime, Path

# Ver timestamp de DLL de la API
Get-Item .\src\AgoraHub360.ERP.Api\bin\Debug\net8.0\AgoraHub360.ERP.Api.dll | Select-Object LastWriteTime

# Ver timestamp de DLL del Web
Get-Item .\src\AgoraHub360.ERP.Web\bin\Debug\net8.0\AgoraHub360.ERP.Web.dll | Select-Object LastWriteTime

# Debe ser posterior a la hora de corrección
```

### Si el timestamp es antiguo:
```powershell
# Matar todos los dotnet
Get-Process dotnet | Stop-Process -Force

# Limpiar todo
dotnet clean
Remove-Item -Recurse -Force .\**\bin, .\**\obj

# Rebuild
dotnet restore
dotnet build
```

---

## 🔍 VERIFICAR TOKEN JWT

### En DevTools Console:
```javascript
// Ver token almacenado
localStorage.getItem('authToken')

// Decodificar token (copiar el valor y pegar en jwt.io)
// O usar esto:
function parseJwt(token) {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(c =>
        '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
    ).join(''));
    return JSON.parse(jsonPayload);
}

const token = localStorage.getItem('authToken');
if (token) {
    console.log('Token Claims:', parseJwt(token));
} else {
    console.log('No token found');
}
```

**Debe incluir:**
```json
{
  "sub": "1",
  "email": "testuser@agora.com",
  "name": "Test User",
  "role": "User",
  "empresaId": "1",
  "exp": 1234567890,
  "iss": "AgoraHub360.ERP",
  "aud": "AgoraHub360.ERP.Web"
}
```

---

## 🔍 PROBAR ENDPOINT DIRECTAMENTE

### Con curl (PowerShell):
```powershell
# Obtener token
$loginBody = @{
    Email = "testuser@agora.com"
    Password = "Test123!"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "https://localhost:7001/api/v1/auth/login" `
    -Method POST `
    -Body $loginBody `
    -ContentType "application/json" `
    -SkipCertificateCheck

$token = $loginResponse.data.token

# Probar /empresas/mis-empresas
$headers = @{
    "Authorization" = "Bearer $token"
}

Invoke-RestMethod -Uri "https://localhost:7001/api/v1/empresas/mis-empresas" `
    -Method GET `
    -Headers $headers `
    -SkipCertificateCheck
```

**Resultado esperado:** 200 OK con lista de empresas

---

## 🔍 PROBAR CREACIÓN DE CLIENTE DIRECTAMENTE

```powershell
# Usar el mismo $token de arriba

$createClienteBody = @{
    Codigo = "TEST999"
    RazonSocial = "Cliente Test API Directo"
    TipoCliente = "General"
} | ConvertTo-Json

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Invoke-RestMethod -Uri "https://localhost:7001/api/v1/clientes" `
    -Method POST `
    -Headers $headers `
    -Body $createClienteBody `
    -SkipCertificateCheck
```

**Resultado esperado:** 201 Created con datos del cliente creado

---

## 🚀 REINICIO RÁPIDO

```powershell
# Matar todo
Get-Process dotnet | Stop-Process -Force
Get-Process -Name "AgoraHub360*" -ErrorAction SilentlyContinue | Stop-Process -Force

# Liberar puertos
netstat -ano | findstr :7001 | ForEach-Object { 
    if ($_ -match "\s+(\d+)$") { 
        Stop-Process -Id $matches[1] -Force -ErrorAction SilentlyContinue 
    } 
}

netstat -ano | findstr :5002 | ForEach-Object { 
    if ($_ -match "\s+(\d+)$") { 
        Stop-Process -Id $matches[1] -Force -ErrorAction SilentlyContinue 
    } 
}

# Build limpio
dotnet clean
dotnet build

# Iniciar
Start-Process powershell -ArgumentList "-NoExit -Command cd '$PWD\src\AgoraHub360.ERP.Api'; dotnet run"
Start-Sleep 3
Start-Process powershell -ArgumentList "-NoExit -Command cd '$PWD\src\AgoraHub360.ERP.Web'; dotnet run"
```

---

## 📞 ÚLTIMO RECURSO

Si nada funciona:

1. **Verificar que el problema original sigue existiendo:**
   - Revertir cambios
   - Confirmar que el error 401 y validación aparecen
   - Re-aplicar cambios

2. **Comparar con repositorio limpio:**
   - Clonar repo en otra carpeta
   - Aplicar cambios manualmente
   - Comparar comportamiento

3. **Revisar logs del sistema:**
   - Event Viewer → Application logs
   - Buscar excepciones de ASP.NET Core

4. **Verificar firewall/antivirus:**
   - Puede estar bloqueando localhost:7001 o 5002
   - Agregar excepciones si es necesario

---

**Fecha:** 2024-02-17
**Autor:** GitHub Copilot + Abel Calvimontes
