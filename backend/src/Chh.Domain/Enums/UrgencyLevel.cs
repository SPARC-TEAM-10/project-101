namespace Chh.Domain.Enums;

/// <summary>Urgency of a blood request (CHH-33/US-CHH-004-01). Supersedes earlier Emergency/High/Normal wording.</summary>
public enum UrgencyLevel
{
    /// <summary>Life-threatening, immediate need.</summary>
    Emergency = 1,

    /// <summary>Needed soon but not immediately life-threatening.</summary>
    Urgent = 2,

    /// <summary>Planned/non-urgent need.</summary>
    Standard = 3
}
