namespace Papertrail.Api.Services;

public sealed class ConversationTurn
{
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public interface IConversationMemoryService
{
    /// <summary>
    /// Adds a new turn to the conversation memory.
    /// </summary>
    /// <param name="conversationId">Unique conversation identifier</param>
    /// <param name="role">Role of the speaker ("user" or "assistant")</param>
    /// <param name="message">Message content</param>
    Task AddTurnAsync(string conversationId, string role, string message);

    /// <summary>
    /// Gets the last N turns from a conversation, capped at 4 turns total (2 pairs).
    /// </summary>
    /// <param name="conversationId">Unique conversation identifier</param>
    /// <returns>List of conversation turns, ordered from oldest to newest</returns>
    Task<List<ConversationTurn>> GetRecentTurnsAsync(string conversationId);

    /// <summary>
    /// Clears the conversation history.
    /// </summary>
    /// <param name="conversationId">Unique conversation identifier</param>
    Task ClearAsync(string conversationId);

    /// <summary>
    /// Formats recent turns into a context string for the prompt.
    /// </summary>
    /// <param name="conversationId">Unique conversation identifier</param>
    /// <returns>Formatted conversation context</returns>
    Task<string> GetFormattedContextAsync(string conversationId);
}
