# Exportación a Excel de Comprobantes Contables

## ?? Resumen

Se implementó la funcionalidad de exportar a Excel los comprobantes contables filtrados en el formulario de asientos contables.

## ? Características Implementadas

### 1. **Botón de Exportación**
- **Ubicación:** Formulario de Asientos Contables (`/contabilidad/asientos`)
- **Posición:** En la barra de filtros, junto al botón de búsqueda
- **Estado:** Se deshabilita cuando:
  - No hay datos para exportar
  - Se está generando el archivo (muestra spinner)

### 2. **Filtros Aplicados**
La exportación respeta todos los filtros activos:
- ? Rango de fechas (desde/hasta)
- ? Tipo de comprobante
- ? Estado (Borrador, Contabilizado, Anulado)
- ? Búsqueda de texto

### 3. **Contenido del Excel**

#### Columnas incluidas:
1. Tipo
2. Número
3. Fecha
4. Gestión
5. Concepto
6. Glosa
7. Tipo Registro
8. T/C Moneda
9. Valor T/C
10. Tipo Pago
11. Nro Documento
12. Total Debe
13. Total Haber
14. Estado
15. Registrado Por

#### Formato y estilos:
- ? Encabezados con fondo azul y texto blanco
- ? Formato de moneda en columnas de importes
- ? Fila de totales al final con suma de Debe y Haber
- ? Columnas auto-ajustadas al contenido
- ? Borde superior doble en la fila de totales

### 4. **Nombre del Archivo**
Formato: `Comprobantes_Contables_YYYYMMDD_HHmmss.xlsx`

Ejemplo: `Comprobantes_Contables_20260226_143022.xlsx`

## ??? Cambios Técnicos

### 1. **Paquete NuGet Agregado**
```xml
<PackageReference Include="EPPlus" Version="7.5.2" />
```

**Proyecto:** `AgoraHub360.ERP.Api`

**Licencia:** Configurada como `NonCommercial` en el controlador

### 2. **Nuevo Endpoint en API**

**Ruta:** `GET /api/v1/contabilidad/asientos/exportar-excel`

**Parámetros Query:**
- `desde` (DateTime?, opcional)
- `hasta` (DateTime?, opcional)
- `estado` (string, opcional)
- `tipoComprobanteId` (int?, opcional)
- `search` (string, opcional)

**Respuesta:** Archivo Excel (application/vnd.openxmlformats-officedocument.spreadsheetml.sheet)

**Código:** `AsientosContablesController.ExportarExcel()`

### 3. **Nuevo Método en Servicio HTTP**

**Archivo:** `AsientoContableHttpService.cs`

**Método:** `ExportarExcelAsync()`

**Retorno:** `Task<byte[]?>` - Bytes del archivo Excel

### 4. **Función JavaScript**

**Archivo:** `index.html`

**Función:** `window.downloadFile(filename, contentType, base64Content)`

**Propósito:** Descargar archivos desde Blazor WebAssembly

### 5. **Componente Razor Actualizado**

**Archivo:** `AsientosContables.razor`

**Cambios:**
- Agregado botón "Exportar Excel" con icono de Excel
- Variable de estado `exportando` para controlar el spinner
- Método `ExportarExcel()` que:
  1. Obtiene los bytes del archivo desde la API
  2. Convierte a Base64
  3. Invoca función JavaScript para descarga
  4. Muestra mensaje de éxito/error

## ?? Uso

### Pasos para el usuario:

1. **Navegar al módulo:**
   - Ir a `Contabilidad > Asientos Contables`

2. **Aplicar filtros (opcional):**
   - Seleccionar rango de fechas
   - Filtrar por tipo de comprobante
   - Filtrar por estado
   - Buscar por texto

3. **Hacer clic en "Exportar Excel":**
   - El botón muestra un spinner mientras genera el archivo
   - El archivo se descarga automáticamente al navegador
   - Mensaje de confirmación aparece al finalizar

4. **Abrir el archivo:**
   - El archivo se guarda en la carpeta de descargas
   - Compatible con Microsoft Excel, LibreOffice Calc, Google Sheets

## ?? Testing

### Escenarios probados:
- ? Compilación exitosa del proyecto
- ? Integración correcta de EPPlus
- ? Endpoint registrado correctamente
- ? Servicio HTTP actualizado
- ? Función JavaScript agregada

### Casos de prueba recomendados:
1. Exportar todos los comprobantes (sin filtros)
2. Exportar comprobantes filtrados por fecha
3. Exportar comprobantes de un tipo específico
4. Exportar comprobantes por estado
5. Exportar con búsqueda de texto
6. Exportar cuando no hay datos (botón deshabilitado)
7. Verificar formato del archivo Excel
8. Verificar totales calculados correctamente

## ?? Archivos Modificados

```
src/
??? AgoraHub360.ERP.Api/
?   ??? AgoraHub360.ERP.Api.csproj          [Modificado - Agregado EPPlus]
?   ??? Controllers/V1/
?       ??? AsientosContablesController.cs  [Modificado - Endpoint exportar]
?
??? AgoraHub360.ERP.Web/
    ??? Pages/Contabilidad/
    ?   ??? AsientosContables.razor         [Modificado - Botón y método]
    ??? Services/
    ?   ??? AsientoContableHttpService.cs   [Modificado - Método exportar]
    ??? wwwroot/
        ??? index.html                      [Modificado - Función JS]
```

## ?? Despliegue

### Requisitos previos:
- Restaurar paquetes NuGet: `dotnet restore`
- Compilar solución: `dotnet build`

### Pasos de despliegue:
1. Publicar API con el nuevo paquete EPPlus
2. Publicar Web con los cambios de UI
3. Verificar que EPPlus.LicenseContext está configurado

## ?? Notas Importantes

### Licencia EPPlus:
- **Versión:** 7.5.2
- **Licencia:** NonCommercial
- **Configuración:** En constructor de `AsientosContablesController`
- **Nota:** Para uso comercial, adquirir licencia de EPPlus

### Limitaciones:
- Exporta solo los datos visibles según filtros aplicados
- No exporta el detalle de líneas de cada comprobante (solo totales)
- Máximo recomendado: 10,000 registros por archivo

### Rendimiento:
- Generación asíncrona en servidor
- Descarga directa al navegador
- No almacena archivos temporales en servidor

## ?? Mejoras Futuras

### Posibles extensiones:
1. **Exportar con detalle de líneas:**
   - Agregar hoja adicional con todas las líneas de los comprobantes

2. **Exportar comprobante individual:**
   - Botón en vista detalle para exportar un solo comprobante con sus líneas

3. **Personalizar columnas:**
   - Permitir al usuario seleccionar qué columnas exportar

4. **Formatos adicionales:**
   - PDF
   - CSV
   - JSON

5. **Exportación programada:**
   - Enviar por email automáticamente
   - Guardar en servidor o nube

## ?? Referencias

- [EPPlus Documentation](https://epplussoftware.com/docs/5.0/)
- [EPPlus GitHub](https://github.com/EPPlusSoftware/EPPlus)
- [Blazor File Download](https://learn.microsoft.com/en-us/aspnet/core/blazor/file-downloads)

---

**Implementado por:** GitHub Copilot
**Fecha:** 2026-02-26
**Versión del sistema:** v1.0
**Estado:** ? Completado y compilado exitosamente
