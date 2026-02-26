namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public interface IPlantillaContableService
{
    /// <summary>Lista todas las plantillas con filtro opcional por tipo documento.</summary>
    Task<Result<IReadOnlyList<PlantillaContableDto>>> GetAllAsync(string? tipoDocumento = null, CancellationToken ct = default);

    /// <summary>Obtiene una plantilla por Id con sus líneas.</summary>
    Task<Result<PlantillaContableDto>> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Obtiene la plantilla activa para un tipo de documento.</summary>
    Task<Result<PlantillaContableDto>> GetByTipoDocumentoAsync(string tipoDocumento, CancellationToken ct = default);

    /// <summary>Crea una plantilla con sus líneas.</summary>
    Task<Result<PlantillaContableDto>> CreateAsync(CreatePlantillaContableDto dto, CancellationToken ct = default);

    /// <summary>Actualiza una plantilla (reemplaza sus líneas).</summary>
    Task<Result<PlantillaContableDto>> UpdateAsync(int id, UpdatePlantillaContableDto dto, CancellationToken ct = default);

    /// <summary>Elimina (soft-delete) una plantilla.</summary>
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Genera las plantillas predefinidas para los tipos de documento existentes.</summary>
    Task<Result<int>> SeedPlantillasAsync(CancellationToken ct = default);
}
