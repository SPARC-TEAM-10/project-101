using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chh.Api.Controllers;

/// <summary>
/// Facility registration endpoints (CHH-78). The "api/v1/facilities" route is applied globally
/// in <c>Program.cs</c> — this class's own empty <see cref="RouteAttribute"/> only exists to
/// satisfy <c>[ApiController]</c>'s "must be attribute-routed" check, which runs before
/// <c>RoutePrefixConvention</c> supplies the real route (same reasoning as <c>IndividualsController</c>).
/// </summary>
[ApiController]
[Route("")]
public class FacilitiesController : ControllerBase
{
    private const string RouteName = "RegisterFacility";

    private readonly IFacilityService _facilityService;

    /// <summary>Creates the controller with its service dependency.</summary>
    /// <param name="facilityService">Logic layer for facility registration.</param>
    public FacilitiesController(IFacilityService facilityService)
    {
        _facilityService = facilityService;
    }

    /// <summary>
    /// Registers a new facility (hospital/blood-bank or NGO), pending System Admin verification.
    /// </summary>
    /// <param name="request">The registration details.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpPost(Name = RouteName)]
    // Anonymous, matching IndividualsController.RegisterAsync's precedent: Hospital/NGO isn't an
    // issued JWT role yet (see frontend AuthProvider.tsx), so there's no session claim to gate on.
    [AllowAnonymous]
    [ProducesResponseType(typeof(FacilityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FacilityDto>> RegisterAsync(
        [FromBody] CreateFacilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _facilityService.RegisterAsync(request, cancellationToken);
        // Same CreatedAtRoute-pointing-back-at-itself simplification as IndividualsController —
        // no GET /facilities/{id} exists yet (out of scope for this ticket).
        return CreatedAtRoute(RouteName, new { id = result.Id }, result);
    }
}
