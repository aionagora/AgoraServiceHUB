# ?? Troubleshooting - Error al cargar Unidades de Medida

## ? Problema
```
Error al cargar unidades de medida.
```

## ?? Diagnóstico

### 1?? Verificar que la API esté corriendo

```powershell
# Verificar proceso
Get-Process | Where-Object {$_.ProcessName -like "*AgoraHub360.ERP.Api*"}

# O revisar si el puerto está en uso
Test-NetConnection -ComputerName localhost -Port 7001
```

**Solución:** Si no está corriendo:
```powershell
cd src\AgoraHub360.ERP.Api
dotnet run --launch-profile https
```

---

### 2?? Verificar configuración de API Base URL

**Archivo:** `src/AgoraHub360.ERP.Web/wwwroot/appsettings.json`

```json
{
  "ApiBaseUrl": "https://localhost:7001/"
}
```

**?? Importante:** 
- Debe terminar con `/`
- Debe ser `https://` no `http://`
- Puerto debe ser `7001` (por defecto)

---

### 3?? Verificar autenticación

El error puede ser porque el token JWT expiró o no es válido.

**Solución:**
1. Cerrar sesión
2. Volver a hacer login con:
   - Email: `admin@agorahub360.com`
   - Password: `Admin123`

---

### 4?? Verificar que la base de datos tenga datos

```sql
-- Conectar a SQL Server
USE AgoraHub360_ERP;

-- Verificar empresa existe
SELECT * FROM core.Empresas;

-- Verificar unidades de medida
SELECT * FROM mdm.UnidadesMedida;

-- Si no hay datos, crear una unidad de prueba
INSERT INTO mdm.UnidadesMedida (Nombre, Abreviatura, EmpresaId, Activo, FechaCreacion)
VALUES ('Kilogramo', 'Kg', 1, 1, GETUTCDATE());
```

---

### 5?? Ver errores en DevTools del navegador

1. Abrir DevTools (F12)
2. Ir a pestaña **Console**
3. Buscar errores en rojo
4. Ir a pestaña **Network**
5. Filtrar por `unidades-medida`
6. Ver el status code de la respuesta

**Posibles errores:**

| Status Code | Problema | Solución |
|-------------|----------|----------|
| 401 Unauthorized | Token inválido | Volver a hacer login |
| 404 Not Found | Endpoint incorrecto | Verificar URL base |
| 500 Internal Server Error | Error en el servidor | Ver logs de la API |
| CORS Error | Configuración CORS | Ver configuración API |

---

### 6?? Verificar logs de la API

Los logs de la API aparecen en la terminal donde ejecutaste:
```powershell
cd src\AgoraHub360.ERP.Api
dotnet run --launch-profile https
```

Buscar líneas con:
- `fail:`
- `error:`
- `exception:`
- `GET /api/v1/unidades-medida`

---

### 7?? Verificar CORS

**Archivo:** `src/AgoraHub360.ERP.Api/Program.cs`

Debe tener:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorWasm", policy =>
    {
        policy.WithOrigins("https://localhost:5002")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Y después de app:
app.UseCors("BlazorWasm");
```

---

### 8?? Test manual del endpoint

Probar el endpoint directamente desde Swagger o curl:

**Swagger:**
```
https://localhost:7001/swagger/index.html
```
Buscar: `UnidadesMedida` ? `GET /api/v1/unidades-medida` ? Try it out ? Execute

**Curl:**
```powershell
curl -X GET "https://localhost:7001/api/v1/unidades-medida" `
  -H "Authorization: Bearer TU_TOKEN_JWT" `
  -k
```

---

### 9?? Verificar que el servicio esté registrado

**Archivo:** `src/AgoraHub360.ERP.Application/DependencyInjection.cs`

Debe contener:
```csharp
services.AddScoped<IUnidadMedidaService, UnidadMedidaService>();
```

**Archivo:** `src/AgoraHub360.ERP.Web/Program.cs`

Debe contener:
```csharp
builder.Services.AddScoped<UnidadMedidaHttpService>();
```

? **VERIFICADO:** Ambos están registrados correctamente.

---

### ?? Verificar HttpClient BaseAddress

**Archivo:** `src/AgoraHub360.ERP.Web/Program.cs`

```csharp
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl")
    ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});
