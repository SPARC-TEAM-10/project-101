namespace Chh.Application.Abstractions;

// Event RSVP domain exceptions (CHH-40/US-CHH-005-03).

/// <summary>Raised when an event has no remaining capacity (AC2 — "Event Full"). Maps to 422 Unprocessable Entity.</summary>
public class EventFullException : ChhException
{
    /// <summary>Creates the exception with the standard "event full" message.</summary>
    public EventFullException()
        : base("This event is full.")
    {
    }
}

/// <summary>
/// Raised when the caller already has an active RSVP for this event (AC3 — one RSVP per person).
/// Maps to 409 Conflict.
/// </summary>
public class AlreadyRsvpdException : ChhException
{
    /// <summary>Creates the exception with the standard already-RSVP'd message.</summary>
    public AlreadyRsvpdException()
        : base("You're already going to this event.")
    {
    }
}
