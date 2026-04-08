namespace AgoraHub360.ERP.Web.Services;

using AgoraHub360.ERP.Shared.DTOs.Compras;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using AgoraHub360.ERP.Shared.DTOs.Inventario;
using AgoraHub360.ERP.Shared.DTOs.MDM;

/// <summary>
/// Agrega en una sola llamada paralela todos los KPIs
/// necesarios para los dashboards principal y por módulo.
/// </summary>
public class DashboardDataService
{
    private readonly OrdenPedidoHttpService _opSvc;
    private readonly OrdenCompraHttpService _ocSvc;
    private readonly RecepcionCompraHttpService _recSvc;
    private readonly HojaImportacionHttpService _impSvc;
    private readonly ExpedienteImportacionHttpService _expSvc;
    private readonly MovimientoInventarioHttpService _movSvc;
    private readonly AsientoContableHttpService _asientoSvc;
    private readonly MdmCompanyProductHttpService _cpSvc;
    private readonly ClienteHttpService _clienteSvc;
    private readonly ProveedorHttpService _proveedorSvc;

    public DashboardDataService(
        OrdenPedidoHttpService opSvc,
        OrdenCompraHttpService ocSvc,
        RecepcionCompraHttpService recSvc,
        HojaImportacionHttpService impSvc,
        ExpedienteImportacionHttpService expSvc,
        MovimientoInventarioHttpService movSvc,
        AsientoContableHttpService asientoSvc,
        MdmCompanyProductHttpService cpSvc,
        ClienteHttpService clienteSvc,
        ProveedorHttpService proveedorSvc)
    {
        _opSvc = opSvc; _ocSvc = ocSvc; _recSvc = recSvc;
        _impSvc = impSvc; _expSvc = expSvc; _movSvc = movSvc;
        _asientoSvc = asientoSvc; _cpSvc = cpSvc;
        _clienteSvc = clienteSvc; _proveedorSvc = proveedorSvc;
    }

    // ?? Compras ???????????????????????????????????????????????????????????????
    public async Task<ComprasKpis> GetComprasKpisAsync()
    {
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        var (ops, ocs, recs, exps, imps) = await (
            _opSvc.GetAllAsync(),
            _ocSvc.GetAllAsync(),
            _recSvc.GetAllAsync(),
            _expSvc.GetAllAsync(),
            _impSvc.GetAllAsync()
        ).WhenAll();

        return new ComprasKpis
        {
            // OP
            OpBorrador          = ops.Count(o => o.Estado == "Borrador"),
            OpEnRevision        = ops.Count(o => o.Estado == "EnRevision"),
            OpPendienteAprobacion = ops.Count(o => o.Estado == "PendienteAprobacion"),
            OpAprobadas         = ops.Count(o => o.Estado == "Aprobado"),
            // OC
            OcTotal             = ocs.Count,
            OcBorrador          = ocs.Count(o => o.Estado == "Borrador"),
            OcAprobadas         = ocs.Count(o => o.Estado == "Aprobado"),
            OcEnProceso         = ocs.Count(o => o.Estado is "ConfirmadaProveedor" or "PagoProgramado"),
            OcCerradas          = ocs.Count(o => o.Estado == "Cerrado"),
            TotalComprasMes     = ocs.Where(o => o.FechaEmision >= inicioMes).Sum(o => o.Total),
            // Recepciones
            RecepcionesMes      = recs.Count(r => r.FechaRecepcion >= inicioMes),
            RecepcionesConDif   = recs.Count(r => r.TieneDiferencias),
            // Expedientes
            ExpEnTransito       = exps.Count(e => e.Estado == "EnTransito"),
            ExpEnAduana         = exps.Count(e => e.Estado is "EnAduana" or "ObservacionAduana"),
            ExpLiberados        = exps.Count(e => e.Estado == "Liberado"),
            // Hojas importacion
            HojasNoLiquidadas   = imps.Count(h => !h.Liquidada),
            // Alertas
            OcPendientesAccion  = ocs.Where(o => o.Estado == "Borrador" || o.Estado == "Confirmado").ToList(),
            ExpedientesActivos  = exps.Where(e => e.Estado is "EnTransito" or "Arribado" or "EnAduana" or "ObservacionAduana").ToList(),
            UltimasRecepciones  = recs.OrderByDescending(r => r.FechaRecepcion).Take(5).ToList(),
        };
    }

