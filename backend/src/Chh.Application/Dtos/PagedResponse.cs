namespace Chh.Application.Dtos;

/// <summary>
/// Standard paging envelope for list endpoints (DOTNET-RULES §12). First introduced by CHH-73 —
/// later list endpoints should reuse this rather than inventing another shape.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public record PagedResponse<T>
{
    /// <summary>The page's items.</summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>Total number of items across all pages.</summary>
    public required int TotalCount { get; init; }

    /// <summary>The requested 1-based page number.</summary>
    public required int Page { get; init; }

    /// <summary>The requested page size.</summary>
    public required int PageSize { get; init; }

    /// <summary>Total number of pages, derived from <see cref="TotalCount"/> and <see cref="PageSize"/>.</summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
