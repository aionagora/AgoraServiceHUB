namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mdm/product-uoms")]
[Authorize]
public class MdmProductUomsController : ControllerBase
{
    private readonly IProductUomService _service;
    public MdmProductUomsController(IProductUomService service) => _service = service;

    [HttpGet("by-product/{productId:long}")]
    public async Task<IActionResult> GetByProduct(long productId, CancellationToken ct)
    {
        var result = await _service.GetByProductAsync(productId, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<ProductUomDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<ProductUomDto>>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductUomDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<ProductUomDto>.Fail(result.Error!));
        return Ok(ApiResponse<ProductUomDto>.Ok(result.Value!, "UdM de producto asignada."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProductUomDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<ProductUomDto>.Fail(result.Error!));
        return Ok(ApiResponse<ProductUomDto>.Ok(result.Value!, "UdM de producto actualizada."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "UdM de producto eliminada."));
    }
}
