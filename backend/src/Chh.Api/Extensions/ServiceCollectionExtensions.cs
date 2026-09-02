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
    public static IServiceCollection AddChhServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ChhDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ChhDatabase")));

        services.AddScoped<IOtpRequestRepository, OtpRequestRepository>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<ISmsGatewayClient, LoggingSmsGatewayClient>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddValidatorsFromAssembly(typeof(OtpRequestRequestValidator).Assembly);
        services.AddFluentValidationAutoValidation();

        return services;
    }
}
