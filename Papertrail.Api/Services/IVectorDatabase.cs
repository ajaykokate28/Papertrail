namespace Papertrail.Api.Services;

public sealed class TextChunkWithEmbedding
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public string Source { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
}

public interface IVectorDatabase
{
    /// <summary>
    /// Adds a text chunk with its embedding to the vector database.
    /// </summary>
    Task AddChunkAsync(TextChunkWithEmbedding chunk);

    /// <summary>
    /// Searches for chunks similar to the query embedding.
    /// </summary>
    /// <param name="queryEmbedding">Query embedding vector</param>
    /// <param name="topK">Number of results to return</param>
    /// <param name="similarityThreshold">Minimum similarity score (0-1)</param>
    /// <returns>List of chunks sorted by similarity (highest first)</returns>
    Task<List<TextChunkWithEmbedding>> SearchAsync(float[] queryEmbedding, int topK = 5, float similarityThreshold = 0.3f);

    /// <summary>
    /// Clears all chunks from the vector database.
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// Gets the total number of chunks in the database.
    /// </summary>
    Task<int> GetCountAsync();
}
