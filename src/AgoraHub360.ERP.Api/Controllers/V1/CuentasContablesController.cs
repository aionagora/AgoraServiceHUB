namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contabilidad/cuentas")]
[Authorize]
public class CuentasContablesController : ControllerBase
{
    private readonly ICuentaContableService _service;

    public CuentasContablesController(ICuentaContableService service)
    {
        _service = service;
    }

    /// <summary>Lista todas las cuentas (flat) con filtros opcionales.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] byte? tipo,
        [FromQuery] bool? permiteMovimientos,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(tipo, permiteMovimientos, search, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<CuentaContableDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<CuentaContableDto>>.Ok(result.Value!));
    }

    /// <summary>Obtiene el plan de cuentas en estructura de árbol.</summary>
    [HttpGet("tree")]
    public async Task<IActionResult> GetTree(CancellationToken ct)
    {
        var result = await _service.GetTreeAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<CuentaContableDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<CuentaContableDto>>.Ok(result.Value!));
    }

    /// <summary>Obtiene una cuenta por Id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<CuentaContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<CuentaContableDto>.Ok(result.Value!));
    }

    /// <summary>Crea una nueva cuenta contable.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCuentaContableDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CuentaContableDto>.Fail(result.Error!));
        return CreatedAtAction(nameof(GetById),
            new { id = result.Value!.CuentaContableId },
            ApiResponse<CuentaContableDto>.Ok(result.Value!, "Cuenta creada."));
    }

    /// <summary>Actualiza una cuenta existente.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCuentaContableDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CuentaContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<CuentaContableDto>.Ok(result.Value!, "Cuenta actualizada."));
    }

    /// <summary>Elimina una cuenta sin movimientos ni subcuentas.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Cuenta eliminada."));
    }

    /// <summary>Genera el plan de cuentas estándar Bolivia/NIIF.</summary>
    [HttpPost("seed")]
    public async Task<IActionResult> Seed(CancellationToken ct)
    {
        var result = await _service.SeedPlanCuentasAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<int>.Fail(result.Error!));
        return Ok(ApiResponse<int>.Ok(result.Value!, $"Plan de cuentas generado: {result.Value} cuentas creadas."));
    }

    // ── Importación CSV ─────────────────────────────────────────────────────

    /// <summary>Descarga la plantilla CSV de ejemplo.</summary>
    [HttpGet("import-template")]
    public IActionResult DownloadTemplate()
    {
        var csv = "Codigo;Nombre;TipoCuenta;Naturaleza;CodigoPadre;Nivel;EsMovimiento;Activo;Descripcion\n"
                + "1;ACTIVO;Activo;Deudora;;1;false;true;\n"
                + "1.1;ACTIVO CORRIENTE;Activo;Deudora;1;2;false;true;\n"
                + "1.1.1;DISPONIBLE;Activo;Deudora;1.1;3;false;true;\n"
                + "1.1.1.01;CAJA;Activo;Deudora;1.1.1;4;true;true;Caja general\n"
                + "1.1.1.02;BANCOS;Activo;Deudora;1.1.1;4;true;true;Cuentas bancarias\n";
        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv)).ToArray();
        return File(bytes, "text/csv", "plantilla-plan-cuentas.csv");
    }

    /// <summary>Valida un archivo CSV y devuelve vista previa.</summary>
    [HttpPost("import-preview")]
    public async Task<IActionResult> ImportPreview(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<PlanCuentaImportPreviewDto>.Fail("Debe enviar un archivo CSV."));

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(ApiResponse<PlanCuentaImportPreviewDto>.Fail("Solo se aceptan archivos CSV."));

        using var reader = new StreamReader(file.OpenReadStream());
        var csvContent = await reader.ReadToEndAsync(ct);
        var result = await _service.PreviewImportAsync(csvContent, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<PlanCuentaImportPreviewDto>.Fail(result.Error!));

        return Ok(ApiResponse<PlanCuentaImportPreviewDto>.Ok(result.Value!));
    }

    /// <summary>Ejecuta la importación de un archivo CSV previamente validado.</summary>
    [HttpPost("import-confirm")]
    public async Task<IActionResult> ImportConfirm(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<PlanCuentaImportResultDto>.Fail("Debe enviar un archivo CSV."));

        using var reader = new StreamReader(file.OpenReadStream());
        var csvContent = await reader.ReadToEndAsync(ct);
        var result = await _service.ConfirmImportAsync(csvContent, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<PlanCuentaImportResultDto>.Fail(result.Error!));

        return Ok(ApiResponse<PlanCuentaImportResultDto>.Ok(result.Value!));
    }
}
