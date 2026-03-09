namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;

/// <summary>
/// Seeds default MDM data (catalog, UoMs, product statuses) for a newly created company.
/// </summary>
public interface IEmpresaSeedService
{
    Task<Result<bool>> SeedDefaultDataAsync(int empresaId, CancellationToken ct = default);
}
