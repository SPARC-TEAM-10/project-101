using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Factories;
using Chh.Domain.Constants;

namespace Chh.Application.Services;

/// <summary>Orchestrates blood request creation (CHH-33/US-CHH-004-01).</summary>
public class BloodRequestService : IBloodRequestService
{
    private readonly IBloodRequestRepository _bloodRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Creates the service with its repository and unit-of-work dependencies.</summary>
    /// <param name="bloodRequestRepository">Data layer for persisting blood requests.</param>
    /// <param name="unitOfWork">Persists changes made during the request.</param>
    public BloodRequestService(IBloodRequestRepository bloodRequestRepository, IUnitOfWork unitOfWork)
    {
        _bloodRequestRepository = bloodRequestRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<BloodRequestDto> CreateAsync(string requesterMobileNumber, CreateBloodRequestRequest request, CancellationToken ct)
    {
        var createdAtUtc = DateTimeOffset.UtcNow;
        var bloodRequest = BloodRequestFactory.Create(requesterMobileNumber, request, createdAtUtc);

        await _bloodRequestRepository.AddAsync(bloodRequest, ct).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return new BloodRequestDto
        {
            Id = bloodRequest.Id,
            PatientName = bloodRequest.PatientName,
            BloodGroup = bloodRequest.BloodGroup,
            UnitsRequired = bloodRequest.UnitsRequired,
            LocationCityArea = bloodRequest.LocationCityArea,
            SearchRadiusKm = bloodRequest.SearchRadiusKm,
            Urgency = bloodRequest.Urgency,
            Status = bloodRequest.Status,
            CreatedAtUtc = bloodRequest.CreatedAtUtc,
            ExpiresAtUtc = bloodRequest.ExpiresAtUtc
        };
    }

    /// <inheritdoc />
    public async Task<PagedResponse<BloodRequestDto>> GetMyRequestsAsync(string requesterMobileNumber, int page, int pageSize, CancellationToken ct)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1
            ? PaginationConstants.DefaultPageSize
            : Math.Min(pageSize, PaginationConstants.MaxPageSize);

        var (requests, totalCount) = await _bloodRequestRepository
            .GetByRequesterAsync(requesterMobileNumber, normalizedPage, normalizedPageSize, ct)
            .ConfigureAwait(false);

        var items = requests.Select(r => new BloodRequestDto
        {
            Id = r.Id,
            PatientName = r.PatientName,
            BloodGroup = r.BloodGroup,
            UnitsRequired = r.UnitsRequired,
            LocationCityArea = r.LocationCityArea,
            SearchRadiusKm = r.SearchRadiusKm,
            Urgency = r.Urgency,
            Status = r.Status,
            CreatedAtUtc = r.CreatedAtUtc,
            ExpiresAtUtc = r.ExpiresAtUtc
        }).ToList();

        return new PagedResponse<BloodRequestDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = normalizedPage,
            PageSize = normalizedPageSize
        };
    }
}
