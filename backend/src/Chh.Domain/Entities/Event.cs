using Chh.Domain.Enums;

namespace Chh.Domain.Entities;

/// <summary>
/// A community health event (CHH-38/US-CHH-005-01, part of Epic CHH-37 — CHH-F05 Location-Aware
/// Events) created by a verified Hospital/NGO facility. Construction logic lives in
/// <see cref="Chh.Application.Factories.EventFactory"/>, matching <see cref="BloodRequest"/>'s
/// established internal-set + factory pattern.
/// </summary>
public class Event
{
    /// <summary>Surrogate primary key.</summary>
    public Guid Id { get; internal set; } = Guid.NewGuid();

    /// <summary>
    /// The organizing facility, resolved from the creator's mobile number at creation time —
    /// never client-supplied (see <c>Chh.Api.Controllers.EventsController</c>).
    /// </summary>
    public Guid FacilityId { get; internal set; }

    /// <summary>Event title (AC1 mandatory field, 5-100 characters).</summary>
    public string Title { get; internal set; } = default!;

    /// <summary>One of the spec's four fixed event types.</summary>
    public EventType EventType { get; internal set; }

    /// <summary>Free-text description, 20-1000 characters.</summary>
    public string Description { get; internal set; } = default!;

    /// <summary>Venue display name, 3-100 characters.</summary>
    public string VenueName { get; internal set; } = default!;

    /// <summary>Venue address, free text, 10-250 characters.</summary>
    public string VenueAddress { get; internal set; } = default!;

    /// <summary>Venue latitude (AC1/AC3 mandatory — "select a valid location on the map").</summary>
    public decimal Latitude { get; internal set; }

    /// <summary>Venue longitude — see <see cref="Latitude"/>.</summary>
    public decimal Longitude { get; internal set; }

    /// <summary>UTC start time — must be at least an hour in the future at creation (AC2).</summary>
    public DateTimeOffset StartAtUtc { get; internal set; }

    /// <summary>UTC end time — must be after <see cref="StartAtUtc"/>.</summary>
    public DateTimeOffset EndAtUtc { get; internal set; }

    /// <summary>Maximum attendees, 1-1000 (AC1, Edge Case: capacity 0 is blocked).</summary>
    public int Capacity { get; internal set; }

    /// <summary>Event coordinator's name, shown to attendees.</summary>
    public string CoordinatorName { get; internal set; } = default!;

    /// <summary>Event coordinator's contact number, shown to attendees on the event page.</summary>
    public string CoordinatorContact { get; internal set; } = default!;

    /// <summary>
    /// Optional early RSVP cutoff — null means RSVPs stay open until the event starts or capacity
    /// is reached. When set, must be before <see cref="StartAtUtc"/>.
    /// </summary>
    public DateTimeOffset? RsvpCutoffAtUtc { get; internal set; }

    /// <summary>Lifecycle state — <see cref="EventStatus.Published"/> from creation (CHH-38 creates directly, no draft state).</summary>
    public EventStatus Status { get; internal set; }

    /// <summary>UTC timestamp the event was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; internal set; }

    /// <summary>UTC timestamp of the last update (e.g. a CHH-41 edit or cancellation).</summary>
    public DateTimeOffset UpdatedAtUtc { get; internal set; }

    /// <summary>
    /// Count of active (non-cancelled) RSVPs (CHH-40/US-CHH-005-03) — mutated only via
    /// <c>IEventRepository.TryReserveSpotAsync</c>/<c>ReleaseSpotAsync</c>'s atomic conditional
    /// update, matching <see cref="BloodRequest.UnitsAccepted"/>'s established race-safety
    /// pattern. <c>Capacity - RsvpCount</c> is "spots remaining".
    /// </summary>
    public int RsvpCount { get; internal set; }

    /// <summary>
    /// The organizer's stated reason for cancelling (CHH-41/US-CHH-005-04 AC1) — shown verbatim to
    /// RSVP'd attendees (EventEditWeb.dc.html's cancel modal: "Attendees see this word for word").
    /// Null until <see cref="EventStatus.Cancelled"/>.
    /// </summary>
    public string? CancellationReason { get; internal set; }
}
