namespace AgoraHub360.ERP.Domain.Interfaces;

/// <summary>
/// Unidad de trabajo para transacciones atómicas.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Inicia una transacción de base de datos explícita. Si ya hay una activa, es un no-op.</summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Confirma la transacción activa y la libera.</summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Revierte la transacción activa y la libera. Es un no-op si no hay transacción.</summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta <paramref name="operation"/> dentro de una transacción gestionada por la
    /// estrategia de ejecución del proveedor (compatible con SqlServerRetryingExecutionStrategy).
    /// Hace commit si <paramref name="operation"/> termina sin excepción, rollback en caso contrario.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ExecuteInTransactionAsync(Func{Task},CancellationToken)"/>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);
}
