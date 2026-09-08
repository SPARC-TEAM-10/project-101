using System.Security.Cryptography;
using System.Text;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Factories;
using Chh.Application.Services;
using Chh.Domain.Constants;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

/// <summary>CHH-10 Hospital/Ngo role-resolution scenarios for <see cref="OtpService.VerifyOtpAsync"/>.</summary>
public class OtpServiceFacilityRoleTests
{
    private readonly Mock<IOtpRequestRepository> _otpRequestRepository = new();
    private readonly Mock<ISmsGatewayClient> _smsGatewayClient = new();
    private readonly Mock<IIndividualProfileRepository> _individualProfileRepository = new();
    private readonly Mock<IFacilityRepository> _facilityRepository = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly OtpService _sut;

    private const string MobileNumber = "9000000001";

    public OtpServiceFacilityRoleTests()
    {
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IndividualProfile?)null);
        _jwtTokenGenerator
            .Setup(j => j.GenerateToken(MobileNumber, It.IsAny<string>()))
            .Returns(("fake-jwt", DateTimeOffset.UtcNow.AddHours(1)));

        _sut = new OtpService(
            _otpRequestRepository.Object,
            _smsGatewayClient.Object,
            _individualProfileRepository.Object,
            _facilityRepository.Object,
            _jwtTokenGenerator.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<OtpService>>());
    }

    private static string HashOtpCode(string otpCode) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(otpCode))).ToLowerInvariant();

    private static Facility CreateFacility(FacilityCategory category, FacilityVerificationStatus status) => new()
    {
        FacilityName = "Test Facility",
        Category = category,
        LicenseNumber = "LIC-TEST-001",
        Address = "1 Test Street",
        VerificationStatus = status,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow,
    };

    [Fact]
    public async Task VerifyOtpAsync_WhenMobileMatchesHospitalContact_IssuesTokenWithHospitalRole()
    {
        var otpRequest = OtpRequestFactory.Create(MobileNumber, HashOtpCode("123456"), DateTimeOffset.UtcNow);
        _otpRequestRepository
            .Setup(r => r.GetLatestTrackedByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateFacility(FacilityCategory.Hospital, FacilityVerificationStatus.Verified));

        var response = await _sut.VerifyOtpAsync(
            new OtpVerifyRequest { MobileNumber = MobileNumber, OtpCode = "123456" }, CancellationToken.None);

        response.Role.Should().Be(RoleConstants.Hospital);
        _jwtTokenGenerator.Verify(j => j.GenerateToken(MobileNumber, RoleConstants.Hospital), Times.Once);
    }

    [Fact]
    public async Task VerifyOtpAsync_WhenMobileMatchesNgoContact_IssuesTokenWithNgoRole()
    {
        var otpRequest = OtpRequestFactory.Create(MobileNumber, HashOtpCode("123456"), DateTimeOffset.UtcNow);
        _otpRequestRepository
            .Setup(r => r.GetLatestTrackedByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateFacility(FacilityCategory.Ngo, FacilityVerificationStatus.Verified));

        var response = await _sut.VerifyOtpAsync(
            new OtpVerifyRequest { MobileNumber = MobileNumber, OtpCode = "123456" }, CancellationToken.None);

        response.Role.Should().Be(RoleConstants.Ngo);
        _jwtTokenGenerator.Verify(j => j.GenerateToken(MobileNumber, RoleConstants.Ngo), Times.Once);
    }

    [Fact]
    public async Task VerifyOtpAsync_WhenFacilityIsPendingVerification_StillIssuesTokenWithFacilityRole()
    {
        var otpRequest = OtpRequestFactory.Create(MobileNumber, HashOtpCode("123456"), DateTimeOffset.UtcNow);
        _otpRequestRepository
            .Setup(r => r.GetLatestTrackedByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateFacility(FacilityCategory.Hospital, FacilityVerificationStatus.Pending));

        var response = await _sut.VerifyOtpAsync(
            new OtpVerifyRequest { MobileNumber = MobileNumber, OtpCode = "123456" }, CancellationToken.None);

        response.Role.Should().Be(RoleConstants.Hospital);
    }

    [Fact]
    public async Task VerifyOtpAsync_WhenNoIndividualProfileAndNoFacilityContact_IssuesTokenWithGuestRole()
    {
        var otpRequest = OtpRequestFactory.Create(MobileNumber, HashOtpCode("123456"), DateTimeOffset.UtcNow);
        _otpRequestRepository
            .Setup(r => r.GetLatestTrackedByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Facility?)null);

        var response = await _sut.VerifyOtpAsync(
            new OtpVerifyRequest { MobileNumber = MobileNumber, OtpCode = "123456" }, CancellationToken.None);

        response.Role.Should().Be(RoleConstants.Guest);
    }

    [Fact]
    public async Task VerifyOtpAsync_WhenIndividualProfileExists_DoesNotQueryFacilityRepository()
    {
        var otpRequest = OtpRequestFactory.Create(MobileNumber, HashOtpCode("123456"), DateTimeOffset.UtcNow);
        _otpRequestRepository
            .Setup(r => r.GetLatestTrackedByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<IndividualProfile>());

        var response = await _sut.VerifyOtpAsync(
            new OtpVerifyRequest { MobileNumber = MobileNumber, OtpCode = "123456" }, CancellationToken.None);

        response.Role.Should().Be(RoleConstants.Individual);
        _facilityRepository.Verify(
            r => r.GetByContactMobileNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
