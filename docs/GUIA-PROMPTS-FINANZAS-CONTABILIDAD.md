# GUÍA DE PROMPTS — Finanzas y Contabilidad
## AgoraHUB360 ERP

**Propósito:** Documento de referencia técnica para elaborar prompts precisos de mejora del módulo de Finanzas y Contabilidad.  
**Fecha:** 2026-04-10  
**Complementa:** `docs/MODULO-FINANCIERO-CONTABLE.md`

---

## 1. MAPA DE ARCHIVOS CLAVE

### Dominio (`Domain/Entities/`)
```
ACC/CuentaContable.cs             Plan de cuentas — árbol N-nivel
ACC/AsientoContable.cs            Comprobante contable (cabecera)
ACC/AsientoContableLinea.cs       Líneas Debe/Haber del comprobante
ACC/PeriodoContable.cs            Control de períodos mensuales
ACC/PlantillaContable.cs          Plantilla para asientos automáticos
ACC/PlantillaContableLinea.cs     Líneas de plantilla (CampoMonto, Factor)
ACC/CierreContable.cs             Cierre de ejercicio fiscal
ACC/TipoComprobante.cs            Catálogo: CI, CE, CT (con prefijo)
ACC/TipoCambio.cs                 Tasas de cambio (USD, UFV, EUR)
ACC/TipoPago.cs                   Medios de pago (Cheque, QR, etc.)
CST/CentroCosto.cs                Centros de costo jerárquicos
DOC/ComprobanteDocumento.cs       Archivos adjuntos a un comprobante
Enums/TipoCuenta.cs               TipoCuenta + NaturalezaCuenta
```

### Aplicación (`Application/`)
```
Services/AsientoContableService.cs       33 KB — CRUD + contabilización manual
Services/CuentaContableService.cs        27 KB — Plan de cuentas + saldos
Services/EstadoFinancieroService.cs      15 KB — 6 reportes financieros
Services/ContabilizacionService.cs       11 KB — Motor automático
Services/PlantillaContableService.cs     16 KB — CRUD de plantillas
Services/PeriodoContableService.cs        7 KB — Apertura/cierre de períodos
Services/CierreContableService.cs              — Cierre de gestión fiscal
Services/CentroCostoService.cs            9 KB — Centros de costo
Services/ComprobanteDocumentoService.cs        — Documentos adjuntos

Interfaces/IAsientoContableService.cs
Interfaces/ICuentaContableService.cs
Interfaces/IEstadoFinancieroService.cs
Interfaces/IContabilizacionService.cs
Interfaces/IPlantillaContableService.cs
Interfaces/IPeriodoContableService.cs
Interfaces/ICierreContableService.cs
Interfaces/ICentroCostoService.cs
Interfaces/IComprobanteDocumentoService.cs
```

### API (`Api/Controllers/V1/`)
```
AsientosContablesController.cs      /api/v1/contabilidad/asientos
CuentasContablesController.cs       /api/v1/contabilidad/cuentas
EstadosFinancierosController.cs     /api/v1/contabilidad/estados-financieros
ContabilidadController.cs           /api/v1/contabilidad
PeriodosContablesController.cs      /api/v1/contabilidad/periodos
PlantillasContablesController.cs    /api/v1/contabilidad/plantillas
CentrosCostoController.cs           /api/v1/contabilidad/centros-costo
```

### DTOs (`Shared/DTOs/Contabilidad/`)
```
AsientoContableDto.cs
CuentaContableDto.cs
EstadosFinancierosDto.cs            Contiene: BalanceGeneralDto, EstadoResultadosDto,
                                    SumasYSaldosDto, LibroDiarioDto, LibroMayorDto,
                                    FlujoDEfectivoDto y sus líneas
PeriodoContableDto.cs
PlantillaContableDto.cs
CentroCostoDto.cs
CierreContableDto.cs
ComprobanteDocumentoDto.cs
```

