namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad;

using System;

public class CierreContableDto
{
    public int Gestion { get; set; }
    public DateTime FechaCierre { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
}

public class EjecutarCierreDto
{
    public int Gestion { get; set; }
    public DateTime FechaCierre { get; set; }
}
