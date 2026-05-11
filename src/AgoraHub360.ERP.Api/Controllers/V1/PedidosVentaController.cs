using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Ventas;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgoraHub360.ERP.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ventas/pedidos")]
[Authorize]
public class PedidosVentaController : ControllerBase
{
    private readonly IPedidoVentaService _service;

    public PedidosVentaController(IPedidoVentaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<PedidoVentaDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<PedidoVentaDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<PedidoVentaDto>.Fail(result.Error!));

            return BadRequest(ApiResponse<PedidoVentaDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<PedidoVentaDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePedidoVentaDto dto, CancellationToken ct)
    {
        // El Controller se reserva a validaciones de payload model/binding superficiales y pasar al Application Service.
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<PedidoVentaDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<PedidoVentaDto>.Ok(result.Value!, "Pedido de venta creado exitosamente."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePedidoVentaDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<PedidoVentaDto>.Fail(result.Error!));

            return BadRequest(ApiResponse<PedidoVentaDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<PedidoVentaDto>.Ok(result.Value!, "Pedido actualizado."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<bool>.Fail(result.Error!));

            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        }

        return Ok(ApiResponse<bool>.Ok(true, "Pedido anulado/borrado exitosamente."));
    }
}
