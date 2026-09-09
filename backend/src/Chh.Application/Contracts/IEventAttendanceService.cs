using Chh.Application.Dtos;

namespace Chh.Application.Contracts;

/// <summary>Logic layer for manual attendance marking (CHH-44/US-CHH-005-07).</summary>
public interface IEventAttendanceService
{
    /// <summary>
    /// Returns every <c>Going</c>/<c>Attended</c> RSVP for the event whose participant matches
    /// <paramref name="search"/> (AC1/AC2), or <c>null</c> if no event exists with that id.
    /// </summary>
    /// <param name="callerMobileNumber">The authenticated caller's mobile number (from the JWT "sub" claim).</param>
    /// <param name="eventId">The event to search participants for.</param>
    /// <param name="search">Free-text search — a name fragment (3+ characters) or a full 10-digit mobile number.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Chh.Application.Abstractions.EventNotOwnedByCallerException">The caller's facility doesn't own this event.</exception>
    /// <exception cref="Chh.Application.Abstractions.ChhValidationException"><paramref name="search"/> doesn't meet AC2's length/format rule.</exception>
    Task<IReadOnlyList<EventParticipantDto>?> SearchParticipantsAsync(string callerMobileNumber, Guid eventId, string search, CancellationToken ct);

    /// <summary>
    /// Marks the given RSVP attended, or <c>null</c> if the event or the RSVP (scoped to that
    /// event) doesn't exist.
    /// </summary>
    /// <param name="callerMobileNumber">The authenticated caller's mobile number (from the JWT "sub" claim).</param>
    /// <param name="eventId">The event the RSVP belongs to.</param>
    /// <param name="rsvpId">The RSVP to mark attended.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Chh.Application.Abstractions.EventNotOwnedByCallerException">The caller's facility doesn't own this event.</exception>
    /// <exception cref="Chh.Application.Abstractions.AlreadyAttendedException">The RSVP is already marked attended (AC1 duplicate prevention).</exception>
    /// <exception cref="Chh.Application.Abstractions.RsvpNotEligibleForAttendanceException">The RSVP was cancelled.</exception>
    /// <exception cref="Chh.Application.Abstractions.AttendanceOutsideWindowException">Outside the check-in window (1 hour before start until the event ends).</exception>
    Task<EventParticipantDto?> MarkAttendedAsync(string callerMobileNumber, Guid eventId, Guid rsvpId, CancellationToken ct);
}
