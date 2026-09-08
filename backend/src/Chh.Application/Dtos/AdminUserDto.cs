using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>
/// Response item for the CHH-76/US-CHH-001-04 admin user search and suspend endpoints. Never the
/// raw <c>IndividualProfile</c> entity (db-standards.md §2b) — deliberately excludes health-screening
/// PII, mirroring <c>IndividualProfileDto</c>.
/// </summary>
public record AdminUserDto
{
    /// <summary>Surrogate primary key.</summary>
    public required Guid Id { get; init; }

    /// <summary>Full mobile number — shown to the admin so they can positively identify the account (UI Notes: search by mobile number or name).</summary>
    public required string MobileNumber { get; init; }

    /// <summary>Full name.</summary>
    public required string FullName { get; init; }

    /// <summary>Blood group.</summary>
    public required BloodGroup BloodGroup { get; init; }

    /// <summary>Current account standing.</summary>
    public required AccountStatus AccountStatus { get; init; }

    /// <summary>Admin's reason for suspension; null unless <see cref="AccountStatus"/> is "Suspended".</summary>
    public string? SuspensionReason { get; init; }

    /// <summary>UTC timestamp the profile was created.</summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }
}
