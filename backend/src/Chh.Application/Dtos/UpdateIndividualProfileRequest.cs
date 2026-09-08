namespace Chh.Application.Dtos;

/// <summary>
/// Request body for <c>PATCH /api/v1/individuals/me</c> (CHH-F02 profile edit). Deliberately
/// narrower than <see cref="CreateIndividualProfileRequest"/> — name, email, blood group,
/// DOB, and gender are not editable here (no product requirement to change them yet); only
/// location and the health-screening flags are.
/// </summary>
public record UpdateIndividualProfileRequest
{
    /// <summary>City/area — free text.</summary>
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
