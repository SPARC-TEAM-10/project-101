using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chh.Api.Controllers;

/// <summary>Individual registration endpoints (CHH-F02). "api/v1" comes from the global convention in <c>Program.cs</c>; "individuals" is this controller's own route.</summary>
[ApiController]
[Route("individuals")]
public class IndividualsController : ControllerBase
{
    private const string RouteName = "RegisterIndividual";

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
    [HttpPost(Name = RouteName)]
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
        // CreatedAtRoute, not a hand-built "/api/v1/individuals/{id}" string — the URL is
        // generated from the route itself, so it can't silently drift if RoutePrefixConvention's
        // prefix or the kebab-case transform ever changes. There's no GET /individuals/{id} yet
        // (out of scope for this ticket, see the doc's Open Questions), so this points back at
        // this same POST route; revisit once that GET exists.
        return CreatedAtRoute(RouteName, new { id = result.Id }, result);
    }
}
