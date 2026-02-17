namespace AgoraHub360.ERP.Persistence.Services;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Persistence.Context;
using AgoraHub360.ERP.Shared.DTOs.AuditLog;
using Microsoft.EntityFrameworkCore;

public class AuditLogService : IAuditLogService
{
    private readonly AgoraDbContext _context;

    public AuditLogService(AgoraDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResultDto<AuditLogDto>> GetLogsAsync(AuditLogFilterDto filter)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        // Filtros
        if (!string.IsNullOrWhiteSpace(filter.Entidad))
            query = query.Where(a => a.Entidad == filter.Entidad);

        if (!string.IsNullOrWhiteSpace(filter.EntidadId))
            query = query.Where(a => a.EntidadId == filter.EntidadId);

        if (!string.IsNullOrWhiteSpace(filter.Accion))
            query = query.Where(a => a.Accion == filter.Accion);

        if (!string.IsNullOrWhiteSpace(filter.Usuario))
            query = query.Where(a => a.Usuario != null && a.Usuario.Contains(filter.Usuario));

        if (filter.EmpresaId.HasValue)
            query = query.Where(a => a.EmpresaId == filter.EmpresaId);

        if (filter.FechaDesde.HasValue)
            query = query.Where(a => a.FechaHora >= filter.FechaDesde.Value);

        if (filter.FechaHasta.HasValue)
            query = query.Where(a => a.FechaHora <= filter.FechaHasta.Value);

        // Total
        var totalItems = await query.CountAsync();

        // Paginación (más recientes primero)
        var items = await query
            .OrderByDescending(a => a.FechaHora)
            .Skip((filter.Pagina - 1) * filter.TamanoPagina)
            .Take(filter.TamanoPagina)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                Entidad = a.Entidad,
                EntidadId = a.EntidadId,
                Accion = a.Accion,
                ValoresAnteriores = a.ValoresAnteriores,
                ValoresNuevos = a.ValoresNuevos,
                CamposModificados = a.CamposModificados,
                EmpresaId = a.EmpresaId,
                Usuario = a.Usuario,
                FechaHora = a.FechaHora
            })
            .ToListAsync();

        return new PaginatedResultDto<AuditLogDto>
        {
            Items = items,
            TotalItems = totalItems,
            Pagina = filter.Pagina,
            TamanoPagina = filter.TamanoPagina
        };
    }

    public async Task<AuditLogDto?> GetByIdAsync(long id)
    {
        var entity = await _context.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (entity is null) return null;

        return new AuditLogDto
        {
            Id = entity.Id,
            Entidad = entity.Entidad,
            EntidadId = entity.EntidadId,
            Accion = entity.Accion,
            ValoresAnteriores = entity.ValoresAnteriores,
            ValoresNuevos = entity.ValoresNuevos,
            CamposModificados = entity.CamposModificados,
            EmpresaId = entity.EmpresaId,
            Usuario = entity.Usuario,
            FechaHora = entity.FechaHora
        };
    }

    public async Task<List<string>> GetEntidadesDistintasAsync()
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Select(a => a.Entidad)
            .Distinct()
            .OrderBy(e => e)
            .ToListAsync();
    }
}
