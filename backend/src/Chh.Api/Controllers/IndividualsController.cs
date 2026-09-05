using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chh.Api.Controllers;

/// <summary>Individual registration endpoints (CHH-F02). The "api/v1/individuals" route is applied globally in <c>Program.cs</c>.</summary>
[ApiController]
public class IndividualsController : ControllerBase
{
    private readonly IIndividualProfileService _individualProfileService;

    /// <summary>Creates the controller with its service dependency.</summary>
    /// <param name="individualProfileService">Logic layer for individual registration.</param>
    public IndividualsController(IIndividualProfileService individualProfileService)
    {
        _individualProfileService = individualProfileService;
    }

    /// <summary>Registers a new individual profile for an OTP-verified mobile number.</summary>
    /// <param name="request">The registration details.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    // Empty-string template, not bare [HttpPost]: [ApiController] validates that every action is
    // attribute-routed *before* RoutePrefixConvention (a controller-model convention) ever runs,
    // so an action with no template of its own fails that check even though the convention would
    // have supplied one. AuthController's actions never hit this because they carry their own
    // literal templates ("otp/request", "otp/verify"); anything relying purely on the controller
    // prefix needs the same explicit (if empty) template.
    [HttpPost("")]
    // Anonymous: no session token exists yet (CHH-9 is verify-only) — the mobile-number-verified
    // guard inside the service is the actual gate, not [Authorize].
    [AllowAnonymous]
    [ProducesResponseType(typeof(IndividualProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IndividualProfileDto>> RegisterAsync(
        [FromBody] CreateIndividualProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _individualProfileService.RegisterAsync(request, cancellationToken);
        return Created($"/api/v1/individuals/{result.Id}", result);
    }
}
