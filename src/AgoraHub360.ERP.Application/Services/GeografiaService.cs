namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Common;

public class GeografiaService : IGeografiaService
{
    private readonly IRepository<Pais> _paisRepo;
    private readonly IRepository<Departamento> _departamentoRepo;
    private readonly IRepository<Provincia> _provinciaRepo;
    private readonly IRepository<Ciudad> _ciudadRepo;
    private readonly IRepository<Zona> _zonaRepo;

    public GeografiaService(
        IRepository<Pais> paisRepo,
        IRepository<Departamento> departamentoRepo,
        IRepository<Provincia> provinciaRepo,
        IRepository<Ciudad> ciudadRepo,
        IRepository<Zona> zonaRepo)
    {
        _paisRepo = paisRepo;
        _departamentoRepo = departamentoRepo;
        _provinciaRepo = provinciaRepo;
        _ciudadRepo = ciudadRepo;
        _zonaRepo = zonaRepo;
    }

    public async Task<Result<IReadOnlyList<GeoCatalogItemDto>>> GetPaisesAsync(CancellationToken ct = default)
    {
        var items = await _paisRepo.FindAsync(x => x.Activo, ct);
        return Result<IReadOnlyList<GeoCatalogItemDto>>.Success(items
            .OrderBy(x => x.Nombre)
            .Select(Map)
            .ToList()
            .AsReadOnly());
    }

    public async Task<Result<IReadOnlyList<GeoCatalogItemDto>>> GetDepartamentosByPaisAsync(Guid paisId, CancellationToken ct = default)
    {
        var items = await _departamentoRepo.FindAsync(x => x.Activo && x.PaisId == paisId, ct);
        return Result<IReadOnlyList<GeoCatalogItemDto>>.Success(items
            .OrderBy(x => x.Nombre)
            .Select(Map)
            .ToList()
            .AsReadOnly());
    }

    public async Task<Result<IReadOnlyList<GeoCatalogItemDto>>> GetProvinciasByDepartamentoAsync(Guid departamentoId, CancellationToken ct = default)
    {
        var items = await _provinciaRepo.FindAsync(x => x.Activo && x.DepartamentoId == departamentoId, ct);
        return Result<IReadOnlyList<GeoCatalogItemDto>>.Success(items
            .OrderBy(x => x.Nombre)
            .Select(Map)
            .ToList()
            .AsReadOnly());
    }

    public async Task<Result<IReadOnlyList<GeoCatalogItemDto>>> GetCiudadesByProvinciaAsync(Guid provinciaId, CancellationToken ct = default)
    {
        var items = await _ciudadRepo.FindAsync(x => x.Activo && x.ProvinciaId == provinciaId, ct);
        return Result<IReadOnlyList<GeoCatalogItemDto>>.Success(items
            .OrderBy(x => x.Nombre)
            .Select(Map)
            .ToList()
            .AsReadOnly());
    }

    public async Task<Result<IReadOnlyList<GeoCatalogItemDto>>> GetZonasByCiudadAsync(Guid ciudadId, CancellationToken ct = default)
    {
        var items = await _zonaRepo.FindAsync(x => x.Activo && x.CiudadId == ciudadId, ct);
        return Result<IReadOnlyList<GeoCatalogItemDto>>.Success(items
            .OrderBy(x => x.Nombre)
            .Select(Map)
            .ToList()
            .AsReadOnly());
    }

    private static GeoCatalogItemDto Map(Pais x) => new() { Id = x.Id, Nombre = x.Nombre };
    private static GeoCatalogItemDto Map(Departamento x) => new() { Id = x.Id, Nombre = x.Nombre };
    private static GeoCatalogItemDto Map(Provincia x) => new() { Id = x.Id, Nombre = x.Nombre };
    private static GeoCatalogItemDto Map(Ciudad x) => new() { Id = x.Id, Nombre = x.Nombre };
    private static GeoCatalogItemDto Map(Zona x) => new() { Id = x.Id, Nombre = x.Nombre };
}
