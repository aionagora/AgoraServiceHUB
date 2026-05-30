namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clientes")]
[Authorize]
public class ClientePerfilesFiscalesController : ControllerBase
{
    private readonly IClientePerfilFiscalService _service;

    public ClientePerfilesFiscalesController(IClientePerfilFiscalService service)
    {
        _service = service;
    }

    [HttpGet("{clienteId:int}/perfiles-fiscales")]
    public async Task<IActionResult> GetAll(int clienteId, CancellationToken ct)
    {
        var result = await _service.GetAllByClienteAsync(clienteId, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<IReadOnlyList<ClientePerfilFiscalDto>>.Fail(result.Error!));

            return BadRequest(ApiResponse<IReadOnlyList<ClientePerfilFiscalDto>>.Fail(result.Error!));
        }

        return Ok(ApiResponse<IReadOnlyList<ClientePerfilFiscalDto>>.Ok(result.Value!));
    }

    [HttpGet("perfiles-fiscales/{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<ClientePerfilFiscalDto>.Fail(result.Error!));

        return Ok(ApiResponse<ClientePerfilFiscalDto>.Ok(result.Value!));
    }

    [HttpPost("{clienteId:int}/perfiles-fiscales")]
    public async Task<IActionResult> Create(int clienteId, [FromBody] CreateClientePerfilFiscalDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(clienteId, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ClientePerfilFiscalDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<ClientePerfilFiscalDto>.Ok(result.Value!, "Perfil fiscal creado exitosamente."));
    }

    [HttpPut("perfiles-fiscales/{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateClientePerfilFiscalDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<ClientePerfilFiscalDto>.Fail(result.Error!));

            return BadRequest(ApiResponse<ClientePerfilFiscalDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<ClientePerfilFiscalDto>.Ok(result.Value!, "Perfil fiscal actualizado exitosamente."));
    }

    [HttpDelete("perfiles-fiscales/{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<bool>.Fail(result.Error!));

            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        }

        return Ok(ApiResponse<bool>.Ok(true, "Perfil fiscal desactivado exitosamente."));
    }

    [HttpPost("perfiles-fiscales/{id:long}/predeterminado")]
    public async Task<IActionResult> SetPredeterminado(long id, [FromBody] SetClientePerfilFiscalPredeterminadoDto? dto, CancellationToken ct)
    {
        var result = await _service.SetPredeterminadoAsync(id, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<bool>.Fail(result.Error!));

            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        }

        return Ok(ApiResponse<bool>.Ok(true, "Perfil fiscal establecido como predeterminado."));
    }
}
