# Papertrail

## Final Report Source

**Date:** 2026-08-28  
**Implementation:** .NET 8, ASP.NET Core Minimal API, local Ollama, SQLite, Angular

## 1. Overview

This project is a foundation for a retrieval-augmented generation assistant over five research papers. The target workflow extracts paper text, divides it into meaningful chunks, creates embeddings, retrieves relevant passages, and asks an open-source language model to answer grounded questions. The user can work through an Angular interface and review successful generation history.

## 2. Architecture

The solution contains:

- `Papertrail.Api`: ASP.NET Core API and application services.
- `Papertrail.Tests`: xUnit tests for validation, provider behavior, prompt construction, and persistence.
- `Papertrail.Web`: Angular research assistant interface.
- SQLite: local storage for successful generation history.
- Ollama: local open-source model provider.

The generation path validates input, builds a grounded RAG prompt, calls Ollama, validates the response, persists the successful result, and returns a predictable response contract.

## 3. Source Documents

The planned corpus contains:

1. Attention Is All You Need
2. BERT: Pre-training of Deep Bidirectional Transformers
3. Language Models are Unsupervised Multitask Learners
4. Improving Language Understanding by Generative Pre-Training
5. XLNet: Generalized Autoregressive Pretraining

## 4. Technical Implementation

The API targets `net8.0` and uses nullable reference types. Ollama configuration is externalized through application configuration and environment variables. The API uses typed HTTP integration, provider timeouts, sanitized provider errors, EF Core SQLite persistence, an initial migration, request-size limits, rate limiting, security headers, and input checks.

The RAG prompt builder treats retrieved passages as untrusted reference data and instructs the model to ignore embedded instructions. The current `Context` request field is the integration point for the future vector retrieval service.

## 5. Evaluation Method

The evaluation plan defines ten questions across the five papers. Each answer is scored from 0 to 4 for relevance, accuracy, contextual awareness, and response quality. The maximum score is 160. The complete evaluation should be run only after PDF ingestion, embedding retrieval, and four-turn conversational memory are implemented.

See [EVALUATION.md](EVALUATION.md) for the questions and rubric.

## 6. Verification Results

The .NET solution builds with zero warnings and errors. The automated suite contains ten passing tests covering validation, prompt construction, Ollama behavior, response formatting, and SQLite history. Manual checks verified health, migrated history access, invalid history limits, and controlled Ollama-unavailable responses.

See [TEST_RESULTS.md](TEST_RESULTS.md) for the recorded commands and results.

## 7. Known Limitations

- PDF download and text extraction are not implemented yet.
- Embeddings and vector database retrieval are not implemented yet.
- The API currently accepts retrieved context supplied by the caller.
- Four-turn conversational memory is not implemented yet.
- The ten-question evaluation has a plan and rubric but no final RAGAS or model-quality scores yet.
- The Angular production build requires `npm install` before it can be verified in this environment.
- Authentication and per-user history isolation are outside the current assignment slice.

## 8. Conclusion

The project has a tested .NET and Angular delivery foundation with local Ollama integration, secure configuration boundaries, persistence, and documented evaluation criteria. The remaining assignment-critical work is the ingestion and retrieval pipeline, embedding model, and conversational memory. Those components should be completed before publishing final quality scores or treating this source document as the final PDF report.
