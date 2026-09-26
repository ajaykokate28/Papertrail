using Papertrail.Api.Models;

namespace Papertrail.Api.Persistence;

public interface IHistoryRepository
{
    Task AddAsync(GenerateRequest request, GenerateResponse response, CancellationToken cancellationToken);

    Task<IReadOnlyList<GenerationHistoryResponse>> GetAsync(
        string? conversationId,
        int limit,
        CancellationToken cancellationToken);
}

public sealed record GenerationHistoryResponse(
    string RequestId,
    string? ConversationId,
    string Prompt,
    string Answer,
    DateTimeOffset CreatedAt);