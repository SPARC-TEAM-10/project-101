using Chh.Application.Dtos;
using Chh.Domain.Entities;

namespace Chh.Application.Factories;

/// <summary>
/// Trims free-text fields, derives <see cref="IndividualProfile.IsReceiverOnly"/> from the
/// health-screening flags (PRD §7 CHH-F02 AC2), and constructs <see cref="IndividualProfile"/>
/// instances. Kept out of the entity per code-review guidance established on CHH-9 — entities
/// hold data, not construction logic. Sets the entity's `internal` setters via object
/// initializer (Chh.Domain.csproj grants Chh.Application <c>InternalsVisibleTo</c>).
/// </summary>
public static class IndividualProfileFactory
{
    /// <summary>Creates a new individual profile from a validated request.</summary>
    /// <param name="request">The validated registration request.</param>
    /// <param name="createdAtUtc">UTC timestamp the profile is created.</param>
    public static IndividualProfile Create(CreateIndividualProfileRequest request, DateTimeOffset createdAtUtc)
    {
        var isReceiverOnly = request.IsChronicIllness
            || request.HasRecentSurgery
            || request.IsInfectiousDisease
            || request.IsUnderweight
            || request.IsOtherIllness;

        return new IndividualProfile
        {
            MobileNumber = request.MobileNumber,
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            BloodGroup = request.BloodGroup,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            LocationCityArea = request.LocationCityArea.Trim(),
            IsChronicIllness = request.IsChronicIllness,
            HasRecentSurgery = request.HasRecentSurgery,
            IsInfectiousDisease = request.IsInfectiousDisease,
            IsUnderweight = request.IsUnderweight,
            IsOtherIllness = request.IsOtherIllness,
            OtherIllnessDetails = request.IsOtherIllness ? request.OtherIllnessDetails?.Trim() : null,
            IsReceiverOnly = isReceiverOnly,
            CreatedAtUtc = createdAtUtc
        };
    }

    /// <summary>
    /// Applies a validated profile edit (CHH-F02 profile edit) to an existing, tracked
    /// <paramref name="profile"/> in place — mutates the entity so EF Core persists the change on
    /// <c>SaveChangesAsync</c>, rather than constructing a replacement. Re-derives
    /// <see cref="IndividualProfile.IsReceiverOnly"/> from the updated health-screening flags,
    /// same rule as <see cref="Create"/>.
    /// </summary>
    /// <param name="profile">The tracked profile to update.</param>
    /// <param name="request">The validated update request.</param>
    public static void ApplyUpdate(IndividualProfile profile, UpdateIndividualProfileRequest request)
    {
        profile.LocationCityArea = request.LocationCityArea.Trim();
        profile.IsChronicIllness = request.IsChronicIllness;
        profile.HasRecentSurgery = request.HasRecentSurgery;
        profile.IsInfectiousDisease = request.IsInfectiousDisease;
        profile.IsUnderweight = request.IsUnderweight;
        profile.IsOtherIllness = request.IsOtherIllness;
        profile.OtherIllnessDetails = request.IsOtherIllness ? request.OtherIllnessDetails?.Trim() : null;
        profile.IsReceiverOnly = request.IsChronicIllness
            || request.HasRecentSurgery
            || request.IsInfectiousDisease
            || request.IsUnderweight
            || request.IsOtherIllness;
    }
}
