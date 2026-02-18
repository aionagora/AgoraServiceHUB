# ?? Diagnóstico: Error al Guardar Nuevo Parámetro

## ? Mejoras Implementadas

Se han agregado mejoras de logging y manejo de errores para diagnosticar el problema:

### 1?? **ParametroHttpService** - Manejo de Errores Mejorado

**Antes:**
```csharp
public async Task<ApiResponse<ParametroSistemaDto>> UpsertAsync(UpsertParametroDto dto)
{
    var response = await _http.PostAsJsonAsync(BaseUrl, dto);
    return await response.Content.ReadFromJsonAsync<ApiResponse<ParametroSistemaDto>>()
        ?? ApiResponse<ParametroSistemaDto>.Fail("Error de comunicación.");
}
```

**Ahora:**
```csharp
public async Task<ApiResponse<ParametroSistemaDto>> UpsertAsync(UpsertParametroDto dto)
{
    try
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return ApiResponse<ParametroSistemaDto>.Fail(
                $"Error HTTP {(int)response.StatusCode}: {errorContent}");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ParametroSistemaDto>>();
        return result ?? ApiResponse<ParametroSistemaDto>.Fail("Respuesta vacía del servidor.");
    }
    catch (Exception ex)
    {
        return ApiResponse<ParametroSistemaDto>.Fail($"Error de comunicación: {ex.Message}");
    }
}
```

### 2?? **SaveParametroAsync** - Logging en el Frontend

Se agregó logging detallado:
```csharp
private async Task SaveParametroAsync()
{
    _saving = true;
    _mensaje = null;
    _esError = false;
    
    try
    {
        Console.WriteLine($"[DEBUG] Guardando parámetro: Clave={_paramModel.Clave}, Valor={_paramModel.Valor}");
        
        var result = await ParamSvc.UpsertAsync(_paramModel);
        
        Console.WriteLine($"[DEBUG] Resultado: Success={result.Success}, Message={result.Message}");
        
        if (result.Success)
        {
            _mensaje = "Parámetro guardado exitosamente.";
            _esError = false;
            CloseParamForm();
            _parametros = await ParamSvc.GetAllAsync();
            
            Console.WriteLine($"[DEBUG] Parámetros recargados: Count={_parametros.Count}");
        }
        else 
        { 
            _mensaje = $"Error al guardar: {result.Message}";
            _esError = true;
            Console.WriteLine($"[ERROR] {_mensaje}");
        }
    }
    catch (Exception ex)
    { 
        _mensaje = $"Error de comunicación: {ex.Message}";
        _esError = true;
        Console.WriteLine($"[EXCEPTION] {ex}");
    }
    finally
    {
        _saving = false;
    }
}
```

### 3?? **ParametrosController** - Logging en el Backend

Se agregó logging en el controlador:
```csharp
[HttpPost]
public async Task<IActionResult> Upsert([FromBody] UpsertParametroDto dto, CancellationToken ct)
{
    _logger.LogInformation("Upsert parámetro: Clave={Clave}, Valor={Valor}", dto.Clave, dto.Valor);
    
    try
    {
        var result = await _service.UpsertAsync(dto, ct);
        
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Error al guardar parámetro: {Error}", result.Error);
            return BadRequest(ApiResponse<ParametroSistemaDto>.Fail(result.Error!));
        }

        _logger.LogInformation("Parámetro guardado exitosamente: Id={Id}", result.Value!.Id);
        return Ok(ApiResponse<ParametroSistemaDto>.Ok(result.Value!, "Parámetro guardado exitosamente."));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Excepción al guardar parámetro");
        return StatusCode(500, ApiResponse<ParametroSistemaDto>.Fail($"Error interno: {ex.Message}"));
    }
}
```

---

## ?? Pasos para Diagnosticar

### Paso 1: Iniciar el Sistema con Logging

