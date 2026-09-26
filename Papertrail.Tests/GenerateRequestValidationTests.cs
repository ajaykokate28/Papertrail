using System.ComponentModel.DataAnnotations;
using Papertrail.Api.Models;

namespace Papertrail.Tests;

public sealed class GenerateRequestValidationTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsMissingPrompt(string prompt)
    {
        var request = new GenerateRequest { Prompt = prompt };

        Assert.False(Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            new List<ValidationResult>(),
            validateAllProperties: true));
    }

    [Fact]
    public void Validate_RejectsPromptOverMaximumLength()
    {
        var request = new GenerateRequest { Prompt = new string('x', 4001) };

        Assert.False(Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            new List<ValidationResult>(),
            validateAllProperties: true));
    }
}