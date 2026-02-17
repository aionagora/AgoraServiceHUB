namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Numeracion;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class NumeracionesController : ControllerBase
{
    private readonly INumeracionDocumentoService _service;

    public NumeracionesController(INumeracionDocumentoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<NumeracionDocumentoDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<NumeracionDocumentoDto>.Fail(result.Error!));

        return Ok(ApiResponse<NumeracionDocumentoDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNumeracionDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<NumeracionDocumentoDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<NumeracionDocumentoDto>.Ok(result.Value!, "Numeración creada exitosamente."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateNumeracionDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada"))
                return NotFound(ApiResponse<NumeracionDocumentoDto>.Fail(result.Error!));
            return BadRequest(ApiResponse<NumeracionDocumentoDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<NumeracionDocumentoDto>.Ok(result.Value!, "Numeración actualizada exitosamente."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Numeración eliminada exitosamente."));
    }
}
