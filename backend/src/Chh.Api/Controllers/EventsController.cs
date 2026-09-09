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

    /// <summary>Creates the controller with its service dependency.</summary>
    /// <param name="eventService">Logic layer for event creation.</param>
    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
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
}
