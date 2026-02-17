# ?? Solución: Error de Conexión al Iniciar Sesión

## ? Problema Identificado

**Error:** "Error de conexión con el servidor" al intentar iniciar sesión.

**Causa raíz:** Desincronización de puertos entre la configuración de la API y la aplicación Web.

---

## ?? Diagnóstico

### Problema 1: Puertos Incorrectos en la API
**Antes:**
- API configurada en: `https://localhost:7284`
- Documentación indicaba: `https://localhost:7001`

### Problema 2: Puertos Incorrectos en la Web
**Antes:**
- Web configurada en: `https://localhost:7112`
- Documentación indicaba: `https://localhost:5002`

### Problema 3: Falta de Configuración de API Base URL
- La aplicación Web no tenía configurada la URL de la API
- Usaba el `HostEnvironment.BaseAddress` por defecto

---

## ? Solución Implementada

### 1. Corrección de Puertos de la API

**Archivo:** `src/AgoraHub360.ERP.Api/Properties/launchSettings.json`

```json
{
  "profiles": {
    "https": {
      "applicationUrl": "https://localhost:7001;http://localhost:5000"
    }
  }
}
```

### 2. Corrección de Puertos de la Web

**Archivo:** `src/AgoraHub360.ERP.Web/Properties/launchSettings.json`

```json
{
  "profiles": {
    "https": {
      "applicationUrl": "https://localhost:5002;http://localhost:5001"
    }
  }
}
```

### 3. Configuración de API Base URL

**Archivo creado:** `src/AgoraHub360.ERP.Web/wwwroot/appsettings.json`

```json
{
  "ApiBaseUrl": "https://localhost:7001/"
}
```

### 4. Script de Inicio Automatizado

**Archivo creado:** `start-system.ps1`

Características:
- ? Detiene procesos existentes
- ? Inicia API en ventana separada
- ? Espera a que la API esté lista
- ? Inicia Web en ventana separada
- ? Verifica conectividad
- ? Abre el navegador automáticamente

---

## ?? Cómo Usar

### Opción 1: Script Automatizado (Recomendado)

```powershell
.\start-system.ps1
```

El script:
1. Detiene aplicaciones previas
2. Inicia la API en puerto 7001
3. Inicia la Web en puerto 5002
4. Abre el navegador en el login
5. Muestra las credenciales

### Opción 2: Inicio Manual

**Terminal 1 - API:**
```powershell
cd src\AgoraHub360.ERP.Api
dotnet run --launch-profile https
```

**Terminal 2 - Web:**
```powershell
cd src\AgoraHub360.ERP.Web
dotnet run --launch-profile https
```

**Navegador:**
```
https://localhost:5002/login
```

---

## ?? Verificación

### Test 1: API Respondiendo
```powershell
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
Invoke-RestMethod -Uri "https://localhost:7001/api/v1/diagnostics/ping"
```

**Resultado esperado:**
```json
{
  "status": "ok",
  "timestamp": "2026-02-17T...",
  "version": "1.0"
}
```

### Test 2: Login desde PowerShell
```powershell
$login = @{
    email = "admin@agorahub360.com"
    password = "Admin123"
} | ConvertTo-Json

[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
$response = Invoke-RestMethod -Uri "https://localhost:7001/api/v1/auth/login" `
    -Method Post -Body $login -ContentType "application/json"

Write-Host "Token recibido: $($response.data.token.Substring(0, 50))..."
```

### Test 3: Desde Swagger
1. Abrir: `https://localhost:7001/swagger`
2. POST `/api/v1/auth/login`
3. Body:
   ```json
   {
     "email": "admin@agorahub360.com",
     "password": "Admin123"
   }
   ```
4. Verificar que recibas un token JWT

---

## ?? Puertos Configurados

| Componente | HTTPS | HTTP | Descripción |
|------------|-------|------|-------------|
| **API** | 7001 | 5000 | Backend REST API |
| **Web** | 5002 | 5001 | Blazor WebAssembly |

---

## ??? Archivos Modificados

1. ? `src/AgoraHub360.ERP.Api/Properties/launchSettings.json`
2. ? `src/AgoraHub360.ERP.Web/Properties/launchSettings.json`
3. ? `src/AgoraHub360.ERP.Web/wwwroot/appsettings.json` (creado)
4. ? `start-system.ps1` (creado)

---

## ?? Migración desde Configuración Anterior

Si ya tenías el sistema corriendo con puertos diferentes:

1. **Detener aplicaciones:**
   ```powershell
   Get-Process | Where-Object {$_.ProcessName -like "*AgoraHub*"} | Stop-Process -Force
   ```

2. **Limpiar compilación:**
   ```powershell
   dotnet clean
   dotnet build --no-incremental
   ```

3. **Reiniciar con nuevos puertos:**
   ```powershell
   .\start-system.ps1
   ```

---

## ?? Solución de Problemas

### Error: "No se puede conectar con el servidor remoto"

**Posibles causas:**
1. La API no está corriendo
2. Firewall bloqueando el puerto
3. Certificado SSL no confiable

**Soluciones:**
```powershell
# Verificar que la API esté corriendo
Get-Process | Where-Object {$_.ProcessName -like "*AgoraHub*"}

# Verificar puerto 7001
Test-NetConnection -ComputerName localhost -Port 7001

# Reiniciar con el script
.\start-system.ps1
```

### Error: "CORS policy"

**Solución:**
Verifica que `appsettings.json` de la API tenga:
```json
{
  "BlazorBaseUrl": "https://localhost:5002"
}
```

### Error: "Failed to fetch"

**Solución:**
1. Verifica que `wwwroot/appsettings.json` de la Web exista
2. Debe contener: `"ApiBaseUrl": "https://localhost:7001/"`
3. Recompila la aplicación Web

---

## ? Checklist de Verificación

- [ ] API corriendo en puerto 7001
- [ ] Web corriendo en puerto 5002
- [ ] Swagger accesible en https://localhost:7001/swagger
- [ ] Login accesible en https://localhost:5002/login
- [ ] Ping API responde OK
- [ ] Login devuelve token JWT
- [ ] Credenciales admin funcionan

---

## ?? Documentación Relacionada

- **CREDENCIALES-DEFAULT.md** - Credenciales de acceso
- **SETUP-COMPLETE.md** - Guía completa de configuración
- **start-system.ps1** - Script de inicio automatizado

---

## ?? Estado Final

? **API:** Configurada en puertos correctos (7001/5000)  
? **Web:** Configurada en puertos correctos (5002/5001)  
? **Comunicación:** API URL configurada correctamente  
? **Script:** Inicio automatizado disponible  
? **Compilación:** Exitosa sin errores  

---

**? Problema Resuelto - Sistema Operativo**

_Fecha: 17/02/2026_
