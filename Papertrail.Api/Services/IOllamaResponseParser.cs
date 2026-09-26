namespace Papertrail.Api.Services;

public interface IOllamaResponseParser
{
    string Parse(OllamaGenerateResponse response);
}