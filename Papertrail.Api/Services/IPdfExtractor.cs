namespace Papertrail.Api.Services;

public interface IPdfExtractor
{
    /// <summary>
    /// Extracts text from a PDF file.
    /// </summary>
    /// <param name="filePath">Path to the PDF file</param>
    /// <returns>Extracted text content</returns>
    Task<string> ExtractTextAsync(string filePath);

    /// <summary>
    /// Extracts text from a PDF stream.
    /// </summary>
    /// <param name="stream">Stream containing PDF data</param>
    /// <returns>Extracted text content</returns>
    Task<string> ExtractTextAsync(Stream stream);
}
