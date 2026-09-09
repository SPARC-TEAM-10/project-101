using System.Security.Claims;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Constants;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chh.Api.Controllers;

/// <summary>
/// Event endpoints (CHH-38/US-CHH-005-01, part of Epic CHH-37 — CHH-F05 Location-Aware Events).
/// The "api/v1/events" route is applied globally in <c>Program.cs</c>.
/// </summary>
[ApiController]
[Route("")]
[Authorize(Roles = $"{RoleConstants.Hospital},{RoleConstants.Ngo}")]
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
}
