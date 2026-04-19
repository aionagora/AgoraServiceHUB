namespace AgoraHub360.ERP.Domain.Entities.Core;

public class Provincia
{
    public Guid Id { get; set; }
    public Guid DepartamentoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime? ModificadoEn { get; set; }

    public Departamento? Departamento { get; set; }
    public ICollection<Ciudad> Ciudades { get; set; } = new List<Ciudad>();
}
