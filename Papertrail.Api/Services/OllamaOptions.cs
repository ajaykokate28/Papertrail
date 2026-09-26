using System.ComponentModel.DataAnnotations;

namespace Papertrail.Api.Services;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    [Required, Url]
    public string BaseUrl { get; init; } = "http://localhost:11434";

    [Required, MinLength(1)]
    public string Model { get; init; } = "llama3.2";

    [Range(1, 300)]
    public int TimeoutSeconds { get; init; } = 120;

    [MinLength(1)]
    public string EmbeddingModel { get; init; } = "nomic-embed-text";
}