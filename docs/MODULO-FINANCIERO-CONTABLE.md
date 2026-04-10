# Módulo Financiero y Contable — Estado Actual

**Proyecto:** AgoraHUB360-ERP  
**Fecha del documento:** 2026-04-09  
**Stack:** .NET 8 · ASP.NET Core · Entity Framework Core · Blazor WebAssembly · SQL Server  
**Arquitectura:** Clean Architecture con DDD (Domain, Application, Persistence, API, Web)

---

## Resumen Ejecutivo

El módulo financiero-contable está **completamente implementado** en todas las capas de la arquitectura. Cubre contabilidad por partida doble, plan de cuentas jerárquico, períodos contables, plantillas de asientos automáticos, centros de costo y los cuatro reportes financieros estándar. Está integrado con el módulo de compras (órdenes, recepciones e importaciones) mediante un motor de contabilización automática.

---

## 1. Alcance Funcional

### 1.1 Contabilidad General (ACC)

| Funcionalidad | Estado | Descripción |
|---|---|---|
| Plan de Cuentas | ✅ Completo | Estructura jerárquica N-niveles, código estructurado (ej. `1.1.3.01`) |
| Tipos de cuenta | ✅ Completo | Activo, Pasivo, Patrimonio, Ingreso, Gasto, Costo |
| Naturaleza de cuenta | ✅ Completo | Deudora / Acreedora con cálculo de saldo automático |
| Asientos Contables | ✅ Completo | Partida doble validada, estados: Borrador → Contabilizado → Anulado |
| Tipos de comprobante | ✅ Completo | Ingreso (CI), Egreso (CE), Traspaso (TRA) con prefijo y numeración |
| Períodos Contables | ✅ Completo | Mensual, estados Abierto / Cerrado, firma histórica de cierre |
| Plantillas Contables | ✅ Completo | Generación automática de asientos desde plantilla parametrizable |
| Motor de contabilización | ✅ Completo | `ContabilizacionService`: genera y contabiliza asientos en un paso |
| Tipos de cambio | ✅ Completo | Registro de tasas (Dólar, UFV), valor capturado al registrar |
| Tipos de pago | ✅ Completo | Cheque, Efectivo, QR, S/D con número de documento |
| Firma histórica | ✅ Completo | `RegistradoPorNombre` capturado al registrar (inmutable) |
| Trazabilidad de origen | ✅ Completo | `OrigenTipo`, `OrigenId`, `OrigenReferencia` para asientos automáticos |

### 1.2 Reportes Financieros

| Reporte | Estado | Lógica |
|---|---|---|
| Balance General | ✅ Completo | Activos = Pasivos + Patrimonio, valida cuadratura, árbol jerárquico |
| Estado de Resultados | ✅ Completo | Ingresos − Costos = Utilidad Bruta; − Gastos = Utilidad Neta |
| Libro Diario | ✅ Completo | Lista cronológica de asientos con filtro por estado |
| Sumas y Saldos | ✅ Completo | Saldo Deudor / Acreedor por cuenta, totales globales |

### 1.3 Centros de Costo (CST)

| Funcionalidad | Estado |
|---|---|
| CRUD de centros de costo | ✅ Completo |
| Estructura jerárquica | ✅ Completo |
| Reglas de costeo (`CostingRule`) | ✅ Completo |
| Perfiles de costo aterrizado (`LandedCostProfile`) | ✅ Completo |

### 1.4 Integración con Compras (CMP)

| Funcionalidad | Estado | Descripción |
|---|---|---|
| Órdenes de Compra | ✅ Completo | Ciclo completo: creación → aprobación → recepción → pago |
| Recepciones de Compra | ✅ Completo | Recepción parcial, valida contra OC, dispara asientos automáticos |
| Expedientes de Importación | ✅ Completo | Seguimiento completo: OC → recepción → hoja importación → costo aterrizado |
| Hojas de Importación | ✅ Completo | Prorrateo de gastos a inventario, contabilización automática |
| Pago de OC | ✅ Completo | `PagoOrdenCompra` con tipo de pago y número de documento |
| Confirmación de proveedor | ✅ Completo | `ConfirmacionProveedor` para tracking de documentos comerciales |

