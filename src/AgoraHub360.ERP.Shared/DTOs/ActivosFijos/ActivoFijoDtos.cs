namespace AgoraHub360.ERP.Shared.DTOs.ActivosFijos;

using System;
using System.Collections.Generic;

public class ActivoFijoDto
{
    public long ActivoFijoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string CategoriaActivo { get; set; } = string.Empty;
    public int CuentaContableId { get; set; }
    public int CuentaDepreciacionId { get; set; }
    public int CuentaGastoDepreciacionId { get; set; }
    public DateTime FechaAdquisicion { get; set; }
    public decimal CostoAdquisicion { get; set; }
    public decimal ValorResidual { get; set; }
    public decimal TasaAnualDS24051 { get; set; }
    public int VidaUtilAnios { get; set; }
    public decimal DepreciacionAcumulada { get; set; }
    public decimal ValorEnLibros { get; set; }
    public bool DepreciacionCompleta { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class CreateActivoFijoDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string CategoriaActivo { get; set; } = string.Empty;
    public int CuentaContableId { get; set; }
    public int CuentaDepreciacionId { get; set; }
    public int CuentaGastoDepreciacionId { get; set; }
    public DateTime FechaAdquisicion { get; set; }
    public decimal CostoAdquisicion { get; set; }
    public decimal ValorResidual { get; set; }
    public decimal TasaAnualDS24051 { get; set; }
    public int VidaUtilAnios { get; set; }
}

public class DepreciacionMensualResultDto
{
    public int TotalActivosProcesados { get; set; }
    public decimal TotalMontoDepreciado { get; set; }
    public int TotalErrores { get; set; }
    public List<string> Errores { get; set; } = new();
}

public class DepreciacionReporteDto
{
    public string CodigoActivo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string CategoriaActivo { get; set; } = string.Empty;
    public int PeriodoContableId { get; set; }
    public DateTime Fecha { get; set; }
    public decimal MontoDepreciacion { get; set; }
    public decimal DepreciacionAcumulada { get; set; }
    public decimal ValorEnLibros { get; set; }
    public string Estado { get; set; } = string.Empty;
}
