namespace Chh.Domain.Entities;

/// <summary>
/// A mobile number granted System Admin access (CHH-F07 Admin Command Center). Replaces the
/// earlier hardcoded <c>RoleConstants.AdminMobileNumber</c> constant per PR #18 review feedback —
/// admin status is now a data-driven flag, checked by <c>OtpService.VerifyOtpAsync</c> against
/// this table instead of a compiled-in mobile number. Still not full Role/RoleId-based
/// authorization (PRD §4) — see the CHH-F07 LLD's "Cross-cutting decisions" — but no longer
/// requires a code change (and redeploy) to add or revoke an admin.
/// </summary>
public class AdminUser
{
    /// <summary>Surrogate primary key.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>The mobile number granted (or previously granted) admin access.</summary>
    public string MobileNumber { get; set; } = default!;

    /// <summary>
    /// Whether this mobile number currently has System Admin access. A row can be kept with this
    /// set to <see langword="false"/> to revoke access without deleting the audit trail of who was
    /// ever granted it.
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>UTC timestamp the row was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
}
