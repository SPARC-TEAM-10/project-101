namespace Chh.Domain.Enums;

/// <summary>
/// Lifecycle state of a <c>BloodRequest</c> (CHH-33/US-CHH-004-01). Only <see cref="Matching"/> is
/// set by this story (AC1) — the transition to <see cref="Expired"/> after the 6-hour window, and
/// to a fulfilled state, belong to the donor-matching/notification stories later in this epic
/// (CHH-34+), not this one.
/// </summary>
public enum BloodRequestStatus
{
    /// <summary>Actively searching for eligible donors within the search radius.</summary>
    Matching = 1,

    /// <summary>Past its 6-hour window with no fulfillment.</summary>
    Expired = 2
}
