using Chh.Domain.Entities;

namespace Chh.Application.Contracts;

/// <summary>Data layer for <see cref="OtpRequest"/>.</summary>
public interface IOtpRequestRepository
{
    /// <summary>Returns the most recently requested OTP for the given mobile number, or <c>null</c> if none exists.</summary>
    /// <param name="mobileNumber">The mobile number to look up.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<OtpRequest?> GetLatestByMobileNumberAsync(string mobileNumber, CancellationToken ct);

    /// <summary>Adds a new OTP request to the context. Does not call <c>SaveChangesAsync</c>.</summary>
    /// <param name="otpRequest">The OTP request to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(OtpRequest otpRequest, CancellationToken ct);
}
