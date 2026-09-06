using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>Request body for <c>POST /api/v1/individuals</c>.</summary>
public record CreateIndividualProfileRequest
{
    /// <summary>Must have a verified OTP (CHH-9) already. Exactly 10 digits, numeric only.</summary>
    public required string MobileNumber { get; init; }

    /// <summary>Full name. 2–50 characters after trimming.</summary>
    public required string FullName { get; init; }

    /// <summary>Email address.</summary>
    public required string Email { get; init; }

    /// <summary>Blood group.</summary>
    public required BloodGroup BloodGroup { get; init; }

    /// <summary>Date of birth. Must indicate an age of 18 or older.</summary>
    public required DateOnly DateOfBirth { get; init; }

    /// <summary>Gender.</summary>
    public required Gender Gender { get; init; }

    /// <summary>City/area — free text for now (see CHH-F02 doc's Open Questions on a predefined location list).</summary>
    public required string LocationCityArea { get; init; }

    /// <summary>Self-reported chronic illness.</summary>
    public bool IsChronicIllness { get; init; }

    /// <summary>Self-reported recent surgery.</summary>
    public bool HasRecentSurgery { get; init; }

    /// <summary>Self-reported infectious disease.</summary>
    public bool IsInfectiousDisease { get; init; }

    /// <summary>Self-reported underweight status.</summary>
    public bool IsUnderweight { get; init; }

    /// <summary>Self-reported "Other" illness flag.</summary>
    public bool IsOtherIllness { get; init; }

    /// <summary>Required (max 200 chars) when <see cref="IsOtherIllness"/> is true.</summary>
    public string? OtherIllnessDetails { get; init; }
}
