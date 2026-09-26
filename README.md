# Papertrail

A .NET 8 foundation for a Retrieval-Augmented Generation (RAG) application. The application will ingest five research papers, retrieve relevant passages, and answer questions with an open-source language model while retaining the last four conversation interactions.

## Current status

Milestones 1 through 8 are complete:

- Assignment requirements are documented in [IMPLEMENTATION_MILESTONES.md](IMPLEMENTATION_MILESTONES.md).
- .NET SDK 8.0.424 is installed and verified.
- `Papertrail.sln` contains the `Papertrail.Api` ASP.NET Core Web API project.
- The API targets `net8.0`, enables nullable reference types, and includes development Swagger/OpenAPI support.
- `GET /health` reports API availability.
- `POST /api/ai/generate` accepts validated prompts and returns a predictable response contract.
- The generation endpoint calls a local Ollama model through a configurable provider.
- The generation flow uses a reusable RAG prompt template and accepts optional retrieved context through `Context`.
- Ollama responses are parsed and rejected when empty before becoming API responses.
- Successful generated responses are stored in a local SQLite database.
- `GET /api/ai/history` returns recent generation history and supports `conversationId` filtering.
- The `Papertrail.Tests` xUnit project covers validation, prompt construction, Ollama behavior, response formatting, and SQLite history.
- The `Papertrail.Web` Angular frontend provides the chat and history experience.
- The API applies request-size limits, security headers, input checks, and generation rate limiting.

Delivery documentation is available in [docs/API.md](docs/API.md), [docs/SAMPLE_REQUESTS.http](docs/SAMPLE_REQUESTS.http), [docs/EVALUATION.md](docs/EVALUATION.md), [docs/TEST_RESULTS.md](docs/TEST_RESULTS.md), and [docs/FINAL_REPORT.md](docs/FINAL_REPORT.md).

## Prerequisites

- .NET 8 SDK
- An open-source model runtime for later milestones, such as Ollama
- An embedding model and vector store for later milestones

Verify the SDK:

```powershell
dotnet --version
```

## Ollama configuration

Install Ollama, start it, and pull the configured model:

```powershell
ollama pull llama3.2
```

The defaults are suitable for a local Ollama installation. Override them with environment variables when needed:

```powershell
$env:Ollama__BaseUrl = "http://localhost:11434"
$env:Ollama__Model = "llama3.2"
$env:Ollama__TimeoutSeconds = "120"
```

The API returns HTTP 503 when Ollama is unavailable or rejects a request.

Example generation request:

```json
{
	"prompt": "What is self-attention?",
	"conversationId": "demo",
	"context": "Retrieved passages from the indexed research papers go here."
}
```

## Build and run

From the repository root:

```powershell
dotnet restore Papertrail.sln
dotnet build Papertrail.sln --configuration Release
dotnet test Papertrail.sln --configuration Release
dotnet run --project Papertrail.Api\Papertrail.Api.csproj
```

When running in Development, Swagger is available at the URL printed by the application followed by `/swagger`.

## Angular frontend

Start the API on the proxy target port, then run the frontend:

```powershell
dotnet run --project Papertrail.Api\Papertrail.Api.csproj --urls http://localhost:5000
Set-Location Papertrail.Web
npm install
npm start
```

Open `http://localhost:4200`. The Angular development proxy forwards `/api` and `/health` to the .NET API on port 5000. The frontend dependencies must be installed with `npm install` before running or building Angular.

## Persistence

Generation history is stored in `papertrail.db` using EF Core SQLite. The application applies pending migrations at startup. The initial schema migration is located under `Papertrail.Api/Migrations`.

Query recent history:

```text
GET /api/ai/history?conversationId=demo&limit=20
```

The history endpoint accepts limits from 1 through 100 and never stores failed Ollama requests.

## Security controls

Generation requests are limited to 10 per minute and rejected with HTTP 429 after the limit is reached. Request bodies are capped at 1 MB, prompt and context values reject unsupported control characters, and retrieved context is explicitly treated as untrusted reference data by the RAG prompt. Provider configuration is loaded from application configuration or environment variables; no credentials are committed to source.

