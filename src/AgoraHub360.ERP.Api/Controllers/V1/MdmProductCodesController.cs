namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mdm/product-codes")]
[Authorize]
public class MdmProductCodesController : ControllerBase
{
    private readonly IProductCodeService _service;
    public MdmProductCodesController(IProductCodeService service) => _service = service;

    [HttpGet("by-product/{productId:long}")]
    public async Task<IActionResult> GetByProduct(long productId, CancellationToken ct)
    {
        var result = await _service.GetByProductAsync(productId, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<ProductCodeDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<ProductCodeDto>>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCodeDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<ProductCodeDto>.Fail(result.Error!));
        return Ok(ApiResponse<ProductCodeDto>.Ok(result.Value!, "Código creado."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProductCodeDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<ProductCodeDto>.Fail(result.Error!));
        return Ok(ApiResponse<ProductCodeDto>.Ok(result.Value!, "Código actualizado."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Código eliminado."));
    }
}
