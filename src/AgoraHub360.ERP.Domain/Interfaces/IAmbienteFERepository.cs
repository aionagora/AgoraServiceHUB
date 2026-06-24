using AgoraHub360.ERP.Domain.Entities.FE;

namespace AgoraHub360.ERP.Domain.Interfaces;

/// <summary>
/// Repositorio del catálogo global de ambientes de Facturación Electrónica.
/// No es tenant-aware — los ambientes son catálogo del sistema.
/// </summary>
public interface IAmbienteFERepository
{
    /// <summary>
    /// Lista todos los ambientes activos.
    /// </summary>
    Task<IReadOnlyList<AmbienteFacturacionElectronica>> ListarActivosAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un ambiente por su Id.
    /// </summary>
    Task<AmbienteFacturacionElectronica?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un ambiente por su código funcional. Ej: "TEST", "PRODUCCION".
    /// </summary>
    Task<AmbienteFacturacionElectronica?> ObtenerPorCodigoAsync(
        string codigo,
        CancellationToken cancellationToken = default);
}
