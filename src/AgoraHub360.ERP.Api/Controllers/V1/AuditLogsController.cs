namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.AuditLog;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PaginatedResultDtoAudit = AgoraHub360.ERP.Shared.DTOs.AuditLog.PaginatedResultDto<AgoraHub360.ERP.Shared.DTOs.AuditLog.AuditLogDto>;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/audit-logs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] AuditLogFilterDto filter)
    {
        var result = await _auditLogService.GetLogsAsync(filter);
        return Ok(ApiResponse<PaginatedResultDtoAudit>.Ok(result));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _auditLogService.GetByIdAsync(id);
        if (result is null)
            return NotFound(ApiResponse<AuditLogDto>.Fail("Registro de auditoría no encontrado."));

        return Ok(ApiResponse<AuditLogDto>.Ok(result));
    }

    [HttpGet("entidades")]
    public async Task<IActionResult> GetEntidadesDistintas()
    {
        var result = await _auditLogService.GetEntidadesDistintasAsync();
        return Ok(ApiResponse<List<string>>.Ok(result));
    }
}
