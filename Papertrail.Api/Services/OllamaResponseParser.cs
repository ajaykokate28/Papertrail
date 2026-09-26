namespace Papertrail.Api.Services;

public sealed class OllamaResponseParser : IOllamaResponseParser
{
    public string Parse(OllamaGenerateResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.Response))
        {
            throw new OllamaProviderException("The local Ollama model returned an empty response.");
        }

        return response.Response.Trim();
    }
}