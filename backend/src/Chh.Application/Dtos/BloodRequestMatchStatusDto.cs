using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>
/// Response body for <c>GET /api/v1/blood-requests/{id}/matches</c> and
/// <c>PATCH /api/v1/blood-requests/{id}/radius</c> (CHH-36). <see cref="NotifiedCount"/> being zero
/// while <see cref="Status"/> is still "Matching" is the AC3 "no eligible donors found" signal —
/// there's no separate flag for it, the client derives it from these two fields.
/// </summary>
public record BloodRequestMatchStatusDto
{
    /// <summary>The blood request this status is for.</summary>
    public required Guid BloodRequestId { get; init; }

    /// <summary>Current lifecycle state.</summary>
    public required BloodRequestStatus Status { get; init; }

    /// <summary>Current search radius, in kilometers.</summary>
    public required int SearchRadiusKm { get; init; }

    /// <summary>Units originally required.</summary>
    public required int UnitsRequired { get; init; }

    /// <summary>Units accepted so far (CHH-35).</summary>
    public required int UnitsAccepted { get; init; }

    /// <summary>UTC timestamp the request auto-expires.</summary>
    public required DateTimeOffset ExpiresAtUtc { get; init; }

    /// <summary>Count of donors notified (AC1 "Notifications Sent").</summary>
    public required int NotifiedCount { get; init; }

    /// <summary>Count of donors who have opened/viewed their notification (AC1 "Viewed").</summary>
    public required int ViewedCount { get; init; }

    /// <summary>Count of donors who accepted (AC1 "Accepted").</summary>
    public required int AcceptedCount { get; init; }

    /// <summary>
    /// The matched-donor list (AC2), shaped per the guest-vs-registered visibility rule — see
    /// <see cref="DonorStatusDto"/>.
    /// </summary>
    public required IReadOnlyList<DonorStatusDto> Donors { get; init; }
}
