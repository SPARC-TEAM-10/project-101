using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>
/// Response body for <c>POST /api/v1/individuals</c>. Deliberately excludes email, DOB, and
/// health-screening detail — PII the registration-confirmation response doesn't need to echo back.
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

    /// <summary>UTC timestamp the profile was created.</summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }
}
