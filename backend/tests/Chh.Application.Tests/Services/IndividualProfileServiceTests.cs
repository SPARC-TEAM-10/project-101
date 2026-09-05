using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Factories;
using Chh.Application.Services;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

public class IndividualProfileServiceTests
{
    private readonly Mock<IIndividualProfileRepository> _individualProfileRepository = new();
    private readonly Mock<IOtpRequestRepository> _otpRequestRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly IndividualProfileService _sut;

    private const string MobileNumber = "9876543210";

    public IndividualProfileServiceTests()
    {
        _sut = new IndividualProfileService(
            _individualProfileRepository.Object,
            _otpRequestRepository.Object,
            _unitOfWork.Object);
    }

    private static CreateIndividualProfileRequest ValidRequest() => new()
    {
        MobileNumber = MobileNumber,
        FullName = "Jane Doe",
        Email = "jane@example.com",
        BloodGroup = BloodGroup.OPositive,
        DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
        Gender = Gender.Female,
        LocationCityArea = "Kochi"
    };

    private static OtpRequest VerifiedOtpRequest() =>
        OtpRequestFactory.Create(MobileNumber, "hash", DateTimeOffset.UtcNow.AddMinutes(-1));

    [Fact]
    public async Task RegisterAsync_WhenMobileNumberIsVerifiedAndUnregistered_PersistsAndReturnsDto()
    {
        var verifiedOtp = VerifiedOtpRequest();
        verifiedOtp.MarkVerified();
        _otpRequestRepository
            .Setup(r => r.GetLatestByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verifiedOtp);
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IndividualProfile?)null);

        var result = await _sut.RegisterAsync(ValidRequest(), CancellationToken.None);

        result.FullName.Should().Be("Jane Doe");
        result.IsReceiverOnly.Should().BeFalse();
        _individualProfileRepository.Verify(r => r.AddAsync(It.IsAny<IndividualProfile>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenAnyHealthFlagIsSet_MarksProfileAsReceiverOnly()
    {
        var verifiedOtp = VerifiedOtpRequest();
        verifiedOtp.MarkVerified();
        _otpRequestRepository
            .Setup(r => r.GetLatestByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verifiedOtp);
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IndividualProfile?)null);

        var result = await _sut.RegisterAsync(ValidRequest() with { IsChronicIllness = true }, CancellationToken.None);

        result.IsReceiverOnly.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_WhenMobileNumberHasNoOtpRequest_ThrowsMobileNumberNotVerifiedException()
    {
        _otpRequestRepository
            .Setup(r => r.GetLatestByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OtpRequest?)null);

        var act = () => _sut.RegisterAsync(ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<MobileNumberNotVerifiedException>();
        _individualProfileRepository.Verify(r => r.AddAsync(It.IsAny<IndividualProfile>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenOtpWasRequestedButNotVerified_ThrowsMobileNumberNotVerifiedException()
    {
        _otpRequestRepository
            .Setup(r => r.GetLatestByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VerifiedOtpRequest()); // IsVerified still false — never called MarkVerified()

        var act = () => _sut.RegisterAsync(ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<MobileNumberNotVerifiedException>();
    }

    [Fact]
    public async Task RegisterAsync_WhenProfileAlreadyExistsForMobileNumber_ThrowsIndividualAlreadyRegisteredException()
    {
        var verifiedOtp = VerifiedOtpRequest();
        verifiedOtp.MarkVerified();
        _otpRequestRepository
            .Setup(r => r.GetLatestByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verifiedOtp);
        var existingProfileRequest = new CreateIndividualProfileRequest
        {
            MobileNumber = MobileNumber,
            FullName = "Existing User",
            Email = "existing@example.com",
            BloodGroup = BloodGroup.APositive,
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
            Gender = Gender.Male,
            LocationCityArea = "Kochi"
        };
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(IndividualProfileFactory.Create(existingProfileRequest, DateTimeOffset.UtcNow));

        var act = () => _sut.RegisterAsync(ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<IndividualAlreadyRegisteredException>();
        _individualProfileRepository.Verify(r => r.AddAsync(It.IsAny<IndividualProfile>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
