using Hangfire;
using Hangfire.Common;
using Hangfire.States;

namespace Chh.Api.Extensions;

/// <summary>
/// No-op <see cref="IBackgroundJobClient"/> used only under the "Testing" environment (see
/// <see cref="ServiceCollectionExtensions.AddInfrastructureServices"/>) — there is no real
/// Postgres for Hangfire storage there, and no test exercises the actual enqueue side effect.
/// </summary>
public class NoOpBackgroundJobClient : IBackgroundJobClient
{
    /// <inheritdoc />
    public string Create(Job job, IState state) => Guid.NewGuid().ToString();

    /// <inheritdoc />
    public bool ChangeState(string jobId, IState state, string? expectedState) => true;
}
