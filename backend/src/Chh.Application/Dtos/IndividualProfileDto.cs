using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>
/// Response body for <c>POST /api/v1/individuals</c> and <c>GET/PATCH /api/v1/individuals/me</c>.
/// Deliberately excludes email, DOB — PII the caller already knows and doesn't need echoed back.
/// The raw health-screening flags ARE included (unlike at first write) so the caller's own
/// profile-edit form (CHH-F02 profile edit) can pre-fill accurately instead of guessing from the
/// derived <see cref="IsReceiverOnly"/> flag alone.
/// </summary>
public record IndividualProfileDto
{
    /// <summary>Surrogate primary key.</summary>
    public required Guid Id { get; init; }

    /// <summary>Full name.</summary>
    public required string FullName { get; init; }

    /// <summary>Blood group.</summary>
    public required BloodGroup BloodGroup { get; init; }

    /// <summary>True if any health-restriction flag was set — excluded from donor search, can still request blood (PRD §7 CHH-F02 AC2).</summary>
    public required bool IsReceiverOnly { get; init; }

    /// <summary>Registered city/area — free text (CHH-81 dashboard profile summary).</summary>
    public required string LocationCityArea { get; init; }

    /// <summary>UTC timestamp the profile was created.</summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>Health-screening flag (CHH-F02 profile edit — pre-fills the edit form).</summary>
    public required bool IsChronicIllness { get; init; }

    /// <summary>Health-screening flag (CHH-F02 profile edit — pre-fills the edit form).</summary>
    public required bool HasRecentSurgery { get; init; }

    /// <summary>Health-screening flag (CHH-F02 profile edit — pre-fills the edit form).</summary>
    public required bool IsInfectiousDisease { get; init; }

    /// <summary>Health-screening flag (CHH-F02 profile edit — pre-fills the edit form).</summary>
    public required bool IsUnderweight { get; init; }

    /// <summary>Health-screening flag (CHH-F02 profile edit — pre-fills the edit form).</summary>
    public required bool IsOtherIllness { get; init; }

    /// <summary>Free-text detail when <see cref="IsOtherIllness"/> is true.</summary>
    public string? OtherIllnessDetails { get; init; }

    /// <summary>
    /// Registered latitude for proximity donor matching (US-CHH-004-02/CHH-80, CHH-84); null if
    /// the caller has never shared their location. Lets the profile-edit UI show sharing status.
    /// </summary>
    public decimal? Latitude { get; init; }

    /// <summary>Registered longitude — see <see cref="Latitude"/>.</summary>
    public decimal? Longitude { get; init; }
}
