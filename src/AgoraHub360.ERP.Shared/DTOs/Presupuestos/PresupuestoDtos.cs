namespace AgoraHub360.ERP.Shared.DTOs.Presupuestos;

using System.Collections.Generic;

public class PresupuestoContableDto
{
    public int PresupuestoContableId { get; set; }
    public int Gestion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public List<PresupuestoContableLineaDto> Lineas { get; set; } = new();
}

public class PresupuestoContableLineaDto
{
    public int PresupuestoContableLineaId { get; set; }
    public int CuentaContableId { get; set; }
    public string? CuentaCodigo { get; set; }
    public string? CuentaNombre { get; set; }
    public int? CentroCostoId { get; set; }
    public string? CentroCostoNombre { get; set; }
    public int Mes { get; set; }
    public decimal MontoPresupuestado { get; set; }
}

public class CreatePresupuestoDto
{
    public int Gestion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public List<CreatePresupuestoContableLineaDto> Lineas { get; set; } = new();
}

public class CreatePresupuestoContableLineaDto
{
    public int CuentaContableId { get; set; }
    public int? CentroCostoId { get; set; }
    public int Mes { get; set; }
    public decimal MontoPresupuestado { get; set; }
}

public class PresupuestoVsRealDto
{
    public string CuentaCodigo { get; set; } = string.Empty;
    public string CuentaNombre { get; set; } = string.Empty;
    public int Mes { get; set; }
    public decimal MontoPresupuestado { get; set; }
    public decimal MontoReal { get; set; }
    public decimal Variacion { get; set; }
    public decimal VariacionPct { get; set; }
    public bool EsFavorable { get; set; }
    public int? CentroCostoId { get; set; }
    public string? CentroCostoNombre { get; set; }
}

public class CargaMasivaResultDto
{
    public int TotalFilas { get; set; }
    public int TotalValidas { get; set; }
    public int TotalErrores { get; set; }
    public List<string> Errores { get; set; } = new();
}
