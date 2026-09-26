using System.ComponentModel.DataAnnotations;

namespace Papertrail.Api.Services;

public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    [Range(1, 1000)]
    public int GenerationRequestsPerMinute { get; init; } = 10;
}