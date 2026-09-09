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
    private readonly IEventRsvpRepository _eventRsvpRepository;
    private readonly IIndividualProfileRepository _individualProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Creates the service with its repository and unit-of-work dependencies.</summary>
    /// <param name="facilityRepository">Resolves the caller's own facility and its verification status (reuses CHH-10/CHH-28's lookup).</param>
    /// <param name="eventRepository">Data layer for persisting events and the atomic RSVP-count reserve/release (CHH-40).</param>
    /// <param name="eventRsvpRepository">Data layer for persisting individual RSVP rows (CHH-40).</param>
    /// <param name="individualProfileRepository">Resolves the caller's own <c>IndividualProfile.Id</c> from their mobile number (CHH-40).</param>
    /// <param name="unitOfWork">Persists changes made during the request.</param>
    public EventService(
        IFacilityRepository facilityRepository,
        IEventRepository eventRepository,
        IEventRsvpRepository eventRsvpRepository,
        IIndividualProfileRepository individualProfileRepository,
        IUnitOfWork unitOfWork)
    {
        _facilityRepository = facilityRepository;
        _eventRepository = eventRepository;
        _eventRsvpRepository = eventRsvpRepository;
        _individualProfileRepository = individualProfileRepository;
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
                SpotsRemaining = r.Candidate.Event.Capacity - r.Candidate.Event.RsvpCount
            })
            .ToList();
    }

    /// <inheritdoc />
    public async Task<EventDetailDto?> GetByIdAsync(
        Guid eventId, string callerMobileNumber, decimal? latitude, decimal? longitude, CancellationToken ct)
    {
        var candidate = await _eventRepository.GetByIdWithFacilityNameAsync(eventId, ct).ConfigureAwait(false);
        if (candidate is null)
        {
            return null;
        }

        var calendarEvent = candidate.Event;

        EventRsvpStatus? myRsvpStatus = null;
        string? myReferenceCode = null;

        var profile = await _individualProfileRepository.GetByMobileNumberAsync(callerMobileNumber, ct).ConfigureAwait(false);
        if (profile is not null)
        {
            var myRsvp = await _eventRsvpRepository.GetByEventAndIndividualAsync(eventId, profile.Id, ct).ConfigureAwait(false);
            if (myRsvp is not null)
            {
                myRsvpStatus = myRsvp.Status;
                myReferenceCode = myRsvp.ReferenceCode;
            }
        }

        return new EventDetailDto
        {
            Id = calendarEvent.Id,
            Title = calendarEvent.Title,
            EventType = calendarEvent.EventType,
            Description = calendarEvent.Description,
            FacilityName = candidate.FacilityName,
            VenueName = calendarEvent.VenueName,
            VenueAddress = calendarEvent.VenueAddress,
            Latitude = calendarEvent.Latitude,
            Longitude = calendarEvent.Longitude,
            StartAtUtc = calendarEvent.StartAtUtc,
            EndAtUtc = calendarEvent.EndAtUtc,
            Capacity = calendarEvent.Capacity,
            SpotsRemaining = calendarEvent.Capacity - calendarEvent.RsvpCount,
            CoordinatorName = calendarEvent.CoordinatorName,
            CoordinatorContact = calendarEvent.CoordinatorContact,
            RsvpCutoffAtUtc = calendarEvent.RsvpCutoffAtUtc,
            Status = calendarEvent.Status,
            DistanceKm = latitude is not null && longitude is not null
                ? HaversineDistanceCalculator.CalculateDistanceKm(latitude.Value, longitude.Value, calendarEvent.Latitude, calendarEvent.Longitude)
                : null,
            MyRsvpStatus = myRsvpStatus,
            MyReferenceCode = myReferenceCode
        };
    }

    /// <inheritdoc />
    public async Task<RsvpResponseDto?> RsvpAsync(string mobileNumber, Guid eventId, CancellationToken ct)
    {
        var profile = await _individualProfileRepository.GetByMobileNumberAsync(mobileNumber, ct).ConfigureAwait(false);
        if (profile is null)
        {
            return null;
        }

        var calendarEvent = await _eventRepository.GetByIdWithFacilityNameAsync(eventId, ct).ConfigureAwait(false);
        if (calendarEvent is null)
        {
            return null;
        }

        var existingRsvp = await _eventRsvpRepository
            .GetTrackedByEventAndIndividualAsync(eventId, profile.Id, ct)
            .ConfigureAwait(false);

        if (existingRsvp is not null && existingRsvp.Status == EventRsvpStatus.Going)
        {
            throw new AlreadyRsvpdException();
        }

        var reserved = await _eventRepository.TryReserveSpotAsync(eventId, ct).ConfigureAwait(false);
        if (!reserved)
        {
            throw new EventFullException();
        }

        var now = DateTimeOffset.UtcNow;

        if (existingRsvp is not null)
        {
            // Re-RSVPing after a prior cancellation — reactivate the same row so ReferenceCode stays stable.
            existingRsvp.Status = EventRsvpStatus.Going;
            existingRsvp.CancelledAtUtc = null;
        }
        else
        {
            var referenceCode = await NextReferenceCodeAsync(eventId, ct).ConfigureAwait(false);
            existingRsvp = EventRsvpFactory.Create(eventId, profile.Id, referenceCode, now);
            await _eventRsvpRepository.AddAsync(existingRsvp, ct).ConfigureAwait(false);
        }

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        var updatedEvent = await _eventRepository.GetByIdWithFacilityNameAsync(eventId, ct).ConfigureAwait(false);

        return new RsvpResponseDto
        {
            EventId = eventId,
            Status = existingRsvp.Status,
            ReferenceCode = existingRsvp.ReferenceCode,
            SpotsRemaining = updatedEvent is null ? 0 : updatedEvent.Event.Capacity - updatedEvent.Event.RsvpCount
        };
    }

    /// <inheritdoc />
    public async Task<RsvpResponseDto?> CancelRsvpAsync(string mobileNumber, Guid eventId, CancellationToken ct)
    {
        var profile = await _individualProfileRepository.GetByMobileNumberAsync(mobileNumber, ct).ConfigureAwait(false);
        if (profile is null)
        {
            return null;
        }

        var rsvp = await _eventRsvpRepository
            .GetTrackedByEventAndIndividualAsync(eventId, profile.Id, ct)
            .ConfigureAwait(false);

        if (rsvp is null || rsvp.Status != EventRsvpStatus.Going)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        rsvp.Status = EventRsvpStatus.Cancelled;
        rsvp.CancelledAtUtc = now;

        await _eventRepository.ReleaseSpotAsync(eventId, ct).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        var updatedEvent = await _eventRepository.GetByIdWithFacilityNameAsync(eventId, ct).ConfigureAwait(false);

        return new RsvpResponseDto
        {
            EventId = eventId,
            Status = rsvp.Status,
            ReferenceCode = null,
            SpotsRemaining = updatedEvent is null ? 0 : updatedEvent.Event.Capacity - updatedEvent.Event.RsvpCount
        };
    }

    private async Task<string> NextReferenceCodeAsync(Guid eventId, CancellationToken ct)
    {
        var count = await _eventRsvpRepository.CountForEventAsync(eventId, ct).ConfigureAwait(false);
        return $"A{count + 1}";
    }
}
