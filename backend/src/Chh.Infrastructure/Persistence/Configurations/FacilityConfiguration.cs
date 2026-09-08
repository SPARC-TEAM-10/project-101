using Chh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chh.Infrastructure.Persistence.Configurations;

/// <summary>EF Core fluent configuration for <see cref="Facility"/> (`.claude/rules/db-standards.md`, CHH-73/CHH-78).</summary>
public class FacilityConfiguration : IEntityTypeConfiguration<Facility>
{
    private const int FacilityNameMaxLength = 200;
    private const int CategoryMaxLength = 50;
    private const int SubCategoryMaxLength = 50;
    private const int LicenseNumberMaxLength = 100;
    private const int AddressMaxLength = 500;
    private const int VerificationStatusMaxLength = 50;
    private const int LicenseDocumentUrlMaxLength = 500;
    private const int RejectionReasonMaxLength = 500;

    /// <summary>Configures the <c>Facility</c> table mapping.</summary>
    public void Configure(EntityTypeBuilder<Facility> builder)
    {
        builder.ToTable("Facility");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.FacilityName)
            .HasMaxLength(FacilityNameMaxLength)
            .IsRequired();

        builder.Property(e => e.Category)
            .HasConversion<string>()
            .HasMaxLength(CategoryMaxLength)
            .IsRequired();

        builder.Property(e => e.SubCategory)
            .HasConversion<string>()
            .HasMaxLength(SubCategoryMaxLength)
            .IsRequired();

        builder.Property(e => e.LicenseNumber)
            .HasMaxLength(LicenseNumberMaxLength)
            .IsRequired();

        builder.Property(e => e.Address)
            .HasMaxLength(AddressMaxLength)
            .IsRequired();

        builder.Property(e => e.VerificationStatus)
            .HasConversion<string>()
            .HasMaxLength(VerificationStatusMaxLength)
            .IsRequired();

        builder.Property(e => e.LicenseDocumentUrl)
            .HasMaxLength(LicenseDocumentUrlMaxLength);

        builder.Property(e => e.RejectionReason)
            .HasMaxLength(RejectionReasonMaxLength);

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        builder.Property(e => e.UpdatedAtUtc)
            .IsRequired();

        // Backs CHH-73's pending-list query: "facilities in this state, oldest registration first".
        builder.HasIndex(e => new { e.VerificationStatus, e.CreatedAtUtc })
            .HasDatabaseName("IX_Facility_VerificationStatus_CreatedAtUtc");

        builder.HasMany(e => e.Contacts)
            .WithOne()
            .HasForeignKey(c => c.FacilityId)
            .HasConstraintName("FK_FacilityContact_Facility_FacilityId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