## Planned RAG workflow

1. Download and extract text from the five required arXiv PDFs.
2. Normalize and split the extracted text into meaningful chunks.
3. Generate embeddings and persist them in a vector database.
4. Retrieve the most relevant chunks for each question.
5. Generate an answer with an open-source model using the question, retrieved context, and the latest four interactions.
6. Evaluate ten predefined questions and document the methodology and results in the final report.

API keys and model credentials must be supplied through user secrets or environment variables. They must never be committed to source control.

## Source papers

- [Attention Is All You Need](https://arxiv.org/pdf/1706.03762.pdf)
- [BERT: Pre-training of Deep Bidirectional Transformers](https://arxiv.org/pdf/1810.04805.pdf)
- [Language Models are Unsupervised Multitask Learners](https://arxiv.org/pdf/2005.14165.pdf)
- [Improving Language Understanding by Generative Pre-Training](https://arxiv.org/pdf/1907.11692.pdf)
- [XLNet: Generalized Autoregressive Pretraining](https://arxiv.org/pdf/1910.10683.pdf)

## RAG Implementation

The RAG (Retrieval-Augmented Generation) components are now fully implemented:

### Quick Start

1. **Setup Documents Folder**
   ```powershell
   mkdir "Papertrail.Api/documents"
   # Add your PDF or text files to this folder
   ```

2. **Start API and Ollama**
   ```powershell
   ollama pull nomic-embed-text  # For embeddings
   ollama serve
   
   # In another terminal:
   dotnet run --project Papertrail.Api\Papertrail.Api.csproj
   ```

3. **Ingest Documents**
   ```powershell
   curl -X POST http://localhost:5000/api/documents/ingest
   ```

4. **Generate with Context Retrieval**
   ```json
   POST /api/ai/generate
   {
     "prompt": "What is the Transformer architecture?",
     "conversationId": "session-1"
   }
   // Context is automatically retrieved and included
   ```

5. **Run Evaluation Suite**
   ```powershell
   curl -X POST http://localhost:5000/api/evaluation/run
   curl -X POST http://localhost:5000/api/evaluation/report
   ```

### New Endpoints

- `POST /api/documents/ingest` - Ingest PDFs from documents folder
- `GET /api/documents/status` - Check ingestion status
- `POST /api/evaluation/run` - Run evaluation on 10 test questions
- `POST /api/evaluation/report` - Generate evaluation report

### New Services

- **IPdfExtractor** - Extract text from PDFs
- **ITextChunker** - Split documents into semantic chunks
- **IEmbeddingService** - Generate vector embeddings via Ollama
- **IVectorDatabase** - In-memory semantic search
- **IRetrievalService** - Retrieve relevant context for queries
- **IConversationMemoryService** - Maintain 4-turn conversation history
- **IEvaluationService** - Run standardized evaluation suite
- **IPdfReportGenerator** - Generate evaluation reports

### Configuration

Ensure Ollama has the embedding model:
```powershell
ollama pull nomic-embed-text
```

Optional environment variable for embedding model:
```powershell
$env:Ollama__EmbeddingModel = "nomic-embed-text"
```

### Evaluation Features

- 10 predefined questions covering the 5 research papers
- Question 10 tests conversation memory by referencing Question 1's answer
- Scoring: Relevance, Accuracy, Contextual Awareness, Response Quality (0-4 each)
- Automatic report generation with summary statistics
- Retrieved context attribution

For detailed RAG documentation, see [docs/RAG_IMPLEMENTATION.md](docs/RAG_IMPLEMENTATION.md).

## Updated Workflow

1. ✅ Download and extract text from five required arXiv PDFs → Place in `documents/` folder
2. ✅ Normalize and split text into meaningful chunks → Handled by `ITextChunker`
3. ✅ Generate embeddings and persist in vector database → Handled by `IEmbeddingService` + `IVectorDatabase`
4. ✅ Retrieve relevant chunks for each question → Handled by `IRetrievalService`
5. ✅ Generate answer with latest four interactions → Handled by `POST /api/ai/generate` with memory service
6. ✅ Evaluate ten questions and document results → Handled by evaluation endpoints