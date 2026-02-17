namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Empresa;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class EmpresasController : ControllerBase
{
    private readonly IEmpresaService _empresaService;

    public EmpresasController(IEmpresaService empresaService)
    {
        _empresaService = empresaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _empresaService.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<EmpresaDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _empresaService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<EmpresaDto>.Fail(result.Error!));

        return Ok(ApiResponse<EmpresaDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmpresaDto dto, CancellationToken ct)
    {
        var result = await _empresaService.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<EmpresaDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<EmpresaDto>.Ok(result.Value!, "Empresa creada exitosamente."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmpresaDto dto, CancellationToken ct)
    {
        var result = await _empresaService.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada"))
                return NotFound(ApiResponse<EmpresaDto>.Fail(result.Error!));
            return BadRequest(ApiResponse<EmpresaDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<EmpresaDto>.Ok(result.Value!, "Empresa actualizada exitosamente."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _empresaService.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Empresa eliminada exitosamente."));
    }
}
