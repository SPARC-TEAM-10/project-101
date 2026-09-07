using Chh.Domain.Entities;
using Chh.Infrastructure.Persistence.Configurations;
using Chh.Infrastructure.Persistence.Encryption;
using Microsoft.EntityFrameworkCore;

namespace Chh.Infrastructure.Persistence;

/// <summary>The single EF Core <see cref="DbContext"/> for the CHH backend service.</summary>
public class ChhDbContext : DbContext
{
    private readonly IFieldEncryptor _fieldEncryptor;

    /// <summary>Creates the context with the given options (connection string, provider, etc.) and field encryptor.</summary>
    /// <param name="options">EF Core context options.</param>
    /// <param name="fieldEncryptor">Encryptor backing PII/health-data columns (db-standards.md §3).</param>
    public ChhDbContext(DbContextOptions<ChhDbContext> options, IFieldEncryptor fieldEncryptor) : base(options)
    {
        _fieldEncryptor = fieldEncryptor;
    }

    /// <summary>Issued OTP requests (CHH-F01).</summary>
    public DbSet<OtpRequest> OtpRequests { get; set; } = default!;

    /// <summary>Individual registration profiles (CHH-F02).</summary>
    public DbSet<IndividualProfile> IndividualProfiles { get; set; } = default!;

    /// <summary>Blood requests with a search radius for proximity donor matching (CHH-33/US-CHH-004-01).</summary>
    public DbSet<BloodRequest> BloodRequests { get; set; } = default!;

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChhDbContext).Assembly);

        // Not auto-discovered by ApplyConfigurationsFromAssembly — see IndividualProfileConfiguration's
        // own doc comment for why (it needs the encryptor injected, not a parameterless constructor).
        IndividualProfileConfiguration.Configure(modelBuilder.Entity<IndividualProfile>(), _fieldEncryptor);

        base.OnModelCreating(modelBuilder);
    }
}
