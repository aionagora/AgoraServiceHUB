namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GestionContableContextService : IGestionContableContextService
{
    private readonly IRepository<PeriodoContable> _periodoRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly ICierreContableService _cierreService;

    public GestionContableContextService(
        IRepository<PeriodoContable> periodoRepo,
        ICurrentUserService currentUser,
        ICierreContableService cierreService)
    {
        _periodoRepo = periodoRepo;
        _currentUser = currentUser;
        _cierreService = cierreService;
    }

    public async Task<Result<IEnumerable<GestionContableDto>>> GetGestionesDisponiblesAsync(CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IEnumerable<GestionContableDto>>.Failure("No hay empresa activa.");

        var periodos = await _periodoRepo.FindAsync(p => p.EmpresaId == empresaId.Value && p.Activo, ct);
        
        var anios = periodos.Select(p => p.Anio).Distinct().OrderByDescending(a => a).ToList();
        var lista = new List<GestionContableDto>();

        var anioActual = DateTime.Today.Year;

        foreach (var anio in anios)
        {
            var pAnio = periodos.Where(p => p.Anio == anio).ToList();
            var estaCerrada = await _cierreService.ExisteCierreAsync(empresaId.Value, anio, ct);
            
            lista.Add(new GestionContableDto
            {
                EmpresaId = empresaId.Value,
                GestionId = anio,
                Anio = anio,
                Nombre = $"Gestión {anio}",
                Activa = false,
                EsGestionEnCurso = anio == anioActual,
                FechaInicio = new DateTime(anio, 1, 1),
                FechaFin = new DateTime(anio, 12, 31),
                Estado = estaCerrada ? "Cerrada" : "Abierta"
            });
        }

        return Result<IEnumerable<GestionContableDto>>.Success(lista);
    }

    public async Task<Result<GestionContableDto?>> GetGestionSugeridaAsync(CancellationToken ct = default)
    {
        var rGestiones = await GetGestionesDisponiblesAsync(ct);
        if (!rGestiones.IsSuccess) return Result<GestionContableDto?>.Failure(rGestiones.Error);

        var gestiones = rGestiones.Value!.ToList();
        if (gestiones.Count == 0)
            return Result<GestionContableDto?>.Success(null);

        var anioActual = DateTime.Today.Year;
        
        // Return current if exists
        var enCurso = gestiones.FirstOrDefault(g => g.Anio == anioActual);
        if (enCurso != null)
            return Result<GestionContableDto?>.Success(enCurso);

        // Fallback to highest open or max year
        var abierta = gestiones.FirstOrDefault(g => g.Estado != "Cerrada");
        if (abierta != null)
            return Result<GestionContableDto?>.Success(abierta);

        return Result<GestionContableDto?>.Success(gestiones.FirstOrDefault());
    }

    public async Task<Result> ValidarGestionAsync(int anio, CancellationToken ct = default)
    {
        var rGestiones = await GetGestionesDisponiblesAsync(ct);
        if (!rGestiones.IsSuccess) return Result.Failure(rGestiones.Error);

        var existe = rGestiones.Value!.Any(g => g.Anio == anio);
        if (!existe)
            return Result.Failure($"La gestión {anio} no existe o no pertenece a la empresa activa.");

        return Result.Success();
    }
}
