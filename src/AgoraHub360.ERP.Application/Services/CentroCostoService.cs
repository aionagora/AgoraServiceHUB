namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.CST;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class CentroCostoService : ICentroCostoService
{
    private readonly IRepository<CentroCosto> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CentroCostoService(
        IRepository<CentroCosto> repo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    // ?? GetAllByEmpresa ????????????????????????????????????????????????????????

    public async Task<Result<IReadOnlyList<CentroCostoDto>>> GetAllByEmpresaAsync(
        int empresaId, CancellationToken ct)
    {
        var items = await _repo.FindAsync(
            c => c.EmpresaId == empresaId && c.Activo, ct);

        var map = items.ToDictionary(c => c.Id);
        var dtos = items
            .OrderBy(c => c.Codigo)
            .Select(c => MapToDto(c, map))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<CentroCostoDto>>.Success(dtos);
    }

    // ?? GetById ???????????????????????????????????????????????????????????????

    public async Task<Result<CentroCostoDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<CentroCostoDto>.Failure("No active company.");

        var cc = await _repo.GetByIdAsync(id, ct);
        if (cc is null || cc.EmpresaId != empresaId.Value || !cc.Activo)
            return Result<CentroCostoDto>.Failure("Centro de costo no encontrado.");

        var all = await _repo.FindAsync(c => c.EmpresaId == empresaId.Value && c.Activo, ct);
        return Result<CentroCostoDto>.Success(MapToDto(cc, all.ToDictionary(c => c.Id)));
    }

    // ?? Create ????????????????????????????????????????????????????????????????

    public async Task<Result<CentroCostoDto>> CreateAsync(CreateCentroCostoDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<CentroCostoDto>.Failure("No active company.");

        // Validación: Nombre requerido, máx 120 chars
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<CentroCostoDto>.Failure("El nombre es obligatorio.");
        if (dto.Nombre.Length > 120)
            return Result<CentroCostoDto>.Failure("El nombre no puede superar 120 caracteres.");

        // Validación: Código único por empresa
        var duplicado = await _repo.FindAsync(
            c => c.EmpresaId == empresaId.Value && c.Codigo == dto.Codigo.Trim() && c.Activo, ct);
        if (duplicado.Count > 0)
            return Result<CentroCostoDto>.Failure($"Ya existe un centro de costo con el código '{dto.Codigo}'.");

        // Validación: ParentId válido (misma empresa)
        if (dto.ParentId.HasValue)
        {
            var parent = await _repo.GetByIdAsync(dto.ParentId.Value, ct);
            if (parent is null || parent.EmpresaId != empresaId.Value || !parent.Activo)
                return Result<CentroCostoDto>.Failure("El centro de costo padre no existe o no pertenece a la empresa.");
        }

        var cc = new CentroCosto
        {
            EmpresaId = empresaId.Value,
            Codigo    = dto.Codigo.Trim(),
            Nombre    = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            ParentId  = dto.ParentId,
            Activo    = true
        };

        await _repo.AddAsync(cc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<CentroCostoDto>.Success(MapToDto(cc, new Dictionary<int, CentroCosto>()));
    }

    // ?? Update ????????????????????????????????????????????????????????????????

    public async Task<Result<CentroCostoDto>> UpdateAsync(int id, UpdateCentroCostoDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<CentroCostoDto>.Failure("No active company.");

        var cc = await _repo.GetByIdAsync(id, ct);
        if (cc is null || cc.EmpresaId != empresaId.Value || !cc.Activo)
            return Result<CentroCostoDto>.Failure("Centro de costo no encontrado.");

        // Validación: Nombre requerido, máx 120 chars
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<CentroCostoDto>.Failure("El nombre es obligatorio.");
        if (dto.Nombre.Length > 120)
            return Result<CentroCostoDto>.Failure("El nombre no puede superar 120 caracteres.");

        // Validación: ParentId válido (misma empresa, no circular)
        if (dto.ParentId.HasValue)
        {
            if (dto.ParentId.Value == id)
                return Result<CentroCostoDto>.Failure("Un centro de costo no puede ser su propio padre.");

            var parent = await _repo.GetByIdAsync(dto.ParentId.Value, ct);
            if (parent is null || parent.EmpresaId != empresaId.Value || !parent.Activo)
                return Result<CentroCostoDto>.Failure("El centro de costo padre no existe o no pertenece a la empresa.");

            // Detectar ciclo: el nuevo padre no debe ser descendiente del nodo actual
            if (await EsDescendienteAsync(dto.ParentId.Value, id, empresaId.Value, ct))
                return Result<CentroCostoDto>.Failure("No se puede asignar un descendiente como padre (referencia circular).");
        }

        cc.Nombre      = dto.Nombre.Trim();
        cc.Descripcion = dto.Descripcion?.Trim();
        cc.ParentId    = dto.ParentId;

        await _repo.UpdateAsync(cc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var all = await _repo.FindAsync(c => c.EmpresaId == empresaId.Value && c.Activo, ct);
        return Result<CentroCostoDto>.Success(MapToDto(cc, all.ToDictionary(c => c.Id)));
    }

    // ?? Delete ????????????????????????????????????????????????????????????????

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No active company.");

        var cc = await _repo.GetByIdAsync(id, ct);
        if (cc is null || cc.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("Centro de costo no encontrado.");

        // No eliminar si tiene hijos activos
        var hijos = await _repo.FindAsync(c => c.ParentId == id && c.Activo, ct);
        if (hijos.Count > 0)
            return Result<bool>.Failure("No se puede eliminar un centro de costo con hijos activos. Elimine primero los centros hijo.");

        cc.Activo = false;
        await _repo.UpdateAsync(cc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    // ?? Helpers ???????????????????????????????????????????????????????????????

    private static CentroCostoDto MapToDto(CentroCosto cc, Dictionary<int, CentroCosto> map)
    {
        map.TryGetValue(cc.ParentId ?? 0, out var parent);
        return new CentroCostoDto
        {
            Id          = cc.Id,
            Codigo      = cc.Codigo,
            Nombre      = cc.Nombre,
            Descripcion = cc.Descripcion,
            ParentId    = cc.ParentId,
            ParentNombre = parent?.Nombre,
            EmpresaId   = cc.EmpresaId,
            Activo      = cc.Activo
        };
    }

    /// <summary>
    /// Verifica si <paramref name="candidateId"/> es descendiente de <paramref name="ancestorId"/>
    /// para evitar referencias circulares en la jerarquía.
    /// </summary>
    private async Task<bool> EsDescendienteAsync(
        int candidateId, int ancestorId, int empresaId, CancellationToken ct)
    {
        var todos = await _repo.FindAsync(c => c.EmpresaId == empresaId && c.Activo, ct);
        var childMap = todos
            .Where(c => c.ParentId.HasValue)
            .GroupBy(c => c.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(c => c.Id).ToList());

        var cola = new Queue<int>();
        cola.Enqueue(ancestorId);

        while (cola.Count > 0)
        {
            var actual = cola.Dequeue();
            if (!childMap.TryGetValue(actual, out var hijos)) continue;
            foreach (var hijo in hijos)
            {
                if (hijo == candidateId) return true;
                cola.Enqueue(hijo);
            }
        }

        return false;
    }
}
