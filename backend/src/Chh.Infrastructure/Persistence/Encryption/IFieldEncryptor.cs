namespace Chh.Infrastructure.Persistence.Encryption;

/// <summary>
/// Encrypts/decrypts individual column values for PII and health-screening data at rest
/// (`.claude/rules/db-standards.md` §3). Used by EF Core value converters — see
/// <c>Chh.Infrastructure.Persistence.Configurations.IndividualProfileConfiguration</c>.
/// </summary>
public interface IFieldEncryptor
{
    /// <summary>Encrypts a plaintext value for storage.</summary>
    /// <param name="plaintext">The value to encrypt.</param>
    string Encrypt(string plaintext);

    /// <summary>Decrypts a value previously produced by <see cref="Encrypt"/>.</summary>
    /// <param name="ciphertext">The stored, encrypted value.</param>
    string Decrypt(string ciphertext);
}
