using Chh.Domain.Utilities;
using FluentAssertions;
using Xunit;

namespace Chh.Application.Tests.Utilities;

public class HaversineDistanceCalculatorTests
{
    [Fact]
    public void CalculateDistanceKm_SamePoint_ReturnsZero()
    {
        var distance = HaversineDistanceCalculator.CalculateDistanceKm(9.9312m, 76.2673m, 9.9312m, 76.2673m);

        distance.Should().Be(0m);
    }

    [Fact]
    public void CalculateDistanceKm_KochiAreaPoints_MatchesExpectedDistanceWithinTolerance()
    {
        // Kochi city center vs. a point ~15km away (Confluence AC2 example: "a donor 15km away should be matched").
        var distance = HaversineDistanceCalculator.CalculateDistanceKm(9.9312m, 76.2673m, 10.0500m, 76.3300m);

        distance.Should().BeInRange(13m, 17m);
    }
}
