namespace AgoraHub360.ERP.Domain.Entities.Core;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Controla el acceso de un usuario a sucursales por empresa.
/// </summary>
public class UsuarioSucursalAcceso : TenantEntity
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }

    public bool EsPredeterminada { get; set; }
    public bool PuedeConsultar { get; set; } = true;
    public bool PuedeOperar { get; set; } = true;
}
