using System.Text;

namespace Papertrail.Api.Services;

public sealed class RetrievalService : IRetrievalService
{
    private readonly IVectorDatabase _vectorDatabase;
    private readonly IEmbeddingService _embeddingService;

    public RetrievalService(IVectorDatabase vectorDatabase, IEmbeddingService embeddingService)
    {
        _vectorDatabase = vectorDatabase ?? throw new ArgumentNullException(nameof(vectorDatabase));
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
    }

    public async Task<string> RetrieveContextAsync(string query, int topK = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return string.Empty;
        }

        try
        {
            // Get embedding for the query
            var queryEmbedding = await _embeddingService.GetEmbeddingAsync(query, cancellationToken);

            // Search for similar chunks
            var relevantChunks = await _vectorDatabase.SearchAsync(queryEmbedding, topK, similarityThreshold: 0.2f);

            if (relevantChunks.Count == 0)
            {
                return string.Empty;
            }

            // Format the retrieved context
            var contextBuilder = new StringBuilder();
            for (int i = 0; i < relevantChunks.Count; i++)
            {
                var chunk = relevantChunks[i];
                contextBuilder.AppendLine($"[Source: {chunk.Source}, Chunk {chunk.ChunkIndex}]");
                contextBuilder.AppendLine(chunk.Text);
                
                if (i < relevantChunks.Count - 1)
                {
                    contextBuilder.AppendLine();
                }
            }

            return contextBuilder.ToString();
        }
        catch (Exception ex)
        {
            // Log error but don't fail the request - continue without context
            System.Diagnostics.Debug.WriteLine($"Retrieval error: {ex.Message}");
            return string.Empty;
        }
    }

    public async Task<bool> HasDocumentsAsync()
    {
        var count = await _vectorDatabase.GetCountAsync();
        return count > 0;
    }
}
