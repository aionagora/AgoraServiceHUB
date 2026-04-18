namespace AgoraHub360.ERP.Domain.Entities.Core;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Representa una sucursal, tienda o ubicación de la Empresa.
/// </summary>
public class Sucursal : TenantEntity
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Codigo { get; set; }

    public string? Direccion { get; set; }

    public string? Ciudad { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public bool EsCentral { get; set; }

    public bool Activo { get; set; } = true;

    // Relación
    public Empresa? Empresa { get; set; }
}
