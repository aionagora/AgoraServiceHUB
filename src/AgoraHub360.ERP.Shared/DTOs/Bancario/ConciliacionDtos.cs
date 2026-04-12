namespace AgoraHub360.ERP.Shared.DTOs.Bancario;

using System;
using System.Collections.Generic;

public class ImportExtractoResultDto
{
    public int TotalFilas { get; set; }
    public int TotalImportadas { get; set; }
    public int TotalErrores { get; set; }
    public List<string> Errores { get; set; } = new();
}

public class SugerenciaConciliacionDto
{
    public long ExtractoBancarioId { get; set; }
    public DateTime FechaExtracto { get; set; }
    public string DescripcionExtracto { get; set; } = string.Empty;
    public decimal MontoExtracto { get; set; }
    
    public long AsientoContableLineaId { get; set; }
    public DateTime FechaAsiento { get; set; }
    public string GlosaAsiento { get; set; } = string.Empty;
    public decimal MontoAsiento { get; set; }
    
    public int DiferenciaDias { get; set; }
    public bool MatchExactoMonto { get; set; }
    public decimal Score { get; set; }
}

public class ResumenConciliacionDto
{
    public int CuentaContableId { get; set; }
    public int PeriodoContableId { get; set; }
    public decimal SaldoExtracto { get; set; }
    public decimal SaldoContable { get; set; }
    public decimal Diferencia { get; set; }
    public bool EstaConciliado { get; set; }
    public string Estado { get; set; } = string.Empty;
    
    public int TotalMovimientosExtracto { get; set; }
    public int TotalMovimientosConciliados { get; set; }
    public int TotalMovimientosPendientes { get; set; }
}
