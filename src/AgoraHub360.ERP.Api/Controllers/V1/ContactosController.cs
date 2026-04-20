namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clientes/{clienteId:int}/contactos")]
[Authorize]
public class ContactosController : ControllerBase
{
    private readonly IContactoService _contactoService;
    private readonly IContactoUsuarioAccesoService _accesoService;

    public ContactosController(
        IContactoService contactoService,
        IContactoUsuarioAccesoService accesoService)
    {
        _contactoService = contactoService;
        _accesoService = accesoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int clienteId, CancellationToken ct)
    {
        var result = await _contactoService.GetAllByClienteAsync(clienteId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<ContactoDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<ContactoDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int clienteId, int id, CancellationToken ct)
    {
        var result = await _contactoService.GetByIdAsync(clienteId, id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<ContactoDto>.Fail(result.Error!));

        return Ok(ApiResponse<ContactoDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create(int clienteId, [FromBody] CreateContactoDto dto, CancellationToken ct)
    {
        var result = await _contactoService.CreateAsync(clienteId, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ContactoDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { clienteId, id = result.Value!.Id },
            ApiResponse<ContactoDto>.Ok(result.Value!, "Contacto creado exitosamente."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int clienteId, int id, [FromBody] UpdateContactoDto dto, CancellationToken ct)
    {
        var result = await _contactoService.UpdateAsync(clienteId, id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<ContactoDto>.Fail(result.Error!));

            return BadRequest(ApiResponse<ContactoDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<ContactoDto>.Ok(result.Value!, "Contacto actualizado exitosamente."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int clienteId, int id, CancellationToken ct)
    {
        var result = await _contactoService.DeleteAsync(clienteId, id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Contacto desactivado exitosamente."));
    }

    [HttpGet("{id:int}/acceso")]
    public async Task<IActionResult> GetAcceso(int clienteId, int id, CancellationToken ct)
    {
        var contactoResult = await _contactoService.GetByIdAsync(clienteId, id, ct);
        if (!contactoResult.IsSuccess)
            return NotFound(ApiResponse<ContactoUsuarioAccesoDto>.Fail(contactoResult.Error!));

        var result = await _accesoService.GetByContactoAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<ContactoUsuarioAccesoDto>.Fail(result.Error!));

        return Ok(ApiResponse<ContactoUsuarioAccesoDto>.Ok(result.Value!));
    }

    [HttpPut("{id:int}/acceso")]
    public async Task<IActionResult> UpsertAcceso(int clienteId, int id, [FromBody] UpsertContactoUsuarioAccesoDto dto, CancellationToken ct)
    {
        var contactoResult = await _contactoService.GetByIdAsync(clienteId, id, ct);
        if (!contactoResult.IsSuccess)
            return NotFound(ApiResponse<ContactoUsuarioAccesoDto>.Fail(contactoResult.Error!));

        var result = await _accesoService.UpsertAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ContactoUsuarioAccesoDto>.Fail(result.Error!));

        return Ok(ApiResponse<ContactoUsuarioAccesoDto>.Ok(result.Value!, "Acceso de contacto actualizado exitosamente."));
    }

    [HttpPatch("{id:int}/acceso/estado")]
    public async Task<IActionResult> SetAccesoEstado(int clienteId, int id, [FromBody] bool activo, CancellationToken ct)
    {
        var contactoResult = await _contactoService.GetByIdAsync(clienteId, id, ct);
        if (!contactoResult.IsSuccess)
            return NotFound(ApiResponse<bool>.Fail(contactoResult.Error!));

        var result = await _accesoService.SetActivoAsync(id, activo, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, activo ? "Acceso activado." : "Acceso desactivado."));
    }
}
