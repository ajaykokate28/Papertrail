namespace Papertrail.Api.Models;

public sealed record GenerateResponse(
    string RequestId,
    string Status,
    string Answer,
    DateTimeOffset GeneratedAt);