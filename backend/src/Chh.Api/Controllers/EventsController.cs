using System.Security.Claims;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Constants;
using Chh.Domain.Enums;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chh.Api.Controllers;

/// <summary>
/// Event endpoints (CHH-38/US-CHH-005-01, CHH-39/US-CHH-005-02, part of Epic CHH-37 — CHH-F05
/// Location-Aware Events). The "api/v1/events" route is applied globally in <c>Program.cs</c>.
/// Class-level <see cref="AuthorizeAttribute"/> is bare (any authenticated role) since discovery
/// (AC1) is open to everyone, not just facilities — <see cref="CreateAsync"/> adds its own
/// Hospital/Ngo role requirement (multiple [Authorize] attributes combine with AND, so the class
/// level must stay unrestricted for that to work as "search: any role, create: Hospital/Ngo only").
/// </summary>
[ApiController]
[Route("")]
[Authorize]
public class EventsController : ControllerBase
{
    private const string RouteName = "CreateEvent";
    private const string GetByIdRouteName = "GetEventById";

    private readonly IEventService _eventService;
    private readonly IEventAttendanceService _eventAttendanceService;
    private readonly IEventAnalyticsService _eventAnalyticsService;

    /// <summary>Creates the controller with its service dependencies.</summary>
    /// <param name="eventService">Logic layer for event creation, discovery, RSVP, and edit/cancellation.</param>
    /// <param name="eventAttendanceService">Logic layer for manual attendance marking (CHH-44).</param>
    /// <param name="eventAnalyticsService">Logic layer for attendance analytics (CHH-45).</param>
    public EventsController(IEventService eventService, IEventAttendanceService eventAttendanceService, IEventAnalyticsService eventAnalyticsService)
    {
        _eventService = eventService;
        _eventAttendanceService = eventAttendanceService;
        _eventAnalyticsService = eventAnalyticsService;
    }

