using Papertrail.Api.Models;

namespace Papertrail.Api.Services;

public interface IPromptBuilder
{
    string Build(GenerateRequest request);
}