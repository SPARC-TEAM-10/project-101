using Chh.Infrastructure.ExternalClients;
using FluentAssertions;
using Xunit;

namespace Chh.Infrastructure.Tests.ExternalClients;

public class Fast2SmsMobileNumberNormalizerTests
{
    [Theory]
    [InlineData("+919999999999", "9999999999")]
    [InlineData("91 99999 99999", "9999999999")]
    [InlineData("99999-99999", "9999999999")]
    [InlineData("9999999999", "9999999999")]
    public void Normalize_StripsCountryCodeAndFormatting(string rawMobileNumber, string expected)
    {
        var result = Fast2SmsMobileNumberNormalizer.Normalize(rawMobileNumber);

        result.Should().Be(expected);
    }
}
