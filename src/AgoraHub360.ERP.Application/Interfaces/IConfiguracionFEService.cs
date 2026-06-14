using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

namespace AgoraHub360.ERP.Application.Interfaces;

/// <summary>
/// Servicio de aplicación para gestionar configuraciones FE por empresa.
/// Cifra secretos al guardar y descifra solo para enviar al provider.
/// Garantiza una sola configuración activa por empresa.
/// </summary>
public interface IConfiguracionFEService
{
    /// <summary>
    /// Lista las configuraciones FE de una empresa.
    /// </summary>
    Task<Result<IReadOnlyList<ConfiguracionFEDto>>> ListarPorEmpresaAsync(
        int empresaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una configuración FE por Id.
    /// </summary>
    Task<Result<ConfiguracionFEDto>> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea una nueva configuración FE. Cifra ClientSecret y PosToken.
    /// </summary>
    Task<Result<ConfiguracionFEDto>> CrearAsync(
        CrearConfiguracionFERequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una configuración FE existente. Si ClientSecret/PosToken vienen null,
    /// conserva los valores cifrados actuales.
    /// </summary>
    Task<Result<ConfiguracionFEDto>> ActualizarAsync(
        int id,
        ActualizarConfiguracionFERequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activa una configuración FE y desactiva las demás de la misma empresa.
    /// </summary>
    Task<Result<ConfiguracionFEDto>> ActivarAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Desactiva una configuración FE.
    /// </summary>
    Task<Result<bool>> DesactivarAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista los proveedores FE disponibles (catálogo global).
    /// </summary>
    Task<Result<IReadOnlyList<ProveedorFEDto>>> ListarProveedoresAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista los ambientes FE disponibles (catálogo global).
    /// </summary>
    Task<Result<IReadOnlyList<AmbienteFEDto>>> ListarAmbientesAsync(
        CancellationToken cancellationToken = default);
}
