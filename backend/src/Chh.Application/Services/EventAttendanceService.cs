using System.Text.RegularExpressions;
using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Constants;
using Chh.Domain.Entities;
using Chh.Domain.Enums;

namespace Chh.Application.Services;

/// <summary>
/// Manual attendance marking (CHH-44/US-CHH-005-07) — search RSVP'd participants and mark one
/// attended. Implements the spec §6.1 audit requirement's "actor" and "timestamp" fields via
/// <see cref="EventRsvp.AttendedByName"/>/<see cref="EventRsvp.AttendedAtUtc"/>; "method" (always
/// manual — QR check-in was dropped from CHH-37 on 2026-09-05) and "source device" aren't modeled,
/// since nothing in this REST API meaningfully observes a caller's device. A separate attendance
/// "correction" flow (§6.3, undo/re-mark with a reason) is out of scope for this story.
/// </summary>
public class EventAttendanceService : IEventAttendanceService
{
    private static readonly Regex MobilePattern = new(@"^\d{10}$", RegexOptions.Compiled);

    private readonly IEventRepository _eventRepository;
    private readonly IEventRsvpRepository _eventRsvpRepository;
    private readonly IFacilityRepository _facilityRepository;
    private readonly IIndividualProfileRepository _individualProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Creates the service with its dependencies.</summary>
    /// <param name="eventRepository">Loads the event to check ownership and the attendance time window.</param>
    /// <param name="eventRsvpRepository">Data layer for participant search and the attendance mutation.</param>
    /// <param name="facilityRepository">Resolves the caller's own facility (ownership check) and the marking contact's display name.</param>
    /// <param name="individualProfileRepository">Resolves the marked participant's own name/mobile for the response DTO.</param>
    /// <param name="unitOfWork">Persists the attendance mutation.</param>
    public EventAttendanceService(
        IEventRepository eventRepository,
        IEventRsvpRepository eventRsvpRepository,
        IFacilityRepository facilityRepository,
        IIndividualProfileRepository individualProfileRepository,
        IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _eventRsvpRepository = eventRsvpRepository;
        _facilityRepository = facilityRepository;
        _individualProfileRepository = individualProfileRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EventParticipantDto>?> SearchParticipantsAsync(
        string callerMobileNumber, Guid eventId, string search, CancellationToken ct)
    {
        var result = await _eventRepository.GetByIdWithFacilityNameAsync(eventId, ct).ConfigureAwait(false);
        if (result is null)
        {
            return null;
        }

        await EnsureCallerOwnsEventAsync(callerMobileNumber, result.Event, ct).ConfigureAwait(false);

        var trimmed = search.Trim();
        var isValidSearch = MobilePattern.IsMatch(trimmed) || trimmed.Length >= EventConstants.MinAttendanceSearchNameLength;
        if (!isValidSearch)
        {
            throw new ChhValidationException(EventConstants.AttendanceSearchTooShortMessage, new Dictionary<string, string[]>
            {
                ["search"] = new[] { EventConstants.AttendanceSearchTooShortMessage }
            });
        }

        var matches = await _eventRsvpRepository.SearchParticipantsAsync(eventId, trimmed, ct).ConfigureAwait(false);
        return matches
            .OrderByDescending(m => m.EventRsvp.CreatedAtUtc)
            .Select(ToDto)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<EventParticipantDto?> MarkAttendedAsync(string callerMobileNumber, Guid eventId, Guid rsvpId, CancellationToken ct)
    {
        var calendarEvent = await _eventRepository.GetTrackedByIdAsync(eventId, ct).ConfigureAwait(false);
        if (calendarEvent is null)
        {
            return null;
        }

        var facility = await EnsureCallerOwnsEventAsync(callerMobileNumber, calendarEvent, ct).ConfigureAwait(false);

        var rsvp = await _eventRsvpRepository.GetTrackedByIdAsync(rsvpId, ct).ConfigureAwait(false);
        if (rsvp is null || rsvp.EventId != eventId)
        {
            return null;
        }

        if (rsvp.Status == EventRsvpStatus.Attended)
        {
            throw new AlreadyAttendedException();
        }

        if (rsvp.Status != EventRsvpStatus.Going)
        {
            throw new RsvpNotEligibleForAttendanceException();
        }

        var now = DateTimeOffset.UtcNow;
        var windowOpensAtUtc = calendarEvent.StartAtUtc - EventConstants.AttendanceWindowBeforeStart;
        if (now < windowOpensAtUtc || now > calendarEvent.EndAtUtc)
        {
            throw new AttendanceOutsideWindowException();
        }

        var markerContact = facility.Contacts.FirstOrDefault(c => c.Mobile == callerMobileNumber);

        rsvp.Status = EventRsvpStatus.Attended;
        rsvp.AttendedAtUtc = now;
        rsvp.AttendedByName = markerContact?.Name ?? facility.FacilityName;

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        var profile = await _individualProfileRepository.GetTrackedByIdAsync(rsvp.IndividualProfileId, ct).ConfigureAwait(false);
        return ToDto(rsvp, profile?.FullName ?? "", profile?.MobileNumber ?? "");
    }

    private async Task<Facility> EnsureCallerOwnsEventAsync(string callerMobileNumber, Event calendarEvent, CancellationToken ct)
    {
        var facility = await _facilityRepository.GetByContactMobileNumberAsync(callerMobileNumber, ct).ConfigureAwait(false);
        if (facility is null || facility.Id != calendarEvent.FacilityId)
        {
            throw new EventNotOwnedByCallerException();
        }

        return facility;
    }

    private static EventParticipantDto ToDto(EventRsvpWithProfileResult result) =>
        ToDto(result.EventRsvp, result.FullName, result.MobileNumber);

    private static EventParticipantDto ToDto(EventRsvp rsvp, string fullName, string mobileNumber) => new()
    {
        RsvpId = rsvp.Id,
        FullName = fullName,
        MaskedMobileNumber = MaskMobileNumber(mobileNumber),
        ReferenceCode = rsvp.ReferenceCode,
        Status = rsvp.Status,
        RsvpCreatedAtUtc = rsvp.CreatedAtUtc,
        AttendedAtUtc = rsvp.AttendedAtUtc,
        AttendedByName = rsvp.AttendedByName
    };

    // ManualAttendance.dc.html: "+91 ••••••7213" — last 4 digits visible, the rest masked.
    private static string MaskMobileNumber(string mobileNumber) =>
        mobileNumber.Length >= 4 ? $"+91 ••••••{mobileNumber[^4..]}" : "+91 ••••••";
}
