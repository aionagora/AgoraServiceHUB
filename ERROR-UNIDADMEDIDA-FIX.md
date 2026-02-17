# ? Error: "Error al cargar unidades de medida"

## ?? Problema Identificado

El error **"Error al cargar unidades de medida"** aparece en la aplicación Blazor porque **la API no está corriendo**.

---

## ?? Causa

La aplicación web Blazor WebAssembly intenta conectarse a:
```
https://localhost:7001/api/v1/unidades-medida
```

Pero la API **NO ESTÁ INICIADA**, por lo que la conexión falla.

---

## ? Solución

### Opción 1: Inicio Automático (Recomendado) ??

```powershell
.\start-system.ps1
```

Este script:
1. Detiene procesos previos
2. Inicia la API en puerto 7001
3. Inicia la Web en puerto 5002
4. Abre el navegador automáticamente

---

### Opción 2: Inicio Manual

**Terminal 1 - Iniciar API:**
```powershell
cd src\AgoraHub360.ERP.Api
dotnet run --launch-profile https
```

Espera hasta ver:
```
Now listening on: https://localhost:7001
Application started. Press Ctrl+C to shut down.
```

**Terminal 2 - Iniciar Web:**
```powershell
cd src\AgoraHub360.ERP.Web
dotnet run --launch-profile https
```

Espera hasta ver:
```
Now listening on: https://localhost:5002
```

---

## ?? Después de Iniciar

1. **Abre el navegador** en: `https://localhost:5002`
2. **Login:**
   - Email: `admin@agorahub360.com`
   - Password: `Admin123`
3. **Navega a:** Datos Maestros ? **Unid. Medida**
4. ? **El error desaparecerá**

---

## ?? Verificación Rápida

Para diagnosticar el problema en el futuro:

```powershell
.\diagnose-unidadmedida.ps1
```

Este script verifica:
- ? Procesos corriendo
- ? Puertos abiertos (7001 y 5002)
- ? Conexión a la API
- ? Configuración correcta
- ? Compilación sin errores

---

## ?? Otros Errores Posibles

Si después de iniciar la API y Web **el error persiste**, puede ser:

### 1. Token JWT Expirado
**Síntoma:** Error 401 Unauthorized  
**Solución:** Cerrar sesión y volver a hacer login

### 2. No hay empresa asignada
**Síntoma:** "No se pudo determinar la empresa activa"  
**Solución:** 
1. Ir a Configuración ? Empresas
2. Verificar que existe al menos una empresa activa
3. Verificar que el usuario está asociado a una empresa

### 3. CORS Error
**Síntoma:** "CORS policy: No 'Access-Control-Allow-Origin'"  
**Solución:** Verificar `src/AgoraHub360.ERP.Api/Program.cs` tenga:
```csharp
app.UseCors("BlazorWasm");
```

### 4. Base de datos vacía
**Síntoma:** Página carga pero no muestra unidades  
**Solución:** Crear unidades de prueba desde la interfaz

---

## ?? Estado del Sistema

### ? Código
- Compilación: **Sin errores**
- DTOs: **Creados correctamente**
- Servicios: **Registrados**
- Controlador API: **Funcional**
- Página Razor: **Completa**

### ? Ejecución
- API: **NO está corriendo** (causa del error)
- Web: **NO está corriendo**

---

## ?? Archivos de Ayuda

| Archivo | Descripción |
|---------|-------------|
| `start-system.ps1` | Inicia API y Web automáticamente |
| `diagnose-unidadmedida.ps1` | Diagnóstico completo del sistema |
| `UNIDADMEDIDA-TROUBLESHOOTING.md` | Guía detallada de troubleshooting |
| `UNIDADMEDIDA-STATUS.md` | Estado completo de la implementación |

---

## ? TL;DR

```powershell
# El error es porque la API no está corriendo
# Solución en 1 comando:
.\start-system.ps1
```

Espera 30 segundos, abre `https://localhost:5002`, haz login y ve a Unid. Medida.

---

**El error desaparecerá cuando la API esté corriendo** ?
