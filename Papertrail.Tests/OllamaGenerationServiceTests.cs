using System.Net;
using System.Net.Http.Json;
using Papertrail.Api.Models;
using Papertrail.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Papertrail.Tests;

public sealed class OllamaGenerationServiceTests
{
    [Fact]
    public async Task GenerateAsync_SendsRagPromptAndParsesAnswer()
    {
        var handler = new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { response = "  generated answer  " })
        });
        var service = CreateService(handler);

        var result = await service.GenerateAsync(new GenerateRequest
        {
            Prompt = "Question",
            Context = "Evidence"
        }, CancellationToken.None);

        Assert.Equal("generated", result.Status);
        Assert.Equal("generated answer", result.Answer);
        Assert.Contains("Evidence", handler.RequestBody);
        Assert.Contains("Question", handler.RequestBody);
    }

    [Fact]
    public async Task GenerateAsync_MapsProviderFailure()
    {
        var service = CreateService(new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        await Assert.ThrowsAsync<OllamaProviderException>(() => service.GenerateAsync(
            new GenerateRequest { Prompt = "Question" }, CancellationToken.None));
    }

    private static OllamaGenerationService CreateService(HttpMessageHandler handler) => new(
        new HttpClient(handler) { BaseAddress = new Uri("http://localhost:11434") },
        Options.Create(new OllamaOptions()),
        new RagPromptBuilder(),
        new OllamaResponseParser(),
        NullLogger<OllamaGenerationService>.Instance);

    private sealed class StubHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public string RequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return responder(request);
        }
    }
}