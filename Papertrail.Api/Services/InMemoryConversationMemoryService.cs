using System.Text;

namespace Papertrail.Api.Services;

public sealed class InMemoryConversationMemoryService : IConversationMemoryService
{
    private readonly Dictionary<string, List<ConversationTurn>> _conversations = new();
    private readonly object _lockObject = new();
    private const int MaxTurns = 4; // 2 user-assistant pairs

    public Task AddTurnAsync(string conversationId, string role, string message)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            throw new ArgumentException("Conversation ID cannot be empty", nameof(conversationId));
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role cannot be empty", nameof(role));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be empty", nameof(message));
        }

        lock (_lockObject)
        {
            if (!_conversations.ContainsKey(conversationId))
            {
                _conversations[conversationId] = new List<ConversationTurn>();
            }

            var turns = _conversations[conversationId];
            turns.Add(new ConversationTurn
            {
                Role = role.ToLower(),
                Message = message,
                Timestamp = DateTime.UtcNow
            });

            // Keep only the last MaxTurns
            if (turns.Count > MaxTurns)
            {
                turns.RemoveRange(0, turns.Count - MaxTurns);
            }
        }

        return Task.CompletedTask;
    }

    public Task<List<ConversationTurn>> GetRecentTurnsAsync(string conversationId)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return Task.FromResult(new List<ConversationTurn>());
        }

        lock (_lockObject)
        {
            if (!_conversations.ContainsKey(conversationId))
            {
                return Task.FromResult(new List<ConversationTurn>());
            }

            return Task.FromResult(new List<ConversationTurn>(_conversations[conversationId]));
        }
    }

    public Task ClearAsync(string conversationId)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return Task.CompletedTask;
        }

        lock (_lockObject)
        {
            _conversations.Remove(conversationId);
        }

        return Task.CompletedTask;
    }

    public async Task<string> GetFormattedContextAsync(string conversationId)
    {
        var turns = await GetRecentTurnsAsync(conversationId);

        if (turns.Count == 0)
        {
            return string.Empty;
        }

        var contextBuilder = new StringBuilder();
        contextBuilder.AppendLine("Previous conversation context:");
        contextBuilder.AppendLine();

        foreach (var turn in turns)
        {
            var roleLabel = turn.Role == "user" ? "User" : "Assistant";
            contextBuilder.AppendLine($"{roleLabel}: {turn.Message}");
            contextBuilder.AppendLine();
        }

        return contextBuilder.ToString();
    }
}
