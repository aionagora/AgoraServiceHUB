namespace AgoraHub360.ERP.Web.Services;

using AgoraHub360.ERP.Shared.DTOs.Empresa;
using AgoraHub360.ERP.Shared.DTOs.Auth;
using AgoraHub360.ERP.Shared.DTOs;
using Microsoft.JSInterop;

/// <summary>
/// Servicio que mantiene el estado de la empresa activa.
/// Persiste en localStorage y notifica cambios a los componentes suscritos.
/// </summary>
public class EmpresaStateService
{
    private readonly IJSRuntime _js;
    private readonly AuthHttpService _authHttp;
    private readonly JwtAuthStateProvider _authState;
    private const string StorageKey = "agorahub360_empresa_activa";

    private EmpresaDto? _empresaActiva;
    private List<EmpresaDto> _empresas = new();

    public event Action? OnChange;

    public EmpresaStateService(IJSRuntime js, AuthHttpService authHttp, JwtAuthStateProvider authState)
    {
        _js = js;
        _authHttp = authHttp;
        _authState = authState;
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

    public async Task<ApiResponse<CambiarEmpresaResponseDto>> CambiarEmpresaActivaAsync(int empresaId)
    {
        var response = await _authHttp.SeleccionarEmpresaAsync(new SeleccionarEmpresaRequestDto { EmpresaId = empresaId });
        if (!response.Success || response.Data is null)
            return response;

        await _authState.ReplaceTokenAsync(response.Data.Token);

        if (response.Data.EmpresasDisponibles.Any())
        {
            _empresas = response.Data.EmpresasDisponibles
                .Select(e => new EmpresaDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    NIT = e.Nit,
                    Activo = e.EsActiva,
                    FechaCreacion = DateTime.UtcNow
                }).ToList();
        }

        _empresaActiva = _empresas.FirstOrDefault(e => e.Id == response.Data.EmpresaActiva.Id)
            ?? new EmpresaDto
            {
                Id = response.Data.EmpresaActiva.Id,
                Nombre = response.Data.EmpresaActiva.Nombre,
                NIT = response.Data.EmpresaActiva.Nit,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, _empresaActiva.Id.ToString());
        OnChange?.Invoke();
        return response;
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
