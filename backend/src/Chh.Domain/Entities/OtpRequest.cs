namespace Chh.Domain.Entities;

/// <summary>
/// An issued OTP code (hashed) for a mobile number, with expiry and resend-cooldown timestamps.
/// A plain property bag — construction logic lives in
/// <see cref="Chh.Application.Factories.OtpRequestFactory"/>, which sets these `internal` setters
/// via object initializer (see <c>Chh.Domain.csproj</c>'s <c>InternalsVisibleTo</c>).
/// </summary>
public class OtpRequest
{
    /// <summary>Surrogate primary key.</summary>
    public Guid Id { get; internal set; } = Guid.NewGuid();

    /// <summary>The 10-digit mobile number the OTP was issued for.</summary>
    public string MobileNumber { get; internal set; } = default!;

    /// <summary>SHA-256 hex hash of the 6-digit OTP code. The raw code is never persisted.</summary>
    public string OtpCodeHash { get; internal set; } = default!;

    /// <summary>UTC timestamp the OTP was requested.</summary>
    public DateTimeOffset OtpRequestedAtUtc { get; internal set; }

    /// <summary>UTC timestamp the OTP expires.</summary>
    public DateTimeOffset OtpExpiresAtUtc { get; internal set; }

    /// <summary>UTC timestamp a resend becomes available.</summary>
    public DateTimeOffset ResendAvailableAtUtc { get; internal set; }

    /// <summary>Whether this OTP has been successfully verified. Set by CHH-9 (verification is out of scope here).</summary>
    public bool IsVerified { get; internal set; }

    /// <summary>Marks this OTP request as successfully verified (CHH-9). Idempotent.</summary>
    public void MarkVerified()
    {
        IsVerified = true;
    }
}
