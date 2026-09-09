using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>
/// Response item for <c>GET /api/v1/events/{id}/rsvps</c> and the result of
/// <c>POST .../rsvps/{rsvpId}/attend</c> (CHH-44/US-CHH-005-07). Never the raw <c>EventRsvp</c>
/// entity (db-standards.md §2b) — deliberately excludes the individual's full mobile number
/// (masked instead, ManualAttendance.dc.html: "+91 ••••••7213").
/// </summary>
public record EventParticipantDto
{
    public required Guid RsvpId { get; init; }
    public required string FullName { get; init; }
    public required string MaskedMobileNumber { get; init; }
    public required string ReferenceCode { get; init; }
    public required EventRsvpStatus Status { get; init; }
    public required DateTimeOffset RsvpCreatedAtUtc { get; init; }
    public DateTimeOffset? AttendedAtUtc { get; init; }
    public string? AttendedByName { get; init; }
}
