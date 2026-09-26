namespace Papertrail.Api.Services;

public interface IDocumentProcessor
{
    /// <summary>
    /// Loads PDF documents from a directory and ingests them into the vector database.
    /// </summary>
    /// <param name="directoryPath">Directory containing PDF files</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of chunks ingested</returns>
    Task<int> IngestDocumentsAsync(string directoryPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a single PDF document and ingests it into the vector database.
    /// </summary>
    /// <param name="filePath">Path to PDF file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of chunks ingested</returns>
    Task<int> IngestDocumentAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the number of documents currently loaded.
    /// </summary>
    Task<int> GetDocumentCountAsync();
}
