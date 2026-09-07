using Chh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chh.Infrastructure.Persistence.Configurations;

/// <summary>EF Core fluent configuration for <see cref="AdminUser"/> (`.claude/rules/db-standards.md`, CHH-F07).</summary>
public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    private const int MobileNumberMaxLength = 10;

    /// <summary>Configures the <c>AdminUser</c> table mapping.</summary>
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("AdminUser");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.MobileNumber)
            .HasMaxLength(MobileNumberMaxLength)
            .IsRequired();
        builder.HasIndex(e => e.MobileNumber)
            .IsUnique()
            .HasDatabaseName("IX_AdminUser_MobileNumber");

        builder.Property(e => e.IsAdmin)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();
    }
}
