using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using Microsoft.AspNetCore.Components;

namespace AgoraHub360.ERP.Web.Pages.Contabilidad;

public partial class CierreContable
{
    private int selectedGestion = DateTime.Today.Year;
    private DateTime fechaCierre = new(DateTime.Today.Year, 12, 31);
    private bool abrirNuevaGestion;
    private string? observaciones;

    private bool cargandoEstado;
    private CierreContableDto? cierreExistente;

    private bool ejecutando;
    private bool mostrarConfirmacion;

    private string? errorMessage;
    private string? successMessage;
    private CierreContableDto? resultado;

    protected override async Task OnInitializedAsync()
    {
        await CargarEstadoCierre();
    }

    private async Task OnGestionChangedEvent(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int g))
            await OnGestionChanged(g);
    }

    private async Task OnGestionChanged(int newGestion)
    {
        selectedGestion = newGestion;
        fechaCierre = new DateTime(selectedGestion, 12, 31);
        resultado = null;
        errorMessage = null;
        successMessage = null;
        cierreExistente = null;
        await CargarEstadoCierre();
    }

    private async Task CargarEstadoCierre()
    {
        cargandoEstado = true;
        StateHasChanged();
        try
        {
            cierreExistente = await CierreService.ObtenerCierreAsync(selectedGestion);
        }
        catch
        {
            cierreExistente = null;
        }
        finally
        {
            cargandoEstado = false;
            StateHasChanged();
        }
    }

    private void SolicitarConfirmacion()
    {
        errorMessage = null;
        successMessage = null;
        mostrarConfirmacion = true;
    }

    private void CancelarConfirmacion()
    {
        mostrarConfirmacion = false;
    }

    private async Task EjecutarCierre()
    {
        mostrarConfirmacion = false;
        ejecutando = true;
        errorMessage = null;
        successMessage = null;
        resultado = null;
        StateHasChanged();

        try
        {
            var dto = new EjecutarCierreAnualDto
            {
                Gestion           = selectedGestion,
                FechaCierre       = fechaCierre,
                AbrirNuevaGestion = abrirNuevaGestion,
                Observaciones     = observaciones
            };

            var response = await CierreService.EjecutarCierreAnualAsync(dto);

            if (response.Success && response.Data != null)
            {
                resultado = response.Data;
                successMessage = $"Cierre contable anual de la gestión {selectedGestion} ejecutado exitosamente.";
                await CargarEstadoCierre();
            }
            else
            {
                errorMessage = response.Message ?? "Error desconocido al ejecutar el cierre anual.";
                if (response.Errors?.Count > 0)
                    errorMessage += " — " + string.Join("; ", response.Errors);
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error de comunicación: {ex.Message}";
        }
        finally
        {
            ejecutando = false;
        }
    }
}
