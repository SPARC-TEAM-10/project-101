using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Factories;
using Chh.Domain.Constants;
using Chh.Domain.Utilities;

namespace Chh.Application.Services;

/// <summary>
/// Orchestrates facility self-registration: uniqueness guard, persistence (CHH-78), and license
/// document upload (CHH-79).
/// </summary>
public class FacilityService : IFacilityService
{
    private readonly IFacilityRepository _facilityRepository;
    private readonly IFacilityDocumentStorageService _documentStorageService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Creates the service with its repository, storage, and unit-of-work dependencies.</summary>
    /// <param name="facilityRepository">Data layer for reading and persisting facilities.</param>
    /// <param name="documentStorageService">Stores the uploaded license document (CHH-79).</param>
    /// <param name="unitOfWork">Persists changes made during the request.</param>
    public FacilityService(
        IFacilityRepository facilityRepository,
        IFacilityDocumentStorageService documentStorageService,
        IUnitOfWork unitOfWork)
    {
        _facilityRepository = facilityRepository;
        _documentStorageService = documentStorageService;
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

        return ToDto(facility);
    }

    /// <inheritdoc />
    public async Task<FacilityDto?> GetMyFacilityAsync(string mobileNumber, CancellationToken ct)
    {
        var facility = await _facilityRepository
            .GetByContactMobileNumberAsync(mobileNumber, ct)
            .ConfigureAwait(false);

        return facility is null ? null : ToDto(facility);
    }

    /// <inheritdoc />
    public async Task<FacilityDto?> UploadLicenseDocumentAsync(
        Guid facilityId, Stream content, string fileName, string contentType, long contentLength, CancellationToken ct)
    {
        var facility = await _facilityRepository.GetTrackedByIdAsync(facilityId, ct).ConfigureAwait(false);
        if (facility is null)
        {
            return null;
        }

        // Never trust the client's own validation (facilityUploadValidation.ts) alone —
        // api-standards.md §5.
        if (contentLength <= 0)
        {
            throw new InvalidFacilityDocumentException(FacilityDocumentConstants.NoFileProvidedMessage);
        }

        if (!FacilityDocumentConstants.AllowedContentTypes.Contains(contentType))
        {
            throw new InvalidFacilityDocumentException(FacilityDocumentConstants.InvalidFileTypeMessage);
        }

        if (contentLength > FacilityDocumentConstants.MaxFileSizeBytes)
        {
            throw new InvalidFacilityDocumentException(FacilityDocumentConstants.FileTooLargeMessage);
        }

        var url = await _documentStorageService
            .SaveAsync(facilityId, fileName, content, contentType, ct)
            .ConfigureAwait(false);

        facility.LicenseDocumentUrl = url;
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return ToDto(facility);
    }

    /// <inheritdoc />
    public async Task<PagedResponse<PublicFacilityDto>> SearchAsync(SearchFacilitiesRequest request, CancellationToken ct)
    {
        var facilities = await _facilityRepository.SearchAsync(request, ct).ConfigureAwait(false);

        IReadOnlyList<Domain.Entities.Facility> ordered = facilities;
        if (request.Latitude is { } callerLatitude && request.Longitude is { } callerLongitude)
        {
            ordered = facilities
                .OrderBy(f => f.Latitude is null || f.Longitude is null)
                .ThenBy(f => f.Latitude is null || f.Longitude is null
                    ? (decimal?)null
                    : HaversineDistanceCalculator.CalculateDistanceKm(callerLatitude, callerLongitude, f.Latitude!.Value, f.Longitude!.Value))
                .ToList();
        }

        var totalCount = ordered.Count;
        var page = ordered
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(f => ToPublicDto(f, request.Latitude, request.Longitude))
            .ToList();

        return new PagedResponse<PublicFacilityDto>
        {
            Items = page,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<PublicFacilityDto?> GetPublicDetailAsync(Guid id, CancellationToken ct)
    {
        var facility = await _facilityRepository.GetVerifiedByIdAsync(id, ct).ConfigureAwait(false);
        return facility is null ? null : ToPublicDto(facility, latitude: null, longitude: null);
    }

    private static PublicFacilityDto ToPublicDto(Domain.Entities.Facility facility, decimal? latitude, decimal? longitude) => new()
    {
        Id = facility.Id,
        FacilityName = facility.FacilityName,
        Category = facility.Category,
        SubCategory = facility.SubCategory,
        Address = facility.Address,
        Latitude = facility.Latitude,
        Longitude = facility.Longitude,
        Contacts = facility.Contacts
            .OrderBy(c => c.SortOrder)
            .Select(c => new FacilityContactDto
            {
                Name = c.Name,
                Designation = c.Designation,
                Mobile = c.Mobile
            })
            .ToList(),
        DistanceKm = latitude is not null && longitude is not null && facility.Latitude is not null && facility.Longitude is not null
            ? HaversineDistanceCalculator.CalculateDistanceKm(latitude.Value, longitude.Value, facility.Latitude.Value, facility.Longitude.Value)
            : null
    };

    private static FacilityDto ToDto(Domain.Entities.Facility facility) => new()
    {
        Id = facility.Id,
        FacilityName = facility.FacilityName,
        Category = facility.Category,
        SubCategory = facility.SubCategory,
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
        RejectionReason = facility.RejectionReason,
        CreatedAtUtc = facility.CreatedAtUtc,
        UpdatedAtUtc = facility.UpdatedAtUtc
    };
}
