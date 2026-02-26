namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad;

/// <summary>DTO de lectura para Asiento Contable.</summary>
public class AsientoContableDto
{
    public long AsientoContableId { get; set; }
    public int EmpresaId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Glosa { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? OrigenTipo { get; set; }
    public long? OrigenId { get; set; }
    public string? OrigenReferencia { get; set; }
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
    public bool Cuadrado { get; set; }
    public List<AsientoContableLineaDto> Lineas { get; set; } = new();
}

/// <summary>DTO de lectura para línea de asiento.</summary>
public class AsientoContableLineaDto
{
    public long AsientoContableLineaId { get; set; }
    public int NumeroLinea { get; set; }
    public int CuentaContableId { get; set; }
    public string CuentaCodigo { get; set; } = string.Empty;
    public string CuentaNombre { get; set; } = string.Empty;
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public string? Glosa { get; set; }
    public string? Referencia { get; set; }
}

/// <summary>DTO para crear un asiento contable manual.</summary>
public class CreateAsientoContableDto
{
    public DateTime Fecha { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public string? OrigenTipo { get; set; }
    public long? OrigenId { get; set; }
    public string? OrigenReferencia { get; set; }
    public List<CreateAsientoLineaDto> Lineas { get; set; } = new();
}

/// <summary>DTO para crear una línea de asiento.</summary>
public class CreateAsientoLineaDto
{
    public int CuentaContableId { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public string? Glosa { get; set; }
    public string? Referencia { get; set; }
}
