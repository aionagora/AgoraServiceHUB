namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mdm/variants")]
[Authorize]
public class MdmVariantsController : ControllerBase
{
    private readonly IProductVariantService _service;
    public MdmVariantsController(IProductVariantService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] long parentProductId, CancellationToken ct)
    {
        var result = await _service.GetByProductAsync(parentProductId, ct);
        return Ok(ApiResponse<IReadOnlyList<ProductVariantDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<ProductVariantDto>.Fail(result.Error!));
        return Ok(ApiResponse<ProductVariantDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductVariantDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<ProductVariantDto>.Fail(result.Error!));
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.VariantId },
            ApiResponse<ProductVariantDto>.Ok(result.Value!, "Variante creada."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProductVariantDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<ProductVariantDto>.Fail(result.Error!));
        return Ok(ApiResponse<ProductVariantDto>.Ok(result.Value!, "Variante actualizada."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Variante eliminada."));
    }
}
