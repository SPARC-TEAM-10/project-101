namespace Chh.Domain.Constants;

/// <summary>Validation bounds and shared messages for event creation (CHH-38/US-CHH-005-01, spec §6.2).</summary>
public static class EventConstants
{
    public const int MinTitleLength = 5;
    public const int MaxTitleLength = 100;
    public const int MinDescriptionLength = 20;
    public const int MaxDescriptionLength = 1000;
    public const int MinVenueNameLength = 3;
    public const int MaxVenueNameLength = 100;
    public const int MinVenueAddressLength = 10;
    public const int MaxVenueAddressLength = 250;
    public const int MinCapacity = 1;
    public const int MaxCapacity = 1000;
    public const int MinCoordinatorNameLength = 2;
    public const int MaxCoordinatorNameLength = 50;

    /// <summary>Minimum lead time between now and the event's start (spec §6.2's "Start Date/Time" rule).</summary>
    public static readonly TimeSpan MinLeadTime = TimeSpan.FromHours(1);

    public const string StartMustBeFutureMessage = "Event must start in the future.";
    public const string EndMustBeAfterStartMessage = "End time must be after start time.";
    public const string CapacityRangeMessage = "Capacity must be between 1 and 1,000.";
    public const string RsvpCutoffMustPrecedeStartMessage = "RSVP cut-off must be before the event start time.";
    public const string FacilityNotVerifiedMessage = "Only a verified facility can create events.";

    /// <summary>Discovery search radius bounds (CHH-39/US-CHH-005-02) — clamped, not rejected (AC3: "caps the radius at 100km").</summary>
    public const int MinSearchRadiusKm = 5;
    public const int MaxSearchRadiusKm = 100;

    /// <summary>Cancellation reason bounds (CHH-41/US-CHH-005-04 AC1, EventEditWeb.dc.html's "104 / 300" counter).</summary>
    public const int MinCancellationReasonLength = 10;
    public const int MaxCancellationReasonLength = 300;

    public const string NotOwningFacilityMessage = "Only the organizing facility can edit or cancel this event.";
    public const string EventAlreadyStartedMessage = "This event has already started, so it can't be edited or cancelled.";
    public const string CapacityBelowRsvpCountMessage = "Capacity cannot be reduced below the number already RSVP'd.";
    public const string EventAlreadyCancelledMessage = "This event has already been cancelled.";
}
