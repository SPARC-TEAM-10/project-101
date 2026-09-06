using Chh.Domain.Entities;
using Chh.Infrastructure.Persistence.Encryption;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chh.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core fluent configuration for <see cref="IndividualProfile"/> (`.claude/rules/db-standards.md`).
/// A plain static method rather than <c>IEntityTypeConfiguration&lt;T&gt;</c> — the encrypted
/// columns need an <see cref="IFieldEncryptor"/> instance, which
/// <c>modelBuilder.ApplyConfigurationsFromAssembly</c> has no way to inject (it requires a
/// parameterless constructor); <see cref="ChhDbContext.OnModelCreating"/> calls this directly instead.
/// </summary>
public static class IndividualProfileConfiguration
{
    private const int MobileNumberMaxLength = 10;
    private const int FullNameMaxLength = 50;
    private const int EmailMaxLength = 254;
    private const int BloodGroupMaxLength = 50;
    private const int GenderMaxLength = 50;
    private const int LocationMaxLength = 200;
    private const int EncryptedBoolMaxLength = 128;
    private const int EncryptedDateMaxLength = 128;
    private const int EncryptedOtherIllnessDetailsMaxLength = 512;

    /// <summary>Configures the <c>IndividualProfile</c> table mapping.</summary>
    /// <param name="builder">The entity type builder for <see cref="IndividualProfile"/>.</param>
    /// <param name="fieldEncryptor">Encryptor backing the PII/health-data columns (db-standards.md §3).</param>
    public static void Configure(EntityTypeBuilder<IndividualProfile> builder, IFieldEncryptor fieldEncryptor)
    {
        builder.ToTable("IndividualProfile");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.MobileNumber)
            .HasMaxLength(MobileNumberMaxLength)
            .IsRequired();
        builder.HasIndex(e => e.MobileNumber)
            .IsUnique()
            .HasDatabaseName("IX_IndividualProfile_MobileNumber");

        builder.Property(e => e.FullName)
            .HasMaxLength(FullNameMaxLength)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasMaxLength(EmailMaxLength)
            .IsRequired();

        builder.Property(e => e.BloodGroup)
            .HasConversion<string>()
            .HasMaxLength(BloodGroupMaxLength)
            .IsRequired();

        builder.Property(e => e.Gender)
            .HasConversion<string>()
            .HasMaxLength(GenderMaxLength)
            .IsRequired();

        builder.Property(e => e.LocationCityArea)
            .HasMaxLength(LocationMaxLength)
            .IsRequired();

        // PII/health-screening columns — AES-256 at rest, never plaintext (db-standards.md §3).
        builder.Property(e => e.DateOfBirth)
            .HasConversion(new EncryptedDateOnlyConverter(fieldEncryptor))
            .HasMaxLength(EncryptedDateMaxLength)
            .IsRequired();

        builder.Property(e => e.IsChronicIllness)
            .HasConversion(new EncryptedBoolConverter(fieldEncryptor))
            .HasMaxLength(EncryptedBoolMaxLength)
            .IsRequired();

        builder.Property(e => e.HasRecentSurgery)
            .HasConversion(new EncryptedBoolConverter(fieldEncryptor))
            .HasMaxLength(EncryptedBoolMaxLength)
            .IsRequired();

        builder.Property(e => e.IsInfectiousDisease)
            .HasConversion(new EncryptedBoolConverter(fieldEncryptor))
            .HasMaxLength(EncryptedBoolMaxLength)
            .IsRequired();

        builder.Property(e => e.IsUnderweight)
            .HasConversion(new EncryptedBoolConverter(fieldEncryptor))
            .HasMaxLength(EncryptedBoolMaxLength)
            .IsRequired();

        builder.Property(e => e.IsOtherIllness)
            .HasConversion(new EncryptedBoolConverter(fieldEncryptor))
            .HasMaxLength(EncryptedBoolMaxLength)
            .IsRequired();

        builder.Property(e => e.OtherIllnessDetails)
            .HasConversion(new EncryptedNullableStringConverter(fieldEncryptor))
            .HasMaxLength(EncryptedOtherIllnessDetailsMaxLength);

        // Derived operational flag — unencrypted by design (db-standards.md §3).
        builder.Property(e => e.IsReceiverOnly)
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();
    }
}
