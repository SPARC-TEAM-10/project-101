namespace Chh.Application.Contracts;

/// <summary>
/// Tracks "last seen active" for individuals (CHH-34 presence tracking) — see
/// <c>Chh.Domain.Constants.PresenceConstants</c> for the active-window/throttle thresholds this
/// feeds, and <c>Chh.Api.Middleware.ActivityTrackingMiddleware</c> for the caller.
/// </summary>
public interface IPresenceTrackerService
{
    /// <summary>
    /// Records that <paramref name="mobileNumber"/> made an authenticated request just now.
    /// A no-op (no database write) if the profile doesn't exist yet, or was already marked active
    /// within <c>PresenceConstants.UpdateThrottle</c>.
    /// </summary>
    /// <param name="mobileNumber">The authenticated caller's mobile number, from the JWT.</param>
    /// <param name="ct">Cancellation token.</param>
    Task TrackActivityAsync(string mobileNumber, CancellationToken ct);
}
