using Chh.Application.Contracts;
using Chh.Domain.Entities;

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
}
