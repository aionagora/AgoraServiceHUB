namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad;

using System;
using System.Collections.Generic;

public class RatiosFinancierosDto
{
    // LIQUIDEZ
    public decimal LiquidezCorriente { get; set; }
    public decimal PruebaAcida { get; set; }
    public decimal RatioEfectivo { get; set; }

    // RENTABILIDAD
    public decimal ROE { get; set; }
    public decimal ROA { get; set; }
    public decimal MargenBruto { get; set; }
    public decimal MargenNeto { get; set; }
    public decimal EBITDA { get; set; }

    // ENDEUDAMIENTO
    public decimal RatioEndeudamiento { get; set; }
    public decimal ApalancamientoFinanciero { get; set; }

    // ACTIVIDAD
    public decimal RotacionInventario { get; set; }
    public decimal DiasInventario { get; set; }
    public decimal RotacionCxC { get; set; }
    public decimal DiasCobro { get; set; }

    // METADATA
    public DateTime FechaCorte { get; set; }
    public string Moneda { get; set; } = "BOB"; // Por defecto según el proyecto
    public Dictionary<string, string> Interpretaciones { get; set; } = new();
}
