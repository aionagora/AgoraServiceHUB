namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.PRC;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/prc/price-lists")]
[Authorize]
public class PrcPriceListsController : ControllerBase
{
    private readonly IPriceListService _service;
    public PrcPriceListsController(IPriceListService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<PriceListDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<PriceListDto>.Fail(result.Error!));
        return Ok(ApiResponse<PriceListDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePriceListDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<PriceListDto>.Fail(result.Error!));
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.PriceListId },
            ApiResponse<PriceListDto>.Ok(result.Value!, "Lista de precios creada."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePriceListDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<PriceListDto>.Fail(result.Error!));
        return Ok(ApiResponse<PriceListDto>.Ok(result.Value!, "Lista de precios actualizada."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Lista de precios eliminada."));
    }

    [HttpGet("{id:long}/items")]
    public async Task<IActionResult> GetItems(long id, CancellationToken ct)
    {
        var result = await _service.GetItemsAsync(id, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<PriceListItemDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<PriceListItemDto>>.Ok(result.Value!));
    }

    [HttpPost("{id:long}/items")]
    public async Task<IActionResult> AddItem(long id, [FromBody] CreatePriceListItemDto dto, CancellationToken ct)
    {
        var effective = dto with { PriceListId = id };
        var result = await _service.AddItemAsync(effective, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<PriceListItemDto>.Fail(result.Error!));
        return Ok(ApiResponse<PriceListItemDto>.Ok(result.Value!, "Precio agregado."));
    }

    [HttpDelete("{id:long}/items/{itemId:long}")]
    public async Task<IActionResult> DeleteItem(long id, long itemId, CancellationToken ct)
    {
        var result = await _service.DeleteItemAsync(itemId, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Precio eliminado."));
    }
}
