namespace AgoraHub360.ERP.Application.Services;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Sucursal;

public class SucursalService : ISucursalService
{
    private readonly IRepository<Sucursal> _sucursalRepository;
    private readonly IRepository<Empresa> _empresaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SucursalService(
        IRepository<Sucursal> sucursalRepository,
        IRepository<Empresa> empresaRepository,
        IUnitOfWork unitOfWork)
    {
        _sucursalRepository = sucursalRepository;
        _empresaRepository = empresaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<SucursalListadoDto>>> GetAllByEmpresaAsync(int empresaId, CancellationToken ct = default)
    {
        var sucursales = await _sucursalRepository.FindAsync(s => s.EmpresaId == empresaId, ct);

        var dtos = sucursales.Select(s => new SucursalListadoDto
        {
            Id = s.Id,
            EmpresaId = s.EmpresaId,
            Nombre = s.Nombre,
            Codigo = s.Codigo,
            Ciudad = s.Ciudad,
            EsCentral = s.EsCentral,
            Activo = s.Activo
        }).ToList().AsReadOnly();

        return Result<IReadOnlyList<SucursalListadoDto>>.Success(dtos);
    }

    public async Task<Result<SucursalDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var sucursal = await _sucursalRepository.GetByIdAsync(id, ct);
        if (sucursal == null)
        {
            return Result<SucursalDto>.Failure("Sucursal no encontrada.");
        }

        var dto = MapToDto(sucursal);
        return Result<SucursalDto>.Success(dto);
    }

    public async Task<Result<SucursalDto>> CreateAsync(CrearSucursalDto dto, CancellationToken ct = default)
    {
        // Validar empresa existente
        var empresa = await _empresaRepository.GetByIdAsync(dto.EmpresaId, ct);
        if (empresa == null)
        {
            return Result<SucursalDto>.Failure("La empresa especificada no existe.");
        }

        var resultValidacion = await ValidarReglasNegocioAsync(dto.EmpresaId, dto.Nombre, dto.Codigo, dto.EsCentral, null, ct);
        if (!resultValidacion.IsSuccess)
        {
            return Result<SucursalDto>.Failure(resultValidacion.Error!);
        }

        var entidad = new Sucursal
        {
            EmpresaId = dto.EmpresaId,
            Nombre = dto.Nombre,
            Codigo = dto.Codigo,
            Direccion = dto.Direccion,
            Ciudad = dto.Ciudad,
            Telefono = dto.Telefono,
            Email = dto.Email,
            EsCentral = dto.EsCentral,
            Activo = dto.Activo
        };

        var sucursal = await _sucursalRepository.AddAsync(entidad, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<SucursalDto>.Success(MapToDto(sucursal));
    }

    public async Task<Result<SucursalDto>> UpdateAsync(int id, ActualizarSucursalDto dto, CancellationToken ct = default)
    {
        var sucursal = await _sucursalRepository.GetByIdAsync(id, ct);
        if (sucursal == null)
        {
            return Result<SucursalDto>.Failure("Sucursal no encontrada.");
        }

        var resultValidacion = await ValidarReglasNegocioAsync(sucursal.EmpresaId, dto.Nombre, dto.Codigo, dto.EsCentral, id, ct);
        if (!resultValidacion.IsSuccess)
        {
            return Result<SucursalDto>.Failure(resultValidacion.Error!);
        }

        sucursal.Nombre = dto.Nombre;
        sucursal.Codigo = dto.Codigo;
        sucursal.Direccion = dto.Direccion;
        sucursal.Ciudad = dto.Ciudad;
        sucursal.Telefono = dto.Telefono;
        sucursal.Email = dto.Email;
        sucursal.EsCentral = dto.EsCentral;
        sucursal.Activo = dto.Activo;

        await _sucursalRepository.UpdateAsync(sucursal, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<SucursalDto>.Success(MapToDto(sucursal));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var sucursal = await _sucursalRepository.GetByIdAsync(id, ct);
        if (sucursal == null)
        {
            return Result<bool>.Failure("Sucursal no encontrada.");
        }

        if (sucursal.EsCentral)
        {
            return Result<bool>.Failure("No se puede eliminar la sucursal central.");
        }

        // Eliminación lógica seteando Activo = false
        sucursal.Activo = false;
        await _sucursalRepository.UpdateAsync(sucursal, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default)
    {
        var sucursal = await _sucursalRepository.GetByIdAsync(id, ct);
        if (sucursal == null)
            return Result<bool>.Failure("Sucursal no encontrada.");

        if (sucursal.EsCentral && !activo)
            return Result<bool>.Failure("No se puede desactivar la sucursal central.");

        sucursal.Activo = activo;
        await _sucursalRepository.UpdateAsync(sucursal, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> EstablecerCentralAsync(int id, CancellationToken ct = default)
    {
        var sucursal = await _sucursalRepository.GetByIdAsync(id, ct);
        if (sucursal == null)
            return Result<bool>.Failure("Sucursal no encontrada.");

        if (sucursal.EsCentral)
            return Result<bool>.Success(true); // Ya es central

        if (!sucursal.Activo)
            return Result<bool>.Failure("No se puede establecer como central una sucursal inactiva.");

        // Obtener todas las sucursales de la empresa
        var sucursalesEmpresa = await _sucursalRepository.FindAsync(s => s.EmpresaId == sucursal.EmpresaId, ct);

        foreach(var s in sucursalesEmpresa.Where(x => x.EsCentral))
        {
            s.EsCentral = false;
            await _sucursalRepository.UpdateAsync(s, ct);
        }

        sucursal.EsCentral = true;
        await _sucursalRepository.UpdateAsync(sucursal, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> ValidarReglasNegocioAsync(
        int empresaId, 
        string nombre, 
        string? codigo, 
        bool esCentral, 
        int? sucursalIdExcluida, 
        CancellationToken ct)
    {
        var sucursalesEmpresa = await _sucursalRepository.FindAsync(s => s.EmpresaId == empresaId, ct);

        if (sucursalIdExcluida.HasValue)
        {
            sucursalesEmpresa = sucursalesEmpresa.Where(s => s.Id != sucursalIdExcluida.Value).ToList();
        }

        // Validar duplicados de nombre
        if (sucursalesEmpresa.Any(s => s.Nombre.Equals(nombre, System.StringComparison.OrdinalIgnoreCase)))
        {
            return Result<bool>.Failure("Ya existe una sucursal con el mismo nombre en esta empresa.");
        }

        // Validar duplicados de código
        if (!string.IsNullOrWhiteSpace(codigo) && 
            sucursalesEmpresa.Any(s => s.Codigo == codigo))
        {
            return Result<bool>.Failure("Ya existe una sucursal con el mismo código en esta empresa.");
        }

        // Validar única sucursal principal
        if (esCentral && sucursalesEmpresa.Any(s => s.EsCentral))
        {
            return Result<bool>.Failure("Ya existe una sucursal central para esta empresa. Solo puede haber una principal.");
        }

        return Result<bool>.Success(true);
    }

    private static SucursalDto MapToDto(Sucursal sucursal)
    {
        return new SucursalDto
        {
            Id = sucursal.Id,
            EmpresaId = sucursal.EmpresaId,
            Nombre = sucursal.Nombre,
            Codigo = sucursal.Codigo,
            Direccion = sucursal.Direccion,
            Ciudad = sucursal.Ciudad,
            Telefono = sucursal.Telefono,
            Email = sucursal.Email,
            EsCentral = sucursal.EsCentral,
            Activo = sucursal.Activo,
            FechaCreacion = sucursal.FechaCreacion
        };
    }
}
