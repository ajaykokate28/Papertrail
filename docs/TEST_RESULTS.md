# Test Results

Last verified: 2026-08-28

## .NET automated tests

Command:

```powershell
dotnet test Papertrail.sln --configuration Release
```

Result:

```text
Passed: 10
Failed: 0
Skipped: 0
```

Coverage includes request validation, prompt construction, Ollama request and failure behavior, response parsing, and SQLite history persistence.

## Build verification

```powershell
dotnet build Papertrail.sln --configuration Release
```

Result: 0 warnings and 0 errors.

## Manual endpoint checks

- `GET /health`: HTTP 200
- `GET /api/ai/history` on a clean migrated SQLite database: HTTP 200 with `[]`
- Invalid history limit: HTTP 400
- Unavailable Ollama provider: HTTP 503

## Angular verification

The Angular source and workspace diagnostics are clean. A production build requires the frontend dependencies to be installed first:

```powershell
Set-Location Papertrail.Web
npm install
npm run build
```
