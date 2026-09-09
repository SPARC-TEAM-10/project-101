namespace Chh.Domain.Enums;

/// <summary>
/// A participant's status as shown on CHH-45's attendance analytics (US-CHH-005-08 AC2) — not a
/// persisted value. <see cref="NoShow"/> is derived, never stored: a <see cref="Entities.EventRsvp"/>
/// stays <see cref="EventRsvpStatus.Going"/> in the database even after the event ends; it only
/// displays as "No-show" once <c>Event.EndAtUtc</c> has passed and it was never marked
/// <see cref="EventRsvpStatus.Attended"/>. See <c>EventAnalyticsService</c> for the derivation.
/// </summary>
public enum AttendanceViewStatus
{
    /// <summary>Still RSVP'd, event hasn't ended yet — outcome not yet known.</summary>
    Going = 1,

    /// <summary>RSVP was cancelled.</summary>
    Cancelled = 2,

    /// <summary>Marked attended by the organizing facility.</summary>
    Attended = 3,

    /// <summary>RSVP'd, the event has ended, and they were never marked attended.</summary>
    NoShow = 4
}
