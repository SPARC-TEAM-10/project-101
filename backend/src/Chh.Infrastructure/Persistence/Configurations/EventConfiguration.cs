using Chh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chh.Infrastructure.Persistence.Configurations;

/// <summary>EF Core fluent configuration for <see cref="Event"/> (`.claude/rules/db-standards.md`, CHH-38/US-CHH-005-01).</summary>
public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    private const int TitleMaxLength = 100;
    private const int EventTypeMaxLength = 50;
    private const int DescriptionMaxLength = 1000;
    private const int VenueNameMaxLength = 100;
    private const int VenueAddressMaxLength = 250;
    private const int CoordinatorNameMaxLength = 50;
    private const int CoordinatorContactMaxLength = 10;
    private const int StatusMaxLength = 50;

    /// <summary>Configures the <c>Event</c> table mapping.</summary>
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Event");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.FacilityId)
            .IsRequired();
        builder.HasIndex(e => e.FacilityId)
            .HasDatabaseName("IX_Event_FacilityId");

        builder.Property(e => e.Title)
            .HasMaxLength(TitleMaxLength)
            .IsRequired();

        builder.Property(e => e.EventType)
            .HasConversion<string>()
            .HasMaxLength(EventTypeMaxLength)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(DescriptionMaxLength)
            .IsRequired();

        builder.Property(e => e.VenueName)
            .HasMaxLength(VenueNameMaxLength)
            .IsRequired();

        builder.Property(e => e.VenueAddress)
            .HasMaxLength(VenueAddressMaxLength)
            .IsRequired();

        builder.Property(e => e.Latitude)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(e => e.Longitude)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(e => e.StartAtUtc)
            .IsRequired();

        builder.Property(e => e.EndAtUtc)
            .IsRequired();

        builder.Property(e => e.Capacity)
            .IsRequired();

        builder.Property(e => e.CoordinatorName)
            .HasMaxLength(CoordinatorNameMaxLength)
            .IsRequired();

        builder.Property(e => e.CoordinatorContact)
            .HasMaxLength(CoordinatorContactMaxLength)
            .IsRequired();

        builder.Property(e => e.RsvpCutoffAtUtc);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(StatusMaxLength)
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        builder.Property(e => e.UpdatedAtUtc)
            .IsRequired();

        // Backs proximity discovery (CHH-39): "published events, starting after now".
        builder.HasIndex(e => new { e.Status, e.StartAtUtc })
            .HasDatabaseName("IX_Event_Status_StartAtUtc");
    }
}
