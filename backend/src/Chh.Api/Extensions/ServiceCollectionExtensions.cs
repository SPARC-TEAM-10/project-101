using Chh.Application.Contracts;
using Chh.Application.Services;
using Chh.Application.Validators;
using Chh.Infrastructure.ExternalClients;
using Chh.Infrastructure.Persistence;
using Chh.Infrastructure.Persistence.Encryption;
using Chh.Infrastructure.Persistence.Repositories;
using FluentValidation;
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

        // Backs the AES-256 value converters on IndividualProfile's PII/health-screening columns
        // (db-standards.md §3). Singleton: stateless besides the key, and ChhDbContext resolves it once per scope anyway.
        services.AddSingleton<IFieldEncryptor, AesFieldEncryptor>();

        services.AddScoped<IOtpRequestRepository, OtpRequestRepository>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IIndividualProfileRepository, IndividualProfileRepository>();
        services.AddScoped<IIndividualProfileService, IndividualProfileService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // SMS gateway: Fast2SMS when an API key is configured (Azure Key Vault / user-secrets —
        // never appsettings.json, api-standards.md §5), otherwise the logging stub. Keeps local
        // dev from spending paid SMS credits by default without hardcoding a provider choice.
        var fast2SmsApiKey = configuration["Fast2Sms:ApiKey"];
        if (!string.IsNullOrWhiteSpace(fast2SmsApiKey))
        {
            services.AddHttpClient<ISmsGatewayClient, Fast2SmsGatewayClient>(client =>
            {
                client.BaseAddress = new Uri("https://www.fast2sms.com/");
                client.DefaultRequestHeaders.Add("authorization", fast2SmsApiKey);
            });
        }
        else
        {
            services.AddScoped<ISmsGatewayClient, LoggingSmsGatewayClient>();
        }

        services.AddValidatorsFromAssembly(typeof(OtpRequestRequestValidator).Assembly);

        return services;
    }
}
