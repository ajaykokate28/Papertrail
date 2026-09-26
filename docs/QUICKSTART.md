# Quick Start Guide - RAG Implementation

This guide walks you through getting the RAG system running end-to-end.

## Prerequisites

✅ .NET 8 SDK installed
✅ Ollama installed and running
✅ Source papers downloaded (or text equivalents)

## Step 1: Prepare Your Documents

Create the documents folder and add your research papers:

```powershell
# From repository root
mkdir "Papertrail.Api\documents"

# Copy your PDF files here:
# Papertrail.Api\documents\
#   ├── AttentionIsAllYouNeed.txt (or .pdf)
#   ├── BERT.txt
#   ├── GPT2.txt
#   ├── GPT.txt
#   └── XLNet.txt
```

**Note:** The current implementation includes a text-based extractor that reads .txt files. For proper PDF handling, use dedicated PDF libraries. See [RAG_IMPLEMENTATION.md](RAG_IMPLEMENTATION.md#future-enhancements) for recommendations.

## Step 2: Set Up Ollama

Ensure Ollama is running with both the LLM and embedding models:

```powershell
# Terminal 1: Start Ollama
ollama serve

# Terminal 2: Verify and pull models
ollama list  # Should show available models

# Pull the generation model (if not already present)
ollama pull llama3.2

# Pull the embedding model
ollama pull nomic-embed-text
```

Verify connectivity:
```powershell
curl http://localhost:11434/api/tags
# Should list available models
```

## Step 3: Build and Run the API

```powershell
# From repository root
dotnet restore Papertrail.sln
dotnet build Papertrail.sln --configuration Debug

# Run the API
dotnet run --project Papertrail.Api\Papertrail.Api.csproj
```

The API will start on `http://localhost:5000`. You should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

## Step 4: Ingest Documents

Once the API is running, ingest your documents into the vector database:

```powershell
# Option 1: Using curl
curl -X POST http://localhost:5000/api/documents/ingest

# Option 2: Using Swagger UI
# Navigate to http://localhost:5000/swagger
# Find the "IngestDocuments" endpoint and click "Try it out" → "Execute"
```

**Expected response:**
```json
{
  "message": "Documents ingested successfully",
  "chunksIngested": 342
}
```

If you get an error about the documents folder not existing, create it and add your files.

## Step 5: Verify Ingestion

Check that documents were loaded:

```powershell
curl http://localhost:5000/api/documents/status

# Response:
# {
#   "documentsLoaded": 5,
#   "chunksIndexed": 342
# }
```

## Step 6: Test Generation with Retrieval

Now test the main generate endpoint with automatic context retrieval:

```powershell
curl -X POST http://localhost:5000/api/ai/generate `
  -H "Content-Type: application/json" `
  -d @- << 'EOF'
{
  "prompt": "What is attention in neural networks?",
  "conversationId": "test-session"
}
EOF
```

**Expected response:**
```json
{
  "requestId": "...",
  "status": "success",
  "answer": "Attention in neural networks...",
  "generatedAt": "2026-08-31T..."
}
```

The endpoint automatically:
- Finds relevant document chunks via semantic search
- Passes them as context to the LLM
- Stores the turn in conversation memory

## Step 7: Run Evaluation

Run the standardized evaluation suite:

```powershell
# Run evaluation and get all 10 results
curl -X POST http://localhost:5000/api/evaluation/run

# Run evaluation and generate a report
curl -X POST http://localhost:5000/api/evaluation/report
```

The evaluation report is saved in `Papertrail.Api/reports/`.

### Evaluation Questions

The 10 questions test understanding of:
1. Transformer architecture advantages
2. Scaled dot-product attention
3. Multiple attention heads
4. BERT's bidirectional training
5. Masked language modeling
6. Next sentence prediction
7. GPT-2 training setup
8. GPT transfer learning
9. XLNet's permutation LM
10. Conversation memory (references Question 1's answer)

## Step 8: View Results

Check the generated report:

```powershell
# Find the latest report
Get-ChildItem "Papertrail.Api\reports\" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

# View the report
Get-Content (Get-ChildItem "Papertrail.Api\reports\" | Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName
```

The report includes:
- Summary statistics (total score, averages by dimension)
- All 10 questions and their responses
- Retrieved context for each question
- Individual scores

## Step 9: Multi-Turn Conversation

Test the 4-turn conversation memory:

```powershell
# Turn 1
curl -X POST http://localhost:5000/api/ai/generate `
  -H "Content-Type: application/json" `
  -d '{
    "prompt": "What is the Transformer architecture?",
    "conversationId": "demo-conversation"
  }'

# Turn 2
curl -X POST http://localhost:5000/api/ai/generate `
  -H "Content-Type: application/json" `
  -d '{
    "prompt": "How is it different from RNNs?",
    "conversationId": "demo-conversation"
  }'

# Turn 3
curl -X POST http://localhost:5000/api/ai/generate `
  -H "Content-Type: application/json" `
  -d '{
    "prompt": "What about positional encoding?",
    "conversationId": "demo-conversation"
  }'

# The assistant should maintain context from previous turns
```

## Step 10: Using the Angular Frontend (Optional)

In a new terminal:

```powershell
Set-Location Papertrail.Web
npm install
npm start
```

Navigate to `http://localhost:4200` to use the web interface.

## Troubleshooting

### Documents not ingesting
- **Problem**: "Documents directory not found"
- **Solution**: Create `Papertrail.Api/documents` folder and add files

### Embedding API errors
- **Problem**: "Failed to call embedding service"
- **Solution**: 
  ```powershell
  ollama pull nomic-embed-text
  curl http://localhost:11434/api/tags  # Verify it's listed
  ```

### Generation returns empty results
- **Problem**: "answer": ""
- **Solution**: 
  - Check Ollama is running: `curl http://localhost:11434/api/tags`
  - Verify LLM model is available: `ollama list`
  - Check logs for provider errors

### Low retrieval quality
- **Problem**: Retrieved context doesn't match query
- **Solution**:
  - Verify documents are properly formatted
  - Check that `chunksIndexed > 0` from status endpoint
  - Try more specific queries
  - Adjust chunk size in `TextChunker.cs` (line 11)

### Port already in use
- **Problem**: "Address already in use"
- **Solution**: 
  ```powershell
  # Find and stop other processes
  Get-NetTCPConnection -LocalPort 5000
  ```

## Next Steps

1. **Score the Evaluation Results**: Open the report in `reports/` and manually score each answer (0-4 for each dimension)
2. **Generate PDF Report**: Extend `SimplePdfReportGenerator` to use iText7 for proper PDF output
3. **Production Setup**: 
   - Migrate from in-memory vector DB to persistent storage
   - Use production-grade PDF extraction library
   - Add database persistence for conversation memory
   - Set up authentication and authorization

## For Developers

See [RAG_IMPLEMENTATION.md](RAG_IMPLEMENTATION.md) for:
- Detailed architecture documentation
- Service descriptions and interfaces
- Configuration options
- Performance considerations
- Future enhancement recommendations

## Support

For issues or questions:
1. Check the [RAG_IMPLEMENTATION.md](RAG_IMPLEMENTATION.md#troubleshooting) troubleshooting section
2. Review the [API.md](API.md) for endpoint documentation
3. Check application logs for error details
4. Verify all prerequisites are met and accessible
