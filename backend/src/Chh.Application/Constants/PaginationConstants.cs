namespace Chh.Application.Constants;

/// <summary>
/// Shared paging defaults for any endpoint returning <c>PagedResponse&lt;T&gt;</c> (DOTNET-RULES
/// §12). First introduced by CHH-73's facility-moderation list — reuse these rather than each
/// service defining its own page-size constants.
/// </summary>
public static class PaginationConstants
{
    /// <summary>Default page size when the caller omits <c>pageSize</c>.</summary>
    public const int DefaultPageSize = 20;

    /// <summary>Maximum allowed page size, regardless of what the caller requests.</summary>
    public const int MaxPageSize = 100;
}
