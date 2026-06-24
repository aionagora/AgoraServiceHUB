namespace AgoraHub360.ERP.Web.Services;

/// <summary>
/// Servicio reutilizable para descarga de PDFs desde el frontend Blazor.
/// Usa el HttpClient autenticado y JS interop para abrir/descargar el PDF.
/// No envía EmpresaId desde frontend — el backend lo resuelve del JWT.
/// </summary>
public class PdfDownloadService
{
    private readonly FileDownloadService _fileDownload;

    public PdfDownloadService(FileDownloadService fileDownload)
    {
        _fileDownload = fileDownload;
    }

    /// <summary>
    /// Descarga el recibo de pago PDF para un pago específico.
    /// GET /api/v1/cuentas-por-cobrar/pagos/{pagoId}/recibo-pdf
    /// </summary>
    public async Task DownloadReciboPagoAsync(long pagoId)
    {
        await _fileDownload.DownloadFromApiAsync(
            $"api/v1/cuentas-por-cobrar/pagos/{pagoId}/recibo-pdf",
            $"recibo-pago-{pagoId:D6}.pdf");
    }

    /// <summary>
    /// Descarga el recibo de pago PDF a partir del Id de una CuentaPorCobrar.
    /// El backend busca el pago asociado a la CxC.
    /// GET /api/v1/cuentas-por-cobrar/{cuentaPorCobrarId}/recibo-pdf
    /// </summary>
    public async Task DownloadReciboPagoPorCuentaAsync(long cuentaPorCobrarId)
    {
        await _fileDownload.DownloadFromApiAsync(
            $"api/v1/cuentas-por-cobrar/{cuentaPorCobrarId}/recibo-pdf",
            $"recibo-pago-cxc-{cuentaPorCobrarId:D6}.pdf");
    }

    /// <summary>
    /// Descarga el estado de cuenta PDF de un cliente.
    /// GET /api/v1/reportes/cxc/estado-cuenta-cliente/{clienteId}
    /// </summary>
    public async Task DownloadEstadoCuentaClienteAsync(
        int clienteId,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        bool soloPendientes = false,
        bool incluirPagos = false)
    {
        var queryParams = new List<string>();
        if (fechaDesde.HasValue)
            queryParams.Add($"fechaDesde={fechaDesde.Value:yyyy-MM-dd}");
        if (fechaHasta.HasValue)
            queryParams.Add($"fechaHasta={fechaHasta.Value:yyyy-MM-dd}");
        if (soloPendientes)
            queryParams.Add("soloPendientes=true");
        if (incluirPagos)
            queryParams.Add("incluirPagos=true");

        var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var url = $"api/v1/reportes/cxc/estado-cuenta-cliente/{clienteId}{query}";

        await _fileDownload.DownloadFromApiAsync(url, $"estado-cuenta-{clienteId}.pdf");
    }

    /// <summary>
    /// Descarga el PDF comercial de una venta (NOTA DE VENTA).
    /// GET /api/v1/reportes/ventas/{ventaId}/pdf
    /// </summary>
    public async Task DownloadVentaPdfAsync(long ventaId, string numeroVenta)
    {
        var nombre = string.IsNullOrWhiteSpace(numeroVenta)
            ? $"Venta-{ventaId:D6}.pdf"
            : $"Venta-{numeroVenta}.pdf";
        await _fileDownload.DownloadFromApiAsync(
            $"api/v1/reportes/ventas/{ventaId}/pdf",
            nombre);
    }

    /// <summary>
    /// Descarga la factura PDF.
    /// GET /api/v1/facturas-venta/{facturaId}/pdf
    /// </summary>
    public async Task DownloadFacturaPdfAsync(long facturaId)
    {
        await _fileDownload.DownloadFromApiAsync(
            $"api/v1/facturas-venta/{facturaId}/pdf",
            $"factura-{facturaId}.pdf");
    }

    /// <summary>
    /// Descarga el reporte general de CxC PDF.
    /// GET /api/v1/reportes/cxc/pdf
    /// </summary>
    public async Task DownloadReporteCxcGeneralAsync(
        int? clienteId = null,
        string? estado = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        bool? vencidas = null,
        bool? conSaldo = null)
    {
        var queryParams = new List<string>();
        if (clienteId.HasValue) queryParams.Add($"clienteId={clienteId}");
        if (!string.IsNullOrWhiteSpace(estado)) queryParams.Add($"estado={Uri.EscapeDataString(estado)}");
        if (fechaDesde.HasValue) queryParams.Add($"fechaDesde={fechaDesde.Value:yyyy-MM-dd}");
        if (fechaHasta.HasValue) queryParams.Add($"fechaHasta={fechaHasta.Value:yyyy-MM-dd}");
        if (vencidas == true) queryParams.Add("vencidas=true");
        if (conSaldo == true) queryParams.Add("conSaldo=true");

        var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        await _fileDownload.DownloadFromApiAsync(
            $"api/v1/reportes/cxc/pdf{query}",
            "reporte-cxc.pdf");
    }

    /// <summary>
    /// Descarga el reporte de CxC vencidas PDF.
    /// GET /api/v1/reportes/cxc/vencidas/pdf
    /// </summary>
    public async Task DownloadReporteCxcVencidasAsync()
    {
        await _fileDownload.DownloadFromApiAsync(
            "api/v1/reportes/cxc/vencidas/pdf",
            "reporte-cxc-vencidas.pdf");
    }
}
