using System.Security.Claims;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chh.Api.Controllers;

/// <summary>
/// Donor notification endpoints (CHH-34/US-CHH-004-03, part of Epic CHH-25 — CHH-F04 Proximity
/// Notifications). The "api/v1/notifications" route is applied globally in <c>Program.cs</c>.
/// </summary>
[ApiController]
[Route("")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    /// <summary>Creates the controller with its service dependency.</summary>
    /// <param name="notificationService">Logic layer for reading and updating the caller's own notifications.</param>
    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>Returns the authenticated caller's own notifications, newest first (AC3).</summary>
    /// <param name="page">1-based page number (default 1).</param>
    /// <param name="pageSize">Items per page (default 20, max 100).</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpGet("mine")]
    [ProducesResponseType(typeof(PagedResponse<DonorNotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<DonorNotificationDto>>> GetMyNotificationsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var mobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _notificationService.GetMyNotificationsAsync(mobileNumber, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Marks one of the caller's own notifications read. 404 if it doesn't exist, belongs to a
    /// different donor, or the caller hasn't registered an individual profile yet.
    /// </summary>
    /// <param name="id">The notification to mark read.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(typeof(DonorNotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DonorNotificationDto>> MarkReadAsync(Guid id, CancellationToken cancellationToken)
    {
        var mobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _notificationService.MarkReadAsync(mobileNumber, id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
