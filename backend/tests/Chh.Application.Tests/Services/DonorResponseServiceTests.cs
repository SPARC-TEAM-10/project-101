using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Services;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

public class DonorResponseServiceTests
{
    private readonly Mock<IIndividualProfileRepository> _individualProfileRepository = new();
    private readonly Mock<IDonorNotificationRepository> _donorNotificationRepository = new();
    private readonly Mock<IBloodRequestRepository> _bloodRequestRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DonorResponseService _sut;

    private const string MobileNumber = "9876543210";

    public DonorResponseServiceTests()
    {
        _sut = new DonorResponseService(
            _individualProfileRepository.Object,
            _donorNotificationRepository.Object,
            _bloodRequestRepository.Object,
            _unitOfWork.Object);
    }

    private static IndividualProfile MakeProfile() => new()
    {
        MobileNumber = MobileNumber,
        FullName = "Jane Doe",
        Email = "jane@example.com",
        BloodGroup = BloodGroup.OPositive,
        DateOfBirth = new DateOnly(1990, 1, 1),
        Gender = Gender.Female,
        LocationCityArea = "Kochi",
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    private static DonorNotification MakeNotification(
        Guid donorProfileId,
        DonorResponseStatus responseStatus = DonorResponseStatus.Pending) => new()
    {
        BloodRequestId = Guid.NewGuid(),
        DonorProfileId = donorProfileId,
        BloodGroup = BloodGroup.OPositive,
        UnitsRequired = 2,
        Urgency = UrgencyLevel.Emergency,
        DistanceKm = 4.2m,
        AreaLabel = "Kaloor, Kochi",
        ResponseStatus = responseStatus,
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    private static BloodRequest MakeBloodRequest(Guid id) => new()
    {
        Id = id,
        RequesterMobileNumber = "9000000001",
        PatientName = "John Doe",
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

    // --- AcceptAsync ---

    [Fact]
    public async Task AcceptAsync_NoProfile_ReturnsNull()
    {
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IndividualProfile?)null);

        var result = await _sut.AcceptAsync(MobileNumber, Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AcceptAsync_NotificationNotFoundOrNotOwned_ReturnsNull()
    {
        var profile = MakeProfile();
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _donorNotificationRepository
            .Setup(r => r.GetTrackedByIdForDonorAsync(It.IsAny<Guid>(), profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DonorNotification?)null);

        var result = await _sut.AcceptAsync(MobileNumber, Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
        _bloodRequestRepository.Verify(
            r => r.TryAcceptUnitAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AcceptAsync_AlreadyResponded_ThrowsDonorAlreadyRespondedException()
    {
        var profile = MakeProfile();
        var notification = MakeNotification(profile.Id, DonorResponseStatus.Declined);
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _donorNotificationRepository
            .Setup(r => r.GetTrackedByIdForDonorAsync(notification.Id, profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var act = () => _sut.AcceptAsync(MobileNumber, notification.Id, CancellationToken.None);

        await act.Should().ThrowAsync<DonorAlreadyRespondedException>();
    }

    [Fact]
    public async Task AcceptAsync_RequestNoLongerActive_ThrowsBloodRequestNoLongerActiveException()
    {
        var profile = MakeProfile();
        var notification = MakeNotification(profile.Id);
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _donorNotificationRepository
            .Setup(r => r.GetTrackedByIdForDonorAsync(notification.Id, profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);
        _bloodRequestRepository
            .Setup(r => r.TryAcceptUnitAsync(notification.BloodRequestId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = () => _sut.AcceptAsync(MobileNumber, notification.Id, CancellationToken.None);

        await act.Should().ThrowAsync<BloodRequestNoLongerActiveException>();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AcceptAsync_Success_MarksAcceptedAndReturnsRequesterContact()
    {
        var profile = MakeProfile();
        var notification = MakeNotification(profile.Id);
        var bloodRequest = MakeBloodRequest(notification.BloodRequestId);
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _donorNotificationRepository
            .Setup(r => r.GetTrackedByIdForDonorAsync(notification.Id, profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);
        _bloodRequestRepository
            .Setup(r => r.TryAcceptUnitAsync(notification.BloodRequestId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _bloodRequestRepository
            .Setup(r => r.GetByIdAsync(notification.BloodRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bloodRequest);

        var result = await _sut.AcceptAsync(MobileNumber, notification.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ResponseStatus.Should().Be(DonorResponseStatus.Accepted);
        result.RequesterMobileNumber.Should().Be("9000000001");
        result.LocationCityArea.Should().Be("Kaloor, Kochi");
        notification.ResponseStatus.Should().Be(DonorResponseStatus.Accepted);
        notification.RespondedAtUtc.Should().NotBeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- DeclineAsync ---

    [Fact]
    public async Task DeclineAsync_NoProfile_ReturnsNull()
    {
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IndividualProfile?)null);

        var result = await _sut.DeclineAsync(MobileNumber, Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeclineAsync_AlreadyResponded_ThrowsDonorAlreadyRespondedException()
    {
        var profile = MakeProfile();
        var notification = MakeNotification(profile.Id, DonorResponseStatus.Accepted);
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _donorNotificationRepository
            .Setup(r => r.GetTrackedByIdForDonorAsync(notification.Id, profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var act = () => _sut.DeclineAsync(MobileNumber, notification.Id, CancellationToken.None);

        await act.Should().ThrowAsync<DonorAlreadyRespondedException>();
    }

    [Fact]
    public async Task DeclineAsync_Success_MarksDeclinedWithoutCheckingRequestStatus()
    {
        var profile = MakeProfile();
        var notification = MakeNotification(profile.Id);
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _donorNotificationRepository
            .Setup(r => r.GetTrackedByIdForDonorAsync(notification.Id, profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var result = await _sut.DeclineAsync(MobileNumber, notification.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ResponseStatus.Should().Be(DonorResponseStatus.Declined);
        result.RequesterMobileNumber.Should().BeNull();
        notification.ResponseStatus.Should().Be(DonorResponseStatus.Declined);
        _bloodRequestRepository.Verify(
            r => r.TryAcceptUnitAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