```powershell
# Terminal 1 - Detener procesos previos
Get-Process | Where-Object {$_.ProcessName -like "*AgoraHub*"} | Stop-Process -Force

# Terminal 2 - Iniciar API con logs visibles
cd D:\AgoraCORE\AgoraHUB360-ERP\src\AgoraHub360.ERP.Api
dotnet run --launch-profile https

# Terminal 3 - Iniciar Web con logs visibles
cd D:\AgoraCORE\AgoraHUB360-ERP\src\AgoraHub360.ERP.Web
dotnet run --launch-profile https
```

### Paso 2: Abrir Consola del Navegador

1. Abre `https://localhost:5002/login`
2. Presiona `F12` para abrir Developer Tools
3. Ve a la pestaña **Console**
4. Mantén la consola abierta

### Paso 3: Intentar Crear Parámetro

1. Navega a: **Sistema ? Parámetros y Numeración**
2. Haz clic en **"Nuevo Parámetro"**
3. Completa el formulario:
   - **Clave:** `TEST_PARAM`
   - **Valor:** `TEST_VALUE`
   - **Descripción:** `Prueba de diagnóstico`
   - **Categoría:** `General`
   - **Tipo:** `String`
4. Haz clic en **"Guardar"**

### Paso 4: Revisar Logs

#### En la Consola del Navegador (F12):

Deberías ver mensajes como:
```
[DEBUG] Guardando parámetro: Clave=TEST_PARAM, Valor=TEST_VALUE
[DEBUG] Resultado: Success=true, Message=Parámetro guardado exitosamente
[DEBUG] Parámetros recargados: Count=1
```

Si hay error:
```
[ERROR] Error al guardar: No se pudo determinar la empresa activa del usuario.
```
O:
```
[EXCEPTION] System.Net.Http.HttpRequestException: ...
```

#### En la Terminal de la API:

Deberías ver:
```
info: AgoraHub360.ERP.Api.Controllers.V1.ParametrosController[0]
      Upsert parámetro: Clave=TEST_PARAM, Valor=TEST_VALUE
info: AgoraHub360.ERP.Api.Controllers.V1.ParametrosController[0]
      Parámetro guardado exitosamente: Id=1
```

Si hay error:
```
warn: AgoraHub360.ERP.Api.Controllers.V1.ParametrosController[0]
      Error al guardar parámetro: No se pudo determinar la empresa activa del usuario.
```

---

## ?? Posibles Causas del Error

### Causa 1: Token JWT sin EmpresaId

**Síntoma:** Mensaje de error "No se pudo determinar la empresa activa del usuario"

**Solución:**
1. Verifica que el JWT contenga el claim `EmpresaId`
2. Revisa el `AuthService` en `src/AgoraHub360.ERP.Api/Services/AuthService.cs`
3. El método `GenerateJwtToken` debe incluir:
   ```csharp
   if (empresaId.HasValue)
       claims.Add(new Claim("EmpresaId", empresaId.Value.ToString()));
   ```

**Test:**
```powershell
# Decodificar el JWT y verificar claims
# Abre la consola del navegador y ejecuta:
localStorage.getItem('agorahub360_auth_token')

# Copia el token y decodifícalo en: https://jwt.io
# Verifica que contenga: "EmpresaId": "1"
```

### Causa 2: Usuario sin Empresa Asignada

**Síntoma:** Token sin `EmpresaId`

**Solución:**
```sql
-- Verificar asignación de empresa
SELECT * FROM core.UsuarioEmpresas WHERE UsuarioId = 1;

-- Si no existe, crear
INSERT INTO core.UsuarioEmpresas (UsuarioId, EmpresaId, Rol)
VALUES (1, 1, 'Admin');

-- Verificar usuario
SELECT * FROM core.Usuarios WHERE Id = 1;
UPDATE core.Usuarios SET EmpresaActivaId = 1 WHERE Id = 1;
```

### Causa 3: Error de CORS

