namespace Chh.Domain.Enums;

/// <summary>A donor's response to their own <c>DonorNotification</c> (CHH-35/US-CHH-004-04).</summary>
public enum DonorResponseStatus
{
    /// <summary>No response recorded yet.</summary>
    Pending = 1,

    /// <summary>Donor accepted (AC1) — contact info is shared with the requester.</summary>
    Accepted = 2,

    /// <summary>Donor declined (AC2) — no further reminders for this request/donor pair.</summary>
    Declined = 3
}
