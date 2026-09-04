namespace Chh.Domain.Entities;

/// <summary>An issued OTP code (hashed) for a mobile number, with expiry and resend-cooldown timestamps.</summary>
public class OtpRequest
{
    /// <summary>Surrogate primary key.</summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>The 10-digit mobile number the OTP was issued for.</summary>
    public string MobileNumber { get; private set; } = default!;

    /// <summary>SHA-256 hex hash of the 6-digit OTP code. The raw code is never persisted.</summary>
    public string OtpCodeHash { get; private set; } = default!;

    /// <summary>UTC timestamp the OTP was requested.</summary>
    public DateTimeOffset OtpRequestedAtUtc { get; private set; }

    /// <summary>UTC timestamp the OTP expires.</summary>
    public DateTimeOffset OtpExpiresAtUtc { get; private set; }

    /// <summary>UTC timestamp a resend becomes available.</summary>
    public DateTimeOffset ResendAvailableAtUtc { get; private set; }

    /// <summary>Whether this OTP has been successfully verified. Set by CHH-9 (verification is out of scope here).</summary>
    public bool IsVerified { get; private set; }

    /// <summary>Reserved for EF Core materialization — entities carry no construction logic (see <see cref="Chh.Application.Factories.OtpRequestFactory"/>).</summary>
    private OtpRequest()
    {
    }

    /// <summary>Creates an OTP request record from already-computed timestamps.</summary>
    /// <param name="mobileNumber">The 10-digit mobile number the OTP was issued for.</param>
    /// <param name="otpCodeHash">SHA-256 hex hash of the OTP code. The raw code is never persisted.</param>
    /// <param name="otpRequestedAtUtc">UTC timestamp the OTP was requested.</param>
    /// <param name="otpExpiresAtUtc">UTC timestamp the OTP expires.</param>
    /// <param name="resendAvailableAtUtc">UTC timestamp a resend becomes available.</param>
    public OtpRequest(
        string mobileNumber,
        string otpCodeHash,
        DateTimeOffset otpRequestedAtUtc,
        DateTimeOffset otpExpiresAtUtc,
        DateTimeOffset resendAvailableAtUtc)
    {
        MobileNumber = mobileNumber;
        OtpCodeHash = otpCodeHash;
        OtpRequestedAtUtc = otpRequestedAtUtc;
        OtpExpiresAtUtc = otpExpiresAtUtc;
        ResendAvailableAtUtc = resendAvailableAtUtc;
        IsVerified = false;
    }
}