    // ?? Inventario ????????????????????????????????????????????????????????????
    public async Task<InventarioKpis> GetInventarioKpisAsync()
    {
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        var (movs, prods) = await (
            _movSvc.GetMovimientosAsync(desde: inicioMes),
            _cpSvc.GetAllAsync()
        ).WhenAll();

        return new InventarioKpis
        {
            ProductosActivos   = prods.Count(p => p.Activo),
            EntradaMes         = movs.Count(m => m.MovementType == "Entrada"),
            SalidaMes          = movs.Count(m => m.MovementType == "Salida"),
            MovimientosMes     = movs.Count,
            UltimosMovimientos = movs.OrderByDescending(m => m.MovementDate).Take(8).ToList(),
        };
    }

    // ?? Contabilidad ??????????????????????????????????????????????????????????
    public async Task<ContabilidadKpis> GetContabilidadKpisAsync()
    {
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        var asientos = await _asientoSvc.GetAllAsync(desde: inicioMes);

        return new ContabilidadKpis
        {
            AsientosMes         = asientos.Count,
            AsientosBorrador    = asientos.Count(a => a.Estado == "Borrador"),
            AsientosContabilizados = asientos.Count(a => a.Estado == "Contabilizado"),
            TotalDebitoMes      = asientos.Where(a => a.Estado == "Contabilizado").Sum(a => a.TotalDebe),
            UltimosAsientos     = asientos.OrderByDescending(a => a.Fecha).Take(6).ToList(),
        };
    }

    // ?? MDM ???????????????????????????????????????????????????????????????????
    public async Task<MdmKpis> GetMdmKpisAsync()
    {
        var (prods, clientes, proveedores) = await (
            _cpSvc.GetAllAsync(),
            _clienteSvc.GetAllAsync(),
            _proveedorSvc.GetAllAsync()
        ).WhenAll();

        return new MdmKpis
        {
            ProductosActivos    = prods.Count(p => p.Activo),
            ClientesActivos     = clientes.Count(c => c.Activo),
            ProveedoresActivos  = proveedores.Count(p => p.Activo),
        };
    }

    // ?? Principal (agrega todo) ???????????????????????????????????????????????
    public async Task<(ComprasKpis Compras, InventarioKpis Inventario, ContabilidadKpis Contabilidad, MdmKpis Mdm)>
        GetAllKpisAsync()
    {
        var (c, i, ct, m) = await (
            GetComprasKpisAsync(),
            GetInventarioKpisAsync(),
            GetContabilidadKpisAsync(),
            GetMdmKpisAsync()
        ).WhenAll();
        return (c, i, ct, m);
    }
}

// ?? Modelos de KPIs ???????????????????????????????????????????????????????????

public class ComprasKpis
{
    public int OpBorrador { get; set; }
    public int OpEnRevision { get; set; }
    public int OpPendienteAprobacion { get; set; }
    public int OpAprobadas { get; set; }
    public int OcTotal { get; set; }
    public int OcBorrador { get; set; }
    public int OcAprobadas { get; set; }
    public int OcEnProceso { get; set; }
    public int OcCerradas { get; set; }
    public decimal TotalComprasMes { get; set; }
    public int RecepcionesMes { get; set; }
    public int RecepcionesConDif { get; set; }
    public int ExpEnTransito { get; set; }
    public int ExpEnAduana { get; set; }
    public int ExpLiberados { get; set; }
    public int HojasNoLiquidadas { get; set; }
    public List<OrdenCompraDto> OcPendientesAccion { get; set; } = new();
    public List<ExpedienteImportacionDto> ExpedientesActivos { get; set; } = new();
    public List<RecepcionCompraDto> UltimasRecepciones { get; set; } = new();
}

public class InventarioKpis
{
    public int ProductosActivos { get; set; }
    public int EntradaMes { get; set; }
    public int SalidaMes { get; set; }
    public int MovimientosMes { get; set; }
    public List<MovimientoInventarioDto> UltimosMovimientos { get; set; } = new();
}

public class ContabilidadKpis
{
    public int AsientosMes { get; set; }
    public int AsientosBorrador { get; set; }
    public int AsientosContabilizados { get; set; }
    public decimal TotalDebitoMes { get; set; }
    public List<AsientoContableDto> UltimosAsientos { get; set; } = new();
}

public class MdmKpis
{
    public int ProductosActivos { get; set; }
    public int ClientesActivos { get; set; }
    public int ProveedoresActivos { get; set; }
}
