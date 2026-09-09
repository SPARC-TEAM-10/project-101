using Chh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chh.Infrastructure.Persistence.Configurations;

/// <summary>EF Core fluent configuration for <see cref="EventRsvp"/> (`.claude/rules/db-standards.md`, CHH-40/US-CHH-005-03).</summary>
public class EventRsvpConfiguration : IEntityTypeConfiguration<EventRsvp>
{
    private const int ReferenceCodeMaxLength = 20;
    private const int StatusMaxLength = 50;

    /// <summary>Configures the <c>EventRsvp</c> table mapping.</summary>
    public void Configure(EntityTypeBuilder<EventRsvp> builder)
    {
        builder.ToTable("EventRsvp");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.EventId)
            .IsRequired();

        builder.Property(e => e.IndividualProfileId)
            .IsRequired();

        // One row per (Event, IndividualProfile) for the row's whole lifetime (see EventRsvp's
        // doc comment) — enforced here, not just in application code.
        builder.HasIndex(e => new { e.EventId, e.IndividualProfileId })
            .IsUnique()
            .HasDatabaseName("IX_EventRsvp_EventId_IndividualProfileId");

        builder.Property(e => e.ReferenceCode)
            .HasMaxLength(ReferenceCodeMaxLength)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(StatusMaxLength)
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        builder.Property(e => e.CancelledAtUtc);
    }
}
