using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Jobs;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Jobs;

public class NotifyEventChangeJobTests
{
    private readonly Mock<IEventRepository> _eventRepository = new();
    private readonly Mock<IEventNotificationDispatchService> _notificationDispatchService = new();
    private readonly Mock<ILogger<NotifyEventChangeJob>> _logger = new();
    private readonly NotifyEventChangeJob _sut;

    public NotifyEventChangeJobTests()
    {
        _sut = new NotifyEventChangeJob(_eventRepository.Object, _notificationDispatchService.Object, _logger.Object);
    }

    private static readonly DateTimeOffset Start = DateTimeOffset.UtcNow.AddDays(2);

    private static Event MakeEvent() => new()
    {
        FacilityId = Guid.NewGuid(),
        Title = "Community blood drive — Kaloor",
        EventType = EventType.BloodDonationCamp,
        Description = "Walk-in donors welcome.",
        VenueName = "Kaloor Community Hall",
        VenueAddress = "Stadium Link Road, Kaloor, Kochi 682017",
        Latitude = 9.996m,
        Longitude = 76.299m,
        StartAtUtc = Start,
        EndAtUtc = Start.AddHours(5),
        Capacity = 60,
        CoordinatorName = "Dr Anitha Varghese",
        CoordinatorContact = "9000010023",
        Status = EventStatus.Published,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task RunUpdatedAsync_EventExists_DispatchesUpdateNotification()
    {
        var evt = MakeEvent();
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });

        await _sut.RunUpdatedAsync(evt.Id, CancellationToken.None);

        _notificationDispatchService.Verify(d => d.NotifyUpdatedAsync(evt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunUpdatedAsync_EventNotFound_DoesNotThrowOrDispatch()
    {
        var eventId = Guid.NewGuid();
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventWithFacilityNameResult?)null);

        var act = () => _sut.RunUpdatedAsync(eventId, CancellationToken.None);

        await act.Should().NotThrowAsync();
        _notificationDispatchService.Verify(d => d.NotifyUpdatedAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunCancelledAsync_EventExists_DispatchesCancellationNotification()
    {
        var evt = MakeEvent();
        evt.Status = EventStatus.Cancelled;
        evt.CancellationReason = "The hall is unavailable after storm damage.";
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });

        await _sut.RunCancelledAsync(evt.Id, CancellationToken.None);

        _notificationDispatchService.Verify(d => d.NotifyCancelledAsync(evt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunCancelledAsync_EventNotFound_DoesNotThrowOrDispatch()
    {
        var eventId = Guid.NewGuid();
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventWithFacilityNameResult?)null);

        var act = () => _sut.RunCancelledAsync(eventId, CancellationToken.None);

        await act.Should().NotThrowAsync();
        _notificationDispatchService.Verify(d => d.NotifyCancelledAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
