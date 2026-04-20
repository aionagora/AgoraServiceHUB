namespace AgoraHub360.ERP.Domain.Entities.Core;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Permiso de perfil por formulario y opcionalmente por acción.
/// </summary>
public class PerfilPermiso : TenantEntity
{
    public int Id { get; set; }

    public int PerfilAccesoId { get; set; }
    public PerfilAcceso? PerfilAcceso { get; set; }

    public int FormularioSistemaId { get; set; }
    public FormularioSistema? FormularioSistema { get; set; }

    public int? AccionSistemaId { get; set; }
    public AccionSistema? AccionSistema { get; set; }

    public bool Permitido { get; set; } = true;
}
