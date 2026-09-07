using Chh.Domain.Enums;

namespace Chh.Domain.Entities;

/// <summary>
/// An individual user's registration profile (CHH-F02), completed after OTP verification
/// (CHH-9). Date of birth and health-screening flags are PII/health data encrypted at rest via
/// an EF Core value converter — see `.claude/rules/db-standards.md` §3 and
/// <c>Chh.Infrastructure.Persistence.Configurations.IndividualProfileConfiguration</c>.
/// A plain property bag — construction/derivation logic (e.g. <c>IsReceiverOnly</c>) lives in
/// <see cref="Chh.Application.Factories.IndividualProfileFactory"/>, which sets these `internal`
/// setters via object initializer (see <c>Chh.Domain.csproj</c>'s <c>InternalsVisibleTo</c>).
/// </summary>
public class IndividualProfile
{
    /// <summary>Surrogate primary key.</summary>
    public Guid Id { get; internal set; } = Guid.NewGuid();

    /// <summary>The OTP-verified mobile number this profile belongs to (CHH-9).</summary>
    public string MobileNumber { get; internal set; } = default!;

    /// <summary>Full name, trimmed, 2–50 characters.</summary>
    public string FullName { get; internal set; } = default!;

    /// <summary>Email address.</summary>
    public string Email { get; internal set; } = default!;

    /// <summary>Blood group.</summary>
    public BloodGroup BloodGroup { get; internal set; }

    /// <summary>Date of birth. Encrypted at rest (PII).</summary>
    public DateOnly DateOfBirth { get; internal set; }

    /// <summary>Gender.</summary>
    public Gender Gender { get; internal set; }

    /// <summary>City/area — free text for now (see CHH-F02 doc's Open Questions on a predefined location list).</summary>
    public string LocationCityArea { get; internal set; } = default!;

    /// <summary>Self-reported chronic illness. Encrypted at rest (health data).</summary>
    public bool IsChronicIllness { get; internal set; }

    /// <summary>Self-reported recent surgery. Encrypted at rest (health data).</summary>
    public bool HasRecentSurgery { get; internal set; }

    /// <summary>Self-reported infectious disease. Encrypted at rest (health data).</summary>
    public bool IsInfectiousDisease { get; internal set; }

    /// <summary>Self-reported underweight status. Encrypted at rest (health data).</summary>
    public bool IsUnderweight { get; internal set; }

    /// <summary>Self-reported "Other" illness flag. Encrypted at rest (health data).</summary>
    public bool IsOtherIllness { get; internal set; }

    /// <summary>Free-text detail when <see cref="IsOtherIllness"/> is set, max 200 chars. Encrypted at rest (health data).</summary>
    public string? OtherIllnessDetails { get; internal set; }

    /// <summary>
    /// True if any health-restriction flag is set (PRD §7 CHH-F02 AC2) — excludes the profile
    /// from donor search while still allowing blood requests. Derived at creation time; stored
    /// unencrypted, it's an operational flag rather than raw health detail (db-standards.md §3).
    /// </summary>
    public bool IsReceiverOnly { get; internal set; }

    /// <summary>UTC timestamp the profile was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; internal set; }

    /// <summary>
    /// Grants the CHH-F07 Admin Command Center's <see cref="Chh.Domain.Constants.RoleConstants.SystemAdmin"/>
    /// role on OTP verification (<c>OtpService.VerifyOtpAsync</c>). Defaults <c>false</c> — never
    /// set from a self-service registration request; flipping it is an out-of-band operation
    /// (direct DB access) until a proper admin-management endpoint exists.
    /// </summary>
    public bool IsAdmin { get; internal set; }

    /// <summary>
    /// Registered latitude, used for proximity donor matching (US-CHH-004-02/CHH-80 AC4) — never
    /// live GPS. Nullable: the CHH-F02 registration flow doesn't collect coordinates yet, so
    /// existing/new profiles start with none; a donor with no coordinates is excluded from
    /// matching rather than treated as an error (documented Edge Case).
    /// </summary>
    public decimal? Latitude { get; internal set; }

    /// <summary>Registered longitude — see <see cref="Latitude"/>.</summary>
    public decimal? Longitude { get; internal set; }

    /// <summary>
    /// Account standing (US-CHH-004-02/CHH-80 AC3). A <see cref="Enums.AccountStatus.Suspended"/>
    /// donor is excluded from proximity matching. Defaults to <see cref="Enums.AccountStatus.Active"/>.
    /// </summary>
    public AccountStatus AccountStatus { get; internal set; } = AccountStatus.Active;
}
