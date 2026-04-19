namespace AgoraHub360.ERP.Domain.Entities.Core;

public class Departamento
{
    public Guid Id { get; set; }
    public Guid PaisId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime? ModificadoEn { get; set; }

    public Pais? Pais { get; set; }
    public ICollection<Provincia> Provincias { get; set; } = new List<Provincia>();
}
