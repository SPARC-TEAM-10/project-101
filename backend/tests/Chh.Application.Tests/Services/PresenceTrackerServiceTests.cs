using Chh.Application.Contracts;
using Chh.Application.Services;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

public class PresenceTrackerServiceTests
{
    private readonly Mock<IIndividualProfileRepository> _individualProfileRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly PresenceTrackerService _sut;

    private const string MobileNumber = "9876543210";

    public PresenceTrackerServiceTests()
    {
        _sut = new PresenceTrackerService(_individualProfileRepository.Object, _unitOfWork.Object);
    }

    private static IndividualProfile MakeProfile(DateTimeOffset? lastActiveAtUtc) => new()
    {
        MobileNumber = MobileNumber,
        FullName = "Jane Doe",
        Email = "jane@example.com",
        BloodGroup = BloodGroup.OPositive,
        DateOfBirth = new DateOnly(1990, 1, 1),
        Gender = Gender.Female,
        LocationCityArea = "Kochi",
        CreatedAtUtc = DateTimeOffset.UtcNow,
        LastActiveAtUtc = lastActiveAtUtc
    };

    [Fact]
    public async Task TrackActivityAsync_NoProfile_DoesNotSave()
    {
        _individualProfileRepository
            .Setup(r => r.GetTrackedByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IndividualProfile?)null);

        await _sut.TrackActivityAsync(MobileNumber, CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TrackActivityAsync_NeverActiveBefore_SetsLastActiveAndSaves()
    {
        var profile = MakeProfile(null);
        _individualProfileRepository
            .Setup(r => r.GetTrackedByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        await _sut.TrackActivityAsync(MobileNumber, CancellationToken.None);

        profile.LastActiveAtUtc.Should().NotBeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TrackActivityAsync_RecentlyActive_DoesNotWriteAgain()
    {
        var recentlyActive = DateTimeOffset.UtcNow.AddSeconds(-5);
        var profile = MakeProfile(recentlyActive);
        _individualProfileRepository
            .Setup(r => r.GetTrackedByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        await _sut.TrackActivityAsync(MobileNumber, CancellationToken.None);

        profile.LastActiveAtUtc.Should().Be(recentlyActive);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TrackActivityAsync_StaleActivity_WritesAgain()
    {
        var staleActive = DateTimeOffset.UtcNow.AddMinutes(-5);
        var profile = MakeProfile(staleActive);
        _individualProfileRepository
            .Setup(r => r.GetTrackedByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        await _sut.TrackActivityAsync(MobileNumber, CancellationToken.None);

        profile.LastActiveAtUtc.Should().NotBe(staleActive);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
