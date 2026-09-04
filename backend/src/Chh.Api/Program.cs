using Chh.Api.Extensions;
using Chh.Api.Filters;
using Chh.Api.Routing;
using Chh.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog, configured entirely from the "Serilog" block in appsettings.json
// (api-standards.md §8). Never log OTP codes, JWTs, full mobile numbers, or
// health-screening data.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// RFC 7807 error responses (api-standards.md §7) — mapping logic lives in
// ProblemDetailsServiceCollectionExtensions, not here.
builder.Services.AddChhProblemDetails(builder.Environment);

// Every controller gets the "api/v1/[controller]" route (api-standards.md §1 URL versioning),
// kebab-cased (e.g. BloodRequestsController -> "api/v1/blood-requests") — declared once here
// instead of a [Route] attribute repeated on each controller.
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RoutePrefixConvention("api/v1/[controller]"));
    options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));
    // Runs FluentValidation and throws ChhValidationException (-> 422) on failure — see
    // FluentValidationActionFilter for why this replaces FluentValidation.AspNetCore's
    // auto-validation (it returned 400, not the required 422).
    options.Filters.Add<FluentValidationActionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Application/Infrastructure service registration (CHH-8: OTP request — ChhDbContext,
// repositories, services, SMS gateway client, FluentValidation). JWT Bearer
// authentication (CHH-F01) is deliberately NOT registered yet — the
// authentication/authorization middleware is added together with it.
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ChhDbContext>();
    await dbContext.Database.MigrateAsync(); // Applies migrations
}

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
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();

/// <summary>Entry point type, exposed so WebApplicationFactory&lt;Program&gt; can host the API in tests.</summary>
public partial class Program
{
}