### UI Blazor (`Web/Pages/Contabilidad/`)
```
PlanCuentas.razor                   Árbol de cuentas
AsientosContables.razor             74 KB — UI principal de comprobantes
AsientosContables.razor.cs          Code-behind
PlantillasContables.razor           Editor de plantillas
PeriodosContables.razor             Gestión y cierre de períodos
CentrosCosto.razor                  Centros de costo
BalanceGeneral.razor                Reporte Balance General
EstadoResultados.razor              Reporte Estado de Resultados
LibroDiario.razor                   Reporte Libro Diario
LibroMayor.razor                    Reporte Libro Mayor por cuenta
SumasYSaldos.razor                  Reporte Balance de Comprobación
FlujoDEfectivo.razor                Reporte Flujo de Efectivo
DashboardContable.razor             KPIs contables
```

### Persistencia (`Persistence/Configurations/ACC/`)
```
CuentaContableConfiguration.cs      acc.CuentasContables
AsientoContableConfiguration.cs     acc.AsientosContables
AsientoContableLineaConfiguration.cs
PeriodoContableConfiguration.cs
PlantillaContableConfiguration.cs
PlantillaContableLineaConfiguration.cs
CierreContableConfiguration.cs
TipoComprobanteConfiguration.cs
TipoCambioConfiguration.cs
TipoPagoConfiguration.cs
```

---

## 2. MODELOS DE DATOS — Referencia rápida

### CuentaContable
| Campo | Tipo | Notas |
|---|---|---|
| `CuentaContableId` | int | PK |
| `Codigo` | string(30) | Único por empresa. Ej: `1.1.3.01` |
| `Nombre` | string(200) | |
| `Tipo` | byte enum | Activo=1, Pasivo=2, Patrimonio=3, Ingreso=4, Gasto=5, Costo=6 |
| `Naturaleza` | byte enum | Deudora=1, Acreedora=2 |
| `Nivel` | int | 1=grupo … 4+=hoja |
| `CuentaPadreId` | int? | Auto-referencial |
| `PermiteMovimientos` | bool | Solo hojas aceptan asientos |
| `SaldoActual` | decimal | Calculado desde líneas de asiento |
| `ClasificacionFlujoEfectivo` | int enum | NoAplica=0, Operacional=1, Inversion=2, Financiacion=3 |
| `EmpresaId` | int | Multi-tenant |
| `Activo` | bool | Soft-delete |

### AsientoContable
| Campo | Tipo | Notas |
|---|---|---|
| `AsientoContableId` | long | PK |
| `TipoComprobanteId` | int? | FK |
| `Numero` | string(30) | Único por empresa. Ej: `CI-001` |
| `Fecha` | DateTime | Debe caer en período abierto |
| `Gestion` | int | Año fiscal |
| `TipoRegistro` | string | `Manual`, `Automático`, `Ajuste` |
| `Estado` | string | `Borrador` → `Contabilizado` → `Anulado` |
| `Glosa` | string(500) | Descripción general |
| `Concepto` | string(300) | Etiqueta/categoría |
| `TipoCambioId` | int? | FK |
| `ValorTipoCambio` | decimal? | Capturado al registrar |
| `TipoPagoId` | int? | FK |
| `NumeroDocumentoPago` | string(100)? | Nro cheque, ref QR |
| `RegistradoPorNombre` | string? | Inmutable |
| `OrigenTipo` | string(50)? | `Recepcion`, `Importacion`, `Venta` |
| `OrigenId` | long? | ID del doc origen |
| `OrigenReferencia` | string(100)? | Ej: `REC-000001` |
| `TotalDebe` | decimal | Calculado |
| `TotalHaber` | decimal | Calculado |
| `EmpresaId` | int | Multi-tenant |

### AsientoContableLinea
| Campo | Tipo | Notas |
|---|---|---|
| `AsientoContableLineaId` | long | PK |
| `AsientoContableId` | long | FK cascade |
| `NumeroLinea` | int | Orden |
| `CuentaContableId` | int | FK |
| `Debe` | decimal | |
| `Haber` | decimal | |
| `Glosa` | string? | Detalle de línea |
| `Referencia` | string? | Nro factura, proveedor, etc. |
| `CentroCostoId` | int? | Opcional |

