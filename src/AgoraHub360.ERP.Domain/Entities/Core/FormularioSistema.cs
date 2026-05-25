namespace AgoraHub360.ERP.Domain.Entities.Core;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Pantalla o formulario navegable del sistema.
/// </summary>
public class FormularioSistema : AuditableEntity
{
    public int Id { get; set; }
    public int ModuloSistemaId { get; set; }
    public ModuloSistema? ModuloSistema { get; set; }

    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
    public string? Icono { get; set; }
    public int Orden { get; set; }
    public bool VisibleEnMenu { get; set; } = true;

    public ICollection<AccionSistema> Acciones { get; set; } = new List<AccionSistema>();
}
