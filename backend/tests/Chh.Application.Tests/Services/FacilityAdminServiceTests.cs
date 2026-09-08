using Chh.Application.Contracts;
using Chh.Application.Services;
using Chh.Domain.Constants;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

public class FacilityAdminServiceTests
{
    private readonly Mock<IFacilityRepository> _facilityRepository = new();
    private readonly FacilityAdminService _sut;

    public FacilityAdminServiceTests()
    {
        _sut = new FacilityAdminService(_facilityRepository.Object);
    }

    private static Facility CreatePendingFacility(string name) => new()
    {
        Id = Guid.NewGuid(),
        FacilityName = name,
        Category = FacilityCategory.Hospital,
        LicenseNumber = "LIC-123",
        Address = "123 Main St",
        VerificationStatus = FacilityVerificationStatus.Pending,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow,
        Contacts = [new FacilityContact { Name = "Jane", Designation = "Admin", Mobile = "9876543210" }]
    };

    [Fact]
    public async Task GetPendingFacilitiesAsync_WhenFacilitiesExist_MapsToDtoAndPreservesPaging()
    {
        var facility = CreatePendingFacility("City Hospital");
        _facilityRepository
            .Setup(r => r.GetByStatusAsync(FacilityVerificationStatus.Pending, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { facility }, 1));

        var result = await _sut.GetPendingFacilitiesAsync(1, 20, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].FacilityName.Should().Be("City Hospital");
        result.Items[0].Contacts.Should().ContainSingle(c => c.Mobile == "9876543210");
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task GetPendingFacilitiesAsync_WhenNoPendingFacilities_ReturnsEmptyItems()
    {
        _facilityRepository
            .Setup(r => r.GetByStatusAsync(FacilityVerificationStatus.Pending, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<Facility>(), 0));

        var result = await _sut.GetPendingFacilitiesAsync(1, 20, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Theory]
    [InlineData(0, 20, 1, 20)]
    [InlineData(-5, 20, 1, 20)]
    [InlineData(1, 0, 1, PaginationConstants.DefaultPageSize)]
    [InlineData(1, 500, 1, PaginationConstants.MaxPageSize)]
    public async Task GetPendingFacilitiesAsync_NormalizesOutOfRangePagingInputs(
        int requestedPage, int requestedPageSize, int expectedPage, int expectedPageSize)
    {
        _facilityRepository
            .Setup(r => r.GetByStatusAsync(FacilityVerificationStatus.Pending, expectedPage, expectedPageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<Facility>(), 0));

        var result = await _sut.GetPendingFacilitiesAsync(requestedPage, requestedPageSize, CancellationToken.None);

        result.Page.Should().Be(expectedPage);
        result.PageSize.Should().Be(expectedPageSize);
        _facilityRepository.Verify(
            r => r.GetByStatusAsync(FacilityVerificationStatus.Pending, expectedPage, expectedPageSize, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
