namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs.Empresa;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class EmpresasController : ControllerBase
{
    private readonly IEmpresaService _empresaService;
    private readonly IConfiguracionInicialEmpresaService _configuracionInicialEmpresaService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUsuarioService _usuarioService;
    private readonly IEmpresaSeedService _seedService;

    public EmpresasController(
        IEmpresaService empresaService,
        IConfiguracionInicialEmpresaService configuracionInicialEmpresaService,
        ICurrentUserService currentUserService,
        IUsuarioService usuarioService,
        IEmpresaSeedService seedService)
    {
        _empresaService = empresaService;
        _configuracionInicialEmpresaService = configuracionInicialEmpresaService;
        _currentUserService = currentUserService;
        _usuarioService = usuarioService;
        _seedService = seedService;
    }

    [HttpGet]
    [Authorize(Policy = PolicyNames.RequirePlatformAdmin)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        if (!IsPlatformAdmin())
            return Forbid();

        var result = await _empresaService.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<EmpresaDto>>.Ok(result.Value!));
    }

    /// <summary>
    /// Obtiene solo las empresas asignadas al usuario logueado.
    /// Solo requiere autenticación, sin roles específicos.
    /// </summary>
    [HttpGet("mis-empresas")]
    [Authorize(Policy = PolicyNames.RequireAuthenticated)]
    public async Task<IActionResult> GetMisEmpresas(CancellationToken ct)
    {
        var userId = _currentUserService.UserIdInt;
        if (!userId.HasValue)
            return Unauthorized(ApiResponse<IReadOnlyList<EmpresaDto>>.Fail("Usuario no autenticado"));

        var allEmpresas = await _empresaService.GetAllAsync(ct);
        if (!allEmpresas.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<EmpresaDto>>.Fail(allEmpresas.Error!));

        // Contrato actual: PlatformAdmin puede ver todas las activas en este endpoint.
        if (IsPlatformAdmin())
        {
            var activasGlobal = allEmpresas.Value!
                .Where(e => e.Activo)
                .ToList()
                .AsReadOnly();
            return Ok(ApiResponse<IReadOnlyList<EmpresaDto>>.Ok(activasGlobal));
        }

        var empresaIds = await GetAssignedActiveCompanyIdsAsync(userId.Value, ct);
        var misEmpresas = allEmpresas.Value!
            .Where(e => e.Activo && empresaIds.Contains(e.Id))
            .ToList();

        return Ok(ApiResponse<IReadOnlyList<EmpresaDto>>.Ok(misEmpresas.AsReadOnly()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        if (!IsPlatformAdmin())
        {
            var userId = _currentUserService.UserIdInt;
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<EmpresaDto>.Fail("Usuario no autenticado"));

            var empresaIds = await GetAssignedActiveCompanyIdsAsync(userId.Value, ct);
            if (!empresaIds.Contains(id))
                return Forbid();
        }

        var result = await _empresaService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<EmpresaDto>.Fail(result.Error!));

        return Ok(ApiResponse<EmpresaDto>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.RequirePlatformSuperAdmin)]
    public async Task<IActionResult> Create([FromBody] CreateEmpresaDto dto, CancellationToken ct)
    {
        if (!IsPlatformSuperAdmin())
            return Forbid();

        var result = await _empresaService.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<EmpresaDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<EmpresaDto>.Ok(result.Value!, "Empresa creada exitosamente."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PolicyNames.RequirePlatformAdmin)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmpresaDto dto, CancellationToken ct)
    {
        if (!IsPlatformAdmin())
            return Forbid();

        var result = await _empresaService.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada"))
                return NotFound(ApiResponse<EmpresaDto>.Fail(result.Error!));
            return BadRequest(ApiResponse<EmpresaDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<EmpresaDto>.Ok(result.Value!, "Empresa actualizada exitosamente."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PolicyNames.RequirePlatformSuperAdmin)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!IsPlatformSuperAdmin())
            return Forbid();

        var result = await _empresaService.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Empresa eliminada exitosamente."));
    }

    /// <summary>
    /// Seeds default MDM data (catalog, UoMs, statuses) for a given company.
    /// Idempotent — skips if data already exists.
    /// </summary>
    [HttpPost("{id:int}/seed")]
    public async Task<IActionResult> SeedDefaultData(int id, CancellationToken ct)
    {
        var empresa = await _empresaService.GetByIdAsync(id, ct);
        if (!empresa.IsSuccess)
            return NotFound(ApiResponse<bool>.Fail(empresa.Error!));

        var result = await _seedService.SeedDefaultDataAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Datos base creados exitosamente."));
    }

    /// <summary>
    /// Seeds default MDM data for the current user's company.
    /// </summary>
    [HttpPost("mi-empresa/seed")]
    public async Task<IActionResult> SeedMyCompanyData(CancellationToken ct)
    {
        var empresaId = _currentUserService.EmpresaId;
        if (!empresaId.HasValue)
            return Unauthorized(ApiResponse<bool>.Fail("No se pudo determinar la empresa activa."));

        var result = await _seedService.SeedDefaultDataAsync(empresaId.Value, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Datos base creados exitosamente."));
    }

    [HttpPost("{empresaId:long}/generar-configuracion-basica")]
    [Authorize(Policy = PolicyNames.RequirePlatformAdmin)]
    public async Task<IActionResult> GenerarConfiguracionBasica(long empresaId, CancellationToken ct)
    {
        if (!IsPlatformAdmin())
            return Forbid();

        var result = await _configuracionInicialEmpresaService.GenerarConfiguracionBasicaAsync(empresaId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ConfiguracionInicialEmpresaResultadoDto>.Fail(result.Error!));

        return Ok(ApiResponse<ConfiguracionInicialEmpresaResultadoDto>.Ok(
            result.Value!,
            "Configuración básica generada exitosamente."));
    }

    private bool IsPlatformSuperAdmin()
        => string.Equals(_currentUserService.PlatformRole, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase);

    private bool IsPlatformAdmin()
        => string.Equals(_currentUserService.PlatformRole, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase)
           || string.Equals(_currentUserService.PlatformRole, Roles.SystemAdmin, StringComparison.OrdinalIgnoreCase);

    private async Task<HashSet<int>> GetAssignedActiveCompanyIdsAsync(int userId, CancellationToken ct)
    {
        var asignadasResult = await _usuarioService.GetEmpresasAsignadasAsync(userId, ct);
        if (!asignadasResult.IsSuccess || asignadasResult.Value is null)
            return new HashSet<int>();

        var allEmpresasResult = await _empresaService.GetAllAsync(ct);
        if (!allEmpresasResult.IsSuccess || allEmpresasResult.Value is null)
            return new HashSet<int>();

        var activas = allEmpresasResult.Value
            .Where(e => e.Activo)
            .Select(e => e.Id)
            .ToHashSet();

        return asignadasResult.Value
            .Select(x => x.EmpresaId)
            .Where(activas.Contains)
            .ToHashSet();
    }
}
