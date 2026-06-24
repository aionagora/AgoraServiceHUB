using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgoraHub360.ERP.Api.Controllers.V1;

/// <summary>
/// API REST para Facturación Electrónica.
/// Endpoints para configuración FE, emisión, anulación y consulta de estado.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/facturacion-electronica")]
[Authorize]
[Produces("application/json")]
public class FacturacionElectronicaController : ControllerBase
{
    private readonly IFacturacionElectronicaService _feService;
    private readonly IConfiguracionFEService _configService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<FacturacionElectronicaController> _logger;

    public FacturacionElectronicaController(
        IFacturacionElectronicaService feService,
        IConfiguracionFEService configService,
        ICurrentUserService currentUser,
        ILogger<FacturacionElectronicaController> logger)
    {
        _feService = feService;
        _configService = configService;
        _currentUser = currentUser;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Catálogos globales
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Obtiene la lista de proveedores FE disponibles (catálogo global).
    /// </summary>
    [HttpGet("proveedores")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ProveedorFEDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProveedores(CancellationToken ct)
    {
        var result = await _configService.ListarProveedoresAsync(ct);
        if (!result.IsSuccess)
            return Ok(ApiResponse<IReadOnlyList<ProveedorFEDto>>.Ok(new List<ProveedorFEDto>()));
        return Ok(ApiResponse<IReadOnlyList<ProveedorFEDto>>.Ok(result.Value!));
    }

    /// <summary>
    /// Obtiene la lista de ambientes FE disponibles (catálogo global).
    /// </summary>
    [HttpGet("ambientes")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AmbienteFEDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAmbientes(CancellationToken ct)
    {
        var result = await _configService.ListarAmbientesAsync(ct);
        if (!result.IsSuccess)
            return Ok(ApiResponse<IReadOnlyList<AmbienteFEDto>>.Ok(new List<AmbienteFEDto>()));
        return Ok(ApiResponse<IReadOnlyList<AmbienteFEDto>>.Ok(result.Value!));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Configuración FE (tenant-aware)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Obtiene todas las configuraciones FE de la empresa activa.
    /// No expone secretos (ClientSecret, PosToken).
    /// </summary>
    [HttpGet("configuraciones")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ConfiguracionFEDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetConfiguraciones(CancellationToken ct)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return BadRequest(ApiResponse<IReadOnlyList<ConfiguracionFEDto>>.Fail("No se pudo determinar la empresa activa."));

        var result = await _configService.ListarPorEmpresaAsync(_currentUser.EmpresaId.Value, ct);
        return Ok(ApiResponse<IReadOnlyList<ConfiguracionFEDto>>.Ok(result.Value!));
    }

    /// <summary>
    /// Obtiene una configuración FE por su Id.
    /// </summary>
    [HttpGet("configuraciones/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ConfiguracionFEDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetConfiguracionById(int id, CancellationToken ct)
    {
        var result = await _configService.ObtenerPorIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<ConfiguracionFEDto>.Fail(result.Error!));

        return Ok(ApiResponse<ConfiguracionFEDto>.Ok(result.Value!));
    }

    /// <summary>
    /// Crea una nueva configuración FE. Los secretos se cifran al almacenarse.
    /// </summary>
    [HttpPost("configuraciones")]
    [ProducesResponseType(typeof(ApiResponse<ConfiguracionFEDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CrearConfiguracion(
        [FromBody] CrearConfiguracionFERequestDto dto,
        CancellationToken ct)
    {
        var result = await _configService.CrearAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ConfiguracionFEDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetConfiguracionById),
            new { id = result.Value!.Id },
            ApiResponse<ConfiguracionFEDto>.Ok(result.Value!, "Configuración FE creada exitosamente."));
    }

    /// <summary>
    /// Actualiza una configuración FE existente.
    /// Si ClientSecret o PosToken vienen vacíos, se conservan los valores cifrados actuales.
    /// </summary>
    [HttpPut("configuraciones/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ConfiguracionFEDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ActualizarConfiguracion(
        int id,
        [FromBody] ActualizarConfiguracionFERequestDto dto,
        CancellationToken ct)
    {
        var result = await _configService.ActualizarAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<ConfiguracionFEDto>.Fail(result.Error!));

            return BadRequest(ApiResponse<ConfiguracionFEDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<ConfiguracionFEDto>.Ok(result.Value!, "Configuración FE actualizada."));
    }

    /// <summary>
    /// Activa una configuración FE y desactiva las demás de la misma empresa.
    /// </summary>
    [HttpPost("configuraciones/{id:int}/activar")]
    [ProducesResponseType(typeof(ApiResponse<ConfiguracionFEDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ActivarConfiguracion(int id, CancellationToken ct)
    {
        var result = await _configService.ActivarAsync(id, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<ConfiguracionFEDto>.Fail(result.Error!));

            return BadRequest(ApiResponse<ConfiguracionFEDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<ConfiguracionFEDto>.Ok(result.Value!, "Configuración FE activada correctamente."));
    }

    /// <summary>
    /// Desactiva una configuración FE.
    /// </summary>
    [HttpPost("configuraciones/{id:int}/desactivar")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DesactivarConfiguracion(int id, CancellationToken ct)
    {
        var result = await _configService.DesactivarAsync(id, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<bool>.Fail(result.Error!));

            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        }

        return Ok(ApiResponse<bool>.Ok(true, "Configuración FE desactivada correctamente."));
    }

    /// <summary>
    /// Prueba la conexión contra el proveedor de una configuración FE.
    /// No requiere token de usuario autenticado — solo validación de empresa.
    /// </summary>
    [HttpPost("configuraciones/{id:int}/test-conexion")]
    [ProducesResponseType(typeof(ApiResponse<TestConexionResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TestConexion(int id, CancellationToken ct)
    {
        var result = await _configService.TestConexionAsync(id, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<TestConexionResultDto>.Fail(result.Error!));

            return BadRequest(ApiResponse<TestConexionResultDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<TestConexionResultDto>.Ok(result.Value!));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Operaciones FE
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Emite una factura electrónica contra el proveedor activo de la empresa.
    /// Si result.Exitoso = false, retorna HTTP 200 con el detalle del error de negocio.
    /// </summary>
    [HttpPost("emitir/{facturaVentaId:long}")]
    [ProducesResponseType(typeof(ApiResponse<EmisionFacturaResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Emitir(long facturaVentaId, CancellationToken ct)
    {
        var result = await _feService.EmitirAsync(facturaVentaId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<EmisionFacturaResultDto>.Fail(result.Error!));

        return Ok(ApiResponse<EmisionFacturaResultDto>.Ok(result.Value!));
    }

    /// <summary>
    /// Anula una factura electrónica previamente emitida contra el proveedor activo.
    /// </summary>
    [HttpPost("anular/{facturaVentaId:long}")]
    [ProducesResponseType(typeof(ApiResponse<AnulacionFacturaResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Anular(
        long facturaVentaId,
        [FromBody] AnularFacturaFERequestDto dto,
        CancellationToken ct)
    {
        var result = await _feService.AnularAsync(facturaVentaId, dto.MotivoAnulacion, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<AnulacionFacturaResultDto>.Fail(result.Error!));

            return BadRequest(ApiResponse<AnulacionFacturaResultDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<AnulacionFacturaResultDto>.Ok(result.Value!));
    }

    /// <summary>
    /// Consulta el estado actual de una factura electrónica contra el proveedor activo.
    /// </summary>
    [HttpGet("estado/{facturaVentaId:long}")]
    [ProducesResponseType(typeof(ApiResponse<EstadoFacturaResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerificarEstado(long facturaVentaId, CancellationToken ct)
    {
        var result = await _feService.VerificarEstadoAsync(facturaVentaId, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<EstadoFacturaResultDto>.Fail(result.Error!));

            return BadRequest(ApiResponse<EstadoFacturaResultDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<EstadoFacturaResultDto>.Ok(result.Value!));
    }
}
