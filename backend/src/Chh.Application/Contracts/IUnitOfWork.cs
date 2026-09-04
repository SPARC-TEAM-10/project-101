namespace Chh.Application.Contracts;

/// <summary>
/// Persists all pending changes tracked by the current request's <c>DbContext</c> in a single
/// call. Services call this exactly once, at the end of the method, per DOTNET-RULES — repositories
/// must never call <c>SaveChangesAsync</c> themselves.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Persists all pending changes to the database.</summary>
    /// <param name="ct">Cancellation token.</param>
    Task SaveChangesAsync(CancellationToken ct);
}
