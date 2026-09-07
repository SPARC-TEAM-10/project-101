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
    /// suspension). Assigned via <see cref="AdminMobileNumber"/> below — see that constant's doc
    /// comment for why.
    /// </summary>
    public const string SystemAdmin = "SystemAdmin";

    /// <summary>
    /// INTERIM SHORTCUT (CHH-F07, added 2026-09-07, by explicit user decision): the mobile number
    /// treated as the System Admin identity. No Role/RoleId or User-with-IsActive infrastructure
    /// exists in this backend yet, so real PRD §4 role-based authorization isn't buildable today —
    /// this hardcoded number is the interim mechanism for the whole CHH-F07 epic (CHH-73/74/75/76).
    /// A mobile number that verifies OTP with this value is issued <see cref="SystemAdmin"/>
    /// instead of the usual Guest/Individual resolution (see <c>OtpService.VerifyOtpAsync</c>).
    /// Documented in the CHH-F07 LLD's "Cross-cutting decisions" — proper RBAC is future scope and
    /// should replace this constant, not extend it.
    /// </summary>
    public const string AdminMobileNumber = "7907468509";
}
