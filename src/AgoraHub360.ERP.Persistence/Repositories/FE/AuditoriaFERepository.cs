using AgoraHub360.ERP.Domain.Entities.FE;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AgoraHub360.ERP.Persistence.Repositories.FE;

/// <summary>
/// Repositorio de auditoría de Facturación Electrónica (tenant-aware).
/// Solo escritura y consulta histórica — sin actualización ni borrado.
/// </summary>
public sealed class AuditoriaFERepository : IAuditoriaFERepository
{
    private readonly AgoraDbContext _context;

    public AuditoriaFERepository(AgoraDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(
        AuditoriaFacturacion auditoria,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<AuditoriaFacturacion>()
            .AddAsync(auditoria, cancellationToken);
    }

    public async Task<IReadOnlyList<AuditoriaFacturacion>> ListarPorFacturaAsync(
        long facturaVentaId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<AuditoriaFacturacion>()
            .Where(x => x.FacturaVentaId == facturaVentaId && x.Activo)
            .OrderByDescending(x => x.FechaHora)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditoriaFacturacion>> ListarPorEmpresaAsync(
        int empresaId,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<AuditoriaFacturacion>()
            .Where(x => x.EmpresaId == empresaId
                        && x.FechaHora >= desde
                        && x.FechaHora <= hasta
                        && x.Activo)
            .OrderByDescending(x => x.FechaHora)
            .ToListAsync(cancellationToken);
    }
}