**Síntoma:** Error en consola: "CORS policy blocked..."

**Solución:**
Verifica `appsettings.json` de la API:
```json
{
  "BlazorBaseUrl": "https://localhost:5002"
}
```

Y que el startup configure CORS correctamente.

### Causa 4: Error de Autenticación

**Síntoma:** Error 401 Unauthorized

**Solución:**
1. Cierra sesión y vuelve a iniciar sesión
2. Verifica que el token esté en localStorage
3. Verifica que el header `Authorization` se esté enviando

**Test en consola del navegador:**
```javascript
console.log(localStorage.getItem('agorahub360_auth_token'));
```

### Causa 5: Validación de Modelo Fallida

**Síntoma:** Error 400 Bad Request con mensaje de validación

**Verificar:**
- Clave no puede estar vacía
- Valor no puede estar vacío
- Clave debe ser única en la empresa
- MaxLength de campos no excedido

---

## ?? Checklist de Diagnóstico

- [ ] API corriendo en puerto 7001
- [ ] Web corriendo en puerto 5002
- [ ] Usuario logueado correctamente
- [ ] Token JWT en localStorage
- [ ] Token contiene claim `EmpresaId`
- [ ] Usuario tiene empresa asignada en BD
- [ ] Consola del navegador sin errores
- [ ] Logs de API mostrando la petición
- [ ] Formulario de parámetro completado correctamente

---

## ??? Comandos Útiles de Diagnóstico

### Verificar Token en el Navegador
```javascript
// Abrir consola (F12) y ejecutar:
const token = localStorage.getItem('agorahub360_auth_token');
console.log('Token:', token);

// Decodificar base64 del payload
const payload = token.split('.')[1];
const decoded = JSON.parse(atob(payload));
console.log('Claims:', decoded);
console.log('EmpresaId:', decoded.EmpresaId);
```

### Verificar Base de Datos
```sql
-- Verificar empresa existe
SELECT * FROM core.Empresas WHERE Id = 1;

-- Verificar usuario y empresa activa
SELECT 
    u.Id,
    u.NombreUsuario,
    u.Email,
    u.EmpresaActivaId,
    ue.EmpresaId,
    ue.Rol
FROM core.Usuarios u
LEFT JOIN core.UsuarioEmpresas ue ON u.Id = ue.UsuarioId
WHERE u.Id = 1;

-- Verificar parámetros existentes
SELECT * FROM core.ParametrosSistema;
```

### Test de API con PowerShell
```powershell
# Obtener token
$login = @{
    email = "admin@agorahub360.com"
    password = "Admin123"
} | ConvertTo-Json

[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
$response = Invoke-RestMethod -Uri "https://localhost:7001/api/v1/auth/login" `
    -Method Post -Body $login -ContentType "application/json"

$token = $response.data.token
Write-Host "Token: $token"

# Crear parámetro
$param = @{
    clave = "TEST_PARAM"
    valor = "TEST_VALUE"
    descripcion = "Test"
    categoria = "General"
    tipoDato = "String"
} | ConvertTo-Json

$headers = @{ Authorization = "Bearer $token" }
$result = Invoke-RestMethod -Uri "https://localhost:7001/api/v1/parametros" `
    -Method Post -Body $param -ContentType "application/json" -Headers $headers

Write-Host "Resultado:" 
$result | ConvertTo-Json -Depth 5
```

---

## ?? Reportar el Error

Una vez que identifiques el error específico, proporciona:

1. **Mensaje de error exacto** de la consola del navegador
2. **Logs de la API** donde se muestra el error
3. **Código HTTP** de la respuesta (200, 400, 401, 500, etc.)
4. **Claims del JWT** (EmpresaId presente o no)
5. **Datos del formulario** que intentaste guardar

---

**? Siguiente paso:** Ejecuta el sistema con estos cambios y reporta los logs que veas en la consola del navegador y en la terminal de la API.

_Fecha: 17/02/2026_
