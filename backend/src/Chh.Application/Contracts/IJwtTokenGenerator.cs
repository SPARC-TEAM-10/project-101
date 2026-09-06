namespace Chh.Application.Contracts;

/// <summary>Issues signed JWT access tokens on successful authentication (CHH-F01 AC3).</summary>
public interface IJwtTokenGenerator
{
    /// <summary>Generates a signed access token for the given mobile number and role.</summary>
    /// <param name="mobileNumber">The authenticated user's mobile number (embedded as the JWT "sub" claim).</param>
    /// <param name="role">The role to embed as the JWT "role" claim (see <c>Chh.Domain.Constants.RoleConstants</c>).</param>
    /// <returns>The signed token and its absolute UTC expiry.</returns>
    (string AccessToken, DateTimeOffset ExpiresAtUtc) GenerateToken(string mobileNumber, string role);
}
