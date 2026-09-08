using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>
/// Query parameters for <c>GET /api/v1/facilities/search</c> (CHH-82/US-CHH-001-01, Epic CHH-68),
/// bundled by the controller before reaching the Service layer.
/// </summary>
public record SearchFacilitiesRequest
{
    /// <summary>Matches facility name or address, case-insensitive (CHH-69 AC2). Null/empty = no filter.</summary>
    public string? Q { get; init; }

    /// <summary>Category filter (CHH-69 AC1). Null = all categories.</summary>
    public FacilityCategory? Category { get; init; }

    /// <summary>Caller's device latitude, for distance sort (CHH-69 AC3). Null if not granted.</summary>
    public decimal? Latitude { get; init; }

    /// <summary>Caller's device longitude — see <see cref="Latitude"/>.</summary>
    public decimal? Longitude { get; init; }

    /// <summary>1-based page number.</summary>
    public required int Page { get; init; }

    /// <summary>Items per page.</summary>
    public required int PageSize { get; init; }
}
