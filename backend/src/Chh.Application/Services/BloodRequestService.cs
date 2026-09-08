using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Factories;
using Chh.Application.Jobs;
using Chh.Domain.Constants;
using Chh.Domain.Enums;
using Hangfire;

namespace Chh.Application.Services;

/// <summary>Orchestrates blood request creation (CHH-33/US-CHH-004-01) and match tracking (CHH-36).</summary>
public class BloodRequestService : IBloodRequestService
{
    private readonly IBloodRequestRepository _bloodRequestRepository;
    private readonly IDonorNotificationRepository _donorNotificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobClient _backgroundJobClient;

    /// <summary>Creates the service with its repository, unit-of-work, and background-job dependencies.</summary>
    /// <param name="bloodRequestRepository">Data layer for persisting blood requests.</param>
    /// <param name="donorNotificationRepository">Data layer for the match-status donor list and counts (CHH-36).</param>
    /// <param name="unitOfWork">Persists changes made during the request.</param>
    /// <param name="backgroundJobClient">Enqueues <see cref="MatchDonorsJob"/> after persistence (US-CHH-004-02/CHH-80) and after a radius expansion (CHH-36 AC4).</param>
    public BloodRequestService(
        IBloodRequestRepository bloodRequestRepository,
        IDonorNotificationRepository donorNotificationRepository,
        IUnitOfWork unitOfWork,
        IBackgroundJobClient backgroundJobClient)
    {
        _bloodRequestRepository = bloodRequestRepository;
        _donorNotificationRepository = donorNotificationRepository;
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

    /// <inheritdoc />
    public async Task<BloodRequestMatchStatusDto?> GetMatchStatusAsync(string requesterMobileNumber, Guid bloodRequestId, bool isGuest, CancellationToken ct)
    {
        var bloodRequest = await _bloodRequestRepository.GetByIdAsync(bloodRequestId, ct).ConfigureAwait(false);
        if (bloodRequest is null || bloodRequest.RequesterMobileNumber != requesterMobileNumber)
        {
            return null;
        }

        var matches = await _donorNotificationRepository
            .GetWithDonorInfoByBloodRequestIdAsync(bloodRequestId, ct)
            .ConfigureAwait(false);

        return ToMatchStatusDto(bloodRequest, matches, isGuest);
    }

    /// <inheritdoc />
    public async Task<BloodRequestMatchStatusDto?> UpdateRadiusAsync(string requesterMobileNumber, Guid bloodRequestId, int newRadiusKm, bool isGuest, CancellationToken ct)
    {
        var bloodRequest = await _bloodRequestRepository.GetTrackedByIdAsync(bloodRequestId, ct).ConfigureAwait(false);
        if (bloodRequest is null || bloodRequest.RequesterMobileNumber != requesterMobileNumber)
        {
            return null;
        }

        if (bloodRequest.Status != BloodRequestStatus.Matching)
        {
            throw new BloodRequestNotMatchingException(bloodRequest.Status.ToString());
        }

        if (newRadiusKm <= bloodRequest.SearchRadiusKm)
        {
            throw new RadiusMustIncreaseException();
        }

        bloodRequest.SearchRadiusKm = newRadiusKm;
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        // Fire-and-forget, same as CreateAsync — re-runs matching over the wider radius. CHH-34's
        // AC4 duplicate-prevention means already-notified donors are skipped; only newly-in-range
        // donors get a fresh notification.
        _backgroundJobClient.Enqueue<MatchDonorsJob>(job => job.RunAsync(bloodRequest.Id, CancellationToken.None));

        var matches = await _donorNotificationRepository
            .GetWithDonorInfoByBloodRequestIdAsync(bloodRequestId, ct)
            .ConfigureAwait(false);

        return ToMatchStatusDto(bloodRequest, matches, isGuest);
    }

    // Guest requester: only Accepted donors are listed at all — pending/sent/viewed donors don't
    // appear. Registered requester: every matched donor is listed, anonymized as "Donor N" (stable
    // match order) until accepted. Both views reveal the real name/number only once accepted —
    // this is the explicit design decision recorded in CHH-36's Business Rules.
    private static BloodRequestMatchStatusDto ToMatchStatusDto(
        Chh.Domain.Entities.BloodRequest bloodRequest,
        IReadOnlyList<DonorNotificationWithDonorInfo> matches,
        bool isGuest)
    {
        var donors = new List<DonorStatusDto>();
        for (var i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            var isAccepted = match.ResponseStatus == DonorResponseStatus.Accepted;

            if (isGuest && !isAccepted)
            {
                continue;
            }

            donors.Add(new DonorStatusDto
            {
                Label = isAccepted ? match.DonorFullName : $"Donor {i + 1}",
                MobileNumber = isAccepted ? match.DonorMobileNumber : null,
                IsAccepted = isAccepted
            });
        }

        return new BloodRequestMatchStatusDto
        {
            BloodRequestId = bloodRequest.Id,
            Status = bloodRequest.Status,
            SearchRadiusKm = bloodRequest.SearchRadiusKm,
            UnitsRequired = bloodRequest.UnitsRequired,
            UnitsAccepted = bloodRequest.UnitsAccepted,
            ExpiresAtUtc = bloodRequest.ExpiresAtUtc,
            NotifiedCount = matches.Count,
            ViewedCount = matches.Count(m => m.IsRead),
            AcceptedCount = matches.Count(m => m.ResponseStatus == DonorResponseStatus.Accepted),
            Donors = donors
        };
    }
}
