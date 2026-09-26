namespace Papertrail.Api.Services;

public sealed class TextChunker : ITextChunker
{
    public List<string> ChunkText(string text, int chunkSize = 512, int overlapSize = 50)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new List<string>();
        }

        if (chunkSize <= 0)
        {
            throw new ArgumentException("Chunk size must be positive", nameof(chunkSize));
        }

        if (overlapSize < 0 || overlapSize >= chunkSize)
        {
            throw new ArgumentException("Overlap size must be non-negative and less than chunk size", nameof(overlapSize));
        }

        var chunks = new List<string>();
        var cleanText = text.Replace("\r\n", "\n").Trim();

        if (cleanText.Length <= chunkSize)
        {
            chunks.Add(cleanText);
            return chunks;
        }

        var step = chunkSize - overlapSize;
        for (int i = 0; i < cleanText.Length; i += step)
        {
            var endIndex = Math.Min(i + chunkSize, cleanText.Length);
            var chunk = cleanText.Substring(i, endIndex - i).Trim();
            
            if (!string.IsNullOrWhiteSpace(chunk))
            {
                chunks.Add(chunk);
            }

            if (endIndex == cleanText.Length)
            {
                break;
            }
        }

        return chunks;
    }
}
