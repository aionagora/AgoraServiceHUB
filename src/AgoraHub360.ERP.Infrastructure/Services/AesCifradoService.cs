using System.Security.Cryptography;
using AgoraHub360.ERP.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgoraHub360.ERP.Infrastructure.Services;

/// <summary>
/// Implementación de ICifradoService usando AES-256-CBC con IV aleatorio.
/// Formato de salida: "v1:{Base64(IV + Ciphertext)}"
/// La clave se obtiene de configuración (Encryption:Key) y debe tener 32 bytes (256 bits).
/// </summary>
public class AesCifradoService : ICifradoService
{
    private readonly byte[] _key;
    private readonly ILogger<AesCifradoService> _logger;

    private const string PrefixV1 = "v1:";
    private const int IvSize = 16; // AES-CBC IV es siempre 16 bytes

    public AesCifradoService(IConfiguration configuration, ILogger<AesCifradoService> logger)
    {
        _logger = logger;
        var keyBase64 = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption:Key no está configurado. Debe ser una clave Base64 de 32 bytes (256 bits).");

        try
        {
            _key = Convert.FromBase64String(keyBase64);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("Encryption:Key no es una cadena Base64 válida.", ex);
        }

        if (_key.Length != 32)
        {
            throw new InvalidOperationException($"Encryption:Key debe tener 32 bytes (256 bits). Longitud actual: {_key.Length} bytes.");
        }
    }

    public string Cifrar(string texto)
    {
        ArgumentNullException.ThrowIfNull(texto);

        var iv = RandomNumberGenerator.GetBytes(IvSize);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(texto);
        var ciphertext = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

        // Formato: IV (16 bytes) + Ciphertext
        var result = new byte[IvSize + ciphertext.Length];
        Buffer.BlockCopy(iv, 0, result, 0, IvSize);
        Buffer.BlockCopy(ciphertext, 0, result, IvSize, ciphertext.Length);

        return PrefixV1 + Convert.ToBase64String(result);
    }

    public string Descifrar(string textoCifrado)
    {
        ArgumentNullException.ThrowIfNull(textoCifrado);

        if (!textoCifrado.StartsWith(PrefixV1, StringComparison.Ordinal))
        {
            _logger.LogWarning("Formato de cifrado no reconocido (falta prefijo v1:)");
            throw new InvalidOperationException("Formato de cifrado no reconocido. Se esperaba prefijo 'v1:'.");
        }

        var combinedBytes = Convert.FromBase64String(textoCifrado[PrefixV1.Length..]);

        if (combinedBytes.Length < IvSize)
        {
            throw new InvalidOperationException("Datos cifrados inválidos: longitud insuficiente.");
        }

        var iv = combinedBytes[..IvSize];
        var ciphertext = combinedBytes[IvSize..];

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        var plaintextBytes = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);

        return System.Text.Encoding.UTF8.GetString(plaintextBytes);
    }
}
