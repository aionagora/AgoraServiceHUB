# AgoraHub360 ERP — Guía de Despliegue en Servidor
# AgoraHub360 ERP � Gu�a de Despliegue en Servidor

> **Versi�n:** 1.0.0  
> **Fecha:** Febrero 2026  
> **Gesti�n:** 2026  
> **Plataforma:** .NET 8.0  
> **Repositorio:** `https://github.com/abelcalvimontes/AgoraHUB360-ERP`  
> **Rama principal:** `main`

---

## ?? �ndice

1. [Visi�n General de Arquitectura](#1-visi�n-general-de-arquitectura)
2. [Requisitos del Servidor](#2-requisitos-del-servidor)
3. [Estructura del Proyecto](#3-estructura-del-proyecto)
4. [Preparaci�n de la Base de Datos](#4-preparaci�n-de-la-base-de-datos)
5. [Configuraci�n de Archivos](#5-configuraci�n-de-archivos)
6. [Publicaci�n y Despliegue � API](#6-publicaci�n-y-despliegue--api)
7. [Publicaci�n y Despliegue � Web (Blazor WASM)](#7-publicaci�n-y-despliegue--web-blazor-wasm)
8. [Configuraci�n de IIS](#8-configuraci�n-de-iis)
9. [Configuraci�n de Nginx (Linux)](#9-configuraci�n-de-nginx-linux)
10. [Variables de Entorno y Seguridad](#10-variables-de-entorno-y-seguridad)
11. [Health Checks y Monitoreo](#11-health-checks-y-monitoreo)
12. [Troubleshooting](#12-troubleshooting)
13. [Checklist de Despliegue](#13-checklist-de-despliegue)

---

## 1. Visi�n General de Arquitectura

```
???????????????????????????????????????????????????????????????????
?                        CLIENTE (Browser)                        ?
?                    Blazor WebAssembly (WASM)                    ?
?              AgoraHub360.ERP.Web  ? archivos est�ticos          ?
???????????????????????????????????????????????????????????????????
                         ? HTTPS (JSON API)
                         ?
???????????????????????????????????????????????????????????????????
?                      SERVIDOR DE API                            ?
?                   AgoraHub360.ERP.Api                            ?
?  ???????????????  ????????????????  ??????????????????????     ?
?  ? Controllers  ???  Application  ???   Persistence (EF)  ?     ?
?  ?  (API v1)    ?  ?  (Services)   ?  ?   (SQL Server)     ?     ?
?  ???????????????  ????????????????  ??????????????????????     ?
?  ???????????????????????????????????????????????????????????   ?
?  ?  Middleware: JWT Auth ? Tenant Validation ? CORS         ?   ?
?  ???????????????????????????????????????????????????????????   ?
???????????????????????????????????????????????????????????????????
                         ?
                         ?
              ???????????????????????
              ?   SQL Server        ?
              ?   db_AgoraERP_Core  ?
              ???????????????????????
```

**El sistema consta de DOS componentes desplegables:**

| Componente | Proyecto | Tipo | Puerto Dev |
|------------|----------|------|------------|
| **API Backend** | `AgoraHub360.ERP.Api` | ASP.NET Core Web API | `https://7001` / `http://5000` |
| **Frontend Web** | `AgoraHub360.ERP.Web` | Blazor WebAssembly (est�tico) | `https://5002` / `http://5001` |

> ?? **Importante:** Blazor WASM se compila a archivos est�ticos (HTML/JS/WASM). No necesita un runtime .NET en el servidor web del frontend. Se puede servir desde cualquier servidor HTTP (IIS, Nginx, Apache, CDN).

---

## 2. Requisitos del Servidor

### 2.1 Servidor de API (Backend)

| Requisito | M�nimo | Recomendado |
|-----------|--------|-------------|
| **OS** | Windows Server 2019+ / Ubuntu 20.04+ | Windows Server 2022 / Ubuntu 22.04 |
| **Runtime** | .NET 8.0 Runtime + ASP.NET Core Runtime | .NET 8.0 SDK (para migraciones) |
| **RAM** | 2 GB | 4 GB+ |
| **CPU** | 2 cores | 4 cores |
| **Disco** | 5 GB | 20 GB (logs) |

### 2.2 Servidor de Base de Datos

| Requisito | M�nimo | Recomendado |
|-----------|--------|-------------|
| **Motor** | SQL Server 2019+ | SQL Server 2022 |
| **Edici�n** | Express | Standard / Developer |
| **RAM** | 4 GB | 8 GB+ |
| **Disco** | 10 GB | 50 GB+ |

### 2.3 Servidor Web Frontend (opcional separado)

| Requisito | Detalle |
|-----------|---------|
| **Servidor HTTP** | IIS 10+, Nginx, Apache, Azure Static Web Apps, o cualquier CDN |
| **No requiere** | Runtime de .NET (es 100% est�tico) |

### 2.4 Software a Instalar

**Windows:**
```powershell
# 1. Instalar .NET 8.0 Runtime + ASP.NET Core
winget install Microsoft.DotNet.AspNetCore.8

# 2. Verificar instalaci�n
dotnet --list-runtimes

# 3. (Opcional) Instalar Hosting Bundle para IIS
# Descargar: https://dotnet.microsoft.com/download/dotnet/8.0
# Archivo: dotnet-hosting-8.0.x-win.exe
```

**Linux (Ubuntu/Debian):**
```bash
# 1. Agregar repositorio de Microsoft
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# 2. Instalar runtimes
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-8.0

# 3. Verificar
dotnet --list-runtimes
```

---

## 3. Estructura del Proyecto

```
AgoraHUB360-ERP/
??? AgoraHub360.ERP.sln                    # Soluci�n
??? src/
?   ??? AgoraHub360.ERP.Api/               # ?? API Backend (se publica)
?   ?   ??? Controllers/V1/                # Endpoints REST
?   ?   ??? Middleware/                     # Auth, CORS, Tenant
?   ?   ??? appsettings.json               # ?? Config principal
?   ?   ??? Program.cs                     # Entry point
?   ?
?   ??? AgoraHub360.ERP.Web/               # ?? Frontend Blazor WASM (se publica)
?   ?   ??? Pages/                         # Componentes Razor
?   ?   ??? Services/                      # HTTP Services
?   ?   ??? wwwroot/                       # Archivos est�ticos
?   ?   ?   ??? appsettings.json           # ?? URL de la API
?   ?   ??? Program.cs                     # Entry point WASM
?   ?
?   ??? AgoraHub360.ERP.Application/       # Capa de l�gica de negocio
?   ??? AgoraHub360.ERP.Domain/            # Entidades y enums
?   ??? AgoraHub360.ERP.Persistence/       # EF Core + Migraciones
?   ??? AgoraHub360.ERP.Infrastructure/    # Servicios externos
?   ??? AgoraHub360.ERP.Shared/            # DTOs compartidos
?
??? tests/
    ??? AgoraHub360.ERP.Tests/             # Tests unitarios
```

---

## 4. Preparaci�n de la Base de Datos

### 4.1 Crear la Base de Datos

```sql
-- Ejecutar en SQL Server Management Studio (SSMS)
CREATE DATABASE db_AgoraERP_Core;
GO

-- Crear usuario de aplicaci�n
CREATE LOGIN usagora WITH PASSWORD = 'P@ssw0rd_Pr0d_S3cur3!';
GO

USE db_AgoraERP_Core;
CREATE USER usagora FOR LOGIN usagora;
ALTER ROLE db_owner ADD MEMBER usagora;
GO
```

### 4.2 Aplicar Migraciones

**Opci�n A � Desde m�quina de desarrollo (recomendado para primer despliegue):**
```powershell
cd D:\AgoraCORE\AgoraHUB360-ERP

# Actualizar connection string en appsettings.json apuntando al servidor de producci�n
# Luego ejecutar:
dotnet ef database update `
  --project src\AgoraHub360.ERP.Persistence `
  --startup-project src\AgoraHub360.ERP.Api
```

**Opci�n B � Generar script SQL para DBA:**
```powershell
dotnet ef migrations script `
  --project src\AgoraHub360.ERP.Persistence `
  --startup-project src\AgoraHub360.ERP.Api `
  --idempotent `
  --output deploy\migration.sql
```
> El flag `--idempotent` genera un script seguro que verifica qu� migraciones ya se aplicaron.

### 4.3 Migraciones Actuales (15 en total)

| # | Migraci�n | Descripci�n |
|---|-----------|-------------|
| 1 | `AddParametroSistemaNumeracionDocumento` | Par�metros del sistema y numeraci�n |
| 2 | `AddDefaultAdminUser` | Usuario admin por defecto |
| 3 | `AddMDMEntities` | Entidades maestras (productos, categor�as) |
| 4 | `MDM_Enhanced` | Mejoras MDM |
| 5 | `MDM_Refinements` | Refinamientos MDM |
| 6 | `SeedProductStatus` | Estados de producto |
| 7 | `SeedDefaultCatalog` | Cat�logo por defecto |
| 8 | `CMP_OrdenesCompra` | �rdenes de compra |
| 9 | `CMP_RecepcionesCompra` | Recepciones de compra |
| 10 | `CMP_Importaciones` | Hojas de importaci�n |
| 11 | `ACC_CuentasContables` | Plan de cuentas contable |
| 12 | `ACC_AsientosContables` | Asientos/comprobantes contables |
| 13 | `ACC_PeriodosContables` | Per�odos contables |
| 14 | `ACC_PlantillasContables` | Plantillas de contabilizaci�n |
| 15 | `ACC_ComprobantesContables` | Tipos de comprobante, cambio y pago |

### 4.4 Schemas de BD

| Schema | M�dulo | Tablas principales |
|--------|--------|--------------------|
| `core` | Core | Empresas, Usuarios, Roles, Par�metros, Numeraci�n |
| `mdm` | Master Data | Productos, Categor�as, Proveedores, Clientes, Almacenes |
| `inv` | Inventario | Movimientos, Stock |
| `cmp` | Compras | OC, Recepciones, Importaciones |
| `acc` | Contabilidad | Cuentas, Asientos, Per�odos, Plantillas, Tipos |

---

## 5. Configuraci�n de Archivos

### 5.1 API � `appsettings.Production.json`

Crear este archivo en `src/AgoraHub360.ERP.Api/`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SERVIDOR_SQL;Database=db_AgoraERP_Core;User Id=usagora;Password=P@ssw0rd_Pr0d_S3cur3!;TrustServerCertificate=true;MultipleActiveResultSets=true;Encrypt=true"
  },
  "Jwt": {
    "Key": "CLAVE-JWT-PRODUCCION-MINIMO-32-CARACTERES-SEGURA!!",
    "Issuer": "AgoraHub360.ERP",
    "Audience": "AgoraHub360.ERP.Web",
    "ExpirationHours": 8
  },
  "BlazorBaseUrl": "https://erp.midominio.com",
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> ?? **IMPORTANTE:** Este archivo NO debe subirse al repositorio. Agregarlo a `.gitignore`.

### 5.2 Frontend � `wwwroot/appsettings.json`

Actualizar `src/AgoraHub360.ERP.Web/wwwroot/appsettings.json`:

```json
{
  "ApiBaseUrl": "https://api-erp.midominio.com/"
}
```

> ?? Este archivo S� se incluye en el publish del frontend ya que es un archivo est�tico p�blico (no contiene secretos).

### 5.3 Tabla de Variables de Configuraci�n

| Variable | Archivo | Ejemplo Producci�n | Descripci�n |
|----------|---------|-------------------|-------------|
| `ConnectionStrings:DefaultConnection` | API `appsettings.Production.json` | `Server=10.0.0.5;Database=db_AgoraERP_Core;...` | Cadena de conexi�n SQL Server |
| `Jwt:Key` | API `appsettings.Production.json` | `Kz9$mP2x...` (?32 chars) | Clave secreta para firmar tokens JWT |
| `Jwt:Issuer` | API `appsettings.Production.json` | `AgoraHub360.ERP` | Emisor del token |
| `Jwt:Audience` | API `appsettings.Production.json` | `AgoraHub360.ERP.Web` | Audiencia del token |
| `Jwt:ExpirationHours` | API `appsettings.Production.json` | `8` | Duraci�n del token en horas |
| `BlazorBaseUrl` | API `appsettings.Production.json` | `https://erp.midominio.com` | URL del frontend (para CORS) |
| `ApiBaseUrl` | Web `wwwroot/appsettings.json` | `https://api-erp.midominio.com/` | URL de la API (con `/` final) |

---

## 6. Publicaci�n y Despliegue � API

### 6.1 Publicar desde L�nea de Comandos

```powershell
cd D:\AgoraCORE\AgoraHUB360-ERP

# Publicar en modo Release
dotnet publish src\AgoraHub360.ERP.Api `
  -c Release `
  -o publish\api `
  --self-contained false

# Resultado en: publish\api\
```

**Contenido generado:**
```
publish/api/
??? AgoraHub360.ERP.Api.dll          # Entry point
??? AgoraHub360.ERP.Api.exe          # (Windows) Ejecutable directo
??? AgoraHub360.ERP.Application.dll
??? AgoraHub360.ERP.Domain.dll
??? AgoraHub360.ERP.Persistence.dll
??? AgoraHub360.ERP.Shared.dll
??? AgoraHub360.ERP.Infrastructure.dll
??? appsettings.json
??? appsettings.Production.json      # ?? Copiar manualmente o crear en servidor
??? web.config                       # Para IIS
??? ...dlls de dependencias
```

### 6.2 Copiar al Servidor

```powershell
# Windows � copiar carpeta
xcopy /E /Y publish\api \\SERVIDOR\c$\inetpub\AgoraHub360-API\

# Linux � via SCP
scp -r publish/api/* usuario@servidor:/opt/agorahub360/api/
```

### 6.3 Crear `appsettings.Production.json` en el servidor

> **NUNCA copiar credenciales desde el repositorio.** Crear el archivo directamente en el servidor.

```powershell
# En el servidor:
# Windows
notepad C:\inetpub\AgoraHub360-API\appsettings.Production.json

# Linux
nano /opt/agorahub360/api/appsettings.Production.json
```

### 6.4 Probar Ejecuci�n Directa

```powershell
# Windows
cd C:\inetpub\AgoraHub360-API
set ASPNETCORE_ENVIRONMENT=Production
dotnet AgoraHub360.ERP.Api.dll --urls "http://0.0.0.0:5000"

# Linux
cd /opt/agorahub360/api
export ASPNETCORE_ENVIRONMENT=Production
dotnet AgoraHub360.ERP.Api.dll --urls "http://0.0.0.0:5000"
```

Verificar: `http://SERVIDOR:5000/health` ? debe responder `Healthy`

---

## 7. Publicaci�n y Despliegue � Web (Blazor WASM)

### 7.1 Actualizar URL de la API

**Antes de publicar**, editar `src/AgoraHub360.ERP.Web/wwwroot/appsettings.json`:

```json
{
  "ApiBaseUrl": "https://api-erp.midominio.com/"
}
```

### 7.2 Publicar

```powershell
dotnet publish src\AgoraHub360.ERP.Web `
  -c Release `
  -o publish\web

# Los archivos est�ticos quedan en:
# publish\web\wwwroot\
```

**Contenido generado:**
```
publish/web/wwwroot/
??? index.html                    # P�gina principal
??? css/                          # Estilos
??? appsettings.json              # Config (URL de API)
??? _framework/
?   ??? blazor.webassembly.js     # Bootstrapper
?   ??? dotnet.wasm               # Runtime .NET en WASM
?   ??? AgoraHub360.ERP.Web.wasm  # App compilada
?   ??? ...otros .wasm/.dll
??? _content/                     # Recursos est�ticos
```

### 7.3 Copiar al Servidor Web

```powershell
# Windows (IIS)
xcopy /E /Y publish\web\wwwroot C:\inetpub\AgoraHub360-Web\

# Linux (Nginx)
scp -r publish/web/wwwroot/* usuario@servidor:/var/www/agorahub360/
```

---

## 8. Configuraci�n de IIS

### 8.1 Prerrequisitos IIS

1. Instalar **IIS** con los m�dulos:
   - Contenido est�tico
   - Compresi�n din�mica
   - M�dulo de reescritura de URL (URL Rewrite)
2. Instalar **.NET 8.0 Hosting Bundle** ([descargar aqu�](https://dotnet.microsoft.com/download/dotnet/8.0))
3. Reiniciar IIS: `iisreset`

### 8.2 Sitio IIS para la API

1. **Crear Application Pool:**
   - Nombre: `AgoraHub360-API`
   - Versi�n de CLR: `No Managed Code`
   - Modo de pipeline: `Integrated`

2. **Crear sitio web:**
   - Nombre: `AgoraHub360-API`
   - Ruta f�sica: `C:\inetpub\AgoraHub360-API`
   - Binding: `https://api-erp.midominio.com:443` (con certificado SSL)
   - Application Pool: `AgoraHub360-API`

3. **Verificar `web.config`** (se genera autom�ticamente al publicar):
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" 
             modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\AgoraHub360.ERP.Api.dll"
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="InProcess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

4. **Crear carpeta de logs:**
```powershell
mkdir C:\inetpub\AgoraHub360-API\logs
```

### 8.3 Sitio IIS para el Frontend (Blazor WASM)

1. **Crear Application Pool:**
   - Nombre: `AgoraHub360-Web`
   - Versi�n de CLR: `No Managed Code`

2. **Crear sitio web:**
   - Nombre: `AgoraHub360-Web`
   - Ruta f�sica: `C:\inetpub\AgoraHub360-Web`
   - Binding: `https://erp.midominio.com:443` (con certificado SSL)

3. **Crear `web.config`** en la ra�z del sitio (OBLIGATORIO para SPA routing):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <staticContent>
      <remove fileExtension=".blat" />
      <remove fileExtension=".dat" />
      <remove fileExtension=".dll" />
      <remove fileExtension=".json" />
      <remove fileExtension=".wasm" />
      <remove fileExtension=".woff" />
      <remove fileExtension=".woff2" />
      <mimeMap fileExtension=".blat" mimeType="application/octet-stream" />
      <mimeMap fileExtension=".dat" mimeType="application/octet-stream" />
      <mimeMap fileExtension=".dll" mimeType="application/octet-stream" />
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <mimeMap fileExtension=".wasm" mimeType="application/wasm" />
      <mimeMap fileExtension=".woff" mimeType="font/woff" />
      <mimeMap fileExtension=".woff2" mimeType="font/woff2" />
    </staticContent>
    <httpCompression>
      <dynamicTypes>
        <add mimeType="application/wasm" enabled="true" />
      </dynamicTypes>
    </httpCompression>
    <rewrite>
      <rules>
        <rule name="SPA Fallback" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/index.html" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

> ?? **Cr�tico:** Sin la regla de reescritura SPA, las rutas tipo `/contabilidad/asientos` dar�n error 404 al refrescar el navegador.

---

## 9. Configuraci�n de Nginx (Linux)

### 9.1 Servicio systemd para la API

Crear `/etc/systemd/system/agorahub360-api.service`:

```ini
[Unit]
Description=AgoraHub360 ERP API
After=network.target

[Service]
WorkingDirectory=/opt/agorahub360/api
ExecStart=/usr/bin/dotnet /opt/agorahub360/api/AgoraHub360.ERP.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=agorahub360-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl daemon-reload
sudo systemctl enable agorahub360-api
sudo systemctl start agorahub360-api
sudo systemctl status agorahub360-api
```

### 9.2 Nginx � Configuraci�n del Sitio

Crear `/etc/nginx/sites-available/agorahub360`:

```nginx
# ???????????????????????????????????????????
# API Backend � Reverse Proxy
# ???????????????????????????????????????????
server {
    listen 443 ssl http2;
    server_name api-erp.midominio.com;

    ssl_certificate     /etc/ssl/certs/agorahub360.crt;
    ssl_certificate_key /etc/ssl/private/agorahub360.key;

    location / {
        proxy_pass         http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_set_header   X-Real-IP $remote_addr;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}

# ???????????????????????????????????????????
# Frontend Blazor WASM � Archivos est�ticos
# ???????????????????????????????????????????
server {
    listen 443 ssl http2;
    server_name erp.midominio.com;

    ssl_certificate     /etc/ssl/certs/agorahub360.crt;
    ssl_certificate_key /etc/ssl/private/agorahub360.key;

    root /var/www/agorahub360;
    index index.html;

    # Compresi�n para WASM
    gzip on;
    gzip_types application/wasm application/json application/javascript text/css;

    # MIME types para Blazor WASM
    types {
        application/wasm wasm;
    }

    # SPA Fallback � CR�TICO para Blazor routing
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Cache agresivo para framework (inmutable)
    location /_framework/ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}

# Redireccionar HTTP ? HTTPS
server {
    listen 80;
    server_name erp.midominio.com api-erp.midominio.com;
    return 301 https://$host$request_uri;
}
```

```bash
sudo ln -s /etc/nginx/sites-available/agorahub360 /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## 10. Variables de Entorno y Seguridad

### 10.1 Variables de Entorno (alternativa a appsettings.Production.json)

En lugar de archivo, se pueden usar variables de entorno:

```bash
# Linux systemd � agregar al .service
Environment=ConnectionStrings__DefaultConnection=Server=...
Environment=Jwt__Key=CLAVE-SEGURA-PRODUCCION...
Environment=BlazorBaseUrl=https://erp.midominio.com
```

```powershell
# Windows � variables de sistema
[System.Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=...", "Machine")
[System.Environment]::SetEnvironmentVariable("Jwt__Key", "CLAVE-SEGURA...", "Machine")
```

> Nota: En .NET, los `:` de la jerarqu�a JSON se reemplazan por `__` (doble gui�n bajo).

### 10.2 Seguridad � Lista de Verificaci�n

| Item | Acci�n |
|------|--------|
| ? JWT Key | Usar clave de al menos 32 caracteres, alfanum�rica con s�mbolos |
| ? SQL Password | Contrase�a fuerte, diferente de desarrollo |
| ? HTTPS | Certificado SSL v�lido en ambos sitios |
| ? CORS | `BlazorBaseUrl` apunta SOLO al dominio del frontend |
| ? Swagger | Deshabilitado en producci�n (solo activo en `Development`) |
| ? Logs | No exponer logs a internet. Verificar permisos de carpeta |
| ? Firewall | Puerto SQL (1433) solo accesible desde servidor API |
| ? appsettings | `appsettings.Production.json` en `.gitignore` |

---

## 11. Health Checks y Monitoreo

### 11.1 Endpoint de Health Check

```
GET https://api-erp.midominio.com/health
```

Respuestas:
- `200 OK` ? `Healthy` � API y BD funcionando
- `503 Service Unavailable` ? `Unhealthy` � Problema de conexi�n a BD

### 11.2 Swagger (solo Desarrollo)

```
GET https://localhost:7001/swagger
```

> Swagger se desactiva autom�ticamente cuando `ASPNETCORE_ENVIRONMENT != Development`.

### 11.3 Monitoreo Recomendado

```powershell
# Verificar health cada 60 segundos (Windows Task Scheduler)
$response = Invoke-WebRequest -Uri "https://api-erp.midominio.com/health" -UseBasicParsing
if ($response.StatusCode -ne 200) {
    Send-MailMessage -To "ops@midominio.com" -Subject "AgoraHub360 API DOWN" ...
}
```

```bash
# Linux cron � cada 5 minutos
*/5 * * * * curl -sf https://api-erp.midominio.com/health || echo "API DOWN" | mail -s "AgoraHub360 Alert" ops@midominio.com
```

---

## 12. Troubleshooting

### Error: `500 Internal Server Error` al iniciar

**Causa:** Falta `appsettings.Production.json` o connection string inv�lido.

```powershell
# Verificar logs de IIS:
type C:\inetpub\AgoraHub360-API\logs\stdout_*.log

# Verificar logs systemd (Linux):
journalctl -u agorahub360-api -n 50 --no-pager
```

### Error: `404 Not Found` al navegar rutas en Blazor

**Causa:** Falta la regla de reescritura SPA en IIS/Nginx.

**Soluci�n:** Verificar que `web.config` (IIS) o `try_files` (Nginx) est� configurado correctamente. Ver secciones 8.3 y 9.2.

### Error: `CORS policy` en el navegador

**Causa:** `BlazorBaseUrl` en `appsettings.Production.json` no coincide con el dominio del frontend.

```json
// ? Incorrecto
"BlazorBaseUrl": "https://localhost:5002"

// ? Correcto
"BlazorBaseUrl": "https://erp.midominio.com"
```

### Error: `401 Unauthorized` � Token JWT inv�lido

**Causa:** La clave JWT de la API (`Jwt:Key`) cambi� o no coincide.

**Soluci�n:** Verificar que el valor sea id�ntico en todas las instancias de la API.

### Error: Blazor no carga (pantalla blanca)

**Causa:** MIME types incorrectos para `.wasm` y `.dll`.

**Soluci�n IIS:** Verificar `web.config` del frontend con los MIME types listados en secci�n 8.3.

### Error: Migraciones fallan

```powershell
# Ver detalle del error:
dotnet ef database update `
  --project src\AgoraHub360.ERP.Persistence `
  --startup-project src\AgoraHub360.ERP.Api `
  --verbose
```

---

## 13. Checklist de Despliegue

### Primera vez (Instalaci�n)

```
?  1.  Instalar .NET 8.0 Runtime + ASP.NET Core en servidor de API
?  2.  Instalar Hosting Bundle (si usa IIS)
?  3.  Crear base de datos SQL Server
?  4.  Crear usuario SQL con permisos db_owner
?  5.  Aplicar migraciones EF Core a la base de datos
?  6.  Publicar API: dotnet publish -c Release
?  7.  Copiar archivos de API al servidor
?  8.  Crear appsettings.Production.json en el servidor (NO copiar del repo)
?  9.  Configurar IIS / Nginx para la API
? 10.  Verificar: GET /health ? Healthy
? 11.  Actualizar ApiBaseUrl en wwwroot/appsettings.json del frontend
? 12.  Publicar Web: dotnet publish -c Release
? 13.  Copiar wwwroot al servidor web
? 14.  Configurar IIS / Nginx para frontend (con reescritura SPA)
? 15.  Verificar CORS: BlazorBaseUrl = URL del frontend
? 16.  Probar login desde el navegador
? 17.  Generar cat�logos desde la UI (Plan de Cuentas, Tipos Comprobante, etc.)
? 18.  Configurar backup autom�tico de SQL Server
? 19.  Configurar monitoreo de /health
? 20.  Documentar credenciales en gestor de secretos (NO en archivos)
```

### Actualizaciones posteriores

```
?  1.  Pull del repositorio: git pull origin main
?  2.  Aplicar migraciones pendientes: dotnet ef database update
?  3.  Publicar API: dotnet publish -c Release -o publish/api
?  4.  Detener servicio API (IIS stop / systemctl stop)
?  5.  Copiar archivos nuevos al servidor (NO sobrescribir appsettings.Production.json)
?  6.  Iniciar servicio API
?  7.  Verificar /health
?  8.  Publicar Web: dotnet publish -c Release -o publish/web
?  9.  Copiar wwwroot al servidor web
? 10.  Forzar refresh en navegador (Ctrl+Shift+R) o limpiar cache
? 11.  Verificar funcionalidad
```

---

## Anexo A � Puertos y URLs por Entorno

| Entorno | API URL | Frontend URL | SQL Server |
|---------|---------|-------------|------------|
| **Desarrollo** | `https://localhost:7001` | `https://localhost:5002` | `192.168.88.14,56885` |
| **Producci�n** | `https://api-erp.midominio.com` | `https://erp.midominio.com` | `SERVIDOR_SQL:1433` |

## Anexo B � Comandos R�pidos

```powershell
# ??? DESARROLLO ???
dotnet build                                          # Compilar
dotnet run --project src\AgoraHub360.ERP.Api          # Ejecutar API
dotnet run --project src\AgoraHub360.ERP.Web          # Ejecutar Web

# ??? MIGRACIONES ???
dotnet ef migrations add NombreMigracion --project src\AgoraHub360.ERP.Persistence --startup-project src\AgoraHub360.ERP.Api
dotnet ef database update --project src\AgoraHub360.ERP.Persistence --startup-project src\AgoraHub360.ERP.Api
dotnet ef migrations script --idempotent --output migration.sql --project src\AgoraHub360.ERP.Persistence --startup-project src\AgoraHub360.ERP.Api

# ??? PUBLICACI�N ???
dotnet publish src\AgoraHub360.ERP.Api -c Release -o publish\api
dotnet publish src\AgoraHub360.ERP.Web -c Release -o publish\web

# ??? SERVIDOR ???
dotnet AgoraHub360.ERP.Api.dll --urls "http://0.0.0.0:5000"  # Ejecutar directamente
curl https://api-erp.midominio.com/health                      # Verificar salud
```

---

> **Contacto soporte t�cnico:** soporte@agorahub360.com  
> **Repositorio:** https://github.com/abelcalvimontes/AgoraHUB360-ERP
