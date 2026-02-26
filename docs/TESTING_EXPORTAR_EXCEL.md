# ?? Guía de Pruebas - Exportación Excel de Comprobantes

## ? Prueba Rápida (5 minutos)

### Paso 1: Iniciar el sistema
```powershell
# Terminal 1 - API
cd src\AgoraHub360.ERP.Api
dotnet run --launch-profile https

# Terminal 2 - Web
cd src\AgoraHub360.ERP.Web
dotnet run --launch-profile https
```

### Paso 2: Acceder al módulo
1. Abrir navegador: `https://localhost:5002`
2. Login con credenciales:
   - Email: `admin@agorahub360.com`
   - Password: `Admin123`
3. Navegar a: **Contabilidad > Asientos Contables**

### Paso 3: Probar exportación básica
1. **Sin filtros:**
   - Hacer clic en el botón **"Exportar Excel"** (botón verde con icono de Excel)
   - Verificar que aparece spinner
   - Esperar descarga automática del archivo
   - Verificar mensaje de éxito

2. **Abrir archivo descargado:**
   - Nombre del archivo: `Comprobantes_Contables_YYYYMMDD_HHmmss.xlsx`
   - Verificar que se abre correctamente en Excel
   - Revisar columnas y formato

### Paso 4: Probar con filtros
1. **Filtrar por fecha:**
   - Seleccionar "Desde" y "Hasta"
   - Hacer clic en buscar (??)
   - Exportar a Excel
   - Verificar que solo exporta comprobantes en ese rango

2. **Filtrar por estado:**
   - Seleccionar estado: "Contabilizado"
   - Exportar
   - Verificar que solo exporta comprobantes contabilizados

---

## ?? Plan de Pruebas Completo

### 1. Pruebas de Funcionalidad

#### TC-001: Exportar todos los comprobantes
**Precondición:** Tener al menos 3 comprobantes registrados

| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Ir a Asientos Contables | Listado visible |
| 2 | Clic en "Exportar Excel" | Spinner visible, botón deshabilitado |
| 3 | Esperar descarga | Archivo descargado automáticamente |
| 4 | Verificar mensaje | "Archivo Excel generado exitosamente" |
| 5 | Abrir archivo | Excel abre sin errores |
| 6 | Verificar datos | Todos los comprobantes presentes |

**Estado:** ? Pendiente / ? Pasó / ? Falló

---

#### TC-002: Exportar con filtro de fechas
**Datos de prueba:** Fecha desde: 01/01/2026, Fecha hasta: 28/02/2026

| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Seleccionar rango de fechas | Fechas configuradas |
| 2 | Clic en buscar | Listado filtrado |
| 3 | Clic en "Exportar Excel" | Archivo descargado |
| 4 | Abrir Excel | Solo comprobantes del rango |
| 5 | Verificar totales | Suma correcta de Debe/Haber |

**Estado:** ? Pendiente / ? Pasó / ? Falló

---

#### TC-003: Exportar por tipo de comprobante
**Datos de prueba:** Tipo: "Comprobante de Diario"

| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Seleccionar tipo de comprobante | Tipo seleccionado |
| 2 | Clic en buscar | Listado filtrado |
| 3 | Exportar | Solo comprobantes de ese tipo |
| 4 | Verificar columna "Tipo" | Todos del mismo tipo |

**Estado:** ? Pendiente / ? Pasó / ? Falló

---

#### TC-004: Exportar por estado
**Datos de prueba:** Estado: "Contabilizado"

| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Seleccionar estado | Estado seleccionado |
| 2 | Buscar y exportar | Archivo descargado |
| 3 | Verificar columna "Estado" | Todos "Contabilizado" |

**Estado:** ? Pendiente / ? Pasó / ? Falló

---

#### TC-005: Exportar con búsqueda de texto
**Datos de prueba:** Búsqueda: "Compra"

| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Ingresar texto en búsqueda | Texto visible |
| 2 | Buscar y exportar | Archivo descargado |
| 3 | Verificar datos | Solo comprobantes con "Compra" en concepto/glosa |

**Estado:** ? Pendiente / ? Pasó / ? Falló

---

#### TC-006: Sin datos para exportar
**Precondición:** Aplicar filtros que no retornen resultados

| Paso | Acción | Resultado Esperado |
|------|--------|-------------------|
| 1 | Aplicar filtro sin resultados | Lista vacía |
| 2 | Verificar botón Excel | Botón deshabilitado |
| 3 | Intentar hacer clic | No responde (deshabilitado) |

**Estado:** ? Pendiente / ? Pasó / ? Falló

---

### 2. Pruebas de Formato Excel

#### TC-007: Verificar estructura del archivo

