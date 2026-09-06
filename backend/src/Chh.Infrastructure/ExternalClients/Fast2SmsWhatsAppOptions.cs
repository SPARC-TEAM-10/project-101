namespace Chh.Infrastructure.ExternalClients;

/// <summary>
/// Bound from <c>Fast2Sms:WhatsApp</c> and validated at startup (<c>ValidateOnStart</c>) so a
/// missing template ID fails at boot rather than on the first OTP.
/// </summary>
public class Fast2SmsWhatsAppOptions
{
    /// <summary>Configuration section this binds from.</summary>
    public const string SectionName = "Fast2Sms:WhatsApp";

    /// <summary>Our WhatsApp Business phone number ID (e.g. "579519398574288").</summary>
    public string PhoneNumberId { get; set; } = default!;

    /// <summary>Approved OTP template ID. Variables: [otpCode].</summary>
    public string OtpMessageId { get; set; } = default!;

    /// <summary>Approved donor-request template ID. Variables: [bloodGroup, hospital, city, linkSuffix].</summary>
    public string DonorRequestMessageId { get; set; } = default!;
}