---

## 2. Arquitectura por Capas

### 2.1 Dominio (`Domain/Entities/ACC`)

```
AsientoContable.cs              — Cabecera del comprobante contable
AsientoContableLinea.cs         — Líneas Debe/Haber del asiento
CuentaContable.cs               — Nodo del plan de cuentas (árbol N-nivel)
PeriodoContable.cs              — Control de períodos contables mensuales
PlantillaContable.cs            — Plantilla para asientos automáticos
PlantillaContableLinea.cs       — Líneas de la plantilla (Campo → Debe/Haber)
TipoCambio.cs                   — Tasa de cambio de divisas
TipoComprobante.cs              — Tipo de voucher con prefijo y secuencia
TipoPago.cs                     — Medio de pago (Cheque, Efectivo, QR, S/D)
```

Entidades base relevantes:
- `TenantEntity` → provee `EmpresaId`, `Activo`, auditoría automática
- `AuditableEntity` → `CreadoEn`, `ModificadoEn`, `CreadoPor`, `ModificadoPor`

### 2.2 Aplicación (`Application/Services`)

| Servicio | Tamaño | Responsabilidad |
|---|---|---|
| `AsientoContableService` | 33 KB | CRUD de asientos, validación de cuadratura, contabilización manual |
| `CuentaContableService` | 27 KB | Gestión del plan de cuentas, jerarquía, saldos |
| `PlantillaContableService` | 16 KB | CRUD de plantillas, resolución de campos de monto |
| `EstadoFinancieroService` | 15 KB | Los 4 reportes financieros con lógica de árbol jerárquico |
| `ContabilizacionService` | 11 KB | Motor de contabilización automática desde documentos fuente |
| `CentroCostoService` | 8.9 KB | Centros de costo y asignación |
| `PeriodoContableService` | 6.7 KB | Apertura, cierre y validación de períodos |

### 2.3 API (`Api/Controllers/V1`)

```
GET/POST/PUT/DELETE  /api/v1/asientos-contables
GET/POST/PUT/DELETE  /api/v1/cuentas-contables
GET/POST/PUT/DELETE  /api/v1/periodos-contables
GET/POST/PUT/DELETE  /api/v1/plantillas-contables
GET/POST/PUT/DELETE  /api/v1/centros-costo

GET  /api/v1/estados-financieros/balance-general?fechaCorte=
GET  /api/v1/estados-financieros/estado-resultados?desde=&hasta=
GET  /api/v1/estados-financieros/libro-diario?desde=&hasta=&estado=
GET  /api/v1/estados-financieros/sumas-saldos?desde=&hasta=
```

### 2.4 Persistencia (`Persistence/Configurations/ACC`)

Tablas principales en SQL Server:

| Tabla | Descripción |
|---|---|
| `ACC_AsientosContables` | Cabecera de comprobantes, multi-tenant (`EmpresaId`) |
| `ACC_AsientosContablesLineas` | Líneas Debe/Haber con FK a cuenta |
| `ACC_CuentasContables` | Plan de cuentas con auto-referencia para jerarquía |
| `ACC_PeriodosContables` | Períodos mensuales con estado y firma de cierre |
| `ACC_PlantillasContables` | Plantillas de asientos |
| `ACC_PlantillasContablesLineas` | Líneas de plantilla con `CampoMonto` y `Factor` |
| `ACC_TiposCambio` | Tasas de cambio de divisas |
| `ACC_TiposComprobante` | Configuración de tipos de comprobante |
| `ACC_TiposPago` | Medios de pago |
| `CST_CentrosCosto` | Centros de costo |

### 2.5 Interfaz de Usuario (`Web/Pages/Contabilidad`)

