namespace AgoraHub360.ERP.Web.Services;

using AgoraHub360.ERP.Shared.DTOs.Empresa;
using Microsoft.JSInterop;

/// <summary>
/// Servicio que mantiene el estado de la empresa activa.
/// Persiste en localStorage y notifica cambios a los componentes suscritos.
/// </summary>
public class EmpresaStateService
{
    private readonly IJSRuntime _js;
    private const string StorageKey = "agorahub360_empresa_activa";

    private EmpresaDto? _empresaActiva;
    private List<EmpresaDto> _empresas = new();

    public event Action? OnChange;

    public EmpresaStateService(IJSRuntime js)
    {
        _js = js;
    }

    public EmpresaDto? EmpresaActiva => _empresaActiva;
    public IReadOnlyList<EmpresaDto> Empresas => _empresas.AsReadOnly();
    public int? EmpresaActivaId => _empresaActiva?.Id;

    public void SetEmpresas(List<EmpresaDto> empresas)
    {
        _empresas = empresas;
    }

    public async Task SetEmpresaActivaAsync(EmpresaDto empresa)
    {
        _empresaActiva = empresa;
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, empresa.Id.ToString());
        OnChange?.Invoke();
    }

    public async Task LoadFromStorageAsync()
    {
        try
        {
            var stored = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (int.TryParse(stored, out var id))
            {
                _empresaActiva = _empresas.FirstOrDefault(e => e.Id == id);
            }

            // Si no hay empresa guardada o no se encontro, tomar la primera activa
            _empresaActiva ??= _empresas.FirstOrDefault(e => e.Activo);
        }
        catch
        {
            // SSR / prerender: localStorage no disponible
            _empresaActiva ??= _empresas.FirstOrDefault(e => e.Activo);
        }

        OnChange?.Invoke();
    }

    public async Task ClearAsync()
    {
        _empresaActiva = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        OnChange?.Invoke();
    }
}
