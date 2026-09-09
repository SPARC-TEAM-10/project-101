using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chh.Api.Controllers;

/// <summary>
/// System Admin user-moderation endpoints (CHH-76/US-CHH-001-04, Epic CHH-72). A sibling of
/// <see cref="AdminController"/>, per that class's own doc comment. Unlike <see cref="AdminController"/>,
/// this class's own name doesn't kebab-case to "admin" (it's "AdminUsers" -> "admin-users"), so the
/// global "api/v1/[controller]" convention's <c>[controller]</c> token would resolve wrong here —
/// this uses an absolute <see cref="RouteAttribute"/> instead, which
/// <c>AttributeRouteModel.CombineAttributeRouteModel</c> takes as-is, overriding (not appending to)
/// the global prefix, landing on "api/v1/admin/users" exactly.
/// </summary>
[ApiController]
[Route("/api/v1/admin/users")]
[Authorize(Roles = RoleConstants.SystemAdmin)]
public class AdminUsersController : ControllerBase
{
    private readonly IUserAdminService _userAdminService;

    /// <summary>Creates the controller with its service dependency.</summary>
    /// <param name="userAdminService">Logic layer for System Admin user moderation.</param>
    public AdminUsersController(IUserAdminService userAdminService)
    {
        _userAdminService = userAdminService;
    }

    /// <summary>
    /// Lists individual profiles, optionally filtered by mobile number or name (UI Notes: "Search
    /// bar to find users by mobile number or name", CHH-76).
    /// </summary>
    /// <param name="search">Free-text search term; omit to list every profile.</param>
    /// <param name="page">1-based page number (default 1).</param>
    /// <param name="pageSize">Items per page (default 20, max 100).</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpGet("")]
    [ProducesResponseType(typeof(PagedResponse<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponse<AdminUserDto>>> SearchAsync(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _userAdminService.SearchUsersAsync(search, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Suspends a user account (AC1) — blocks its next OTP login and invalidates any currently
    /// live session (<c>Chh.Api.Middleware.AccountStatusMiddleware</c>). 404 if no such user.
    /// </summary>
    /// <param name="id">The individual profile being suspended.</param>
    /// <param name="request">Mandatory suspension reason.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpPatch("{id:guid}/suspend")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AdminUserDto>> SuspendAsync(
        Guid id,
        [FromBody] SuspendUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _userAdminService.SuspendUserAsync(id, request.Reason, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
