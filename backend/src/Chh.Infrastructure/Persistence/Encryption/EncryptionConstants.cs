namespace Chh.Infrastructure.Persistence.Encryption;

/// <summary>Fixed formatting details shared by the encrypted value converters in this namespace.</summary>
public static class EncryptionConstants
{
    /// <summary>Round-trip date format used before encrypting/after decrypting a <see cref="DateOnly"/> column.</summary>
    public const string EncryptedDateFormat = "yyyy-MM-dd";
}
