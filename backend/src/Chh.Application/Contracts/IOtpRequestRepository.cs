using Chh.Domain.Entities;

namespace Chh.Application.Contracts;

/// <summary>Data layer for <see cref="OtpRequest"/>.</summary>
public interface IOtpRequestRepository
{
    /// <summary>Returns the most recently requested OTP for the given mobile number, or <c>null</c> if none exists.</summary>
    Task<OtpRequest?> GetLatestByMobileNumberAsync(string mobileNumber, CancellationToken ct);

    /// <summary>Adds a new OTP request to the context. Does not call <c>SaveChangesAsync</c>.</summary>
    Task AddAsync(OtpRequest otpRequest, CancellationToken ct);
}
