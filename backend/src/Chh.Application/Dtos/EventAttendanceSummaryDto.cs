using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>
/// Response body for <c>GET /api/v1/events/{id}/attendance/summary</c> (CHH-45/US-CHH-005-08 AC1),
/// matching EventAnalyticsWeb.dc.html's stat cards and "from notified to attended" funnel.
/// </summary>
public record EventAttendanceSummaryDto
{
    public required Guid EventId { get; init; }
    public required string Title { get; init; }
    public required EventType EventType { get; init; }
    public required string VenueName { get; init; }
    public required string FacilityName { get; init; }
    public required EventStatus Status { get; init; }
    public required DateTimeOffset StartAtUtc { get; init; }
    public required DateTimeOffset EndAtUtc { get; init; }
    public required int Capacity { get; init; }

    /// <summary>Individuals targeted by the publish-notification fan-out (CHH-42) — 0 until that job has run.</summary>
    public required int NotifiedCount { get; init; }

    /// <summary>Going + Attended (non-cancelled RSVPs).</summary>
    public required int RsvpdCount { get; init; }

    public required int AttendedCount { get; init; }

    /// <summary>RSVP'd, event ended, never marked attended. Always 0 before the event ends.</summary>
    public required int NoShowCount { get; init; }

    public required int CancelledCount { get; init; }

    /// <summary>Capacity minus RsvpdCount.</summary>
    public required int RemainingCapacity { get; init; }

    /// <summary>Attended ÷ RsvpdCount as a whole-number percentage (0 if nobody has RSVP'd).</summary>
    public required int AttendanceRatePercent { get; init; }
}
