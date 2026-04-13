namespace AgoraHub360.ERP.Domain.Entities.ACC;

using System.Collections.Generic;
using AgoraHub360.ERP.Domain.Common;

public class PresupuestoContable : TenantEntity
{
    public int PresupuestoContableId { get; private set; }
    public int Gestion { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Estado { get; private set; } = "Borrador"; // Borrador|Aprobado|Cerrado
    public string? Observaciones { get; private set; }

    public ICollection<PresupuestoContableLinea> Lineas { get; private set; } = new List<PresupuestoContableLinea>();

    protected PresupuestoContable() { }

    public PresupuestoContable(int empresaId, int gestion, string nombre, string? observaciones = null)
    {
        EmpresaId = empresaId;
        Gestion = gestion;
        Nombre = nombre;
        Observaciones = observaciones;
    }

    public Result Aprobar()
    {
        if (Estado != "Borrador")
            return Result.Failure("Solo un presupuesto en borrador puede aprobarse.");

        Estado = "Aprobado";
        return Result.Success();
    }

    public Result Cerrar()
    {
        if (Estado != "Aprobado")
            return Result.Failure("Solo un presupuesto aprobado puede cerrarse.");

        Estado = "Cerrado";
        return Result.Success();
    }

    public Result AgregarLinea(PresupuestoContableLinea linea)
    {
        if (Estado != "Borrador")
            return Result.Failure("No se pueden agregar líneas a un presupuesto que no está en borrador.");

        Lineas.Add(linea);
        return Result.Success();
    }
}
