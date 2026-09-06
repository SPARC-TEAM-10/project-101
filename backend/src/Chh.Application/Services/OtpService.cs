using System.Security.Cryptography;
using System.Text;
using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Factories;
using Chh.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace Chh.Application.Services;

/// <summary>Orchestrates OTP generation, persistence, and dispatch (CHH-F01).</summary>
public class OtpService : IOtpService
{
    private readonly IOtpRequestRepository _otpRequestRepository;
    private readonly ISmsGatewayClient _smsGatewayClient;
    private readonly IIndividualProfileRepository _individualProfileRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OtpService> _logger;

    /// <summary>Creates the service with its repository, SMS gateway, JWT, unit-of-work, and logger dependencies.</summary>
    /// <param name="otpRequestRepository">Data layer for reading and persisting OTP requests.</param>
    /// <param name="smsGatewayClient">Gateway used to dispatch the generated OTP code.</param>
    /// <param name="individualProfileRepository">Used to resolve the role claim (Individual vs. Guest) on verify.</param>
    /// <param name="jwtTokenGenerator">Issues the access token returned on successful verification.</param>
    /// <param name="unitOfWork">Persists changes made during the request.</param>
    /// <param name="logger">Logger for dispatch-failure diagnostics.</param>
    public OtpService(
        IOtpRequestRepository otpRequestRepository,
        ISmsGatewayClient smsGatewayClient,
        IIndividualProfileRepository individualProfileRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork,
        ILogger<OtpService> logger)
    {
        _otpRequestRepository = otpRequestRepository;
        _smsGatewayClient = smsGatewayClient;
        _individualProfileRepository = individualProfileRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
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
        var otpRequest = OtpRequestFactory.Create(request.MobileNumber, otpCodeHash, requestedAtUtc);

        await _otpRequestRepository.AddAsync(otpRequest, ct).ConfigureAwait(false);

        try
        {
            await _smsGatewayClient.SendOtpAsync(request.MobileNumber, otpCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "SMS gateway dispatch failed for mobile number {MaskedMobileNumber}",
                OtpConstants.MaskMobileNumber(request.MobileNumber));
            throw new OtpDispatchException(ex);
        }

        var response = new OtpRequestResponse
        {
            MaskedMobileNumber = OtpConstants.MaskMobileNumber(request.MobileNumber),
            OtpExpiresAtUtc = otpRequest.OtpExpiresAtUtc,
            ResendAvailableAtUtc = otpRequest.ResendAvailableAtUtc
        };

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return response;
    }

    /// <inheritdoc />
    public async Task<OtpVerifyResponse> VerifyOtpAsync(OtpVerifyRequest request, CancellationToken ct)
    {
        var verifiedAtUtc = DateTimeOffset.UtcNow;

        var latest = await _otpRequestRepository
            .GetLatestTrackedByMobileNumberAsync(request.MobileNumber, ct)
            .ConfigureAwait(false);

        // "Never requested" folds into InvalidOtpException (not OtpExpiredException) so the
        // response can't be used to enumerate which mobile numbers have ever requested an OTP.
        if (latest is null || !IsMatchingOtpCode(request.OtpCode, latest.OtpCodeHash))
        {
            throw new InvalidOtpException();
        }

        // Checked after the code match so a wrong code on an expired request still reports
        // "invalid" rather than leaking that a (now-expired) OTP had existed for this number —
        // only a *matching* code past its expiry gets the distinct "expired" message.
        if (latest.OtpExpiresAtUtc < verifiedAtUtc)
        {
            throw new OtpExpiredException();
        }

        latest.MarkVerified();

        var profile = await _individualProfileRepository
            .GetByMobileNumberAsync(request.MobileNumber, ct)
            .ConfigureAwait(false);
        var role = profile is not null ? RoleConstants.Individual : RoleConstants.Guest;

        var (accessToken, tokenExpiresAtUtc) = _jwtTokenGenerator.GenerateToken(request.MobileNumber, role);

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return new OtpVerifyResponse
        {
            MaskedMobileNumber = OtpConstants.MaskMobileNumber(request.MobileNumber),
            VerifiedAtUtc = verifiedAtUtc,
            AccessToken = accessToken,
            TokenExpiresAtUtc = tokenExpiresAtUtc,
            Role = role
        };
    }

    private static bool IsMatchingOtpCode(string otpCode, string expectedHash)
    {
        var actualHashBytes = Encoding.UTF8.GetBytes(HashOtpCode(otpCode));
        var expectedHashBytes = Encoding.UTF8.GetBytes(expectedHash);

        // Constant-time comparison — a length/early-exit-sensitive compare here would leak
        // timing information about the stored hash.
        return actualHashBytes.Length == expectedHashBytes.Length
            && CryptographicOperations.FixedTimeEquals(actualHashBytes, expectedHashBytes);
    }

    private static string GenerateOtpCode()
    {
        var code = RandomNumberGenerator.GetInt32(0, (int)Math.Pow(10, OtpConstants.CodeLength));
        return code.ToString(new string('0', OtpConstants.CodeLength));
    }

    private static string HashOtpCode(string otpCode)
    {
        var bytes = Encoding.UTF8.GetBytes(otpCode);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
