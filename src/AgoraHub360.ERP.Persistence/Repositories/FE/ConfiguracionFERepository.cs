using AgoraHub360.ERP.Domain.Entities.FE;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AgoraHub360.ERP.Persistence.Repositories.FE;

/// <summary>
/// Repositorio de configuración FE por empresa (tenant-aware).
/// Incluye navegaciones a ProveedorFacturacionElectronica y AmbienteFacturacionElectronica.
/// No descifra secretos — eso es responsabilidad de la capa Application.
/// </summary>
public sealed class ConfiguracionFERepository : IConfiguracionFERepository
{
    private readonly AgoraDbContext _context;

    public ConfiguracionFERepository(AgoraDbContext context)
    {
        _context = context;
    }

    public async Task<ConfiguracionFacturacionElectronica?> ObtenerActivaPorEmpresaAsync(
        int empresaId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<ConfiguracionFacturacionElectronica>()
            .Include(p => p.ProveedorFacturacionElectronica)
            .Include(a => a.AmbienteFacturacionElectronica)
            .FirstOrDefaultAsync(
                x => x.EmpresaId == empresaId && x.EsConfiguracionActiva && x.Activo,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ConfiguracionFacturacionElectronica>> ListarPorEmpresaAsync(
        int empresaId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<ConfiguracionFacturacionElectronica>()
            .Include(p => p.ProveedorFacturacionElectronica)
            .Include(a => a.AmbienteFacturacionElectronica)
            .Where(x => x.EmpresaId == empresaId && x.Activo)
            .OrderByDescending(x => x.EsConfiguracionActiva)
            .ThenBy(x => x.NombreConfiguracion)
            .ToListAsync(cancellationToken);
    }

    public async Task<ConfiguracionFacturacionElectronica?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<ConfiguracionFacturacionElectronica>()
            .Include(p => p.ProveedorFacturacionElectronica)
            .Include(a => a.AmbienteFacturacionElectronica)
            .FirstOrDefaultAsync(x => x.Id == id && x.Activo, cancellationToken);
    }

    public async Task AgregarAsync(
        ConfiguracionFacturacionElectronica configuracion,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<ConfiguracionFacturacionElectronica>()
            .AddAsync(configuracion, cancellationToken);
    }

    public Task ActualizarAsync(
        ConfiguracionFacturacionElectronica configuracion,
        CancellationToken cancellationToken = default)
    {
        _context.Set<ConfiguracionFacturacionElectronica>().Update(configuracion);
        return Task.CompletedTask;
    }
}
