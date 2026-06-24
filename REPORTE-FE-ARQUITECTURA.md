# Reporte de Validación Arquitectónica — Facturación Electrónica (FE)

**Fecha:** 2026-06-14  
**Rama:** FE_Agora (commit 17aa6cd)  
**Base de datos:** db_AgoraERP_Core  
**Build:** ✅ Correcto (0 errores, 0 warnings)

---

## 1. Resumen Ejecutivo

| Pregunta | Respuesta |
|----------|-----------|
| ¿Código y BD están alineados? | **Parcialmente.** Código y BD tienen las mismas tablas, pero la BD se creó sin `__EFMigrationsHistory`. Posiblemente mediante `EnsureCreated` o scripts externos. |
| ¿Se guarda en las tablas correctas? | **Sí.** El flujo FE actualiza `[vta].[FacturasVenta]` (campos FE) e inserta en `[cfg].[AuditoriaFacturacion]`. |
| ¿Existe uso real de `[fe]`? | **No.** El esquema `[fe]` no existe en BD ni en código. |
| ¿Hay tablas huérfanas o duplicadas? | **Sí.** En `[dbo]` existen 4 tablas heredadas: `FacturasVenta`, `ACC_CierresContables`, `CobranzasVenta`, `Ventas`. |

---

## 2. Mapa de Tablas por Esquema

### 2.1 `[cfg]` — Configuración (4 tablas, 100% FE)

| Tabla | Propósito | # Registros |
|-------|-----------|-------------|
| `AmbientesFacturacionElectronica` | Catálogo de ambientes FE (TEST, PRODUCCION, PILOTO) | 3 |
| `ProveedoresFacturacionElectronica` | Catálogo de proveedores FE (CIRRUS, AGORAFC, SIAT_DIRECTO) | 3 |
| `ConfiguracionFacturacionElectronica` | Configuración FE por empresa (tenant-aware, credenciales cifradas) | 1 |
| `AuditoriaFacturacion` | Traza de cada operación FE (emisión, anulación, consulta) | 1 |

**Propósito:** `[cfg]` almacena configuración técnica + auditoría operacional. Correcto.

### 2.2 `[vta]` — Ventas (8 tablas, 3 con campos FE)

| Tabla | Propósito | Campos FE |
|-------|-----------|-----------|
| `Ventas` | Documento de venta base | `FacturaGenerada` |
| `VentaDetalles` | Líneas de venta | — |
| `VentaPagos` | Pagos asociados | `FacturaVentaId` (FK) |
| `VentaFacturacionDatos` | Datos fiscales de la venta | `Facturar`, `NitFactura`, `RazonSocialFactura`, `EmailFactura`, `EstadoFactura`, `FacturaId` |
| `FacturasVenta` | **Factura emitida** con datos FE | `BillUuid`, `Cuf`, `Cufd`, `Cuis`, `EstadoSiat`, `SiatQr`, `EnlaceXml`, `EnlacePdf`, `ActivityCode`, `TipoDocumentoFactura` |
| `FacturaVentaDetalles` | Líneas de la factura | — |
| `PedidosVenta` | Pedido (pre-venta) | — |
| `SiatMetodosPago` | Catálogo métodos pago SIAT | — |

**Propósito:** `[vta]` almacena los documentos comerciales y sus campos FE. Correcto.

### 2.3 `[fe]` — No existe

El esquema `[fe]` **no fue creado** en la BD ni existe referencia en código.

### 2.4 `[dbo]` — Tablas huérfanas (4)

| Tabla | Problema |
|-------|----------|
| `FacturasVenta` | Columnas tipo workflow (`EstadoActualId`, `ResponsableUltimoEstadoId`). **Versión antigua**, diferente estructura a `[vta].[FacturasVenta]`. Sin FK ni migración que la respalde. |
| `ACC_CierresContables` | También existe en `[acc].[ACC_CierresContables]` |
| `CobranzasVenta` | Sin migración que la respalde |
| `Ventas` | También existe en `[vta].[Ventas]` |

---

