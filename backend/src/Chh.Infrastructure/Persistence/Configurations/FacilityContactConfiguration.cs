using Chh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chh.Infrastructure.Persistence.Configurations;

/// <summary>EF Core fluent configuration for <see cref="FacilityContact"/> (`.claude/rules/db-standards.md`, CHH-78/US-CHH-003-01).</summary>
public class FacilityContactConfiguration : IEntityTypeConfiguration<FacilityContact>
{
    private const int NameMaxLength = 100;
    private const int DesignationMaxLength = 100;
    private const int MobileNumberMaxLength = 10;

    /// <summary>Configures the <c>FacilityContact</c> table mapping.</summary>
    public void Configure(EntityTypeBuilder<FacilityContact> builder)
    {
        builder.ToTable("FacilityContact");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.FacilityId)
            .IsRequired();
        builder.HasIndex(e => e.FacilityId)
            .HasDatabaseName("IX_FacilityContact_FacilityId");

        builder.Property(e => e.Name)
            .HasMaxLength(NameMaxLength)
            .IsRequired();

        builder.Property(e => e.Designation)
            .HasMaxLength(DesignationMaxLength)
            .IsRequired();

        builder.Property(e => e.Mobile)
            .HasMaxLength(MobileNumberMaxLength)
            .IsRequired();

        builder.Property(e => e.SortOrder)
            .IsRequired();
    }
}
