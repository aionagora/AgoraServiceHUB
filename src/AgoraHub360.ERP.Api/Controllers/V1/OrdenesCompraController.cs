namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Compras;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compras/ordenes")]
[Authorize]
public class OrdenesCompraController : ControllerBase
{
    private readonly IOrdenCompraService _service;

    public OrdenesCompraController(IOrdenCompraService service)
    {
        _service = service;
    }

    /// <summary>Lista todas las OC con filtros opcionales.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? proveedorId,
        [FromQuery] string? estado,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(proveedorId, estado, desde, hasta, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<OrdenCompraDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<OrdenCompraDto>>.Ok(result.Value!));
    }

    /// <summary>Obtiene una OC por Id con sus líneas.</summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<OrdenCompraDto>.Fail(result.Error!));
        return Ok(ApiResponse<OrdenCompraDto>.Ok(result.Value!));
    }

    /// <summary>Crea una nueva OC en estado Borrador.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrdenCompraDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<OrdenCompraDto>.Fail(result.Error!));
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.OrdenCompraId },
            ApiResponse<OrdenCompraDto>.Ok(result.Value!, "Orden de compra creada exitosamente."));
    }

    /// <summary>Actualiza la cabecera de una OC en Borrador.</summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateOrdenCompraDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<OrdenCompraDto>.Fail(result.Error!));
        return Ok(ApiResponse<OrdenCompraDto>.Ok(result.Value!, "Orden de compra actualizada."));
    }

    /// <summary>Agrega una línea a una OC en Borrador.</summary>
    [HttpPost("{id:long}/lineas")]
    public async Task<IActionResult> AddLinea(long id, [FromBody] AddOrdenCompraLineaDto dto, CancellationToken ct)
    {
        var result = await _service.AddLineaAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<OrdenCompraDto>.Fail(result.Error!));
        return Ok(ApiResponse<OrdenCompraDto>.Ok(result.Value!, "Línea agregada."));
    }

    /// <summary>Actualiza una línea existente.</summary>
    [HttpPut("{id:long}/lineas/{lineaId:long}")]
    public async Task<IActionResult> UpdateLinea(long id, long lineaId, [FromBody] UpdateOrdenCompraLineaDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateLineaAsync(id, lineaId, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<OrdenCompraDto>.Fail(result.Error!));
        return Ok(ApiResponse<OrdenCompraDto>.Ok(result.Value!, "Línea actualizada."));
    }

    /// <summary>Elimina una línea de una OC en Borrador.</summary>
    [HttpDelete("{id:long}/lineas/{lineaId:long}")]
    public async Task<IActionResult> RemoveLinea(long id, long lineaId, CancellationToken ct)
    {
        var result = await _service.RemoveLineaAsync(id, lineaId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<OrdenCompraDto>.Fail(result.Error!));
        return Ok(ApiResponse<OrdenCompraDto>.Ok(result.Value!, "Línea eliminada."));
    }

    /// <summary>Cambia el estado de la OC (Confirmar, Aprobar, Anular).</summary>
    [HttpPost("{id:long}/estado")]
    public async Task<IActionResult> CambiarEstado(long id, [FromQuery] string nuevoEstado, CancellationToken ct)
    {
        var result = await _service.CambiarEstadoAsync(id, nuevoEstado, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<OrdenCompraDto>.Fail(result.Error!));
        return Ok(ApiResponse<OrdenCompraDto>.Ok(result.Value!, $"Estado cambiado a {nuevoEstado}."));
    }

    /// <summary>Gate [Finanzas]: aprueba o rechaza la OC.</summary>
    [HttpPost("{id:long}/aprobar")]
    public async Task<IActionResult> AprobarRechazar(long id, [FromBody] AprobarRechazarOrdenCompraDto dto, CancellationToken ct)
    {
        var result = await _service.AprobarRechazarAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<OrdenCompraDto>.Fail(result.Error!));
        return Ok(ApiResponse<OrdenCompraDto>.Ok(result.Value!, dto.Aprobado ? "OC aprobada." : "OC rechazada."));
    }

    /// <summary>Gate [4]: registra confirmación o negociación del proveedor (PI).</summary>
    [HttpPost("{id:long}/confirmacion-proveedor")]
    public async Task<IActionResult> RegistrarConfirmacionProveedor(long id, [FromBody] RegistrarConfirmacionProveedorDto dto, CancellationToken ct)
    {
        var result = await _service.RegistrarConfirmacionProveedorAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ConfirmacionProveedorDto>.Fail(result.Error!));
        return Ok(ApiResponse<ConfirmacionProveedorDto>.Ok(result.Value!, "Confirmación del proveedor registrada."));
    }

    /// <summary>Historial de confirmaciones/negociaciones del proveedor.</summary>
    [HttpGet("{id:long}/confirmaciones")]
    public async Task<IActionResult> GetConfirmaciones(long id, CancellationToken ct)
    {
        var result = await _service.GetConfirmacionesAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<ConfirmacionProveedorDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<ConfirmacionProveedorDto>>.Ok(result.Value!));
    }

    /// <summary>Gate [5]: programa un pago (anticipo/saldo) para la OC.</summary>
    [HttpPost("{id:long}/pagos")]
    public async Task<IActionResult> ProgramarPago(long id, [FromBody] ProgramarPagoDto dto, CancellationToken ct)
    {
        var result = await _service.ProgramarPagoAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<PagoOrdenCompraDto>.Fail(result.Error!));
        return Ok(ApiResponse<PagoOrdenCompraDto>.Ok(result.Value!, "Pago programado."));
    }

    /// <summary>Registra la ejecución de un pago programado.</summary>
    [HttpPost("{id:long}/pagos/{pagoId:long}/ejecutar")]
    public async Task<IActionResult> EjecutarPago(long id, long pagoId, [FromBody] EjecutarPagoDto dto, CancellationToken ct)
    {
        var result = await _service.EjecutarPagoAsync(id, pagoId, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<PagoOrdenCompraDto>.Fail(result.Error!));
        return Ok(ApiResponse<PagoOrdenCompraDto>.Ok(result.Value!, "Pago ejecutado."));
    }

    /// <summary>Lista pagos programados/ejecutados de la OC.</summary>
    [HttpGet("{id:long}/pagos")]
    public async Task<IActionResult> GetPagos(long id, CancellationToken ct)
    {
        var result = await _service.GetPagosAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<PagoOrdenCompraDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<PagoOrdenCompraDto>>.Ok(result.Value!));
    }

    /// <summary>Elimina (soft-delete) una OC en Borrador.</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Orden de compra eliminada."));
    }
}
