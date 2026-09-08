using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chh.Api.Controllers;

/// <summary>
/// System Admin endpoints (CHH-F07 Admin Command Center, Epic CHH-72). Named <c>Admin</c> (not
/// <c>AdminFacilities</c>) so the global "api/v1/[controller]" convention resolves
/// <c>[controller]</c> to "admin", and this class's own <see cref="RouteAttribute"/> only needs
/// to add the "facilities" segment — no absolute-route override needed, unlike an earlier
/// revision of this file. CHH-76's future sibling endpoints ("admin/users/...") should live in a
/// separate <c>AdminUsersController</c> with its own <c>[Route("users")]</c>, following the same
/// pattern, rather than growing this class to cover both resource groups.
/// </summary>
[ApiController]
[Route("facilities")]
[Authorize(Roles = RoleConstants.SystemAdmin)]
public class AdminController : ControllerBase
{
    private readonly IFacilityAdminService _facilityAdminService;

    /// <summary>Creates the controller with its service dependency.</summary>
    /// <param name="facilityAdminService">Logic layer for System Admin facility moderation.</param>
    public AdminController(IFacilityAdminService facilityAdminService)
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
