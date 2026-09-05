using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Chh.Infrastructure.Persistence.Encryption;

/// <summary>EF Core value converter encrypting a <see cref="bool"/> column at rest via <see cref="IFieldEncryptor"/>.</summary>
public class EncryptedBoolConverter : ValueConverter<bool, string>
{
    /// <summary>Creates the converter backed by the given encryptor.</summary>
    /// <param name="fieldEncryptor">Encryptor used to protect the column value.</param>
    public EncryptedBoolConverter(IFieldEncryptor fieldEncryptor)
        : base(
            v => fieldEncryptor.Encrypt(v ? "1" : "0"),
            v => fieldEncryptor.Decrypt(v) == "1")
    {
    }
}

/// <summary>EF Core value converter encrypting a <see cref="DateOnly"/> column at rest via <see cref="IFieldEncryptor"/>.</summary>
public class EncryptedDateOnlyConverter : ValueConverter<DateOnly, string>
{
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>Creates the converter backed by the given encryptor.</summary>
    /// <param name="fieldEncryptor">Encryptor used to protect the column value.</param>
    public EncryptedDateOnlyConverter(IFieldEncryptor fieldEncryptor)
        : base(
            v => fieldEncryptor.Encrypt(ToDateString(v)),
            v => ParseDateString(fieldEncryptor.Decrypt(v)))
    {
    }

    // DateOnly.ToString/ParseExact declare optional parameters, which C# expression trees
    // (required by ValueConverter's constructor) cannot call directly even when every argument
    // is supplied explicitly — routing through plain static methods sidesteps that restriction.
    private static string ToDateString(DateOnly date) => date.ToString(DateFormat, CultureInfo.InvariantCulture);

    private static DateOnly ParseDateString(string date) => DateOnly.ParseExact(date, DateFormat, CultureInfo.InvariantCulture);
}

/// <summary>EF Core value converter encrypting a nullable <see cref="string"/> column at rest via <see cref="IFieldEncryptor"/>.</summary>
public class EncryptedNullableStringConverter : ValueConverter<string?, string?>
{
    /// <summary>Creates the converter backed by the given encryptor.</summary>
    /// <param name="fieldEncryptor">Encryptor used to protect the column value.</param>
    public EncryptedNullableStringConverter(IFieldEncryptor fieldEncryptor)
        : base(
            v => v == null ? null : fieldEncryptor.Encrypt(v),
            v => v == null ? null : fieldEncryptor.Decrypt(v))
    {
    }
}
