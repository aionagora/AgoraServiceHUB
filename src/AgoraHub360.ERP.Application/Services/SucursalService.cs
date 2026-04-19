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
        var sucursales = await _sucursalRepository.FindIgnoreQueryFiltersAsync(s => s.EmpresaId == empresaId, ct);

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
        var sucursal = await _sucursalRepository.GetByIdIgnoreQueryFiltersAsync(id, ct);
        if (sucursal == null)
        {
            return Result<SucursalDto>.Failure("Sucursal no encontrada.");
        }

        var dto = MapToDto(sucursal);
        return Result<SucursalDto>.Success(dto);
    }

    public async Task<Result<SucursalDto>> CreateAsync(CrearSucursalDto dto, CancellationToken ct = default)
    {
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
            CodigoInterno = dto.CodigoInterno,
            Sigla = dto.Sigla,
            Descripcion = dto.Descripcion,
            PaisId = dto.PaisId,
            DepartamentoId = dto.DepartamentoId,
            ProvinciaId = dto.ProvinciaId,
            CiudadId = dto.CiudadId,
            ZonaId = dto.ZonaId,
            Pais = dto.Pais,
            Departamento = dto.Departamento,
            Provincia = dto.Provincia,
            Zona = dto.Zona,
            Referencia = dto.Referencia,
            Latitud = dto.Latitud,
            Longitud = dto.Longitud,
            UrlMapa = dto.UrlMapa,
            Direccion = dto.Direccion,
            Ciudad = dto.Ciudad,
            ResponsableNombre = dto.ResponsableNombre,
            ResponsableCargo = dto.ResponsableCargo,
            Celular = dto.Celular,
            WhatsApp = dto.WhatsApp,
            EmailAlternativo = dto.EmailAlternativo,
            Telefono = dto.Telefono,
            Email = dto.Email,
            EsCentral = dto.EsCentral,
            PermiteVentas = dto.PermiteVentas,
            PermiteCompras = dto.PermiteCompras,
            PermiteInventario = dto.PermiteInventario,
            PermiteDespacho = dto.PermiteDespacho,
            PermiteFacturacion = dto.PermiteFacturacion,
            ManejaAlmacen = dto.ManejaAlmacen,
            CodigoSucursalFiscal = dto.CodigoSucursalFiscal,
            PrefijoDocumental = dto.PrefijoDocumental,
            Observaciones = dto.Observaciones,
            Activo = dto.Activo
        };

        var sucursal = await _sucursalRepository.AddAsync(entidad, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<SucursalDto>.Success(MapToDto(sucursal));
    }

    public async Task<Result<SucursalDto>> UpdateAsync(int id, ActualizarSucursalDto dto, CancellationToken ct = default)
    {
        var sucursal = await _sucursalRepository.GetByIdIgnoreQueryFiltersAsync(id, ct);
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
        sucursal.CodigoInterno = dto.CodigoInterno;
        sucursal.Sigla = dto.Sigla;
        sucursal.Descripcion = dto.Descripcion;
        sucursal.PaisId = dto.PaisId;
        sucursal.DepartamentoId = dto.DepartamentoId;
        sucursal.ProvinciaId = dto.ProvinciaId;
        sucursal.CiudadId = dto.CiudadId;
        sucursal.ZonaId = dto.ZonaId;
        sucursal.Pais = dto.Pais;
        sucursal.Departamento = dto.Departamento;
        sucursal.Provincia = dto.Provincia;
        sucursal.Zona = dto.Zona;
        sucursal.Referencia = dto.Referencia;
        sucursal.Latitud = dto.Latitud;
        sucursal.Longitud = dto.Longitud;
        sucursal.UrlMapa = dto.UrlMapa;
        sucursal.Direccion = dto.Direccion;
        sucursal.Ciudad = dto.Ciudad;
        sucursal.ResponsableNombre = dto.ResponsableNombre;
        sucursal.ResponsableCargo = dto.ResponsableCargo;
        sucursal.Celular = dto.Celular;
        sucursal.WhatsApp = dto.WhatsApp;
        sucursal.EmailAlternativo = dto.EmailAlternativo;
        sucursal.Telefono = dto.Telefono;
        sucursal.Email = dto.Email;
        sucursal.EsCentral = dto.EsCentral;
        sucursal.PermiteVentas = dto.PermiteVentas;
        sucursal.PermiteCompras = dto.PermiteCompras;
        sucursal.PermiteInventario = dto.PermiteInventario;
        sucursal.PermiteDespacho = dto.PermiteDespacho;
        sucursal.PermiteFacturacion = dto.PermiteFacturacion;
        sucursal.ManejaAlmacen = dto.ManejaAlmacen;
        sucursal.CodigoSucursalFiscal = dto.CodigoSucursalFiscal;
        sucursal.PrefijoDocumental = dto.PrefijoDocumental;
        sucursal.Observaciones = dto.Observaciones;
        sucursal.Activo = dto.Activo;

        await _sucursalRepository.UpdateAsync(sucursal, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<SucursalDto>.Success(MapToDto(sucursal));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var sucursal = await _sucursalRepository.GetByIdIgnoreQueryFiltersAsync(id, ct);
        if (sucursal == null)
        {
            return Result<bool>.Failure("Sucursal no encontrada.");
        }

        if (sucursal.EsCentral)
        {
            return Result<bool>.Failure("No se puede eliminar la sucursal central.");
        }

        sucursal.Activo = false;
        await _sucursalRepository.UpdateAsync(sucursal, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default)
    {
        var sucursal = await _sucursalRepository.GetByIdIgnoreQueryFiltersAsync(id, ct);
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
        var sucursal = await _sucursalRepository.GetByIdIgnoreQueryFiltersAsync(id, ct);
        if (sucursal == null)
            return Result<bool>.Failure("Sucursal no encontrada.");

        if (sucursal.EsCentral)
            return Result<bool>.Success(true);

        if (!sucursal.Activo)
            return Result<bool>.Failure("No se puede establecer como central una sucursal inactiva.");

        var sucursalesEmpresa = await _sucursalRepository.FindIgnoreQueryFiltersAsync(s => s.EmpresaId == sucursal.EmpresaId, ct);

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
        var sucursalesEmpresa = await _sucursalRepository.FindIgnoreQueryFiltersAsync(s => s.EmpresaId == empresaId, ct);

        if (sucursalIdExcluida.HasValue)
        {
            sucursalesEmpresa = sucursalesEmpresa.Where(s => s.Id != sucursalIdExcluida.Value).ToList().AsReadOnly();
        }

        if (sucursalesEmpresa.Any(s => s.Nombre.Equals(nombre, System.StringComparison.OrdinalIgnoreCase)))
        {
            return Result<bool>.Failure("Ya existe una sucursal con el mismo nombre en esta empresa.");
        }

        if (!string.IsNullOrWhiteSpace(codigo) && 
            sucursalesEmpresa.Any(s => s.Codigo == codigo))
        {
            return Result<bool>.Failure("Ya existe una sucursal con el mismo código en esta empresa.");
        }

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
            CodigoInterno = sucursal.CodigoInterno,
            Sigla = sucursal.Sigla,
            Descripcion = sucursal.Descripcion,
            PaisId = sucursal.PaisId,
            DepartamentoId = sucursal.DepartamentoId,
            ProvinciaId = sucursal.ProvinciaId,
            CiudadId = sucursal.CiudadId,
            ZonaId = sucursal.ZonaId,
            Pais = sucursal.Pais,
            Departamento = sucursal.Departamento,
            Provincia = sucursal.Provincia,
            Zona = sucursal.Zona,
            Referencia = sucursal.Referencia,
            Latitud = sucursal.Latitud,
            Longitud = sucursal.Longitud,
            UrlMapa = sucursal.UrlMapa,
            Direccion = sucursal.Direccion,
            Ciudad = sucursal.Ciudad,
            ResponsableNombre = sucursal.ResponsableNombre,
            ResponsableCargo = sucursal.ResponsableCargo,
            Celular = sucursal.Celular,
            WhatsApp = sucursal.WhatsApp,
            EmailAlternativo = sucursal.EmailAlternativo,
            Telefono = sucursal.Telefono,
            Email = sucursal.Email,
            EsCentral = sucursal.EsCentral,
            PermiteVentas = sucursal.PermiteVentas,
            PermiteCompras = sucursal.PermiteCompras,
            PermiteInventario = sucursal.PermiteInventario,
            PermiteDespacho = sucursal.PermiteDespacho,
            PermiteFacturacion = sucursal.PermiteFacturacion,
            ManejaAlmacen = sucursal.ManejaAlmacen,
            CodigoSucursalFiscal = sucursal.CodigoSucursalFiscal,
            PrefijoDocumental = sucursal.PrefijoDocumental,
            Observaciones = sucursal.Observaciones,
            Activo = sucursal.Activo,
            FechaCreacion = sucursal.FechaCreacion
        };
    }
}
