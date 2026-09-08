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
    private readonly IDonorResponseService _donorResponseService;

    /// <summary>Creates the controller with its service dependencies.</summary>
    /// <param name="notificationService">Logic layer for reading and updating the caller's own notifications.</param>
    /// <param name="donorResponseService">Logic layer for accepting/declining a matched request (CHH-35).</param>
    public NotificationsController(INotificationService notificationService, IDonorResponseService donorResponseService)
    {
        _notificationService = notificationService;
        _donorResponseService = donorResponseService;
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

    /// <summary>
    /// Accepts the matched blood request behind this notification (CHH-35 AC1). 404 if it doesn't
    /// exist, belongs to a different donor, or the caller hasn't registered an individual profile
    /// yet; 409 if already responded; 422 ("This request is no longer active") if the request has
    /// since been fulfilled or expired, or its last remaining unit was just taken (AC3/Edge Case).
    /// </summary>
    /// <param name="id">The notification being responded to.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpPatch("{id:guid}/accept")]
    [ProducesResponseType(typeof(DonorResponseResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<DonorResponseResultDto>> AcceptAsync(Guid id, CancellationToken cancellationToken)
    {
        var mobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _donorResponseService.AcceptAsync(mobileNumber, id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Declines the matched blood request behind this notification (CHH-35 AC2). 404 if it doesn't
    /// exist, belongs to a different donor, or the caller hasn't registered an individual profile
    /// yet; 409 if already responded.
    /// </summary>
    /// <param name="id">The notification being responded to.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpPatch("{id:guid}/decline")]
    [ProducesResponseType(typeof(DonorResponseResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DonorResponseResultDto>> DeclineAsync(Guid id, CancellationToken cancellationToken)
    {
        var mobileNumber = User.FindFirstValue(ClaimTypes.MobilePhone)!;
        var result = await _donorResponseService.DeclineAsync(mobileNumber, id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
