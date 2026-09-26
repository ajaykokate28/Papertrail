# RAG Implementation Guide

This document describes the Retrieval-Augmented Generation (RAG) implementation added to the Papertrail API.

## Overview

The following components have been implemented to enable full RAG functionality:

1. **PDF Text Extraction** - Extract text from PDF documents
2. **Text Chunking** - Split documents into semantic chunks
3. **Embeddings** - Generate vector embeddings using Ollama
4. **Vector Database** - In-memory vector storage for semantic search
5. **Retrieval Service** - Retrieve relevant chunks based on queries
6. **Conversation Memory** - Maintain last 4 conversation turns
7. **Evaluation Harness** - Run standardized evaluation against 10 questions
8. **Report Generation** - Generate evaluation reports

## Architecture

### Service Hierarchy

```
IGenerationService (Ollama)
    ↓
/api/ai/generate (Main endpoint)
    ↓
├─ IRetrievalService (fetches context)
│   ├─ IEmbeddingService (Ollama embeddings)
│   └─ IVectorDatabase (in-memory storage)
│
├─ IConversationMemoryService (4-turn history)
│
└─ IHistoryRepository (SQLite persistence)

IDocumentProcessor (Orchestrates ingestion)
    ├─ IPdfExtractor
    ├─ ITextChunker
    ├─ IEmbeddingService
    └─ IVectorDatabase

IEvaluationService (Runs eval suite)
    └─ IPdfReportGenerator
```

## New API Endpoints

### Document Management

#### POST `/api/documents/ingest`
Ingest PDF documents from the `documents` folder.

**Response:**
```json
{
  "message": "Documents ingested successfully",
  "chunksIngested": 150
}
```

**Setup:**
1. Create a `documents` folder in the API's base directory
2. Add PDF or text files (*.pdf, *.txt)
3. Call this endpoint to ingest

#### GET `/api/documents/status`
Check ingestion status.

**Response:**
```json
{
  "documentsLoaded": 5,
  "chunksIndexed": 250
}
```

### Generation (Enhanced)

#### POST `/api/ai/generate`
Generate responses with automatic context retrieval.

**Request:**
```json
{
  "prompt": "What is attention?",
  "conversationId": "session-123",
  "context": null  // Optional - auto-retrieved if not provided
}
```

**Response:**
```json
{
  "requestId": "...",
  "status": "success",
  "answer": "Attention is...",
  "generatedAt": "2026-08-31T..."
}
```

**Changes:**
- Automatically retrieves relevant context from vector database
- Stores turns in 4-turn conversation memory
- Maintains conversation context across requests

### Evaluation

#### POST `/api/evaluation/run`
Run the complete evaluation suite with all 10 questions.

**Response:**
```json
{
  "message": "Evaluation completed",
  "resultsCount": 10,
  "results": [
    {
      "questionNumber": 1,
      "question": "What problem does the Transformer architecture solve...",
      "retrievedContext": "[Source: Attention_Is_All_You_Need, Chunk 5]...",
      "response": "The Transformer architecture solves...",
      "score": {
        "relevance": 0,
        "accuracy": 0,
        "contextualAwareness": 0,
        "responseQuality": 0,
        "total": 0
      }
    }
    // ... 9 more questions
  ]
}
```

#### POST `/api/evaluation/report`
Run evaluation and generate a text report.

**Response:**
```json
{
  "message": "Report generated successfully",
  "reportPath": "/path/to/report-2026-08-31-12-30-45.txt",
  "summary": {
    "totalQuestions": 10,
    "scoredQuestions": 10,
    "totalScore": 0,
    "maxPossibleScore": 160,
    "averageScore": 0.0,
    "averageRelevance": 0.0,
    "averageAccuracy": 0.0,
    "averageContextualAwareness": 0.8,
    "averageResponseQuality": 0.0
  }
}
```

## Service Implementations

### IPdfExtractor / PdfExtractor
Extracts text from PDF and text files.

- Supports file paths and streams
- Falls back to text file reading if PDF parsing fails
- Thread-safe async operations

### ITextChunker / TextChunker
Splits text into overlapping chunks.

- Configurable chunk size (default: 512 characters)
- Configurable overlap (default: 50 characters)
- Preserves text boundaries (no mid-word splits)

### IEmbeddingService / OllamaEmbeddingService
Generates embeddings using Ollama's embedding API.

- Uses configured embedding model (default: `nomic-embed-text`)
- Includes cosine similarity calculation
- Error handling for provider failures

### IVectorDatabase / InMemoryVectorDatabase
In-memory vector storage with similarity search.

- Thread-safe with locking
- Cosine similarity-based search
- Minimum threshold filtering
- Configurable top-K results

### IRetrievalService / RetrievalService
Orchestrates retrieval workflow.

- Converts queries to embeddings
- Searches vector database
- Formats results with source attribution
- Graceful failure (returns empty string if no results)

### IDocumentProcessor / DocumentProcessor
Orchestrates document ingestion.

- Batch processes PDF/text files
- Tracks document and chunk counts
- Detailed error handling
- Progress tracking

### IConversationMemoryService / InMemoryConversationMemoryService
Maintains conversation context.

- Stores last 4 turns (2 user-assistant pairs)
- Thread-safe operations
- Formatted context retrieval
- Per-conversation isolation

### IEvaluationService / EvaluationService
Runs standardized evaluation suite.

