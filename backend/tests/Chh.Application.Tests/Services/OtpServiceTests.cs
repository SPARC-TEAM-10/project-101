using System.Security.Cryptography;
using System.Text;
using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Factories;
using Chh.Application.Services;
using Chh.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

public class OtpServiceTests
{
    private readonly Mock<IOtpRequestRepository> _otpRequestRepository = new();
    private readonly Mock<ISmsGatewayClient> _smsGatewayClient = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly OtpService _sut;

    private const string MobileNumber = "9876543210";

    public OtpServiceTests()
    {
        _sut = new OtpService(
            _otpRequestRepository.Object,
            _smsGatewayClient.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<OtpService>>());
    }

    private static string HashOtpCode(string otpCode) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(otpCode))).ToLowerInvariant();

    [Fact]
    public async Task RequestOtpAsync_WhenNoPreviousOtpExists_DispatchesAndPersists()
    {
        _otpRequestRepository
            .Setup(r => r.GetLatestByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OtpRequest?)null);

        var response = await _sut.RequestOtpAsync(new OtpRequestRequest { MobileNumber = MobileNumber }, CancellationToken.None);

        response.MaskedMobileNumber.Should().Be("********10");
        _otpRequestRepository.Verify(r => r.AddAsync(It.IsAny<OtpRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _smsGatewayClient.Verify(s => s.SendOtpAsync(MobileNumber, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestOtpAsync_WhenResendCooldownStillActive_ThrowsWithoutDispatchingOrPersisting()
    {
        var latest = OtpRequestFactory.Create(MobileNumber, HashOtpCode("111111"), DateTimeOffset.UtcNow);
        _otpRequestRepository
            .Setup(r => r.GetLatestByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(latest);

        var act = () => _sut.RequestOtpAsync(new OtpRequestRequest { MobileNumber = MobileNumber }, CancellationToken.None);

        await act.Should().ThrowAsync<OtpResendCooldownException>();
        _otpRequestRepository.Verify(r => r.AddAsync(It.IsAny<OtpRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _smsGatewayClient.Verify(s => s.SendOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RequestOtpAsync_WhenSmsGatewayFails_ThrowsOtpDispatchException()
    {
        _otpRequestRepository
            .Setup(r => r.GetLatestByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OtpRequest?)null);
        _smsGatewayClient
            .Setup(s => s.SendOtpAsync(MobileNumber, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("gateway down"));

        var act = () => _sut.RequestOtpAsync(new OtpRequestRequest { MobileNumber = MobileNumber }, CancellationToken.None);

        await act.Should().ThrowAsync<OtpDispatchException>();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VerifyOtpAsync_WhenCodeMatchesAndUnexpired_MarksVerifiedAndReturnsResponse()
    {
        var otpRequest = OtpRequestFactory.Create(MobileNumber, HashOtpCode("123456"), DateTimeOffset.UtcNow);
        _otpRequestRepository
            .Setup(r => r.GetLatestTrackedByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);

        var response = await _sut.VerifyOtpAsync(
            new OtpVerifyRequest { MobileNumber = MobileNumber, OtpCode = "123456" }, CancellationToken.None);

        response.MaskedMobileNumber.Should().Be("********10");
        otpRequest.IsVerified.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyOtpAsync_WhenNoOtpWasEverRequested_ThrowsInvalidOtpException()
    {
        _otpRequestRepository
            .Setup(r => r.GetLatestTrackedByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OtpRequest?)null);

        var act = () => _sut.VerifyOtpAsync(
            new OtpVerifyRequest { MobileNumber = MobileNumber, OtpCode = "123456" }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOtpException>();
    }

    [Fact]
    public async Task VerifyOtpAsync_WhenCodeIsExpired_ThrowsInvalidOtpException()
    {
        var otpRequest = OtpRequestFactory.Create(MobileNumber, HashOtpCode("123456"), DateTimeOffset.UtcNow.AddMinutes(-10));
        _otpRequestRepository
            .Setup(r => r.GetLatestTrackedByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);

        var act = () => _sut.VerifyOtpAsync(
            new OtpVerifyRequest { MobileNumber = MobileNumber, OtpCode = "123456" }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOtpException>();
        otpRequest.IsVerified.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyOtpAsync_WhenCodeDoesNotMatch_ThrowsInvalidOtpExceptionAndDoesNotPersist()
    {
        var otpRequest = OtpRequestFactory.Create(MobileNumber, HashOtpCode("123456"), DateTimeOffset.UtcNow);
        _otpRequestRepository
            .Setup(r => r.GetLatestTrackedByMobileNumberAsync(MobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpRequest);

        var act = () => _sut.VerifyOtpAsync(
            new OtpVerifyRequest { MobileNumber = MobileNumber, OtpCode = "000000" }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOtpException>();
        otpRequest.IsVerified.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
