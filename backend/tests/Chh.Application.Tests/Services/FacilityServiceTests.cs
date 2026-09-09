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
    private readonly Mock<IFacilityDocumentStorageService> _documentStorageService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly FacilityService _sut;

    public FacilityServiceTests()
    {
        _sut = new FacilityService(_facilityRepository.Object, _documentStorageService.Object, _unitOfWork.Object);
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

    // --- UploadLicenseDocumentAsync (CHH-79) ---

    private static Facility MakeFacility() => new()
    {
        FacilityName = "City General Hospital",
        Category = FacilityCategory.Hospital,
        SubCategory = FacilitySubCategory.Government,
        LicenseNumber = "KL-HOSP-000000",
        Address = "123 Main St, Kochi",
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task UploadLicenseDocumentAsync_FacilityNotFound_ReturnsNull()
    {
        _facilityRepository
            .Setup(r => r.GetTrackedByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Facility?)null);

        var result = await _sut.UploadLicenseDocumentAsync(
            Guid.NewGuid(), new MemoryStream([1, 2, 3]), "license.pdf", "application/pdf", 3, CancellationToken.None);

        result.Should().BeNull();
        _documentStorageService.Verify(
            s => s.SaveAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UploadLicenseDocumentAsync_NoFileProvided_ThrowsInvalidFacilityDocumentException()
    {
        var facility = MakeFacility();
        _facilityRepository
            .Setup(r => r.GetTrackedByIdAsync(facility.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        var act = () => _sut.UploadLicenseDocumentAsync(
            facility.Id, Stream.Null, string.Empty, string.Empty, 0, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidFacilityDocumentException>()
            .WithMessage(Chh.Domain.Constants.FacilityDocumentConstants.NoFileProvidedMessage);
    }

    [Fact]
    public async Task UploadLicenseDocumentAsync_DisallowedContentType_ThrowsInvalidFacilityDocumentException()
    {
        var facility = MakeFacility();
        _facilityRepository
            .Setup(r => r.GetTrackedByIdAsync(facility.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        var act = () => _sut.UploadLicenseDocumentAsync(
            facility.Id, new MemoryStream([1, 2, 3]), "malware.exe", "application/x-msdownload", 3, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidFacilityDocumentException>()
            .WithMessage(Chh.Domain.Constants.FacilityDocumentConstants.InvalidFileTypeMessage);
    }

    [Fact]
    public async Task UploadLicenseDocumentAsync_TooLarge_ThrowsInvalidFacilityDocumentException()
    {
        var facility = MakeFacility();
        _facilityRepository
            .Setup(r => r.GetTrackedByIdAsync(facility.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        var act = () => _sut.UploadLicenseDocumentAsync(
            facility.Id,
            new MemoryStream([1, 2, 3]),
            "license.pdf",
            "application/pdf",
            Chh.Domain.Constants.FacilityDocumentConstants.MaxFileSizeBytes + 1,
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidFacilityDocumentException>()
            .WithMessage(Chh.Domain.Constants.FacilityDocumentConstants.FileTooLargeMessage);
    }

    [Fact]
    public async Task UploadLicenseDocumentAsync_Success_StoresFileAndUpdatesLicenseDocumentUrl()
    {
        var facility = MakeFacility();
        _facilityRepository
            .Setup(r => r.GetTrackedByIdAsync(facility.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);
        _documentStorageService
            .Setup(s => s.SaveAsync(facility.Id, "license.pdf", It.IsAny<Stream>(), "application/pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/facility-documents/abc/license.pdf");

        var result = await _sut.UploadLicenseDocumentAsync(
            facility.Id, new MemoryStream([1, 2, 3]), "license.pdf", "application/pdf", 3, CancellationToken.None);

        result.Should().NotBeNull();
        result!.LicenseDocumentUrl.Should().Be("/uploads/facility-documents/abc/license.pdf");
        facility.LicenseDocumentUrl.Should().Be("/uploads/facility-documents/abc/license.pdf");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
