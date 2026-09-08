using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>
/// Response body for <c>PATCH /api/v1/notifications/{id}/accept</c> and
/// <c>.../decline</c> (CHH-35). The contact/location fields (AC4's "confirmation view") are only
/// populated when <see cref="ResponseStatus"/> is <see cref="DonorResponseStatus.Accepted"/> — a
/// decline has nothing to confirm.
/// </summary>
public record DonorResponseResultDto
{
    /// <summary>The notification this response was recorded against.</summary>
    public required Guid NotificationId { get; init; }

    /// <summary>The donor's recorded response.</summary>
    public required DonorResponseStatus ResponseStatus { get; init; }

    /// <summary>The requester's mobile number (AC4) — only present when accepted.</summary>
    public string? RequesterMobileNumber { get; init; }

    /// <summary>
    /// The request's location — the closest available stand-in for AC4's "facility location"
    /// since blood requests aren't linked to a registered facility in this build. Only present
    /// when accepted.
    /// </summary>
    public string? LocationCityArea { get; init; }

    /// <summary>Latitude of the request's location — only present when accepted.</summary>
    public decimal? Latitude { get; init; }

    /// <summary>Longitude of the request's location — only present when accepted.</summary>
    public decimal? Longitude { get; init; }
}
