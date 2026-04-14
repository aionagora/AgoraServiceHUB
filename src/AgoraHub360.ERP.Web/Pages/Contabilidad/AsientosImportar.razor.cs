using AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
namespace AgoraHub360.ERP.Web.Pages.Contabilidad;

public partial class AsientosImportar
{
    private bool descargandoPlantilla = false;
    private bool validando = false;
    private bool importando = false;
    private bool procesando => validando || importando;
    private bool puedeImportar => archivoSeleccionado != null && validacionResult != null && validacionResult.EsValido && !procesando;

    private string? successMessage;
    private string? errorMessage;
    private bool contabilizarInmediatamente = false;

    private IBrowserFile? archivoSeleccionado;
    private Stream? fileStream;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    private ImportValidacionDto? validacionResult;
    private ImportResultDto? importacionResult;

    private async Task DescargarPlantilla()
    {
        try
        {
            descargandoPlantilla = true;
            errorMessage = null;

            var bytes = await AsientoService.DescargarPlantillaImportacionAsync();
            if (bytes != null)
            {
                var fileName = $"Plantilla_Importacion_Asientos_{DateTime.Today:yyyyMMdd}.xlsx";
                await GuardarArchivoLocalAsync(fileName, bytes);
            }
            else
            {
                errorMessage = "Error al descargar la plantilla desde el servidor.";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error técnico: {ex.Message}";
        }
        finally
        {
            descargandoPlantilla = false;
        }
    }

    private async Task OnInputFileChange(InputFileChangeEventArgs e)
    {
        errorMessage = null;
        successMessage = null;
        validacionResult = null;
        importacionResult = null;
        archivoSeleccionado = null;

        var file = e.File;
        
        // Validar tamaño y extensión básica
        if (file.Size > MaxFileSize)
        {
            errorMessage = "El archivo excede el tamaño máximo permitido (5 MB).";
            return;
        }
        if (!file.Name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = "El archivo debe ser un Excel (.xlsx).";
            return;
        }

        archivoSeleccionado = file;
        fileStream = file.OpenReadStream(MaxFileSize);
    }

    private async Task ValidarArchivo()
    {
        if (archivoSeleccionado == null || fileStream == null)
            return;

        try
        {
            validando = true;
            errorMessage = null;
            successMessage = null;
            importacionResult = null;

            // Restablecer el stream
            fileStream.Position = 0;

            var response = await AsientoService.ValidarImportacionAsync(fileStream, archivoSeleccionado.Name);

            if (response.Success && response.Data != null)
            {
                validacionResult = response.Data;
                if (validacionResult.EsValido)
                {
                    successMessage = $"Archivo validado correctamente. {validacionResult.TotalAsientosDetectados} asientos detectados.";
                }
                else
                {
                    errorMessage = $"El archivo tiene {validacionResult.Errores.Count} error(es) estructurales o de negocio. Revise el panel de resultados.";
                }
            }
            else
            {
                errorMessage = response.Message ?? "Error desconocido en la validación.";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Ocurrió un error al validar: {ex.Message}";
        }
        finally
        {
            validando = false;
        }
    }

    private async Task ImportarArchivo()
    {
        if (!puedeImportar || fileStream == null || archivoSeleccionado == null)
            return;

        try
        {
            importando = true;
            errorMessage = null;
            successMessage = null;
            validacionResult = null;

            // Restablecer el stream
            fileStream.Position = 0;

            var response = await AsientoService.ImportarAsync(fileStream, archivoSeleccionado.Name, contabilizarInmediatamente);

            if (response.Success && response.Data != null)
            {
                importacionResult = response.Data;
                
                if (importacionResult.TotalErrores == 0)
                {
                    successMessage = $"Importación exitosa. Se crearon {importacionResult.TotalAsientosImportados} asientos con {importacionResult.TotalLineasImportadas} líneas.";
                    // Limpiamos el archivo para evitar re-importación accidental
                    archivoSeleccionado = null;
                    fileStream = null;
                }
                else
                {
                    errorMessage = $"Importación parcial con {importacionResult.TotalErrores} error(es). Se importaron {importacionResult.TotalAsientosImportados} asientos.";
                }
            }
            else
            {
                errorMessage = response.Message ?? "Error desconocido al importar el archivo.";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Ocurrió un error al importar: {ex.Message}";
        }
        finally
        {
            importando = false;
        }
    }

    private async Task GuardarArchivoLocalAsync(string filename, byte[] content)
    {
        await FileDownload.DownloadFromBase64Async(filename, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", content);
    }
}
