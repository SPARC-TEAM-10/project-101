using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Factories;
using Chh.Application.Jobs;
using Chh.Domain.Constants;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using Chh.Domain.Utilities;
using Hangfire;

namespace Chh.Application.Services;

/// <summary>
/// Orchestrates event creation (CHH-38/US-CHH-005-01: verified-facility guard, persistence, and
/// CHH-42/US-CHH-005-05's publish notification fan-out), proximity discovery
/// (CHH-39/US-CHH-005-02), and edit/cancellation (CHH-41/US-CHH-005-04).
/// </summary>
public class EventService : IEventService
{
    private readonly IFacilityRepository _facilityRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IEventRsvpRepository _eventRsvpRepository;
    private readonly IIndividualProfileRepository _individualProfileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobClient _backgroundJobClient;

    /// <summary>Creates the service with its repository, unit-of-work, and background-job dependencies.</summary>
    /// <param name="facilityRepository">Resolves the caller's own facility and its verification status (reuses CHH-10/CHH-28's lookup).</param>
    /// <param name="eventRepository">Data layer for persisting events and the atomic RSVP-count reserve/release (CHH-40).</param>
    /// <param name="eventRsvpRepository">Data layer for persisting individual RSVP rows (CHH-40).</param>
    /// <param name="individualProfileRepository">Resolves the caller's own <c>IndividualProfile.Id</c> from their mobile number (CHH-40).</param>
    /// <param name="unitOfWork">Persists changes made during the request.</param>
    /// <param name="backgroundJobClient">
    /// Enqueues <see cref="NotifyEventChangeJob"/> after a notify-worthy edit or cancellation
    /// (CHH-41), and <see cref="NotifyEventPublishedJob"/> after creation (CHH-42).
    /// </param>
    public EventService(
        IFacilityRepository facilityRepository,
        IEventRepository eventRepository,
        IEventRsvpRepository eventRsvpRepository,
        IIndividualProfileRepository individualProfileRepository,
        IUnitOfWork unitOfWork,
        IBackgroundJobClient backgroundJobClient)
    {
        _facilityRepository = facilityRepository;
        _eventRepository = eventRepository;
        _eventRsvpRepository = eventRsvpRepository;
        _individualProfileRepository = individualProfileRepository;
        _unitOfWork = unitOfWork;
        _backgroundJobClient = backgroundJobClient;
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

        // Fire-and-forget: proximity notification fan-out must not delay this response
        // (api-standards.md §6 NFR) — CHH-42/US-CHH-005-05.
        _backgroundJobClient.Enqueue<NotifyEventPublishedJob>(job => job.RunAsync(calendarEvent.Id, CancellationToken.None));

        return ToDto(calendarEvent);
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
            CancellationReason = calendarEvent.CancellationReason,
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

    /// <inheritdoc />
    public async Task<IReadOnlyList<EventDto>> GetMineAsync(string callerMobileNumber, CancellationToken ct)
    {
        var facility = await _facilityRepository.GetByContactMobileNumberAsync(callerMobileNumber, ct).ConfigureAwait(false);
        if (facility is null)
        {
            return Array.Empty<EventDto>();
        }

        var events = await _eventRepository.GetByFacilityAsync(facility.Id, ct).ConfigureAwait(false);
        return events.Select(ToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<EventDto?> UpdateAsync(string callerMobileNumber, Guid eventId, UpdateEventRequest request, CancellationToken ct)
    {
        var calendarEvent = await _eventRepository.GetTrackedByIdAsync(eventId, ct).ConfigureAwait(false);
        if (calendarEvent is null)
        {
            return null;
        }

        await EnsureCallerOwnsEventAsync(callerMobileNumber, calendarEvent, ct).ConfigureAwait(false);
        EnsureEditable(calendarEvent);

        var newStartAtUtc = request.StartAtUtc ?? calendarEvent.StartAtUtc;
        var newEndAtUtc = request.EndAtUtc ?? calendarEvent.EndAtUtc;
        if (newEndAtUtc <= newStartAtUtc)
        {
            throw new ChhValidationException(EventConstants.EndMustBeAfterStartMessage, new Dictionary<string, string[]>
            {
                ["endAtUtc"] = new[] { EventConstants.EndMustBeAfterStartMessage }
            });
        }

        var newCapacity = request.Capacity ?? calendarEvent.Capacity;
        if (newCapacity < calendarEvent.RsvpCount)
        {
            throw new CapacityBelowRsvpCountException();
        }

        // Only these fields are attendee-facing per EventEditWeb.dc.html's literal rule ("Venue,
        // date, time and cancellation changes send a notification. Typo fixes to the description
        // do not.") — title/description/coordinator/capacity/RSVP-cutoff changes don't notify.
        var notifyWorthy =
            (request.VenueName is not null && request.VenueName != calendarEvent.VenueName) ||
            (request.VenueAddress is not null && request.VenueAddress != calendarEvent.VenueAddress) ||
            (request.Latitude is not null && request.Latitude != calendarEvent.Latitude) ||
            (request.Longitude is not null && request.Longitude != calendarEvent.Longitude) ||
            (request.StartAtUtc is not null && request.StartAtUtc != calendarEvent.StartAtUtc) ||
            (request.EndAtUtc is not null && request.EndAtUtc != calendarEvent.EndAtUtc);

        if (request.Title is not null) calendarEvent.Title = request.Title.Trim();
        if (request.EventType is not null) calendarEvent.EventType = request.EventType.Value;
        if (request.Description is not null) calendarEvent.Description = request.Description.Trim();
        if (request.VenueName is not null) calendarEvent.VenueName = request.VenueName.Trim();
        if (request.VenueAddress is not null) calendarEvent.VenueAddress = request.VenueAddress.Trim();
        if (request.Latitude is not null) calendarEvent.Latitude = request.Latitude.Value;
        if (request.Longitude is not null) calendarEvent.Longitude = request.Longitude.Value;
        calendarEvent.StartAtUtc = newStartAtUtc;
        calendarEvent.EndAtUtc = newEndAtUtc;
        calendarEvent.Capacity = newCapacity;
        if (request.CoordinatorName is not null) calendarEvent.CoordinatorName = request.CoordinatorName.Trim();
        if (request.CoordinatorContact is not null) calendarEvent.CoordinatorContact = request.CoordinatorContact.Trim();
        if (request.RsvpCutoffAtUtc is not null) calendarEvent.RsvpCutoffAtUtc = request.RsvpCutoffAtUtc;
        calendarEvent.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        if (notifyWorthy)
        {
            // Fire-and-forget: notification fan-out must not delay this response (api-standards.md §6 NFR).
            _backgroundJobClient.Enqueue<NotifyEventChangeJob>(job => job.RunUpdatedAsync(calendarEvent.Id, CancellationToken.None));
        }

        return ToDto(calendarEvent);
    }

    /// <inheritdoc />
    public async Task<EventDto?> CancelAsync(string callerMobileNumber, Guid eventId, CancelEventRequest request, CancellationToken ct)
    {
        var calendarEvent = await _eventRepository.GetTrackedByIdAsync(eventId, ct).ConfigureAwait(false);
        if (calendarEvent is null)
        {
            return null;
        }

        await EnsureCallerOwnsEventAsync(callerMobileNumber, calendarEvent, ct).ConfigureAwait(false);
        EnsureEditable(calendarEvent);

        calendarEvent.Status = EventStatus.Cancelled;
        calendarEvent.CancellationReason = request.Reason.Trim();
        calendarEvent.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        _backgroundJobClient.Enqueue<NotifyEventChangeJob>(job => job.RunCancelledAsync(calendarEvent.Id, CancellationToken.None));

        return ToDto(calendarEvent);
    }

    private async Task EnsureCallerOwnsEventAsync(string callerMobileNumber, Event calendarEvent, CancellationToken ct)
    {
        var facility = await _facilityRepository.GetByContactMobileNumberAsync(callerMobileNumber, ct).ConfigureAwait(false);
        if (facility is null || facility.Id != calendarEvent.FacilityId)
        {
            throw new EventNotOwnedByCallerException();
        }
    }

    private static void EnsureEditable(Event calendarEvent)
    {
        if (calendarEvent.Status == EventStatus.Cancelled)
        {
            throw new EventAlreadyCancelledException();
        }

        if (calendarEvent.StartAtUtc <= DateTimeOffset.UtcNow)
        {
            throw new EventAlreadyStartedException();
        }
    }

    private static EventDto ToDto(Event calendarEvent) => new()
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
        UpdatedAtUtc = calendarEvent.UpdatedAtUtc,
        CancellationReason = calendarEvent.CancellationReason
    };
}
