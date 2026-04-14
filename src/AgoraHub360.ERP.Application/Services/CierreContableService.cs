namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class CierreContableService : ICierreContableService
{
    private readonly IRepository<CierreContable> _cierreRepo;
    private readonly IRepository<CuentaContable> _cuentaRepo;
    private readonly IRepository<AsientoContable> _asientoRepo;
    private readonly IRepository<AsientoContableLinea> _lineaRepo;
    private readonly IRepository<TipoComprobante> _tipoCompRepo;
    private readonly IRepository<PeriodoContable> _periodoRepo;
    private readonly IAsientoContableService _asientoService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEstadoFinancieroService _estadoFinancieroService;
    private readonly IImpuestoService _impuestoService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CierreContableService> _logger;

    public CierreContableService(
        IRepository<CierreContable> cierreRepo,
        IRepository<CuentaContable> cuentaRepo,
        IRepository<AsientoContable> asientoRepo,
        IRepository<AsientoContableLinea> lineaRepo,
        IRepository<TipoComprobante> tipoCompRepo,
        IRepository<PeriodoContable> periodoRepo,
        IAsientoContableService asientoService,
        IUnitOfWork unitOfWork,
        IEstadoFinancieroService estadoFinancieroService,
        IImpuestoService impuestoService,
        ICurrentUserService currentUser,
        ILogger<CierreContableService> logger)
    {
        _cierreRepo = cierreRepo;
        _cuentaRepo = cuentaRepo;
        _asientoRepo = asientoRepo;
        _lineaRepo = lineaRepo;
        _tipoCompRepo = tipoCompRepo;
        _periodoRepo = periodoRepo;
        _asientoService = asientoService;
        _unitOfWork = unitOfWork;
        _estadoFinancieroService = estadoFinancieroService;
        _impuestoService = impuestoService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<bool> ExisteCierreAsync(int empresaId, int gestion, CancellationToken ct)
    {
        var cierres = await _cierreRepo.FindAsync(c => c.EmpresaId == empresaId && c.Gestion == gestion && c.Estado != "ANULADO", ct);
        return cierres.Any();
    }

    public async Task<CierreContableDto?> ObtenerCierreAsync(int empresaId, int gestion, CancellationToken ct)
    {
        var cierres = await _cierreRepo.FindAsync(c => c.EmpresaId == empresaId && c.Gestion == gestion && c.Estado != "ANULADO", ct);
        var cierre = cierres.FirstOrDefault();

        if (cierre == null) return null;

        return new CierreContableDto
        {
            Gestion = cierre.Gestion,
            FechaCierre = cierre.FechaCierre,
            Estado = cierre.Estado,
            Observaciones = cierre.Observaciones
        };
    }

    public async Task<CierreContableDto> EjecutarCierreAsync(int empresaId, EjecutarCierreDto dto, CancellationToken ct)
    {
        // 1. Validar que no exista
        if (await ExisteCierreAsync(empresaId, dto.Gestion, ct))
        {
            throw new InvalidOperationException($"Ya existe un cierre contable para la gestión {dto.Gestion}.");
        }

        // 2. Obtener cuentas de resultado
        var cuentasResultado = await _cuentaRepo.FindAsync(c => c.EmpresaId == empresaId 
            && c.PermiteMovimientos
            && (c.Tipo == TipoCuenta.Ingreso || c.Tipo == TipoCuenta.Gasto || c.Tipo == TipoCuenta.Costo), ct);

        var cuentaIds = cuentasResultado.Select(c => c.CuentaContableId).ToHashSet();

        // Obtener asientos de la gestión
        var asientos = await _asientoRepo.FindAsync(a => a.EmpresaId == empresaId 
            && a.Gestion == dto.Gestion 
            && a.Estado == "Contabilizado" 
            && a.Activo, ct);

        var asientoIds = asientos.Select(a => a.AsientoContableId).ToHashSet();

        // Obtener líneas
        var lineas = asientoIds.Count > 0
            ? await _lineaRepo.FindAsync(l => l.Activo && asientoIds.Contains(l.AsientoContableId) && cuentaIds.Contains(l.CuentaContableId), ct)
            : new List<AsientoContableLinea>();

        // Calcular saldos (agrupados por cuenta)
        // Saldo = Debe - Haber
        var saldos = lineas.GroupBy(l => l.CuentaContableId)
            .Select(g => new
            {
                CuentaId = g.Key,
                Saldo = g.Sum(x => x.Debe - x.Haber)
            }).ToList();

        // Necesitamos la cuenta Resultados Acumulados o Resultado del Ejercicio
        var cuentasPatrimonio = await _cuentaRepo.FindAsync(c => c.EmpresaId == empresaId && c.Tipo == TipoCuenta.Patrimonio && c.PermiteMovimientos, ct);
        var cuentaResultadoEjercicio = cuentasPatrimonio.FirstOrDefault(c => c.Nombre.Contains("Resultado", StringComparison.OrdinalIgnoreCase));
        
        if (cuentaResultadoEjercicio == null)
        {
            // Fallback a primera de patrimonio
            cuentaResultadoEjercicio = cuentasPatrimonio.FirstOrDefault() 
                ?? throw new InvalidOperationException("No se encontró una cuenta de Patrimonio para registrar el resultado.");
        }

        // 3. Generar Asiento
        var crearAsientoDto = new CreateAsientoContableDto
        {
            Fecha = dto.FechaCierre,
            Concepto = $"Cierre de Gestión {dto.Gestion}",
            Glosa = $"Asiento de cierre de resultados - Gestión {dto.Gestion}",
            // Default TipoComprobante: Diarios 
            // Better to find a TipoComprobante "Diario" or "Traspaso"
        };
        
        var tiposComprobante = await _tipoCompRepo.FindAsync(t => t.EmpresaId == empresaId && t.Activo, ct);
        var tipoTraspaso = tiposComprobante.FirstOrDefault(t => t.Codigo.ToUpper().Contains("TRA") || t.Codigo.ToUpper().Contains("DIA")) 
            ?? tiposComprobante.FirstOrDefault();

        if (tipoTraspaso == null) throw new InvalidOperationException("No se encontraron tipos de comprobante.");
        crearAsientoDto.TipoComprobanteId = tipoTraspaso.TipoComprobanteId;

        // Líneas para cancelar cuentas de resultado
        var nuevasLineas = new List<CreateAsientoLineaDto>();
        decimal totalResultado = 0; // Utilidad si es que Ingresos > Gastos

        foreach (var s in saldos.Where(x => Math.Round(x.Saldo, 2) != 0))
        {
            // Para cancelar un saldo deudor (positivo), abonamos (Haber)
            // Para cancelar un saldo acreedor (negativo), debitamos (Debe)
            decimal debe = s.Saldo < 0 ? Math.Abs(s.Saldo) : 0;
            decimal haber = s.Saldo > 0 ? s.Saldo : 0;

            nuevasLineas.Add(new CreateAsientoLineaDto
            {
                CuentaContableId = s.CuentaId,
                Debe = debe,
                Haber = haber,
                Glosa = $"Cierre cuenta de resultados - Gestión {dto.Gestion}"
            });

            // Si es Ingreso, saldo < 0 (Acreedor). Cancelarlo genera Debe positivo en nuevasLineas.
            // Por partida doble, el resultado será Haber en la cuenta patrimonial.
            totalResultado += (haber - debe); 
        }

        if (nuevasLineas.Count > 0)
        {
            // Registrar la diferencia a la cuenta de resultado del ejercicio
            decimal resultadoDebe = totalResultado < 0 ? Math.Abs(totalResultado) : 0;
            decimal resultadoHaber = totalResultado > 0 ? totalResultado : 0;

            if (Math.Round(resultadoDebe + resultadoHaber, 2) > 0)
            {
                nuevasLineas.Add(new CreateAsientoLineaDto
                {
                    CuentaContableId = cuentaResultadoEjercicio.CuentaContableId,
                    Debe = resultadoDebe,
                    Haber = resultadoHaber,
                    Glosa = $"Resultado del Ejercicio - Gestión {dto.Gestion}"
                });
            }

            crearAsientoDto.Lineas = nuevasLineas;

            var rAsiento = await _asientoService.CreateAsync(crearAsientoDto, ct);
            if (!rAsiento.IsSuccess)
                throw new InvalidOperationException($"Error creando asiento de cierre: {rAsiento.Error}");

            var rContabilizar = await _asientoService.ContabilizarAsync(rAsiento.Value!.AsientoContableId, ct);
            if (!rContabilizar.IsSuccess)
                throw new InvalidOperationException($"Error contabilizando asiento de cierre: {rContabilizar.Error}");
        }

        // 4. Crear registro CierreContable
        var cierreEntidad = CierreContable.Crear(empresaId, dto.Gestion, dto.FechaCierre);
        cierreEntidad.MarcarComoCerrado();
        
        await _cierreRepo.AddAsync(cierreEntidad, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CierreContableDto
        {
            Gestion = cierreEntidad.Gestion,
            FechaCierre = cierreEntidad.FechaCierre,
            Estado = cierreEntidad.Estado,
            Observaciones = cierreEntidad.Observaciones
        };
    }

    public async Task EjecutarAperturaAsync(int empresaId, int gestionNueva, CancellationToken ct)
    {
        // 1. Obtener Balance General al 31 de diciembre del año anterior
        var fechaCierreAnterior = new DateTime(gestionNueva - 1, 12, 31);
        var rBalance = await _estadoFinancieroService.GetBalanceGeneralAsync(fechaCierreAnterior, ct);
        if (!rBalance.IsSuccess)
        {
            throw new InvalidOperationException($"No se pudo obtener el Balance General: {rBalance.Error}");
        }

        var balance = rBalance.Value!;

        // 2. Extraer los saldos de las cuentas finales (hojas)
        var cuentasFinales = new Dictionary<string, decimal>();

        void RecolectarSaldos(List<BalanceGrupoDto> nodos)
        {
            foreach (var nodo in nodos)
            {
                if (!nodo.EsAgrupador && nodo.Saldo != 0)
                {
                    cuentasFinales[nodo.Codigo] = nodo.Saldo;
                }

                if (nodo.SubCuentas != null && nodo.SubCuentas.Count > 0)
                {
                    RecolectarSaldos(nodo.SubCuentas);
                }
            }
        }

        RecolectarSaldos(balance.Activos);
        RecolectarSaldos(balance.Pasivos);
        RecolectarSaldos(balance.Patrimonio);

        if (cuentasFinales.Count == 0)
        {
            throw new InvalidOperationException("No se encontraron saldos de balance general para la gestión anterior.");
        }

        // 3. Mapear Codigo -> CuentaContableId y construir lineas del asiento de apertura (solo cuentas operativas)
        var cuentas = await _cuentaRepo.FindAsync(c => c.EmpresaId == empresaId && c.Activo && c.PermiteMovimientos, ct);
        var cuentaMap = cuentas.ToDictionary(c => c.Codigo);

        var lineasApertura = new List<CreateAsientoLineaDto>();
        decimal totalDebe = 0;
        decimal totalHaber = 0;

        foreach (var kvp in cuentasFinales)
        {
            if (cuentaMap.TryGetValue(kvp.Key, out var cuenta))
            {
                // BalanceGeneralDto.Saldo es positivo absoluto. Naturaleza nos dice si va en Debe o Haber.
                decimal debe = 0;
                decimal haber = 0;

                // Las cuentas de Activo suelen ser Deudoras (Debe). Pasivo/Patrimonio Acreedoras (Haber).
                // Pero un saldo negativo en Activo iría en Haber, etc.
                if (cuenta.Naturaleza == NaturalezaCuenta.Deudora)
                {
                    if (kvp.Value >= 0) debe = kvp.Value;
                    else haber = Math.Abs(kvp.Value);
                }
                else
                {
                    if (kvp.Value >= 0) haber = kvp.Value;
                    else debe = Math.Abs(kvp.Value);
                }

                if (debe > 0 || haber > 0)
                {
                    lineasApertura.Add(new CreateAsientoLineaDto
                    {
                        CuentaContableId = cuenta.CuentaContableId,
                        Debe = debe,
                        Haber = haber,
                        Glosa = $"Apertura Gestión {gestionNueva}"
                    });
                    totalDebe += debe;
                    totalHaber += haber;
                }
            }
        }

        // Ajuste por redondeo o centavos si fuera necesario (aunque un balance cuadrado debe dar Debe == Haber)
        var diferencia = Math.Round(totalDebe - totalHaber, 2);
        if (diferencia != 0)
        {
            throw new InvalidOperationException($"El asiento de apertura no cuadra. Diferencia: {diferencia:N2}");
        }

        // 4. Buscar Tipo de Comprobante para Apertura o Traspaso
        var tiposComprobante = await _tipoCompRepo.FindAsync(t => t.EmpresaId == empresaId && t.Activo, ct);
        var tipoApertura = tiposComprobante.FirstOrDefault(t => t.Codigo == "APE") 
            ?? tiposComprobante.FirstOrDefault(t => t.Codigo == "TRA") 
            ?? tiposComprobante.FirstOrDefault();

        if (tipoApertura == null)
            throw new InvalidOperationException("No se encontró un tipo de comprobante para el asiento de apertura.");

        var crearAsientoDto = new CreateAsientoContableDto
        {
            TipoComprobanteId = tipoApertura.TipoComprobanteId,
            Fecha = new DateTime(gestionNueva, 1, 1),
            Concepto = $"Apertura Gestión {gestionNueva}",
            Glosa = $"Asiento automático de apertura de la gestión {gestionNueva}",
            Lineas = lineasApertura
        };

        var rAsiento = await _asientoService.CreateAsync(crearAsientoDto, ct);
        if (!rAsiento.IsSuccess)
            throw new InvalidOperationException($"Error creando asiento de apertura: {rAsiento.Error}");

        var rContabilizar = await _asientoService.ContabilizarAsync(rAsiento.Value!.AsientoContableId, ct);
        if (!rContabilizar.IsSuccess)
            throw new InvalidOperationException($"Error contabilizando asiento de apertura: {rContabilizar.Error}");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CIERRE ANUAL COMPLETO
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<CierreContableDto> EjecutarCierreAnualAsync(
        EjecutarCierreAnualDto dto,
        CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId
            ?? throw new InvalidOperationException("No hay empresa activa en el contexto del usuario.");

        _logger.LogInformation(
            "Iniciando cierre anual completo. EmpresaId={EmpresaId}, Gestión={Gestion}",
            empresaId, dto.Gestion);

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
            // ── PASO 1: Validar que existan los 12 períodos del año ───────────
            var periodos = await _periodoRepo.FindAsync(
                p => p.EmpresaId == empresaId && p.Anio == dto.Gestion && p.Activo, ct);

            if (periodos.Count == 0)
                throw new InvalidOperationException(
                    $"No existen períodos contables para la gestión {dto.Gestion}. Genérelos antes de ejecutar el cierre.");

            if (periodos.Count < 12)
                throw new InvalidOperationException(
                    $"La gestión {dto.Gestion} no tiene los 12 períodos generados. Se encontraron {periodos.Count}.");

            // ── PASO 2: Validar que no existan asientos en borrador ──────────
            var borradores = await _asientoRepo.FindAsync(
                a => a.EmpresaId == empresaId
                  && a.Gestion == dto.Gestion
                  && a.Estado == "Borrador"
                  && a.Activo, ct);

            if (borradores.Count > 0)
                throw new InvalidOperationException(
                    $"Existen {borradores.Count} asiento(s) en estado Borrador para la gestión {dto.Gestion}. " +
                    "Contabilícelos o elimínelos antes de ejecutar el cierre.");

            // ── PASO 3: Validar que no exista cierre previo ──────────────────
            if (await ExisteCierreAsync(empresaId, dto.Gestion, ct))
                throw new InvalidOperationException(
                    $"Ya existe un cierre contable para la gestión {dto.Gestion} en esta empresa.");

            // ── Cargar catálogos ─────────────────────────────────────────────
            var tiposComp = await _tipoCompRepo.FindAsync(
                t => t.EmpresaId == empresaId && t.Activo, ct);

            var tipoCierre = tiposComp
                .OrderBy(t => t.Orden)
                .FirstOrDefault(t => new[] { "CIE", "TRA", "DIA" }.Contains(t.Codigo.ToUpper()))
                ?? tiposComp.OrderBy(t => t.Orden).FirstOrDefault()
                ?? throw new InvalidOperationException("No se encontró ningún tipo de comprobante activo para registrar los asientos de cierre.");

            var todasCuentas = await _cuentaRepo.FindAsync(
                c => c.EmpresaId == empresaId && c.Activo, ct);

            // ── PASO 4: Calcular IUE ─────────────────────────────────────────
            decimal montoIUE = 0m;
            long? asientoIUEId = null;

            try
            {
                var rIUE = await _impuestoService.CalcularIUEAsync(dto.Gestion, ct);
                if (rIUE.IsSuccess && rIUE.Value != null && rIUE.Value.MontoCalculado > 0)
                {
                    montoIUE = Math.Round(rIUE.Value.MontoCalculado, 2);
                    _logger.LogInformation(
                        "IUE calculado. Gestión={Gestion}, MontoIUE={MontoIUE:N2}", dto.Gestion, montoIUE);
                }
                else
                {
                    _logger.LogWarning(
                        "No se pudo calcular el IUE para la gestión {Gestion}: {Detalle}. Se omitirá el asiento de IUE.",
                        dto.Gestion, rIUE.IsSuccess ? "resultado vacío o cero" : rIUE.Error);
                }
            }
            catch (Exception exIUE)
            {
                _logger.LogWarning(exIUE,
                    "Error al calcular IUE para la gestión {Gestion}. Se omitirá el asiento de IUE. " +
                    "Verifique que la migración 'TRB_RegistrosImpuesto' haya sido aplicada a la base de datos.",
                    dto.Gestion);
                montoIUE = 0m;
            }

            // Obtener utilidad bruta ANTES de crear cualquier asiento de cierre
            var fechaInicio = new DateTime(dto.Gestion, 1, 1);
            var rEstado = await _estadoFinancieroService.GetEstadoResultadosAsync(fechaInicio, dto.FechaCierre, ct);
            if (!rEstado.IsSuccess)
                throw new InvalidOperationException(
                    $"No se pudo obtener el Estado de Resultados para la gestión {dto.Gestion}: {rEstado.Error}");

            var utilidadBruta = rEstado.Value!.UtilidadNeta;
            _logger.LogInformation(
                "Estado de Resultados. Gestión={Gestion}, UtilidadBruta={UB:N2}, IUE={IUE:N2}",
                dto.Gestion, utilidadBruta, montoIUE);

            // ── PASO 5: Generar asiento del IUE ─────────────────────────────
            if (montoIUE > 0)
            {
                var cuentaGastoIUE = BuscarCuentaRequerida(
                    todasCuentas, TipoCuenta.Gasto,
                    ["IUE", "Impuesto Utilidades", "Impuesto sobre Utilidades"],
                    "Gasto – Impuesto a las Utilidades de las Empresas (IUE)");

                var cuentaIUEPorPagar = BuscarCuentaRequerida(
                    todasCuentas, TipoCuenta.Pasivo,
                    ["IUE", "Impuesto Utilidades por Pagar", "IUE por Pagar"],
                    "Pasivo – IUE por Pagar");

                asientoIUEId = await CrearYContabilizarAsientoDirectoAsync(
                    empresaId,
                    tipoCierre.TipoComprobanteId,
                    dto.FechaCierre,
                    $"IUE Gestión {dto.Gestion}",
                    $"Impuesto a las Utilidades de las Empresas – Gestión {dto.Gestion}",
                    "CIERRE_IUE",
                    [
                        (cuentaGastoIUE.CuentaContableId, montoIUE, 0m,
                            $"Gasto IUE gestión {dto.Gestion}"),
                        (cuentaIUEPorPagar.CuentaContableId, 0m, montoIUE,
                            $"IUE por pagar gestión {dto.Gestion}")
                    ],
                    ct);

                _logger.LogInformation("Asiento IUE creado. AsientoId={Id}", asientoIUEId);
            }

            // ── Construir asiento de cierre de cuentas de resultado ──────────
            // Incluye el gasto IUE recién registrado (ya está contabilizado)
            var cuentasResultado = todasCuentas
                .Where(c => c.PermiteMovimientos
                         && (c.Tipo == TipoCuenta.Ingreso
                          || c.Tipo == TipoCuenta.Gasto
                          || c.Tipo == TipoCuenta.Costo))
                .ToList();

            var cuentaIdsResultado = cuentasResultado
                .Select(c => c.CuentaContableId)
                .ToHashSet();

            var asientosGestion = await _asientoRepo.FindAsync(
                a => a.EmpresaId == empresaId
                  && a.Gestion == dto.Gestion
                  && a.Estado == "Contabilizado"
                  && a.Activo, ct);

            var asientoIdsGestion = asientosGestion
                .Select(a => a.AsientoContableId)
                .ToHashSet();

            var lineasResultado = asientoIdsGestion.Count > 0
                ? await _lineaRepo.FindAsync(
                    l => l.Activo
                      && asientoIdsGestion.Contains(l.AsientoContableId)
                      && cuentaIdsResultado.Contains(l.CuentaContableId), ct)
                : (IReadOnlyList<AsientoContableLinea>)[];

            var saldosPorCuenta = lineasResultado
                .GroupBy(l => l.CuentaContableId)
                .Select(g => new { CuentaId = g.Key, Saldo = g.Sum(x => x.Debe - x.Haber) })
                .Where(s => Math.Round(Math.Abs(s.Saldo), 2) > 0)
                .ToList();

            // Cuentas patrimoniales obligatorias
            var cuentaResultadoEjercicio = BuscarCuentaRequerida(
                todasCuentas, TipoCuenta.Patrimonio,
                ["Resultado del Ejercicio", "Resultado Ejercicio", "Resultado Periodo"],
                "Patrimonio – Resultado del Ejercicio");

            var cuentaResultadosAcumulados = BuscarCuentaRequerida(
                todasCuentas, TipoCuenta.Patrimonio,
                ["Resultados Acumulados", "Resultado Acumulado", "Utilidades Retenidas", "Utilidades Acumuladas"],
                "Patrimonio – Resultados Acumulados");

            // ── PASO 7 (primera parte): Cancelar cuentas de resultado → RE ───
            long? asientoResultadoId = null;
            decimal utilidadPostIUE = 0m;

            if (saldosPorCuenta.Count > 0)
            {
                var lineasCierre = new List<(int cuentaId, decimal debe, decimal haber, string glosa)>();
                decimal totalAResultadoEjercicio = 0m;

                foreach (var s in saldosPorCuenta)
                {
                    // Saldo Dr (positivo) = Gasto/Costo → cancelar con Cr
                    // Saldo Cr (negativo) = Ingreso    → cancelar con Dr
                    decimal debe  = s.Saldo < 0 ? Math.Abs(s.Saldo) : 0m;
                    decimal haber = s.Saldo > 0 ? s.Saldo : 0m;

                    lineasCierre.Add((s.CuentaId, debe, haber,
                        $"Cierre cuenta de resultados – Gestión {dto.Gestion}"));

                    // neto hacia RE: +income, -expense
                    totalAResultadoEjercicio += debe - haber;
                }

                utilidadPostIUE = Math.Round(totalAResultadoEjercicio, 2);

                // Línea de contrapartida en Resultado del Ejercicio
                if (Math.Abs(utilidadPostIUE) > 0.001m)
                {
                    decimal reDebe  = utilidadPostIUE < 0 ? Math.Abs(utilidadPostIUE) : 0m;
                    decimal reHaber = utilidadPostIUE > 0 ? utilidadPostIUE : 0m;
                    lineasCierre.Add((
                        cuentaResultadoEjercicio.CuentaContableId,
                        reDebe, reHaber,
                        $"Resultado del Ejercicio – Gestión {dto.Gestion}"));
                }

                asientoResultadoId = await CrearYContabilizarAsientoDirectoAsync(
                    empresaId,
                    tipoCierre.TipoComprobanteId,
                    dto.FechaCierre,
                    $"Cierre de Resultados Gestión {dto.Gestion}",
                    $"Cancelación de cuentas de resultado – Gestión {dto.Gestion}",
                    "CIERRE_RESULTADOS",
                    lineasCierre,
                    ct);

                _logger.LogInformation(
                    "Asiento cierre de resultados creado. AsientoId={Id}, UtilPostIUE={Util:N2}",
                    asientoResultadoId, utilidadPostIUE);
            }
            else
            {
                _logger.LogInformation(
                    "Gestión {Gestion}: sin saldos en cuentas de resultado. Se omite el asiento de cierre.",
                    dto.Gestion);
            }

            // ── PASO 6 y PASO 7 (segunda parte): Reserva legal y transferencia
            decimal reservaLegal = 0m;
            long? asientoReservaLegalId = null;
            long? asientoTransferenciaId = null;

            if (utilidadPostIUE > 0)
            {
                // PASO 6: Reserva legal 5 %
                reservaLegal = Math.Round(utilidadPostIUE * 0.05m, 2);
                _logger.LogInformation(
                    "Reserva legal 5%. Gestión={Gestion}, ReservaLegal={RL:N2}",
                    dto.Gestion, reservaLegal);

                if (reservaLegal > 0)
                {
                    var cuentaReservaLegal = BuscarCuentaRequerida(
                        todasCuentas, TipoCuenta.Patrimonio,
                        ["Reserva Legal", "Reserva Estatutaria", "Reservas"],
                        "Patrimonio – Reserva Legal");

                    asientoReservaLegalId = await CrearYContabilizarAsientoDirectoAsync(
                        empresaId,
                        tipoCierre.TipoComprobanteId,
                        dto.FechaCierre,
                        $"Reserva Legal Gestión {dto.Gestion}",
                        $"Constitución de reserva legal 5 % sobre utilidad neta – Gestión {dto.Gestion}",
                        "CIERRE_RESERVA_LEGAL",
                        [
                            (cuentaResultadoEjercicio.CuentaContableId, reservaLegal, 0m,
                                $"Reserva legal 5 % – Gestión {dto.Gestion}"),
                            (cuentaReservaLegal.CuentaContableId, 0m, reservaLegal,
                                $"Reserva legal 5 % – Gestión {dto.Gestion}")
                        ],
                        ct);

                    _logger.LogInformation(
                        "Asiento reserva legal creado. AsientoId={Id}", asientoReservaLegalId);
                }

                // PASO 7: Transferir utilidad restante a Resultados Acumulados
                var utilidadATransferir = Math.Round(utilidadPostIUE - reservaLegal, 2);
                if (utilidadATransferir > 0)
                {
                    asientoTransferenciaId = await CrearYContabilizarAsientoDirectoAsync(
                        empresaId,
                        tipoCierre.TipoComprobanteId,
                        dto.FechaCierre,
                        $"Transferencia Resultado Gestión {dto.Gestion}",
                        $"Transferencia de utilidad neta a resultados acumulados – Gestión {dto.Gestion}",
                        "CIERRE_RESULTADO",
                        [
                            (cuentaResultadoEjercicio.CuentaContableId, utilidadATransferir, 0m,
                                $"Utilidad neta gestión {dto.Gestion}"),
                            (cuentaResultadosAcumulados.CuentaContableId, 0m, utilidadATransferir,
                                $"Resultados acumulados gestión {dto.Gestion}")
                        ],
                        ct);

                    _logger.LogInformation(
                        "Asiento transferencia utilidad creado. AsientoId={Id}, Monto={M:N2}",
                        asientoTransferenciaId, utilidadATransferir);
                }
            }
            else if (utilidadPostIUE < 0)
            {
                // PASO 7: Transferir pérdida a Resultados Acumulados
                var perdida = Math.Round(Math.Abs(utilidadPostIUE), 2);

                asientoTransferenciaId = await CrearYContabilizarAsientoDirectoAsync(
                    empresaId,
                    tipoCierre.TipoComprobanteId,
                    dto.FechaCierre,
                    $"Absorción Pérdida Gestión {dto.Gestion}",
                    $"Absorción de pérdida del ejercicio en resultados acumulados – Gestión {dto.Gestion}",
                    "CIERRE_RESULTADO",
                    [
                        (cuentaResultadosAcumulados.CuentaContableId, perdida, 0m,
                            $"Absorción pérdida gestión {dto.Gestion}"),
                        (cuentaResultadoEjercicio.CuentaContableId, 0m, perdida,
                            $"Pérdida del ejercicio {dto.Gestion}")
                    ],
                    ct);

                _logger.LogInformation(
                    "Asiento absorción de pérdida creado. AsientoId={Id}, Monto={M:N2}",
                    asientoTransferenciaId, perdida);
            }

            // ── PASO 8: Cerrar todos los períodos del año que sigan abiertos ─
            var periodosAbiertos = periodos.Where(p => p.Estado != "Cerrado").ToList();
            foreach (var periodo in periodosAbiertos)
            {
                periodo.Estado = "Cerrado";
                periodo.FechaCierre = DateTime.UtcNow;
                periodo.CerradoPorId = _currentUser.UserIdInt;
                periodo.CerradoPorNombre = _currentUser.UserName;
                await _periodoRepo.UpdateAsync(periodo, ct);
            }
            if (periodosAbiertos.Count > 0)
            {
                await _unitOfWork.SaveChangesAsync(ct);
                _logger.LogInformation(
                    "{Count} período(s) cerrado(s) durante el cierre anual de gestión {Gestion}.",
                    periodosAbiertos.Count, dto.Gestion);
            }

            // ── PASO 9: Crear registro de cierre contable ────────────────────
            var cierreEntidad = CierreContable.Crear(empresaId, dto.Gestion, dto.FechaCierre);
            cierreEntidad.MarcarComoCerrado();
            if (!string.IsNullOrWhiteSpace(dto.Observaciones))
                cierreEntidad.ActualizarObservaciones(dto.Observaciones);

            await _cierreRepo.AddAsync(cierreEntidad, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Registro de cierre contable creado. Gestión={Gestion}, Estado={Estado}.",
                cierreEntidad.Gestion, cierreEntidad.Estado);

            // ── PASO 10: Apertura de nueva gestión (opcional) ────────────────
            if (dto.AbrirNuevaGestion)
            {
                var gestionNueva = dto.Gestion + 1;
                _logger.LogInformation("Ejecutando apertura de gestión {GestionNueva}.", gestionNueva);
                await EjecutarAperturaAsync(empresaId, gestionNueva, ct);
                _logger.LogInformation("Apertura de gestión {GestionNueva} completada.", gestionNueva);
            }

            _logger.LogInformation(
                "Cierre anual gestión {Gestion} completado. IUE={IUE:N2}, RL={RL:N2}, UtilPostIUE={U:N2}.",
                dto.Gestion, montoIUE, reservaLegal, utilidadPostIUE);

            return new CierreContableDto
            {
                Gestion               = cierreEntidad.Gestion,
                FechaCierre           = cierreEntidad.FechaCierre,
                Estado                = cierreEntidad.Estado,
                Observaciones         = cierreEntidad.Observaciones,
                UtilidadNeta          = utilidadBruta,
                MontoIUE              = montoIUE > 0 ? montoIUE : null,
                ReservaLegal          = reservaLegal > 0 ? reservaLegal : null,
                AsientoIUEId          = asientoIUEId,
                AsientoReservaLegalId = asientoReservaLegalId,
                AsientoResultadoId    = asientoResultadoId,
                AsientoTransferenciaId = asientoTransferenciaId,
                NuevaGestionAbierta   = dto.AbrirNuevaGestion
            };
            }, ct); // ExecuteInTransactionAsync — commit/rollback gestionado internamente
        }
        catch (Exception ex)
        {
            // Desenrollar hasta la causa raíz (e.g. SqlException)
            var root = ex;
            while (root.InnerException != null) root = root.InnerException;

            _logger.LogError(ex,
                "Error durante el cierre anual de la gestión {Gestion}. Causa raíz: {RootMessage}",
                dto.Gestion, root.Message);

            throw new InvalidOperationException(
                $"Error al ejecutar el cierre anual de la gestión {dto.Gestion}: {root.Message}", ex);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers privados
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Crea un asiento contable DIRECTAMENTE vía repositorios, lo contabiliza y actualiza
    /// los saldos de las cuentas, omitiendo la validación de período abierto.
    /// Uso exclusivo del proceso de cierre anual.
    /// </summary>
    private async Task<long> CrearYContabilizarAsientoDirectoAsync(
        int empresaId,
        int tipoComprobanteId,
        DateTime fecha,
        string concepto,
        string glosa,
        string origenTipo,
        List<(int cuentaContableId, decimal debe, decimal haber, string lineaGlosa)> lineas,
        CancellationToken ct)
    {
        if (lineas.Count == 0)
            throw new InvalidOperationException("No se puede crear un asiento de cierre sin líneas.");

        var totalDebe  = lineas.Sum(l => l.debe);
        var totalHaber = lineas.Sum(l => l.haber);

        if (Math.Round(Math.Abs(totalDebe - totalHaber), 2) > 0.01m)
            throw new InvalidOperationException(
                $"El asiento de cierre '{concepto}' no cuadra. " +
                $"Debe: {totalDebe:N2}, Haber: {totalHaber:N2}.");

        // Generar número de comprobante — Formato: {Prefijo}-{Mes:D2}-{secuencial:D4}
        // Alcance único: Empresa + Gestión + TipoComprobante + Periodo (mes)
        var tipoComp = await _tipoCompRepo.GetByIdAsync(tipoComprobanteId, ct);
        var prefijo  = tipoComp?.Prefijo ?? "CIE";
        var mes = fecha.Month;
        var prefijoMes = $"{prefijo}-{mes:D2}-";

        var enMismoPeriodo = await _asientoRepo.FindAsync(
            a => a.EmpresaId == empresaId
              && a.Gestion == fecha.Year
              && a.Numero.StartsWith(prefijoMes), ct);

        var maxSeq = 0;
        foreach (var a in enMismoPeriodo)
        {
            var lastDash = a.Numero.LastIndexOf('-');
            if (lastDash >= 0 && int.TryParse(a.Numero[(lastDash + 1)..], out int n) && n > maxSeq)
                maxSeq = n;
        }

        var numero = $"{prefijo}-{mes:D2}-{(maxSeq + 1):D4}";

        var asiento = new AsientoContable
        {
            EmpresaId           = empresaId,
            TipoComprobanteId   = tipoComprobanteId,
            Numero              = numero,
            Fecha               = fecha,
            Gestion             = fecha.Year,
            TipoRegistro        = "Automático",
            Estado              = "Borrador",
            Concepto            = concepto,
            Glosa               = glosa,
            RegistradoPorId     = _currentUser.UserIdInt,
            RegistradoPorNombre = _currentUser.UserName,
            OrigenTipo          = origenTipo,
            Activo              = true
        };
        asiento.EstablecerTotales(totalDebe, totalHaber);

        await _asientoRepo.AddAsync(asiento, ct);
        await _unitOfWork.SaveChangesAsync(ct); // obtiene AsientoContableId

        int lineNum = 1;
        foreach (var (cuentaId, debe, haber, lineaGlosa) in lineas)
        {
            await _lineaRepo.AddAsync(new AsientoContableLinea
            {
                AsientoContableId = asiento.AsientoContableId,
                NumeroLinea       = lineNum++,
                CuentaContableId  = cuentaId,
                Debe              = debe,
                Haber             = haber,
                Glosa             = lineaGlosa,
                Activo            = true
            }, ct);
        }
        await _unitOfWork.SaveChangesAsync(ct);

        // Contabilizar (cambia estado y valida cuadratura en dominio)
        var contabilizarResult = asiento.Contabilizar(_currentUser.UserName);
        if (!contabilizarResult.IsSuccess)
            throw new InvalidOperationException(
                $"Error al contabilizar asiento de cierre '{concepto}': {contabilizarResult.Error}");

        // Actualizar saldos de cuentas (misma lógica que AsientoContableService.ContabilizarAsync)
        foreach (var (cuentaId, debe, haber, _) in lineas)
        {
            var cuenta = await _cuentaRepo.GetByIdAsync(cuentaId, ct);
            if (cuenta is null) continue;

            cuenta.SaldoActual += cuenta.Naturaleza == NaturalezaCuenta.Deudora
                ? debe - haber
                : haber - debe;

            await _cuentaRepo.UpdateAsync(cuenta, ct);
        }

        await _asientoRepo.UpdateAsync(asiento, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogDebug(
            "Asiento directo creado y contabilizado. Numero={Num}, OrigenTipo={Origen}",
            asiento.Numero, origenTipo);

        return asiento.AsientoContableId;
    }

    /// <summary>
    /// Busca una cuenta contable por tipo y palabras clave en código o nombre.
    /// Lanza <see cref="InvalidOperationException"/> descriptiva si no la encuentra.
    /// </summary>
    private static CuentaContable BuscarCuentaRequerida(
        IReadOnlyList<CuentaContable> cuentas,
        TipoCuenta tipo,
        string[] keywords,
        string descripcion)
    {
        var candidatas = cuentas.Where(c => c.Tipo == tipo && c.PermiteMovimientos);
        foreach (var kw in keywords)
        {
            var cuenta = candidatas.FirstOrDefault(c =>
                c.Codigo.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                c.Nombre.Contains(kw, StringComparison.OrdinalIgnoreCase));
            if (cuenta is not null) return cuenta;
        }

        throw new InvalidOperationException(
            $"No se encontró la cuenta requerida: '{descripcion}' (Tipo: {tipo}). " +
            $"Configure el plan de cuentas con una cuenta que contenga alguna de las palabras clave: " +
            $"{string.Join(", ", keywords)}.");
    }
}
