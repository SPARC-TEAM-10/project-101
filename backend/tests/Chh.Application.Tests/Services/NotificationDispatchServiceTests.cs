using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Services;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

public class NotificationDispatchServiceTests
{
    private readonly Mock<IDonorNotificationRepository> _donorNotificationRepository = new();
    private readonly Mock<ISmsGatewayClient> _smsGatewayClient = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ILogger<NotificationDispatchService>> _logger = new();
    private readonly NotificationDispatchService _sut;

    public NotificationDispatchServiceTests()
    {
        _sut = new NotificationDispatchService(
            _donorNotificationRepository.Object,
            _smsGatewayClient.Object,
            _unitOfWork.Object,
            _logger.Object);
    }

    private static BloodRequest MakeRequest() => new()
    {
        RequesterMobileNumber = "9876543210",
        PatientName = "Jane Doe",
        BloodGroup = BloodGroup.OPositive,
        UnitsRequired = 2,
        LocationCityArea = "Kaloor, Kochi",
        Latitude = 9.9312m,
        Longitude = 76.2673m,
        SearchRadiusKm = 20,
        Urgency = UrgencyLevel.Emergency,
        Status = BloodRequestStatus.Matching,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        ExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(6)
    };

    private static MatchedDonorResult MakeMatch(DateTimeOffset? lastActiveAtUtc) => new()
    {
        DonorProfileId = Guid.NewGuid(),
        MobileNumber = "9000000000",
        DistanceKm = 4.2m,
        LastActiveAtUtc = lastActiveAtUtc
    };

    [Fact]
    public async Task DispatchAsync_DonorAlreadyNotifiedForRequest_SkipsAndDoesNotSendSms()
    {
        var request = MakeRequest();
        var match = MakeMatch(null);
        _donorNotificationRepository
            .Setup(r => r.ExistsAsync(request.Id, match.DonorProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.DispatchAsync(request, [match], CancellationToken.None);

        _donorNotificationRepository.Verify(r => r.AddAsync(It.IsAny<DonorNotification>(), It.IsAny<CancellationToken>()), Times.Never);
        _smsGatewayClient.Verify(s => s.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DispatchAsync_DonorCurrentlyActive_CreatesNotificationWithoutSms()
    {
        var request = MakeRequest();
        var match = MakeMatch(DateTimeOffset.UtcNow.AddSeconds(-30));
        _donorNotificationRepository
            .Setup(r => r.ExistsAsync(request.Id, match.DonorProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        DonorNotification? added = null;
        _donorNotificationRepository
            .Setup(r => r.AddAsync(It.IsAny<DonorNotification>(), It.IsAny<CancellationToken>()))
            .Callback<DonorNotification, CancellationToken>((n, _) => added = n)
            .Returns(Task.CompletedTask);

        await _sut.DispatchAsync(request, [match], CancellationToken.None);

        added.Should().NotBeNull();
        added!.SmsSent.Should().BeFalse();
        added.IsRead.Should().BeFalse();
        _smsGatewayClient.Verify(s => s.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_DonorNotActive_SendsSmsAndMarksSmsSent()
    {
        var request = MakeRequest();
        var match = MakeMatch(DateTimeOffset.UtcNow.AddMinutes(-10));
        _donorNotificationRepository
            .Setup(r => r.ExistsAsync(request.Id, match.DonorProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        DonorNotification? added = null;
        _donorNotificationRepository
            .Setup(r => r.AddAsync(It.IsAny<DonorNotification>(), It.IsAny<CancellationToken>()))
            .Callback<DonorNotification, CancellationToken>((n, _) => added = n)
            .Returns(Task.CompletedTask);

        await _sut.DispatchAsync(request, [match], CancellationToken.None);

        added!.SmsSent.Should().BeTrue();
        _smsGatewayClient.Verify(s => s.SendMessageAsync(match.MobileNumber, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_DonorWithNoLastActive_TreatedAsNotActive_SendsSms()
    {
        var request = MakeRequest();
        var match = MakeMatch(null);
        _donorNotificationRepository
            .Setup(r => r.ExistsAsync(request.Id, match.DonorProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _sut.DispatchAsync(request, [match], CancellationToken.None);

        _smsGatewayClient.Verify(s => s.SendMessageAsync(match.MobileNumber, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_SmsGatewayThrows_StillCreatesNotificationWithoutThrowing()
    {
        var request = MakeRequest();
        var match = MakeMatch(null);
        _donorNotificationRepository
            .Setup(r => r.ExistsAsync(request.Id, match.DonorProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _smsGatewayClient
            .Setup(s => s.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("gateway down"));

        DonorNotification? added = null;
        _donorNotificationRepository
            .Setup(r => r.AddAsync(It.IsAny<DonorNotification>(), It.IsAny<CancellationToken>()))
            .Callback<DonorNotification, CancellationToken>((n, _) => added = n)
            .Returns(Task.CompletedTask);

        var act = () => _sut.DispatchAsync(request, [match], CancellationToken.None);

        await act.Should().NotThrowAsync();
        added.Should().NotBeNull();
        added!.SmsSent.Should().BeFalse();
    }

    [Fact]
    public async Task DispatchAsync_NoMatches_DoesNotSaveChanges()
    {
        var request = MakeRequest();

        await _sut.DispatchAsync(request, [], CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DispatchAsync_CreatedNotification_CopiesRequestFactsNotPatientName()
    {
        var request = MakeRequest();
        var match = MakeMatch(DateTimeOffset.UtcNow);
        _donorNotificationRepository
            .Setup(r => r.ExistsAsync(request.Id, match.DonorProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        DonorNotification? added = null;
        _donorNotificationRepository
            .Setup(r => r.AddAsync(It.IsAny<DonorNotification>(), It.IsAny<CancellationToken>()))
            .Callback<DonorNotification, CancellationToken>((n, _) => added = n)
            .Returns(Task.CompletedTask);

        await _sut.DispatchAsync(request, [match], CancellationToken.None);

        added!.BloodGroup.Should().Be(request.BloodGroup);
        added.UnitsRequired.Should().Be(request.UnitsRequired);
        added.Urgency.Should().Be(request.Urgency);
        added.AreaLabel.Should().Be(request.LocationCityArea);
        added.DistanceKm.Should().Be(match.DistanceKm);
        added.BloodRequestId.Should().Be(request.Id);
        added.DonorProfileId.Should().Be(match.DonorProfileId);
    }
}
