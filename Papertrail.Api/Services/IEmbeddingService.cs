namespace Papertrail.Api.Services;

public interface IEmbeddingService
{
    /// <summary>
    /// Generates embeddings for a given text using the configured embedding model.
    /// </summary>
    /// <param name="text">Text to embed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Float array representing the embedding</returns>
    Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates cosine similarity between two embeddings.
    /// </summary>
    /// <param name="embedding1">First embedding vector</param>
    /// <param name="embedding2">Second embedding vector</param>
    /// <returns>Similarity score between -1 and 1</returns>
    float CosineSimilarity(float[] embedding1, float[] embedding2);
}
