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

public class EventAnalyticsServiceTests
{
    private readonly Mock<IEventRepository> _eventRepository = new();
    private readonly Mock<IEventRsvpRepository> _eventRsvpRepository = new();
    private readonly Mock<IFacilityRepository> _facilityRepository = new();
    private readonly EventAnalyticsService _sut;

    public EventAnalyticsServiceTests()
    {
        _sut = new EventAnalyticsService(_eventRepository.Object, _eventRsvpRepository.Object, _facilityRepository.Object);
    }

    private const string OwnerMobileNumber = "9876543210";
    private const string OtherFacilityMobileNumber = "9000000002";

    private static Event MakeEvent(Guid facilityId, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, int notifiedCount = 0) => new()
    {
        FacilityId = facilityId,
        Title = "Community blood drive — Kaloor",
        EventType = EventType.BloodDonationCamp,
        Description = "Walk-in donors welcome.",
        VenueName = "Kaloor Community Hall",
        VenueAddress = "Stadium Link Road, Kaloor, Kochi 682017",
        Latitude = 9.996m,
        Longitude = 76.299m,
        StartAtUtc = startAtUtc,
        EndAtUtc = endAtUtc,
        Capacity = 60,
        CoordinatorName = "Dr Anitha Varghese",
        CoordinatorContact = "9000010023",
        Status = EventStatus.Published,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow,
        NotifiedCount = notifiedCount
    };

    private static Facility MakeFacility() => new()
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

    private static EventRsvpWithProfileResult MakeParticipant(Guid eventId, EventRsvpStatus status, string fullName = "Nithya Menon", string mobile = "9000000001") => new()
    {
        EventRsvp = new EventRsvp
        {
            EventId = eventId,
            IndividualProfileId = Guid.NewGuid(),
            ReferenceCode = "A1",
            Status = status,
            CreatedAtUtc = DateTimeOffset.UtcNow
        },
        FullName = fullName,
        MobileNumber = mobile
    };

    // --- GetSummaryAsync ---

