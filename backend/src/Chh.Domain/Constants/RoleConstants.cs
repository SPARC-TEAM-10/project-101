namespace Chh.Domain.Constants;

/// <summary>
/// Role names issued as the JWT "role" claim on OTP verification (PRD §4 Role & Permission
/// Matrix). <see cref="Guest"/>, <see cref="Individual"/>, and <see cref="SystemAdmin"/> are
/// resolvable today — Hospital Admin and NGO are assigned through registration/verification flows
/// not yet built.
/// </summary>
public static class RoleConstants
{
    /// <summary>Default role for a verified mobile number with no completed registration.</summary>
    public const string Guest = "Guest";

    /// <summary>Role for a verified mobile number with a completed individual registration (CHH-F02).</summary>
    public const string Individual = "Individual";

    /// <summary>
    /// Role gating the CHH-F07 Admin Command Center endpoints (facility verification, user
    /// suspension). Assigned to any mobile number with an active <c>AdminUser.IsAdmin</c> grant
    /// (see <c>IAdminUserRepository</c>, checked in <c>OtpService.VerifyOtpAsync</c>) — not yet
    /// full Role/RoleId-based authorization (PRD §4 Role & Permission Matrix), but data-driven
    /// rather than a compiled-in mobile number (PR #18 review feedback superseded the earlier
    /// <c>AdminMobileNumber</c> constant this class used to carry).
    /// </summary>
    public const string SystemAdmin = "SystemAdmin";
}
