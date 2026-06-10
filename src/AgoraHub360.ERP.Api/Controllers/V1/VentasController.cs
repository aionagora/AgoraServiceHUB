using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Ventas;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgoraHub360.ERP.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ventas")]
[Authorize]
public class VentasController : ControllerBase
{
    private readonly IVentaService _service;

    public VentasController(IVentaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] VentaFilterDto filter, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(filter, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<PaginatedResultDto<VentaResumenDto>>.Fail(result.Error!));

        return Ok(ApiResponse<PaginatedResultDto<VentaResumenDto>>.Ok(result.Value!));
    }

    [HttpGet("pagos")]
    public async Task<IActionResult> GetPagos([FromQuery] VentaPagoFilterDto filter, CancellationToken ct)
    {
        var result = await _service.GetPagosPagedAsync(filter, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<PaginatedResultDto<VentaPagoDto>>.Fail(result.Error!));

        return Ok(ApiResponse<PaginatedResultDto<VentaPagoDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase)
                || result.Error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<VentaDto>.Fail(result.Error!));
            }

            return BadRequest(ApiResponse<VentaDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<VentaDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearVentaRequestDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<VentaDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<VentaDto>.Ok(result.Value!, "Venta creada exitosamente."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] ActualizarVentaRequestDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase)
                || result.Error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<VentaDto>.Fail(result.Error!));
            }

            return BadRequest(ApiResponse<VentaDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<VentaDto>.Ok(result.Value!, "Venta actualizada."));
    }

    [HttpPost("{id:long}/confirmar")]
    public async Task<IActionResult> Confirmar(long id, [FromBody] ConfirmarVentaRequestDto? dto, CancellationToken ct)
    {
        var result = await _service.ConfirmarAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase)
                || result.Error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<VentaDto>.Fail(result.Error!));
            }

            return BadRequest(ApiResponse<VentaDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<VentaDto>.Ok(result.Value!, "Venta confirmada."));
    }

    [HttpPost("desde-pedido")]
    public async Task<IActionResult> CrearDesdePedido([FromBody] GenerarVentaDesdePedidoRequestDto dto, CancellationToken ct)
    {
        var result = await _service.CrearDesdePedidoAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<VentaDto>.Fail(result.Error!));

        return Ok(ApiResponse<VentaDto>.Ok(result.Value!, "Venta generada desde pedido."));
    }

    [HttpPost("{id:long}/pagos")]
    public async Task<IActionResult> RegistrarPago(long id, [FromBody] RegistrarPagoVentaRequestDto dto, CancellationToken ct)
    {
        var result = await _service.RegistrarPagoAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase)
                || result.Error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<VentaDto>.Fail(result.Error!));
            }

            return BadRequest(ApiResponse<VentaDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<VentaDto>.Ok(result.Value!, "Pago registrado."));
    }

    [HttpPost("{id:long}/anular")]
    public async Task<IActionResult> Anular(long id, [FromBody] AnularVentaRequestDto dto, CancellationToken ct)
    {
        var result = await _service.AnularAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase)
                || result.Error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<VentaDto>.Fail(result.Error!));
            }

            return BadRequest(ApiResponse<VentaDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<VentaDto>.Ok(result.Value!, "Venta anulada."));
    }

    [HttpPost("pagos/{id:long}/anular")]
    public async Task<IActionResult> AnularPago(long id, [FromBody] AnularPagoVentaRequestDto dto, CancellationToken ct)
    {
        var result = await _service.AnularPagoAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado", StringComparison.OrdinalIgnoreCase)
                || result.Error.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<VentaDto>.Fail(result.Error!));
            }

            return BadRequest(ApiResponse<VentaDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<VentaDto>.Ok(result.Value!, "Pago anulado exitosamente."));
    }
}
