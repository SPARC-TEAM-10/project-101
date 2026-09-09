using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Services;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

/// <summary>Tests for <see cref="FacilityService.SearchAsync"/> and <see cref="FacilityService.GetPublicDetailAsync"/> (CHH-82/Epic CHH-68).</summary>
public class FacilitySearchServiceTests
{
    private readonly Mock<IFacilityRepository> _facilityRepository = new();
    private readonly Mock<IFacilityDocumentStorageService> _documentStorageService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly FacilityService _sut;

    public FacilitySearchServiceTests()
    {
        _sut = new FacilityService(_facilityRepository.Object, _documentStorageService.Object, _unitOfWork.Object);
    }

    private static SearchFacilitiesRequest MakeRequest(
        decimal? latitude = null, decimal? longitude = null, int page = 1, int pageSize = 20) => new()
    {
        Latitude = latitude,
        Longitude = longitude,
        Page = page,
        PageSize = pageSize
    };

    private static Facility MakeFacility(
        string name, decimal? latitude = null, decimal? longitude = null) => new()
    {
        FacilityName = name,
        Category = FacilityCategory.Hospital,
        SubCategory = FacilitySubCategory.Government,
        LicenseNumber = $"LIC-{Guid.NewGuid():N}",
        Address = "1 Test Street",
        Latitude = latitude,
        Longitude = longitude,
        VerificationStatus = FacilityVerificationStatus.Verified,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow,
        Contacts = [new FacilityContact { Name = "Jane Doe", Designation = "Administrator", Mobile = "9876543210", SortOrder = 0 }]
    };

    [Fact]
    public async Task SearchAsync_MapsFacilityToPublicFacilityDto_ExcludingLicenseFields()
    {
        var facility = MakeFacility("City General Hospital");
        _facilityRepository
            .Setup(r => r.SearchAsync(It.IsAny<SearchFacilitiesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([facility]);

        var result = await _sut.SearchAsync(MakeRequest(), CancellationToken.None);

        var dto = result.Items.Should().ContainSingle().Subject;
        dto.FacilityName.Should().Be("City General Hospital");
        dto.Contacts.Should().ContainSingle(c => c.Mobile == "9876543210");
    }

    [Fact]
    public async Task SearchAsync_WhenCallerSuppliesCoordinates_OrdersResultsByAscendingDistance()
    {
        var near = MakeFacility("Near Hospital", latitude: 10.01m, longitude: 76.01m);
        var far = MakeFacility("Far Hospital", latitude: 12.00m, longitude: 78.00m);
        _facilityRepository
            .Setup(r => r.SearchAsync(It.IsAny<SearchFacilitiesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([far, near]);

        var result = await _sut.SearchAsync(MakeRequest(latitude: 10.00m, longitude: 76.00m), CancellationToken.None);

        result.Items.Select(i => i.FacilityName).Should().Equal("Near Hospital", "Far Hospital");
    }

    [Fact]
    public async Task SearchAsync_WhenSomeFacilitiesHaveNoCoordinates_SortsThemLastWithNullDistance()
    {
        var withCoords = MakeFacility("Has Coordinates", latitude: 10.00m, longitude: 76.00m);
        var withoutCoords = MakeFacility("No Coordinates");
        _facilityRepository
            .Setup(r => r.SearchAsync(It.IsAny<SearchFacilitiesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([withoutCoords, withCoords]);

        var result = await _sut.SearchAsync(MakeRequest(latitude: 10.00m, longitude: 76.00m), CancellationToken.None);

        result.Items.Select(i => i.FacilityName).Should().Equal("Has Coordinates", "No Coordinates");
        result.Items.Last().DistanceKm.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_WhenCallerSuppliesNoCoordinates_ReturnsNullDistanceForEveryItem()
    {
        var first = MakeFacility("First", latitude: 10.00m, longitude: 76.00m);
        var second = MakeFacility("Second");
        _facilityRepository
            .Setup(r => r.SearchAsync(It.IsAny<SearchFacilitiesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([first, second]);

        var result = await _sut.SearchAsync(MakeRequest(), CancellationToken.None);

        result.Items.Should().OnlyContain(i => i.DistanceKm == null);
        result.Items.Select(i => i.FacilityName).Should().Equal("First", "Second");
    }

    [Fact]
    public async Task SearchAsync_AppliesPagingOverTheFullFilteredSet()
    {
        var facilities = Enumerable.Range(1, 5).Select(i => MakeFacility($"Facility {i}")).ToList();
        _facilityRepository
            .Setup(r => r.SearchAsync(It.IsAny<SearchFacilitiesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(facilities);

        var result = await _sut.SearchAsync(MakeRequest(page: 2, pageSize: 2), CancellationToken.None);

        result.TotalCount.Should().Be(5);
        result.Items.Select(i => i.FacilityName).Should().Equal("Facility 3", "Facility 4");
    }

    [Fact]
    public async Task GetPublicDetailAsync_WhenFacilityExistsAndVerified_ReturnsDto()
    {
        var facility = MakeFacility("City General Hospital");
        _facilityRepository
            .Setup(r => r.GetVerifiedByIdAsync(facility.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        var result = await _sut.GetPublicDetailAsync(facility.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.FacilityName.Should().Be("City General Hospital");
        result.DistanceKm.Should().BeNull();
    }

    [Fact]
    public async Task GetPublicDetailAsync_WhenNoVerifiedFacilityMatches_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _facilityRepository
            .Setup(r => r.GetVerifiedByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Facility?)null);

        var result = await _sut.GetPublicDetailAsync(id, CancellationToken.None);

        result.Should().BeNull();
    }
}
