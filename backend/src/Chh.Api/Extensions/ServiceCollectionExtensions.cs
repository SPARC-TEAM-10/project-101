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

        services.AddFast2Sms(configuration);

        services.AddValidatorsFromAssembly(typeof(OtpRequestRequestValidator).Assembly);

        return services;
    }

    /// <summary>
    /// Registers the OTP-dispatch channel. No API key configured -&gt; <see cref="LoggingSmsGatewayClient"/>
    /// (keeps local dev from spending paid credits by default). API key configured -&gt;
    /// <c>Fast2Sms:Channel</c> picks <see cref="Fast2SmsWhatsAppGatewayClient"/> ("whatsapp", not
    /// subject to TRAI DLT — the current working channel) or <see cref="Fast2SmsGatewayClient"/>
    /// ("sms" or unset — blocked pending DLT registration, kept for when that completes). Also
    /// registers <see cref="Fast2SmsWalletHealthCheck"/> whenever a key is configured, since a
    /// drained wallet affects both channels.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">App configuration.</param>
    private static void AddFast2Sms(this IServiceCollection services, IConfiguration configuration)
    {
        var apiKey = configuration["Fast2Sms:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            services.AddScoped<ISmsGatewayClient, LoggingSmsGatewayClient>();
            return;
        }

        void ConfigureClient(HttpClient client)
        {
            client.BaseAddress = new Uri(configuration["Fast2Sms:BaseUrl"]!);
            client.DefaultRequestHeaders.Add(Fast2SmsConstants.AuthorizationHeaderName, apiKey);
        }

        services.AddHttpClient<Fast2SmsWalletHealthCheck>(ConfigureClient);
        // Tagged "external", not part of the default set — see Program.cs: "/health" stays a pure
        // liveness probe (an orchestrator would otherwise restart a perfectly healthy container
        // over Fast2SMS being down or low on credit, which fixes nothing). "/health/ready" includes it.
        services.AddHealthChecks().AddCheck<Fast2SmsWalletHealthCheck>("fast2sms-wallet", tags: ["external"]);

        var channel = configuration["Fast2Sms:Channel"];
        if (string.Equals(channel, "whatsapp", StringComparison.OrdinalIgnoreCase))
        {
            services.AddOptions<Fast2SmsWhatsAppOptions>()
                .Bind(configuration.GetSection(Fast2SmsWhatsAppOptions.SectionName))
                .Validate(o => !string.IsNullOrWhiteSpace(o.PhoneNumberId), "Fast2Sms:WhatsApp:PhoneNumberId is required.")
                .Validate(o => !string.IsNullOrWhiteSpace(o.OtpMessageId), "Fast2Sms:WhatsApp:OtpMessageId is required.")
                .Validate(o => !string.IsNullOrWhiteSpace(o.DonorRequestMessageId), "Fast2Sms:WhatsApp:DonorRequestMessageId is required.")
                .ValidateOnStart();

            services.AddHttpClient<IWhatsAppTemplateClient, Fast2SmsWhatsAppTemplateClient>(ConfigureClient);
            services.AddScoped<ISmsGatewayClient, Fast2SmsWhatsAppGatewayClient>();
        }
        else
        {
            services.AddHttpClient<ISmsGatewayClient, Fast2SmsGatewayClient>(ConfigureClient);
        }
    }
}
