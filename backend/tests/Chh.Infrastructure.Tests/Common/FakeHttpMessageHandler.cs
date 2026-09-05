using System.Net;

namespace Chh.Infrastructure.Tests.Common;

/// <summary>
/// Minimal <see cref="HttpMessageHandler"/> test double: returns a fixed response and captures the
/// last outgoing request (so tests can assert on the built URI) without hitting the network or
/// needing Moq's protected-member setup for every call site.
/// </summary>
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseBody;

    /// <summary>The most recent request this handler received.</summary>
    public HttpRequestMessage? LastRequest { get; private set; }

    /// <summary>Creates the handler, always returning the given status and body.</summary>
    /// <param name="statusCode">The status code every response carries.</param>
    /// <param name="responseBody">The body every response carries.</param>
    public FakeHttpMessageHandler(HttpStatusCode statusCode, string responseBody)
    {
        _statusCode = statusCode;
        _responseBody = responseBody;
    }

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseBody)
        });
    }
}
