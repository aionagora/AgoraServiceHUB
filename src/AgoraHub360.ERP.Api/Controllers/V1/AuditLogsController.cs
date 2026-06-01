namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
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
    private readonly ICurrentUserService _currentUserService;

    public AuditLogsController(IAuditLogService auditLogService, ICurrentUserService currentUserService)
    {
        _auditLogService = auditLogService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] AuditLogFilterDto filter)
    {
        if (!CanReadAudit())
            return Forbid();

        if (!IsGlobalAuditReader())
        {
            var tenantId = GetSelectedTenantId();
            if (!tenantId.HasValue)
                return Forbid();

            // Tenant user no puede forzar EmpresaId arbitrario.
            filter.EmpresaId = tenantId.Value;
        }

        var result = await _auditLogService.GetLogsAsync(filter);
        return Ok(ApiResponse<PaginatedResultDtoAudit>.Ok(result));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        if (!CanReadAudit())
            return Forbid();

        var result = await _auditLogService.GetByIdAsync(id);
        if (result is null)
            return NotFound(ApiResponse<AuditLogDto>.Fail("Registro de auditoría no encontrado."));

        if (!IsGlobalAuditReader())
        {
            var tenantId = GetSelectedTenantId();
            if (!tenantId.HasValue)
                return Forbid();

            if (result.EmpresaId != tenantId.Value)
                return Forbid();
        }

        return Ok(ApiResponse<AuditLogDto>.Ok(result));
    }

    [HttpGet("entidades")]
    public async Task<IActionResult> GetEntidadesDistintas()
    {
        if (!CanReadAudit())
            return Forbid();

        int? empresaScope = null;
        if (!IsGlobalAuditReader())
        {
            empresaScope = GetSelectedTenantId();
            if (!empresaScope.HasValue)
                return Forbid();
        }

        var result = await _auditLogService.GetEntidadesDistintasAsync(empresaScope);
        return Ok(ApiResponse<List<string>>.Ok(result));
    }

    private bool IsPlatformAdmin()
        => string.Equals(_currentUserService.PlatformRole, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase)
           || string.Equals(_currentUserService.PlatformRole, Roles.SystemAdmin, StringComparison.OrdinalIgnoreCase);

    private bool IsSecurityAuditor()
        => string.Equals(_currentUserService.PlatformRole, Roles.SecurityAuditor, StringComparison.OrdinalIgnoreCase);

    private bool IsTenantAdmin()
        => string.Equals(_currentUserService.TenantRole, Roles.TenantOwner, StringComparison.OrdinalIgnoreCase)
           || string.Equals(_currentUserService.TenantRole, Roles.AdminEmpresa, StringComparison.OrdinalIgnoreCase);

    private bool IsGlobalAuditReader() => IsPlatformAdmin() || IsSecurityAuditor();

    private bool CanReadAudit() => IsGlobalAuditReader() || IsTenantAdmin();

    private int? GetSelectedTenantId()
    {
        if (!string.Equals(_currentUserService.TenantStatus, TenantStatus.Selected, StringComparison.OrdinalIgnoreCase))
            return null;

        return _currentUserService.TenantId ?? _currentUserService.EmpresaId;
    }
}