| Componente Blazor | Tamaño | Funcionalidad |
|---|---|---|
| `AsientosContables.razor` | 74 KB | UI principal de asientos contables (la más completa) |
| `PlanCuentas.razor` | 24 KB | Gestión del árbol de cuentas |
| `PlantillasContables.razor` | 19 KB | Editor de plantillas de asientos |
| `CentrosCosto.razor` | 16 KB | Gestión de centros de costo |
| `LibroDiario.razor` | 11 KB | Visualización del libro diario con filtros |
| `SumasYSaldos.razor` | 9 KB | Reporte de sumas y saldos |
| `BalanceGeneral.razor` | 8.1 KB | Balance general con árbol jerárquico |
| `EstadoResultados.razor` | 8.3 KB | Estado de resultados por período |
| `PeriodosContables.razor` | 9 KB | Gestión y cierre de períodos |

---

## 3. Flujos Clave

### 3.1 Registro Manual de Asiento

```
Usuario → AsientosContables.razor
    → POST /api/v1/asientos-contables
    → AsientoContableService.CreateAsync()
        → Valida período abierto
        → Valida cuadratura (TotalDebe == TotalHaber)
        → Genera número secuencial por tipo+gestión
        → Estado inicial: "Borrador"
    → POST /api/v1/asientos-contables/{id}/post
    → Estado: "Contabilizado"
    → Actualiza SaldoActual en cada CuentaContable
```

### 3.2 Contabilización Automática desde Compras

```
RecepcionCompra / HojaImportacion confirmada
    → ContabilizacionService.ContabilizarDocumentoAsync(tipoDocumento, montos, ...)
        → Busca PlantillaContable por TipoDocumento
        → Valida período no cerrado
        → Resuelve montos por CampoMonto (ej: "Total", "Costo", "IGV")
        → Aplica Factor por línea
        → Valida cuadratura (ajuste automático ±0.01 por redondeo)
        → Construye glosa desde plantilla ({Numero}, {Proveedor}, {Fecha})
        → Tipo de comprobante: TRA (Traspaso)
        → Crea AsientoContable con TipoRegistro="Automático", Estado="Contabilizado"
        → Actualiza SaldoActual de cuentas afectadas
```

### 3.3 Generación de Reportes Financieros

```
EstadoFinancieroService
    → CalcularSaldosAlAsync(fechaCorte)       ← solo asientos "Contabilizado"
    → BuildTree(cuentas, saldos, TipoCuenta)  ← árbol jerárquico por tipo
    → SumarSaldoTree(grupos)                  ← acumulación recursiva
    → BalanceGeneralDto { Cuadrado = Activos == Pasivos + Patrimonio }
```

---

## 4. Capacidades Transversales

| Capacidad | Implementación |
|---|---|
| **Multi-tenancy** | `EmpresaId` en todas las tablas, filtrado automático por tenant |
| **Multi-divisa** | `TipoCambio` con valor capturado al registrar (`ValorTipoCambio`) |
| **Auditoría** | `CreadoEn`, `ModificadoEn`, `CreadoPor`, `ModificadoPor` automáticos |
| **Firma histórica** | `RegistradoPorNombre` / `CerradoPorNombre` inmutables en impresiones |
| **Trazabilidad** | `OrigenTipo` + `OrigenId` + `OrigenReferencia` en asientos automáticos |
| **Período seguro** | Bloqueo de contabilización en períodos cerrados |
| **Cuadratura** | Validación estricta Debe == Haber; ajuste automático ≤ 0.01 por redondeo |
| **Soft delete** | Columna `Activo` en todas las entidades, sin borrado físico |
| **Versioning API** | Prefijo `/api/v1/` en todos los endpoints |
| **JWT Auth** | Todos los endpoints requieren autenticación |

---

## 5. DTOs Principales

### `AsientoContableDto`
```
AsientoContableId, EmpresaId, Numero, Fecha, Gestion
TipoRegistro (Manual / Automático / Ajuste)
Estado (Borrador / Contabilizado / Anulado)
Glosa, Concepto
TipoComprobanteId/Codigo/Nombre
TipoCambioId, ValorTipoCambio
TipoPagoId, NumeroDocumentoPago
RegistradoPorId, RegistradoPorNombre
OrigenTipo, OrigenId, OrigenReferencia
TotalDebe, TotalHaber, Cuadrado
Lineas: List<AsientoContableLineaDto>
```

