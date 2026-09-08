using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Services;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

public class FacilityServiceTests
{
    private readonly Mock<IFacilityRepository> _facilityRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly FacilityService _sut;

    public FacilityServiceTests()
    {
        _sut = new FacilityService(_facilityRepository.Object, _unitOfWork.Object);
    }

    private static CreateFacilityRequest ValidRequest() => new()
    {
        FacilityName = "City General Hospital",
        Category = FacilityCategory.Hospital,
        SubCategory = FacilitySubCategory.Government,
        LicenseNumber = "KL-HOSP-000000",
        Address = "123 Main St, Kochi",
        Contacts =
        [
            new CreateFacilityContactRequest { Name = "Jane Doe", Designation = "Administrator", Mobile = "9876543210" }
        ]
    };

    [Fact]
    public async Task RegisterAsync_WhenLicenseNumberIsUnused_PersistsAndReturnsDto()
    {
        _facilityRepository
            .Setup(r => r.GetByLicenseNumberAsync("KL-HOSP-000000", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Facility?)null);

        var result = await _sut.RegisterAsync(ValidRequest(), CancellationToken.None);

        result.FacilityName.Should().Be("City General Hospital");
        result.Category.Should().Be(FacilityCategory.Hospital);
        result.SubCategory.Should().Be(FacilitySubCategory.Government);
        result.VerificationStatus.Should().Be(FacilityVerificationStatus.Pending);
        result.Contacts.Should().ContainSingle(c => c.Mobile == "9876543210");
        _facilityRepository.Verify(r => r.AddAsync(It.IsAny<Facility>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenLicenseNumberAlreadyRegistered_ThrowsFacilityAlreadyRegisteredException()
    {
        var existing = new Facility
        {
            FacilityName = "Existing Hospital",
            Category = FacilityCategory.Hospital,
            SubCategory = FacilitySubCategory.Private,
            LicenseNumber = "KL-HOSP-000000",
            Address = "Somewhere",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        _facilityRepository
            .Setup(r => r.GetByLicenseNumberAsync("KL-HOSP-000000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var act = () => _sut.RegisterAsync(ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<FacilityAlreadyRegisteredException>();
        _facilityRepository.Verify(r => r.AddAsync(It.IsAny<Facility>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetMyFacilityAsync_WhenMobileNumberMatchesAContact_ReturnsDto()
    {
        var facility = new Facility
        {
            FacilityName = "City General Hospital",
            Category = FacilityCategory.Hospital,
            SubCategory = FacilitySubCategory.Government,
            LicenseNumber = "KL-HOSP-000000",
            Address = "123 Main St, Kochi",
            VerificationStatus = FacilityVerificationStatus.Pending,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            Contacts =
            [
                new FacilityContact { Name = "Jane Doe", Designation = "Administrator", Mobile = "9876543210", SortOrder = 0 }
            ]
        };
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync("9876543210", It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        var result = await _sut.GetMyFacilityAsync("9876543210", CancellationToken.None);

        result.Should().NotBeNull();
        result!.FacilityName.Should().Be("City General Hospital");
        result.VerificationStatus.Should().Be(FacilityVerificationStatus.Pending);
    }

    [Fact]
    public async Task GetMyFacilityAsync_WhenNoFacilityMatches_ReturnsNull()
    {
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync("9876543210", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Facility?)null);

        var result = await _sut.GetMyFacilityAsync("9876543210", CancellationToken.None);

        result.Should().BeNull();
    }
}
