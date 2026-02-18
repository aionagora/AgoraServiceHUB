namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mdm/categories")]
[Authorize]
public class MdmCategoriesController : ControllerBase
{
    private readonly ICategoryService _service;
    public MdmCategoriesController(ICategoryService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] long catalogId, CancellationToken ct)
    {
        var result = await _service.GetByCatalogAsync(catalogId, ct);
        return Ok(ApiResponse<IReadOnlyList<CategoryDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<CategoryDto>.Fail(result.Error!));
        return Ok(ApiResponse<CategoryDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<CategoryDto>.Fail(result.Error!));
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.CategoryId },
            ApiResponse<CategoryDto>.Ok(result.Value!, "Categoría creada."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCategoryDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<CategoryDto>.Fail(result.Error!));
        return Ok(ApiResponse<CategoryDto>.Ok(result.Value!, "Categoría actualizada."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Categoría eliminada."));
    }
}
