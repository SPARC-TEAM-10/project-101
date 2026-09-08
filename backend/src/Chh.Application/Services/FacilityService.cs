using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Factories;

namespace Chh.Application.Services;

/// <summary>Orchestrates facility self-registration: uniqueness guard, persistence (CHH-78).</summary>
public class FacilityService : IFacilityService
{
    private readonly IFacilityRepository _facilityRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Creates the service with its repository and unit-of-work dependencies.</summary>
    /// <param name="facilityRepository">Data layer for reading and persisting facilities.</param>
    /// <param name="unitOfWork">Persists changes made during the request.</param>
    public FacilityService(IFacilityRepository facilityRepository, IUnitOfWork unitOfWork)
    {
        _facilityRepository = facilityRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<FacilityDto> RegisterAsync(CreateFacilityRequest request, CancellationToken ct)
    {
        var existingFacility = await _facilityRepository
            .GetByLicenseNumberAsync(request.LicenseNumber.Trim(), ct)
            .ConfigureAwait(false);

        if (existingFacility is not null)
        {
            throw new FacilityAlreadyRegisteredException();
        }

        var facility = FacilityFactory.Create(request, DateTimeOffset.UtcNow);

        await _facilityRepository.AddAsync(facility, ct).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return new FacilityDto
        {
            Id = facility.Id,
            FacilityName = facility.FacilityName,
            Category = facility.Category,
            SubCategory = facility.SubCategory,
            LicenseNumber = facility.LicenseNumber,
            Address = facility.Address,
            Contacts = facility.Contacts.Select(c => new FacilityContactDto
            {
                Name = c.Name,
                Designation = c.Designation,
                Mobile = c.Mobile
            }).ToList(),
            VerificationStatus = facility.VerificationStatus,
            LicenseDocumentUrl = facility.LicenseDocumentUrl,
            CreatedAtUtc = facility.CreatedAtUtc
        };
    }
}
