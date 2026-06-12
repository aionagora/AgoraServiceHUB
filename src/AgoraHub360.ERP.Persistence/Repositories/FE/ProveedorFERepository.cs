using AgoraHub360.ERP.Domain.Entities.FE;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AgoraHub360.ERP.Persistence.Repositories.FE;

/// <summary>
/// Repositorio del catálogo global de proveedores FE (no tenant-aware).
/// </summary>
public sealed class ProveedorFERepository : IProveedorFERepository
{
    private readonly AgoraDbContext _context;

    public ProveedorFERepository(AgoraDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProveedorFacturacionElectronica>> ListarActivosAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<ProveedorFacturacionElectronica>()
            .Where(x => x.Activo)
            .OrderBy(x => x.Codigo)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProveedorFacturacionElectronica?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<ProveedorFacturacionElectronica>()
            .FirstOrDefaultAsync(x => x.Id == id && x.Activo, cancellationToken);
    }

    public async Task<ProveedorFacturacionElectronica?> ObtenerPorCodigoAsync(
        string codigo,
        CancellationToken cancellationToken = default)
    {
        var codigoNormalizado = codigo.Trim().ToUpperInvariant();

        return await _context.Set<ProveedorFacturacionElectronica>()
            .FirstOrDefaultAsync(
                x => x.Codigo.Trim().ToUpper() == codigoNormalizado && x.Activo,
                cancellationToken);
    }
}
