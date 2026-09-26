using Papertrail.Api.Models;
using Papertrail.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Papertrail.Api.Persistence;

public sealed class HistoryRepository(PapertrailDbContext database) : IHistoryRepository
{
    public async Task AddAsync(
        GenerateRequest request,
        GenerateResponse response,
        CancellationToken cancellationToken)
    {
        database.GenerationHistory.Add(new GenerationHistoryEntry
        {
            RequestId = response.RequestId,
            ConversationId = request.ConversationId,
            Prompt = request.Prompt.Trim(),
            Answer = response.Answer,
            CreatedAt = response.GeneratedAt.UtcDateTime
        });

        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GenerationHistoryResponse>> GetAsync(
        string? conversationId,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = database.GenerationHistory
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(conversationId))
        {
            query = query.Where(entry => entry.ConversationId == conversationId.Trim());
        }

        return await query
            .OrderByDescending(entry => entry.CreatedAt)
            .Take(limit)
            .Select(entry => new GenerationHistoryResponse(
                entry.RequestId,
                entry.ConversationId,
                entry.Prompt,
                entry.Answer,
                new DateTimeOffset(entry.CreatedAt, TimeSpan.Zero)))
            .ToListAsync(cancellationToken);
    }
}