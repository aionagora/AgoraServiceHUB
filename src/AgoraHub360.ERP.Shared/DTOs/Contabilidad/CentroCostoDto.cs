namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad;

/// <summary>DTO de lectura para Centro de Costo.</summary>
public class CentroCostoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? ParentId { get; set; }
    public string? ParentNombre { get; set; }
    public int EmpresaId { get; set; }
    public bool Activo { get; set; }
}

/// <summary>DTO para crear un Centro de Costo.</summary>
public class CreateCentroCostoDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? ParentId { get; set; }
}

/// <summary>DTO para actualizar un Centro de Costo.</summary>
public class UpdateCentroCostoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? ParentId { get; set; }
}
