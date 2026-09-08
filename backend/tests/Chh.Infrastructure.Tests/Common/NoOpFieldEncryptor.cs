using Chh.Infrastructure.Persistence.Encryption;

namespace Chh.Infrastructure.Tests.Common;

/// <summary>
/// Pass-through <see cref="IFieldEncryptor"/> for repository tests against entities with no
/// encrypted columns (e.g. <c>Facility</c>/<c>FacilityContact</c>). <see cref="ChhDbContext"/>'s
/// constructor requires an encryptor regardless of whether the entity under test uses one.
/// </summary>
public class NoOpFieldEncryptor : IFieldEncryptor
{
    /// <inheritdoc />
    public string Encrypt(string plaintext) => plaintext;

    /// <inheritdoc />
    public string Decrypt(string ciphertext) => ciphertext;
}
