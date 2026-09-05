using Chh.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chh.Infrastructure.Persistence;

/// <summary>The single EF Core <see cref="DbContext"/> for the CHH backend service.</summary>
public class ChhDbContext : DbContext
{
    /// <summary>Creates the context with the given options (connection string, provider, etc.).</summary>
    public ChhDbContext(DbContextOptions<ChhDbContext> options) : base(options)
    {
    }

    /// <summary>Issued OTP requests (CHH-F01).</summary>
    public DbSet<OtpRequest> OtpRequests { get; set; } = default!;

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChhDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
