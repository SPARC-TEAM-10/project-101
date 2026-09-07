namespace Chh.Domain.Enums;

/// <summary>
/// Verification lifecycle state of a <c>Facility</c> (CHH-F03). Only <see cref="Pending"/> is set
/// by this story (CHH-78 AC1) — the transition to <see cref="Approved"/>/<see cref="Rejected"/>
/// belongs to the admin verification story (CHH-28), not this one.
/// </summary>
public enum FacilityVerificationStatus
{
    /// <summary>Submitted, awaiting System Admin review — restricts high-impact features (LLD §6.1).</summary>
    Pending = 1,

    /// <summary>Verified by a System Admin — full feature access.</summary>
    Approved = 2,

    /// <summary>Rejected by a System Admin — may resubmit corrected information.</summary>
    Rejected = 3
}
