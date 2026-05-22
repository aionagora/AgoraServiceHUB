namespace AgoraHub360.ERP.Web.Services;

using AgoraHub360.ERP.Shared.DTOs.Seguridad;
using Microsoft.JSInterop;

public class SesionUsuarioStateService
{
    private readonly SeguridadDinamicaHttpService _seguridadHttp;
    private readonly IJSRuntime _js;
    private const string SucursalStorageKey = "agorahub360_sucursal_activa";

    private SesionContextoDto? _contexto;
    private int? _sucursalActivaId;

    public event Action? OnChange;

    public SesionUsuarioStateService(SeguridadDinamicaHttpService seguridadHttp, IJSRuntime js)
    {
        _seguridadHttp = seguridadHttp;
        _js = js;
    }

    public SesionContextoDto? Contexto => _contexto;
    public bool IsLoaded => _contexto is not null;
    public IReadOnlyList<ModuloSistemaDto> Modulos => _contexto is null ? new List<ModuloSistemaDto>() : _contexto.ModulosPermitidos;
    public IReadOnlyList<PerfilUsuarioSesionDto> Perfiles => _contexto is null ? new List<PerfilUsuarioSesionDto>() : _contexto.Perfiles;
    public IReadOnlyList<UsuarioSucursalAccesoDto> Sucursales => _contexto is null ? new List<UsuarioSucursalAccesoDto>() : _contexto.SucursalesPermitidas;

    public int? SucursalActivaId => _sucursalActivaId;

    public async Task InitializeAsync(bool forceReload = false)
    {
        if (!forceReload && _contexto is not null)
            return;

        if (forceReload)
        {
            if (_contexto is not null)
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", BuildSucursalKey());
            }
            _contexto = null;
            _sucursalActivaId = null;
        }

        _contexto = await _seguridadHttp.GetContextoSesionAsync();
        await LoadSucursalActivaAsync();
        OnChange?.Invoke();
    }

    public bool CanAccessFormulario(string formularioCodigo)
    {
        if (_contexto is null || string.IsNullOrWhiteSpace(formularioCodigo))
            return false;

        return _contexto.ModulosPermitidos
            .SelectMany(m => m.Formularios)
            .Any(f => string.Equals(f.Codigo, formularioCodigo, StringComparison.OrdinalIgnoreCase));
    }

    public bool CanExecute(string formularioCodigo, string accionCodigo)
    {
        if (_contexto is null || string.IsNullOrWhiteSpace(formularioCodigo) || string.IsNullOrWhiteSpace(accionCodigo))
            return false;

        return _contexto.ModulosPermitidos
            .SelectMany(m => m.Formularios)
            .Where(f => string.Equals(f.Codigo, formularioCodigo, StringComparison.OrdinalIgnoreCase))
            .SelectMany(f => f.Acciones)
            .Any(a => string.Equals(a.Codigo, accionCodigo, StringComparison.OrdinalIgnoreCase));
    }

    public async Task SetSucursalActivaAsync(int sucursalId)
    {
        if (_contexto is null)
            return;

        var existe = _contexto.SucursalesPermitidas.Any(x => x.Activo && x.SucursalId == sucursalId);
        if (!existe)
            return;

        _sucursalActivaId = sucursalId;
        await _js.InvokeVoidAsync("localStorage.setItem", BuildSucursalKey(), sucursalId.ToString());
        OnChange?.Invoke();
    }

    public async Task ClearAsync()
    {
        if (_contexto is not null)
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", BuildSucursalKey());
        }

        _contexto = null;
        _sucursalActivaId = null;
        OnChange?.Invoke();
    }

    private async Task LoadSucursalActivaAsync()
    {
        _sucursalActivaId = null;
        if (_contexto is null)
            return;

        try
        {
            var stored = await _js.InvokeAsync<string?>("localStorage.getItem", BuildSucursalKey());
            if (int.TryParse(stored, out var sucursalId) && _contexto.SucursalesPermitidas.Any(x => x.Activo && x.SucursalId == sucursalId))
            {
                _sucursalActivaId = sucursalId;
                return;
            }
        }
        catch
        {
            // ignore storage availability issues
        }

        _sucursalActivaId = _contexto.SucursalPredeterminadaId
            ?? _contexto.SucursalesPermitidas.FirstOrDefault(x => x.Activo)?.SucursalId;

        if (_sucursalActivaId.HasValue)
            await _js.InvokeVoidAsync("localStorage.setItem", BuildSucursalKey(), _sucursalActivaId.Value.ToString());
    }

    private string BuildSucursalKey()
    {
        var user = _contexto?.UsuarioId ?? 0;
        var empresa = _contexto?.EmpresaId ?? 0;
        return $"{SucursalStorageKey}:{user}:{empresa}";
    }
}
