namespace AgoraHub360.ERP.Shared.DTOs.MDM;

using System.ComponentModel.DataAnnotations;

public class UpsertContactoUsuarioAccesoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Usuario inválido.")]
    public int UsuarioId { get; set; }

    public int? PerfilAccesoId { get; set; }

    public bool AccesoWeb { get; set; }
    public bool AccesoMovil { get; set; }
    public bool Activo { get; set; } = true;
}
