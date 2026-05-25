# ✅ CONFIGURACIÓN VERIFICADA Y CORREGIDA

## 📋 PUERTOS Y DIRECCIONES CONFIGURADAS

### API (Backend)
```
Puerto Principal: https://localhost:7001
Puerto Alternativo: http://localhost:5000
Swagger: https://localhost:7001/swagger
Health Check: https://localhost:7001/health
```

**Archivos configurados:**
- `src\AgoraHub360.ERP.Api\Properties\launchSettings.json` → Profile "https"
- `src\AgoraHub360.ERP.Api\appsettings.json` → BlazorBaseUrl
- `src\AgoraHub360.ERP.Api\appsettings.Development.json` → URLs de API

### Blazor WebAssembly (Frontend)
```
Puerto Principal: https://localhost:5002
Puerto Alternativo: http://localhost:5001
API Target: https://localhost:7001/
```

**Archivos configurados:**
- `src\AgoraHub360.ERP.Web\Properties\launchSettings.json` → Profile "https"
- `src\AgoraHub360.ERP.Web\wwwroot\appsettings.json` → ApiBaseUrl
- `src\AgoraHub360.ERP.Web\wwwroot\appsettings.Development.json` → ApiBaseUrl

### Base de Datos
```
Servidor: 192.168.88.14
Puerto: 56885
Base de Datos: db_AgoraERP_Core
Usuario: usagora
```

---

## 🔧 CAMBIOS REALIZADOS

### 1. **API - appsettings.json**
✅ Agregado sección `ApiUrls` para documentar puertos
✅ Confirmado `BlazorBaseUrl: https://localhost:5002`

### 2. **API - appsettings.Development.json**
✅ Agregadas URLs de referencia
✅ Aumentado logging a nivel Debug
✅ Confirmada configuración de CORS

### 3. **Scripts de inicio (.bat)**
✅ Agregado flag `--launch-profile https` para forzar el perfil correcto
✅ Agregada información de diagnóstico
✅ Agregada compilación con `--configuration Debug`

### 4. **Blazor - appsettings.json y appsettings.Development.json**
✅ Confirmado `ApiBaseUrl: https://localhost:7001/`

---

## 🚀 CÓMO INICIAR (MÉTODO CORRECTO)

### Opción A: Usando los scripts .bat (RECOMENDADO)

#### Terminal 1 - API:
```cmd
1-iniciar-api.bat
```
**Espera a ver:** `Now listening on: https://localhost:7001`

#### Terminal 2 - Blazor:
```cmd
2-iniciar-blazor.bat
```
**Espera a ver:** `Now listening on: https://localhost:5002`

#### Navegador:
```
https://localhost:5002
```

---

### Opción B: Usando comandos manuales

#### Terminal 1 - API:
```cmd
cd src\AgoraHub360.ERP.Api
dotnet run --launch-profile https
```

#### Terminal 2 - Blazor:
```cmd
cd src\AgoraHub360.ERP.Web
dotnet run --launch-profile https
```

---

### Opción C: Desde Visual Studio

1. **Configurar múltiples proyectos de inicio:**
   - Right-click en la solución → **Set Startup Projects**
   - Selecciona **Multiple startup projects**
   - Establece:
     - `AgoraHub360.ERP.Api` → **Start**
     - `AgoraHub360.ERP.Web` → **Start**

2. **Seleccionar el perfil correcto:**
   - Para **API**: Selecciona perfil **"https"**
   - Para **Blazor**: Selecciona perfil **"https"**

3. **Presiona F5** para iniciar ambos proyectos

---

## ✅ VERIFICACIÓN DE CONFIGURACIÓN

### Verificar que los puertos estén libres:
```cmd
verificar-puertos.bat
```

### Verificar SQL Server:
```cmd
0-test-sqlserver.bat
```

### Verificar que la API responda:
```powershell
# PowerShell
Invoke-WebRequest -Uri https://localhost:7001/health -SkipCertificateCheck
```

