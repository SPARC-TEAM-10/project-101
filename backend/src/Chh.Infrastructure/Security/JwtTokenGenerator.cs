using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Chh.Application.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Chh.Infrastructure.Security;

/// <summary>
/// <see cref="IJwtTokenGenerator"/> implementation issuing HMAC-SHA256-signed JWTs per
/// <c>Jwt</c> configuration (api-standards.md §5, CHH-F01 AC3).
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _signingCredentials;

    /// <summary>Creates the generator, pre-building signing credentials from the configured key.</summary>
    /// <param name="options">Bound <c>Jwt</c> configuration.</param>
    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        var keyBytes = Convert.FromBase64String(_options.SigningKeyBase64);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }

    /// <inheritdoc />
    public (string AccessToken, DateTimeOffset ExpiresAtUtc) GenerateToken(string mobileNumber, string role)
    {
        var issuedAtUtc = DateTimeOffset.UtcNow;
        var expiresAtUtc = issuedAtUtc.AddMinutes(_options.AccessTokenLifetimeMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, mobileNumber),
            new Claim(ClaimTypes.MobilePhone, mobileNumber),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAtUtc.UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: _signingCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return (accessToken, expiresAtUtc);
    }
}
