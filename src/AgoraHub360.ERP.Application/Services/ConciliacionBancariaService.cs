namespace AgoraHub360.ERP.Application.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Entities.BNC;
using AgoraHub360.ERP.Domain.Exceptions;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Bancario;

public class ConciliacionBancariaService : IConciliacionBancariaService
{
    private readonly IRepository<ExtractoBancario> _extractoRepo;
    private readonly IRepository<ConciliacionBancaria> _conciliacionRepo;
    private readonly IRepository<PeriodoContable> _periodoRepo;
    private readonly IRepository<AsientoContable> _asientoRepo;
    private readonly IRepository<AsientoContableLinea> _lineaRepo;
    private readonly IRepository<CuentaContable> _cuentaRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEnumerable<IExtractoImportService> _importServices;

    public ConciliacionBancariaService(
        IRepository<ExtractoBancario> extractoRepo,
        IRepository<ConciliacionBancaria> conciliacionRepo,
        IRepository<PeriodoContable> periodoRepo,
        IRepository<AsientoContable> asientoRepo,
        IRepository<AsientoContableLinea> lineaRepo,
        IRepository<CuentaContable> cuentaRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEnumerable<IExtractoImportService> importServices)
    {
        _extractoRepo = extractoRepo;
        _conciliacionRepo = conciliacionRepo;
        _periodoRepo = periodoRepo;
        _asientoRepo = asientoRepo;
        _lineaRepo = lineaRepo;
        _cuentaRepo = cuentaRepo;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _importServices = importServices;
    }

    private int ObtenerEmpresaId()
    {
        return _currentUserService.EmpresaId ?? throw new DomainException("No se pudo obtener la empresa actual.");
    }

    public async Task<Result<ImportExtractoResultDto>> ImportarExtractoAsync(int cuentaId, int periodoId, Stream archivo, string formato, CancellationToken ct = default)
    {
        var empresaId = ObtenerEmpresaId();
        
        var cuenta = await _cuentaRepo.GetByIdAsync(cuentaId, ct);
        if (cuenta == null || cuenta.EmpresaId != empresaId)
            return Result<ImportExtractoResultDto>.Failure("Cuenta contable inválida.");

        var periodo = await _periodoRepo.GetByIdAsync(periodoId, ct);
        if (periodo == null || periodo.EmpresaId != empresaId || periodo.Estado == "Cerrado")
            return Result<ImportExtractoResultDto>.Failure("Período contable inválido o cerrado.");

        var importService = _importServices.FirstOrDefault();
        if (importService != null)
        {
            return await importService.ImportarAsync(empresaId, cuentaId, archivo, formato, ct);
        }
        else
        {
            return await ParsearComoCsvBasicoAsync(empresaId, cuentaId, archivo, ct);
        }
    }

    private async Task<Result<ImportExtractoResultDto>> ParsearComoCsvBasicoAsync(int empresaId, int cuentaId, Stream archivo, CancellationToken ct)
    {
        var result = new ImportExtractoResultDto();
        using var reader = new StreamReader(archivo);
        string? line;
        bool isFirst = true;

        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            if (isFirst) { isFirst = false; continue; } // Skipeamos el header

            result.TotalFilas++;
            var parts = line.Split(',');

            if (parts.Length < 3) 
            {
                result.Errores.Add($"Fila {result.TotalFilas}: Formato incorrecto, se esperaban al menos 3 columnas.");
                result.TotalErrores++;
                continue;
            }

            if (DateTime.TryParse(parts[0], out var fecha) && decimal.TryParse(parts[2], out var monto))
            {
                string desc = parts[1];
                string? refNum = parts.Length > 3 ? parts[3] : null;

                // Prevenir duplicados (mismo día exacto, monto y descripción)
                // OJO: En la vida real, los extractos pueden tener transacciones idénticas en el mismo día. 
                // Se suele usar un hash o identificar bien la referenca del banco. 
                // Para el sprint, usamos esta regla simple:
                var existe = (await _extractoRepo.FindAsync(e => 
                    e.EmpresaId == empresaId && 
                    e.CuentaContableId == cuentaId && 
                    e.Fecha.Date == fecha.Date && 
                    e.Monto == monto && 
                    e.Descripcion == desc, ct)).Any();

                if (!existe)
                {
                    var extracto = new ExtractoBancario(empresaId, cuentaId, fecha, desc, monto, refNum);
                    await _extractoRepo.AddAsync(extracto, ct);
                    result.TotalImportadas++;
                }
                else
                {
                    result.Errores.Add($"Fila {result.TotalFilas}: Movimiento duplicado.");
                    result.TotalErrores++;
                }
            }
            else
            {
                result.Errores.Add($"Fila {result.TotalFilas}: Formato de fecha o monto inválido.");
                result.TotalErrores++;
            }
        }
        
