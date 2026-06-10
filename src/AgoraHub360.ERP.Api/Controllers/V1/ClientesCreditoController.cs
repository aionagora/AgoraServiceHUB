namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.CxC;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clientes")]
[Authorize]
public class ClientesCreditoController : ControllerBase
{
    private readonly IClienteCreditoConfiguracionService _service;
    private readonly ILogger<ClientesCreditoController> _logger;

    public ClientesCreditoController(
        IClienteCreditoConfiguracionService service,
        ILogger<ClientesCreditoController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("{clienteId:int}/credito-configuracion")]
    public async Task<IActionResult> GetByCliente(int clienteId, CancellationToken ct)
    {
        var result = await _service.GetByClienteAsync(clienteId, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no existe", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<ClienteCreditoConfiguracionDto>.Fail(result.Error!));

            return BadRequest(ApiResponse<ClienteCreditoConfiguracionDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<ClienteCreditoConfiguracionDto>.Ok(result.Value!));
    }

    [HttpPut("{clienteId:int}/credito-configuracion")]
    public async Task<IActionResult> Guardar(
        int clienteId,
        [FromBody] GuardarClienteCreditoConfiguracionRequestDto dto,
        CancellationToken ct)
    {
        var result = await _service.GuardarAsync(clienteId, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no existe", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<ClienteCreditoConfiguracionDto>.Fail(result.Error!));

            return BadRequest(ApiResponse<ClienteCreditoConfiguracionDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<ClienteCreditoConfiguracionDto>.Ok(result.Value!, "Configuración de crédito guardada exitosamente."));
    }
}
