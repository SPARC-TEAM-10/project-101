using Chh.Application.Contracts;
using Chh.Application.Services;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

public class EventNotificationDispatchServiceTests
{
    private readonly Mock<IEventRsvpRepository> _eventRsvpRepository = new();
    private readonly Mock<ISmsGatewayClient> _smsGatewayClient = new();
    private readonly Mock<ILogger<EventNotificationDispatchService>> _logger = new();
    private readonly EventNotificationDispatchService _sut;

    public EventNotificationDispatchServiceTests()
    {
        _sut = new EventNotificationDispatchService(_eventRsvpRepository.Object, _smsGatewayClient.Object, _logger.Object);
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
    public async Task NotifyUpdatedAsync_SendsSmsToEveryGoingRsvpHolder()
    {
        var evt = MakeEvent();
        _eventRsvpRepository
            .Setup(r => r.GetGoingMobileNumbersAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "9000000001", "9000000002" });

        await _sut.NotifyUpdatedAsync(evt, CancellationToken.None);

        _smsGatewayClient.Verify(s => s.SendMessageAsync("9000000001", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _smsGatewayClient.Verify(s => s.SendMessageAsync("9000000002", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NotifyUpdatedAsync_MessageIncludesVenueAndTime()
    {
        var evt = MakeEvent();
        _eventRsvpRepository
            .Setup(r => r.GetGoingMobileNumbersAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "9000000001" });

        string? sentMessage = null;
        _smsGatewayClient
            .Setup(s => s.SendMessageAsync("9000000001", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, m, _) => sentMessage = m)
            .Returns(Task.CompletedTask);

        await _sut.NotifyUpdatedAsync(evt, CancellationToken.None);

        sentMessage.Should().Contain(evt.VenueName);
        sentMessage.Should().Contain(evt.VenueAddress);
        sentMessage.Should().StartWith("Updated:");
    }

    [Fact]
    public async Task NotifyCancelledAsync_MessageIncludesReason()
    {
        var evt = MakeEvent();
        evt.Status = EventStatus.Cancelled;
        evt.CancellationReason = "The hall is unavailable after storm damage.";
        _eventRsvpRepository
            .Setup(r => r.GetGoingMobileNumbersAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "9000000001" });

        string? sentMessage = null;
        _smsGatewayClient
            .Setup(s => s.SendMessageAsync("9000000001", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, m, _) => sentMessage = m)
            .Returns(Task.CompletedTask);

        await _sut.NotifyCancelledAsync(evt, CancellationToken.None);

        sentMessage.Should().StartWith("Cancelled:");
        sentMessage.Should().Contain(evt.CancellationReason);
    }

    [Fact]
    public async Task NotifyUpdatedAsync_NoRsvpHolders_DoesNotCallSmsGateway()
    {
        var evt = MakeEvent();
        _eventRsvpRepository
            .Setup(r => r.GetGoingMobileNumbersAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<string>());

        await _sut.NotifyUpdatedAsync(evt, CancellationToken.None);

        _smsGatewayClient.Verify(s => s.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task NotifyUpdatedAsync_OneRecipientSmsFailure_StillNotifiesTheRest()
    {
        var evt = MakeEvent();
        _eventRsvpRepository
            .Setup(r => r.GetGoingMobileNumbersAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "9000000001", "9000000002" });
        _smsGatewayClient
            .Setup(s => s.SendMessageAsync("9000000001", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("gateway down"));

        var act = () => _sut.NotifyUpdatedAsync(evt, CancellationToken.None);

        await act.Should().NotThrowAsync();
        _smsGatewayClient.Verify(s => s.SendMessageAsync("9000000002", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
