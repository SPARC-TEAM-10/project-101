using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
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

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetGoingMobileNumbersAsync(Guid eventId, CancellationToken ct) =>
        await _context.EventRsvps
            .AsNoTracking()
            .Where(r => r.EventId == eventId && r.Status == EventRsvpStatus.Going)
            .Join(
                _context.IndividualProfiles.AsNoTracking(),
                r => r.IndividualProfileId,
                p => p.Id,
                (r, p) => p.MobileNumber)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<EventRsvp?> GetTrackedByIdAsync(Guid rsvpId, CancellationToken ct) =>
        await _context.EventRsvps
            .FirstOrDefaultAsync(r => r.Id == rsvpId, ct)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<IReadOnlyList<EventRsvpWithProfileResult>> SearchParticipantsAsync(Guid eventId, string search, CancellationToken ct) =>
        await _context.EventRsvps
            .AsNoTracking()
            .Where(r => r.EventId == eventId && (r.Status == EventRsvpStatus.Going || r.Status == EventRsvpStatus.Attended))
            .Join(
                _context.IndividualProfiles.AsNoTracking(),
                r => r.IndividualProfileId,
                p => p.Id,
                (r, p) => new { Rsvp = r, p.FullName, p.MobileNumber })
            .Where(x => x.MobileNumber == search || x.FullName.ToLower().Contains(search.ToLower()))
            .Select(x => new EventRsvpWithProfileResult { EventRsvp = x.Rsvp, FullName = x.FullName, MobileNumber = x.MobileNumber })
            .ToListAsync(ct)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<IReadOnlyList<EventRsvpWithProfileResult>> GetAllWithProfileAsync(Guid eventId, CancellationToken ct) =>
        await _context.EventRsvps
            .AsNoTracking()
            .Where(r => r.EventId == eventId)
            .Join(
                _context.IndividualProfiles.AsNoTracking(),
                r => r.IndividualProfileId,
                p => p.Id,
                (r, p) => new EventRsvpWithProfileResult { EventRsvp = r, FullName = p.FullName, MobileNumber = p.MobileNumber })
            .ToListAsync(ct)
            .ConfigureAwait(false);
}