- 10 predefined evaluation questions
- Question 10 tests memory by referencing Question 1's answer
- Calculates summary statistics
- Scores: Relevance (0-4), Accuracy (0-4), Contextual Awareness (0-4), Response Quality (0-4)

### IPdfReportGenerator / SimplePdfReportGenerator
Generates text-based reports.

- Outputs formatted text (easily convertible to PDF)
- Includes all question results and scores
- Summary statistics
- Source attribution

## Configuration

### appsettings.json Updates

No additional settings required beyond existing Ollama configuration:

```json
{
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "Model": "llama3.2",
    "EmbeddingModel": "nomic-embed-text",
    "TimeoutSeconds": 120
  }
}
```

### Required Directories

```
Papertrail.Api/
├── documents/          # Create this folder and add PDFs
└── reports/            # Auto-created for evaluation reports
```

## Evaluation Questions

The evaluation suite includes these 10 questions:

1. What problem does the Transformer architecture solve compared with recurrent processing?
2. How does scaled dot-product attention calculate its output?
3. Why does the Transformer use multiple attention heads?
4. What is BERT's bidirectional pre-training objective?
5. How does masked language modeling support BERT's representations?
6. What is the purpose of next sentence prediction in BERT?
7. What distinguishes GPT-2's training setup from task-specific supervised training?
8. How does GPT pre-training transfer to downstream tasks?
9. What is XLNet's permutation language modeling objective?
10. Given a previous answer, can the assistant explain how the cited method relates to the earlier comparison? *(Tests conversation memory)*

## Scoring Rubric

Each answer is scored 0-4 on four dimensions:

- **Relevance**: Directly addresses the question (0-4)
- **Accuracy**: Agrees with source passages (0-4)
- **Contextual Awareness**: Uses recent conversation context when applicable (0-4)
- **Response Quality**: Clear, concise, technically understandable (0-4)

**Maximum Score**: 160 (10 questions × 4 dimensions × 4 points)

## Workflow Example

### 1. Setup
```bash
# Create documents folder
mkdir documents
# Add PDF files (Transformer papers, BERT, GPT-2, XLNet)
```

### 2. Ingest Documents
```bash
curl -X POST http://localhost:5000/api/documents/ingest
```

### 3. Check Status
```bash
curl http://localhost:5000/api/documents/status
# Response: { "documentsLoaded": 5, "chunksIndexed": 250 }
```

### 4. Run Evaluation
```bash
curl -X POST http://localhost:5000/api/evaluation/run
# Returns results for all 10 questions
```

### 5. Generate Report
```bash
curl -X POST http://localhost:5000/api/evaluation/report
# Creates report file in reports/ folder
```

## Performance Considerations

### In-Memory Vector Database
- Current implementation stores all embeddings in memory
- Suitable for ~50K-100K chunks
- For production with millions of chunks, use persistent vector DB (Pinecone, Weaviate, Milvus)

### Embedding Generation
- Each document ingestion calls Ollama's embedding API
- Batch embedding would improve performance
- Consider caching embeddings for frequently-used chunks

### Conversation Memory
- Limited to 4 turns per conversation
- Stored in memory only
- For persistence, store in SQLite (extend `IConversationMemoryService`)

## Future Enhancements

1. **Persistent Vector Database**
   - Replace in-memory storage with SQLite FTS or vector DB
   - Enable durability across restarts

2. **Advanced PDF Extraction**
   - Replace text-file fallback with proper PDF library (PdfPig, PDFium)
   - Preserve formatting and metadata
   - Handle scanned PDFs with OCR

3. **Batch Embedding**
   - Group documents for more efficient embedding generation
   - Reduce API calls to Ollama

4. **Semantic Routing**
   - Route queries to different endpoints based on content
   - Separate retrieval for different document types

5. **Reranking**
   - Add cross-encoder reranking for top-K results
   - Improve precision of retrieved context

6. **HyDE (Hypothetical Document Embeddings)**
   - Generate hypothetical answers for queries
   - Improve semantic matching

7. **Interactive Scoring UI**
   - Web interface for manual evaluation scoring
   - Visual comparison of results

8. **Real PDF Report Generation**
   - Use iText7 or PdfSharp to generate proper PDF reports
   - Include charts, formatting, metadata

## Troubleshooting

### No documents found error
- Ensure `documents/` folder exists in the API's base directory
- Check that PDF files are readable
- Verify Ollama embedding API is accessible

### Low retrieval quality
- Ensure documents are properly ingested
- Check chunk size (current: 512 chars)
- Verify embedding model is suitable for your content
- Consider fine-tuning the similarity threshold

### Embedding API errors
- Verify Ollama is running: `curl http://localhost:11434/api/tags`
- Ensure embedding model is pulled: `ollama pull nomic-embed-text`
- Check timeout settings (current: 120 seconds)

## Source Code Location

All RAG services are located in `Papertrail.Api/Services/`:

- `IPdfExtractor.cs` / `PdfExtractor.cs`
- `ITextChunker.cs` / `TextChunker.cs`
- `IEmbeddingService.cs` / `OllamaEmbeddingService.cs`
- `IVectorDatabase.cs` / `InMemoryVectorDatabase.cs`
- `IRetrievalService.cs` / `RetrievalService.cs`
- `IDocumentProcessor.cs` / `DocumentProcessor.cs`
- `IConversationMemoryService.cs` / `InMemoryConversationMemoryService.cs`
- `IEvaluationService.cs` / `EvaluationService.cs`
- `IPdfReportGenerator.cs`

Program.cs was updated to wire up all services and add new endpoints.
