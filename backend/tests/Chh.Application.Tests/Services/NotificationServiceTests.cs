using Chh.Application.Contracts;
using Chh.Application.Services;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<IIndividualProfileRepository> _individualProfileRepository = new();
    private readonly Mock<IDonorNotificationRepository> _donorNotificationRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly NotificationService _sut;

    private const string MobileNumber = "9876543210";

    public NotificationServiceTests()
    {
        _sut = new NotificationService(
            _individualProfileRepository.Object,
            _donorNotificationRepository.Object,
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

    private static DonorNotification MakeNotification(Guid donorProfileId, bool isRead = false) => new()
    {
        BloodRequestId = Guid.NewGuid(),
        DonorProfileId = donorProfileId,
        BloodGroup = BloodGroup.OPositive,
        UnitsRequired = 2,
        Urgency = UrgencyLevel.Emergency,
        DistanceKm = 4.2m,
        AreaLabel = "Kaloor, Kochi",
        IsRead = isRead,
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task GetMyNotificationsAsync_NoProfile_ReturnsEmptyPage()
    {
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IndividualProfile?)null);

        var result = await _sut.GetMyNotificationsAsync(MobileNumber, 1, 20, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        _donorNotificationRepository.Verify(
            r => r.GetByDonorAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMyNotificationsAsync_WithProfile_ReturnsMappedPage()
    {
        var profile = MakeProfile();
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        var notification = MakeNotification(profile.Id);
        _donorNotificationRepository
            .Setup(r => r.GetByDonorAsync(profile.Id, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<DonorNotification> { notification }, 1));

        var result = await _sut.GetMyNotificationsAsync(MobileNumber, 1, 20, CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].AreaLabel.Should().Be("Kaloor, Kochi");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task MarkReadAsync_NoProfile_ReturnsNull()
    {
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IndividualProfile?)null);

        var result = await _sut.MarkReadAsync(MobileNumber, Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task MarkReadAsync_NotificationNotFoundOrNotOwned_ReturnsNull()
    {
        var profile = MakeProfile();
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _donorNotificationRepository
            .Setup(r => r.GetTrackedByIdForDonorAsync(It.IsAny<Guid>(), profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DonorNotification?)null);

        var result = await _sut.MarkReadAsync(MobileNumber, Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkReadAsync_OwnedNotification_MarksReadAndSaves()
    {
        var profile = MakeProfile();
        var notification = MakeNotification(profile.Id);
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _donorNotificationRepository
            .Setup(r => r.GetTrackedByIdForDonorAsync(notification.Id, profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var result = await _sut.MarkReadAsync(MobileNumber, notification.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsRead.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
