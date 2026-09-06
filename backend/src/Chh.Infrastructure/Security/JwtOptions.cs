namespace Chh.Infrastructure.Security;

/// <summary>
/// Bound from <c>Jwt</c> and validated at startup (<c>ValidateOnStart</c>) so a missing signing
/// key fails at boot rather than on the first login.
/// </summary>
public class JwtOptions
{
    /// <summary>Configuration section this binds from.</summary>
    public const string SectionName = "Jwt";

    /// <summary>Token issuer ("iss" claim).</summary>
    public string Issuer { get; set; } = default!;

    /// <summary>Token audience ("aud" claim).</summary>
    public string Audience { get; set; } = default!;

    /// <summary>Access token lifetime in minutes (1 hour per CHH-F01 AC3).</summary>
    public int AccessTokenLifetimeMinutes { get; set; }

    /// <summary>
    /// Base64-encoded HMAC-SHA256 signing key (32+ bytes). Generate with a CSPRNG — e.g.
    /// <c>openssl rand -base64 32</c> — never reuse a key across environments.
    /// </summary>
    public string SigningKeyBase64 { get; set; } = default!;
}
