namespace AgoraHub360.ERP.Domain.Entities.CST;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Perfil de costo de importación (landed cost).
/// AllocationMethod: 1=Peso, 2=Volumen, 3=Valor, 4=Unidades
/// </summary>
public class LandedCostProfile : TenantEntity
{
    public long ProfileId { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>1=Peso, 2=Volumen, 3=Valor, 4=Unidades</summary>
    public byte AllocationMethod { get; set; } = 3;

    public bool IsDefault { get; set; }
}
