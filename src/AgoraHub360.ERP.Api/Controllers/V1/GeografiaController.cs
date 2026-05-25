namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Common;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/geografia")]
[Authorize]
public class GeografiaController : ControllerBase
{
    private readonly IGeografiaService _service;

    public GeografiaController(IGeografiaService service)
    {
        _service = service;
    }

    [HttpGet("paises")]
    [HttpGet("~/api/v{version:apiVersion}/paises")]
    public async Task<IActionResult> GetPaises(CancellationToken ct)
    {
        var result = await _service.GetPaisesAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<GeoCatalogItemDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<GeoCatalogItemDto>>.Ok(result.Value!));
    }

    [HttpGet("paises/{paisId:guid}/departamentos")]
    [HttpGet("~/api/v{version:apiVersion}/departamentos")]
    public async Task<IActionResult> GetDepartamentosByPais([FromRoute] Guid? paisId, [FromQuery(Name = "paisId")] Guid? paisIdQuery, CancellationToken ct)
    {
        var id = paisId ?? paisIdQuery;
        if (!id.HasValue)
            return BadRequest(ApiResponse<IReadOnlyList<GeoCatalogItemDto>>.Fail("paisId es obligatorio."));

        var result = await _service.GetDepartamentosByPaisAsync(id.Value, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<GeoCatalogItemDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<GeoCatalogItemDto>>.Ok(result.Value!));
    }

    [HttpGet("departamentos/{departamentoId:guid}/provincias")]
    [HttpGet("~/api/v{version:apiVersion}/provincias")]
    public async Task<IActionResult> GetProvinciasByDepartamento([FromRoute] Guid? departamentoId, [FromQuery(Name = "departamentoId")] Guid? departamentoIdQuery, CancellationToken ct)
    {
        var id = departamentoId ?? departamentoIdQuery;
        if (!id.HasValue)
            return BadRequest(ApiResponse<IReadOnlyList<GeoCatalogItemDto>>.Fail("departamentoId es obligatorio."));

        var result = await _service.GetProvinciasByDepartamentoAsync(id.Value, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<GeoCatalogItemDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<GeoCatalogItemDto>>.Ok(result.Value!));
    }

    [HttpGet("provincias/{provinciaId:guid}/ciudades")]
    [HttpGet("~/api/v{version:apiVersion}/ciudades")]
    public async Task<IActionResult> GetCiudadesByProvincia([FromRoute] Guid? provinciaId, [FromQuery(Name = "provinciaId")] Guid? provinciaIdQuery, CancellationToken ct)
    {
        var id = provinciaId ?? provinciaIdQuery;
        if (!id.HasValue)
            return BadRequest(ApiResponse<IReadOnlyList<GeoCatalogItemDto>>.Fail("provinciaId es obligatorio."));

        var result = await _service.GetCiudadesByProvinciaAsync(id.Value, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<GeoCatalogItemDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<GeoCatalogItemDto>>.Ok(result.Value!));
    }

    [HttpGet("ciudades/{ciudadId:guid}/zonas")]
    [HttpGet("~/api/v{version:apiVersion}/zonas")]
    public async Task<IActionResult> GetZonasByCiudad([FromRoute] Guid? ciudadId, [FromQuery(Name = "ciudadId")] Guid? ciudadIdQuery, CancellationToken ct)
    {
        var id = ciudadId ?? ciudadIdQuery;
        if (!id.HasValue)
            return BadRequest(ApiResponse<IReadOnlyList<GeoCatalogItemDto>>.Fail("ciudadId es obligatorio."));

        var result = await _service.GetZonasByCiudadAsync(id.Value, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<GeoCatalogItemDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<GeoCatalogItemDto>>.Ok(result.Value!));
    }
}
