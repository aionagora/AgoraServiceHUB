namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mdm/products")]
[Authorize]
public class MdmProductsController : ControllerBase
{
    private readonly IProductService _service;
    private readonly IProductCodeService _codeService;
    private readonly IAttributeDefinitionService _attrService;

    public MdmProductsController(
        IProductService service,
        IProductCodeService codeService,
        IAttributeDefinitionService attrService)
    {
        _service = service;
        _codeService = codeService;
        _attrService = attrService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] long? catalogId, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(catalogId, ct);
        return Ok(ApiResponse<IReadOnlyList<ProductDto2>>.Ok(result.Value!));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<ProductDto2>.Fail(result.Error!));
        return Ok(ApiResponse<ProductDto2>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto2 dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<ProductDto2>.Fail(result.Error!));
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.ProductId },
            ApiResponse<ProductDto2>.Ok(result.Value!, "Producto creado exitosamente."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProductDto2 dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<ProductDto2>.Fail(result.Error!));
        return Ok(ApiResponse<ProductDto2>.Ok(result.Value!, "Producto actualizado."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Producto eliminado."));
    }

    // ── Códigos del producto ────────────────────────────────────────────────
    [HttpGet("{id:long}/codes")]
    public async Task<IActionResult> GetCodes(long id, CancellationToken ct)
    {
        var result = await _codeService.GetByProductAsync(id, ct);
        return Ok(ApiResponse<IReadOnlyList<ProductCodeDto>>.Ok(result.Value!));
    }

    [HttpPost("{id:long}/codes")]
    public async Task<IActionResult> AddCode(long id, [FromBody] CreateProductCodeDto dto, CancellationToken ct)
    {
        var effective = dto with { ProductId = id };
        var result = await _codeService.CreateAsync(effective, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<ProductCodeDto>.Fail(result.Error!));
        return Ok(ApiResponse<ProductCodeDto>.Ok(result.Value!, "Código agregado."));
    }

    [HttpDelete("{id:long}/codes/{codeId:long}")]
    public async Task<IActionResult> DeleteCode(long id, long codeId, CancellationToken ct)
    {
        var result = await _codeService.DeleteAsync(codeId, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Código eliminado."));
    }

    // ── Atributos del producto ──────────────────────────────────────────────
    [HttpGet("{id:long}/attributes")]
    public async Task<IActionResult> GetAttributes(long id, CancellationToken ct)
    {
        var result = await _attrService.GetProductAttributesAsync(id, ct);
        return Ok(ApiResponse<IReadOnlyList<ProductAttributeDto>>.Ok(result.Value!));
    }

    [HttpPost("{id:long}/attributes")]
    public async Task<IActionResult> UpsertAttribute(long id, [FromBody] UpsertProductAttributeDto dto, CancellationToken ct)
    {
        var effective = dto with { ProductId = id };
        var result = await _attrService.UpsertProductAttributeAsync(effective, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<ProductAttributeDto>.Fail(result.Error!));
        return Ok(ApiResponse<ProductAttributeDto>.Ok(result.Value!, "Atributo guardado."));
    }
}
