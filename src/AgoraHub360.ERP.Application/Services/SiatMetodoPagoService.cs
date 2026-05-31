namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Ventas;

public class SiatMetodoPagoService : ISiatMetodoPagoService
{
    private readonly IRepository<SiatMetodoPago> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public SiatMetodoPagoService(
        IRepository<SiatMetodoPago> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<SiatMetodoPagoDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<IReadOnlyList<SiatMetodoPagoDto>>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;

        var metodos = await _repository.FindAsync(x => x.EmpresaId == empresaId, ct);
        var result = metodos
            .OrderByDescending(x => x.EsPredeterminado)
            .ThenBy(x => x.Codigo)
            .Select(MapToDto)
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<SiatMetodoPagoDto>>.Success(result);
    }

    public async Task<Result<SiatMetodoPagoDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<SiatMetodoPagoDto>.Failure(empresaIdResult.Error!);

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null || entity.EmpresaId != empresaIdResult.Value)
            return Result<SiatMetodoPagoDto>.Failure("Método de pago SIAT no encontrado.");

        return Result<SiatMetodoPagoDto>.Success(MapToDto(entity));
    }

    public async Task<Result<SiatMetodoPagoDto>> CreateAsync(CrearSiatMetodoPagoRequestDto dto, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<SiatMetodoPagoDto>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;

        var validacionBasica = ValidarCamposBasicos(dto.Codigo, dto.Nombre);
        if (!validacionBasica.IsSuccess)
            return Result<SiatMetodoPagoDto>.Failure(validacionBasica.Error!);

        var codigo = dto.Codigo.Trim();

        var existeCodigo = await _repository.FindAsync(
            x => x.EmpresaId == empresaId && x.Codigo == codigo,
            ct);

        if (existeCodigo.Any())
            return Result<SiatMetodoPagoDto>.Failure("Ya existe un método de pago SIAT con el mismo código para la empresa activa.");

        var metodosEmpresa = await _repository.FindAsync(x => x.EmpresaId == empresaId, ct);
        var metodoDebeSerPredeterminado = dto.EsPredeterminado || !metodosEmpresa.Any(x => x.Activo);

        if (metodoDebeSerPredeterminado)
            await ClearPredeterminadosAsync(metodosEmpresa.Where(x => x.Activo), ct);

        var entity = new SiatMetodoPago
        {
            EmpresaId = empresaId,
            Codigo = codigo,
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            ModoPago = dto.ModoPago?.Trim(),
            EsPredeterminado = metodoDebeSerPredeterminado,
            Activo = dto.Activo
        };

        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<SiatMetodoPagoDto>.Success(MapToDto(entity));
    }

    public async Task<Result<SiatMetodoPagoDto>> UpdateAsync(long id, ActualizarSiatMetodoPagoRequestDto dto, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<SiatMetodoPagoDto>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null || entity.EmpresaId != empresaId)
            return Result<SiatMetodoPagoDto>.Failure("Método de pago SIAT no encontrado.");

        var validacionBasica = ValidarCamposBasicos(dto.Codigo, dto.Nombre);
        if (!validacionBasica.IsSuccess)
            return Result<SiatMetodoPagoDto>.Failure(validacionBasica.Error!);

        var codigo = dto.Codigo.Trim();
        var existeCodigo = await _repository.FindAsync(
            x => x.EmpresaId == empresaId && x.Codigo == codigo && x.Id != id,
            ct);

        if (existeCodigo.Any())
            return Result<SiatMetodoPagoDto>.Failure("Ya existe otro método de pago SIAT con el mismo código para la empresa activa.");

        var metodosEmpresa = await _repository.FindAsync(x => x.EmpresaId == empresaId && x.Id != id, ct);

        if (dto.Activo && dto.EsPredeterminado)
            await ClearPredeterminadosAsync(metodosEmpresa.Where(x => x.Activo), ct);

        if (!dto.Activo)
            entity.EsPredeterminado = false;
        else
            entity.EsPredeterminado = dto.EsPredeterminado;

        entity.Codigo = codigo;
        entity.Nombre = dto.Nombre.Trim();
        entity.Descripcion = dto.Descripcion?.Trim();
        entity.ModoPago = dto.ModoPago?.Trim();
        entity.Activo = dto.Activo;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<SiatMetodoPagoDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<bool>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null || entity.EmpresaId != empresaId)
            return Result<bool>.Failure("Método de pago SIAT no encontrado.");

        if (!entity.Activo)
            return Result<bool>.Success(true);

        entity.Activo = false;
        entity.EsPredeterminado = false;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<IReadOnlyList<SiatMetodoPagoDto>>> SeedDefaultAsync(CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<IReadOnlyList<SiatMetodoPagoDto>>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;
        var existentes = await _repository.FindAsync(x => x.EmpresaId == empresaId, ct);

        if (!existentes.Any())
        {
            var defaults = new List<SiatMetodoPago>
            {
                new()
                {
                    EmpresaId = empresaId,
                    Codigo = "1",
                    Nombre = "Efectivo",
                    ModoPago = "Efectivo",
                    EsPredeterminado = true,
                    Activo = true
                },
                new()
                {
                    EmpresaId = empresaId,
                    Codigo = "2",
                    Nombre = "Tarjeta",
                    ModoPago = "Tarjeta",
                    EsPredeterminado = false,
                    Activo = true
                },
                new()
                {
                    EmpresaId = empresaId,
                    Codigo = "3",
                    Nombre = "Cheque",
                    ModoPago = "Cheque",
                    EsPredeterminado = false,
                    Activo = true
                },
                new()
                {
                    EmpresaId = empresaId,
                    Codigo = "4",
                    Nombre = "Transferencia / Depósito",
                    ModoPago = "Transferencia",
                    EsPredeterminado = false,
                    Activo = true
                },
                new()
                {
                    EmpresaId = empresaId,
                    Codigo = "7",
                    Nombre = "QR",
                    ModoPago = "Qr",
                    EsPredeterminado = false,
                    Activo = true
                }
            };

            foreach (var item in defaults)
                await _repository.AddAsync(item, ct);

            await _unitOfWork.SaveChangesAsync(ct);
            existentes = defaults;
        }

        var result = existentes
            .OrderByDescending(x => x.EsPredeterminado)
            .ThenBy(x => x.Codigo)
            .Select(MapToDto)
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<SiatMetodoPagoDto>>.Success(result);
    }

    private Result<int> TryGetEmpresaId()
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<int>.Failure("No se pudo determinar la empresa activa.");

        return Result<int>.Success(_currentUser.EmpresaId.Value);
    }

    private async Task ClearPredeterminadosAsync(IEnumerable<SiatMetodoPago> metodos, CancellationToken ct)
    {
        foreach (var metodo in metodos.Where(x => x.EsPredeterminado))
        {
            metodo.EsPredeterminado = false;
            await _repository.UpdateAsync(metodo, ct);
        }
    }

    private static Result<bool> ValidarCamposBasicos(string? codigo, string? nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return Result<bool>.Failure("El código es obligatorio.");

        if (string.IsNullOrWhiteSpace(nombre))
            return Result<bool>.Failure("El nombre es obligatorio.");

        return Result<bool>.Success(true);
    }

    private static SiatMetodoPagoDto MapToDto(SiatMetodoPago entity) => new()
    {
        Id = entity.Id,
        Codigo = entity.Codigo,
        Nombre = entity.Nombre,
        Descripcion = entity.Descripcion,
        ModoPago = entity.ModoPago,
        EsPredeterminado = entity.EsPredeterminado,
        Activo = entity.Activo
    };
}
