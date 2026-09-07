using System.Security.Claims;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chh.Api.Controllers;

/// <summary>
/// Facility registration endpoints (CHH-78/US-CHH-003-01, part of Epic CHH-77 — CHH-F03 Facility
/// Verification). The "api/v1/facilities" route is applied globally in <c>Program.cs</c>.
/// </summary>
[ApiController]
[Route("")]
[Authorize]
public class FacilitiesController : ControllerBase
{
    private const string RouteName = "CreateFacility";

    private readonly IFacilityService _facilityService;

    /// <summary>Creates the controller with its service dependency.</summary>
    /// <param name="facilityService">Logic layer for facility registration.</param>
    public FacilitiesController(IFacilityService facilityService)
    {
        _facilityService = facilityService;
    }

    /// <summary>
    /// Registers a new facility in "Pending" status (AC1). Requires a valid JWT
    /// (api-standards.md §5) — no Hospital/NGO-specific role check yet, since that role isn't
    /// issued (see plan Open Questions). The creator's mobile number is taken from the token's
    /// "sub" claim, never trusted from the request body.
    /// </summary>
    /// <param name="request">The facility registration details.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(FacilityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FacilityDto>> CreateAsync(
        [FromBody] CreateFacilityRequest request,
        CancellationToken cancellationToken)
    {
        var createdByMobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _facilityService.CreateAsync(createdByMobileNumber, request, cancellationToken);

        // Same CreatedAtRoute-pointing-back-at-itself simplification as BloodRequestsController —
        // no GET /facilities/{id} exists yet (out of scope for this story).
        return CreatedAtRoute(RouteName, new { id = result.Id }, result);
    }
}