    /// <summary>
    /// Creates a new published event for the caller's (verified) facility (AC1). Requires the
    /// Hospital or Ngo role — the creator's mobile number is taken from the token's "sub" claim,
    /// never trusted from the request body.
    /// </summary>
    /// <param name="request">The event creation details.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpPost(Name = RouteName)]
    [Authorize(Roles = $"{RoleConstants.Hospital},{RoleConstants.Ngo}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EventDto>> CreateAsync(
        [FromBody] CreateEventRequest request,
        CancellationToken cancellationToken)
    {
        var creatorMobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _eventService.CreateAsync(creatorMobileNumber, request, cancellationToken);

        return CreatedAtRoute(GetByIdRouteName, new { id = result.Id }, result);
    }

    /// <summary>
    /// Returns the event detail view (CHH-40 detail page) — open to any authenticated role.
    /// Includes the caller's own RSVP status when they're an Individual who has RSVP'd.
    /// </summary>
    /// <param name="id">The event's id.</param>
    /// <param name="latitude">Optional caller latitude, to compute <c>distanceKm</c>.</param>
    /// <param name="longitude">Optional caller longitude, to compute <c>distanceKm</c>.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpGet("{id:guid}", Name = GetByIdRouteName)]
    [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDetailDto>> GetByIdAsync(
        [FromRoute] Guid id,
        [FromQuery] decimal? latitude,
        [FromQuery] decimal? longitude,
        CancellationToken cancellationToken)
    {
        var callerMobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _eventService.GetByIdAsync(id, callerMobileNumber, latitude, longitude, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// RSVPs the caller to the event (AC1). Requires the Individual role. 422 if the event is full
    /// (AC2), 409 if the caller already has an active RSVP (AC3).
    /// </summary>
    /// <param name="id">The event to RSVP to.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpPost("{id:guid}/rsvp")]
    [Authorize(Roles = RoleConstants.Individual)]
    [ProducesResponseType(typeof(RsvpResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RsvpResponseDto>> RsvpAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var mobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _eventService.RsvpAsync(mobileNumber, id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Cancels the caller's own active RSVP and releases the spot (Edge Case). Requires the
    /// Individual role. 404 if the caller has no active RSVP for that event.
    /// </summary>
    /// <param name="id">The event to cancel the RSVP for.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpDelete("{id:guid}/rsvp")]
    [Authorize(Roles = RoleConstants.Individual)]
    [ProducesResponseType(typeof(RsvpResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RsvpResponseDto>> CancelRsvpAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var mobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _eventService.CancelRsvpAsync(mobileNumber, id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Returns published, upcoming events within a radius of the given coordinates, nearest-start
    /// first (AC1) — open to any authenticated role. <c>radiusKm</c> is clamped to [5,100], not
    /// rejected (AC3).
    /// </summary>
    /// <param name="latitude">Search origin latitude.</param>
    /// <param name="longitude">Search origin longitude.</param>
    /// <param name="radiusKm">Requested search radius in kilometers.</param>
    /// <param name="eventType">Optional event type filter.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<EventSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<EventSummaryDto>>> SearchAsync(
        [FromQuery] decimal latitude,
        [FromQuery] decimal longitude,
        [FromQuery] int radiusKm,
        [FromQuery] EventType? eventType,
        CancellationToken cancellationToken)
    {
        var result = await _eventService.SearchAsync(latitude, longitude, radiusKm, eventType, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns every event (any status) belonging to the caller's own facility, most recent start
    /// first (CHH-41's manage-event entry point). Requires the Hospital or Ngo role.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpGet("mine")]
    [Authorize(Roles = $"{RoleConstants.Hospital},{RoleConstants.Ngo}")]
    [ProducesResponseType(typeof(IReadOnlyList<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetMineAsync(CancellationToken cancellationToken)
    {
        var callerMobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _eventService.GetMineAsync(callerMobileNumber, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Partially updates the event (AC3). Requires the Hospital or Ngo role, and only the
    /// organizing facility may edit its own event (403 otherwise). 422 if the event already
    /// started, is already cancelled, or the new capacity is below the current RSVP count.
    /// </summary>
    /// <param name="id">The event to update.</param>
    /// <param name="request">The partial update.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = $"{RoleConstants.Hospital},{RoleConstants.Ngo}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateEventRequest request,
        CancellationToken cancellationToken)
    {
        var callerMobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _eventService.UpdateAsync(callerMobileNumber, id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Cancels the event and notifies its RSVP'd attendees (AC1/AC2). Requires the Hospital or Ngo
    /// role, and only the organizing facility may cancel its own event (403 otherwise). 422 if the
    /// event already started or is already cancelled.
    /// </summary>
    /// <param name="id">The event to cancel.</param>
    /// <param name="request">The mandatory cancellation reason.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = $"{RoleConstants.Hospital},{RoleConstants.Ngo}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> CancelAsync(
        [FromRoute] Guid id,
        [FromBody] CancelEventRequest request,
        CancellationToken cancellationToken)
    {
        var callerMobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _eventService.CancelAsync(callerMobileNumber, id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Searches the event's RSVP'd participants by name (3+ characters) or full mobile number
    /// (CHH-44 AC1/AC2) — the organizer's "mark attendance" lookup. Requires the Hospital or Ngo
    /// role; only the organizing facility may search its own event's participants (403 otherwise).
    /// </summary>
    /// <param name="id">The event to search participants for.</param>
    /// <param name="search">A name fragment (3+ characters) or a full 10-digit mobile number.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpGet("{id:guid}/rsvps")]
    [Authorize(Roles = $"{RoleConstants.Hospital},{RoleConstants.Ngo}")]
    [ProducesResponseType(typeof(IReadOnlyList<EventParticipantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<EventParticipantDto>>> SearchParticipantsAsync(
        [FromRoute] Guid id,
        [FromQuery] string search,
        CancellationToken cancellationToken)
    {
        var callerMobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _eventAttendanceService.SearchParticipantsAsync(callerMobileNumber, id, search ?? "", cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Marks the given RSVP attended (CHH-44 AC1). Requires the Hospital or Ngo role; only the
    /// organizing facility may mark its own event's participants (403 otherwise). 409 if already
    /// marked (duplicate prevention), 422 if the RSVP was cancelled or it's outside the check-in
    /// window (1 hour before the event starts until it ends).
    /// </summary>
    /// <param name="id">The event the RSVP belongs to.</param>
    /// <param name="rsvpId">The RSVP to mark attended.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpPost("{id:guid}/rsvps/{rsvpId:guid}/attend")]
    [Authorize(Roles = $"{RoleConstants.Hospital},{RoleConstants.Ngo}")]
    [ProducesResponseType(typeof(EventParticipantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventParticipantDto>> MarkAttendedAsync(
        [FromRoute] Guid id,
        [FromRoute] Guid rsvpId,
        CancellationToken cancellationToken)
    {
        var callerMobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _eventAttendanceService.MarkAttendedAsync(callerMobileNumber, id, rsvpId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Returns the event's attendance summary (CHH-45 AC1). Requires the Hospital or Ngo role;
    /// only the organizing facility may view its own event's analytics (403 otherwise).
    /// </summary>
    /// <param name="id">The event to summarize.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpGet("{id:guid}/attendance/summary")]
    [Authorize(Roles = $"{RoleConstants.Hospital},{RoleConstants.Ngo}")]
    [ProducesResponseType(typeof(EventAttendanceSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventAttendanceSummaryDto>> GetAttendanceSummaryAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var callerMobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _eventAnalyticsService.GetSummaryAsync(callerMobileNumber, id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Returns the event's participants, optionally filtered by status and/or a name/mobile search
    /// term (CHH-45 AC2). Requires the Hospital or Ngo role; only the organizing facility may view
    /// its own event's participants (403 otherwise).
    /// </summary>
    /// <param name="id">The event to list participants for.</param>
    /// <param name="status">Optional view-status filter (Going, Cancelled, Attended, or NoShow).</param>
    /// <param name="search">Optional free-text name/mobile filter.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpGet("{id:guid}/attendance/participants")]
    [Authorize(Roles = $"{RoleConstants.Hospital},{RoleConstants.Ngo}")]
    [ProducesResponseType(typeof(IReadOnlyList<EventParticipantAttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<EventParticipantAttendanceDto>>> GetAttendanceParticipantsAsync(
        [FromRoute] Guid id,
        [FromQuery] AttendanceViewStatus? status,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var callerMobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _eventAnalyticsService.GetParticipantsAsync(callerMobileNumber, id, status, search, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Exports the event's participants as CSV — permitted fields only, no raw mobile number (CHH-45
    /// AC3). Requires the Hospital or Ngo role; only the organizing facility may export its own
    /// event's attendance (403 otherwise).
    /// </summary>
    /// <param name="id">The event to export.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpGet("{id:guid}/attendance/export")]
    [Authorize(Roles = $"{RoleConstants.Hospital},{RoleConstants.Ngo}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportAttendanceCsvAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var callerMobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var csv = await _eventAnalyticsService.ExportCsvAsync(callerMobileNumber, id, cancellationToken);
        if (csv is null)
        {
            return NotFound();
        }

        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"event-{id}-attendance.csv");
    }
}
