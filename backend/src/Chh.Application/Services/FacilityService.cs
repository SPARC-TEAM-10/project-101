using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Factories;

namespace Chh.Application.Services;

/// <summary>Orchestrates facility registration creation (CHH-78/US-CHH-003-01).</summary>
public class FacilityService : IFacilityService
{
    private readonly IFacilityRepository _facilityRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Creates the service with its repository and unit-of-work dependencies.</summary>
    /// <param name="facilityRepository">Data layer for persisting facilities.</param>
    /// <param name="unitOfWork">Persists changes made during the request.</param>
    public FacilityService(IFacilityRepository facilityRepository, IUnitOfWork unitOfWork)
    {
        _facilityRepository = facilityRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<FacilityDto> CreateAsync(string createdByMobileNumber, CreateFacilityRequest request, CancellationToken ct)
    {
        var createdAtUtc = DateTimeOffset.UtcNow;
        var facility = FacilityFactory.Create(createdByMobileNumber, request, createdAtUtc);

        await _facilityRepository.AddAsync(facility, ct).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return new FacilityDto
        {
            Id = facility.Id,
            FacilityName = facility.FacilityName,
            Category = facility.Category,
            LicenseNumber = facility.LicenseNumber,
            Address = facility.Address,
            Contacts = facility.Contacts
                .OrderBy(c => c.SortOrder)
                .Select(c => new FacilityContactDto
                {
                    Name = c.Name,
                    Designation = c.Designation,
                    Mobile = c.Mobile
                })
                .ToList(),
            VerificationStatus = facility.VerificationStatus,
            LicenseDocumentUrl = facility.LicenseDocumentUrl,
            CreatedAtUtc = facility.CreatedAtUtc
        };
    }
}
