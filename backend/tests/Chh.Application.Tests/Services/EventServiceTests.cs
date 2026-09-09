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
    private readonly Mock<IEventRsvpRepository> _eventRsvpRepository = new();
    private readonly Mock<IIndividualProfileRepository> _individualProfileRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly EventService _sut;

    public EventServiceTests()
    {
        _sut = new EventService(
            _facilityRepository.Object,
            _eventRepository.Object,
            _eventRsvpRepository.Object,
            _individualProfileRepository.Object,
            _unitOfWork.Object);
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
    public async Task SearchAsync_ReturnsSpotsRemainingEqualToCapacity_WhenNoRsvpsExist()
    {
        var evt = NearbyEvent();
        _eventRepository
            .Setup(r => r.GetUpcomingPublishedWithFacilityNameAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" } });

        var result = await _sut.SearchAsync(SearchLatitude, SearchLongitude, 25, null, CancellationToken.None);

        result.Single().SpotsRemaining.Should().Be(evt.Capacity);
    }

    // --- CHH-40/US-CHH-005-03: GetByIdAsync, RsvpAsync, CancelRsvpAsync ---

    private const string CallerMobileNumber = "9876543210";

    private static IndividualProfile MakeProfile() => new()
    {
        MobileNumber = CallerMobileNumber,
        FullName = "Jane Doe",
        Email = "jane@example.com",
        BloodGroup = BloodGroup.OPositive,
        DateOfBirth = new DateOnly(1990, 1, 1),
        Gender = Gender.Female,
        LocationCityArea = "Kochi",
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenEventDoesNotExist()
    {
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventWithFacilityNameResult?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), CallerMobileNumber, null, null, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_IncludesCallerOwnRsvpStatus_WhenTheyHaveRsvpd()
    {
        var evt = NearbyEvent();
        var profile = MakeProfile();
        var rsvp = new EventRsvp
        {
            EventId = evt.Id,
            IndividualProfileId = profile.Id,
            ReferenceCode = "A1",
            Status = EventRsvpStatus.Going,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(CallerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _eventRsvpRepository
            .Setup(r => r.GetByEventAndIndividualAsync(evt.Id, profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rsvp);

        var result = await _sut.GetByIdAsync(evt.Id, CallerMobileNumber, null, null, CancellationToken.None);

        result.Should().NotBeNull();
        result!.MyRsvpStatus.Should().Be(EventRsvpStatus.Going);
        result.MyReferenceCode.Should().Be("A1");
    }

    [Fact]
    public async Task RsvpAsync_ReturnsNull_WhenEventDoesNotExist()
    {
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(CallerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeProfile());
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventWithFacilityNameResult?)null);

        var result = await _sut.RsvpAsync(CallerMobileNumber, Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RsvpAsync_ThrowsEventFullException_WhenNoCapacityRemains()
    {
        var evt = NearbyEvent();
        var profile = MakeProfile();
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(CallerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });
        _eventRsvpRepository
            .Setup(r => r.GetTrackedByEventAndIndividualAsync(evt.Id, profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventRsvp?)null);
        _eventRepository
            .Setup(r => r.TryReserveSpotAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = () => _sut.RsvpAsync(CallerMobileNumber, evt.Id, CancellationToken.None);

        await act.Should().ThrowAsync<EventFullException>();
    }

    [Fact]
    public async Task RsvpAsync_ThrowsAlreadyRsvpdException_WhenCallerAlreadyGoing()
    {
        var evt = NearbyEvent();
        var profile = MakeProfile();
        var existingRsvp = new EventRsvp
        {
            EventId = evt.Id,
            IndividualProfileId = profile.Id,
            ReferenceCode = "A1",
            Status = EventRsvpStatus.Going,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(CallerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });
        _eventRsvpRepository
            .Setup(r => r.GetTrackedByEventAndIndividualAsync(evt.Id, profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRsvp);

        var act = () => _sut.RsvpAsync(CallerMobileNumber, evt.Id, CancellationToken.None);

        await act.Should().ThrowAsync<AlreadyRsvpdException>();
        _eventRepository.Verify(r => r.TryReserveSpotAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RsvpAsync_Succeeds_ReservesSpotAndAssignsAReferenceCode()
    {
        var evt = NearbyEvent();
        var profile = MakeProfile();
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(CallerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });
        _eventRsvpRepository
            .Setup(r => r.GetTrackedByEventAndIndividualAsync(evt.Id, profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventRsvp?)null);
        _eventRepository
            .Setup(r => r.TryReserveSpotAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _eventRsvpRepository
            .Setup(r => r.CountForEventAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var result = await _sut.RsvpAsync(CallerMobileNumber, evt.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(EventRsvpStatus.Going);
        result.ReferenceCode.Should().Be("A6");
        _eventRsvpRepository.Verify(r => r.AddAsync(It.IsAny<EventRsvp>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelRsvpAsync_ReturnsNull_WhenCallerHasNoActiveRsvp()
    {
        var evt = NearbyEvent();
        var profile = MakeProfile();
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(CallerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _eventRsvpRepository
            .Setup(r => r.GetTrackedByEventAndIndividualAsync(evt.Id, profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventRsvp?)null);

        var result = await _sut.CancelRsvpAsync(CallerMobileNumber, evt.Id, CancellationToken.None);

        result.Should().BeNull();
        _eventRepository.Verify(r => r.ReleaseSpotAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelRsvpAsync_Succeeds_ReleasesTheSpot()
    {
        var evt = NearbyEvent();
        var profile = MakeProfile();
        var rsvp = new EventRsvp
        {
            EventId = evt.Id,
            IndividualProfileId = profile.Id,
            ReferenceCode = "A1",
            Status = EventRsvpStatus.Going,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _individualProfileRepository
            .Setup(r => r.GetByMobileNumberAsync(CallerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _eventRsvpRepository
            .Setup(r => r.GetTrackedByEventAndIndividualAsync(evt.Id, profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rsvp);
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });

        var result = await _sut.CancelRsvpAsync(CallerMobileNumber, evt.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(EventRsvpStatus.Cancelled);
        rsvp.Status.Should().Be(EventRsvpStatus.Cancelled);
        rsvp.CancelledAtUtc.Should().NotBeNull();
        _eventRepository.Verify(r => r.ReleaseSpotAsync(evt.Id, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
