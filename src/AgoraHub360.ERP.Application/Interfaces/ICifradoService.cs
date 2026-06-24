namespace AgoraHub360.ERP.Application.Interfaces;

/// <summary>
/// Servicio de cifrado simétrico para proteger secretos en reposo.
/// Interfaz en Application, implementación en Infrastructure.
/// </summary>
public interface ICifradoService
{
    /// <summary>
    /// Cifra un texto plano usando AES-256-CBC con IV aleatorio.
    /// Retorna string en formato "v1:{base64}".
    /// </summary>
    string Cifrar(string texto);

    /// <summary>
    /// Descifra un texto cifrado en formato "v1:{base64}".
    /// Retorna el texto plano original.
    /// </summary>
    string Descifrar(string textoCifrado);
}