## 3. Trazabilidad del Flujo FE

### 3.1 "Emitir Factura FE" — Ruta completa

```
WEB UI                           API LAYER                     APPLICATION              INFRASTRUCTURE          BD
[Facturas.razor]                 [FacturacionElectronicaController]                   [Provider]
                                                                                        
Botón "Emitir FE"                                                                        
  │                                                                                      
  ▼                                                                                      
FacturacionFEHttpService                                                                 
.EmitirAsync(id)                                                                         
  │                                                                                      
  ▼ POST /api/v1/facturacion-electronica/emitir/{id}                                    
  │                                                                                      
  ▼                                                                                      
controller.Emitir(id)                                                                    
  │                                                                                      
  ▼                                                                                      
FacturacionElectronicaService                                                            
.EmitirAsync(facturaVentaId)                                                             
  │                                                                                      
  ├─ 1. Cargar FacturaVenta desde [vta].FacturasVenta (repo)                             
  ├─ 2. Validar estado (Borrador/Generada, sin CUF)                                      
  ├─ 3. Cargar detalles desde [vta].FacturaVentaDetalles                                 
  ├─ 4. Validar ItemCode en cada detalle                                                 
  ├─ 5. Obtener Configuración FE activa desde [cfg].ConfiguracionFacturacionElectronica  
  │     (incluye ProveedorFacturacionElectronica + AmbienteFacturacionElectronica)       
  ├─ 6. Resolver provider por código (CIRRUS → CirrusFacturacionProvider)                
  │     └─ Inyectado como IFacturacionElectronicaProvider                                
  ├─ 7. Generar BillUuid si no existe                                                    
  │                                                                                      
  ▼                                                                                      
CirrusFacturacionProvider                                                                
.EmitirFacturaAsync(config, request)                                                     
  ├─ Obtener token OAuth2 (client_credentials con cifrado descifrado)                    
  ├─ POST {ApiBillingUrl}/bill/sales (Bearer + PosToken)                                 
  ├─ Parsear respuesta Cirrus (Cuf, Cufd, Qr, PdfUrl, XmlUrl, Status)                    
  └─ Retornar EmisionFacturaResultDto                                                    
  │                                                                                      
  ▼                                                                                      
FacturacionElectronicaService (post-provider)                                            
  ├─ 10. Si exitoso → Actualizar FacturaVenta:                                           
  │     [vta].[FacturasVenta]: Cuf, Cufd, SiatQr, EnlacePdf, EnlaceXml,                 
  │     EstadoSiat=Validada/Pendiente, EstadoFactura=Generada                            
  ├─ 11. SIEMPRE → Insertar AuditoriaFacturacion:                                       
  │     [cfg].[AuditoriaFacturacion]: FacturaVentaId, ProveedorCodigo,                  
  │     AmbienteCodigo, BillUuid, Cuf, EstadoSiat, Exitoso,                             
  │     TiempoRespuestaMs, MensajeError, UsuarioId, FechaHora                           
  └─ 12. Guardar cambios (SaveChangesAsync)                                              
```

### 3.2 Archivos involucrados