### PlantillaContableLinea
| Campo | Tipo | Notas |
|---|---|---|
| `CampoMonto` | string | `Subtotal`, `Impuesto`, `Total`, `GastoAsignado`, `CostoTotal`, `Cantidad` |
| `TipoMovimiento` | string | `Debe` o `Haber` |
| `Factor` | decimal | Multiplicador. Ej: `0.13` para IVA 13% |
| `Glosa` | string? | Tokens: `{Numero}`, `{Proveedor}`, `{Fecha}`, `{CuentaCodigo}`, `{CuentaNombre}` |

### CierreContable
| Campo | Tipo | Notas |
|---|---|---|
| `Id` | Guid | PK |
| `Gestion` | int | Año fiscal |
| `Estado` | string | `BORRADOR`, `CERRADO`, `ANULADO` |
| `FechaCierre` | DateTime | |
| `Observaciones` | string | |

---

## 3. ENDPOINTS COMPLETOS — Referencia

```
/api/v1/contabilidad/asientos
  GET    /                        ?desde=&hasta=&estado=&tipoComprobanteId=&search=
  GET    /{id}
  POST   /
  PUT    /{id}                    Solo si Estado == Borrador
  DELETE /{id}                    Solo si Estado == Borrador
  POST   /{id}/contabilizar
  POST   /{id}/anular
  POST   /{id}/copiar
  GET    /exportar-excel          Mismos filtros que GET /
  GET    /tipos-comprobante
  GET    /tipos-cambio
  GET    /tipos-pago
  POST   /seed-catalogos

/api/v1/contabilidad/cuentas
  GET    /                        ?tipo=&permiteMovimientos=&search=
  GET    /tree                    Árbol jerárquico completo
  GET    /{id}
  POST   /
  PUT    /{id}                    No permite cambiar Codigo
  DELETE /{id}                    Falla si tiene subcuentas o asientos
  POST   /seed                    Plan estándar Bolivia/NIIF ~120 cuentas

/api/v1/contabilidad/estados-financieros
  GET    /balance-general         ?fechaCorte=
  GET    /balance-general/export  ?fechaCorte=&format=excel|pdf
  GET    /estado-resultados       ?desde=&hasta=
  GET    /estado-resultados/excel
  GET    /sumas-saldos            ?desde=&hasta=
  GET    /libro-diario            ?desde=&hasta=&estado=
  GET    /libro-mayor             ?cuentaId=&desde=&hasta=
  GET    /flujo-efectivo          ?desde=&hasta=

/api/v1/contabilidad/periodos
  GET    /                        ?anio=
  POST   /generar                 Body: { anio: int } → crea 12 períodos
  POST   /{id}/cerrar
  POST   /{id}/reabrir

/api/v1/contabilidad/plantillas
  GET    /                        ?tipoDocumento=
  GET    /{id}
  GET    /tipo/{tipoDocumento}    Obtiene plantilla activa por tipo
  POST   /
  PUT    /{id}                    Reemplaza líneas completas
  DELETE /{id}
  POST   /seed

/api/v1/contabilidad/centros-costo
  GET    /
  GET    /{id}
  POST   /
  PUT    /{id}
  DELETE /{id}                    Falla si tiene hijos activos

/api/v1/contabilidad
  POST   /cierre-contable         Body: EjecutarCierreDto
  GET    /cierre-contable/{gestion}
  POST   /cierre-contable/apertura/{gestionNueva}
```

---

## 4. REGLAS DE NEGOCIO CRÍTICAS

