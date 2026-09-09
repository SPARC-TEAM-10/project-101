using Chh.Application.Dtos;
using Chh.Domain.Enums;

namespace Chh.Application.Contracts;

/// <summary>Logic layer for event attendance analytics (CHH-45/US-CHH-005-08).</summary>
public interface IEventAnalyticsService
{
    /// <summary>
    /// Returns the event's attendance summary (AC1), or <c>null</c> if no event exists with that id.
    /// </summary>
    /// <param name="callerMobileNumber">The authenticated caller's mobile number (from the JWT "sub" claim).</param>
    /// <param name="eventId">The event to summarize.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Chh.Application.Abstractions.EventNotOwnedByCallerException">The caller's facility doesn't own this event.</exception>
    Task<EventAttendanceSummaryDto?> GetSummaryAsync(string callerMobileNumber, Guid eventId, CancellationToken ct);

    /// <summary>
    /// Returns the event's participants (AC2), optionally filtered by <paramref name="statusFilter"/>
    /// and/or a name/mobile <paramref name="search"/> term, or <c>null</c> if no event exists with
    /// that id.
    /// </summary>
    /// <param name="callerMobileNumber">The authenticated caller's mobile number (from the JWT "sub" claim).</param>
    /// <param name="eventId">The event to list participants for.</param>
    /// <param name="statusFilter">Optional view-status filter.</param>
    /// <param name="search">Optional free-text name/mobile filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Chh.Application.Abstractions.EventNotOwnedByCallerException">The caller's facility doesn't own this event.</exception>
    Task<IReadOnlyList<EventParticipantAttendanceDto>?> GetParticipantsAsync(
        string callerMobileNumber, Guid eventId, AttendanceViewStatus? statusFilter, string? search, CancellationToken ct);

    /// <summary>
    /// Returns a CSV export of the event's participants (AC3 — permitted fields only, no raw
    /// mobile number), or <c>null</c> if no event exists with that id.
    /// </summary>
    /// <param name="callerMobileNumber">The authenticated caller's mobile number (from the JWT "sub" claim).</param>
    /// <param name="eventId">The event to export.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Chh.Application.Abstractions.EventNotOwnedByCallerException">The caller's facility doesn't own this event.</exception>
    Task<string?> ExportCsvAsync(string callerMobileNumber, Guid eventId, CancellationToken ct);
}