| Capa | Archivo | Rol |
|------|---------|-----|
| **Web** | `Pages/Ventas/Facturas.razor` | UI: listado, botón "Emitir FE" |
| **Web** | `Services/FacturacionFEHttpService.cs` | HTTP Client para API FE |
| **Web** | `Pages/Configuracion/ConfiguracionFE.razor` | UI: CRUD configuración FE |
| **Web** | `Pages/Configuracion/AuditoriaFE.razor` | UI: consulta de auditoría |
| **API** | `Controllers/V1/FacturacionElectronicaController.cs` | Endpoints REST: emitir, anular, estado |
| **API** | `Controllers/V1/FacturasVentaController.cs` | CRUD facturas de venta |
| **App** | `Services/FacturacionElectronicaService.cs` | Orquestador del flujo FE |
| **App** | `Interfaces/IFacturacionElectronicaService.cs` | Contrato del servicio |
| **App** | `Interfaces/IFacturacionElectronicaProvider.cs` | Contrato del provider técnico |
| **Infra** | `FacturacionElectronica/CirrusFacturacionProvider.cs` | Provider Cirrus (OAuth2 + REST) |
| **Infra** | `DependencyInjection.cs` (línea 21) | Registro DI de providers |
| **App** | `DependencyInjection.cs` (línea 118) | Registro DI de servicio FE |
| **Domain** | `Entities/VTA/FacturaVenta.cs` | Entidad con campos FE (BillUuid, Cuf, Cufd, EstadoSiat...) |
| **Domain** | `Entities/FE/AuditoriaFacturacion.cs` | Entidad de auditoría |
| **Domain** | `Entities/FE/ConfiguracionFacturacionElectronica.cs` | Configuración tenant FE |
| **Domain** | `Entities/FE/AmbienteFacturacionElectronica.cs` | Catálogo de ambientes |
| **Domain** | `Entities/FE/ProveedorFacturacionElectronica.cs` | Catálogo de proveedores |
| **Domain** | `Enums/EstadoSiatFactura.cs` | Enum: NoEnviada, Pendiente, Validada, Rechazada, Anulada |
| **Persistence** | `Configurations/FE/*Configuration.cs` | EF Core mappings → `[cfg]` |
| **Persistence** | `Configurations/VTA/FacturaVentaConfiguration.cs` | EF Core mapping → `[vta]` |
| **Persistence** | `Migrations/20260610222150_FE_CfgFacturacionElectronica.cs` | Migración FE |
| **Persistence** | `Migrations/AgoraDbContextModelSnapshot.cs` | Snapshot (FE entidades en cfg líneas 3921-4255) |
| **Persistence** | `Repositories/FE/*Repository.cs` | Repositorios EF para cfg |

---

## 4. Validación de Consistencia

### 4.1 Configuraciones EF Core vs ModelSnapshot ✅

Todas las configuraciones en `Configurations/FE/` coinciden con el `ModelSnapshot`:

| Entidad | Configuration | ModelSnapshot |
|---------|---------------|---------------|
| `AmbienteFacturacionElectronica` | `ToTable("AmbientesFacturacionElectronica", "cfg")` | ✅ `ToTable("AmbientesFacturacionElectronica", "cfg")` |
| `AuditoriaFacturacion` | `ToTable("AuditoriaFacturacion", "cfg")` | ✅ `ToTable("AuditoriaFacturacion", "cfg")` |
| `ConfiguracionFacturacionElectronica` | `ToTable("ConfiguracionFacturacionElectronica", "cfg")` | ✅ `ToTable("ConfiguracionFacturacionElectronica", "cfg")` |
| `ProveedorFacturacionElectronica` | `ToTable("ProveedoresFacturacionElectronica", "cfg")` | ✅ `ToTable("ProveedoresFacturacionElectronica", "cfg")` |
| `FacturaVenta` | `ToTable("FacturasVenta", "vta")` | ✅ `ToTable("FacturasVenta", "vta")` |

### 4.2 Migraciones vs BD Real ⚠️

La tabla `[dbo].[__EFMigrationsHistory]` **no existe** en la BD. Esto indica que la BD no fue creada con `dotnet ef database update`. Posiblemente se usó `EnsureCreated()` o scripts manuales. Sin embargo, el esquema físico coincide con el ModelSnapshot, lo cual sugiere que la BD fue generada a partir del mismo modelo.

### 4.3 FKs entre esquemas ✅

| FK | Origen | Destino | Es correcto |
|----|--------|---------|-------------|
| `FK_AuditoriaFacturacion_FacturasVenta_FacturaVentaId` | `[cfg].[AuditoriaFacturacion]` → `[vta].[FacturasVenta]` | ✅ Cross-schema necesario |
| `FK_ConfigFE_Ambientes` | `[cfg].[ConfiguracionFE]` → `[cfg].[AmbientesFE]` | ✅ Intra-schema |
| `FK_ConfigFE_Proveedores` | `[cfg].[ConfiguracionFE]` → `[cfg].[ProveedoresFE]` | ✅ Intra-schema |

