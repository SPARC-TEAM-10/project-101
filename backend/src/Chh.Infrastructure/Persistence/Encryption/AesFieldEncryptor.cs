using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Chh.Infrastructure.Persistence.Encryption;

/// <summary>
/// AES-256-CBC implementation of <see cref="IFieldEncryptor"/>. The key comes from
/// <c>Encryption:HealthDataKeyBase64</c> — Azure Key Vault / user-secrets at runtime, never
/// committed (`.claude/rules/api-standards.md` §5). Each call generates a fresh random IV,
/// stored alongside the ciphertext (both base64-encoded together) — standard practice so
/// encrypting the same plaintext twice doesn't produce the same ciphertext.
/// </summary>
public class AesFieldEncryptor : IFieldEncryptor
{
    private const int KeySizeBytes = 32; // AES-256
    private readonly byte[] _key;

    /// <summary>
    /// Creates the encryptor, reading the base64-encoded 256-bit key from configuration. If
    /// unconfigured (e.g. local dev with no secret set up yet), generates a random per-process key
    /// instead of failing to start — logged loudly, since data encrypted under it becomes
    /// unreadable the moment the process restarts. Never falls back like this in a real
    /// deployment: Production always resolves the real key from Azure Key Vault.
    /// </summary>
    /// <param name="configuration">App configuration, used to resolve <c>Encryption:HealthDataKeyBase64</c>.</param>
    /// <param name="logger">Logs a warning when falling back to an ephemeral key.</param>
    public AesFieldEncryptor(IConfiguration configuration, ILogger<AesFieldEncryptor> logger)
    {
        var keyBase64 = configuration["Encryption:HealthDataKeyBase64"];
        if (string.IsNullOrWhiteSpace(keyBase64))
        {
            logger.LogWarning(
                "Encryption:HealthDataKeyBase64 is not configured — generating a random ephemeral key for this " +
                "process. Data encrypted now will NOT be decryptable after a restart. Configure a real key " +
                "(Azure Key Vault / user-secrets) before storing data you need to keep.");
            _key = RandomNumberGenerator.GetBytes(KeySizeBytes);
            return;
        }

        _key = Convert.FromBase64String(keyBase64);
        if (_key.Length != KeySizeBytes)
        {
            throw new InvalidOperationException(
                $"Encryption:HealthDataKeyBase64 must decode to {KeySizeBytes} bytes (AES-256), got {_key.Length}.");
        }
    }

    /// <inheritdoc />
    public string Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

        // IV || ciphertext, base64-encoded as a single stored value.
        var combined = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);
        return Convert.ToBase64String(combined);
    }

    /// <inheritdoc />
    public string Decrypt(string ciphertext)
    {
        var combined = Convert.FromBase64String(ciphertext);

        using var aes = Aes.Create();
        aes.Key = _key;

        var iv = new byte[aes.BlockSize / 8];
        var cipherBytes = new byte[combined.Length - iv.Length];
        Buffer.BlockCopy(combined, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(combined, iv.Length, cipherBytes, 0, cipherBytes.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plaintextBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return System.Text.Encoding.UTF8.GetString(plaintextBytes);
    }
}
