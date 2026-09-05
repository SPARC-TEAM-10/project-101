using Chh.Domain.Enums;

namespace Chh.Domain.Entities;

/// <summary>
/// An individual user's registration profile (CHH-F02), completed after OTP verification
/// (CHH-9). Date of birth and health-screening flags are PII/health data encrypted at rest via
/// an EF Core value converter — see `.claude/rules/db-standards.md` §3 and
/// <c>Chh.Infrastructure.Persistence.Configurations.IndividualProfileConfiguration</c>.
/// </summary>
public class IndividualProfile
{
    /// <summary>Surrogate primary key.</summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>The OTP-verified mobile number this profile belongs to (CHH-9).</summary>
    public string MobileNumber { get; private set; } = default!;

    /// <summary>Full name, trimmed, 2–50 characters.</summary>
    public string FullName { get; private set; } = default!;

    /// <summary>Email address.</summary>
    public string Email { get; private set; } = default!;

    /// <summary>Blood group.</summary>
    public BloodGroup BloodGroup { get; private set; }

    /// <summary>Date of birth. Encrypted at rest (PII).</summary>
    public DateOnly DateOfBirth { get; private set; }

    /// <summary>Gender.</summary>
    public Gender Gender { get; private set; }

    /// <summary>City/area — free text for now (see CHH-F02 doc's Open Questions on a predefined location list).</summary>
    public string LocationCityArea { get; private set; } = default!;

    /// <summary>Self-reported chronic illness. Encrypted at rest (health data).</summary>
    public bool IsChronicIllness { get; private set; }

    /// <summary>Self-reported recent surgery. Encrypted at rest (health data).</summary>
    public bool HasRecentSurgery { get; private set; }

    /// <summary>Self-reported infectious disease. Encrypted at rest (health data).</summary>
    public bool IsInfectiousDisease { get; private set; }

    /// <summary>Self-reported underweight status. Encrypted at rest (health data).</summary>
    public bool IsUnderweight { get; private set; }

    /// <summary>Self-reported "Other" illness flag. Encrypted at rest (health data).</summary>
    public bool IsOtherIllness { get; private set; }

    /// <summary>Free-text detail when <see cref="IsOtherIllness"/> is set, max 200 chars. Encrypted at rest (health data).</summary>
    public string? OtherIllnessDetails { get; private set; }

    /// <summary>
    /// True if any health-restriction flag is set (PRD §7 CHH-F02 AC2) — excludes the profile
    /// from donor search while still allowing blood requests. Derived at creation time; stored
    /// unencrypted, it's an operational flag rather than raw health detail (db-standards.md §3).
    /// </summary>
    public bool IsReceiverOnly { get; private set; }

    /// <summary>UTC timestamp the profile was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }

    /// <summary>Reserved for EF Core materialization — entities carry no construction logic (see <see cref="Chh.Application.Factories.IndividualProfileFactory"/>).</summary>
    private IndividualProfile()
    {
    }

    /// <summary>Creates an individual profile record from already-validated, already-derived field values.</summary>
    /// <param name="mobileNumber">The OTP-verified mobile number this profile belongs to.</param>
    /// <param name="fullName">Full name, trimmed.</param>
    /// <param name="email">Email address.</param>
    /// <param name="bloodGroup">Blood group.</param>
    /// <param name="dateOfBirth">Date of birth.</param>
    /// <param name="gender">Gender.</param>
    /// <param name="locationCityArea">City/area.</param>
    /// <param name="isChronicIllness">Self-reported chronic illness.</param>
    /// <param name="hasRecentSurgery">Self-reported recent surgery.</param>
    /// <param name="isInfectiousDisease">Self-reported infectious disease.</param>
    /// <param name="isUnderweight">Self-reported underweight status.</param>
    /// <param name="isOtherIllness">Self-reported "Other" illness flag.</param>
    /// <param name="otherIllnessDetails">Free-text detail when <paramref name="isOtherIllness"/> is set.</param>
    /// <param name="isReceiverOnly">Whether any health-restriction flag is set.</param>
    /// <param name="createdAtUtc">UTC timestamp the profile was created.</param>
    public IndividualProfile(
        string mobileNumber,
        string fullName,
        string email,
        BloodGroup bloodGroup,
        DateOnly dateOfBirth,
        Gender gender,
        string locationCityArea,
        bool isChronicIllness,
        bool hasRecentSurgery,
        bool isInfectiousDisease,
        bool isUnderweight,
        bool isOtherIllness,
        string? otherIllnessDetails,
        bool isReceiverOnly,
        DateTimeOffset createdAtUtc)
    {
        MobileNumber = mobileNumber;
        FullName = fullName;
        Email = email;
        BloodGroup = bloodGroup;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        LocationCityArea = locationCityArea;
        IsChronicIllness = isChronicIllness;
        HasRecentSurgery = hasRecentSurgery;
        IsInfectiousDisease = isInfectiousDisease;
        IsUnderweight = isUnderweight;
        IsOtherIllness = isOtherIllness;
        OtherIllnessDetails = otherIllnessDetails;
        IsReceiverOnly = isReceiverOnly;
        CreatedAtUtc = createdAtUtc;
    }
}
