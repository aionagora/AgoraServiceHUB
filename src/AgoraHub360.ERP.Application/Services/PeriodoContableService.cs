namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using Microsoft.Extensions.Logging;
using System.Globalization;

public class PeriodoContableService : IPeriodoContableService
{
    private readonly IRepository<PeriodoContable> _periodoRepo;
    private readonly IRepository<AsientoContable> _asientoRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IPeriodoContableNotificacionService _notificacionPeriodo;
    private readonly ILogger<PeriodoContableService> _logger;

    private static readonly string[] NombresMes =
    {
        "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
        "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
    };

    public PeriodoContableService(
        IRepository<PeriodoContable> periodoRepo,
        IRepository<AsientoContable> asientoRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IPeriodoContableNotificacionService notificacionPeriodo,
        ILogger<PeriodoContableService> logger)
    {
        _periodoRepo = periodoRepo;
        _asientoRepo = asientoRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notificacionPeriodo = notificacionPeriodo;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<PeriodoContableDto>>> GetAllAsync(int? anio, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<PeriodoContableDto>>.Failure("No active company.");

        var periodos = await _periodoRepo.FindAsync(
            p => p.EmpresaId == empresaId.Value && p.Activo
                && (!anio.HasValue || p.Anio == anio.Value), ct);

        // Get asiento counts per period
        var asientos = await _asientoRepo.FindAsync(
            a => a.EmpresaId == empresaId.Value && a.Activo && a.Estado == "Contabilizado"
                && (!anio.HasValue || a.Fecha.Year == anio.Value), ct);

        var asientosPorMes = asientos
            .GroupBy(a => new { a.Fecha.Year, a.Fecha.Month })
            .ToDictionary(g => g.Key, g => g.Count());

        var dtos = periodos
            .OrderBy(p => p.Anio).ThenBy(p => p.Mes)
            .Select(p =>
            {
                asientosPorMes.TryGetValue(new { Year = p.Anio, Month = p.Mes }, out var count);
                return MapToDto(p, count);
            })
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<PeriodoContableDto>>.Success(dtos);
    }

    public async Task<Result<int>> GenerarPeriodosAsync(int anio, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<int>.Failure("No active company.");

        if (anio < 2000 || anio > 2100)
            return Result<int>.Failure("Invalid year. Must be between 2000 and 2100.");

        // Check existing
        var existentes = await _periodoRepo.FindAsync(
            p => p.EmpresaId == empresaId.Value && p.Anio == anio && p.Activo, ct);
        if (existentes.Count > 0)
            return Result<int>.Failure($"Periods for year {anio} already exist. Found {existentes.Count} period(s).");

        for (int mes = 1; mes <= 12; mes++)
        {
            var periodo = new PeriodoContable
            {
                EmpresaId = empresaId.Value,
                Anio = anio,
                Mes = mes,
                Nombre = $"{NombresMes[mes]} {anio}",
                Estado = "Abierto",
                Activo = true
            };
            await _periodoRepo.AddAsync(periodo, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<int>.Success(12);
    }

    public async Task<Result<PeriodoContableDto>> CerrarAsync(int id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<PeriodoContableDto>.Failure("No active company.");

        var periodo = await _periodoRepo.GetByIdAsync(id, ct);
        if (periodo is null || periodo.EmpresaId != empresaId.Value || !periodo.Activo)
            return Result<PeriodoContableDto>.Failure("Period not found.");

        if (periodo.Estado == "Cerrado")
            return Result<PeriodoContableDto>.Failure("Period is already closed.");

        // Check for draft entries in this period
        var borradores = await _asientoRepo.FindAsync(
            a => a.EmpresaId == empresaId.Value && a.Activo
                && a.Estado == "Borrador"
                && a.Fecha.Year == periodo.Anio && a.Fecha.Month == periodo.Mes, ct);

        if (borradores.Count > 0)
            return Result<PeriodoContableDto>.Failure(
                $"El período '{periodo.Nombre}' tiene {borradores.Count} asiento(s) en Borrador. " +
                "Contabilícelos o elimínelos antes de cerrar el período.");

        periodo.Estado = "Cerrado";
        periodo.FechaCierre = DateTime.UtcNow;
        periodo.CerradoPorId = _currentUser.UserIdInt;
        periodo.CerradoPorNombre = _currentUser.UserName;
        await _periodoRepo.UpdateAsync(periodo, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var asientos = await _asientoRepo.FindAsync(
            a => a.EmpresaId == empresaId.Value && a.Activo && a.Estado == "Contabilizado"
                && a.Fecha.Year == periodo.Anio && a.Fecha.Month == periodo.Mes, ct);

        try
        {
            await _notificacionPeriodo.VerificarPeriodosProximosAsync(empresaId.Value, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al verificar períodos próximos tras cerrar período {PeriodoId} (EmpresaId={EmpresaId}).",
                id, empresaId.Value);
        }

        return Result<PeriodoContableDto>.Success(MapToDto(periodo, asientos.Count));
    }

    public async Task<Result<PeriodoContableDto>> ReabrirAsync(int id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<PeriodoContableDto>.Failure("No active company.");

        var periodo = await _periodoRepo.GetByIdAsync(id, ct);
        if (periodo is null || periodo.EmpresaId != empresaId.Value || !periodo.Activo)
            return Result<PeriodoContableDto>.Failure("Period not found.");

        if (periodo.Estado == "Abierto")
            return Result<PeriodoContableDto>.Failure("Period is already open.");

        periodo.Estado = "Abierto";
        periodo.FechaCierre = null;
        periodo.CerradoPorId = null;
        periodo.CerradoPorNombre = null;
        await _periodoRepo.UpdateAsync(periodo, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        try
        {
            await _notificacionPeriodo.VerificarPeriodosProximosAsync(empresaId.Value, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al verificar períodos próximos tras reabrir período {PeriodoId} (EmpresaId={EmpresaId}).",
                id, empresaId.Value);
        }

        return Result<PeriodoContableDto>.Success(MapToDto(periodo, 0));
    }

    private static PeriodoContableDto MapToDto(PeriodoContable p, int cantidadAsientos) => new()
    {
        PeriodoContableId = p.PeriodoContableId,
        EmpresaId = p.EmpresaId,
        Anio = p.Anio,
        Mes = p.Mes,
        Nombre = p.Nombre,
        Estado = p.Estado,
        FechaCierre = p.FechaCierre,
        CerradoPorId = p.CerradoPorId,
        CerradoPorNombre = p.CerradoPorNombre,
        CantidadAsientos = cantidadAsientos
    };
}
