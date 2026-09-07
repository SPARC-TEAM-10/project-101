using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chh.Api.Controllers;

/// <summary>
/// System Admin facility moderation endpoints (CHH-F07 Admin Command Center, Epic CHH-72).
/// "api/v1" comes from the global convention in <c>Program.cs</c>; "admin/facilities" is this
/// controller's own route, matching the contract (`contracts/chh-api.v1.yaml`)'s two-segment
/// path and CHH-76's future sibling "admin/users/...".
/// </summary>
[ApiController]
[Route("admin/facilities")]
[Authorize(Roles = RoleConstants.SystemAdmin)]
public class AdminFacilitiesController : ControllerBase
{
    private readonly IFacilityAdminService _facilityAdminService;

    /// <summary>Creates the controller with its service dependency.</summary>
    /// <param name="facilityAdminService">Logic layer for System Admin facility moderation.</param>
    public AdminFacilitiesController(IFacilityAdminService facilityAdminService)
    {
        _facilityAdminService = facilityAdminService;
    }

    /// <summary>
    /// Lists facilities awaiting verification, paginated (CHH-73/US-CHH-001-01 AC1). An empty
    /// result is a normal <c>200</c> with an empty <c>items</c> array — the "No pending
    /// verifications at this time" message (AC2) is a frontend concern.
    /// </summary>
    /// <param name="page">1-based page number (default 1).</param>
    /// <param name="pageSize">Items per page (default 20, max 100).</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(PagedResponse<FacilityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponse<FacilityDto>>> GetPendingAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _facilityAdminService.GetPendingFacilitiesAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }
}
