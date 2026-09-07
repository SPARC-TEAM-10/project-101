using System.Security.Claims;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
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
}
