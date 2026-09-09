using Chh.Application.Contracts;
using Chh.Application.Services;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

public class EventPublishNotificationDispatchServiceTests
{
    private readonly Mock<IIndividualProfileRepository> _individualProfileRepository = new();
    private readonly Mock<ISmsGatewayClient> _smsGatewayClient = new();
    private readonly Mock<ILogger<EventPublishNotificationDispatchService>> _logger = new();
    private readonly EventPublishNotificationDispatchService _sut;

    public EventPublishNotificationDispatchServiceTests()
    {
        _sut = new EventPublishNotificationDispatchService(_individualProfileRepository.Object, _smsGatewayClient.Object, _logger.Object);
    }

    // Kochi Metro venue coordinates (matches CHH-39's EventServiceTests fixtures).
    private const decimal VenueLatitude = 9.9312m;
    private const decimal VenueLongitude = 76.2673m;

    private static readonly DateTimeOffset Start = DateTimeOffset.UtcNow.AddDays(2);

    private static Event MakeEvent() => new()
    {
        FacilityId = Guid.NewGuid(),
        Title = "Community blood drive — Kaloor",
        EventType = EventType.BloodDonationCamp,
        Description = "Walk-in donors welcome.",
        VenueName = "Kaloor Community Hall",
        VenueAddress = "Stadium Link Road, Kaloor, Kochi 682017",
        Latitude = VenueLatitude,
        Longitude = VenueLongitude,
        StartAtUtc = Start,
        EndAtUtc = Start.AddHours(5),
        Capacity = 60,
        CoordinatorName = "Dr Anitha Varghese",
        CoordinatorContact = "9000010023",
        Status = EventStatus.Published,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    private static IndividualProfile MakeProfile(string mobileNumber, decimal? latitude, decimal? longitude, AccountStatus status = AccountStatus.Active) => new()
    {
        MobileNumber = mobileNumber,
        FullName = "Jane Doe",
        Email = "jane@example.com",
        BloodGroup = BloodGroup.OPositive,
        DateOfBirth = new DateOnly(1990, 1, 1),
        Gender = Gender.Female,
        LocationCityArea = "Kochi",
        Latitude = latitude,
        Longitude = longitude,
        AccountStatus = status,
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task NotifyPublishedAsync_NotifiesIndividualsWithinRadius()
    {
        var evt = MakeEvent();
        var nearby = MakeProfile("9000000001", VenueLatitude, VenueLongitude);
        _individualProfileRepository
            .Setup(r => r.GetActiveWithKnownLocationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { nearby });

        var targetedCount = await _sut.NotifyPublishedAsync(evt, CancellationToken.None);

        _smsGatewayClient.Verify(s => s.SendMessageAsync("9000000001", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        targetedCount.Should().Be(1);
    }

    [Fact]
    public async Task NotifyPublishedAsync_ExcludesIndividualsOutsideTheNotificationRadius()
    {
        var evt = MakeEvent();
        // Roughly Thrissur — about 60km away, outside the 15km notification radius.
        var far = MakeProfile("9000000002", 10.5276m, 76.2144m);
        _individualProfileRepository
            .Setup(r => r.GetActiveWithKnownLocationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { far });

        await _sut.NotifyPublishedAsync(evt, CancellationToken.None);

        _smsGatewayClient.Verify(s => s.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task NotifyPublishedAsync_ExcludesSuspendedIndividuals()
    {
        var evt = MakeEvent();
        var suspended = MakeProfile("9000000003", VenueLatitude, VenueLongitude, AccountStatus.Suspended);
        _individualProfileRepository
            .Setup(r => r.GetActiveWithKnownLocationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { suspended });

        await _sut.NotifyPublishedAsync(evt, CancellationToken.None);

        _smsGatewayClient.Verify(s => s.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task NotifyPublishedAsync_MessageIncludesEventFactsInOrder()
    {
        var evt = MakeEvent();
        var nearby = MakeProfile("9000000001", VenueLatitude, VenueLongitude);
        _individualProfileRepository
            .Setup(r => r.GetActiveWithKnownLocationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { nearby });

        string? sentMessage = null;
        _smsGatewayClient
            .Setup(s => s.SendMessageAsync("9000000001", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, m, _) => sentMessage = m)
            .Returns(Task.CompletedTask);

        await _sut.NotifyPublishedAsync(evt, CancellationToken.None);

        sentMessage.Should().StartWith("Blood donation camp");
        sentMessage.Should().Contain(evt.VenueName);
        sentMessage.Should().Contain("60 spots left");
        sentMessage.Should().Contain("Reply YES to RSVP, NO to stop.");
    }

    [Fact]
    public async Task NotifyPublishedAsync_OneRecipientSmsFailure_StillNotifiesTheRest()
    {
        var evt = MakeEvent();
        var candidates = new[]
        {
            MakeProfile("9000000001", VenueLatitude, VenueLongitude),
            MakeProfile("9000000002", VenueLatitude, VenueLongitude),
        };
        _individualProfileRepository
            .Setup(r => r.GetActiveWithKnownLocationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);
        _smsGatewayClient
            .Setup(s => s.SendMessageAsync("9000000001", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("gateway down"));

        var result = await _sut.NotifyPublishedAsync(evt, CancellationToken.None);

        _smsGatewayClient.Verify(s => s.SendMessageAsync("9000000002", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        // The failed recipient was still targeted — a delivery failure doesn't shrink the count.
        result.Should().Be(2);
    }
}
