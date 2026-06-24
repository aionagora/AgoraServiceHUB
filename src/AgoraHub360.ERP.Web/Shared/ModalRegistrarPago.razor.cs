using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Ventas;
using AgoraHub360.ERP.Web.Helpers;
using AgoraHub360.ERP.Web.Services;
using Microsoft.AspNetCore.Components;

namespace AgoraHub360.ERP.Web.Shared;

/// <summary>
/// Modal reutilizable para registrar un pago a una venta existente.
/// Se abre llamando a <see cref="OpenAsync(VentaDto)"/> con la venta a pagar.
/// Emite <see cref="OnPagoRegistrado"/> cuando el pago se completa exitosamente.
/// </summary>
public partial class ModalRegistrarPago
{
    // ─── Inyecciones ──────────────────────────────────────────────────────────
    [Inject] private VentaHttpService VentaService { get; set; } = default!;

    // ─── Parámetros ───────────────────────────────────────────────────────────
    /// <summary>Callback que se invoca cuando un pago se registra exitosamente.</summary>
    [Parameter] public EventCallback<VentaDto> OnPagoRegistrado { get; set; }

    // ─── Estado interno ───────────────────────────────────────────────────────
    private bool _visible;
    private bool _isSubmitting;
    private string? _errorMessage;
    private string? _successMessage;

    private VentaDto? _venta;
    private decimal _totalPagado;
    private decimal _saldoPendiente;

    private readonly RegistrarPagoVentaRequestDto _model = new()
    {
        FechaPago = DateTime.Today,
        TipoPago = "Contado",
        ModoPago = "Efectivo",
        TipoCambio = 1m,
        Monto = 0
    };

    // ─── Opciones fijas (misma convención que VentaComercialForm) ─────────────
    private readonly string[] _tipoPagoOptions = ["Contado", "Credito", "Mixto"];
    private readonly string[] _modoPagoOptions = ["Efectivo", "QR", "Transferencia", "Tarjeta", "Cheque", "Deposito", "Otro"];

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>Abre el modal para registrar un pago a la venta indicada.</summary>
    public async Task OpenAsync(VentaDto venta)
    {
        _visible = true;
        _errorMessage = null;
        _successMessage = null;
        _isSubmitting = false;
        _venta = venta;

        // Calcular saldo pendiente
        _totalPagado = venta.Pagos?.Sum(p => p.Monto) ?? 0;
        _saldoPendiente = venta.Total - _totalPagado;

        // Resetear modelo
        _model.FechaPago = DateTime.Today;
        _model.Monto = _saldoPendiente > 0 ? _saldoPendiente : 0;
        _model.TipoPago = "Contado";
        _model.ModoPago = "Efectivo";
        _model.CuentaCajaBancoId = null;
        _model.MonedaId = venta.MonedaId;
        _model.MonedaCodigo = venta.MonedaCodigo;
        _model.TipoCambio = venta.TipoCambio > 0 ? venta.TipoCambio : 1m;
        _model.Referencia = null;
        _model.FacturaVentaId = null;
        _model.VentaId = (int)venta.Id;
        _model.EstadoPago = "Pagado";

        StateHasChanged();
        await Task.CompletedTask;
    }

    /// <summary>Cierra el modal.</summary>
    public async Task CloseAsync()
    {
        _visible = false;
        _venta = null;
        StateHasChanged();
        await Task.CompletedTask;
    }

    // ─── Handlers ─────────────────────────────────────────────────────────────

    private async Task SubmitAsync()
    {
        _errorMessage = null;
        _successMessage = null;

        // Validaciones del lado cliente
        if (_venta is null)
        {
            _errorMessage = "No hay una venta seleccionada.";
            return;
        }

        if (_model.Monto <= 0)
        {
            _errorMessage = "El monto del pago debe ser mayor a cero.";
            return;
        }

        if (_model.Monto > _saldoPendiente)
        {
            _errorMessage = $"El monto ({FormatMoney(_model.Monto)}) excede el saldo pendiente ({FormatMoney(_saldoPendiente)}).";
            return;
        }

        if (string.IsNullOrWhiteSpace(_model.TipoPago))
        {
            _errorMessage = "Debe seleccionar el tipo de pago.";
            return;
        }

        if (string.IsNullOrWhiteSpace(_model.ModoPago))
        {
            _errorMessage = "Debe seleccionar el modo de pago.";
            return;
        }

        _isSubmitting = true;
        StateHasChanged();

        try
        {
            var response = await VentaService.RegistrarPagoAsync(_venta.Id, _model);

            if (!response.Success || response.Data is null)
            {
                _errorMessage = BuildErrorMessage(response);
                _isSubmitting = false;
                StateHasChanged();
                return;
            }

            _successMessage = "Pago registrado exitosamente.";

            // Notificar al padre
            await OnPagoRegistrado.InvokeAsync(response.Data);

            // Cerrar después de 1.2s
            await Task.Delay(1200);
            await CloseAsync();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error al registrar el pago: {ex.Message}";
        }
        finally
        {
            _isSubmitting = false;
            StateHasChanged();
        }
    }

    private void OnBackdropClick() => _ = CloseAsync();

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static string FormatMoney(decimal value) => FormatHelper.Money(value);

    private static string BuildErrorMessage<T>(ApiResponse<T> response)
    {
        if (response.Errors is { Count: > 0 })
            return string.Join(" | ", response.Errors);

        return !string.IsNullOrWhiteSpace(response.Message)
            ? response.Message
            : "Error desconocido al procesar la solicitud.";
    }
}
