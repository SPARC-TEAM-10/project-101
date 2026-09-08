using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Factories;
using Chh.Application.Jobs;
using Chh.Domain.Constants;
using Hangfire;

namespace Chh.Application.Services;

/// <summary>Orchestrates blood request creation (CHH-33/US-CHH-004-01).</summary>
public class BloodRequestService : IBloodRequestService
{
    private readonly IBloodRequestRepository _bloodRequestRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobClient _backgroundJobClient;

    /// <summary>Creates the service with its repository, unit-of-work, and background-job dependencies.</summary>
    /// <param name="bloodRequestRepository">Data layer for persisting blood requests.</param>
    /// <param name="unitOfWork">Persists changes made during the request.</param>
    /// <param name="backgroundJobClient">Enqueues <see cref="MatchDonorsJob"/> after persistence (US-CHH-004-02/CHH-80).</param>
    public BloodRequestService(IBloodRequestRepository bloodRequestRepository, IUnitOfWork unitOfWork, IBackgroundJobClient backgroundJobClient)
    {
        _bloodRequestRepository = bloodRequestRepository;
        _unitOfWork = unitOfWork;
        _backgroundJobClient = backgroundJobClient;
    }

    /// <inheritdoc />
    public async Task<BloodRequestDto> CreateAsync(string requesterMobileNumber, CreateBloodRequestRequest request, CancellationToken ct)
    {
        var createdAtUtc = DateTimeOffset.UtcNow;
        var bloodRequest = BloodRequestFactory.Create(requesterMobileNumber, request, createdAtUtc);

        await _bloodRequestRepository.AddAsync(bloodRequest, ct).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        // Fire-and-forget: matching must not delay this response (api-standards.md §6 NFR).
        // CancellationToken.None — the job runs after this request has already returned.
        _backgroundJobClient.Enqueue<MatchDonorsJob>(job => job.RunAsync(bloodRequest.Id, CancellationToken.None));

        return new BloodRequestDto
        {
            Id = bloodRequest.Id,
            RequesterName = bloodRequest.RequesterName,
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
            RequesterName = r.RequesterName,
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
