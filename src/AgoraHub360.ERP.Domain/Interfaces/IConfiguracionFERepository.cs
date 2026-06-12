using AgoraHub360.ERP.Domain.Entities.FE;

namespace AgoraHub360.ERP.Domain.Interfaces;

/// <summary>
/// Repositorio de configuración de Facturación Electrónica por empresa (tenant-aware).
/// </summary>
public interface IConfiguracionFERepository
{
    /// <summary>
    /// Obtiene la configuración activa de FE para una empresa específica.
    /// </summary>
    Task<ConfiguracionFacturacionElectronica?> ObtenerActivaPorEmpresaAsync(
        int empresaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista todas las configuraciones FE de una empresa.
    /// </summary>
    Task<IReadOnlyList<ConfiguracionFacturacionElectronica>> ListarPorEmpresaAsync(
        int empresaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una configuración FE por su Id.
    /// </summary>
    Task<ConfiguracionFacturacionElectronica?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega una nueva configuración FE.
    /// </summary>
    Task AgregarAsync(
        ConfiguracionFacturacionElectronica configuracion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una configuración FE existente.
    /// </summary>
    Task ActualizarAsync(
        ConfiguracionFacturacionElectronica configuracion,
        CancellationToken cancellationToken = default);
}
