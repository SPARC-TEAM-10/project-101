namespace Chh.Domain.Enums;

/// <summary>Category of a community health event (CHH-38/US-CHH-005-01) — the spec's fixed four types.</summary>
public enum EventType
{
    /// <summary>A blood donation drive.</summary>
    BloodDonationCamp = 1,

    /// <summary>A general health screening/checkup camp.</summary>
    HealthCamp = 2,

    /// <summary>An awareness/education session.</summary>
    AwarenessSession = 3,

    /// <summary>Any other community health activity.</summary>
    Other = 4
}