### `EstadosFinancierosDto` (reportes)
```
BalanceGeneralDto    → Empresa, FechaCorte, Activos/Pasivos/Patrimonio (árbol), Cuadrado
EstadoResultadosDto  → Empresa, FechaDesde/Hasta, Ingresos/Costos/Gastos, UtilidadBruta/Neta
LibroDiarioDto       → Empresa, FechaDesde/Hasta, Entradas[], TotalDebe, TotalHaber
SumasYSaldosDto      → Empresa, FechaDesde/Hasta, Lineas[], Totales
```

---

## 6. Estado de Implementación por Componente

| Componente | Capa | Estado | Notas |
|---|---|---|---|
| Entidades ACC (9 entidades) | Domain | ✅ | Completas con navegación y documentación XML |
| Enums (`TipoCuenta`, `NaturalezaCuenta`) | Domain | ✅ | Completos |
| Interfaces de servicios ACC | Application | ✅ | Contratos definidos |
| `AsientoContableService` | Application | ✅ | CRUD + validación + contabilización |
| `CuentaContableService` | Application | ✅ | Jerarquía + saldos |
| `PeriodoContableService` | Application | ✅ | Apertura/cierre de períodos |
| `PlantillaContableService` | Application | ✅ | CRUD de plantillas |
| `ContabilizacionService` | Application | ✅ | Motor automático completo |
| `EstadoFinancieroService` | Application | ✅ | 4 reportes implementados |
| `CentroCostoService` | Application | ✅ | CRUD de centros |
| Configuraciones EF (ACC) | Persistence | ✅ | Fluent API por entidad |
| Migraciones DB | Persistence | ✅ | Snapshot actualizado |
| Controllers ACC (5) | API | ✅ | CRUD + endpoints especializados |
| `EstadosFinancierosController` | API | ✅ | 4 endpoints de reportes |
| HTTP Services (Web) | Web | ✅ | Clientes API para todos los servicios |
| Páginas Blazor Contabilidad | Web | ✅ | 9 páginas funcionales |
| Páginas Blazor Compras | Web | ✅ | 5 páginas funcionales |
| DTOs Contabilidad | Shared | ✅ | Completos |
| DTOs Compras | Shared | ✅ | Completos |

---

## 7. Integraciones con Otros Módulos

```
Contabilidad (ACC)
    ← Compras (CMP):     RecepcionCompra, HojaImportacion  →  asiento automático
    ← MDM:               Empresa, Usuario                  →  multi-tenant, auditoría
    ← Core:              NumeracionDocumento                →  numeración secuencial
    ← DOC:               ComprobanteDocumento               →  enlace asiento ↔ documento
    ← CST:               CentroCosto                        →  asignación de costos
```

---

## 8. Observaciones y Próximos Pasos Sugeridos

1. **Contabilización de ventas:** El motor `ContabilizacionService` está listo; falta conectar el módulo de ventas/facturación cuando sea implementado.
2. **Asientos de apertura de período:** Se puede agregar generación automática de asiento de apertura al abrir un nuevo período.
3. **Conciliación bancaria:** No implementada. Requeriría un submódulo de extractos bancarios.
4. **Exportación de reportes:** Las páginas Blazor de reportes están implementadas visualmente; se puede agregar exportación a PDF/Excel.
5. **Partidas presupuestarias:** No existe módulo de presupuesto (budget). Es una extensión natural sobre el plan de cuentas existente.
6. **Retenciones e impuestos:** `TipoPago` cubre los medios de pago, pero no hay lógica de cálculo automático de retenciones/impuestos (IVA, IT, etc.).

---

*Documento generado automáticamente mediante análisis estático del código fuente. Última revisión: 2026-04-09.*
