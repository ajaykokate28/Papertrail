# API Reference

Base URL: `http://localhost:5000`

## `GET /health`

Returns service availability.

Example response:

```json
{
  "status": "healthy",
  "service": "Papertrail.Api"
}
```

## `POST /api/ai/generate`

Sends a validated question to the configured local Ollama model and stores a successful response in SQLite.

Request:

```json
{
  "prompt": "What is self-attention?",
  "conversationId": "demo",
  "context": "Retrieved passages from the indexed research papers go here."
}
```

Response:

```json
{
  "requestId": "9f1a8de92f25446a9b1a0e9d9b45ab15",
  "status": "generated",
  "answer": "The model response appears here.",
  "generatedAt": "2026-08-28T12:00:00+00:00"
}
```

Possible statuses: `200`, `400` for invalid input, `429` when the generation rate limit is reached, and `503` when Ollama is unavailable or returns an invalid response.

## `GET /api/ai/history`

Returns recent successful generations. `conversationId` is optional. `limit` defaults to 20 and must be between 1 and 100.

```text
GET /api/ai/history?conversationId=demo&limit=20
```

## Configuration

- `Ollama__BaseUrl`: Ollama URL, default `http://localhost:11434`
- `Ollama__Model`: model name, default `llama3.2`
- `Ollama__TimeoutSeconds`: provider timeout, default `120`
- `ConnectionStrings__DefaultConnection`: SQLite connection string
- `Security__GenerationRequestsPerMinute`: generation rate limit, default `10`
