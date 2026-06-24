namespace AgoraHub360.ERP.Shared.Constants;

/// <summary>
/// Constantes de roles del sistema RBAC.
/// </summary>
public static class Roles
{
    // Compatibilidad legacy
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
    public const string Viewer = "Viewer";

    // Platform roles (autoridad global)
    public const string SuperAdmin = "SuperAdmin";
    public const string SystemAdmin = "SystemAdmin";
    public const string SecurityAuditor = "SecurityAuditor";
    public const string PlatformSupport = "PlatformSupport";
    public const string None = "None";

    // Tenant roles (por empresa)
    public const string TenantOwner = "TenantOwner";
    public const string AdminEmpresa = "AdminEmpresa";
    public const string Supervisor = "Supervisor";
    public const string Operador = "Operador";
    public const string NoAccess = "NoAccess";
}
