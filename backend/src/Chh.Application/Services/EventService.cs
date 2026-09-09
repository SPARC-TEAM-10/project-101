using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Factories;
using Chh.Domain.Constants;
using Chh.Domain.Enums;
using Chh.Domain.Utilities;

namespace Chh.Application.Services;

/// <summary>
/// Orchestrates event creation (CHH-38/US-CHH-005-01: verified-facility guard, persistence) and
/// proximity discovery (CHH-39/US-CHH-005-02).
/// </summary>
public class EventService : IEventService
{
    private readonly IFacilityRepository _facilityRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Creates the service with its repository and unit-of-work dependencies.</summary>
    /// <param name="facilityRepository">Resolves the caller's own facility and its verification status (reuses CHH-10/CHH-28's lookup).</param>
    /// <param name="eventRepository">Data layer for persisting events.</param>
    /// <param name="unitOfWork">Persists changes made during the request.</param>
    public EventService(IFacilityRepository facilityRepository, IEventRepository eventRepository, IUnitOfWork unitOfWork)
    {
        _facilityRepository = facilityRepository;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<EventDto> CreateAsync(string creatorMobileNumber, CreateEventRequest request, CancellationToken ct)
    {
        var facility = await _facilityRepository
            .GetByContactMobileNumberAsync(creatorMobileNumber, ct)
            .ConfigureAwait(false);

        if (facility is null || facility.VerificationStatus != FacilityVerificationStatus.Verified)
        {
            throw new FacilityNotVerifiedException();
        }

        var createdAtUtc = DateTimeOffset.UtcNow;
        var calendarEvent = EventFactory.Create(facility.Id, request, createdAtUtc);

        await _eventRepository.AddAsync(calendarEvent, ct).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return new EventDto
        {
            Id = calendarEvent.Id,
            FacilityId = calendarEvent.FacilityId,
            Title = calendarEvent.Title,
            EventType = calendarEvent.EventType,
            Description = calendarEvent.Description,
            VenueName = calendarEvent.VenueName,
            VenueAddress = calendarEvent.VenueAddress,
            Latitude = calendarEvent.Latitude,
            Longitude = calendarEvent.Longitude,
            StartAtUtc = calendarEvent.StartAtUtc,
            EndAtUtc = calendarEvent.EndAtUtc,
            Capacity = calendarEvent.Capacity,
            CoordinatorName = calendarEvent.CoordinatorName,
            CoordinatorContact = calendarEvent.CoordinatorContact,
            RsvpCutoffAtUtc = calendarEvent.RsvpCutoffAtUtc,
            Status = calendarEvent.Status,
            CreatedAtUtc = calendarEvent.CreatedAtUtc,
            UpdatedAtUtc = calendarEvent.UpdatedAtUtc
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EventSummaryDto>> SearchAsync(
        decimal latitude, decimal longitude, int radiusKm, EventType? eventType, CancellationToken ct)
    {
        var clampedRadiusKm = Math.Clamp(radiusKm, EventConstants.MinSearchRadiusKm, EventConstants.MaxSearchRadiusKm);

        var candidates = await _eventRepository
            .GetUpcomingPublishedWithFacilityNameAsync(ct)
            .ConfigureAwait(false);

        var results = new List<(EventWithFacilityNameResult Candidate, decimal DistanceKm)>();
        foreach (var candidate in candidates)
        {
            if (eventType is not null && candidate.Event.EventType != eventType)
            {
                continue;
            }

            var distanceKm = HaversineDistanceCalculator.CalculateDistanceKm(
                latitude, longitude, candidate.Event.Latitude, candidate.Event.Longitude);

            if (distanceKm > clampedRadiusKm)
            {
                continue;
            }

            results.Add((candidate, distanceKm));
        }

        return results
            .OrderBy(r => r.Candidate.Event.StartAtUtc)
            .Select(r => new EventSummaryDto
            {
                Id = r.Candidate.Event.Id,
                Title = r.Candidate.Event.Title,
                EventType = r.Candidate.Event.EventType,
                FacilityName = r.Candidate.FacilityName,
                VenueName = r.Candidate.Event.VenueName,
                Latitude = r.Candidate.Event.Latitude,
                Longitude = r.Candidate.Event.Longitude,
                StartAtUtc = r.Candidate.Event.StartAtUtc,
                EndAtUtc = r.Candidate.Event.EndAtUtc,
                DistanceKm = r.DistanceKm,
                Capacity = r.Candidate.Event.Capacity,
                // No RSVP entity exists yet (CHH-40) — every event's true current spotsRemaining is
                // its full capacity, not a placeholder.
                SpotsRemaining = r.Candidate.Event.Capacity
            })
            .ToList();
    }
}
