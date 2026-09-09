using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Constants;
using Chh.Domain.Entities;
using Chh.Domain.Enums;

namespace Chh.Application.Services;

/// <summary>Orchestrates System Admin facility moderation (CHH-F07 Admin Command Center).</summary>
public class FacilityAdminService : IFacilityAdminService
{
    private readonly IFacilityRepository _facilityRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Creates the service with its repository and unit-of-work dependencies.</summary>
    /// <param name="facilityRepository">Data layer for reading and updating facilities.</param>
    /// <param name="unitOfWork">Persists the review decision (CHH-75).</param>
    public FacilityAdminService(IFacilityRepository facilityRepository, IUnitOfWork unitOfWork)
    {
        _facilityRepository = facilityRepository;
        _unitOfWork = unitOfWork;
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

        return new PagedResponse<FacilityDto>
        {
            Items = facilities.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = normalizedPage,
            PageSize = normalizedPageSize
        };
    }

    /// <inheritdoc />
    public async Task<FacilityDto?> ReviewFacilityAsync(Guid facilityId, FacilityVerificationDecision decision, string? rejectionReason, CancellationToken ct)
    {
        var facility = await _facilityRepository.GetTrackedByIdAsync(facilityId, ct).ConfigureAwait(false);
        if (facility is null)
        {
            return null;
        }

        if (facility.VerificationStatus != FacilityVerificationStatus.Pending)
        {
            throw new FacilityAlreadyReviewedException(facility.VerificationStatus.ToString());
        }

        facility.VerificationStatus = decision == FacilityVerificationDecision.Approve
            ? FacilityVerificationStatus.Verified
            : FacilityVerificationStatus.Rejected;
        // Only a Reject carries a reason (AC2) — an Approve clears any stale reason from a prior
        // rejected-then-resubmitted cycle, though CHH-78 has no resubmission path yet either.
        facility.RejectionReason = decision == FacilityVerificationDecision.Reject ? rejectionReason : null;
        facility.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return MapToDto(facility);
    }

    private static FacilityDto MapToDto(Facility f) => new()
    {
        Id = f.Id,
        FacilityName = f.FacilityName,
        Category = f.Category,
        SubCategory = f.SubCategory,
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
        RejectionReason = f.RejectionReason,
        CreatedAtUtc = f.CreatedAtUtc,
        UpdatedAtUtc = f.UpdatedAtUtc
    };
}
