namespace AgoraHub360.ERP.Domain.Entities.Core;

public class Pais
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? CodigoIso2 { get; set; }
    public string? CodigoIso3 { get; set; }
    public bool Activo { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime? ModificadoEn { get; set; }

    public ICollection<Departamento> Departamentos { get; set; } = new List<Departamento>();
}
