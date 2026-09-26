using System.Text;

namespace Papertrail.Api.Services;

public interface IPdfReportGenerator
{
    /// <summary>
    /// Generates a PDF report from evaluation results.
    /// </summary>
    /// <param name="results">Evaluation results to include in the report</param>
    /// <param name="outputPath">Path where the PDF should be saved</param>
    /// <param name="summary">Summary statistics</param>
    Task GenerateReportAsync(
        List<EvaluationResult> results,
        string outputPath,
        Dictionary<string, object> summary);
}

public sealed class SimplePdfReportGenerator : IPdfReportGenerator
{
    public async Task GenerateReportAsync(
        List<EvaluationResult> results,
        string outputPath,
        Dictionary<string, object> summary)
    {
        await Task.Run(() =>
        {
            var sb = new StringBuilder();
            
            // Header
            sb.AppendLine("GENERATIVE AI ASSIGNMENT - EVALUATION REPORT");
            sb.AppendLine("=" + new string('=', 60));
            sb.AppendLine();
            sb.AppendLine($"Report Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}");
            sb.AppendLine();

            // Summary Statistics
            sb.AppendLine("SUMMARY STATISTICS");
            sb.AppendLine("-" + new string('-', 60));
            foreach (var kvp in summary)
            {
                if (kvp.Value is double doubleValue)
                {
                    sb.AppendLine($"{kvp.Key}: {doubleValue:F2}");
                }
                else
                {
                    sb.AppendLine($"{kvp.Key}: {kvp.Value}");
                }
            }
            sb.AppendLine();

            // Detailed Results
            sb.AppendLine("DETAILED EVALUATION RESULTS");
            sb.AppendLine("=" + new string('=', 60));
            sb.AppendLine();

            foreach (var result in results)
            {
                sb.AppendLine($"QUESTION {result.QuestionNumber}");
                sb.AppendLine("-" + new string('-', 60));
                sb.AppendLine($"Question: {result.Question}");
                sb.AppendLine();
                
                sb.AppendLine("Retrieved Context:");
                sb.AppendLine(string.IsNullOrWhiteSpace(result.RetrievedContext)
                    ? "  [No relevant context retrieved]"
                    : "  " + result.RetrievedContext.Replace("\n", "\n  "));
                sb.AppendLine();

                sb.AppendLine("Response:");
                sb.AppendLine("  " + result.Response.Replace("\n", "\n  "));
                sb.AppendLine();

                sb.AppendLine("Scoring:");
                sb.AppendLine($"  Relevance: {result.Score.Relevance}/4");
                sb.AppendLine($"  Accuracy: {result.Score.Accuracy}/4");
                sb.AppendLine($"  Contextual Awareness: {result.Score.ContextualAwareness}/4");
                sb.AppendLine($"  Response Quality: {result.Score.ResponseQuality}/4");
                sb.AppendLine($"  Total Score: {result.Score.Total}/16");
                sb.AppendLine();
            }

            // Footer
            sb.AppendLine("=" + new string('=', 60));
            sb.AppendLine("END OF REPORT");

            // Write to file
            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        });
    }
}
