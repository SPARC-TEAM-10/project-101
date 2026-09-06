namespace Chh.Domain.Constants;

/// <summary>
/// Role names issued as the JWT "role" claim on OTP verification (PRD §4 Role & Permission
/// Matrix). Only <see cref="Guest"/> and <see cref="Individual"/> are resolvable today — Hospital
/// Admin, NGO, and System Admin are assigned through registration/verification flows not yet built.
/// </summary>
public static class RoleConstants
{
    /// <summary>Default role for a verified mobile number with no completed registration.</summary>
    public const string Guest = "Guest";

    /// <summary>Role for a verified mobile number with a completed individual registration (CHH-F02).</summary>
    public const string Individual = "Individual";
}
