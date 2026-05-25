namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad;

using System;

public class GestionContableDto
{
    public int GestionId { get; set; } // Same as Anio for now
    public int EmpresaId { get; set; }
    public int Anio { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activa { get; set; } // Does the current date fall into it? OR is it the specifically selected one?
    public bool EsGestionEnCurso { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = string.Empty; // Abierta / Cerrada
}

public class ContextoContableDto
{
    public int EmpresaId { get; set; }
    public int? GestionId { get; set; } // The active year manually chosen
    public int? AnioGestion { get; set; }
    public string? NombreGestion { get; set; }
}
