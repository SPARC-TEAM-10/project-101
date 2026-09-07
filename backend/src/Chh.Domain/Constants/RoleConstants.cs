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
    /// suspension). Assigned to a mobile number whose <see cref="Chh.Domain.Entities.IndividualProfile.IsAdmin"/>
    /// flag is set (see <c>OtpService.VerifyOtpAsync</c>) — that flag has no self-service way to
    /// become <c>true</c> yet, so granting it is a manual, out-of-band DB operation until a proper
    /// admin-management endpoint exists.
    /// </summary>
    public const string SystemAdmin = "SystemAdmin";
}
