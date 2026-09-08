using System.Net;
using Chh.Infrastructure.ExternalClients;
using Chh.Infrastructure.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Chh.Infrastructure.Tests.ExternalClients;

public class Fast2SmsGatewayClientTests
{
    private const string MobileNumber = "9999999999";
    private const string OtpCode = "654321";
    private const string SuccessBody = """{"return": true, "request_id": "abc123"}""";

    private static Fast2SmsGatewayClient CreateSut(
        FakeHttpMessageHandler handler, Mock<ILogger<Fast2SmsGatewayClient>>? logger = null)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://www.fast2sms.com/") };
        return new Fast2SmsGatewayClient(httpClient, (logger ?? new Mock<ILogger<Fast2SmsGatewayClient>>()).Object);
    }

    [Fact]
    public async Task SendOtpAsync_BuildsAQuickSmsGetRequestWithTheOtpEmbeddedInTheMessage()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, SuccessBody);
        var sut = CreateSut(handler);

        await sut.SendOtpAsync(MobileNumber, OtpCode, CancellationToken.None);

        var request = handler.LastRequest!;
        request.Method.Should().Be(HttpMethod.Get);
        var uri = request.RequestUri!;
        uri.AbsolutePath.Should().Be("/dev/bulkV2");

        var parsed = QueryHelpers.ParseQuery(uri.Query);
        parsed["route"].ToString().Should().Be("q");
        parsed["numbers"].ToString().Should().Be(MobileNumber);
        parsed["message"].ToString().Should().Contain(OtpCode);
    }

    [Fact]
    public async Task SendOtpAsync_WhenReturnTrue_CompletesSuccessfully()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, SuccessBody);
        var sut = CreateSut(handler);

        var act = () => sut.SendOtpAsync(MobileNumber, OtpCode, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendOtpAsync_WhenReturnFalse_ThrowsHttpRequestException()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """{"return": false, "message": "Invalid Authentication"}""");
        var sut = CreateSut(handler);

        var act = () => sut.SendOtpAsync(MobileNumber, OtpCode, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task SendOtpAsync_WhenHttpStatusIsNotSuccessful_ThrowsHttpRequestException()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.BadGateway, "<html>Bad Gateway</html>");
        var sut = CreateSut(handler);

        var act = () => sut.SendOtpAsync(MobileNumber, OtpCode, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task SendOtpAsync_OnFailure_LogsMaskedMobileNumberNotTheFullNumber()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """{"return": false}""");
        var logger = new Mock<ILogger<Fast2SmsGatewayClient>>();
        var messages = logger.CaptureMessages();
        var sut = CreateSut(handler, logger);

        await Assert.ThrowsAsync<HttpRequestException>(() => sut.SendOtpAsync(MobileNumber, OtpCode, CancellationToken.None));

        var maskedMobileNumber = "********" + MobileNumber.Substring(MobileNumber.Length - 2);
        messages.Should().NotContain(m => m.Contains(MobileNumber));
        messages.Should().Contain(m => m.Contains(maskedMobileNumber));
    }
}
