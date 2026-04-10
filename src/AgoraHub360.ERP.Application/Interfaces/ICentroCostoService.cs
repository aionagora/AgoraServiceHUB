namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public interface ICentroCostoService
{
    /// <summary>Obtiene todos los centros de costo activos de la empresa.</summary>
    Task<Result<IReadOnlyList<CentroCostoDto>>> GetAllByEmpresaAsync(
        int empresaId, CancellationToken ct = default);

    /// <summary>Obtiene un centro de costo por Id.</summary>
    Task<Result<CentroCostoDto>> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Crea un nuevo centro de costo.</summary>
    Task<Result<CentroCostoDto>> CreateAsync(CreateCentroCostoDto dto, CancellationToken ct = default);

    /// <summary>Actualiza nombre, descripción o padre de un centro de costo existente.</summary>
    Task<Result<CentroCostoDto>> UpdateAsync(int id, UpdateCentroCostoDto dto, CancellationToken ct = default);

    /// <summary>Soft-delete de un centro de costo sin hijos activos.</summary>
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
