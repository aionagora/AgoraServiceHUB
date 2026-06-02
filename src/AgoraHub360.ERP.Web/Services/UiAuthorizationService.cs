namespace AgoraHub360.ERP.Web.Services;

using AgoraHub360.ERP.Shared.Constants;

public class UiAuthorizationService
{
    private readonly SesionUsuarioStateService _sesionState;

    public UiAuthorizationService(SesionUsuarioStateService sesionState)
    {
        _sesionState = sesionState;
    }

    public bool IsSuperAdmin =>
        string.Equals(_sesionState.PlatformRole, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase);

    public bool IsPlatformAdmin =>
        IsSuperAdmin || string.Equals(_sesionState.PlatformRole, Roles.SystemAdmin, StringComparison.OrdinalIgnoreCase);

    public bool IsTenantAdmin => _sesionState.IsTenantAdmin;
    public bool HasTenantSelected => _sesionState.HasTenantSelected;
    public bool IsOperator => string.Equals(_sesionState.TenantRole, Roles.Operador, StringComparison.OrdinalIgnoreCase);
    public bool IsViewer => string.Equals(_sesionState.TenantRole, Roles.Viewer, StringComparison.OrdinalIgnoreCase);

    public bool CanSeeGlobalAdminMenu() => IsPlatformAdmin;

    public bool CanSeeTenantAdminMenu() => IsTenantAdmin && HasTenantSelected;

    public bool CanSeeOperationalMenu() => HasTenantSelected;

    public bool CanManageCompanies() => IsPlatformAdmin;

    public bool CanCreateCompany() => IsSuperAdmin;

    public bool CanCreateDemoCompany() => IsPlatformAdmin;

    public bool CanManageEmpresas() => CanManageCompanies();

    public bool CanCreateEmpresa() => CanCreateCompany();

    public bool CanManageUsuarios() => IsPlatformAdmin || IsTenantAdmin;

    public bool CanAssignPlatformRole() => IsPlatformAdmin;

    public bool CanManageSucursales() => IsTenantAdmin && HasTenantSelected;

    public bool CanManageParametrosNumeracion() => IsTenantAdmin && HasTenantSelected;

    public bool CanManageRoles() => IsPlatformAdmin;

    public bool CanAccessConfigSection() => IsPlatformAdmin || IsTenantAdmin;

    public bool CanAccessAuditoria() => IsPlatformAdmin;

    public bool CanAccessWorkflow() => IsPlatformAdmin || (IsTenantAdmin && HasTenantSelected);

    public bool CanManageWorkflow() => IsTenantAdmin && HasTenantSelected;

    public bool CanAccessEmpresaDemo() => IsPlatformAdmin;

    public bool CanExecuteEmpresaDemo() =>
        string.Equals(_sesionState.PlatformRole, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase);

    public string ForbiddenMessage => "No tienes permisos para acceder a esta sección.";
}
