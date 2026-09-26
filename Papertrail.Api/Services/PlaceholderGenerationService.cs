using Papertrail.Api.Models;

namespace Papertrail.Api.Services;

public sealed class PlaceholderGenerationService : IGenerationService
{
    public Task<GenerateResponse> GenerateAsync(
        GenerateRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var response = new GenerateResponse(
            Guid.NewGuid().ToString("N"),
            "not_configured",
            "The AI generation provider will be configured in Milestone 4.",
            DateTimeOffset.UtcNow);

        return Task.FromResult(response);
    }
}