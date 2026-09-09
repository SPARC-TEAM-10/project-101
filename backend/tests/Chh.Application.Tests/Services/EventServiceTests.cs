using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Services;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

public class EventServiceTests
{
    private readonly Mock<IFacilityRepository> _facilityRepository = new();
    private readonly Mock<IEventRepository> _eventRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly EventService _sut;

    public EventServiceTests()
    {
        _sut = new EventService(_facilityRepository.Object, _eventRepository.Object, _unitOfWork.Object);
    }

    private static readonly DateTimeOffset Start = DateTimeOffset.UtcNow.AddDays(2);

    private static CreateEventRequest ValidRequest() => new()
    {
        Title = "Community blood drive — Kaloor",
        EventType = EventType.BloodDonationCamp,
        Description = "Walk-in donors welcome. Bring a photo ID. Refreshments provided.",
        VenueName = "Kaloor Community Hall",
        VenueAddress = "Stadium Link Road, Kaloor, Kochi 682017",
        Latitude = 9.996m,
        Longitude = 76.299m,
        StartAtUtc = Start,
        EndAtUtc = Start.AddHours(5),
        Capacity = 60,
        CoordinatorName = "Dr Anitha Varghese",
        CoordinatorContact = "9000010023"
    };

    private static Facility VerifiedFacility() => new()
    {
        FacilityName = "Kochi Metro Hospital",
        Category = FacilityCategory.Hospital,
        SubCategory = FacilitySubCategory.Government,
        LicenseNumber = "KL-HOSP-448120",
        Address = "Marine Drive, Kochi",
        VerificationStatus = FacilityVerificationStatus.Verified,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task CreateAsync_WhenFacilityIsVerified_PersistsAndReturnsDto()
    {
        var facility = VerifiedFacility();
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync("9876543210", It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        var result = await _sut.CreateAsync("9876543210", ValidRequest(), CancellationToken.None);

        result.Title.Should().Be("Community blood drive — Kaloor");
        result.FacilityId.Should().Be(facility.Id);
        result.Status.Should().Be(EventStatus.Published);
        _eventRepository.Verify(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenFacilityIsPending_ThrowsFacilityNotVerifiedException()
    {
        var facility = VerifiedFacility();
        facility.VerificationStatus = FacilityVerificationStatus.Pending;
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync("9876543210", It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        var act = () => _sut.CreateAsync("9876543210", ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<FacilityNotVerifiedException>();
        _eventRepository.Verify(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenCallerHasNoFacility_ThrowsFacilityNotVerifiedException()
    {
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync("9876543210", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Facility?)null);

        var act = () => _sut.CreateAsync("9876543210", ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<FacilityNotVerifiedException>();
        _eventRepository.Verify(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Kochi Metro Hospital's coordinates (search origin for the tests below).
    private const decimal SearchLatitude = 9.9312m;
    private const decimal SearchLongitude = 76.2673m;

    private static Event NearbyEvent(EventType eventType = EventType.BloodDonationCamp, DateTimeOffset? startAtUtc = null) => new()
    {
        FacilityId = Guid.NewGuid(),
        Title = "Community blood drive — Kaloor",
        EventType = eventType,
        Description = "Walk-in donors welcome.",
        VenueName = "Kaloor Community Hall",
        VenueAddress = "Stadium Link Road, Kaloor, Kochi 682017",
        Latitude = 9.996m,
        Longitude = 76.299m,
        StartAtUtc = startAtUtc ?? Start,
        EndAtUtc = (startAtUtc ?? Start).AddHours(5),
        Capacity = 60,
        CoordinatorName = "Dr Anitha Varghese",
        CoordinatorContact = "9000010023",
        Status = EventStatus.Published,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    // Roughly Thrissur — about 60km from the Kochi search origin above, so a 25km search excludes
    // it but a 100km search includes it.
    private static Event FarEvent() => new()
    {
        FacilityId = Guid.NewGuid(),
        Title = "Thrissur health camp",
        EventType = EventType.HealthCamp,
        Description = "General health screening.",
        VenueName = "Thrissur Town Hall",
        VenueAddress = "Round East, Thrissur 680001",
        Latitude = 10.5276m,
        Longitude = 76.2144m,
        StartAtUtc = Start.AddDays(1),
        EndAtUtc = Start.AddDays(1).AddHours(4),
        Capacity = 40,
        CoordinatorName = "Rahul Menon",
        CoordinatorContact = "9876500112",
        Status = EventStatus.Published,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task SearchAsync_ReturnsEventsWithinRadius_SortedByStartTimeAscending()
    {
        var soon = NearbyEvent(startAtUtc: Start);
        var later = NearbyEvent(startAtUtc: Start.AddDays(3));
        _eventRepository
            .Setup(r => r.GetUpcomingPublishedWithFacilityNameAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new EventWithFacilityNameResult { Event = later, FacilityName = "Kochi Metro Hospital" },
                new EventWithFacilityNameResult { Event = soon, FacilityName = "Kochi Metro Hospital" },
            });

        var result = await _sut.SearchAsync(SearchLatitude, SearchLongitude, 25, null, CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].StartAtUtc.Should().Be(soon.StartAtUtc);
        result[1].StartAtUtc.Should().Be(later.StartAtUtc);
    }

    [Fact]
    public async Task SearchAsync_ExcludesEventsOutsideTheRequestedRadius()
    {
        var near = NearbyEvent();
        var far = FarEvent();
        _eventRepository
            .Setup(r => r.GetUpcomingPublishedWithFacilityNameAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new EventWithFacilityNameResult { Event = near, FacilityName = "Kochi Metro Hospital" },
                new EventWithFacilityNameResult { Event = far, FacilityName = "Thrissur NGO" },
            });

        var result = await _sut.SearchAsync(SearchLatitude, SearchLongitude, 25, null, CancellationToken.None);

        result.Should().ContainSingle(r => r.Title == near.Title);
    }

    [Fact]
    public async Task SearchAsync_ClampsAnOutOfRangeRadius_RatherThanRejecting()
    {
        var far = FarEvent();
        _eventRepository
            .Setup(r => r.GetUpcomingPublishedWithFacilityNameAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new EventWithFacilityNameResult { Event = far, FacilityName = "Thrissur NGO" } });

        var result = await _sut.SearchAsync(SearchLatitude, SearchLongitude, 500, null, CancellationToken.None);

        result.Should().ContainSingle(r => r.Title == far.Title, "radiusKm=500 should clamp to 100, not throw or reject");
    }

    [Fact]
    public async Task SearchAsync_FiltersByEventType_WhenProvided()
    {
        var camp = NearbyEvent(eventType: EventType.BloodDonationCamp);
        var health = NearbyEvent(eventType: EventType.HealthCamp);
        _eventRepository
            .Setup(r => r.GetUpcomingPublishedWithFacilityNameAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new EventWithFacilityNameResult { Event = camp, FacilityName = "Kochi Metro Hospital" },
                new EventWithFacilityNameResult { Event = health, FacilityName = "Kochi Metro Hospital" },
            });

        var result = await _sut.SearchAsync(SearchLatitude, SearchLongitude, 25, EventType.HealthCamp, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].EventType.Should().Be(EventType.HealthCamp);
    }

    [Fact]
    public async Task SearchAsync_ReturnsSpotsRemainingEqualToCapacity_NoRsvpEntityYet()
    {
        var evt = NearbyEvent();
        _eventRepository
            .Setup(r => r.GetUpcomingPublishedWithFacilityNameAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" } });

        var result = await _sut.SearchAsync(SearchLatitude, SearchLongitude, 25, null, CancellationToken.None);

        result.Single().SpotsRemaining.Should().Be(evt.Capacity);
    }
}
