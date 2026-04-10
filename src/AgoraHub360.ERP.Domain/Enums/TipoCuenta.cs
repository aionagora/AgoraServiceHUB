namespace AgoraHub360.ERP.Domain.Enums;

/// <summary>
/// Tipo de cuenta contable según la ecuación contable fundamental.
/// </summary>
public enum TipoCuenta : byte
{
    Activo = 1,
    Pasivo = 2,
    Patrimonio = 3,
    Ingreso = 4,
    Gasto = 5,
    Costo = 6
}

/// <summary>
/// Naturaleza del saldo de la cuenta.
/// </summary>
public enum NaturalezaCuenta : byte
{
    Deudora = 1,
    Acreedora = 2
}
