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

        // Same CreatedAtRoute-pointing-back-at-itself simplification as FacilitiesController —
        // no GET /events/{id} exists yet (out of scope for this story).
        return CreatedAtRoute(RouteName, new { id = result.Id }, result);
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
