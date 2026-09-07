using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Constants;
using Chh.Domain.Enums;

namespace Chh.Application.Services;

/// <summary>Orchestrates System Admin facility moderation (CHH-F07 Admin Command Center).</summary>
public class FacilityAdminService : IFacilityAdminService
{
    private readonly IFacilityRepository _facilityRepository;

    /// <summary>Creates the service with its repository dependency.</summary>
    /// <param name="facilityRepository">Data layer for reading facilities.</param>
    public FacilityAdminService(IFacilityRepository facilityRepository)
    {
        _facilityRepository = facilityRepository;
    }

    /// <inheritdoc />
    public async Task<PagedResponse<FacilityDto>> GetPendingFacilitiesAsync(int page, int pageSize, CancellationToken ct)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1
            ? PaginationConstants.DefaultPageSize
            : Math.Min(pageSize, PaginationConstants.MaxPageSize);

        var (facilities, totalCount) = await _facilityRepository
            .GetByStatusAsync(FacilityVerificationStatus.Pending, normalizedPage, normalizedPageSize, ct)
            .ConfigureAwait(false);

        var items = facilities.Select(f => new FacilityDto
        {
            Id = f.Id,
            FacilityName = f.FacilityName,
            Category = f.Category,
            LicenseNumber = f.LicenseNumber,
            Address = f.Address,
            Contacts = f.Contacts.Select(c => new FacilityContactDto
            {
                Name = c.Name,
                Designation = c.Designation,
                Mobile = c.Mobile
            }).ToList(),
            VerificationStatus = f.VerificationStatus,
            LicenseDocumentUrl = f.LicenseDocumentUrl,
            CreatedAtUtc = f.CreatedAtUtc
        }).ToList();

        return new PagedResponse<FacilityDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = normalizedPage,
            PageSize = normalizedPageSize
        };
    }
}
