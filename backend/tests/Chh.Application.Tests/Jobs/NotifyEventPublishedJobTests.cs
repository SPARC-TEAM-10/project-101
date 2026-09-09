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

public class NotifyEventPublishedJobTests
{
    private readonly Mock<IEventRepository> _eventRepository = new();
    private readonly Mock<IEventPublishNotificationDispatchService> _notificationDispatchService = new();
    private readonly Mock<ILogger<NotifyEventPublishedJob>> _logger = new();
    private readonly NotifyEventPublishedJob _sut;

    public NotifyEventPublishedJobTests()
    {
        _sut = new NotifyEventPublishedJob(_eventRepository.Object, _notificationDispatchService.Object, _logger.Object);
    }

    private static Event MakeEvent(DateTimeOffset startAtUtc, EventStatus status = EventStatus.Published) => new()
    {
        FacilityId = Guid.NewGuid(),
        Title = "Community blood drive — Kaloor",
        EventType = EventType.BloodDonationCamp,
        Description = "Walk-in donors welcome.",
        VenueName = "Kaloor Community Hall",
        VenueAddress = "Stadium Link Road, Kaloor, Kochi 682017",
        Latitude = 9.996m,
        Longitude = 76.299m,
        StartAtUtc = startAtUtc,
        EndAtUtc = startAtUtc.AddHours(5),
        Capacity = 60,
        CoordinatorName = "Dr Anitha Varghese",
        CoordinatorContact = "9000010023",
        Status = status,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task RunAsync_UpcomingPublishedEvent_DispatchesNotification()
    {
        var evt = MakeEvent(DateTimeOffset.UtcNow.AddDays(2));
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });

        await _sut.RunAsync(evt.Id, CancellationToken.None);

        _notificationDispatchService.Verify(d => d.NotifyPublishedAsync(evt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_EventNotFound_DoesNotThrowOrDispatch()
    {
        var eventId = Guid.NewGuid();
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventWithFacilityNameResult?)null);

        var act = () => _sut.RunAsync(eventId, CancellationToken.None);

        await act.Should().NotThrowAsync();
        _notificationDispatchService.Verify(d => d.NotifyPublishedAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_EventAlreadyStarted_DoesNotDispatch()
    {
        var evt = MakeEvent(DateTimeOffset.UtcNow.AddHours(-1));
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });

        await _sut.RunAsync(evt.Id, CancellationToken.None);

        _notificationDispatchService.Verify(d => d.NotifyPublishedAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_EventCancelled_DoesNotDispatch()
    {
        var evt = MakeEvent(DateTimeOffset.UtcNow.AddDays(2), EventStatus.Cancelled);
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });

        await _sut.RunAsync(evt.Id, CancellationToken.None);

        _notificationDispatchService.Verify(d => d.NotifyPublishedAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
