namespace AgoraHub360.ERP.Domain.Entities.Core;

public class Ciudad
{
    public Guid Id { get; set; }
    public Guid ProvinciaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime? ModificadoEn { get; set; }

    public Provincia? Provincia { get; set; }
    public ICollection<Zona> Zonas { get; set; } = new List<Zona>();
}