    [Fact]
    public async Task GetSummaryAsync_ReturnsNull_WhenEventDoesNotExist()
    {
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventWithFacilityNameResult?)null);

        var result = await _sut.GetSummaryAsync(OwnerMobileNumber, Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSummaryAsync_WhenCallerDoesNotOwnEvent_ThrowsEventNotOwnedByCallerException()
    {
        var facility = MakeFacility();
        var evt = MakeEvent(Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(-6), DateTimeOffset.UtcNow.AddHours(-1));
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(OtherFacilityMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        var act = () => _sut.GetSummaryAsync(OtherFacilityMobileNumber, evt.Id, CancellationToken.None);

        await act.Should().ThrowAsync<EventNotOwnedByCallerException>();
    }

    [Fact]
    public async Task GetSummaryAsync_BeforeEventEnds_NoShowCountIsZero()
    {
        var facility = MakeFacility();
        // Started but not yet ended.
        var evt = MakeEvent(facility.Id, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow.AddHours(4), notifiedCount: 340);
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(OwnerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);
        _eventRsvpRepository
            .Setup(r => r.GetAllWithProfileAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                MakeParticipant(evt.Id, EventRsvpStatus.Attended),
                MakeParticipant(evt.Id, EventRsvpStatus.Going),
                MakeParticipant(evt.Id, EventRsvpStatus.Cancelled),
            });

        var result = await _sut.GetSummaryAsync(OwnerMobileNumber, evt.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.AttendedCount.Should().Be(1);
        result.NoShowCount.Should().Be(0);
        result.CancelledCount.Should().Be(1);
        result.RsvpdCount.Should().Be(2);
        result.NotifiedCount.Should().Be(340);
        result.RemainingCapacity.Should().Be(58);
        result.AttendanceRatePercent.Should().Be(50);
    }

    [Fact]
    public async Task GetSummaryAsync_AfterEventEnds_GoingRsvpsCountAsNoShow()
    {
        var facility = MakeFacility();
        var evt = MakeEvent(facility.Id, DateTimeOffset.UtcNow.AddHours(-6), DateTimeOffset.UtcNow.AddHours(-1));
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(OwnerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);
        _eventRsvpRepository
            .Setup(r => r.GetAllWithProfileAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                MakeParticipant(evt.Id, EventRsvpStatus.Attended),
                MakeParticipant(evt.Id, EventRsvpStatus.Going),
                MakeParticipant(evt.Id, EventRsvpStatus.Going),
            });

        var result = await _sut.GetSummaryAsync(OwnerMobileNumber, evt.Id, CancellationToken.None);

        result!.AttendedCount.Should().Be(1);
        result.NoShowCount.Should().Be(2);
        result.RsvpdCount.Should().Be(3);
    }

    // --- GetParticipantsAsync ---

    [Fact]
    public async Task GetParticipantsAsync_FiltersByNoShowStatus_AfterEventEnds()
    {
        var facility = MakeFacility();
        var evt = MakeEvent(facility.Id, DateTimeOffset.UtcNow.AddHours(-6), DateTimeOffset.UtcNow.AddHours(-1));
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(OwnerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);
        _eventRsvpRepository
            .Setup(r => r.GetAllWithProfileAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                MakeParticipant(evt.Id, EventRsvpStatus.Attended, "Rahul Suresh"),
                MakeParticipant(evt.Id, EventRsvpStatus.Going, "Tom Jacob"),
            });

        var result = await _sut.GetParticipantsAsync(OwnerMobileNumber, evt.Id, AttendanceViewStatus.NoShow, null, CancellationToken.None);

        result.Should().ContainSingle(p => p.FullName == "Tom Jacob" && p.Status == AttendanceViewStatus.NoShow);
    }

    [Fact]
    public async Task GetParticipantsAsync_FiltersBySearchTerm()
    {
        var facility = MakeFacility();
        var evt = MakeEvent(facility.Id, DateTimeOffset.UtcNow.AddDays(2), DateTimeOffset.UtcNow.AddDays(2).AddHours(5));
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(OwnerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);
        _eventRsvpRepository
            .Setup(r => r.GetAllWithProfileAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                MakeParticipant(evt.Id, EventRsvpStatus.Going, "Rahul Suresh"),
                MakeParticipant(evt.Id, EventRsvpStatus.Going, "Tom Jacob"),
            });

        var result = await _sut.GetParticipantsAsync(OwnerMobileNumber, evt.Id, null, "rahul", CancellationToken.None);

        result.Should().ContainSingle(p => p.FullName == "Rahul Suresh");
    }

    [Fact]
    public async Task GetParticipantsAsync_ReturnsNull_WhenEventDoesNotExist()
    {
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventWithFacilityNameResult?)null);

        var result = await _sut.GetParticipantsAsync(OwnerMobileNumber, Guid.NewGuid(), null, null, CancellationToken.None);

        result.Should().BeNull();
    }

    // --- ExportCsvAsync ---

    [Fact]
    public async Task ExportCsvAsync_IncludesHeaderAndMaskedMobileNumber_NotRawMobileNumber()
    {
        var facility = MakeFacility();
        var evt = MakeEvent(facility.Id, DateTimeOffset.UtcNow.AddDays(2), DateTimeOffset.UtcNow.AddDays(2).AddHours(5));
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(OwnerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);
        _eventRsvpRepository
            .Setup(r => r.GetAllWithProfileAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { MakeParticipant(evt.Id, EventRsvpStatus.Going, "Rahul Suresh", "9000000001") });

        var csv = await _sut.ExportCsvAsync(OwnerMobileNumber, evt.Id, CancellationToken.None);

        csv.Should().NotBeNull();
        csv.Should().Contain("Full Name,Reference Code,Mobile Number");
        csv.Should().Contain("Rahul Suresh");
        csv.Should().Contain("+91 ••••••0001");
        csv.Should().NotContain("9000000001");
    }

    [Fact]
    public async Task ExportCsvAsync_ReturnsNull_WhenEventDoesNotExist()
    {
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventWithFacilityNameResult?)null);

        var csv = await _sut.ExportCsvAsync(OwnerMobileNumber, Guid.NewGuid(), CancellationToken.None);

        csv.Should().BeNull();
    }
}
