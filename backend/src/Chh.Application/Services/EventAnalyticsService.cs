using System.Text;
using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Entities;
using Chh.Domain.Enums;

namespace Chh.Application.Services;

/// <summary>
/// Event attendance analytics (CHH-45/US-CHH-005-08). "No-show" is derived at read time — see
/// <see cref="AttendanceViewStatus"/>'s doc comment — never stored. Scope simplifications
/// consistent with this epic's established tradeoffs: no "notified" breakdown by delivery status,
/// no feedback/audit-history tabs (EventAnalyticsWeb.dc.html's Feedback/Audit history tabs — no
/// story collects feedback or models an audit trail), and CSV export has no participant filter
/// (always all participants, matching both artboards' one-click "Export CSV" with no filter step).
/// </summary>
public class EventAnalyticsService : IEventAnalyticsService
{
    private readonly IEventRepository _eventRepository;
    private readonly IEventRsvpRepository _eventRsvpRepository;
    private readonly IFacilityRepository _facilityRepository;

    /// <summary>Creates the service with its dependencies.</summary>
    /// <param name="eventRepository">Loads the event to check ownership and read its identity/capacity fields.</param>
    /// <param name="eventRsvpRepository">Data layer for the full RSVP list backing every analytics view.</param>
    /// <param name="facilityRepository">Resolves the caller's own facility for the ownership check.</param>
    public EventAnalyticsService(IEventRepository eventRepository, IEventRsvpRepository eventRsvpRepository, IFacilityRepository facilityRepository)
    {
        _eventRepository = eventRepository;
        _eventRsvpRepository = eventRsvpRepository;
        _facilityRepository = facilityRepository;
    }

