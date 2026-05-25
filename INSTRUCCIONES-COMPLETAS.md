# 🔧 SOLUCION: "No hay conexión con el servidor" - AgoraHub360 ERP

## 📋 RESUMEN DEL PROBLEMA

El sistema no inicia el login ni muestra la web debido a problemas de conexión entre:
1. **Blazor WebAssembly** (Frontend) → **API** (Backend)
2. **API** → **SQL Server** (Base de Datos)

---

## ✅ CONFIGURACIÓN ACTUAL

| Componente | URL / Servidor | Puerto |
|------------|----------------|--------|
| **API (Backend)** | `https://localhost:7001/` | 7001 |
| **Blazor WASM (Frontend)** | `https://localhost:5002/` | 5002 |
| **SQL Server** | `192.168.88.14` | 56885 |
| **Base de Datos** | `db_AgoraERP_Core` | - |
| **Usuario BD** | `usagora` | - |

---

## 🚀 PASOS PARA SOLUCIONAR

### PASO 1: Verificar SQL Server

Ejecuta el archivo: **`0-test-sqlserver.bat`**

- ✅ **Si TcpTestSucceeded: True** → SQL Server está accesible
- ❌ **Si TcpTestSucceeded: False** → SQL Server NO está accesible

#### Si falla la conexión a SQL Server:

1. Verifica que SQL Server esté corriendo en `192.168.88.14`
2. Verifica que el firewall permita el puerto `56885`
3. Verifica que SQL Server acepte conexiones remotas:
   ```sql
   -- Ejecutar en SQL Server Management Studio
   EXEC sp_configure 'remote access', 1;
   RECONFIGURE;
   ```
4. Verifica que TCP/IP esté habilitado en SQL Server Configuration Manager

---

### PASO 2: Iniciar la API (Backend)

Abre una **primera terminal** y ejecuta: **`1-iniciar-api.bat`**

O manualmente:
```cmd
cd src\AgoraHub360.ERP.Api
dotnet run
```

**Debes ver:**
```
Now listening on: https://localhost:7001
```

#### Si falla al iniciar la API:

- **Error de compilación**: Revisa los errores en rojo
- **Error de conexión a BD**: Vuelve al PASO 1
- **Puerto ocupado**: Cierra otras instancias de la API

---

### PASO 3: Iniciar Blazor WebAssembly (Frontend)

Abre una **segunda terminal** (SIN CERRAR LA PRIMERA) y ejecuta: **`2-iniciar-blazor.bat`**

O manualmente:
```cmd
cd src\AgoraHub360.ERP.Web
dotnet run
```

**Debes ver:**
```
Now listening on: https://localhost:5002
```

---

### PASO 4: Abrir en el navegador

1. Abre tu navegador
2. Ve a: `https://localhost:5002`
3. Acepta el certificado de desarrollo si te lo pide
4. Debes ver la **página de Login**

---

## 🔍 DIAGNÓSTICO DE ERRORES COMUNES

### Error: "CORS policy"
**Causa**: La API no permite peticiones desde `https://localhost:5002`

**Solución**:
```json
// src\AgoraHub360.ERP.Api\appsettings.json
{
  "BlazorBaseUrl": "https://localhost:5002"  // Verificar esta línea
}
```

### Error: "Failed to fetch" o "ERR_CONNECTION_REFUSED"
**Causa**: La API no está corriendo

**Solución**: Asegúrate de que la API esté corriendo (PASO 2)

### Error: "A network-related or instance-specific error"
**Causa**: No puede conectar a SQL Server

**Solución**: Vuelve al PASO 1

### Error: "Login failed for user 'usagora'"
**Causa**: Credenciales incorrectas o permisos faltantes

**Solución**:
1. Verifica usuario y contraseña en `src\AgoraHub360.ERP.Api\appsettings.json`
2. Verifica que el usuario tenga permisos en SQL Server

---

## 🛠️ ARCHIVOS CREADOS PARA AYUDARTE

| Archivo | Descripción |
|---------|-------------|
| `0-test-sqlserver.bat` | Verifica conexión a SQL Server |
| `1-iniciar-api.bat` | Inicia la API (Backend) |
| `2-iniciar-blazor.bat` | Inicia Blazor WASM (Frontend) |
| `GUIA-DIAGNOSTICO.txt` | Guía rápida de comandos PowerShell |
| `diagnostico-conexiones.ps1` | Script completo de diagnóstico |

---

## 📞 VERIFICACIÓN MANUAL

### Verificar que la API responde:
```powershell
Invoke-WebRequest -Uri https://localhost:7001/health -SkipCertificateCheck
```

### Verificar que Blazor está configurado correctamente:
```json
// src\AgoraHub360.ERP.Web\wwwroot\appsettings.json
{
  "ApiBaseUrl": "https://localhost:7001/"
}
```

### Verificar en el navegador (F12 → Console):
Debes ver:
```
🔧 API Base URL configurada: https://localhost:7001/
```

---

## ✨ CAMBIOS REALIZADOS

He modificado los siguientes archivos para mejorar el diagnóstico:

1. **`src\AgoraHub360.ERP.Web\Program.cs`**
   - Agregado logging de la URL de la API
   - Agregado fallback a `https://localhost:7001/`

2. **`src\AgoraHub360.ERP.Web\wwwroot\appsettings.json`**
   - Verificada configuración correcta

3. **`src\AgoraHub360.ERP.Web\wwwroot\appsettings.Development.json`** (NUEVO)
   - Creado archivo de configuración para desarrollo

---

## 🎯 RESUMEN RÁPIDO

```cmd
REM Terminal 1
1-iniciar-api.bat

REM Terminal 2 (abrir nueva terminal)
2-iniciar-blazor.bat

REM Navegador
https://localhost:5002
```

---

¿Necesitas más ayuda? Ejecuta `0-test-sqlserver.bat` y envía el resultado.
