namespace Chh.Domain.Constants;

/// <summary>Facility-related constants (CHH-78/US-CHH-003-01) — contact bounds and user-facing messages.</summary>
public static class FacilityConstants
{
    /// <summary>Minimum number of contacts a facility must have (business rule).</summary>
    public const int MinContacts = 1;

    /// <summary>Maximum number of contacts a facility may have (AC2).</summary>
    public const int MaxContacts = 3;

    /// <summary>Validation message when a mobile number is reused across contacts (Edge Case).</summary>
    public const string DuplicateContactMobileMessage = "Each contact needs a different mobile number.";

    /// <summary>Validation message when a facility has more than <see cref="MaxContacts"/> contacts (AC2).</summary>
    public const string TooManyContactsMessage = "Three contacts is the maximum.";

    /// <summary>Validation message when the license number contains characters outside letters, numbers, and hyphens.</summary>
    public const string InvalidLicenseNumberMessage = "Licence number can contain letters, numbers and hyphens only.";
}
