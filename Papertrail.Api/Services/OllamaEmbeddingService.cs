using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Papertrail.Api.Services;

public sealed class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _embeddingModel;

    public OllamaEmbeddingService(HttpClient httpClient, IOptions<OllamaOptions> optionsAccessor)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _embeddingModel = optionsAccessor.Value.EmbeddingModel ?? "nomic-embed-text";
    }

    public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be empty", nameof(text));
        }

        var request = new
        {
            model = _embeddingModel,
            prompt = text.Trim()
        };

        var jsonContent = JsonSerializer.Serialize(request);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/embed")
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        try
        {
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new OllamaProviderException(
                    $"Embedding service returned {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonResponse = JsonSerializer.Deserialize<EmbeddingResponse>(content);
            
            return jsonResponse?.Embedding ?? throw new OllamaProviderException("No embedding returned from service");
        }
        catch (HttpRequestException ex)
        {
            throw new OllamaProviderException($"Failed to call embedding service: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            throw new OllamaProviderException($"Embedding service request timed out: {ex.Message}");
        }
    }

    public float CosineSimilarity(float[] embedding1, float[] embedding2)
    {
        if (embedding1 == null || embedding2 == null)
        {
            throw new ArgumentNullException(embedding1 == null ? nameof(embedding1) : nameof(embedding2));
        }

        if (embedding1.Length != embedding2.Length)
        {
            throw new ArgumentException("Embeddings must have the same dimension");
        }

        float dotProduct = 0;
        float norm1 = 0;
        float norm2 = 0;

        for (int i = 0; i < embedding1.Length; i++)
        {
            dotProduct += embedding1[i] * embedding2[i];
            norm1 += embedding1[i] * embedding1[i];
            norm2 += embedding2[i] * embedding2[i];
        }

        if (norm1 == 0 || norm2 == 0)
        {
            return 0;
        }

        return dotProduct / (float)(Math.Sqrt(norm1) * Math.Sqrt(norm2));
    }

    private sealed class EmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public float[]? Embedding { get; set; }
    }
}
