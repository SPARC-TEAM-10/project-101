namespace Chh.Domain.Enums;

/// <summary>
/// Account standing for an <see cref="Entities.IndividualProfile"/> (US-CHH-004-02/CHH-80).
/// A <see cref="Suspended"/> donor is excluded from proximity matching (AC3) regardless of blood
/// group compatibility or distance. Flipping this is an out-of-band operation (direct DB access)
/// until a moderation workflow exists — same posture as <see cref="Entities.IndividualProfile.IsAdmin"/>.
/// </summary>
public enum AccountStatus
{
    /// <summary>Eligible for donor matching (default).</summary>
    Active = 1,

    /// <summary>Excluded from donor matching (AC3).</summary>
    Suspended = 2
}
