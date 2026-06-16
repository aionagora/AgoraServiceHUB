# CxC Module - Status Report

## Build & Test Results
- **Full solution build**: ✅ 0 errors, 0 warnings across 8 projects
- **Tests executed**: ✅ 125/125 passed in 1.27s

## Architecture Applied

### Clean Architecture Layers

**Domain** (`AgoraHub360.ERP.Domain`)
- `CuentaPorCobrar` entity in `Entities/CXC/` extending `TenantEntity`
- `EstadoCuentaPorCobrar` enum: `Pendiente, Parcial, Pagada, Vencida, Anulada`
- Denormalized client info for query performance
- `SaldoPendiente` as computed column (`TotalFactura - TotalPagado`)

**Application** (`AgoraHub360.ERP.Application`)
- `ICuentasPorCobrarService` interface + `CuentasPorCobrarService` implementation
- `Result<T>` pattern for all operations (no exceptions for expected failures)
- Business rules enforced in service layer:
  - No duplicate CxC per FacturaVentaId (unique index)
  - Estado recalculado dinámicamente (Vencida si fecha pasada)
  - No permitir operaciones sobre CxC anulada
  - Auto-creación de CxC si no existe al registrar pago

**Persistence** (`AgoraHub360.ERP.Persistence`)
- `CuentaPorCobrarConfiguration` schema: `cxc.CuentasPorCobrar`
- Computed column `SaldoPendiente` (stored)
- Global tenant filter via `EmpresaId`
- Indexes: `(EmpresaId, FacturaVentaId)` unique, `(EmpresaId, Estado)`, etc.

**API** (`AgoraHub360.ERP.Api`)
- Route: `api/v1/cuentas-por-cobrar`
- Endpoints: GetAll, GetById, GenerarDesdeFactura, ActualizarPorPago, Anular, GetSaldoPendiente, GetAntiguedadSaldos
- JWT auth + tenant isolation

**Web** (`AgoraHub360.ERP.Web`)
- `Pages/Ventas/CuentasPorCobrar.razor` — full CRUD + filters + pagination
- Summary cards (saldo total, pendientes, vencidas, pagadas)
- Detail modal with pagos asociados
- Confirmación de anulación
- Recalcular saldo button
- `CuentasPorCobrarHttpService` for API communication

**Shared** (`AgoraHub360.ERP.Shared`)
- DTOs: `CuentaPorCobrarResumenDto`, `CuentaPorCobrarDetalleDto`, `CuentaPorCobrarFilterDto`, `PagoAplicadoDto`, `AntiguedadSaldos*Dto`

**Tests** (`AgoraHub360.ERP.Tests`)
- `CuentasPorCobrarServiceTests`: GetAll, GetById, GenerarDesdeFactura, ActualizarPorPago, ActualizarPorPagoVenta, Anular, GetSaldoTotalPendiente, GetAntiguedadSaldos
- Cross-tenant isolation tested

## Key Design Decisions
1. **No PagoCuentaPorCobrar entity**: Payments reuse `VentaPago` (existing entity) — avoids duplication
2. **No MetodoPagoCuentaPorCobrar enum**: Uses existing `TipoPago`/`ModoPago` from Ventas module
3. **Dynamic estado**: Calculated from actual data (fecha vencimiento vs today), not stored redundantly
4. **Computed SaldoPendiente**: Database-managed computed column ensures consistency
5. **Integration via VentaId**: CxC reads pagos from FacturaVenta → Venta → VentaPagos chain

## Files Created
- Domain: `CuentaPorCobrar.cs`, `EstadoCuentaPorCobrar.cs`
- Persistence: `CuentaPorCobrarConfiguration.cs`
- Application: `ICuentasPorCobrarService.cs`, `CuentasPorCobrarService.cs`
- API: `CuentasPorCobrarController.cs`
- Shared: `CuentaPorCobrarDtos.cs`
- Web: `CuentasPorCobrar.razor`, `CuentasPorCobrarHttpService.cs`
- Tests: `CuentasPorCobrarServiceTests.cs`