    /// <inheritdoc />
    public async Task<EventAttendanceSummaryDto?> GetSummaryAsync(string callerMobileNumber, Guid eventId, CancellationToken ct)
    {
        var result = await _eventRepository.GetByIdWithFacilityNameAsync(eventId, ct).ConfigureAwait(false);
        if (result is null)
        {
            return null;
        }

        await EnsureCallerOwnsEventAsync(callerMobileNumber, result.Event, ct).ConfigureAwait(false);

        var rsvps = await _eventRsvpRepository.GetAllWithProfileAsync(eventId, ct).ConfigureAwait(false);
        var calendarEvent = result.Event;
        var hasEnded = calendarEvent.EndAtUtc <= DateTimeOffset.UtcNow;

        var attendedCount = rsvps.Count(r => r.EventRsvp.Status == EventRsvpStatus.Attended);
        var cancelledCount = rsvps.Count(r => r.EventRsvp.Status == EventRsvpStatus.Cancelled);
        var goingCount = rsvps.Count(r => r.EventRsvp.Status == EventRsvpStatus.Going);
        var noShowCount = hasEnded ? goingCount : 0;
        var rsvpdCount = attendedCount + goingCount;

        return new EventAttendanceSummaryDto
        {
            EventId = calendarEvent.Id,
            Title = calendarEvent.Title,
            EventType = calendarEvent.EventType,
            VenueName = calendarEvent.VenueName,
            FacilityName = result.FacilityName,
            Status = calendarEvent.Status,
            StartAtUtc = calendarEvent.StartAtUtc,
            EndAtUtc = calendarEvent.EndAtUtc,
            Capacity = calendarEvent.Capacity,
            NotifiedCount = calendarEvent.NotifiedCount,
            RsvpdCount = rsvpdCount,
            AttendedCount = attendedCount,
            NoShowCount = noShowCount,
            CancelledCount = cancelledCount,
            RemainingCapacity = calendarEvent.Capacity - rsvpdCount,
            AttendanceRatePercent = rsvpdCount == 0 ? 0 : (int)Math.Round(attendedCount * 100m / rsvpdCount)
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EventParticipantAttendanceDto>?> GetParticipantsAsync(
        string callerMobileNumber, Guid eventId, AttendanceViewStatus? statusFilter, string? search, CancellationToken ct)
    {
        var result = await _eventRepository.GetByIdWithFacilityNameAsync(eventId, ct).ConfigureAwait(false);
        if (result is null)
        {
            return null;
        }

        await EnsureCallerOwnsEventAsync(callerMobileNumber, result.Event, ct).ConfigureAwait(false);

        var rsvps = await _eventRsvpRepository.GetAllWithProfileAsync(eventId, ct).ConfigureAwait(false);
        var hasEnded = result.Event.EndAtUtc <= DateTimeOffset.UtcNow;
        var trimmedSearch = search?.Trim();

        return rsvps
            .Select(r => (Row: r, ViewStatus: ToViewStatus(r.EventRsvp.Status, hasEnded)))
            .Where(x => statusFilter is null || x.ViewStatus == statusFilter)
            .Where(x => string.IsNullOrEmpty(trimmedSearch)
                || x.Row.FullName.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase)
                || x.Row.MobileNumber == trimmedSearch)
            .OrderByDescending(x => x.Row.EventRsvp.CreatedAtUtc)
            .Select(x => ToDto(x.Row, x.ViewStatus))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<string?> ExportCsvAsync(string callerMobileNumber, Guid eventId, CancellationToken ct)
    {
        var result = await _eventRepository.GetByIdWithFacilityNameAsync(eventId, ct).ConfigureAwait(false);
        if (result is null)
        {
            return null;
        }

        await EnsureCallerOwnsEventAsync(callerMobileNumber, result.Event, ct).ConfigureAwait(false);

        var rsvps = await _eventRsvpRepository.GetAllWithProfileAsync(eventId, ct).ConfigureAwait(false);
        var hasEnded = result.Event.EndAtUtc <= DateTimeOffset.UtcNow;

        var csv = new StringBuilder();
        csv.AppendLine("Full Name,Reference Code,Mobile Number,RSVP'd At (UTC),Status,Attended At (UTC),Marked By");
        foreach (var r in rsvps.OrderByDescending(r => r.EventRsvp.CreatedAtUtc))
        {
            var viewStatus = ToViewStatus(r.EventRsvp.Status, hasEnded);
            csv.AppendLine(string.Join(",",
                CsvField(r.FullName),
                CsvField(r.EventRsvp.ReferenceCode),
                CsvField(MaskMobileNumber(r.MobileNumber)),
                CsvField(r.EventRsvp.CreatedAtUtc.ToString("u")),
                CsvField(viewStatus.ToString()),
                CsvField(r.EventRsvp.AttendedAtUtc?.ToString("u") ?? ""),
                CsvField(r.EventRsvp.AttendedByName ?? "")));
        }

        return csv.ToString();
    }

    private async Task EnsureCallerOwnsEventAsync(string callerMobileNumber, Event calendarEvent, CancellationToken ct)
    {
        var facility = await _facilityRepository.GetByContactMobileNumberAsync(callerMobileNumber, ct).ConfigureAwait(false);
        if (facility is null || facility.Id != calendarEvent.FacilityId)
        {
            throw new EventNotOwnedByCallerException();
        }
    }

    // A Going RSVP reads as NoShow once the event has ended and it was never marked Attended —
    // see AttendanceViewStatus's doc comment for why this is derived rather than stored.
    private static AttendanceViewStatus ToViewStatus(EventRsvpStatus status, bool hasEnded) => status switch
    {
        EventRsvpStatus.Cancelled => AttendanceViewStatus.Cancelled,
        EventRsvpStatus.Attended => AttendanceViewStatus.Attended,
        EventRsvpStatus.Going when hasEnded => AttendanceViewStatus.NoShow,
        _ => AttendanceViewStatus.Going
    };

    private static EventParticipantAttendanceDto ToDto(EventRsvpWithProfileResult result, AttendanceViewStatus viewStatus) => new()
    {
        RsvpId = result.EventRsvp.Id,
        FullName = result.FullName,
        MaskedMobileNumber = MaskMobileNumber(result.MobileNumber),
        ReferenceCode = result.EventRsvp.ReferenceCode,
        Status = viewStatus,
        RsvpCreatedAtUtc = result.EventRsvp.CreatedAtUtc,
        AttendedAtUtc = result.EventRsvp.AttendedAtUtc,
        AttendedByName = result.EventRsvp.AttendedByName
    };

    // Matches EventAttendanceService's masking format ("+91 ••••••7213") — kept local rather than
    // shared since it's one line; not worth an abstraction across two services.
    private static string MaskMobileNumber(string mobileNumber) =>
        mobileNumber.Length >= 4 ? $"+91 ••••••{mobileNumber[^4..]}" : "+91 ••••••";

    private static string CsvField(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
