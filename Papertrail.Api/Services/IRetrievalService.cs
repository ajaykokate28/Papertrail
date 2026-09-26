namespace Papertrail.Api.Services;

public interface IRetrievalService
{
    /// <summary>
    /// Retrieves relevant context chunks based on a query.
    /// </summary>
    /// <param name="query">User query</param>
    /// <param name="topK">Number of chunks to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Formatted context string combining retrieved chunks</returns>
    Task<string> RetrieveContextAsync(string query, int topK = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the database has any documents loaded.
    /// </summary>
    Task<bool> HasDocumentsAsync();
}
