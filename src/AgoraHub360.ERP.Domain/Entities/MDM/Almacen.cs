namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;

/// <summary>
/// Almacén o bodega de la empresa.
/// </summary>
public class Almacen : TenantEntity
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    public int? SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }

    public Empresa? Empresa { get; set; }

    public string? Direccion { get; set; }
    public string? Responsable { get; set; }
    public string? Telefono { get; set; }
}
