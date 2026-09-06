using Chh.Application.Contracts;
using Chh.Application.Services;
using Chh.Application.Validators;
using Chh.Infrastructure.ExternalClients;
using Chh.Infrastructure.Persistence;
using Chh.Infrastructure.Persistence.Encryption;
using Chh.Infrastructure.Persistence.Repositories;
using Chh.Infrastructure.Security;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
        services.AddJwt(configuration);
        services.AddCorsPolicy(configuration);

        services.AddValidatorsFromAssembly(typeof(OtpRequestRequestValidator).Assembly);

        return services;
    }

    /// <summary>Name of the CORS policy registered by <see cref="AddCorsPolicy"/>, applied via <c>app.UseCors</c> in <c>Program.cs</c>.</summary>
    public const string FrontendCorsPolicy = "FrontendCorsPolicy";

    /// <summary>
    /// Registers a CORS policy allowing only the configured frontend origin(s) — the Vercel
    /// deployment(s) plus local dev — to call this API from a browser (root CLAUDE.md Decisions
    /// Log 2026-09-05: split-cloud Vercel/AWS deployment requires explicit CORS). No wildcard
    /// origin: every origin is listed explicitly in <c>Cors:AllowedOrigins</c>.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">App configuration.</param>
    private static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(FrontendCorsPolicy, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    /// <summary>
    /// Registers JWT issuance (<see cref="IJwtTokenGenerator"/>) and the JWT Bearer authentication
    /// scheme used by <c>[Authorize]</c> on every endpoint except the two OTP endpoints
    /// (api-standards.md §5, CHH-F01 AC3). Both sides — issuing and validating — share the same
    /// signing key and <c>Issuer</c>/<c>Audience</c>/lifetime configuration.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">App configuration.</param>
    private static void AddJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer is required.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Audience is required.")
            .Validate(o => o.AccessTokenLifetimeMinutes > 0, "Jwt:AccessTokenLifetimeMinutes must be positive.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.SigningKeyBase64), "Jwt:SigningKeyBase64 is required.")
            .Validate(o =>
            {
                try
                {
                    // HMAC-SHA256 needs a key of at least 256 bits (32 bytes) — a shorter key
                    // would fail at first-token-issuance instead of at startup.
                    return Convert.FromBase64String(o.SigningKeyBase64).Length >= 32;
                }
                catch (FormatException)
                {
                    return false;
                }
            }, "Jwt:SigningKeyBase64 must be a base64-encoded key of at least 32 bytes.")
            .ValidateOnStart();

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var signingKeyBase64 = jwtSection["SigningKeyBase64"];
                var signingKey = string.IsNullOrWhiteSpace(signingKeyBase64)
                    ? new byte[32] // Placeholder only so the app can start with the key still unset
                                   // in Development; AddJwt's ValidateOnStart above is what actually
                                   // enforces a real key everywhere else.
                    : Convert.FromBase64String(signingKeyBase64);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(signingKey),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();
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
