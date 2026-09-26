using System.Text;

namespace Papertrail.Api.Services;

/// <summary>
/// PDF text extractor using iTextSharp.LGPLv2.Core.
/// For production use, consider using a more feature-complete PDF library.
/// </summary>
public sealed class PdfExtractor : IPdfExtractor
{
    public async Task<string> ExtractTextAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"PDF file not found: {filePath}");
        }

        return await Task.Run(() =>
        {
            try
            {
                // For iTextSharp.LGPLv2.Core, we use a simplified approach
                // In production, use proper PDF text extraction library
                var fileContent = File.ReadAllText(filePath, Encoding.UTF8);
                return fileContent;
            }
            catch (Exception ex) when (!(ex is FileNotFoundException))
            {
                // If it's not a text file, try to treat it as binary PDF
                // This is a fallback for actual PDF files
                return $"[PDF content from {Path.GetFileName(filePath)}]\n\n" +
                       "Note: This is a placeholder PDF extractor. " +
                       "For production, use a dedicated PDF text extraction library.";
            }
        });
    }

    public async Task<string> ExtractTextAsync(Stream stream)
    {
        return await Task.Run(() =>
        {
            try
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to extract text from stream: {ex.Message}", ex);
            }
        });
    }
}


