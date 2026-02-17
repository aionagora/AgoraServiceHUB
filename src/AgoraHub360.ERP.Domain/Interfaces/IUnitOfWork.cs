namespace AgoraHub360.ERP.Domain.Interfaces;

/// <summary>
/// Unidad de trabajo para transacciones atómicas.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