        if (result.TotalImportadas > 0)
            await _unitOfWork.SaveChangesAsync(ct);

        return Result<ImportExtractoResultDto>.Success(result);
    }

    public async Task<Result<IEnumerable<SugerenciaConciliacionDto>>> GetSugerenciasAsync(int cuentaId, int periodoId, CancellationToken ct = default)
    {
        var empresaId = ObtenerEmpresaId();

        var periodo = await _periodoRepo.GetByIdAsync(periodoId, ct);
        if (periodo == null || periodo.EmpresaId != empresaId)
            return Result<IEnumerable<SugerenciaConciliacionDto>>.Failure("Período no encontrado.");

        var extractosNoConciliados = await _extractoRepo.FindAsync(e => 
            e.EmpresaId == empresaId && 
            e.CuentaContableId == cuentaId && 
            !e.Conciliado && 
            e.Fecha.Year == periodo.Anio && 
            e.Fecha.Month == periodo.Mes, ct);

        // Obtener asientos del mismo periodo / contablizados
        var asientos = await _asientoRepo.FindAsync(a => 
            a.EmpresaId == empresaId && 
            a.Estado == "Contabilizado" && 
            a.Fecha.Year == periodo.Anio && 
            a.Fecha.Month == periodo.Mes, ct);
        
        var asientoIds = asientos.Select(a => a.AsientoContableId).ToList();
        
        // Obtener líneas que correspondan a la cuenta banco
        var lineasBancariasTodas = await _lineaRepo.FindAsync(l => 
            asientoIds.Contains(l.AsientoContableId) && 
            l.CuentaContableId == cuentaId, ct);

        // Obtener las IDs que ya se conciliaron
        var extractosTodosConciliados = await _extractoRepo.FindAsync(e => e.EmpresaId == empresaId && e.CuentaContableId == cuentaId && e.Conciliado, ct);
        var lineasConciliadasIds = extractosTodosConciliados.Where(e => e.AsientoContableLineaId.HasValue).Select(e => e.AsientoContableLineaId!.Value).ToHashSet();

        var lineasPendientes = lineasBancariasTodas.Where(l => !lineasConciliadasIds.Contains(l.AsientoContableLineaId)).ToList();

        var sugerencias = new List<SugerenciaConciliacionDto>();

        foreach (var ext in extractosNoConciliados)
        {
            foreach (var lin in lineasPendientes)
            {
                var asiento = asientos.First(a => a.AsientoContableId == lin.AsientoContableId);
                var isIngresoBanco = ext.Monto > 0;
                
                // En bancos, un ingreso incrementa el debe (Activo). 
                // Monto neto de la linea, asumiendo Banco es deudora:
                var montoLinea = lin.Debe - lin.Haber; 

                var diffDias = Math.Abs((ext.Fecha.Date - asiento.Fecha.Date).Days);
                var matchExacto = ext.Monto == montoLinea;

                if (matchExacto || diffDias <= 3) // Ejemplo: solo evaluamos hasta 3 días o que el monto pegue.
                {
                    decimal score = 0;
                    if (matchExacto) score += 50;
                    if (diffDias == 0) score += 30;
                    else if (diffDias <= 3) score += (30 - diffDias * 5); // Puntaje descendente

                    // Puntaje adicional si la glosa tiene coincidencias (muy básico)
                    if (!string.IsNullOrEmpty(ext.Descripcion) && !string.IsNullOrEmpty(lin.Glosa) && ext.Descripcion.Contains(lin.Glosa, StringComparison.OrdinalIgnoreCase))
                        score += 20;

                    if (score > 30) // Tiene sentido
                    {
                        sugerencias.Add(new SugerenciaConciliacionDto
                        {
                            ExtractoBancarioId = ext.ExtractoBancarioId,
                            FechaExtracto = ext.Fecha,
                            DescripcionExtracto = ext.Descripcion,
                            MontoExtracto = ext.Monto,
                            AsientoContableLineaId = lin.AsientoContableLineaId,
                            FechaAsiento = asiento.Fecha,
                            GlosaAsiento = lin.Glosa ?? asiento.Glosa,
                            MontoAsiento = montoLinea,
                            DiferenciaDias = diffDias,
                            MatchExactoMonto = matchExacto,
                            Score = score
                        });
                    }
                }
            }
        }

        return Result<IEnumerable<SugerenciaConciliacionDto>>.Success(sugerencias.OrderByDescending(s => s.Score));
    }

    public async Task<Result<bool>> ConciliarAsync(long extractoId, long asientoLineaId, CancellationToken ct = default)
    {
        var empresaId = ObtenerEmpresaId();
        var extracto = await _extractoRepo.GetByIdAsync(extractoId, ct);
        if (extracto == null || extracto.EmpresaId != empresaId)
            return Result<bool>.Failure("Extracto no encontrado.");

        if (extracto.Conciliado)
            return Result<bool>.Failure("El extracto ya está conciliado.");

        var linea = await _lineaRepo.GetByIdAsync(asientoLineaId, ct);
        if (linea == null || linea.CuentaContableId != extracto.CuentaContableId)
            return Result<bool>.Failure("Línea contable no válida o no corresponde a la misma cuenta.");

        // Ojo: obtener periodo del extracto (se asume que se sacó del mes del extracto)
        var periodos = await _periodoRepo.FindAsync(p => p.EmpresaId == empresaId && p.Anio == extracto.Fecha.Year && p.Mes == extracto.Fecha.Month, ct);
        var periodo = periodos.FirstOrDefault();

        if (periodo != null && periodo.Estado == "Cerrado")
            return Result<bool>.Failure("No se puede conciliar en un período cerrado.");

        extracto.Conciliar(asientoLineaId);
        await _extractoRepo.UpdateAsync(extracto, ct);
        
        if (periodo != null)
        {
            await RecalcularConciliacionAsync(empresaId, extracto.CuentaContableId, periodo.PeriodoContableId, ct);
        }
        else 
        {
            await _unitOfWork.SaveChangesAsync(ct); // Si no hay período asociado, al menos guardamos estado de la línea
        }

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> DesconciliarAsync(long extractoId, CancellationToken ct = default)
    {
        var empresaId = ObtenerEmpresaId();
        var extracto = await _extractoRepo.GetByIdAsync(extractoId, ct);
        if (extracto == null || extracto.EmpresaId != empresaId)
            return Result<bool>.Failure("Extracto no encontrado.");

        extracto.Desconciliar();
        await _extractoRepo.UpdateAsync(extracto, ct);

        var periodos = await _periodoRepo.FindAsync(p => p.EmpresaId == empresaId && p.Anio == extracto.Fecha.Year && p.Mes == extracto.Fecha.Month, ct);
        var periodo = periodos.FirstOrDefault();

        if (periodo != null && periodo.Estado == "Cerrado")
            return Result<bool>.Failure("No se puede desconciliar en un período cerrado.");

        if (periodo != null)
            await RecalcularConciliacionAsync(empresaId, extracto.CuentaContableId, periodo.PeriodoContableId, ct);
        else
            await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<ResumenConciliacionDto>> GetResumenAsync(int cuentaId, int periodoId, CancellationToken ct = default)
    {
        var empresaId = ObtenerEmpresaId();
        var periodo = await _periodoRepo.GetByIdAsync(periodoId, ct);
        if (periodo == null || periodo.EmpresaId != empresaId)
            return Result<ResumenConciliacionDto>.Failure("Período no encontrado.");

        var extractos = await _extractoRepo.FindAsync(e => e.EmpresaId == empresaId && e.CuentaContableId == cuentaId && e.Fecha.Year == periodo.Anio && e.Fecha.Month == periodo.Mes, ct);
        decimal saldoExtracto = extractos.Sum(e => e.Monto);

        var asientos = await _asientoRepo.FindAsync(a => a.EmpresaId == empresaId && a.Estado == "Contabilizado" && a.Fecha.Year == periodo.Anio && a.Fecha.Month == periodo.Mes, ct);
        var asientoIds = asientos.Select(a => a.AsientoContableId).ToList();
        var lineas = await _lineaRepo.FindAsync(l => asientoIds.Contains(l.AsientoContableId) && l.CuentaContableId == cuentaId, ct);
        decimal saldoContable = lineas.Sum(l => l.Debe - l.Haber);

        var conciliacion = (await _conciliacionRepo.FindAsync(c => c.EmpresaId == empresaId && c.CuentaContableId == cuentaId && c.PeriodoContableId == periodoId, ct)).FirstOrDefault();

        // Opcional: Reconciliar en read action (lazy) si los saldos no match
        if (conciliacion == null || conciliacion.SaldoContable != saldoContable || conciliacion.SaldoExtracto != saldoExtracto)
        {
            await RecalcularConciliacionAsync(empresaId, cuentaId, periodoId, ct);
            conciliacion = (await _conciliacionRepo.FindAsync(c => c.EmpresaId == empresaId && c.CuentaContableId == cuentaId && c.PeriodoContableId == periodoId, ct)).FirstOrDefault();
        }

        var extractosConciliados = extractos.Count(e => e.Conciliado);
        var pendientes = extractos.Count - extractosConciliados;

        var resumen = new ResumenConciliacionDto
        {
            CuentaContableId = cuentaId,
            PeriodoContableId = periodoId,
            SaldoExtracto = conciliacion?.SaldoExtracto ?? saldoExtracto,
            SaldoContable = conciliacion?.SaldoContable ?? saldoContable,
            Diferencia = conciliacion?.Diferencia ?? (saldoExtracto - saldoContable),
            EstaConciliado = conciliacion?.EstaConciliado ?? (Math.Abs(saldoExtracto - saldoContable) <= 0.01m),
            Estado = conciliacion?.Estado ?? "EnProceso",
            TotalMovimientosExtracto = extractos.Count,
            TotalMovimientosConciliados = extractosConciliados,
            TotalMovimientosPendientes = pendientes
        };

        return Result<ResumenConciliacionDto>.Success(resumen);
    }

    public async Task<Result<bool>> AprobarAsync(int conciliacionId, CancellationToken ct = default)
    {
        var empresaId = ObtenerEmpresaId();
        var conciliacion = await _conciliacionRepo.GetByIdAsync(conciliacionId, ct);
        
        if (conciliacion == null || conciliacion.EmpresaId != empresaId)
            return Result<bool>.Failure("Conciliación no encontrada.");

        var resAprobar = conciliacion.Aprobar();
        if (!resAprobar.IsSuccess)
            return Result<bool>.Failure(resAprobar.Error!);

        await _conciliacionRepo.UpdateAsync(conciliacion, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    private async Task RecalcularConciliacionAsync(int empresaId, int cuentaId, int periodoId, CancellationToken ct)
    {
        var periodo = await _periodoRepo.GetByIdAsync(periodoId, ct);
        if (periodo == null) return;

        var extractos = await _extractoRepo.FindAsync(e => e.EmpresaId == empresaId && e.CuentaContableId == cuentaId && e.Fecha.Year == periodo.Anio && e.Fecha.Month == periodo.Mes, ct);
        decimal saldoExtracto = extractos.Sum(e => e.Monto);

        var asientos = await _asientoRepo.FindAsync(a => a.EmpresaId == empresaId && a.Estado == "Contabilizado" && a.Fecha.Year == periodo.Anio && a.Fecha.Month == periodo.Mes, ct);
        var asientoIds = asientos.Select(a => a.AsientoContableId).ToList();
        var lineas = await _lineaRepo.FindAsync(l => asientoIds.Contains(l.AsientoContableId) && l.CuentaContableId == cuentaId, ct);
        decimal saldoContable = lineas.Sum(l => l.Debe - l.Haber);

        var conciliacion = (await _conciliacionRepo.FindAsync(c => c.EmpresaId == empresaId && c.CuentaContableId == cuentaId && c.PeriodoContableId == periodoId, ct)).FirstOrDefault();
        
        if (conciliacion == null)
        {
            conciliacion = new ConciliacionBancaria(empresaId, cuentaId, periodoId, saldoExtracto, saldoContable);
            await _conciliacionRepo.AddAsync(conciliacion, ct);
        }
        else
        {
            if (conciliacion.Estado == "Aprobado") return; // Prevención extra
            conciliacion.ActualizarSaldos(saldoExtracto, saldoContable);
            await _conciliacionRepo.UpdateAsync(conciliacion, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
