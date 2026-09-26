namespace Papertrail.Api.Services;

public sealed class InMemoryVectorDatabase : IVectorDatabase
{
    private readonly List<TextChunkWithEmbedding> _chunks = new();
    private readonly object _lockObject = new();

    public Task AddChunkAsync(TextChunkWithEmbedding chunk)
    {
        if (chunk == null)
        {
            throw new ArgumentNullException(nameof(chunk));
        }

        if (chunk.Embedding == null || chunk.Embedding.Length == 0)
        {
            throw new ArgumentException("Chunk must have a valid embedding", nameof(chunk));
        }

        lock (_lockObject)
        {
            _chunks.Add(chunk);
        }

        return Task.CompletedTask;
    }

    public Task<List<TextChunkWithEmbedding>> SearchAsync(float[] queryEmbedding, int topK = 5, float similarityThreshold = 0.3f)
    {
        if (queryEmbedding == null || queryEmbedding.Length == 0)
        {
            throw new ArgumentException("Query embedding cannot be empty", nameof(queryEmbedding));
        }

        if (topK <= 0)
        {
            throw new ArgumentException("topK must be positive", nameof(topK));
        }

        var embeddingService = new OllamaEmbeddingService(null!, null!);
        
        lock (_lockObject)
        {
            var results = _chunks
                .Select(chunk => new
                {
                    Chunk = chunk,
                    Similarity = ComputeSimilarity(queryEmbedding, chunk.Embedding)
                })
                .Where(x => x.Similarity >= similarityThreshold)
                .OrderByDescending(x => x.Similarity)
                .Take(topK)
                .Select(x => x.Chunk)
                .ToList();

            return Task.FromResult(results);
        }
    }

    public Task ClearAsync()
    {
        lock (_lockObject)
        {
            _chunks.Clear();
        }

        return Task.CompletedTask;
    }

    public Task<int> GetCountAsync()
    {
        lock (_lockObject)
        {
            return Task.FromResult(_chunks.Count);
        }
    }

    private static float ComputeSimilarity(float[] embedding1, float[] embedding2)
    {
        if (embedding1.Length != embedding2.Length)
        {
            return 0f;
        }

        float dotProduct = 0;
        float norm1 = 0;
        float norm2 = 0;

        for (int i = 0; i < embedding1.Length; i++)
        {
            dotProduct += embedding1[i] * embedding2[i];
            norm1 += embedding1[i] * embedding1[i];
            norm2 += embedding2[i] * embedding2[i];
        }

        if (norm1 == 0 || norm2 == 0)
        {
            return 0;
        }

        return dotProduct / (float)(Math.Sqrt(norm1) * Math.Sqrt(norm2));
    }
}
