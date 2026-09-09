using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Chh.Infrastructure.Persistence.Repositories;

/// <summary>EF Core-backed implementation of <see cref="IEventRepository"/>.</summary>
public class EventRepository : IEventRepository
{
    private readonly ChhDbContext _context;

    /// <summary>Creates the repository with the shared <see cref="ChhDbContext"/>.</summary>
    /// <param name="context">The shared EF Core database context.</param>
    public EventRepository(ChhDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task AddAsync(Event calendarEvent, CancellationToken ct) =>
        await _context.Events.AddAsync(calendarEvent, ct).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<IReadOnlyList<EventWithFacilityNameResult>> GetUpcomingPublishedWithFacilityNameAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        return await _context.Events
            .AsNoTracking()
            .Where(e => e.Status == EventStatus.Published && e.StartAtUtc >= now)
            .Join(
                _context.Facilities.AsNoTracking(),
                e => e.FacilityId,
                f => f.Id,
                (e, f) => new EventWithFacilityNameResult { Event = e, FacilityName = f.FacilityName })
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }
}
