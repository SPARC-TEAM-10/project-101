using Chh.Application.Contracts;
using Chh.Application.Services;
using Chh.Domain.Constants;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

public class UserAdminServiceTests
{
    private readonly Mock<IIndividualProfileRepository> _individualProfileRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UserAdminService _sut;

    public UserAdminServiceTests()
    {
        _sut = new UserAdminService(_individualProfileRepository.Object, _unitOfWork.Object);
    }

    private static IndividualProfile CreateProfile(string fullName, string mobileNumber) => new()
    {
        Id = Guid.NewGuid(),
        MobileNumber = mobileNumber,
        FullName = fullName,
        BloodGroup = BloodGroup.OPositive,
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task SearchUsersAsync_MapsToDtoAndPreservesPaging()
    {
        var profile = CreateProfile("Ananya Nair", "9876543210");
        _individualProfileRepository
            .Setup(r => r.SearchAsync("ananya", 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { profile }, 1));

        var result = await _sut.SearchUsersAsync("ananya", 1, 20, CancellationToken.None);

        result.Items.Should().ContainSingle(u => u.FullName == "Ananya Nair" && u.MobileNumber == "9876543210");
        result.TotalCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0, 20, 1, 20)]
    [InlineData(1, 0, 1, PaginationConstants.DefaultPageSize)]
    [InlineData(1, 500, 1, PaginationConstants.MaxPageSize)]
    public async Task SearchUsersAsync_NormalizesOutOfRangePagingInputs(
        int requestedPage, int requestedPageSize, int expectedPage, int expectedPageSize)
    {
        _individualProfileRepository
            .Setup(r => r.SearchAsync(null, expectedPage, expectedPageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<IndividualProfile>(), 0));

        var result = await _sut.SearchUsersAsync(null, requestedPage, requestedPageSize, CancellationToken.None);

        result.Page.Should().Be(expectedPage);
        result.PageSize.Should().Be(expectedPageSize);
    }

    [Fact]
    public async Task SuspendUserAsync_WhenProfileExists_SetsSuspendedAndReason()
    {
        var profile = CreateProfile("Ravi Kumar", "9123456789");
        _individualProfileRepository
            .Setup(r => r.GetTrackedByIdAsync(profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _sut.SuspendUserAsync(profile.Id, "Repeated no-shows after accepting requests", CancellationToken.None);

        result.Should().NotBeNull();
        result!.AccountStatus.Should().Be(AccountStatus.Suspended);
        result.SuspensionReason.Should().Be("Repeated no-shows after accepting requests");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SuspendUserAsync_WhenProfileDoesNotExist_ReturnsNull()
    {
        _individualProfileRepository
            .Setup(r => r.GetTrackedByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IndividualProfile?)null);

        var result = await _sut.SuspendUserAsync(Guid.NewGuid(), "reason", CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