```

**Debug:** Agregar console.log temporal:
```csharp
Console.WriteLine($"[DEBUG] API Base URL: {apiBaseUrl}");
```

---

## ?? Errores Comunes y Soluciones

### Error: "Failed to fetch"
**Causa:** La API no está corriendo  
**Solución:** Iniciar la API

### Error: "401 Unauthorized"
**Causa:** Token JWT expiró  
**Solución:** Volver a hacer login

### Error: "404 Not Found"
**Causa:** URL del endpoint incorrecta  
**Solución:** Verificar que sea `/api/v1/unidades-medida` (no `unidades_medida`)

### Error: "CORS policy"
**Causa:** Configuración CORS incorrecta  
**Solución:** Verificar que la API tenga configurado el origen correcto

### Error: "No se pudo determinar la empresa activa"
**Causa:** El usuario no tiene una empresa asignada  
**Solución:** 
1. Ir a Configuración ? Empresas
2. Crear o verificar que existe una empresa
3. Verificar que el usuario esté asociado a esa empresa

---

## ? Checklist de Verificación

Marca cada item cuando lo hayas verificado:

- [ ] 1. La API está corriendo en https://localhost:7001
- [ ] 2. La Web está corriendo en https://localhost:5002
- [ ] 3. El archivo appsettings.json tiene ApiBaseUrl correcto
- [ ] 4. Puedo hacer login correctamente
- [ ] 5. Existe al menos una empresa en la BD
- [ ] 6. El usuario está asociado a una empresa
- [ ] 7. En DevTools no hay errores 401/404/500
- [ ] 8. En DevTools no hay errores CORS
- [ ] 9. Swagger muestra el endpoint /api/v1/unidades-medida
- [ ] 10. Puedo ejecutar el endpoint desde Swagger

---

## ?? Solución Rápida (Script)

```powershell
# Script de diagnóstico rápido
Write-Host "?? Diagnóstico de UnidadMedida..." -ForegroundColor Yellow

# 1. Verificar procesos
$apiProcess = Get-Process | Where-Object {$_.ProcessName -like "*AgoraHub360.ERP.Api*"}
$webProcess = Get-Process | Where-Object {$_.ProcessName -like "*AgoraHub360.ERP.Web*"}

if ($apiProcess) {
    Write-Host "? API corriendo (PID: $($apiProcess.Id))" -ForegroundColor Green
} else {
    Write-Host "? API NO está corriendo" -ForegroundColor Red
    Write-Host "   Ejecuta: cd src\AgoraHub360.ERP.Api; dotnet run" -ForegroundColor Yellow
}

if ($webProcess) {
    Write-Host "? Web corriendo (PID: $($webProcess.Id))" -ForegroundColor Green
} else {
    Write-Host "? Web NO está corriendo" -ForegroundColor Red
    Write-Host "   Ejecuta: cd src\AgoraHub360.ERP.Web; dotnet run" -ForegroundColor Yellow
}

# 2. Test de conexión
Write-Host "`n?? Probando conexión a la API..." -ForegroundColor Yellow
try {
    $result = Invoke-WebRequest -Uri "https://localhost:7001/health" -SkipCertificateCheck
    if ($result.StatusCode -eq 200) {
        Write-Host "? API responde correctamente" -ForegroundColor Green
    }
} catch {
    Write-Host "? No se puede conectar a la API" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Gray
}

# 3. Verificar archivo de configuración
$configPath = "src\AgoraHub360.ERP.Web\wwwroot\appsettings.json"
if (Test-Path $configPath) {
    Write-Host "`n?? Configuración encontrada" -ForegroundColor Green
    $config = Get-Content $configPath | ConvertFrom-Json
    Write-Host "   ApiBaseUrl: $($config.ApiBaseUrl)" -ForegroundColor Cyan
} else {
    Write-Host "`n? Archivo de configuración NO encontrado" -ForegroundColor Red
}

Write-Host "`n??????????????????????????????????????????????????????????" -ForegroundColor DarkGray
Write-Host "Diagnóstico completado" -ForegroundColor Yellow
```

Guarda este script como `diagnose-unidadmedida.ps1` y ejecútalo:
```powershell
.\diagnose-unidadmedida.ps1
```

---

## ?? Si el problema persiste

1. **Compartir logs de la API**
   - Copiar todo el output de la terminal donde corre la API
   - Buscar el momento en que se ejecutó `GET /api/v1/unidades-medida`

2. **Compartir errores de DevTools**
   - F12 ? Console ? Copiar errores en rojo
   - F12 ? Network ? Filtrar por `unidades-medida` ? Click derecho ? Copy as cURL

3. **Compartir configuración**
   - Contenido de `appsettings.json` (Web)
   - Contenido de `appsettings.json` (Api)

---

## ?? Referencias

- [API Troubleshooting](DATABASE-TEST-GUIDE.md)
- [Login Error Fix](LOGIN-ERROR-FIX.md)
- [Connection Troubleshooting](DATABASE-CONNECTION-FIX.md)

---

**Última actualización:** 2026-02-17  
**Mantenido por:** Equipo AgoraHUB360 ERP
