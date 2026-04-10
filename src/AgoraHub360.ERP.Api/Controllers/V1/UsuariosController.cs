namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Usuario;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _usuarioService.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<UsuarioDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _usuarioService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<UsuarioDto>.Fail(result.Error!));

        return Ok(ApiResponse<UsuarioDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioDto dto, CancellationToken ct)
    {
        var result = await _usuarioService.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<UsuarioDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<UsuarioDto>.Ok(result.Value!, "Usuario creado exitosamente."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioDto dto, CancellationToken ct)
    {
        var result = await _usuarioService.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado"))
                return NotFound(ApiResponse<UsuarioDto>.Fail(result.Error!));
            return BadRequest(ApiResponse<UsuarioDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<UsuarioDto>.Ok(result.Value!, "Usuario actualizado exitosamente."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _usuarioService.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Usuario eliminado exitosamente."));
    }

    /// <summary>
    /// Asigna o actualiza el rol de un usuario en una empresa.
    /// </summary>
    [HttpPost("{id:int}/roles")]
    public async Task<IActionResult> AsignarRol(int id, [FromBody] AsignarRolDto dto, CancellationToken ct)
    {
        var result = await _usuarioService.AsignarRolAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Rol asignado exitosamente."));
    }

    /// <summary>
    /// Remueve un usuario de una empresa.
    /// </summary>
    [HttpDelete("{id:int}/empresas/{empresaId:int}")]
    public async Task<IActionResult> RemoverDeEmpresa(int id, int empresaId, CancellationToken ct)
    {
        var result = await _usuarioService.RemoverDeEmpresaAsync(id, empresaId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Usuario removido de la empresa."));
    }

    /// <summary>
    /// Resetea la contraseña de cualquier usuario. Solo Admin.
    /// </summary>
    [HttpPut("{id:int}/reset-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto, CancellationToken ct)
    {
        var result = await _usuarioService.ResetPasswordAsync(id, dto, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Contraseña reseteada exitosamente."));
    }
}
