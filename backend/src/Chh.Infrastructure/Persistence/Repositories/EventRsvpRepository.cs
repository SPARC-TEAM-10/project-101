using Chh.Application.Contracts;
using Chh.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chh.Infrastructure.Persistence.Repositories;

/// <summary>EF Core-backed implementation of <see cref="IEventRsvpRepository"/>.</summary>
public class EventRsvpRepository : IEventRsvpRepository
{
    private readonly ChhDbContext _context;

    /// <summary>Creates the repository with the shared <see cref="ChhDbContext"/>.</summary>
    /// <param name="context">The shared EF Core database context.</param>
    public EventRsvpRepository(ChhDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task AddAsync(EventRsvp rsvp, CancellationToken ct) =>
        await _context.EventRsvps.AddAsync(rsvp, ct).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<EventRsvp?> GetTrackedByEventAndIndividualAsync(Guid eventId, Guid individualProfileId, CancellationToken ct) =>
        await _context.EventRsvps
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.IndividualProfileId == individualProfileId, ct)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<EventRsvp?> GetByEventAndIndividualAsync(Guid eventId, Guid individualProfileId, CancellationToken ct) =>
        await _context.EventRsvps
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.IndividualProfileId == individualProfileId, ct)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<int> CountForEventAsync(Guid eventId, CancellationToken ct) =>
        await _context.EventRsvps
            .AsNoTracking()
            .CountAsync(r => r.EventId == eventId, ct)
            .ConfigureAwait(false);
}
