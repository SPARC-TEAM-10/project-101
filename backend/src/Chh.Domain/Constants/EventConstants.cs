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

    /// <summary>
    /// Proximity radius for the "new event published nearby" notification (CHH-42/US-CHH-005-05
    /// AC1 — the PRD/spec never gives a number for "the venue's region", so this reuses the
    /// frontend discovery page's default search radius (useEventDiscovery.ts's DEFAULT_RADIUS_KM)
    /// for consistency rather than inventing an unrelated value).
    /// </summary>
    public const int EventPublishNotificationRadiusKm = 15;

    /// <summary>Minimum characters for a name search (CHH-44/US-CHH-005-07 AC2) — a full 10-digit mobile number is accepted at any point count.</summary>
    public const int MinAttendanceSearchNameLength = 3;

    /// <summary>How long before <see cref="Entities.Event.StartAtUtc"/> the attendance check-in window opens (spec §6.1: "1 hour before event start until event end").</summary>
    public static readonly TimeSpan AttendanceWindowBeforeStart = TimeSpan.FromHours(1);

    public const string AttendanceSearchTooShortMessage = "Enter at least 3 characters of a name, or a full mobile number.";
    public const string AlreadyAttendedMessage = "This participant has already been marked attended.";
    public const string AttendanceOutsideWindowMessage = "Attendance can only be marked from 1 hour before the event starts until it ends.";
    public const string RsvpNotEligibleForAttendanceMessage = "This RSVP was cancelled, so attendance can't be marked.";
}
