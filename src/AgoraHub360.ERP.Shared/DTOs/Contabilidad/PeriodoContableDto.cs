namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad;

/// <summary>DTO de lectura para Período Contable.</summary>
public class PeriodoContableDto
{
    public int PeriodoContableId { get; set; }
    public int EmpresaId { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime? FechaCierre { get; set; }
    public int? CerradoPorId { get; set; }
    public string? CerradoPorNombre { get; set; }
    public int CantidadAsientos { get; set; }
}

/// <summary>DTO para generar períodos de un año fiscal.</summary>
public class GenerarPeriodosDto
{
    public int Anio { get; set; }
}
