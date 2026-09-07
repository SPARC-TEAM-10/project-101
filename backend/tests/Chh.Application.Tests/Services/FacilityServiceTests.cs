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

    private const string CreatedByMobileNumber = "9876543210";

    public FacilityServiceTests()
    {
        _sut = new FacilityService(_facilityRepository.Object, _unitOfWork.Object);
    }

    private static CreateFacilityRequest ValidRequest() => new()
    {
        FacilityName = "Kochi Metro Hospital",
        Category = FacilityCategory.Hospital,
        LicenseNumber = "KL-HOSP-448120",
        Address = "4th Block, Marine Drive, Ernakulam, Kochi 682031",
        Contacts =
        [
            new CreateFacilityContactRequest { Name = "Anitha Varghese", Designation = "Blood bank officer", Mobile = "9876500112" },
            new CreateFacilityContactRequest { Name = "Rahul Nair", Designation = "Duty manager", Mobile = "9876500240" }
        ]
    };

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_PersistsAndReturnsPendingStatus()
    {
        var response = await _sut.CreateAsync(CreatedByMobileNumber, ValidRequest(), CancellationToken.None);

        response.VerificationStatus.Should().Be(FacilityVerificationStatus.Pending);
        response.FacilityName.Should().Be("Kochi Metro Hospital");
        _facilityRepository.Verify(r => r.AddAsync(
            It.Is<Facility>(f => f.CreatedByMobileNumber == CreatedByMobileNumber),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DoesNotTrustClientForCreatedByMobileNumber()
    {
        Facility? captured = null;
        _facilityRepository
            .Setup(r => r.AddAsync(It.IsAny<Facility>(), It.IsAny<CancellationToken>()))
            .Callback<Facility, CancellationToken>((f, _) => captured = f)
            .Returns(Task.CompletedTask);

        await _sut.CreateAsync(CreatedByMobileNumber, ValidRequest(), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.CreatedByMobileNumber.Should().Be(CreatedByMobileNumber);
    }

    [Fact]
    public async Task CreateAsync_PreservesContactOrder_FirstContactIsPrimary()
    {
        var response = await _sut.CreateAsync(CreatedByMobileNumber, ValidRequest(), CancellationToken.None);

        response.Contacts.Should().HaveCount(2);
        response.Contacts[0].Name.Should().Be("Anitha Varghese");
        response.Contacts[1].Name.Should().Be("Rahul Nair");
    }

    [Fact]
    public async Task CreateAsync_TrimsFacilityNameAndLicenseNumber()
    {
        var request = ValidRequest() with { FacilityName = "  Kochi Metro Hospital  ", LicenseNumber = " KL-HOSP-448120 " };

        var response = await _sut.CreateAsync(CreatedByMobileNumber, request, CancellationToken.None);

        response.FacilityName.Should().Be("Kochi Metro Hospital");
        response.LicenseNumber.Should().Be("KL-HOSP-448120");
    }
}
