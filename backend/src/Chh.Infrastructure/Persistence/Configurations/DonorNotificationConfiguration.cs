using Chh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chh.Infrastructure.Persistence.Configurations;

/// <summary>EF Core fluent configuration for <see cref="DonorNotification"/> (`.claude/rules/db-standards.md`, CHH-34).</summary>
public class DonorNotificationConfiguration : IEntityTypeConfiguration<DonorNotification>
{
    private const int BloodGroupMaxLength = 50;
    private const int UrgencyMaxLength = 50;
    private const int AreaLabelMaxLength = 100;

    /// <summary>Configures the <c>DonorNotification</c> table mapping.</summary>
    public void Configure(EntityTypeBuilder<DonorNotification> builder)
    {
        builder.ToTable("DonorNotification");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.BloodRequestId)
            .IsRequired();

        builder.Property(e => e.DonorProfileId)
            .IsRequired();

        // AC4 (duplicate prevention): at most one notification per (request, donor) pair, enforced
        // at the database level rather than trusting the application-layer existence check alone.
        builder.HasIndex(e => new { e.BloodRequestId, e.DonorProfileId })
            .IsUnique()
            .HasDatabaseName("IX_DonorNotification_BloodRequestId_DonorProfileId");

        // Backs GET /notifications/mine's "newest first, for this donor" query (AC3).
        builder.HasIndex(e => new { e.DonorProfileId, e.CreatedAtUtc })
            .HasDatabaseName("IX_DonorNotification_DonorProfileId_CreatedAtUtc");

        builder.Property(e => e.BloodGroup)
            .HasConversion<string>()
            .HasMaxLength(BloodGroupMaxLength)
            .IsRequired();

        builder.Property(e => e.UnitsRequired)
            .IsRequired();

        builder.Property(e => e.Urgency)
            .HasConversion<string>()
            .HasMaxLength(UrgencyMaxLength)
            .IsRequired();

        builder.Property(e => e.DistanceKm)
            .HasPrecision(9, 3)
            .IsRequired();

        builder.Property(e => e.AreaLabel)
            .HasMaxLength(AreaLabelMaxLength)
            .IsRequired();

        builder.Property(e => e.IsRead)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(e => e.SmsSent)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();
    }
}
