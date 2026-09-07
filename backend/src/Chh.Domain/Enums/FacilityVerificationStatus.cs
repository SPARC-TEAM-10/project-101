namespace Chh.Domain.Enums;

/// <summary>Facility verification lifecycle state (CHH-F07 Admin Command Center).</summary>
public enum FacilityVerificationStatus
{
    /// <summary>Awaiting System Admin review — the default state on registration.</summary>
    Pending = 1,

    /// <summary>Approved by a System Admin (CHH-75 AC1).</summary>
    Verified = 2,

    /// <summary>Rejected by a System Admin, with a mandatory reason (CHH-75 AC2).</summary>
    Rejected = 3
}