| Regla | Dónde se valida |
|---|---|
| `TotalDebe == TotalHaber` antes de contabilizar | `AsientoContableService.ContabilizarAsync()` |
| Período contable abierto para crear asientos | `AsientoContableService` + `PeriodoContableService` |
| `Estado: Borrador → Contabilizado → Anulado` (sin retroceso) | `AsientoContableService` |
| Código cuenta único por `(EmpresaId, Codigo)` | `CuentaContableConfiguration` (índice UNIQUE) |
| No eliminar cuenta con subcuentas o asientos | `CuentaContableService.DeleteAsync()` |
| No eliminar centro de costo con hijos activos | `CentroCostoService.DeleteAsync()` |
| Código plantilla único por empresa | `PlantillaContableConfiguration` |
| Ajuste automático de cuadratura ≤ 0.01 (redondeo) | `ContabilizacionService` |
| `RegistradoPorNombre` inmutable (captura snapshot) | `AsientoContableService.ContabilizarAsync()` |
| `CerradoPorNombre` inmutable | `PeriodoContableService.CerrarAsync()` |
| Global tenant filter: todas las consultas filtran por `EmpresaId` | `AgoraDbContext` |

---

## 5. INTEGRACIONES ENTRE MÓDULOS

```
ContabilizacionService  ←  RecepcionCompraService   tipoDocumento="Recepcion"
ContabilizacionService  ←  HojaImportacionService   tipoDocumento="Importacion"
ContabilizacionService  ←  [Ventas — pendiente]     tipoDocumento="Venta"

AsientoContableService  →  CuentaContable.SaldoActual   (actualiza al contabilizar/anular)
EstadoFinancieroService →  AsientoContable (solo Estado="Contabilizado")
CierreContableService   →  EstadoFinancieroService + AsientoContableService
```

**Tipos de documento soportados en plantillas:**
- `Recepcion` — recepciones de compra
- `Importacion` — hojas de importación
- `Venta` — ventas (motor listo, módulo pendiente)
- `CostoVenta` — costo de ventas
- `AjusteInventario` — ajustes de inventario

---

## 6. MIGRACIONES EF CORE (orden cronológico)

| Fecha | Migración | Cambio |
|---|---|---|
| 2026-02-26 | `ACC_CuentasContables` | Tabla plan de cuentas |
| 2026-02-26 | `ACC_AsientosContables` | Tabla asientos + líneas |
| 2026-02-26 | `ACC_PeriodosContables` | Tabla períodos |
| 2026-02-26 | `ACC_PlantillasContables` | Tabla plantillas + líneas |
| 2026-02-26 | `ACC_ComprobantesContables` | TipoComprobante, TipoCambio, TipoPago |
| 2026-03-11 | `ACC_AsientoContable_NuevosCampos` | Origen, TipoPago, campos extra |
| 2026-04-09 | `ACC_CuentaContable_ClasificacionFlujo` | Campo ClasificacionFlujoEfectivo |

---

## 7. ÁREAS DE MEJORA IDENTIFICADAS

### Funcionalidades pendientes de implementar

| # | Área | Descripción | Archivos a crear/modificar |
|---|---|---|---|
| 1 | **Presupuestos** | Módulo presupuesto vs. real por cuenta/período | Nueva entidad `PresupuestoContable`, nuevo servicio, nuevo endpoint |
| 2 | **Activos Fijos** | Depreciación automática periódica | Nueva entidad `ActivoFijo`, integración con `ContabilizacionService` |
| 3 | **Impuestos / Retenciones** | Cálculo automático IVA, IT, retenciones | Extensión de `PlantillaContableLinea.CampoMonto` |
| 4 | **Conciliación Bancaria** | Cruce de extractos con asientos | Nuevo submódulo |
| 5 | **Consolidación multi-empresa** | Consolidar estados financieros de varias empresas | Extensión de `EstadoFinancieroService` |
| 6 | **Carga masiva desde Excel** | Importar asientos desde plantilla Excel | Nuevo endpoint + validación partida doble |
| 7 | **Ratios/Indicadores financieros** | Liquidez, rentabilidad, endeudamiento | Extensión de `EstadoFinancieroService` + `DashboardContable.razor` |
| 8 | **Caché plan de cuentas** | El plan se consulta en cada asiento | Agregar `IMemoryCache` en `CuentaContableService` |
| 9 | **Exportación Libro Mayor PDF** | El Libro Mayor solo exporta Excel | Agregar format=pdf en `EstadosFinancierosController` |
| 10 | **Notificaciones de período** | Alerta cuando un período está próximo a cerrar | Hook en `PeriodoContableService` |

