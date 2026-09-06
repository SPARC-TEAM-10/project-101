using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chh.Api.Controllers;

/// <summary>Mobile-number + OTP authentication endpoints (CHH-F01). The "api/v1/auth" prefix comes from the global convention in <c>Program.cs</c>; "otp" is this controller's own shared segment.</summary>
[ApiController]
[Route("otp")]
public class AuthController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>Creates the controller with its service and logger dependencies.</summary>
    public AuthController(IOtpService otpService, ILogger<AuthController> logger)
    {
        _otpService = otpService;
        _logger = logger;
    }

    /// <summary>Requests a one-time password for a mobile number and dispatches it via the SMS gateway.</summary>
    /// <param name="request">The mobile number to send the OTP to.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpPost("request")]
    // One of only 2 endpoints allowed [AllowAnonymous] in this system — see api-standards.md §5
    [AllowAnonymous]
    [ProducesResponseType(typeof(OtpRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<OtpRequestResponse>> RequestOtpAsync(
        [FromBody] OtpRequestRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _otpService.RequestOtpAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Verifies a submitted OTP code for a mobile number.</summary>
    /// <param name="request">The mobile number and OTP code to verify.</param>
    /// <param name="cancellationToken">Cancellation token forwarded through the service and repository layers.</param>
    [HttpPost("verify")]
    // One of only 2 endpoints allowed [AllowAnonymous] in this system — see api-standards.md §5
    [AllowAnonymous]
    [ProducesResponseType(typeof(OtpVerifyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OtpVerifyResponse>> VerifyOtpAsync(
        [FromBody] OtpVerifyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _otpService.VerifyOtpAsync(request, cancellationToken);
        return Ok(result);
    }
}
