using System.Security.Cryptography;
using System.Text;
using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Chh.Application.Services;

/// <summary>Orchestrates OTP generation, persistence, and dispatch (CHH-F01).</summary>
public class OtpService : IOtpService
{
    private const int OtpCodeLength = 6;
    private const int MaskedVisibleDigits = 2;
    private const char MaskChar = '*';

    private readonly IOtpRequestRepository _otpRequestRepository;
    private readonly ISmsGatewayClient _smsGatewayClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OtpService> _logger;

    /// <summary>Creates the service with its repository, SMS gateway, unit-of-work, and logger dependencies.</summary>
    public OtpService(
        IOtpRequestRepository otpRequestRepository,
        ISmsGatewayClient smsGatewayClient,
        IUnitOfWork unitOfWork,
        ILogger<OtpService> logger)
    {
        _otpRequestRepository = otpRequestRepository;
        _smsGatewayClient = smsGatewayClient;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<OtpRequestResponse> RequestOtpAsync(OtpRequestRequest request, CancellationToken ct)
    {
        var requestedAtUtc = DateTimeOffset.UtcNow;

        var latest = await _otpRequestRepository
            .GetLatestByMobileNumberAsync(request.MobileNumber, ct)
            .ConfigureAwait(false);

        if (latest is not null && latest.ResendAvailableAtUtc > requestedAtUtc)
        {
            throw new OtpResendCooldownException();
        }

        var otpCode = GenerateOtpCode();
        var otpCodeHash = HashOtpCode(otpCode);
        var otpRequest = OtpRequest.Create(request.MobileNumber, otpCodeHash, requestedAtUtc);

        await _otpRequestRepository.AddAsync(otpRequest, ct).ConfigureAwait(false);

        try
        {
            await _smsGatewayClient.SendOtpAsync(request.MobileNumber, otpCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "SMS gateway dispatch failed for mobile number {MaskedMobileNumber}",
                MaskMobileNumber(request.MobileNumber));
            throw new OtpDispatchException(ex);
        }

        var response = new OtpRequestResponse
        {
            MaskedMobileNumber = MaskMobileNumber(request.MobileNumber),
            OtpExpiresAtUtc = otpRequest.OtpExpiresAtUtc,
            ResendAvailableAtUtc = otpRequest.ResendAvailableAtUtc
        };

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return response;
    }

    private static string GenerateOtpCode()
    {
        var code = RandomNumberGenerator.GetInt32(0, (int)Math.Pow(10, OtpCodeLength));
        return code.ToString(new string('0', OtpCodeLength));
    }

    private static string HashOtpCode(string otpCode)
    {
        var bytes = Encoding.UTF8.GetBytes(otpCode);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string MaskMobileNumber(string mobileNumber)
    {
        var visible = mobileNumber[^MaskedVisibleDigits..];
        var maskedLength = mobileNumber.Length - MaskedVisibleDigits;
        return new string(MaskChar, maskedLength) + visible;
    }
}