---

## 8. PLANTILLAS DE PROMPTS PARA MEJORAS

Usa estas plantillas como base para elaborar prompts de implementación:

### Prompt para nueva funcionalidad
```
En [NombreServicio].cs, implementar el método [NombreMetodo] que:
- Recibe: [parámetros con tipos]
- Valida: [reglas de negocio]
- Persiste: [entidad a crear/modificar]
- Retorna: [tipo de retorno DTO]
Seguir el patrón existente de [MetodoSimilar] en el mismo servicio.
Agregar la interfaz en [INombreServicio].cs y el endpoint en [Controller].cs.
```

### Prompt para nuevo reporte financiero
```
En EstadoFinancieroService.cs, agregar el método Get[NombreReporte]Async(empresaId, desde, hasta, ct) que:
- Consulte AsientoContable con Estado="Contabilizado" en el rango de fechas
- Agrupe por [criterio]
- Retorne [NombreReporteDto] con los campos [lista de campos]
Agregar el DTO en EstadosFinancierosDto.cs y el endpoint GET /api/v1/contabilidad/estados-financieros/[ruta] en EstadosFinancierosController.cs.
```

### Prompt para nueva entidad
```
Crear la entidad [Nombre].cs en Domain/Entities/[modulo]/ con los campos:
- [campo]: [tipo], [restricción]
Agregar la configuración EF en Persistence/Configurations/[modulo]/[Nombre]Configuration.cs 
con tabla "[schema].[Nombre]s" e índice único ([EmpresaId], [campo]).
Agregar DbSet en AgoraDbContext.cs y generar la migración [YYYYMMDD]_[Nombre].
```

### Prompt para mejora de rendimiento
```
En [NombreServicio].cs, el método [NombreMetodo] consulta [entidad] en cada llamada.
Agregar caché con IMemoryCache:
- Clave: "[prefix]-{empresaId}"
- TTL: [N] minutos
- Invalidar caché en Create/Update/Delete de [entidad]
Inyectar IMemoryCache en el constructor.
```

### Prompt para extensión de exportación
```
En EstadosFinancierosController.cs, endpoint GET /[ruta], agregar parámetro ?format=excel|pdf.
Para Excel: usar el patrón existente en balance-general/export con ClosedXML.
Para PDF: usar el patrón existente en [otro endpoint PDF].
Devolver FileContentResult con el ContentType correcto.
```

---

## 9. CONVENCIONES DEL PROYECTO

- **Arquitectura:** Clean Architecture — Domain → Application → Persistence/API/Web
- **Patrón de repositorio:** `IRepository<T>` + `IUnitOfWork`
- **Multi-tenant:** `ICurrentUserService` para obtener `EmpresaId` del JWT
- **Numeración:** `NumeracionDocumento` para secuenciales únicos por tipo
- **Soft-delete:** Campo `Activo = false`, nunca `DELETE` físico
- **Estado asiento:** Siempre a través de métodos del servicio, nunca directo a la BD
- **DTOs de lectura:** `[Entidad]Dto` — DTOs de escritura: `Create[Entidad]Dto` / `Update[Entidad]Dto`
- **Tests:** Carpeta `tests/` con unit tests por servicio
- **Esquema BD contabilidad:** `acc.*`
- **Esquema BD centros de costo:** `cst.*`
- **Esquema BD documentos:** `doc.*`

---

*Documento generado a partir de análisis estático del código fuente. Actualizar al implementar nuevas funcionalidades.*
