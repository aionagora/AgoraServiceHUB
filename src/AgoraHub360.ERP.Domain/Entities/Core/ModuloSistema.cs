namespace AgoraHub360.ERP.Domain.Entities.Core;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Catálogo de módulos funcionales del ERP para menú y seguridad dinámica.
/// </summary>
public class ModuloSistema : AuditableEntity
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public int Orden { get; set; }

    public ICollection<FormularioSistema> Formularios { get; set; } = new List<FormularioSistema>();
}
