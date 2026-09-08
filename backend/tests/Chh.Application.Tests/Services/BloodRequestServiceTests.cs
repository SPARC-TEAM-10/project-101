using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Jobs;
using Chh.Application.Services;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

public class BloodRequestServiceTests
{
    private readonly Mock<IBloodRequestRepository> _bloodRequestRepository = new();
    private readonly Mock<IDonorNotificationRepository> _donorNotificationRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IBackgroundJobClient> _backgroundJobClient = new();
    private readonly BloodRequestService _sut;

    private const string RequesterMobileNumber = "9876543210";

    public BloodRequestServiceTests()
    {
        _sut = new BloodRequestService(
            _bloodRequestRepository.Object,
            _donorNotificationRepository.Object,
            _unitOfWork.Object,
            _backgroundJobClient.Object);
    }

    private static CreateBloodRequestRequest ValidRequest() => new()
    {
        RequesterName = "Jane Requester",
        PatientName = "John Doe",
        BloodGroup = BloodGroup.OPositive,
        UnitsRequired = 2,
        LocationCityArea = "Kochi",
        Latitude = 9.9312m,
        Longitude = 76.2673m,
        SearchRadiusKm = 10,
        Urgency = UrgencyLevel.Emergency
    };

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_PersistsAndReturnsMatchingStatus()
    {
        var response = await _sut.CreateAsync(RequesterMobileNumber, ValidRequest(), CancellationToken.None);

        response.Status.Should().Be(BloodRequestStatus.Matching);
        response.PatientName.Should().Be("John Doe");
        _bloodRequestRepository.Verify(r => r.AddAsync(
            It.Is<BloodRequest>(b => b.RequesterMobileNumber == RequesterMobileNumber),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_SetsExpiryToSixHoursAfterCreation()
    {
        var response = await _sut.CreateAsync(RequesterMobileNumber, ValidRequest(), CancellationToken.None);

        (response.ExpiresAtUtc - response.CreatedAtUtc).Should().Be(TimeSpan.FromHours(6));
    }

    [Fact]
    public async Task CreateAsync_DoesNotTrustClientForRequesterMobileNumber()
    {
        BloodRequest? captured = null;
        _bloodRequestRepository
            .Setup(r => r.AddAsync(It.IsAny<BloodRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BloodRequest, CancellationToken>((b, _) => captured = b)
            .Returns(Task.CompletedTask);

        await _sut.CreateAsync(RequesterMobileNumber, ValidRequest(), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.RequesterMobileNumber.Should().Be(RequesterMobileNumber);
    }

    [Fact]
    public async Task CreateAsync_EnqueuesMatchDonorsJobExactlyOnce()
    {
        await _sut.CreateAsync(RequesterMobileNumber, ValidRequest(), CancellationToken.None);

        // IBackgroundJobClient.Enqueue<T> is a static extension over Create(Job, IState) — Hangfire's
        // own recommended way to verify an enqueue happened without a real job storage backend.
        _backgroundJobClient.Verify(
            c => c.Create(
                It.Is<Job>(job => job.Type == typeof(MatchDonorsJob) && job.Method.Name == nameof(MatchDonorsJob.RunAsync)),
                It.IsAny<EnqueuedState>()),
            Times.Once);
    }

    [Fact]
    public async Task GetMyRequestsAsync_ReturnsPagedResultForRequester()
    {
        var created = DateTimeOffset.UtcNow;
        var requests = new List<BloodRequest>
        {
            new()
            {
                RequesterMobileNumber = RequesterMobileNumber,
                PatientName = "Jane Doe",
                BloodGroup = BloodGroup.OPositive,
                UnitsRequired = 1,
                LocationCityArea = "Kochi",
                Latitude = 9.9312m,
                Longitude = 76.2673m,
                SearchRadiusKm = 10,
                Urgency = UrgencyLevel.Emergency,
                Status = BloodRequestStatus.Matching,
                CreatedAtUtc = created,
                ExpiresAtUtc = created.AddHours(6)
            }
        };
        _bloodRequestRepository
            .Setup(r => r.GetByRequesterAsync(RequesterMobileNumber, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((requests, 1));

        var result = await _sut.GetMyRequestsAsync(RequesterMobileNumber, 1, 20, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Items[0].PatientName.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task GetMyRequestsAsync_NoRequests_ReturnsEmptyPageWithoutThrowing()
    {
        _bloodRequestRepository
            .Setup(r => r.GetByRequesterAsync(RequesterMobileNumber, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<BloodRequest>(), 0));

        var result = await _sut.GetMyRequestsAsync(RequesterMobileNumber, 1, 20, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    // --- GetMatchStatusAsync / UpdateRadiusAsync (CHH-36) ---

    private static BloodRequest MakeBloodRequest(
        int unitsRequired = 2,
        int unitsAccepted = 0,
        int searchRadiusKm = 10,
        BloodRequestStatus status = BloodRequestStatus.Matching,
        DateTimeOffset? expiresAtUtc = null) => new()
    {
        RequesterMobileNumber = RequesterMobileNumber,
        PatientName = "Jane Doe",
        BloodGroup = BloodGroup.OPositive,
        UnitsRequired = unitsRequired,
        UnitsAccepted = unitsAccepted,
        LocationCityArea = "Kochi",
        Latitude = 9.9312m,
        Longitude = 76.2673m,
        SearchRadiusKm = searchRadiusKm,
        Urgency = UrgencyLevel.Emergency,
        Status = status,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        ExpiresAtUtc = expiresAtUtc ?? DateTimeOffset.UtcNow.AddHours(6)
    };

    private static DonorNotificationWithDonorInfo MakeMatch(
        DonorResponseStatus responseStatus = DonorResponseStatus.Pending,
        bool isRead = false,
        string donorFullName = "Some Donor",
        string donorMobileNumber = "9000000000") => new()
    {
        DonorProfileId = Guid.NewGuid(),
        ResponseStatus = responseStatus,
        IsRead = isRead,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        DonorFullName = donorFullName,
        DonorMobileNumber = donorMobileNumber
    };

    [Fact]
    public async Task GetMatchStatusAsync_NotFound_ReturnsNull()
    {
        _bloodRequestRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BloodRequest?)null);

        var result = await _sut.GetMatchStatusAsync(RequesterMobileNumber, Guid.NewGuid(), isGuest: false, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMatchStatusAsync_NotOwnedByCaller_ReturnsNull()
    {
        var request = MakeBloodRequest();
        _bloodRequestRepository
            .Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var result = await _sut.GetMatchStatusAsync("9999999999", request.Id, isGuest: false, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMatchStatusAsync_RegisteredRequester_AnonymizesPendingDonorsInStableOrder()
    {
        var request = MakeBloodRequest();
        var matches = new List<DonorNotificationWithDonorInfo> { MakeMatch(), MakeMatch() };
        _bloodRequestRepository
            .Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        _donorNotificationRepository
            .Setup(r => r.GetWithDonorInfoByBloodRequestIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(matches);

        var result = await _sut.GetMatchStatusAsync(RequesterMobileNumber, request.Id, isGuest: false, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Donors.Should().HaveCount(2);
        result.Donors[0].Label.Should().Be("Donor 1");
        result.Donors[1].Label.Should().Be("Donor 2");
        result.Donors.Should().OnlyContain(d => d.MobileNumber == null && !d.IsAccepted);
        result.NotifiedCount.Should().Be(2);
    }

    [Fact]
    public async Task GetMatchStatusAsync_RegisteredRequester_RevealsAcceptedDonorIdentity()
    {
        var request = MakeBloodRequest();
        var matches = new List<DonorNotificationWithDonorInfo>
        {
            MakeMatch(),
            MakeMatch(DonorResponseStatus.Accepted, donorFullName: "Ravi Kumar", donorMobileNumber: "9123456789")
        };
        _bloodRequestRepository
            .Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        _donorNotificationRepository
            .Setup(r => r.GetWithDonorInfoByBloodRequestIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(matches);

        var result = await _sut.GetMatchStatusAsync(RequesterMobileNumber, request.Id, isGuest: false, CancellationToken.None);

        result!.Donors.Should().HaveCount(2);
        var accepted = result.Donors.Single(d => d.IsAccepted);
        accepted.Label.Should().Be("Ravi Kumar");
        accepted.MobileNumber.Should().Be("9123456789");
        result.AcceptedCount.Should().Be(1);
    }

    [Fact]
    public async Task GetMatchStatusAsync_GuestRequester_OnlyListsAcceptedDonors()
    {
        var request = MakeBloodRequest();
        var matches = new List<DonorNotificationWithDonorInfo>
        {
            MakeMatch(),
            MakeMatch(DonorResponseStatus.Declined),
            MakeMatch(DonorResponseStatus.Accepted, donorFullName: "Ravi Kumar", donorMobileNumber: "9123456789")
        };
        _bloodRequestRepository
            .Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        _donorNotificationRepository
            .Setup(r => r.GetWithDonorInfoByBloodRequestIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(matches);

        var result = await _sut.GetMatchStatusAsync(RequesterMobileNumber, request.Id, isGuest: true, CancellationToken.None);

        result!.Donors.Should().ContainSingle();
        result.Donors[0].Label.Should().Be("Ravi Kumar");
        result.Donors[0].MobileNumber.Should().Be("9123456789");
        // Aggregate counts still reflect everyone notified, even though the guest's list is narrowed.
        result.NotifiedCount.Should().Be(3);
    }

    [Fact]
    public async Task GetMatchStatusAsync_NoMatches_ReturnsZeroNotifiedWithStillMatchingStatus()
    {
        var request = MakeBloodRequest();
        _bloodRequestRepository
            .Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        _donorNotificationRepository
            .Setup(r => r.GetWithDonorInfoByBloodRequestIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DonorNotificationWithDonorInfo>());

        var result = await _sut.GetMatchStatusAsync(RequesterMobileNumber, request.Id, isGuest: false, CancellationToken.None);

        result!.NotifiedCount.Should().Be(0);
        result.Status.Should().Be(BloodRequestStatus.Matching);
        result.Donors.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateRadiusAsync_NotFound_ReturnsNull()
    {
        _bloodRequestRepository
            .Setup(r => r.GetTrackedByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BloodRequest?)null);

        var result = await _sut.UpdateRadiusAsync(RequesterMobileNumber, Guid.NewGuid(), 20, isGuest: false, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateRadiusAsync_NotOwnedByCaller_ReturnsNull()
    {
        var request = MakeBloodRequest();
        _bloodRequestRepository
            .Setup(r => r.GetTrackedByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var result = await _sut.UpdateRadiusAsync("9999999999", request.Id, 20, isGuest: false, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateRadiusAsync_RequestNotMatching_ThrowsBloodRequestNotMatchingException()
    {
        var request = MakeBloodRequest(status: BloodRequestStatus.Fulfilled);
        _bloodRequestRepository
            .Setup(r => r.GetTrackedByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var act = () => _sut.UpdateRadiusAsync(RequesterMobileNumber, request.Id, 50, isGuest: false, CancellationToken.None);

        await act.Should().ThrowAsync<Chh.Application.Abstractions.BloodRequestNotMatchingException>();
    }

    [Fact]
    public async Task UpdateRadiusAsync_RadiusNotLarger_ThrowsRadiusMustIncreaseException()
    {
        var request = MakeBloodRequest(searchRadiusKm: 20);
        _bloodRequestRepository
            .Setup(r => r.GetTrackedByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var act = () => _sut.UpdateRadiusAsync(RequesterMobileNumber, request.Id, 20, isGuest: false, CancellationToken.None);

        await act.Should().ThrowAsync<Chh.Application.Abstractions.RadiusMustIncreaseException>();
    }

    [Fact]
    public async Task UpdateRadiusAsync_Success_UpdatesRadiusAndReEnqueuesMatching()
    {
        var request = MakeBloodRequest(searchRadiusKm: 10);
        _bloodRequestRepository
            .Setup(r => r.GetTrackedByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        _donorNotificationRepository
            .Setup(r => r.GetWithDonorInfoByBloodRequestIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DonorNotificationWithDonorInfo>());

        var result = await _sut.UpdateRadiusAsync(RequesterMobileNumber, request.Id, 50, isGuest: false, CancellationToken.None);

        result!.SearchRadiusKm.Should().Be(50);
        request.SearchRadiusKm.Should().Be(50);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _backgroundJobClient.Verify(
            c => c.Create(
                It.Is<Job>(job => job.Type == typeof(MatchDonorsJob) && job.Method.Name == nameof(MatchDonorsJob.RunAsync)),
                It.IsAny<EnqueuedState>()),
            Times.Once);
    }
}