### 4.4 Datos de prueba (BD real)

- **Configuración activa:** 1 registro (empresa configurada con CIRRUS/TEST)
- **FacturasVenta:** 14 registros, 1 con BillUuid asignado (Id=14)
- **Auditoría:** 1 registro (Id=1, FacturaVentaId=14, CIRRUS/TEST, **Exitoso=0**)

---

## 5. Hallazgos

### ✅ Correcto

| # | Hallazgo |
|---|----------|
| C1 | No existen referencias al esquema `[fe]` en código ni BD |
| C2 | Las 4 entidades FE están correctamente mapeadas a `[cfg]` |
| C3 | `FacturasVenta` con todos sus campos FE mapeados a `[vta]` |
| C4 | El flujo FE está limpio: no usa SQL crudo ni Dapper |
| C5 | La auditoría registra operaciones exitosas y fallidas |
| C6 | Los secretos se cifran al almacenarse (`ClientSecretEncrypted`, `PosTokenEncrypted`) |
| C7 | El provider se resuelve por código string (extensible sin modificar servicio) |
| C8 | Build exitoso (0 errores) |

### ⚠️ Riesgo

| # | Hallazgo | Riesgo |
|---|----------|--------|
| R1 | `[dbo]` tiene 4 tablas huérfanas (`FacturasVenta`, `ACC_CierresContables`, `CobranzasVenta`, `Ventas`) | **Medio.** Nadie las usa pero pueden causar confusión. Podrían tener datos no migrados. |
| R2 | `__EFMigrationsHistory` no existe en BD | **Medio.** Al aplicar futuras migraciones, EF Core intentará crearla. Si el snapshot no coincide exactamente, generará migración inicial falsa. |
| R3 | Auditoría (`AuditoriaFacturacion`) está en `[cfg]` en lugar de un esquema `[aud]` o `[log]` | **Bajo.** Coherente con la arquitectura actual, pero conceptualmente mezcla configuración con datos operacionales. |

### ❌ Inconsistencia

| # | Hallazgo |
|---|----------|
| I1 | **Ninguna inconsistencia grave.** Código y BD están alineados en estructura y esquemas. |

---

## 6. Recomendación Final

### No hacer cambios ahora

El sistema es funcional y consistente. Las tablas `[dbo]` huérfanas **no interfieren** con el funcionamiento actual porque EF Core nunca las consulta ni modifica.

### Acción futura sugerida

1. **Verificar tablas `[dbo]` huérfanas**: Consultar si tienen datos (con `SELECT COUNT(*)`). Si no tienen datos relevantes, planificar un script de limpieza (`DROP TABLE`) para la próxima ventana de mantenimiento.
2. **Forzar migraciones**: Ejecutar `dotnet ef database update` contra una base de datos de prueba para confirmar que la migración `FE_CfgFacturacionElectronica` se aplica correctamente y crea el `__EFMigrationsHistory`.
3. **Documentar regla arquitectónica**:
   - `[cfg]` = configuración técnica + auditoría operacional (aceptado actualmente)
   - `[vta]` = documentos comerciales con campos FE embebidos
   - `[fe]` = reservado, no usado, podría albergar en el futuro documentos XML/JSON de intercambio con proveedores si se decide separarlos de `[vta]`
4. **No mover `AuditoriaFacturacion`** de `[cfg]` a menos que se cree un esquema `[aud]` global.

### Resumen de acciones

| Acción | Prioridad | ¿Requiere migración? |
|--------|-----------|---------------------|
| ✅ No cambiar nada | Inmediata | No |
| 🔍 Verificar tablas `[dbo]` | Corto plazo | No |
| 📝 Ejecutar `database update` en test | Corto plazo | Sí (aplica existente) |
| 🧹 Limpiar `[dbo]` huérfanas | Medio plazo | Script SQL manual |
| 📄 Documentar regla arquitectónica | Inmediata | No |

---

*Reporte generado automáticamente por análisis de código + consultas SQL Server.*
