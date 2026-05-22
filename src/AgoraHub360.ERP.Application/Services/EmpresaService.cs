namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Empresa;

public class EmpresaService : IEmpresaService
{
    private readonly IRepository<Empresa> _repository;
    private readonly IRepository<Sucursal> _sucursalRepository;
    private readonly IRepository<AgoraHub360.ERP.Domain.Entities.MDM.Almacen> _almacenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmpresaSeedService _seedService;

    public EmpresaService(
        IRepository<Empresa> repository, 
        IRepository<Sucursal> sucursalRepository,
        IRepository<AgoraHub360.ERP.Domain.Entities.MDM.Almacen> almacenRepository,
        IUnitOfWork unitOfWork, 
        IEmpresaSeedService seedService)
    {
        _repository = repository;
        _sucursalRepository = sucursalRepository;
        _almacenRepository = almacenRepository;
        _unitOfWork = unitOfWork;
        _seedService = seedService;
    }

    public async Task<Result<IReadOnlyList<EmpresaDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var empresas = await _repository.GetAllAsync(ct);
        var dtos = empresas.Select(MapToDto).ToList().AsReadOnly();
        return Result<IReadOnlyList<EmpresaDto>>.Success(dtos);
    }

    public async Task<Result<EmpresaDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var empresa = await _repository.GetByIdAsync(id, ct);
        if (empresa is null)
            return Result<EmpresaDto>.Failure($"Empresa con Id {id} no encontrada.");

        return Result<EmpresaDto>.Success(MapToDto(empresa));
    }

    public async Task<Result<EmpresaDto>> CreateAsync(CreateEmpresaDto dto, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(dto.NIT))
        {
            var existing = await _repository.FindAsync(e => e.NIT == dto.NIT, ct);
            if (existing.Count > 0)
                return Result<EmpresaDto>.Failure($"Ya existe una empresa con NIT '{dto.NIT}'.");
        }

        var empresa = new Empresa
        {
            Nombre = dto.Nombre,
            NIT = dto.NIT,
            Direccion = dto.Direccion,
            Telefono = dto.Telefono,
            Email = dto.Email,
            MonedaBaseId = string.IsNullOrWhiteSpace(dto.MonedaBaseId) ? null : dto.MonedaBaseId,
            Activo = true
        };

        await _repository.AddAsync(empresa, ct);
        // Guardamos primero para obtener el Id de la Empresa
        await _unitOfWork.SaveChangesAsync(ct);

        // FASE 4: Crear Sucursal Principal y Almacén Principal por defecto
        var sucursalPrincipal = new Sucursal
        {
            EmpresaId = empresa.Id,
            Codigo = "SUC01",
            Nombre = "Sucursal Principal",
            EsCentral = true,
            PermiteVentas = true,
            PermiteCompras = true,
            PermiteInventario = true,
            PermiteDespacho = true,
            PermiteFacturacion = true,
            ManejaAlmacen = true,
            Activo = true
        };

        await _sucursalRepository.AddAsync(sucursalPrincipal, ct);
        await _unitOfWork.SaveChangesAsync(ct); // Guardar para obtener SucursalId

        var almacenPrincipal = new AgoraHub360.ERP.Domain.Entities.MDM.Almacen
        {
            EmpresaId = empresa.Id,
            SucursalId = sucursalPrincipal.Id,
            Codigo = "ALM-SUC01",
            Nombre = "Almacén Principal",
            Activo = true
        };

        await _almacenRepository.AddAsync(almacenPrincipal, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Seed default MDM data for the new company
        await _seedService.SeedDefaultDataAsync(empresa.Id, ct);

        return Result<EmpresaDto>.Success(MapToDto(empresa));
    }

    public async Task<Result<EmpresaDto>> UpdateAsync(int id, UpdateEmpresaDto dto, CancellationToken ct = default)
    {
        var empresa = await _repository.GetByIdAsync(id, ct);
        if (empresa is null)
            return Result<EmpresaDto>.Failure($"Empresa con Id {id} no encontrada.");

        if (!string.IsNullOrWhiteSpace(dto.NIT))
        {
            var existing = await _repository.FindAsync(e => e.NIT == dto.NIT && e.Id != id, ct);
            if (existing.Count > 0)
                return Result<EmpresaDto>.Failure($"Ya existe otra empresa con NIT '{dto.NIT}'.");
        }

        empresa.Nombre = dto.Nombre;
        empresa.NIT = dto.NIT;
        empresa.Direccion = dto.Direccion;
        empresa.Telefono = dto.Telefono;
        empresa.Email = dto.Email;
        empresa.MonedaBaseId = string.IsNullOrWhiteSpace(dto.MonedaBaseId) ? null : dto.MonedaBaseId;
        empresa.Activo = dto.Activo;

        await _repository.UpdateAsync(empresa, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<EmpresaDto>.Success(MapToDto(empresa));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var empresa = await _repository.GetByIdAsync(id, ct);
        if (empresa is null)
            return Result<bool>.Failure($"Empresa con Id {id} no encontrada.");

        await _repository.DeleteAsync(empresa, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    private static EmpresaDto MapToDto(Empresa e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        NIT = e.NIT,
        Direccion = e.Direccion,
        Telefono = e.Telefono,
        Email = e.Email,
        MonedaBaseId = e.MonedaBaseId,
        Activo = e.Activo,
        FechaCreacion = e.FechaCreacion
    };
}
