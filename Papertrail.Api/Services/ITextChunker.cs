namespace Papertrail.Api.Services;

public interface ITextChunker
{
    /// <summary>
    /// Chunks text into smaller pieces with optional overlap.
    /// </summary>
    /// <param name="text">Text to chunk</param>
    /// <param name="chunkSize">Size of each chunk in characters</param>
    /// <param name="overlapSize">Overlap between consecutive chunks</param>
    /// <returns>List of text chunks</returns>
    List<string> ChunkText(string text, int chunkSize = 512, int overlapSize = 50);
}
