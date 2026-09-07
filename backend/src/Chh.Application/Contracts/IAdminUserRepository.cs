namespace Chh.Application.Contracts;

/// <summary>Data layer for <c>AdminUser</c> (CHH-F07 Admin Command Center).</summary>
public interface IAdminUserRepository
{
    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="mobileNumber"/> has an active
    /// (<c>IsAdmin = true</c>) admin grant.
    /// </summary>
    /// <param name="mobileNumber">The mobile number to check.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> IsAdminAsync(string mobileNumber, CancellationToken ct);
}
