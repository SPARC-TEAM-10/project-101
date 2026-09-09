using Chh.Domain.Entities;

namespace Chh.Application.Dtos;

/// <summary>
/// An <see cref="Event"/> joined with its organizing facility's name (CHH-39/US-CHH-005-02) — the
/// "Kochi Metro Hospital" line on each discovery card. Internal repository projection, not a wire
/// DTO — <see cref="EventSummaryDto"/> is what actually crosses the API boundary.
/// </summary>
public record EventWithFacilityNameResult
{
    public required Event Event { get; init; }
    public required string FacilityName { get; init; }
}
