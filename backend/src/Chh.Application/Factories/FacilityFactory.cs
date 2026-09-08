using Chh.Application.Dtos;
using Chh.Domain.Entities;
using Chh.Domain.Enums;

namespace Chh.Application.Factories;

/// <summary>
/// Trims free-text fields and constructs <see cref="Facility"/> instances from a validated
/// registration request (CHH-78) — kept out of the entity, matching <see cref="IndividualProfileFactory"/>'s
/// established pattern.
/// </summary>
public static class FacilityFactory
{
    /// <summary>Creates a new facility (with its contacts) from a validated request.</summary>
    /// <param name="request">The validated registration request.</param>
    /// <param name="createdAtUtc">UTC timestamp the facility is created.</param>
    public static Facility Create(CreateFacilityRequest request, DateTimeOffset createdAtUtc) => new()
    {
        FacilityName = request.FacilityName.Trim(),
        Category = request.Category,
        SubCategory = request.SubCategory,
        LicenseNumber = request.LicenseNumber.Trim(),
        Address = request.Address.Trim(),
        VerificationStatus = FacilityVerificationStatus.Pending,
        CreatedAtUtc = createdAtUtc,
        UpdatedAtUtc = createdAtUtc,
        Contacts = request.Contacts.Select(c => new FacilityContact
        {
            Name = c.Name.Trim(),
            Designation = c.Designation.Trim(),
            Mobile = c.Mobile.Trim()
        }).ToList()
    };
}
