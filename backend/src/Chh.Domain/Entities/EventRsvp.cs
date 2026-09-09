using Chh.Domain.Enums;

namespace Chh.Domain.Entities;

/// <summary>
/// One individual's RSVP to an <see cref="Event"/> (CHH-40/US-CHH-005-03). One row per (Event,
/// IndividualProfile) pair for the row's whole lifetime — cancelling and re-RSVPing toggles
/// <see cref="Status"/> in place rather than inserting a new row, so <see cref="ReferenceCode"/>
/// (shown to the attendee and used by CHH-44's manual attendance lookup) stays stable.
/// </summary>
public class EventRsvp
{
    /// <summary>Surrogate primary key.</summary>
    public Guid Id { get; internal set; } = Guid.NewGuid();

    /// <summary>The event this RSVP is for.</summary>
    public Guid EventId { get; internal set; }

    /// <summary>The RSVP'ing individual.</summary>
    public Guid IndividualProfileId { get; internal set; }

    /// <summary>
    /// Short human-readable code (e.g. "A24") the attendee gives at the organizer's desk (CHH-44).
    /// Best-effort sequential per event, assigned from the count of RSVP rows that existed for
    /// this event at creation time — not strictly gapless/collision-proof under heavy concurrent
    /// RSVPs, which is an accepted tradeoff since it's a lookup convenience, not a security or
    /// uniqueness-critical identifier.
    /// </summary>
    public string ReferenceCode { get; internal set; } = default!;

    /// <summary>Current status — toggles between <see cref="EventRsvpStatus.Going"/> and <see cref="EventRsvpStatus.Cancelled"/>, or advances to <see cref="EventRsvpStatus.Attended"/>.</summary>
    public EventRsvpStatus Status { get; internal set; }

    /// <summary>UTC timestamp the RSVP was first created.</summary>
    public DateTimeOffset CreatedAtUtc { get; internal set; }

    /// <summary>UTC timestamp of the most recent cancellation, if any.</summary>
    public DateTimeOffset? CancelledAtUtc { get; internal set; }

    /// <summary>UTC timestamp attendance was marked (CHH-44/US-CHH-005-07 AC1). Null until <see cref="EventRsvpStatus.Attended"/>.</summary>
    public DateTimeOffset? AttendedAtUtc { get; internal set; }

    /// <summary>
    /// Display name of whoever marked attendance — resolved once, at mark time, from the marking
    /// facility contact's name (falling back to the facility name if no contact record matches the
    /// caller's mobile number), so a read of this row never needs a second lookup. Part of the
    /// spec §6.1 audit requirement's "actor" field; "method" (always manual here, QR having been
    /// dropped) and "source device" are not modeled — see EventAttendanceService's doc comment.
    /// </summary>
    public string? AttendedByName { get; internal set; }
}
