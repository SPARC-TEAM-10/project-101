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

    /// <summary>UTC timestamp the OTP expires (requested + 5 minutes).</summary>
    public DateTimeOffset OtpExpiresAtUtc { get; private set; }

    /// <summary>UTC timestamp a resend becomes available (requested + 120 seconds).</summary>
    public DateTimeOffset ResendAvailableAtUtc { get; private set; }

    /// <summary>Whether this OTP has been successfully verified. Set by CHH-9 (verification is out of scope here).</summary>
    public bool IsVerified { get; private set; }

    private static readonly TimeSpan OtpValidity = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(120);

    private OtpRequest()
    {
    }

    /// <summary>Creates a new OTP request, computing the expiry and resend-cooldown timestamps from <paramref name="requestedAtUtc"/>.</summary>
    public static OtpRequest Create(string mobileNumber, string otpCodeHash, DateTimeOffset requestedAtUtc)
    {
        return new OtpRequest
        {
            MobileNumber = mobileNumber,
            OtpCodeHash = otpCodeHash,
            OtpRequestedAtUtc = requestedAtUtc,
            OtpExpiresAtUtc = requestedAtUtc.Add(OtpValidity),
            ResendAvailableAtUtc = requestedAtUtc.Add(ResendCooldown),
            IsVerified = false
        };
    }
}
