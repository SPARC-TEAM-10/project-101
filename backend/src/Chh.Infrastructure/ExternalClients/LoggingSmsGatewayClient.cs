using Chh.Application.Contracts;
using Chh.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace Chh.Infrastructure.ExternalClients;

/// <summary>
/// Stub <see cref="ISmsGatewayClient"/> implementation that logs the dispatch instead of calling a
/// real provider. The real SMS provider (Twilio vs Firebase) is an open PRD question — this stub
/// keeps the rest of the OTP flow (CHH-F01) unblocked until that decision is made. Replace with a
/// real typed <c>IHttpClientFactory</c> client once the provider is chosen.
/// </summary>
public class LoggingSmsGatewayClient : ISmsGatewayClient
{
    private readonly ILogger<LoggingSmsGatewayClient> _logger;

    /// <summary>Creates the stub client with its logger dependency.</summary>
    /// <param name="logger">Logger the stub writes the simulated dispatch to.</param>
    public LoggingSmsGatewayClient(ILogger<LoggingSmsGatewayClient> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task SendOtpAsync(string mobileNumber, string otpCode, CancellationToken ct)
    {
        // Never log the OTP code itself (api-standards.md §8).
        _logger.LogInformation(
            "Stub SMS gateway: would dispatch OTP to {MaskedMobileNumber}",
            OtpConstants.MaskMobileNumber(mobileNumber));
        return Task.CompletedTask;
    }
}
