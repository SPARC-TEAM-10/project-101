using Chh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chh.Infrastructure.Persistence.Configurations;

/// <summary>EF Core fluent configuration for <see cref="BloodRequest"/> (`.claude/rules/db-standards.md`, CHH-33/US-CHH-004-01).</summary>
public class BloodRequestConfiguration : IEntityTypeConfiguration<BloodRequest>
{
    private const int MobileNumberMaxLength = 10;
    private const int PatientNameMaxLength = 100;
    private const int BloodGroupMaxLength = 50;
    private const int LocationMaxLength = 100;
    private const int UrgencyMaxLength = 50;
    private const int StatusMaxLength = 50;

    /// <summary>Configures the <c>BloodRequest</c> table mapping.</summary>
    public void Configure(EntityTypeBuilder<BloodRequest> builder)
    {
        builder.ToTable("BloodRequest");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.RequesterMobileNumber)
            .HasMaxLength(MobileNumberMaxLength)
            .IsRequired();
        builder.HasIndex(e => e.RequesterMobileNumber)
            .HasDatabaseName("IX_BloodRequest_RequesterMobileNumber");

        builder.Property(e => e.PatientName)
            .HasMaxLength(PatientNameMaxLength)
            .IsRequired();

        builder.Property(e => e.BloodGroup)
            .HasConversion<string>()
            .HasMaxLength(BloodGroupMaxLength)
            .IsRequired();

        builder.Property(e => e.UnitsRequired)
            .IsRequired();

        builder.Property(e => e.LocationCityArea)
            .HasMaxLength(LocationMaxLength)
            .IsRequired();

        builder.Property(e => e.Latitude)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(e => e.Longitude)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(e => e.SearchRadiusKm)
            .IsRequired();

        builder.Property(e => e.Urgency)
            .HasConversion<string>()
            .HasMaxLength(UrgencyMaxLength)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(StatusMaxLength)
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        builder.Property(e => e.ExpiresAtUtc)
            .IsRequired();

        // Backs the donor-matching query (CHH-34+): "open requests, in this state, expiring
        // after now" — not exercised by this story, but cheap to add alongside the table.
        builder.HasIndex(e => new { e.Status, e.ExpiresAtUtc })
            .HasDatabaseName("IX_BloodRequest_Status_ExpiresAtUtc");
    }
}
