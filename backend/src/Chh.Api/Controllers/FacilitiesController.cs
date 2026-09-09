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

    /// <summary>
    /// Uploads a license document for a facility (CHH-79/US-CHH-003-02), replacing any previously
    /// uploaded one. 404 if no facility exists for <paramref name="id"/>; 422 if the file's type
    /// isn't PDF/JPEG/PNG or it exceeds 5MB.
    /// </summary>
    /// <param name="id">The facility the document belongs to.</param>
    /// <param name="file">The uploaded file (multipart/form-data, field name "file").</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpPost("{id:guid}/upload")]
    // Anonymous: the wizard uploads the document immediately after anonymous registration
    // (CHH-10's Hospital/Ngo role isn't issued until the contact's first OTP verification), same
    // reasoning as RegisterAsync above.
    [AllowAnonymous]
    [RequestSizeLimit(6 * 1024 * 1024)] // 5MB file + form-data overhead headroom (AC3).
    [ProducesResponseType(typeof(FacilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FacilityDto>> UploadLicenseDocumentAsync(
        Guid id,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        await using var content = file?.OpenReadStream() ?? Stream.Null;
        var result = await _facilityService.UploadLicenseDocumentAsync(
            id,
            content,
            file?.FileName ?? string.Empty,
            file?.ContentType ?? string.Empty,
            file?.Length ?? 0,
            cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Searches verified facilities for the Emergency Services Hub (CHH-82/US-CHH-001-01, Epic
    /// CHH-68). Always restricted to <see cref="FacilityVerificationStatus.Verified"/> facilities
    /// server-side, regardless of caller role. Requires a valid JWT — any authenticated role,
    /// including Guest (api-standards.md §5).
    /// </summary>
    /// <param name="q">Matches facility name or address, case-insensitive (AC2).</param>
    /// <param name="category">Optional category filter (AC1).</param>
    /// <param name="latitude">Caller's device latitude, for distance sort (AC3).</param>
    /// <param name="longitude">Caller's device longitude, for distance sort (AC3).</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpGet("search")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResponse<PublicFacilityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<PublicFacilityDto>>> SearchAsync(
        [FromQuery] string? q,
        [FromQuery] FacilityCategory? category,
        [FromQuery] decimal? latitude,
        [FromQuery] decimal? longitude,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var request = new SearchFacilitiesRequest
        {
            Q = q,
            Category = category,
            Latitude = latitude,
            Longitude = longitude,
            Page = page,
            PageSize = pageSize
        };
        var result = await _facilityService.SearchAsync(request, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Returns one facility's public detail for the Emergency Services Hub (CHH-82/US-CHH-001-02,
    /// Epic CHH-68) — facility name, category, full address, and contacts. 404 if the facility
    /// doesn't exist or isn't Verified.
    /// </summary>
    /// <param name="id">The facility id.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(PublicFacilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicFacilityDto>> GetPublicDetailAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _facilityService.GetPublicDetailAsync(id, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }
}
