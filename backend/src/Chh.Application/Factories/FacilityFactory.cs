using Chh.Application.Dtos;
using Chh.Domain.Entities;
using Chh.Domain.Enums;

namespace Chh.Application.Factories;

/// <summary>
/// Constructs <see cref="Facility"/> (with its <see cref="FacilityContact"/> children) from a
/// validated request, setting the initial <see cref="FacilityVerificationStatus.Pending"/> state
/// (AC1) and preserving contact order (AC2) — kept out of the entity, matching
/// <see cref="BloodRequestFactory"/>'s pattern.
/// </summary>
public static class FacilityFactory
{
    /// <summary>Creates a new facility from a validated request.</summary>
    /// <param name="createdByMobileNumber">The authenticated creator's mobile number (from the JWT "sub" claim, never client-supplied).</param>
    /// <param name="request">The validated facility registration details.</param>
    /// <param name="createdAtUtc">UTC timestamp the facility is created.</param>
    public static Facility Create(string createdByMobileNumber, CreateFacilityRequest request, DateTimeOffset createdAtUtc)
    {
        var facility = new Facility
        {
            FacilityName = request.FacilityName.Trim(),
            Category = request.Category,
            LicenseNumber = request.LicenseNumber.Trim(),
            Address = request.Address.Trim(),
            VerificationStatus = FacilityVerificationStatus.Pending,
            CreatedByMobileNumber = createdByMobileNumber,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };

        var sortOrder = 1;
        foreach (var contact in request.Contacts)
        {
            facility.Contacts.Add(new FacilityContact
            {
                FacilityId = facility.Id,
                Name = contact.Name.Trim(),
                Designation = contact.Designation.Trim(),
                Mobile = contact.Mobile.Trim(),
                SortOrder = sortOrder++
            });
        }

        return facility;
    }
}
