namespace Chh.Domain.Utilities;

/// <summary>
/// Great-circle distance between two coordinate pairs (US-CHH-004-02/CHH-80 AC2/AC4) — the
/// Haversine formula named explicitly on the Epic's Confluence Data Dictionary
/// (<c>calculated_distance</c>). Pure function, no dependencies.
/// </summary>
public static class HaversineDistanceCalculator
{
    private const double EarthRadiusKm = 6371.0;

    /// <summary>Returns the great-circle distance between two coordinate pairs, in kilometers.</summary>
    /// <param name="latitude1">Latitude of the first point, in degrees.</param>
    /// <param name="longitude1">Longitude of the first point, in degrees.</param>
    /// <param name="latitude2">Latitude of the second point, in degrees.</param>
    /// <param name="longitude2">Longitude of the second point, in degrees.</param>
    public static decimal CalculateDistanceKm(decimal latitude1, decimal longitude1, decimal latitude2, decimal longitude2)
    {
        var lat1Rad = ToRadians((double)latitude1);
        var lat2Rad = ToRadians((double)latitude2);
        var deltaLatRad = ToRadians((double)(latitude2 - latitude1));
        var deltaLonRad = ToRadians((double)(longitude2 - longitude1));

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2)
            + Math.Cos(lat1Rad) * Math.Cos(lat2Rad) * Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return (decimal)(EarthRadiusKm * c);
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
