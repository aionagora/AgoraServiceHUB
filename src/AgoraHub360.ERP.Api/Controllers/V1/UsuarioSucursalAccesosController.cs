namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Seguridad;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[Authorize]
public class UsuarioSucursalAccesosController : ControllerBase
{
    private readonly IUsuarioSucursalAccesoService _service;

    public UsuarioSucursalAccesosController(IUsuarioSucursalAccesoService service)
    {
        _service = service;
    }

    [HttpGet("usuarios/{usuarioId:int}/sucursales-accesos")]
    public async Task<IActionResult> GetByUsuario(int usuarioId, CancellationToken ct)
    {
        var result = await _service.GetByUsuarioAsync(usuarioId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<UsuarioSucursalAccesoDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<UsuarioSucursalAccesoDto>>.Ok(result.Value!));
    }

    [HttpPut("usuarios/{usuarioId:int}/sucursales-accesos")]
    public async Task<IActionResult> Upsert(int usuarioId, [FromBody] UpsertUsuarioSucursalAccesoDto dto, CancellationToken ct)
    {
        var result = await _service.UpsertAsync(usuarioId, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<UsuarioSucursalAccesoDto>.Fail(result.Error!));

        return Ok(ApiResponse<UsuarioSucursalAccesoDto>.Ok(result.Value!, "Acceso de sucursal actualizado."));
    }

    [HttpPatch("usuarios/{usuarioId:int}/sucursales-accesos/{sucursalId:int}/predeterminada")]
    public async Task<IActionResult> SetPredeterminada(int usuarioId, int sucursalId, CancellationToken ct)
    {
        var result = await _service.SetPredeterminadaAsync(usuarioId, sucursalId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Sucursal predeterminada actualizada."));
    }

    [HttpDelete("usuarios/{usuarioId:int}/sucursales-accesos/{sucursalId:int}")]
    public async Task<IActionResult> Remove(int usuarioId, int sucursalId, CancellationToken ct)
    {
        var result = await _service.RemoveAsync(usuarioId, sucursalId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Acceso desactivado."));
    }

    [HttpGet("sucursales-accesos/validar/{sucursalId:int}")]
    public async Task<IActionResult> ValidarActual(int sucursalId, [FromQuery] bool requiereOperacion = false, CancellationToken ct = default)
    {
        var result = await _service.ValidarAccesoActualAsync(sucursalId, requiereOperacion, ct);
        if (!result.IsSuccess)
            return Forbid();

        return Ok(ApiResponse<ValidarSucursalAccesoDto>.Ok(result.Value!));
    }
}
