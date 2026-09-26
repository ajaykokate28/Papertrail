using System.Text;

namespace Papertrail.Api.Services;

public sealed class EvaluationQuestion
{
    public int QuestionNumber { get; set; }
    public string Question { get; set; } = string.Empty;
    public string? ConversationContext { get; set; } // For question 10
}

public sealed class EvaluationScore
{
    public int Relevance { get; set; } // 0-4
    public int Accuracy { get; set; } // 0-4
    public int ContextualAwareness { get; set; } // 0-4
    public int ResponseQuality { get; set; } // 0-4

    public int Total => Relevance + Accuracy + ContextualAwareness + ResponseQuality;
}

public sealed class EvaluationResult
{
    public int QuestionNumber { get; set; }
    public string Question { get; set; } = string.Empty;
    public string RetrievedContext { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public EvaluationScore Score { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public interface IEvaluationService
{
    /// <summary>
    /// Gets the standard evaluation questions.
    /// </summary>
    List<EvaluationQuestion> GetEvaluationQuestions();

    /// <summary>
    /// Runs the complete evaluation suite.
    /// </summary>
    /// <param name="generationService">Generation service to test</param>
    /// <param name="retrievalService">Retrieval service to test</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of evaluation results</returns>
    Task<List<EvaluationResult>> RunEvaluationAsync(
        IGenerationService generationService,
        IRetrievalService retrievalService,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates summary statistics from evaluation results.
    /// </summary>
    Dictionary<string, object> GetSummaryStatistics(List<EvaluationResult> results);
}
