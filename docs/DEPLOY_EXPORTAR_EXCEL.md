# ?? Instrucciones de Deploy - Exportación Excel

## ? Estado Actual
- ? Código implementado
- ? Compilación exitosa
- ? Documentación completa
- ? Listo para commit

---

## ?? Realizar Commit

### Opción 1: Usando el mensaje preparado
```bash
# Ver archivos modificados
git status

# Agregar archivos al staging
git add src/AgoraHub360.ERP.Api/AgoraHub360.ERP.Api.csproj
git add src/AgoraHub360.ERP.Api/Controllers/V1/AsientosContablesController.cs
git add src/AgoraHub360.ERP.Web/Pages/Contabilidad/AsientosContables.razor
git add src/AgoraHub360.ERP.Web/Services/AsientoContableHttpService.cs
git add src/AgoraHub360.ERP.Web/wwwroot/index.html
git add docs/EXPORTAR_EXCEL_ASIENTOS.md
git add docs/TESTING_EXPORTAR_EXCEL.md
git add docs/RESUMEN_EXPORTAR_EXCEL.md

# Commit con mensaje preparado
git commit -F COMMIT_MESSAGE.txt

# Push a la rama actual
git push
```

### Opción 2: Commit manual
```bash
git add .
git commit -m "feat(contabilidad): agregar exportación a Excel de comprobantes contables"
git push
```

---

## ?? Restaurar Paquetes NuGet

Antes del primer uso, restaurar paquetes:

```bash
# En la raíz del proyecto
dotnet restore

# O específicamente en la API
cd src/AgoraHub360.ERP.Api
dotnet restore
```

---

## ?? Testing Post-Deploy

### 1. Verificar API
```bash
# Iniciar API
cd src/AgoraHub360.ERP.Api
dotnet run --launch-profile https

# En otro terminal, probar endpoint
curl -X GET "https://localhost:7001/api/v1/contabilidad/asientos/exportar-excel" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -o test.xlsx
```

### 2. Verificar Web
```bash
# Iniciar Web
cd src/AgoraHub360.ERP.Web
dotnet run --launch-profile https

# Abrir navegador
start https://localhost:5002/contabilidad/asientos
```

### 3. Prueba Manual Rápida
1. Login: `admin@agorahub360.com` / `Admin123`
2. Ir a: Contabilidad ? Asientos Contables
3. Clic en "Exportar Excel"
4. Verificar descarga y apertura del archivo

---

## ?? Publicación (Producción)

### Opción A: Publicar con dotnet CLI
```bash
# API
cd src/AgoraHub360.ERP.Api
dotnet publish -c Release -o ./publish

# Web
cd src/AgoraHub360.ERP.Web
dotnet publish -c Release -o ./publish
```

### Opción B: Publicar con Visual Studio
1. Click derecho en proyecto `AgoraHub360.ERP.Api`
2. "Publish..."
3. Seleccionar perfil de publicación
4. Repetir para `AgoraHub360.ERP.Web`

---

## ?? Configuración Post-Deploy

### 1. Verificar licencia EPPlus

Si es uso comercial, agregar licencia en `appsettings.json`:

```json
{
  "EPPlus": {
    "LicenseContext": "Commercial",
    "LicenseKey": "YOUR_LICENSE_KEY"
  }
}
```

Y actualizar el código:

```csharp
// En AsientosContablesController.cs
public AsientosContablesController(IAsientoContableService service, IConfiguration config)
{
    _service = service;
    ExcelPackage.LicenseContext = LicenseContext.Commercial;
    // Cargar licencia si es necesario
}
```

### 2. Verificar permisos de archivo

Asegurar que el servidor tiene permisos para crear archivos temporales.

---

## ?? Troubleshooting

### Problema: "EPPlus requires a license"
**Solución:**
```csharp
// Ya está configurado en el constructor como NonCommercial
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
```

### Problema: "Cannot download file"
**Verificar:**
1. Que la función JavaScript está en `index.html`
2. Que no hay errores en consola del navegador
3. Que el token JWT es válido

### Problema: "File is corrupted"
**Verificar:**
1. Que EPPlus generó el archivo correctamente
2. Que no hay errores en la API
3. Revisar logs del servidor

---

## ?? Monitoreo Post-Deploy

### Métricas a observar:

1. **Performance:**
   - Tiempo de respuesta del endpoint
   - Uso de memoria en servidor
   - Tamaño de archivos generados

2. **Uso:**
   - Cantidad de exportaciones por día
   - Usuarios que exportan
   - Filtros más utilizados

3. **Errores:**
   - Errores 500 en endpoint
   - Timeouts
   - Archivos corruptos

### Logs a revisar:
```bash
# Logs de la API
tail -f /var/log/agorahub360-api/logs.txt

# O en Windows con IIS
# Event Viewer ? Application Logs
```

---

## ?? Seguridad

### Consideraciones implementadas:
- ? Endpoint requiere autenticación (`[Authorize]`)
- ? Solo exporta datos de la empresa del usuario (filtro por EmpresaId)
- ? Respeta permisos de roles
- ? No almacena archivos en servidor

### Recomendaciones adicionales:
- ? Limitar cantidad de registros por exportación
- ? Rate limiting (ej: 10 exportaciones por minuto)
- ? Auditoría de exportaciones
- ? Encriptar datos sensibles en tránsito

---

## ?? Documentación Disponible

1. **Técnica completa:**
   - `docs/EXPORTAR_EXCEL_ASIENTOS.md`

2. **Plan de pruebas:**
   - `docs/TESTING_EXPORTAR_EXCEL.md`

3. **Resumen ejecutivo:**
   - `docs/RESUMEN_EXPORTAR_EXCEL.md`

4. **Este archivo:**
   - `docs/DEPLOY_EXPORTAR_EXCEL.md`

---

## ? Checklist Pre-Deploy

```
[ ] Código compilado sin errores
[ ] Tests unitarios pasando (si existen)
[ ] Documentación actualizada
[ ] Commit realizado
[ ] Push a repositorio
[ ] Paquetes NuGet restaurados
[ ] Configuración de producción verificada
[ ] Backup de base de datos (opcional)
[ ] Plan de rollback definido
```

---

## ?? Plan de Rollback

Si algo sale mal después del deploy:

```bash
# 1. Revertir commit
git revert HEAD

# 2. Push del revert
git push

# 3. Re-deploy de la versión anterior
dotnet publish -c Release

# 4. Reiniciar servicios
# (Depende de la infraestructura)
```

---

## ?? Contacto de Emergencia

**Soporte Técnico:**
- Email: soporte@agorahub360.com
- Slack: #agorahub360-support
- OnCall: [Número de emergencia]

**Responsable de Feature:**
- Developer: GitHub Copilot
- Reviewer: [Nombre del reviewer]
- Approver: [Nombre del approver]

---

## ? Post-Deploy

### Tareas inmediatas:
1. ? Verificar que el endpoint responde
2. ? Probar exportación manual
3. ? Verificar logs sin errores
4. ? Notificar a usuarios clave
5. ? Actualizar release notes

### Seguimiento (1 semana):
1. ? Revisar métricas de uso
2. ? Recopilar feedback de usuarios
3. ? Identificar bugs si existen
4. ? Planear mejoras

---

**Última actualización:** 2026-02-26
**Versión:** v1.0
**Estado:** ? Listo para Deploy

