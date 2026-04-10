namespace AgoraHub360.ERP.Domain.Entities.ACC;

using AgoraHub360.ERP.Domain.Common;
using System;

public class CierreContable : TenantEntity
{
    public Guid Id { get; private set; }
    public int Gestion { get; private set; }
    public DateTime FechaCierre { get; private set; }
    public string Estado { get; private set; } = "BORRADOR";
    public string Observaciones { get; private set; } = string.Empty;

    private CierreContable() 
    {
    }

    private CierreContable(int empresaId, int gestion, DateTime fechaCierre)
    {
        Id = Guid.NewGuid();
        EmpresaId = empresaId;
        Gestion = gestion;
        FechaCierre = fechaCierre;
        Estado = "BORRADOR";
    }

    public static CierreContable Crear(int empresaId, int gestion, DateTime fecha)
    {
        return new CierreContable(empresaId, gestion, fecha);
    }

    public void MarcarComoCerrado()
    {
        Estado = "CERRADO";
    }

    public void Anular()
    {
        Estado = "ANULADO";
    }

    public void ActualizarObservaciones(string observaciones)
    {
        Observaciones = observaciones ?? string.Empty;
    }
}
