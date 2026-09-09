using Chh.Domain.Entities;

namespace Chh.Application.Dtos;

/// <summary>
/// An <see cref="EventRsvp"/> joined with its individual's name/mobile — internal repository
/// projection for CHH-44's participant search, matching <see cref="EventWithFacilityNameResult"/>'s pattern.
/// </summary>
public class EventRsvpWithProfileResult
{
    public required EventRsvp EventRsvp { get; init; }
    public required string FullName { get; init; }
    public required string MobileNumber { get; init; }
}
