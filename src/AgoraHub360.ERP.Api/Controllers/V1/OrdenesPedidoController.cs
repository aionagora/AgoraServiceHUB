namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Compras;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compras/pedidos")]
[Authorize]
public class OrdenesPedidoController : ControllerBase
{
    private readonly IOrdenPedidoService _service;

    public OrdenesPedidoController(IOrdenPedidoService service)
    {
        _service = service;
    }

    /// <summary>Lista todas las OP con filtros opcionales.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? estado,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(estado, desde, hasta, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<OrdenPedidoDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<OrdenPedidoDto>>.Ok(result.Value!));
    }

    /// <summary>Obtiene una OP por Id con sus líneas.</summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<OrdenPedidoDto>.Fail(result.Error!));
        return Ok(ApiResponse<OrdenPedidoDto>.Ok(result.Value!));
    }

    /// <summary>Crea una nueva Orden de Pedido en estado Borrador.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrdenPedidoDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<OrdenPedidoDto>.Fail(result.Error!));
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.OrdenPedidoId },
            ApiResponse<OrdenPedidoDto>.Ok(result.Value!, "Orden de pedido creada."));
    }

    /// <summary>Actualiza una OP en Borrador.</summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateOrdenPedidoDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<OrdenPedidoDto>.Fail(result.Error!));
        return Ok(ApiResponse<OrdenPedidoDto>.Ok(result.Value!, "Orden de pedido actualizada."));
    }

    /// <summary>Envía la OP a Compras Central para revisión (Borrador ? EnRevision).</summary>
    [HttpPost("{id:long}/enviar-revision")]
    public async Task<IActionResult> EnviarRevision(long id, CancellationToken ct)
    {
        var result = await _service.EnviarARevisionAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<OrdenPedidoDto>.Fail(result.Error!));
        return Ok(ApiResponse<OrdenPedidoDto>.Ok(result.Value!, "Orden de pedido enviada a revisión."));
    }

    /// <summary>
    /// Gate de stock: Compras revisa disponibilidad.
    /// Si cubre ? AbastecidoConStock. Si no ? PendienteAprobacion.
    /// </summary>
    [HttpPost("{id:long}/revisar-stock")]
    public async Task<IActionResult> RevisarStock(long id, [FromBody] RevisarStockOrdenPedidoDto dto, CancellationToken ct)
    {
        var result = await _service.RevisarStockAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<OrdenPedidoDto>.Fail(result.Error!));
        return Ok(ApiResponse<OrdenPedidoDto>.Ok(result.Value!,
            dto.StockCubre ? "Stock suficiente. OP abastecida con stock." : "Stock insuficiente. OP pendiente de aprobación."));
    }

    /// <summary>Gate de aprobación: Finanzas/Dirección aprueba o rechaza.</summary>
    [HttpPost("{id:long}/aprobar")]
    public async Task<IActionResult> AprobarRechazar(long id, [FromBody] AprobarRechazarOrdenPedidoDto dto, CancellationToken ct)
    {
        var result = await _service.AprobarRechazarAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<OrdenPedidoDto>.Fail(result.Error!));
        return Ok(ApiResponse<OrdenPedidoDto>.Ok(result.Value!, dto.Aprobado ? "OP aprobada." : "OP rechazada."));
    }

    /// <summary>Anula la OP.</summary>
    [HttpPost("{id:long}/anular")]
    public async Task<IActionResult> Anular(long id, [FromQuery] string? motivo, CancellationToken ct)
    {
        var result = await _service.AnularAsync(id, motivo, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<OrdenPedidoDto>.Fail(result.Error!));
        return Ok(ApiResponse<OrdenPedidoDto>.Ok(result.Value!, "Orden de pedido anulada."));
    }

    /// <summary>Elimina (soft-delete) una OP en Borrador.</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Orden de pedido eliminada."));
    }
}
