using Chh.Api.Extensions;
using Chh.Application.Abstractions;
using Serilog;

// Bootstrap logger: captures failures that happen before the host is built.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Chh.Api host");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog, configured entirely from the "Serilog" block in appsettings.json
    // (api-standards.md §8). Never log OTP codes, JWTs, full mobile numbers, or
    // health-screening data.
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // RFC 7807 error responses (api-standards.md §7). Fully qualified to avoid
    // ambiguity with the built-in ASP.NET Core 8 AddProblemDetails overload.
    Hellang.Middleware.ProblemDetails.ProblemDetailsExtensions.AddProblemDetails(
        builder.Services,
        options =>
        {
            options.IncludeExceptionDetails = (_, _) => builder.Environment.IsDevelopment();
            options.Map<NotImplementedException>(_ =>
                new Hellang.Middleware.ProblemDetails.StatusCodeProblemDetails(StatusCodes.Status501NotImplemented));
            // CHH-8: OTP request domain exception mappings.
            options.Map<OtpResendCooldownException>(ex =>
                new Hellang.Middleware.ProblemDetails.StatusCodeProblemDetails(StatusCodes.Status429TooManyRequests)
                {
                    Detail = ex.Message
                });
            options.Map<OtpDispatchException>(ex =>
                new Hellang.Middleware.ProblemDetails.StatusCodeProblemDetails(StatusCodes.Status502BadGateway)
                {
                    Detail = ex.Message
                });
            options.Map<ChhValidationException>(ex =>
                new Hellang.Middleware.ProblemDetails.StatusCodeProblemDetails(StatusCodes.Status422UnprocessableEntity)
                {
                    Detail = ex.Message
                });
            // Further domain exception mappings are added by the tickets that introduce them.
        });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // FluentValidation failures return 422 (RFC 7807 ValidationProblemDetails), not the ASP.NET
    // Core default of 400 — api-standards.md §7 / CHH-8 error-handling table.
    builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
    {
        var defaultFactory = options.InvalidModelStateResponseFactory;
        options.InvalidModelStateResponseFactory = context =>
        {
            var result = defaultFactory(context);
            if (result is Microsoft.AspNetCore.Mvc.ObjectResult objectResult)
            {
                objectResult.StatusCode = StatusCodes.Status422UnprocessableEntity;
            }

            return result;
        };
    });

    // Application/Infrastructure service registration (CHH-8: OTP request — ChhDbContext,
    // repositories, services, SMS gateway client, FluentValidation). JWT Bearer
    // authentication (CHH-F01) is deliberately NOT registered yet — the
    // authentication/authorization middleware is added together with it.
    builder.Services.AddChhServices(builder.Configuration);

    var app = builder.Build();

    Hellang.Middleware.ProblemDetails.ProblemDetailsExtensions.UseProblemDetails(app);

    app.UseSerilogRequestLogging();

    // Swagger UI is Development-only (api-standards.md §4).
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // Liveness probe. Anonymous by design — it is infrastructure, not an API resource,
    // so it is not part of contracts/chh-api.v1.yaml and carries no /api/v1 prefix.
    app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
       .WithName("HealthCheck");

    app.MapControllers();

    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Chh.Api host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>Entry point type, exposed so WebApplicationFactory&lt;Program&gt; can host the API in tests.</summary>
public partial class Program
{
}
