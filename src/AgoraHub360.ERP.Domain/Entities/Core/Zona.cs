namespace AgoraHub360.ERP.Domain.Entities.Core;

public class Zona
{
    public Guid Id { get; set; }
    public Guid CiudadId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime? ModificadoEn { get; set; }

    public Ciudad? Ciudad { get; set; }
}
