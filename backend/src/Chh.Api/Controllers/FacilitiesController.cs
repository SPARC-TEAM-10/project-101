using System.Security.Claims;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Constants;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chh.Api.Controllers;

/// <summary>
/// Facility registration (CHH-78) and status (CHH-28/US-CHH-003-03) endpoints, part of Epic
/// CHH-77 — CHH-F03 Facility Verification. The "api/v1/facilities" route is applied globally in
/// <c>Program.cs</c> — this class's own empty <see cref="RouteAttribute"/> only exists to satisfy
/// <c>[ApiController]</c>'s "must be attribute-routed" check, which runs before
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
    // Anonymous: registering IS what makes a mobile number resolve to the Hospital/Ngo role
    // (CHH-10 matches on FacilityContact.Mobile) — gating this action behind that role would make
    // a facility's first-ever registration impossible.
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

    /// <summary>
    /// Returns the facility owned by the caller (CHH-28 AC1/AC4), resolved by matching the JWT's
    /// mobile number against a <see cref="Chh.Domain.Entities.FacilityContact"/> — same lookup
    /// <c>OtpService.VerifyOtpAsync</c> uses to grant the Hospital/Ngo role (CHH-10).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpGet("me")]
    [Authorize(Roles = $"{RoleConstants.Hospital},{RoleConstants.Ngo}")]
    [ProducesResponseType(typeof(FacilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FacilityDto>> GetMyFacilityAsync(CancellationToken cancellationToken)
    {
        var mobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _facilityService.GetMyFacilityAsync(mobileNumber, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }
}
