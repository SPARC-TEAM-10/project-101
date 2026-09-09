using Chh.Application.Dtos;
using Chh.Domain.Entities;
using Chh.Domain.Enums;

namespace Chh.Application.Factories;

/// <summary>
/// Constructs <see cref="Event"/> instances from a validated request, matching
/// <see cref="BloodRequestFactory"/>'s established pattern.
/// </summary>
public static class EventFactory
{
    /// <summary>Creates a new published event from a validated request.</summary>
    /// <param name="facilityId">The organizing (verified) facility's id.</param>
    /// <param name="request">The validated event creation details.</param>
    /// <param name="createdAtUtc">UTC timestamp the event is created.</param>
    public static Event Create(Guid facilityId, CreateEventRequest request, DateTimeOffset createdAtUtc)
    {
        return new Event
        {
            FacilityId = facilityId,
            Title = request.Title.Trim(),
            EventType = request.EventType,
            Description = request.Description.Trim(),
            VenueName = request.VenueName.Trim(),
            VenueAddress = request.VenueAddress.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            StartAtUtc = request.StartAtUtc,
            EndAtUtc = request.EndAtUtc,
            Capacity = request.Capacity,
            CoordinatorName = request.CoordinatorName.Trim(),
            CoordinatorContact = request.CoordinatorContact.Trim(),
            RsvpCutoffAtUtc = request.RsvpCutoffAtUtc,
            Status = EventStatus.Published,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }
}
