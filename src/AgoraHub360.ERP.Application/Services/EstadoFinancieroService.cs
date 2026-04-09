namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class EstadoFinancieroService : IEstadoFinancieroService
{
    private readonly IRepository<CuentaContable> _cuentaRepo;
    private readonly IRepository<AsientoContable> _asientoRepo;
    private readonly IRepository<AsientoContableLinea> _lineaRepo;
    private readonly IRepository<Empresa> _empresaRepo;
    private readonly ICurrentUserService _currentUser;

    public EstadoFinancieroService(
        IRepository<CuentaContable> cuentaRepo,
        IRepository<AsientoContable> asientoRepo,
        IRepository<AsientoContableLinea> lineaRepo,
        IRepository<Empresa> empresaRepo,
        ICurrentUserService currentUser)
    {
        _cuentaRepo = cuentaRepo;
        _asientoRepo = asientoRepo;
        _lineaRepo = lineaRepo;
        _empresaRepo = empresaRepo;
        _currentUser = currentUser;
    }

    // ????????????????????????????????????????????
    // BALANCE GENERAL
    // ????????????????????????????????????????????
    public async Task<Result<BalanceGeneralDto>> GetBalanceGeneralAsync(DateTime fechaCorte, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<BalanceGeneralDto>.Failure("No active company.");

        var empresa = await _empresaRepo.GetByIdAsync(empresaId.Value, ct);
        var cuentas = await _cuentaRepo.FindAsync(c => c.EmpresaId == empresaId.Value && c.Activo, ct);
        var saldos = await CalcularSaldosAlAsync(empresaId.Value, fechaCorte, ct);

        var activos = BuildTree(cuentas, saldos, TipoCuenta.Activo);
        var pasivos = BuildTree(cuentas, saldos, TipoCuenta.Pasivo);
        var patrimonio = BuildTree(cuentas, saldos, TipoCuenta.Patrimonio);

        var totalActivos = SumarSaldoTree(activos);
        var totalPasivos = SumarSaldoTree(pasivos);
        var totalPatrimonio = SumarSaldoTree(patrimonio);

        return Result<BalanceGeneralDto>.Success(new BalanceGeneralDto
        {
            Empresa = empresa?.Nombre ?? "",
            FechaCorte = fechaCorte,
            Gestion = fechaCorte.Year,
            Activos = activos,
            Pasivos = pasivos,
            Patrimonio = patrimonio,
            TotalActivos = totalActivos,
            TotalPasivos = totalPasivos,
            TotalPatrimonio = totalPatrimonio,
            TotalPasivoPatrimonio = totalPasivos + totalPatrimonio,
            Cuadrado = totalActivos == totalPasivos + totalPatrimonio
        });
    }

    // ????????????????????????????????????????????
    // ESTADO DE RESULTADOS
    // ????????????????????????????????????????????
    public async Task<Result<EstadoResultadosDto>> GetEstadoResultadosAsync(DateTime desde, DateTime hasta, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<EstadoResultadosDto>.Failure("No active company.");

        var empresa = await _empresaRepo.GetByIdAsync(empresaId.Value, ct);
        var cuentas = await _cuentaRepo.FindAsync(c => c.EmpresaId == empresaId.Value && c.Activo, ct);
        var saldos = await CalcularSaldosPeriodoAsync(empresaId.Value, desde, hasta, ct);

        var ingresos = BuildTree(cuentas, saldos, TipoCuenta.Ingreso);
        var costos = BuildTree(cuentas, saldos, TipoCuenta.Costo);
        var gastos = BuildTree(cuentas, saldos, TipoCuenta.Gasto);

        var totalIngresos = SumarSaldoTree(ingresos);
        var totalCostos = SumarSaldoTree(costos);
        var totalGastos = SumarSaldoTree(gastos);

        return Result<EstadoResultadosDto>.Success(new EstadoResultadosDto
        {
            Empresa = empresa?.Nombre ?? "",
            FechaDesde = desde,
            FechaHasta = hasta,
            Gestion = hasta.Year,
            Ingresos = ingresos,
            Costos = costos,
            Gastos = gastos,
            TotalIngresos = totalIngresos,
            TotalCostos = totalCostos,
            UtilidadBruta = totalIngresos - totalCostos,
            TotalGastos = totalGastos,
            UtilidadNeta = totalIngresos - totalCostos - totalGastos
        });
    }

    // ????????????????????????????????????????????
    // SUMAS Y SALDOS
    // ????????????????????????????????????????????
    public async Task<Result<SumasYSaldosDto>> GetSumasYSaldosAsync(DateTime desde, DateTime hasta, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<SumasYSaldosDto>.Failure("No active company.");

        var empresa = await _empresaRepo.GetByIdAsync(empresaId.Value, ct);
        var cuentas = await _cuentaRepo.FindAsync(c => c.EmpresaId == empresaId.Value && c.Activo, ct);

        // Get all contabilized lines in the period
        var asientos = await _asientoRepo.FindAsync(
            a => a.EmpresaId == empresaId.Value && a.Activo
                && a.Estado == "Contabilizado"
                && a.Fecha >= desde && a.Fecha <= hasta.AddDays(1).AddSeconds(-1), ct);

        var asientoIds = asientos.Select(a => a.AsientoContableId).ToHashSet();
        var lineas = await _lineaRepo.FindAsync(
            l => asientoIds.Contains(l.AsientoContableId) && l.Activo, ct);

        var grouped = lineas.GroupBy(l => l.CuentaContableId)
            .ToDictionary(g => g.Key, g => (Debe: g.Sum(l => l.Debe), Haber: g.Sum(l => l.Haber)));

        var result = new List<SumasYSaldosLineaDto>();
        foreach (var cuenta in cuentas.OrderBy(c => c.Codigo))
        {
            grouped.TryGetValue(cuenta.CuentaContableId, out var totals);
            var debe = totals.Debe;
            var haber = totals.Haber;
            if (debe == 0 && haber == 0 && !cuenta.PermiteMovimientos) continue;
            if (debe == 0 && haber == 0) continue;

            var diff = debe - haber;
            result.Add(new SumasYSaldosLineaDto
            {
                CuentaContableId = cuenta.CuentaContableId,
                Codigo = cuenta.Codigo,
                Nombre = cuenta.Nombre,
                Tipo = cuenta.Tipo.ToString(),
                Nivel = cuenta.Nivel,
                SumaDebe = debe,
                SumaHaber = haber,
                SaldoDeudor = diff > 0 ? diff : 0,
                SaldoAcreedor = diff < 0 ? Math.Abs(diff) : 0
            });
        }

        return Result<SumasYSaldosDto>.Success(new SumasYSaldosDto
        {
            Empresa = empresa?.Nombre ?? "",
            FechaDesde = desde,
            FechaHasta = hasta,
            Gestion = hasta.Year,
            Lineas = result,
            TotalSumaDebe = result.Sum(l => l.SumaDebe),
            TotalSumaHaber = result.Sum(l => l.SumaHaber),
            TotalSaldoDeudor = result.Sum(l => l.SaldoDeudor),
            TotalSaldoAcreedor = result.Sum(l => l.SaldoAcreedor)
        });
    }

    // ????????????????????????????????????????????
    // LIBRO DIARIO
    // ????????????????????????????????????????????
    public async Task<Result<LibroDiarioDto>> GetLibroDiarioAsync(DateTime desde, DateTime hasta, string? estado, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<LibroDiarioDto>.Failure("No active company.");

        var empresa = await _empresaRepo.GetByIdAsync(empresaId.Value, ct);

        var asientos = await _asientoRepo.FindAsync(
            a => a.EmpresaId == empresaId.Value && a.Activo
                && a.Fecha >= desde && a.Fecha <= hasta.AddDays(1).AddSeconds(-1)
                && (string.IsNullOrEmpty(estado) || a.Estado == estado), ct);

        var asientoIds = asientos.Select(a => a.AsientoContableId).ToHashSet();
        var lineas = asientoIds.Count > 0
            ? await _lineaRepo.FindAsync(l => asientoIds.Contains(l.AsientoContableId) && l.Activo, ct)
            : new List<AsientoContableLinea>();

        // Load accounts
        var cuentaIds = lineas.Select(l => l.CuentaContableId).Distinct().ToList();
        var cuentas = cuentaIds.Count > 0
            ? await _cuentaRepo.FindAsync(c => cuentaIds.Contains(c.CuentaContableId), ct)
            : new List<CuentaContable>();
        var cuentaMap = cuentas.ToDictionary(c => c.CuentaContableId);

        // Load tipo comprobante names from asiento
        var tipoCompIds = asientos.Where(a => a.TipoComprobanteId.HasValue)
            .Select(a => a.TipoComprobanteId!.Value).Distinct().ToHashSet();

        var entradas = new List<LibroDiarioEntradaDto>();
        foreach (var a in asientos.OrderBy(x => x.Fecha).ThenBy(x => x.Numero))
        {
            var asientoLineas = lineas
                .Where(l => l.AsientoContableId == a.AsientoContableId)
                .OrderBy(l => l.NumeroLinea)
                .Select(l =>
                {
                    cuentaMap.TryGetValue(l.CuentaContableId, out var cuenta);
                    return new LibroDiarioLineaDto
                    {
                        CuentaCodigo = cuenta?.Codigo ?? "—",
                        CuentaNombre = cuenta?.Nombre ?? "—",
                        Glosa = l.Glosa,
                        Debe = l.Debe,
                        Haber = l.Haber
                    };
                }).ToList();

            entradas.Add(new LibroDiarioEntradaDto
            {
                AsientoContableId = a.AsientoContableId,
                TipoComprobante = a.Numero.Split('-').FirstOrDefault() ?? "—",
                Numero = a.Numero,
                Fecha = a.Fecha,
                Concepto = a.Concepto,
                Glosa = a.Glosa,
                Estado = a.Estado,
                Lineas = asientoLineas,
                TotalDebe = a.TotalDebe,
                TotalHaber = a.TotalHaber
            });
        }

        return Result<LibroDiarioDto>.Success(new LibroDiarioDto
        {
            Empresa = empresa?.Nombre ?? "",
            FechaDesde = desde,
            FechaHasta = hasta,
            Gestion = hasta.Year,
            Entradas = entradas,
            TotalDebe = entradas.Sum(e => e.TotalDebe),
            TotalHaber = entradas.Sum(e => e.TotalHaber)
        });
    }

    // ????????????????????????????????????????????
    // FLUJO DE EFECTIVO
    // ????????????????????????????????????????????
    public async Task<FlujoDEfectivoDto> GetFlujoDEfectivoAsync(int empresaId, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        // 7. Calcular Saldo Inicial Efectivo
        var asientosCaja = await _asientoRepo.FindAsync(
            a => a.EmpresaId == empresaId && a.Activo 
                && a.Estado == "Contabilizado" 
                && a.Fecha < desde, ct);

        var asientosCajaIds = asientosCaja.Select(a => a.AsientoContableId).ToHashSet();
        decimal saldoInicialEfectivo = 0;

        if (asientosCajaIds.Count > 0)
        {
            var lineasCaja = await _lineaRepo.FindAsync(l => asientosCajaIds.Contains(l.AsientoContableId) && l.Activo, ct);
            var cuentasCajaIds = lineasCaja.Select(l => l.CuentaContableId).Distinct().ToList();
            var cuentasCaja = cuentasCajaIds.Count > 0 
                ? await _cuentaRepo.FindAsync(c => cuentasCajaIds.Contains(c.CuentaContableId) && c.Codigo.StartsWith("1.1.01"), ct) 
                : new List<CuentaContable>();

            var cuentasCajaMap = cuentasCaja.ToDictionary(c => c.CuentaContableId);
            foreach (var l in lineasCaja)
            {
                if (cuentasCajaMap.TryGetValue(l.CuentaContableId, out var c))
                {
                    var neto = l.Debe - l.Haber;
                    if (c.Naturaleza == NaturalezaCuenta.Acreedora)
                        neto = -neto;
                    saldoInicialEfectivo += neto;
                }
            }
        }

        // 1 & 2. Movimientos del periodo
        var asientosPeriodo = await _asientoRepo.FindAsync(
            a => a.EmpresaId == empresaId && a.Activo
                && a.Estado == "Contabilizado"
                && a.Fecha >= desde && a.Fecha <= hasta.AddDays(1).AddSeconds(-1), ct);

        var asientoPeriodoIds = asientosPeriodo.Select(a => a.AsientoContableId).ToHashSet();
        var lineasPeriodo = asientoPeriodoIds.Count > 0
            ? await _lineaRepo.FindAsync(l => asientoPeriodoIds.Contains(l.AsientoContableId) && l.Activo, ct)
            : new List<AsientoContableLinea>();

        var cuentasIds = lineasPeriodo.Select(l => l.CuentaContableId).Distinct().ToList();
        var cuentas = cuentasIds.Count > 0 
            ? await _cuentaRepo.FindAsync(c => cuentasIds.Contains(c.CuentaContableId), ct) 
            : new List<CuentaContable>();

        var cuentasMap = cuentas.ToDictionary(c => c.CuentaContableId);

        var lineasOperacional = new List<FlujoDEfectivoLineaDto>();
        var lineasInversion = new List<FlujoDEfectivoLineaDto>();
        var lineasFinanciacion = new List<FlujoDEfectivoLineaDto>();

        // 4, 5, 6
        var groupedLineas = lineasPeriodo.GroupBy(l => l.CuentaContableId);
        foreach (var group in groupedLineas)
        {
            if (!cuentasMap.TryGetValue(group.Key, out var cuenta)) continue;

            // 3. Excluir NoAplica
            if (cuenta.ClasificacionFlujo == ClasificacionFlujoEfectivo.NoAplica) continue;

            var neto = group.Sum(l => l.Debe) - group.Sum(l => l.Haber);
            if (cuenta.Naturaleza == NaturalezaCuenta.Acreedora)
                neto = -neto;

            if (neto == 0) continue;

            var lineaDto = new FlujoDEfectivoLineaDto
            {
                CodigoCuenta = cuenta.Codigo,
                NombreCuenta = cuenta.Nombre,
                Monto = neto
            };

            switch (cuenta.ClasificacionFlujo)
            {
                case ClasificacionFlujoEfectivo.Operacional:
                    lineasOperacional.Add(lineaDto);
                    break;
                case ClasificacionFlujoEfectivo.Inversion:
                    lineasInversion.Add(lineaDto);
                    break;
                case ClasificacionFlujoEfectivo.Financiacion:
                    lineasFinanciacion.Add(lineaDto);
                    break;
            }
        }

        var totalOp = lineasOperacional.Sum(l => l.Monto);
        var totalInv = lineasInversion.Sum(l => l.Monto);
        var totalFin = lineasFinanciacion.Sum(l => l.Monto);
        var variacion = totalOp + totalInv + totalFin;

        // 8. Construir DTO
        return new FlujoDEfectivoDto
        {
            EmpresaId = empresaId,
            Desde = desde,
            Hasta = hasta,
            LineasOperacional = lineasOperacional,
            LineasInversion = lineasInversion,
            LineasFinanciacion = lineasFinanciacion,
            TotalOperacional = totalOp,
            TotalInversion = totalInv,
            TotalFinanciacion = totalFin,
            VariacionNetaEfectivo = variacion,
            SaldoInicialEfectivo = saldoInicialEfectivo,
            SaldoFinalEfectivo = saldoInicialEfectivo + variacion
        };
    }

    // ????????????????????????????????????????????
    // LIBRO MAYOR
    // ????????????????????????????????????????????
    public async Task<LibroMayorDto> GetLibroMayorAsync(int empresaId, int cuentaContableId, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var cuenta = await _cuentaRepo.GetByIdAsync(cuentaContableId, ct);
        if (cuenta == null || cuenta.EmpresaId != empresaId)
        {
            throw new Exception("Cuenta contable no encontrada.");
        }

        // Calcular Saldo Anterior
        var asientosAnteriores = await _asientoRepo.FindAsync(
            a => a.EmpresaId == empresaId && a.Activo
                && a.Estado == "Contabilizado"
                && a.Fecha < desde, ct);

        var asientosAnterioresIds = asientosAnteriores.Select(a => a.AsientoContableId).ToHashSet();
        decimal saldoAnterior = 0;

        if (asientosAnterioresIds.Count > 0)
        {
            var lineasAnteriores = await _lineaRepo.FindAsync(l => asientosAnterioresIds.Contains(l.AsientoContableId) && l.CuentaContableId == cuentaContableId && l.Activo, ct);
            foreach (var l in lineasAnteriores)
            {
                if (cuenta.Naturaleza == NaturalezaCuenta.Deudora)
                    saldoAnterior += l.Debe - l.Haber;
                else
                    saldoAnterior += l.Haber - l.Debe;
            }
        }

        // Consultar movimientos del periodo
        var asientosPeriodo = await _asientoRepo.FindAsync(
            a => a.EmpresaId == empresaId && a.Activo
                && a.Estado == "Contabilizado"
                && a.Fecha >= desde && a.Fecha <= hasta.AddDays(1).AddSeconds(-1), ct);

        var asientoPeriodoIds = asientosPeriodo.Select(a => a.AsientoContableId).ToHashSet();
        var lineasPeriodo = asientoPeriodoIds.Count > 0
            ? await _lineaRepo.FindAsync(l => asientoPeriodoIds.Contains(l.AsientoContableId) && l.CuentaContableId == cuentaContableId && l.Activo, ct)
            : new List<AsientoContableLinea>();

        var asientosMap = asientosPeriodo.ToDictionary(a => a.AsientoContableId);

        var lineasDto = new List<LibroMayorLineaDto>();
        decimal saldoProgresivo = saldoAnterior;
        decimal totalDebe = 0;
        decimal totalHaber = 0;

        // Ordenar líneas
        var lineasOrdenadas = lineasPeriodo
            .OrderBy(l => asientosMap.ContainsKey(l.AsientoContableId) ? asientosMap[l.AsientoContableId].Fecha : DateTime.MinValue)
            .ThenBy(l => l.AsientoContableLineaId) // Orden estable adicional
            .ToList();

        foreach (var l in lineasOrdenadas)
        {
            var asientoInfo = asientosMap.TryGetValue(l.AsientoContableId, out var asient) ? asient : null;

            totalDebe += l.Debe;
            totalHaber += l.Haber;

            if (cuenta.Naturaleza == NaturalezaCuenta.Deudora)
                saldoProgresivo += l.Debe - l.Haber;
            else
                saldoProgresivo += l.Haber - l.Debe;

            lineasDto.Add(new LibroMayorLineaDto
            {
                Fecha = asientoInfo?.Fecha ?? DateTime.MinValue,
                NumeroAsiento = asientoInfo?.Numero ?? string.Empty,
                Glosa = string.IsNullOrWhiteSpace(l.Glosa) ? (asientoInfo?.Glosa ?? string.Empty) : l.Glosa,
                Debe = l.Debe,
                Haber = l.Haber,
                SaldoProgresivo = saldoProgresivo
            });
        }

        return new LibroMayorDto
        {
            CuentaContableId = cuenta.CuentaContableId,
            CodigoCuenta = cuenta.Codigo,
            NombreCuenta = cuenta.Nombre,
            SaldoAnterior = saldoAnterior,
            TotalDebe = totalDebe,
            TotalHaber = totalHaber,
            SaldoFinal = saldoProgresivo,
            Lineas = lineasDto
        };
    }

    // ????????????????????????????????????????????
    // HELPERS
    // ????????????????????????????????????????????

    private async Task<Dictionary<int, decimal>> CalcularSaldosAlAsync(int empresaId, DateTime fechaCorte, CancellationToken ct)
    {
        var asientos = await _asientoRepo.FindAsync(
            a => a.EmpresaId == empresaId && a.Activo
                && a.Estado == "Contabilizado"
                && a.Fecha <= fechaCorte.AddDays(1).AddSeconds(-1), ct);

        var asientoIds = asientos.Select(a => a.AsientoContableId).ToHashSet();
        var lineas = asientoIds.Count > 0
            ? await _lineaRepo.FindAsync(l => asientoIds.Contains(l.AsientoContableId) && l.Activo, ct)
            : new List<AsientoContableLinea>();

        var cuentas = await _cuentaRepo.FindAsync(c => c.EmpresaId == empresaId && c.Activo, ct);
        var cuentaMap = cuentas.ToDictionary(c => c.CuentaContableId);

        var saldos = new Dictionary<int, decimal>();
        foreach (var group in lineas.GroupBy(l => l.CuentaContableId))
        {
            var debe = group.Sum(l => l.Debe);
            var haber = group.Sum(l => l.Haber);
            cuentaMap.TryGetValue(group.Key, out var cuenta);
            if (cuenta is null) continue;

            decimal saldo;
            if (cuenta.Naturaleza == NaturalezaCuenta.Deudora)
                saldo = debe - haber;
            else
                saldo = haber - debe;

            saldos[group.Key] = saldo;
        }
        return saldos;
    }

    private async Task<Dictionary<int, decimal>> CalcularSaldosPeriodoAsync(int empresaId, DateTime desde, DateTime hasta, CancellationToken ct)
    {
        var asientos = await _asientoRepo.FindAsync(
            a => a.EmpresaId == empresaId && a.Activo
                && a.Estado == "Contabilizado"
                && a.Fecha >= desde && a.Fecha <= hasta.AddDays(1).AddSeconds(-1), ct);

        var asientoIds = asientos.Select(a => a.AsientoContableId).ToHashSet();
        var lineas = asientoIds.Count > 0
            ? await _lineaRepo.FindAsync(l => asientoIds.Contains(l.AsientoContableId) && l.Activo, ct)
            : new List<AsientoContableLinea>();

        var cuentas = await _cuentaRepo.FindAsync(c => c.EmpresaId == empresaId && c.Activo, ct);
        var cuentaMap = cuentas.ToDictionary(c => c.CuentaContableId);

        var saldos = new Dictionary<int, decimal>();
        foreach (var group in lineas.GroupBy(l => l.CuentaContableId))
        {
            var debe = group.Sum(l => l.Debe);
            var haber = group.Sum(l => l.Haber);
            cuentaMap.TryGetValue(group.Key, out var cuenta);
            if (cuenta is null) continue;

            decimal saldo;
            if (cuenta.Naturaleza == NaturalezaCuenta.Deudora)
                saldo = debe - haber;
            else
                saldo = haber - debe;

            saldos[group.Key] = saldo;
        }
        return saldos;
    }

    private static List<BalanceGrupoDto> BuildTree(
        IReadOnlyList<CuentaContable> cuentas,
        Dictionary<int, decimal> saldos,
        TipoCuenta tipoCuenta)
    {
        var filtered = cuentas.Where(c => c.Tipo == tipoCuenta).ToList();
        var roots = filtered.Where(c => c.CuentaPadreId == null
            || !filtered.Any(p => p.CuentaContableId == c.CuentaPadreId)).ToList();

        return roots.OrderBy(c => c.Codigo)
            .Select(c => BuildNode(c, filtered, saldos))
            .Where(n => n.Saldo != 0 || n.SubCuentas.Any())
            .ToList();
    }

    private static BalanceGrupoDto BuildNode(
        CuentaContable cuenta,
        List<CuentaContable> allCuentas,
        Dictionary<int, decimal> saldos)
    {
        var children = allCuentas
            .Where(c => c.CuentaPadreId == cuenta.CuentaContableId)
            .OrderBy(c => c.Codigo)
            .Select(c => BuildNode(c, allCuentas, saldos))
            .Where(n => n.Saldo != 0 || n.SubCuentas.Any())
            .ToList();

        saldos.TryGetValue(cuenta.CuentaContableId, out var saldo);
        var totalSaldo = children.Count > 0 ? children.Sum(c => c.Saldo) + saldo : saldo;

        return new BalanceGrupoDto
        {
            Codigo = cuenta.Codigo,
            Nombre = cuenta.Nombre,
            Nivel = cuenta.Nivel,
            Saldo = totalSaldo,
            EsAgrupador = !cuenta.PermiteMovimientos,
            SubCuentas = children
        };
    }

    private static decimal SumarSaldoTree(List<BalanceGrupoDto> grupos)
    {
        return grupos.Sum(g => g.Saldo);
    }
}
