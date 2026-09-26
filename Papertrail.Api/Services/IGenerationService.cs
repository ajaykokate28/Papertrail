using Papertrail.Api.Models;

namespace Papertrail.Api.Services;

public interface IGenerationService
{
    Task<GenerateResponse> GenerateAsync(
        GenerateRequest request,
        CancellationToken cancellationToken);
}