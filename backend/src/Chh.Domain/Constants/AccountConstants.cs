namespace Chh.Domain.Constants;

/// <summary>User-facing copy for individual-account moderation (CHH-76/US-CHH-001-04).</summary>
public static class AccountConstants
{
    /// <summary>
    /// Shown both when a suspended account attempts OTP login (AC2) and when an already-live
    /// session's next authenticated request is blocked (AC1) — see
    /// <c>Chh.Api.Middleware.AccountStatusMiddleware</c>.
    /// </summary>
    public const string SuspendedMessage = "Your account has been suspended. Contact support.";
}
