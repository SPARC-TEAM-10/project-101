namespace Chh.Domain.Constants;

/// <summary>
/// Shared paging defaults for list endpoints (DOTNET-RULES §12, `.claude/rules/api-standards.md`
/// §6). Centralized here — rather than duplicated as private consts per service — so every
/// paginated endpoint stays consistent, matching the pattern already used for
/// <see cref="OtpConstants"/>.
/// </summary>
public static class PaginationConstants
{
    /// <summary>Default page size when the caller omits <c>pageSize</c>.</summary>
    public const int DefaultPageSize = 20;

    /// <summary>Maximum allowed page size.</summary>
    public const int MaxPageSize = 100;
}
