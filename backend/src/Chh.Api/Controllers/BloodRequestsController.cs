using System.Security.Claims;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Constants;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chh.Api.Controllers;

/// <summary>
/// Blood request endpoints (CHH-33/US-CHH-004-01, part of Epic CHH-25 — CHH-F04 Proximity
/// Notifications). The "api/v1/blood-requests" route is applied globally in <c>Program.cs</c>.
/// </summary>
[ApiController]
[Route("")]
[Authorize]
public class BloodRequestsController : ControllerBase
{
    private const string RouteName = "CreateBloodRequest";

    private readonly IBloodRequestService _bloodRequestService;

    /// <summary>Creates the controller with its service dependency.</summary>
    /// <param name="bloodRequestService">Logic layer for blood request creation.</param>
    public BloodRequestsController(IBloodRequestService bloodRequestService)
    {
        _bloodRequestService = bloodRequestService;
    }

    /// <summary>
    /// Creates a new blood request for the authenticated requester, transitioning it to
    /// "Matching" (AC1). Requires a valid JWT (api-standards.md §5) — the requester's mobile
    /// number is taken from the token's "sub" claim, never trusted from the request body.
    /// </summary>
    /// <param name="request">The blood request details.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(BloodRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BloodRequestDto>> CreateAsync(
        [FromBody] CreateBloodRequestRequest request,
        CancellationToken cancellationToken)
    {
        var requesterMobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _bloodRequestService.CreateAsync(requesterMobileNumber, request, cancellationToken);

        // Same CreatedAtRoute-pointing-back-at-itself simplification as IndividualsController —
        // no GET /blood-requests/{id} exists yet (out of scope for this story).
        return CreatedAtRoute(RouteName, new { id = result.Id }, result);
    }

    /// <summary>Returns the authenticated caller's own blood requests, newest first (CHH-81 Individual Dashboard).</summary>
    /// <param name="page">1-based page number (default 1).</param>
    /// <param name="pageSize">Items per page (default 20, max 100).</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpGet("mine")]
    [ProducesResponseType(typeof(PagedResponse<BloodRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<BloodRequestDto>>> GetMyRequestsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var requesterMobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _bloodRequestService.GetMyRequestsAsync(requesterMobileNumber, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns the caller's own request's match/response status (CHH-36 AC1/AC2/AC3). 404 if it
    /// doesn't exist or wasn't created by this caller.
    /// </summary>
    /// <param name="id">The blood request to look up.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpGet("{id:guid}/matches")]
    [ProducesResponseType(typeof(BloodRequestMatchStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BloodRequestMatchStatusDto>> GetMatchStatusAsync(Guid id, CancellationToken cancellationToken)
    {
        var requesterMobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var isGuest = User.IsInRole(RoleConstants.Guest);
        var result = await _bloodRequestService.GetMatchStatusAsync(requesterMobileNumber, id, isGuest, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Expands the caller's own request's search radius and re-triggers matching (CHH-36 AC4).
    /// 404 if it doesn't exist or wasn't created by this caller; 422 if the request is no longer
    /// "Matching" (fulfilled/expired) or the new radius isn't larger than the current one.
    /// </summary>
    /// <param name="id">The blood request to update.</param>
    /// <param name="request">The new search radius.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpPatch("{id:guid}/radius")]
    [ProducesResponseType(typeof(BloodRequestMatchStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BloodRequestMatchStatusDto>> UpdateRadiusAsync(
        Guid id,
        [FromBody] UpdateBloodRequestRadiusRequest request,
        CancellationToken cancellationToken)
    {
        var requesterMobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var isGuest = User.IsInRole(RoleConstants.Guest);
        var result = await _bloodRequestService.UpdateRadiusAsync(requesterMobileNumber, id, request.SearchRadiusKm, isGuest, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
