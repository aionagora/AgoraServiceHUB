using AgoraHub360.ERP.Domain.Entities.FE;

namespace AgoraHub360.ERP.Domain.Interfaces;

/// <summary>
/// Repositorio del catálogo global de proveedores de Facturación Electrónica.
/// No es tenant-aware — los proveedores son catálogo del sistema.
/// </summary>
public interface IProveedorFERepository
{
    /// <summary>
    /// Lista todos los proveedores activos.
    /// </summary>
    Task<IReadOnlyList<ProveedorFacturacionElectronica>> ListarActivosAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un proveedor por su Id.
    /// </summary>
    Task<ProveedorFacturacionElectronica?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un proveedor por su código funcional. Ej: "CIRRUS".
    /// </summary>
    Task<ProveedorFacturacionElectronica?> ObtenerPorCodigoAsync(
        string codigo,
        CancellationToken cancellationToken = default);
}
