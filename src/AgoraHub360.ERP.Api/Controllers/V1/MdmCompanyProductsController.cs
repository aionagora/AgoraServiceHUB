namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;
using AgoraHub360.ERP.Shared.DTOs.RUL;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mdm/company-products")]
[Authorize]
public class MdmCompanyProductsController : ControllerBase
{
    private readonly ICompanyProductService _service;
    private readonly IProductVariantService _variantService;

    public MdmCompanyProductsController(ICompanyProductService service, IProductVariantService variantService)
    {
        _service = service;
        _variantService = variantService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<CompanyProductDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<CompanyProductDto>.Fail(result.Error!));
        return Ok(ApiResponse<CompanyProductDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCompanyProductDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<CompanyProductDto>.Fail(result.Error!));
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.CompanyProductId },
            ApiResponse<CompanyProductDto>.Ok(result.Value!, "Producto activado para la empresa."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCompanyProductDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<CompanyProductDto>.Fail(result.Error!));
        return Ok(ApiResponse<CompanyProductDto>.Ok(result.Value!, "Producto empresa actualizado."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Producto empresa eliminado."));
    }

    // ── Features ────────────────────────────────────────────────────────────
    [HttpGet("{id:long}/features")]
    public async Task<IActionResult> GetFeatures(long id, CancellationToken ct)
    {
        var result = await _service.GetFeaturesAsync(id, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<CompanyProductFeatureDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<CompanyProductFeatureDto>>.Ok(result.Value!));
    }

    [HttpPost("{id:long}/features")]
    public async Task<IActionResult> SetFeature(long id, [FromBody] SetFeatureDto dto, CancellationToken ct)
    {
        var result = await _service.SetFeatureAsync(id, dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<CompanyProductFeatureDto>.Fail(result.Error!));
        return Ok(ApiResponse<CompanyProductFeatureDto>.Ok(result.Value!, "Feature actualizada."));
    }

    [HttpPost("{id:long}/apply-rules/{industryId:int}")]
    public async Task<IActionResult> ApplyRules(long id, int industryId, CancellationToken ct)
    {
        var result = await _service.ApplyIndustryRulesAsync(id, industryId, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Reglas de industria aplicadas."));
    }

    // ── Variantes ────────────────────────────────────────────────────────────
    [HttpGet("{id:long}/variants")]
    public async Task<IActionResult> GetVariants(long id, CancellationToken ct)
    {
        // Obtener el ProductId del CompanyProduct para cargar variantes
        var cp = await _service.GetByIdAsync(id, ct);
        if (!cp.IsSuccess) return NotFound(ApiResponse<IReadOnlyList<ProductVariantDto>>.Fail(cp.Error!));
        var result = await _variantService.GetByProductAsync(cp.Value!.ProductId, ct);
        return Ok(ApiResponse<IReadOnlyList<ProductVariantDto>>.Ok(result.Value!));
    }
}
