using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Papertrail.Api.Models;
using Microsoft.Extensions.Options;

namespace Papertrail.Api.Services;

public sealed class OllamaGenerationService(
    HttpClient httpClient,
    IOptions<OllamaOptions> options,
    IPromptBuilder promptBuilder,
    IOllamaResponseParser responseParser,
    ILogger<OllamaGenerationService> logger) : IGenerationService
{
    public async Task<GenerateResponse> GenerateAsync(
        GenerateRequest request,
        CancellationToken cancellationToken)
    {
        var ollamaRequest = new OllamaGenerateRequest(
            options.Value.Model,
            promptBuilder.Build(request),
            Stream: false);

        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                "/api/generate",
                ollamaRequest,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Ollama returned HTTP status code {StatusCode}.", response.StatusCode);
                throw new OllamaProviderException("The local Ollama model rejected the request.");
            }

            var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken)
                ?? throw new OllamaProviderException("The local Ollama model returned an empty response.");
            var answer = responseParser.Parse(result);

            return new GenerateResponse(
                Guid.NewGuid().ToString("N"),
                "generated",
                answer,
                DateTimeOffset.UtcNow);
        }
        catch (OllamaProviderException)
        {
            throw;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new OllamaProviderException("The local Ollama model timed out.");
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Could not connect to the local Ollama service.");
            throw new OllamaProviderException("The local Ollama service is unavailable.");
        }
        catch (System.Text.Json.JsonException exception)
        {
            logger.LogWarning(exception, "Ollama returned an invalid response.");
            throw new OllamaProviderException("The local Ollama model returned an invalid response.");
        }
    }
}

public sealed class OllamaProviderException(string message) : Exception(message);

internal sealed record OllamaGenerateRequest(
    string Model,
    string Prompt,
    bool Stream);

public sealed record OllamaGenerateResponse(
    [property: JsonPropertyName("response")] string Response);