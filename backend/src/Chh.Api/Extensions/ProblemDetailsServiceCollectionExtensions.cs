using Chh.Application.Abstractions;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

namespace Chh.Api.Extensions;

/// <summary>
/// Registers RFC 7807 <see cref="ProblemDetails"/> error handling (api-standards.md §7) — kept out
/// of <c>Program.cs</c> so domain-exception-to-status-code mappings live in one dedicated place as
/// the set of domain exceptions grows.
/// </summary>
public static class ProblemDetailsServiceCollectionExtensions
{
    /// <summary>
    /// Maps CHH domain exceptions to RFC 7807 <see cref="ProblemDetails"/> responses, and forces
    /// FluentValidation failures to 422 Unprocessable Entity instead of the ASP.NET Core default of 400.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="environment">Used to gate exception detail exposure to Development only.</param>
    public static IServiceCollection AddChhProblemDetails(this IServiceCollection services, IWebHostEnvironment environment)
    {
        // Fully qualified to avoid ambiguity with the built-in ASP.NET Core 8 AddProblemDetails overload.
        Hellang.Middleware.ProblemDetails.ProblemDetailsExtensions.AddProblemDetails(services, options =>
        {
            options.IncludeExceptionDetails = (_, _) => environment.IsDevelopment();
            options.Map<NotImplementedException>(_ =>
                new StatusCodeProblemDetails(StatusCodes.Status501NotImplemented));
            // CHH-8: OTP request domain exception mappings.
            options.Map<OtpResendCooldownException>(ex =>
                new StatusCodeProblemDetails(StatusCodes.Status429TooManyRequests)
                {
                    Detail = ex.Message
                });
            options.Map<OtpDispatchException>(ex =>
                new StatusCodeProblemDetails(StatusCodes.Status502BadGateway)
                {
                    Detail = ex.Message
                });
            // CHH-9: OTP verify domain exception mapping.
            options.Map<InvalidOtpException>(ex =>
                new StatusCodeProblemDetails(StatusCodes.Status422UnprocessableEntity)
                {
                    Detail = ex.Message
                });
            // Surfaces the per-field failure messages (not just the generic exception message) —
            // api-standards.md §7's documented ValidationProblemDetails(ex.Failures) shape.
            options.Map<ChhValidationException>(ex =>
                new ValidationProblemDetails(ex.Failures)
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = ex.Message
                });
            // Further domain exception mappings are added by the tickets that introduce them.
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            var defaultFactory = options.InvalidModelStateResponseFactory;
            options.InvalidModelStateResponseFactory = context =>
            {
                var result = defaultFactory(context);
                if (result is ObjectResult objectResult)
                {
                    objectResult.StatusCode = StatusCodes.Status422UnprocessableEntity;
                }

                return result;
            };
        });

        return services;
    }
}
