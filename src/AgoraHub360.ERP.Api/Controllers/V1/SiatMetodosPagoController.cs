namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Ventas;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/siat/metodos-pago")]
[Authorize]
public class SiatMetodosPagoController : ControllerBase
{
    private readonly ISiatMetodoPagoService _service;

    public SiatMetodosPagoController(ISiatMetodoPagoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<SiatMetodoPagoDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<SiatMetodoPagoDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado", StringComparison.OrdinalIgnoreCase)
                || result.Error.Contains("no existe", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<SiatMetodoPagoDto>.Fail(result.Error!));
            }

            return BadRequest(ApiResponse<SiatMetodoPagoDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<SiatMetodoPagoDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearSiatMetodoPagoRequestDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<SiatMetodoPagoDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<SiatMetodoPagoDto>.Ok(result.Value!, "Método de pago SIAT creado exitosamente."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] ActualizarSiatMetodoPagoRequestDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado", StringComparison.OrdinalIgnoreCase)
                || result.Error.Contains("no existe", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<SiatMetodoPagoDto>.Fail(result.Error!));
            }

            return BadRequest(ApiResponse<SiatMetodoPagoDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<SiatMetodoPagoDto>.Ok(result.Value!, "Método de pago SIAT actualizado exitosamente."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado", StringComparison.OrdinalIgnoreCase)
                || result.Error.Contains("no existe", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<bool>.Fail(result.Error!));
            }

            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        }

        return Ok(ApiResponse<bool>.Ok(true, "Método de pago SIAT desactivado exitosamente."));
    }

    [HttpPost("seed-default")]
    public async Task<IActionResult> SeedDefault(CancellationToken ct)
    {
        var result = await _service.SeedDefaultAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<SiatMetodoPagoDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<SiatMetodoPagoDto>>.Ok(result.Value!, "Catálogo SIAT de métodos de pago inicializado."));
    }
}
