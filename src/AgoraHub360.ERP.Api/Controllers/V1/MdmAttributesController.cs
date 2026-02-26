namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mdm/attributes")]
[Authorize]
public class MdmAttributesController : ControllerBase
{
    private readonly IAttributeDefinitionService _service;
    public MdmAttributesController(IAttributeDefinitionService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int industryId, CancellationToken ct)
    {
        var result = await _service.GetByIndustryAsync(industryId, ct);
        return Ok(ApiResponse<IReadOnlyList<AttributeDefinitionDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<AttributeDefinitionDto>.Fail(result.Error!));
        return Ok(ApiResponse<AttributeDefinitionDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAttributeDefinitionDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<AttributeDefinitionDto>.Fail(result.Error!));
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.AttributeId },
            ApiResponse<AttributeDefinitionDto>.Ok(result.Value!, "Atributo creado."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Atributo eliminado."));
    }

    [HttpPost("{id:long}/options")]
    public async Task<IActionResult> AddOption(long id, [FromBody] CreateAttributeOptionDto dto, CancellationToken ct)
    {
        var effective = dto with { AttributeId = id };
        var result = await _service.AddOptionAsync(effective, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<AttributeOptionDto>.Fail(result.Error!));
        return Ok(ApiResponse<AttributeOptionDto>.Ok(result.Value!, "Opción agregada."));
    }

    [HttpDelete("{id:long}/options/{optionId:long}")]
    public async Task<IActionResult> DeleteOption(long id, long optionId, CancellationToken ct)
    {
        var result = await _service.DeleteOptionAsync(optionId, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Opción eliminada."));
    }

    [HttpGet("product/{productId:long}")]
    public async Task<IActionResult> GetProductAttributes(long productId, CancellationToken ct)
    {
        var result = await _service.GetProductAttributesAsync(productId, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<ProductAttributeDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<ProductAttributeDto>>.Ok(result.Value!));
    }

    [HttpPost("product")]
    public async Task<IActionResult> UpsertProductAttribute([FromBody] UpsertProductAttributeDto dto, CancellationToken ct)
    {
        var result = await _service.UpsertProductAttributeAsync(dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<ProductAttributeDto>.Fail(result.Error!));
        return Ok(ApiResponse<ProductAttributeDto>.Ok(result.Value!, "Atributo asignado."));
    }
}
