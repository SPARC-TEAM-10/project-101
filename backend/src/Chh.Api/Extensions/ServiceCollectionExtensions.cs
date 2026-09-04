using Chh.Application.Contracts;
using Chh.Application.Services;
using Chh.Application.Validators;
using Chh.Infrastructure.ExternalClients;
using Chh.Infrastructure.Persistence;
using Chh.Infrastructure.Persistence.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

namespace Chh.Api.Extensions;

/// <summary>Composition-root registration for CHH Application and Infrastructure services.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ChhDbContext"/> (Npgsql), repositories, services, the SMS gateway client,
    /// and FluentValidation validators. This is the only extension referencing <c>Chh.Infrastructure</c>
    /// types from <c>Chh.Api</c>.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">App configuration, used to resolve the database connection string.</param>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ChhDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IOtpRequestRepository, OtpRequestRepository>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<ISmsGatewayClient, LoggingSmsGatewayClient>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddValidatorsFromAssembly(typeof(OtpRequestRequestValidator).Assembly);
        services.AddFluentValidationAutoValidation();

        return services;
    }
}
