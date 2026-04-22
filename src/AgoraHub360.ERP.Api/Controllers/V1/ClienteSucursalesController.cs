namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clientes/{clienteId:int}/sucursales")]
[Authorize]
public class ClienteSucursalesController : ControllerBase
{
    private readonly IClienteSucursalService _service;

    public ClienteSucursalesController(IClienteSucursalService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int clienteId, CancellationToken ct)
    {
        var result = await _service.GetAllByClienteAsync(clienteId, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<IReadOnlyList<ClienteSucursalDto>>.Fail(result.Error!));

            return BadRequest(ApiResponse<IReadOnlyList<ClienteSucursalDto>>.Fail(result.Error!));
        }

        return Ok(ApiResponse<IReadOnlyList<ClienteSucursalDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int clienteId, int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(clienteId, id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<ClienteSucursalDto>.Fail(result.Error!));

        return Ok(ApiResponse<ClienteSucursalDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create(int clienteId, [FromBody] CreateClienteSucursalDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(clienteId, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ClienteSucursalDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { clienteId, id = result.Value!.Id },
            ApiResponse<ClienteSucursalDto>.Ok(result.Value!, "Sucursal de cliente creada exitosamente."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int clienteId, int id, [FromBody] UpdateClienteSucursalDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(clienteId, id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<ClienteSucursalDto>.Fail(result.Error!));

            return BadRequest(ApiResponse<ClienteSucursalDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<ClienteSucursalDto>.Ok(result.Value!, "Sucursal de cliente actualizada exitosamente."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int clienteId, int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(clienteId, id, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<bool>.Fail(result.Error!));

            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        }

        return Ok(ApiResponse<bool>.Ok(true, "Sucursal de cliente desactivada exitosamente."));
    }

    [HttpPatch("{id:int}/principal")]
    public async Task<IActionResult> SetPrincipal(int clienteId, int id, CancellationToken ct)
    {
        var result = await _service.SetPrincipalAsync(clienteId, id, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<bool>.Fail(result.Error!));

            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        }

        return Ok(ApiResponse<bool>.Ok(true, "Sucursal de cliente establecida como principal."));
    }
}
