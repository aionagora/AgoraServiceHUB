namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
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
    private readonly IAsientoContableService _asientoService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEstadoFinancieroService _estadoFinancieroService;

    public CierreContableService(
        IRepository<CierreContable> cierreRepo,
        IRepository<CuentaContable> cuentaRepo,
        IRepository<AsientoContable> asientoRepo,
        IRepository<AsientoContableLinea> lineaRepo,
        IRepository<TipoComprobante> tipoCompRepo,
        IAsientoContableService asientoService,
        IUnitOfWork unitOfWork,
        IEstadoFinancieroService estadoFinancieroService)
    {
        _cierreRepo = cierreRepo;
        _cuentaRepo = cuentaRepo;
        _asientoRepo = asientoRepo;
        _lineaRepo = lineaRepo;
        _tipoCompRepo = tipoCompRepo;
        _asientoService = asientoService;
        _unitOfWork = unitOfWork;
        _estadoFinancieroService = estadoFinancieroService;
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
}
