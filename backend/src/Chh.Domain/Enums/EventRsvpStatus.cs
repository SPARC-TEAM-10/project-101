namespace Chh.Domain.Enums;

/// <summary>Lifecycle state of an <see cref="Entities.EventRsvp"/> (CHH-40/US-CHH-005-03).</summary>
public enum EventRsvpStatus
{
    /// <summary>Actively holding a spot — counts against <see cref="Entities.Event.RsvpCount"/>.</summary>
    Going = 1,

    /// <summary>Cancelled by the individual (Edge Case) — the spot has been released back to the pool.</summary>
    Cancelled = 2
}
