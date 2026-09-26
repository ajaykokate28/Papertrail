using Papertrail.Api.Models;
using Papertrail.Api.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Papertrail.Tests;

public sealed class HistoryRepositoryTests
{
    [Fact]
    public async Task Repository_AddsAndFiltersHistory()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<PapertrailDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var database = new PapertrailDbContext(options);
        await database.Database.EnsureCreatedAsync();
        var repository = new HistoryRepository(database);
        var request = new GenerateRequest { Prompt = "  question  ", ConversationId = "conversation-a" };
        var response = new GenerateResponse("request-1", "generated", "answer", DateTimeOffset.UtcNow);

        await repository.AddAsync(request, response, CancellationToken.None);

        var history = await repository.GetAsync("conversation-a", 10, CancellationToken.None);

        var item = Assert.Single(history);
        Assert.Equal("question", item.Prompt);
        Assert.Equal("answer", item.Answer);
        Assert.Empty(await repository.GetAsync("conversation-b", 10, CancellationToken.None));
    }
}