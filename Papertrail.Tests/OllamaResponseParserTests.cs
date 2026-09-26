using Papertrail.Api.Services;

namespace Papertrail.Tests;

public sealed class OllamaResponseParserTests
{
    [Fact]
    public void Parse_TrimsValidResponse()
    {
        var answer = new OllamaResponseParser().Parse(new OllamaGenerateResponse("  answer  "));

        Assert.Equal("answer", answer);
    }

    [Fact]
    public void Parse_RejectsEmptyResponse()
    {
        Assert.Throws<OllamaProviderException>(() =>
            new OllamaResponseParser().Parse(new OllamaGenerateResponse(" ")));
    }
}