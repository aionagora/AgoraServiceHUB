namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.RUL;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/rul/industries")]
[Authorize]
public class RulIndustriesController : ControllerBase
{
    private readonly IIndustryService _service;
    public RulIndustriesController(IIndustryService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<IndustryDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<IndustryDto>.Fail(result.Error!));
        return Ok(ApiResponse<IndustryDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIndustryDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IndustryDto>.Fail(result.Error!));
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.IndustryId },
            ApiResponse<IndustryDto>.Ok(result.Value!, "Industria creada."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateIndustryDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IndustryDto>.Fail(result.Error!));
        return Ok(ApiResponse<IndustryDto>.Ok(result.Value!, "Industria actualizada."));
    }

    [HttpGet("{id:int}/rules")]
    public async Task<IActionResult> GetRules(int id, CancellationToken ct)
    {
        var result = await _service.GetRulesAsync(id, ct);
        return Ok(ApiResponse<IReadOnlyList<ProductIndustryRuleDto>>.Ok(result.Value!));
    }

    [HttpPost("{id:int}/rules")]
    public async Task<IActionResult> CreateRule(int id, [FromBody] CreateProductIndustryRuleDto dto, CancellationToken ct)
    {
        var effective = dto with { IndustryId = id };
        var result = await _service.CreateRuleAsync(effective, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<ProductIndustryRuleDto>.Fail(result.Error!));
        return Ok(ApiResponse<ProductIndustryRuleDto>.Ok(result.Value!, "Regla creada."));
    }

    [HttpDelete("{id:int}/rules/{ruleId:long}")]
    public async Task<IActionResult> DeleteRule(int id, long ruleId, CancellationToken ct)
    {
        var result = await _service.DeleteRuleAsync(ruleId, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Regla eliminada."));
    }
}
