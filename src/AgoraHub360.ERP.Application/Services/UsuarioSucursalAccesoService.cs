namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Seguridad;

public class UsuarioSucursalAccesoService : IUsuarioSucursalAccesoService
{
    private readonly IRepository<UsuarioSucursalAcceso> _accesoRepository;
    private readonly IRepository<UsuarioEmpresa> _usuarioEmpresaRepository;
    private readonly IRepository<Sucursal> _sucursalRepository;
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UsuarioSucursalAccesoService(
        IRepository<UsuarioSucursalAcceso> accesoRepository,
        IRepository<UsuarioEmpresa> usuarioEmpresaRepository,
        IRepository<Sucursal> sucursalRepository,
        IRepository<Usuario> usuarioRepository,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _accesoRepository = accesoRepository;
        _usuarioEmpresaRepository = usuarioEmpresaRepository;
        _sucursalRepository = sucursalRepository;
        _usuarioRepository = usuarioRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<UsuarioSucursalAccesoDto>>> GetByUsuarioAsync(int usuarioId, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<IReadOnlyList<UsuarioSucursalAccesoDto>>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;
        var usuarioOk = await UsuarioPerteneceAEmpresaAsync(usuarioId, empresaId, ct);
        if (!usuarioOk)
            return Result<IReadOnlyList<UsuarioSucursalAccesoDto>>.Failure("El usuario no pertenece a la empresa activa.");

        var accesos = await _accesoRepository.FindAsync(x => x.UsuarioId == usuarioId && x.EmpresaId == empresaId, ct);
        var dtos = accesos
            .OrderByDescending(x => x.EsPredeterminada)
            .ThenBy(x => x.SucursalId)
            .Select(MapToDto)
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<UsuarioSucursalAccesoDto>>.Success(dtos);
    }

    public async Task<Result<UsuarioSucursalAccesoDto>> UpsertAsync(int usuarioId, UpsertUsuarioSucursalAccesoDto dto, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<UsuarioSucursalAccesoDto>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;
        var usuarioOk = await UsuarioPerteneceAEmpresaAsync(usuarioId, empresaId, ct);
        if (!usuarioOk)
            return Result<UsuarioSucursalAccesoDto>.Failure("El usuario no pertenece a la empresa activa.");

        var sucursal = await _sucursalRepository.GetByIdAsync(dto.SucursalId, ct);
        if (sucursal is null || sucursal.EmpresaId != empresaId)
            return Result<UsuarioSucursalAccesoDto>.Failure("Sucursal no válida para la empresa activa.");

        var existente = (await _accesoRepository.FindAsync(x =>
            x.UsuarioId == usuarioId &&
            x.EmpresaId == empresaId &&
            x.SucursalId == dto.SucursalId, ct)).FirstOrDefault();

        if (dto.EsPredeterminada)
            await ClearPredeterminadaAsync(usuarioId, empresaId, dto.SucursalId, ct);

        if (existente is null)
        {
            existente = new UsuarioSucursalAcceso
            {
                UsuarioId = usuarioId,
                EmpresaId = empresaId,
                SucursalId = dto.SucursalId,
                EsPredeterminada = dto.EsPredeterminada,
                PuedeConsultar = dto.PuedeConsultar,
                PuedeOperar = dto.PuedeOperar,
                Activo = dto.Activo
            };

            await _accesoRepository.AddAsync(existente, ct);
        }
        else
        {
            existente.EsPredeterminada = dto.EsPredeterminada;
            existente.PuedeConsultar = dto.PuedeConsultar;
            existente.PuedeOperar = dto.PuedeOperar;
            existente.Activo = dto.Activo;
            await _accesoRepository.UpdateAsync(existente, ct);
        }

        if (existente.Activo && !HasDefault(await _accesoRepository.FindAsync(x => x.UsuarioId == usuarioId && x.EmpresaId == empresaId, ct), existente))
        {
            existente.EsPredeterminada = true;
            await _accesoRepository.UpdateAsync(existente, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<UsuarioSucursalAccesoDto>.Success(MapToDto(existente));
    }

    public async Task<Result<bool>> SetPredeterminadaAsync(int usuarioId, int sucursalId, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<bool>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;
        var acceso = (await _accesoRepository.FindAsync(x =>
            x.UsuarioId == usuarioId && x.EmpresaId == empresaId && x.SucursalId == sucursalId && x.Activo, ct)).FirstOrDefault();

        if (acceso is null)
            return Result<bool>.Failure("El usuario no tiene acceso activo a la sucursal indicada.");

        await ClearPredeterminadaAsync(usuarioId, empresaId, sucursalId, ct);
        acceso.EsPredeterminada = true;
        await _accesoRepository.UpdateAsync(acceso, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> RemoveAsync(int usuarioId, int sucursalId, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<bool>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;
        var acceso = (await _accesoRepository.FindAsync(x =>
            x.UsuarioId == usuarioId && x.EmpresaId == empresaId && x.SucursalId == sucursalId, ct)).FirstOrDefault();

        if (acceso is null)
            return Result<bool>.Failure("Acceso no encontrado.");

        acceso.Activo = false;
        var eraPredeterminada = acceso.EsPredeterminada;
        acceso.EsPredeterminada = false;
        await _accesoRepository.UpdateAsync(acceso, ct);

        if (eraPredeterminada)
        {
            var reemplazo = (await _accesoRepository.FindAsync(x =>
                x.UsuarioId == usuarioId && x.EmpresaId == empresaId && x.Activo && x.SucursalId != sucursalId, ct))
                .OrderBy(x => x.Id)
                .FirstOrDefault();

            if (reemplazo is not null)
            {
                reemplazo.EsPredeterminada = true;
                await _accesoRepository.UpdateAsync(reemplazo, ct);
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<ValidarSucursalAccesoDto>> ValidarAccesoActualAsync(int sucursalId, bool requiereOperacion, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<ValidarSucursalAccesoDto>.Failure(empresaIdResult.Error!);

        if (!_currentUser.UserIdInt.HasValue)
            return Result<ValidarSucursalAccesoDto>.Failure("No se pudo determinar el usuario autenticado.");

        var empresaId = empresaIdResult.Value;
        var userId = _currentUser.UserIdInt.Value;

        var acceso = (await _accesoRepository.FindAsync(x =>
            x.UsuarioId == userId &&
            x.EmpresaId == empresaId &&
            x.SucursalId == sucursalId &&
            x.Activo, ct)).FirstOrDefault();

        if (acceso is null)
            return Result<ValidarSucursalAccesoDto>.Failure("Usuario sin acceso a la sucursal solicitada.");

        if (!acceso.PuedeConsultar)
            return Result<ValidarSucursalAccesoDto>.Failure("El usuario no tiene permiso de consulta en la sucursal solicitada.");

        if (requiereOperacion && !acceso.PuedeOperar)
            return Result<ValidarSucursalAccesoDto>.Failure("El usuario no tiene permiso operativo en la sucursal solicitada.");

        return Result<ValidarSucursalAccesoDto>.Success(new ValidarSucursalAccesoDto
        {
            SucursalId = sucursalId,
            PuedeConsultar = acceso.PuedeConsultar,
            PuedeOperar = acceso.PuedeOperar
        });
    }

    private async Task ClearPredeterminadaAsync(int usuarioId, int empresaId, int exceptSucursalId, CancellationToken ct)
    {
        var predeterminadas = await _accesoRepository.FindAsync(x =>
            x.UsuarioId == usuarioId &&
            x.EmpresaId == empresaId &&
            x.EsPredeterminada &&
            x.Activo &&
            x.SucursalId != exceptSucursalId, ct);

        foreach (var item in predeterminadas)
        {
            item.EsPredeterminada = false;
            await _accesoRepository.UpdateAsync(item, ct);
        }
    }

    private Result<int> TryGetEmpresaId()
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<int>.Failure("No se pudo determinar la empresa activa.");

        return Result<int>.Success(_currentUser.EmpresaId.Value);
    }

    private async Task<bool> UsuarioPerteneceAEmpresaAsync(int usuarioId, int empresaId, CancellationToken ct)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId, ct);
        if (usuario is null || !usuario.Activo)
            return false;

        var asignacion = await _usuarioEmpresaRepository.FindAsync(x => x.UsuarioId == usuarioId && x.EmpresaId == empresaId, ct);
        return asignacion.Any();
    }

    private static bool HasDefault(IReadOnlyList<UsuarioSucursalAcceso> accesos, UsuarioSucursalAcceso actual)
        => accesos.Any(x => x.Activo && x.EsPredeterminada && x.Id != actual.Id);

    private static UsuarioSucursalAccesoDto MapToDto(UsuarioSucursalAcceso e) => new()
    {
        Id = e.Id,
        UsuarioId = e.UsuarioId,
        EmpresaId = e.EmpresaId,
        SucursalId = e.SucursalId,
        EsPredeterminada = e.EsPredeterminada,
        PuedeConsultar = e.PuedeConsultar,
        PuedeOperar = e.PuedeOperar,
        Activo = e.Activo
    };
}
