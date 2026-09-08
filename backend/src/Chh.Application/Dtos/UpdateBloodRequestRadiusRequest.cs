namespace Chh.Application.Dtos;

/// <summary>Request body for <c>PATCH /api/v1/blood-requests/{id}/radius</c> (CHH-36 AC4).</summary>
public record UpdateBloodRequestRadiusRequest
{
    /// <summary>The new search radius, in kilometers. Must be larger than the current one, up to 100km.</summary>
    public required int SearchRadiusKm { get; init; }
}
