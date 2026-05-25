namespace AgoraHub360.ERP.Domain.Entities.Core;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Acción de seguridad aplicable sobre un formulario (Ver, Crear, Editar, etc.).
/// </summary>
public class AccionSistema : AuditableEntity
{
    public int Id { get; set; }
    public int FormularioSistemaId { get; set; }
    public FormularioSistema? FormularioSistema { get; set; }

    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Orden { get; set; }
}
