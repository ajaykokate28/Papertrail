namespace Papertrail.Api.Persistence.Entities;

public sealed class GenerationHistoryEntry
{
    public required string RequestId { get; init; }

    public string? ConversationId { get; init; }

    public required string Prompt { get; init; }

    public required string Answer { get; init; }

    public DateTime CreatedAt { get; init; }
}