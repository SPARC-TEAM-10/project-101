using Chh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chh.Infrastructure.Persistence.Configurations;

/// <summary>EF Core fluent configuration for <see cref="OtpRequest"/> (`.claude/rules/db-standards.md`).</summary>
public class OtpRequestConfiguration : IEntityTypeConfiguration<OtpRequest>
{
    private const int MobileNumberMaxLength = 10;
    private const int OtpCodeHashMaxLength = 64;

    /// <summary>Configures the <c>OtpRequest</c> table mapping.</summary>
    public void Configure(EntityTypeBuilder<OtpRequest> builder)
    {
        builder.ToTable("OtpRequest");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.MobileNumber)
            .HasMaxLength(MobileNumberMaxLength)
            .IsRequired();

        builder.Property(e => e.OtpCodeHash)
            .HasMaxLength(OtpCodeHashMaxLength)
            .IsRequired();

        builder.Property(e => e.OtpRequestedAtUtc)
            .IsRequired();

        builder.Property(e => e.OtpExpiresAtUtc)
            .IsRequired();

        builder.Property(e => e.ResendAvailableAtUtc)
            .IsRequired();

        builder.Property(e => e.IsVerified)
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasIndex(e => e.MobileNumber)
            .HasDatabaseName("IX_OtpRequest_MobileNumber");
    }
}
