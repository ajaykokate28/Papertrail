using Papertrail.Api.Models;
using Papertrail.Api.Services;

namespace Papertrail.Tests;

public sealed class RagPromptBuilderTests
{
    [Fact]
    public void Build_IncludesQuestionAndRetrievedContext()
    {
        var prompt = new RagPromptBuilder().Build(new GenerateRequest
        {
            Prompt = "What is attention?",
            Context = "Attention maps queries to values."
        });

        Assert.Contains("What is attention?", prompt);
        Assert.Contains("Attention maps queries to values.", prompt);
        Assert.Contains("do not invent facts", prompt);
    }

    [Fact]
    public void Build_UsesFallbackWhenContextIsMissing()
    {
        var prompt = new RagPromptBuilder().Build(new GenerateRequest { Prompt = "Question" });

        Assert.Contains("No retrieved context was provided", prompt);
    }
}