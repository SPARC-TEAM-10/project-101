using Chh.Application.Dtos;
using Chh.Domain.Entities;

namespace Chh.Application.Factories;

/// <summary>
/// Trims free-text fields, derives <see cref="IndividualProfile.IsReceiverOnly"/> from the
/// health-screening flags (PRD §7 CHH-F02 AC2), and constructs <see cref="IndividualProfile"/>
/// instances. Kept out of the entity per code-review guidance established on CHH-9 — entities
/// hold data, not construction logic.
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

        return new IndividualProfile(
            request.MobileNumber,
            request.FullName.Trim(),
            request.Email.Trim(),
            request.BloodGroup,
            request.DateOfBirth,
            request.Gender,
            request.LocationCityArea.Trim(),
            request.IsChronicIllness,
            request.HasRecentSurgery,
            request.IsInfectiousDisease,
            request.IsUnderweight,
            request.IsOtherIllness,
            request.IsOtherIllness ? request.OtherIllnessDetails?.Trim() : null,
            isReceiverOnly,
            createdAtUtc);
    }
}
