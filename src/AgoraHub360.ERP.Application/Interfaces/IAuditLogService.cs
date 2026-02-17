namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Shared.DTOs.AuditLog;

public interface IAuditLogService
{
    Task<PaginatedResultDto<AuditLogDto>> GetLogsAsync(AuditLogFilterDto filter);
    Task<AuditLogDto?> GetByIdAsync(long id);
    Task<List<string>> GetEntidadesDistintasAsync();
}
