namespace Chh.Domain.Enums;

/// <summary>Facility verification lifecycle state (CHH-F03/CHH-F07). Only <see cref="Pending"/> is set by CHH-78 (AC1) — the transition to <see cref="Verified"/>/<see cref="Rejected"/> belongs to the admin verification stories (CHH-75), not this one.</summary>
public enum FacilityVerificationStatus
{
    /// <summary>Awaiting System Admin review — the default state on registration; restricts high-impact features (CHH-F03 LLD §6.1).</summary>
    Pending = 1,

    /// <summary>Approved by a System Admin (CHH-75 AC1).</summary>
    Verified = 2,

    /// <summary>Rejected by a System Admin, with a mandatory reason (CHH-75 AC2) — may resubmit corrected information.</summary>
    Rejected = 3
}