```cmd
# CMD
curl -k https://localhost:7001/health
```

### Verificar en el navegador:
1. Abre `https://localhost:7001/swagger` → Debe mostrar Swagger UI
2. Abre `https://localhost:5002` → Debe redirigir a `/login`
3. En DevTools (F12) → Console, busca: `🔧 API Base URL configurada: https://localhost:7001/`

---

## 🎯 DIAGRAMA DE FLUJO

```
┌─────────────────────────────────────────────────────────┐
│  NAVEGADOR                                              │
│  https://localhost:5002                                 │
└───────────────────┬─────────────────────────────────────┘
                    │
                    │ [Blazor WebAssembly]
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  API                                                    │
│  https://localhost:7001                                 │
│  - CORS permite: https://localhost:5002                │
│  - Endpoints: /api/v1/*                                │
└───────────────────┬─────────────────────────────────────┘
                    │
                    │ [Entity Framework Core]
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  SQL SERVER                                             │
│  192.168.88.14:56885                                    │
│  db_AgoraERP_Core                                       │
└─────────────────────────────────────────────────────────┘
```

---

## 🔍 SOLUCIÓN DE PROBLEMAS

### Error: "The configured user limit (128) on the number of inotify instances has been exceeded"
```bash
# Linux/WSL
echo fs.inotify.max_user_instances=524288 | sudo tee -a /etc/sysctl.conf
sudo sysctl -p
```

### Error: "Address already in use"
```powershell
# Ver qué proceso usa el puerto
Get-NetTCPConnection -LocalPort 7001 | Select-Object OwningProcess
Get-Process -Id <PID>

# Matar el proceso
Stop-Process -Id <PID> -Force
```

### Error: "CORS policy"
✅ **YA CORREGIDO** en `src\AgoraHub360.ERP.Api\appsettings.json`:
```json
"BlazorBaseUrl": "https://localhost:5002"
```

### Error: "Failed to fetch"
- ✅ Verifica que la API esté corriendo: `https://localhost:7001/health`
- ✅ Verifica el certificado SSL (aceptar en navegador)
- ✅ Verifica en DevTools → Network la petición

### Error de conexión a SQL Server
```cmd
0-test-sqlserver.bat
```
- Si falla, revisa firewall, SQL Server Configuration Manager (TCP/IP habilitado)

---

## 📦 ARCHIVOS DE CONFIGURACIÓN ACTUALIZADOS

| Archivo | Estado | Cambios |
|---------|--------|---------|
| `src\AgoraHub360.ERP.Api\appsettings.json` | ✅ ACTUALIZADO | Agregado `ApiUrls` |
| `src\AgoraHub360.ERP.Api\appsettings.Development.json` | ✅ ACTUALIZADO | URLs y logging |
| `src\AgoraHub360.ERP.Web\wwwroot\appsettings.json` | ✅ OK | ApiBaseUrl correcto |
| `src\AgoraHub360.ERP.Web\wwwroot\appsettings.Development.json` | ✅ OK | ApiBaseUrl correcto |
| `1-iniciar-api.bat` | ✅ MEJORADO | Fuerza perfil https |
| `2-iniciar-blazor.bat` | ✅ MEJORADO | Fuerza perfil https |
| `verificar-puertos.bat` | ✅ NUEVO | Diagnóstico de puertos |

---

## 🎬 RESUMEN: CONFIGURACIÓN FINAL

```json
API:     https://localhost:7001  ← Escucha aquí
         CORS permite → https://localhost:5002

Blazor:  https://localhost:5002  ← Usuario accede aquí
         Apunta a API → https://localhost:7001/

SQL:     192.168.88.14:56885
         db_AgoraERP_Core
```

**TODO ESTÁ CONFIGURADO CORRECTAMENTE. AHORA EJECUTA:**
```cmd
1-iniciar-api.bat       (Terminal 1)
2-iniciar-blazor.bat    (Terminal 2)
```

Luego abre: **https://localhost:5002**
