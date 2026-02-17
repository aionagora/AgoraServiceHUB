namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Rol;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRolService _rolService;

    public RolesController(IRolService rolService)
    {
        _rolService = rolService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _rolService.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<RolDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _rolService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<RolDto>.Fail(result.Error!));

        return Ok(ApiResponse<RolDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRolDto dto, CancellationToken ct)
    {
        var result = await _rolService.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<RolDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<RolDto>.Ok(result.Value!, "Rol creado exitosamente."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRolDto dto, CancellationToken ct)
    {
        var result = await _rolService.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado"))
                return NotFound(ApiResponse<RolDto>.Fail(result.Error!));
            return BadRequest(ApiResponse<RolDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<RolDto>.Ok(result.Value!, "Rol actualizado exitosamente."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _rolService.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Rol eliminado exitosamente."));
    }
}
