namespace Papertrail.Api.Services;

public sealed class DocumentProcessor : IDocumentProcessor
{
    private readonly IPdfExtractor _pdfExtractor;
    private readonly ITextChunker _textChunker;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorDatabase _vectorDatabase;
    private int _documentCount;

    public DocumentProcessor(
        IPdfExtractor pdfExtractor,
        ITextChunker textChunker,
        IEmbeddingService embeddingService,
        IVectorDatabase vectorDatabase)
    {
        _pdfExtractor = pdfExtractor ?? throw new ArgumentNullException(nameof(pdfExtractor));
        _textChunker = textChunker ?? throw new ArgumentNullException(nameof(textChunker));
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
        _vectorDatabase = vectorDatabase ?? throw new ArgumentNullException(nameof(vectorDatabase));
    }

    public async Task<int> IngestDocumentsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
        }

        var pdfFiles = Directory.GetFiles(directoryPath, "*.pdf");
        int totalChunks = 0;

        foreach (var pdfFile in pdfFiles)
        {
            var chunks = await IngestDocumentAsync(pdfFile, cancellationToken);
            totalChunks += chunks;
        }

        return totalChunks;
    }

    public async Task<int> IngestDocumentAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        // Extract text from PDF
        var text = await _pdfExtractor.ExtractTextAsync(filePath);

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException($"No text extracted from PDF: {filePath}");
        }

        // Chunk the text
        var chunks = _textChunker.ChunkText(text, chunkSize: 512, overlapSize: 50);

        // Generate embeddings and add to vector database
        int chunkIndex = 0;
        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var embedding = await _embeddingService.GetEmbeddingAsync(chunk, cancellationToken);
                var chunkWithEmbedding = new TextChunkWithEmbedding
                {
                    Text = chunk,
                    Embedding = embedding,
                    Source = Path.GetFileNameWithoutExtension(filePath),
                    ChunkIndex = chunkIndex++
                };

                await _vectorDatabase.AddChunkAsync(chunkWithEmbedding);
            }
            catch (OllamaProviderException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to embed chunk {chunkIndex}: {ex.Message}");
                throw;
            }
        }

        _documentCount++;
        return chunks.Count;
    }

    public Task<int> GetDocumentCountAsync()
    {
        return Task.FromResult(_documentCount);
    }
}