| # | Elemento | Verificación | ? |
|---|----------|--------------|---|
| 1 | Nombre hoja | "Comprobantes Contables" | ? |
| 2 | Encabezados | Fondo azul, texto blanco | ? |
| 3 | Columnas | 15 columnas presentes | ? |
| 4 | Formato moneda | Debe/Haber con 2 decimales | ? |
| 5 | Fila totales | Suma correcta de Debe/Haber | ? |
| 6 | Ancho columnas | Auto-ajustado al contenido | ? |
| 7 | Borde totales | Línea doble superior | ? |

---

#### TC-008: Verificar datos exportados

| Columna | Presente | Formato Correcto | ? |
|---------|----------|------------------|---|
| Tipo | ? | Badge/código | ? |
| Número | ? | Texto | ? |
| Fecha | ? | dd/MM/yyyy | ? |
| Gestión | ? | Año | ? |
| Concepto | ? | Texto | ? |
| Glosa | ? | Texto completo | ? |
| Tipo Registro | ? | Manual/Automático | ? |
| T/C Moneda | ? | USD/BOB | ? |
| Valor T/C | ? | Número 2 decimales | ? |
| Tipo Pago | ? | Nombre tipo | ? |
| Nro Documento | ? | Texto | ? |
| Total Debe | ? | Formato moneda | ? |
| Total Haber | ? | Formato moneda | ? |
| Estado | ? | Borrador/Contabilizado/Anulado | ? |
| Registrado Por | ? | Usuario | ? |

---

### 3. Pruebas de Rendimiento

#### TC-009: Exportar 100 comprobantes
**Precondición:** Base de datos con 100+ comprobantes

| Métrica | Objetivo | Resultado | ? |
|---------|----------|-----------|---|
| Tiempo de generación | < 5 segundos | ___ seg | ? |
| Tamaño archivo | < 1 MB | ___ KB | ? |
| Memoria API | Estable | ___ MB | ? |
| Sin errores | 0 errores | ___ | ? |

---

#### TC-010: Exportar 1,000 comprobantes
**Precondición:** Base de datos con 1,000+ comprobantes

| Métrica | Objetivo | Resultado | ? |
|---------|----------|-----------|---|
| Tiempo de generación | < 15 segundos | ___ seg | ? |
| Tamaño archivo | < 5 MB | ___ KB | ? |
| Uso CPU servidor | < 80% | ___ % | ? |
| Sin timeout | OK | ___ | ? |

---

### 4. Pruebas de Navegadores

#### TC-011: Compatibilidad de navegadores

| Navegador | Versión | Descarga OK | Excel Abre | ? |
|-----------|---------|-------------|------------|---|
| Chrome | Última | ? | ? | ? |
| Edge | Última | ? | ? | ? |
| Firefox | Última | ? | ? | ? |
| Safari | Última | ? | ? | ? |

---

### 5. Pruebas de Errores

#### TC-012: Manejo de errores

| Escenario | Acción | Resultado Esperado | ? |
|-----------|--------|-------------------|---|
| API caída | Exportar | Mensaje de error visible | ? |
| Sin autenticación | Exportar | Redirigir a login | ? |
| Sin permisos | Exportar | Mensaje de error 403 | ? |
| Timeout | Esperar largo | Mensaje timeout | ? |

---

## ?? Reporte de Pruebas

### Resumen de Ejecución

**Fecha:** __________________
**Ejecutado por:** __________________
**Versión:** v1.0

| Estado | Cantidad | Porcentaje |
|--------|----------|------------|
| ? Pasó | __ / 12 | __% |
| ? Falló | __ / 12 | __% |
| ? Pendiente | __ / 12 | __% |

### Bugs Encontrados

| ID | Severidad | Descripción | Estado |
|----|-----------|-------------|--------|
| BUG-001 | Alta/Media/Baja | ___________________ | Abierto/Cerrado |
| BUG-002 | Alta/Media/Baja | ___________________ | Abierto/Cerrado |

### Observaciones

```
Notas generales sobre las pruebas:
_____________________________________________________________
_____________________________________________________________
_____________________________________________________________
```

---

## ?? Troubleshooting

### Problema: El botón está deshabilitado
**Solución:**
- Verificar que hay datos en la lista
- Recargar la página
- Verificar conexión con la API

### Problema: Error al descargar
**Solución:**
1. Abrir consola del navegador (F12)
2. Ver errores en Network tab
3. Verificar que API responde: `GET /api/v1/contabilidad/asientos/exportar-excel`
4. Verificar autenticación (token JWT válido)

### Problema: Excel no abre correctamente
**Solución:**
- Verificar que el archivo no está corrupto
- Intentar abrir con LibreOffice/Google Sheets
- Verificar tamaño del archivo (> 0 bytes)

### Problema: Datos incorrectos en Excel
**Solución:**
- Verificar filtros aplicados
- Comparar con listado en pantalla
- Revisar logs de la API

---

## ?? Contacto

**Soporte Técnico:**
- Documentación: `docs/EXPORTAR_EXCEL_ASIENTOS.md`
- Issues: GitHub Repository Issues
- Email: soporte@agorahub360.com

---

**Última actualización:** 2026-02-26
**Versión del documento:** 1.0
