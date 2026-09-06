using System.Net;
using Chh.Infrastructure.ExternalClients;
using Chh.Infrastructure.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Chh.Infrastructure.Tests.ExternalClients;

public class Fast2SmsWhatsAppTemplateClientTests
{
    private const string MessageId = "template-123";
    private const string PhoneNumberId = "phone-456";
    private const string MobileNumber = "9999999999";
    private const string SuccessBody = """{"status": true, "request_id": "abc123"}""";

    private static Fast2SmsWhatsAppTemplateClient CreateSut(
        FakeHttpMessageHandler handler, Mock<ILogger<Fast2SmsWhatsAppTemplateClient>>? logger = null)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://www.fast2sms.com/") };
        var options = Options.Create(new Fast2SmsWhatsAppOptions
        {
            PhoneNumberId = PhoneNumberId,
            OtpMessageId = MessageId,
            DonorRequestMessageId = "donor-template"
        });
        return new Fast2SmsWhatsAppTemplateClient(
            httpClient, options, (logger ?? new Mock<ILogger<Fast2SmsWhatsAppTemplateClient>>()).Object);
    }

    [Fact]
    public async Task SendTemplateAsync_BuildsQueryStringWithPercentEncodedPipeSeparator()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, SuccessBody);
        var sut = CreateSut(handler);

        await sut.SendTemplateAsync(MessageId, MobileNumber, new[] { "Rahul", "1234" }, CancellationToken.None);

        var uri = handler.LastRequest!.RequestUri!;
        uri.AbsolutePath.Should().Be("/dev/whatsapp");
        uri.Query.Should().Contain("%7C", "the pipe separator between variables must be percent-encoded on the wire");

        var parsed = QueryHelpers.ParseQuery(uri.Query);
        parsed["message_id"].ToString().Should().Be(MessageId);
        parsed["phone_number_id"].ToString().Should().Be(PhoneNumberId);
        parsed["numbers"].ToString().Should().Be(MobileNumber);
        parsed["variables_values"].ToString().Should().Be("Rahul|1234");
    }

    [Theory]
    [InlineData("+919999999999")]
    [InlineData("91 99999 99999")]
    [InlineData("99999-99999")]
    public async Task SendTemplateAsync_NormalizesMobileNumberTo10DigitsBeforeSending(string rawMobileNumber)
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, SuccessBody);
        var sut = CreateSut(handler);

        await sut.SendTemplateAsync(MessageId, rawMobileNumber, new[] { "1234" }, CancellationToken.None);

        var parsed = QueryHelpers.ParseQuery(handler.LastRequest!.RequestUri!.Query);
        parsed["numbers"].ToString().Should().Be("9999999999");
    }

    [Fact]
    public async Task SendTemplateAsync_WhenStatusTrue_ReturnsRequestId()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, SuccessBody);
        var sut = CreateSut(handler);

        var requestId = await sut.SendTemplateAsync(MessageId, MobileNumber, new[] { "1234" }, CancellationToken.None);

        requestId.Should().Be("abc123");
    }

    [Fact]
    public async Task SendTemplateAsync_WhenStatusFalse_ThrowsHttpRequestException()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """{"status": false, "message": "invalid template"}""");
        var sut = CreateSut(handler);

        var act = () => sut.SendTemplateAsync(MessageId, MobileNumber, new[] { "1234" }, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task SendTemplateAsync_WhenResponseBodyIsNotJson_ThrowsHttpRequestExceptionNotJsonException()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.BadGateway, "<html>Bad Gateway</html>");
        var sut = CreateSut(handler);

        var act = () => sut.SendTemplateAsync(MessageId, MobileNumber, new[] { "1234" }, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task SendTemplateAsync_WhenVariableContainsPipe_RejectsBeforeSendingRequest()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, SuccessBody);
        var sut = CreateSut(handler);

        var act = () => sut.SendTemplateAsync(MessageId, MobileNumber, new[] { "bad|value" }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
        handler.LastRequest.Should().BeNull("a stray '|' must be rejected before any HTTP call is made");
    }

    [Fact]
    public async Task SendTemplateAsync_NeverLogsTheTemplateVariableValue()
    {
        const string otpCode = "654321";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, SuccessBody);
        var logger = new Mock<ILogger<Fast2SmsWhatsAppTemplateClient>>();
        var messages = logger.CaptureMessages();
        var sut = CreateSut(handler, logger);

        await sut.SendTemplateAsync(MessageId, MobileNumber, new[] { otpCode }, CancellationToken.None);

        messages.Should().NotContain(m => m.Contains(otpCode));
    }
}
