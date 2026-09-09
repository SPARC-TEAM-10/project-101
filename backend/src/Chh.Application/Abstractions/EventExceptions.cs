namespace Chh.Application.Abstractions;

// Event creation domain exceptions (CHH-38/US-CHH-005-01).

/// <summary>
/// Raised when the authenticated caller's mobile number doesn't resolve to a
/// <see cref="Chh.Domain.Enums.FacilityVerificationStatus.Verified"/> facility (spec's Business
/// Rule: "Events must be created by verified facilities only"). Maps to 403 Forbidden.
/// </summary>
public class FacilityNotVerifiedException : ChhException
{
    /// <summary>Creates the exception with the standard not-verified message.</summary>
    public FacilityNotVerifiedException()
        : base(Chh.Domain.Constants.EventConstants.FacilityNotVerifiedMessage)
    {
    }
}

// Event edit/cancel domain exceptions (CHH-41/US-CHH-005-04).

/// <summary>
/// Raised when the caller's facility doesn't own the event being edited/cancelled (Business Rule:
/// "Only the creating facility can cancel/edit the event"). Maps to 403 Forbidden.
/// </summary>
public class EventNotOwnedByCallerException : ChhException
{
    /// <summary>Creates the exception with the standard not-owner message.</summary>
    public EventNotOwnedByCallerException()
        : base(Chh.Domain.Constants.EventConstants.NotOwningFacilityMessage)
    {
    }
}

/// <summary>
/// Raised when editing or cancelling an event whose <c>StartAtUtc</c> has already passed (Edge
/// Case: "Cancelling an event that has already started should be restricted" —
/// EventManageMobile.dc.html blocks editing too). Maps to 422 Unprocessable Entity.
/// </summary>
public class EventAlreadyStartedException : ChhException
{
    /// <summary>Creates the exception with the standard already-started message.</summary>
    public EventAlreadyStartedException()
        : base(Chh.Domain.Constants.EventConstants.EventAlreadyStartedMessage)
    {
    }
}

/// <summary>
/// Raised when an edit would reduce <see cref="Chh.Domain.Entities.Event.Capacity"/> below the
/// event's current <see cref="Chh.Domain.Entities.Event.RsvpCount"/> (spec §6.2's Capacity rule).
/// Maps to 422 Unprocessable Entity.
/// </summary>
public class CapacityBelowRsvpCountException : ChhException
{
    /// <summary>Creates the exception with the standard capacity-floor message.</summary>
    public CapacityBelowRsvpCountException()
        : base(Chh.Domain.Constants.EventConstants.CapacityBelowRsvpCountMessage)
    {
    }
}

/// <summary>
/// Raised when attempting to edit or cancel an event that is already
/// <see cref="Chh.Domain.Enums.EventStatus.Cancelled"/> — cancellation is one-directional
/// (EventEditWeb.dc.html's cancel modal: "cannot be reopened"). Maps to 422 Unprocessable Entity.
/// </summary>
public class EventAlreadyCancelledException : ChhException
{
    /// <summary>Creates the exception with the standard already-cancelled message.</summary>
    public EventAlreadyCancelledException()
        : base(Chh.Domain.Constants.EventConstants.EventAlreadyCancelledMessage)
    {
    }
}
