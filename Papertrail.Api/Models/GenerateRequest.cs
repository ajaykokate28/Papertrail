using System.ComponentModel.DataAnnotations;

namespace Papertrail.Api.Models;

public sealed class GenerateRequest : IValidatableObject
{
    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Prompt { get; init; } = string.Empty;

    [StringLength(100)]
    public string? ConversationId { get; init; }

    [StringLength(12000)]
    public string? Context { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Prompt))
        {
            yield return new ValidationResult(
                "Prompt must contain at least one non-whitespace character.",
                new[] { nameof(Prompt) });
        }

        if (ContainsUnsafeControlCharacter(Prompt))
        {
            yield return new ValidationResult(
                "Prompt contains unsupported control characters.",
                new[] { nameof(Prompt) });
        }

        if (Context is not null && ContainsUnsafeControlCharacter(Context))
        {
            yield return new ValidationResult(
                "Context contains unsupported control characters.",
                new[] { nameof(Context) });
        }
    }

    private static bool ContainsUnsafeControlCharacter(string value)
    {
        return value.Any(character => char.IsControl(character) && character is not ('\r' or '\n' or '\t'));
    }
}