using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>
/// Response item for <c>GET /api/v1/events/{id}/attendance/participants</c> (CHH-45/US-CHH-005-08
/// AC2) — like <see cref="EventParticipantDto"/> but carries the derived <see cref="AttendanceViewStatus"/>
/// (including "NoShow") instead of the raw persisted <see cref="Chh.Domain.Enums.EventRsvpStatus"/>.
/// Never the raw EventRsvp entity (db-standards.md §2b) — the mobile number is masked.
/// </summary>
public record EventParticipantAttendanceDto
{
    public required Guid RsvpId { get; init; }
    public required string FullName { get; init; }
    public required string MaskedMobileNumber { get; init; }
    public required string ReferenceCode { get; init; }
    public required AttendanceViewStatus Status { get; init; }
    public required DateTimeOffset RsvpCreatedAtUtc { get; init; }
    public DateTimeOffset? AttendedAtUtc { get; init; }
    public string? AttendedByName { get; init; }
}
