using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;
using Papertrail.Api.Models;
using Papertrail.Api.Persistence;
using Papertrail.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1_048_576;
});

var securityConfiguration = builder.Configuration
    .GetSection(SecurityOptions.SectionName)
    .Get<SecurityOptions>() ?? new SecurityOptions();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddOptions<SecurityOptions>()
    .Bind(builder.Configuration.GetSection(SecurityOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("ai-generation", _ =>
        RateLimitPartition.GetFixedWindowLimiter(
            "global",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = securityConfiguration.GenerationRequestsPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});
builder.Services.AddCors(options => options.AddPolicy("AngularDevelopment", policy =>
    policy.WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));
builder.Services.AddSingleton<IPromptBuilder, RagPromptBuilder>();
builder.Services.AddSingleton<IOllamaResponseParser, OllamaResponseParser>();
builder.Services.AddDbContext<PapertrailDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=papertrail.db"));
builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();
builder.Services.AddOptions<OllamaOptions>()
    .Bind(builder.Configuration.GetSection(OllamaOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddHttpClient<IGenerationService, OllamaGenerationService>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

// Add RAG services
builder.Services.AddSingleton<IPdfExtractor, PdfExtractor>();
builder.Services.AddSingleton<ITextChunker, TextChunker>();
builder.Services.AddSingleton<IVectorDatabase, InMemoryVectorDatabase>();
builder.Services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});
builder.Services.AddSingleton<IRetrievalService, RetrievalService>();
builder.Services.AddSingleton<IDocumentProcessor, DocumentProcessor>();
builder.Services.AddSingleton<IConversationMemoryService, InMemoryConversationMemoryService>();
builder.Services.AddSingleton<IEvaluationService, EvaluationService>();
builder.Services.AddSingleton<IPdfReportGenerator, SimplePdfReportGenerator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<PapertrailDbContext>();
    await database.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseCors("AngularDevelopment");
app.UseRateLimiter();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
    await next(context);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Papertrail.Api"
}))
.WithName("HealthCheck")
.WithOpenApi();

app.MapPost("/api/ai/generate", async (
    GenerateRequest request,
    IGenerationService generationService,
    IHistoryRepository historyRepository,
    IRetrievalService retrievalService,
    IConversationMemoryService conversationMemory,
    IPromptBuilder promptBuilder,
    CancellationToken cancellationToken) =>
{
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(request);
    if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
    {
        var errors = validationResults
            .SelectMany(result => result.MemberNames.DefaultIfEmpty(nameof(GenerateRequest.Prompt)),
                (result, memberName) => new { memberName, error = result.ErrorMessage ?? "The value is invalid." })
            .GroupBy(item => item.memberName)
            .ToDictionary(group => group.Key, group => group.Select(item => item.error).ToArray());

        return Results.ValidationProblem(errors);
    }

    try
    {
        // Retrieve relevant context if not provided
        var contextToUse = request.Context;
        if (string.IsNullOrWhiteSpace(contextToUse))
        {
            contextToUse = await retrievalService.RetrieveContextAsync(request.Prompt, topK: 5, cancellationToken);
        }

        // Create request with retrieved context
        var requestWithContext = new GenerateRequest
        {
            Prompt = request.Prompt,
            ConversationId = request.ConversationId,
            Context = contextToUse
        };

        var response = await generationService.GenerateAsync(requestWithContext, cancellationToken);
        
        // Store in history
        await historyRepository.AddAsync(requestWithContext, response, cancellationToken);

        // Store in conversation memory if conversationId is provided
        if (!string.IsNullOrWhiteSpace(request.ConversationId))
        {
            await conversationMemory.AddTurnAsync(request.ConversationId, "user", request.Prompt);
                await conversationMemory.AddTurnAsync(request.ConversationId, "assistant", response.Answer);
        }

        return Results.Ok(response);
    }
    catch (OllamaProviderException exception)
    {
        return Results.Problem(
            title: "AI provider unavailable",
            detail: exception.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
})
.WithName("GenerateAiResponse")
.RequireRateLimiting("ai-generation")
.WithOpenApi();

app.MapGet("/api/ai/history", async (
    string? conversationId,
    int? limit,
    IHistoryRepository historyRepository,
    CancellationToken cancellationToken) =>
{
    var requestedLimit = limit ?? 20;
    if (requestedLimit is < 1 or > 100)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["limit"] = ["Limit must be between 1 and 100."]
        });
    }

    var history = await historyRepository.GetAsync(conversationId, requestedLimit, cancellationToken);
    return Results.Ok(history);
})
.WithName("GetGenerationHistory")
.WithOpenApi();

app.MapPost("/api/documents/ingest", async (
    IDocumentProcessor documentProcessor,
    CancellationToken cancellationToken) =>
{
    try
    {
        var documentsPath = Path.Combine(AppContext.BaseDirectory, "documents");
        if (!Directory.Exists(documentsPath))
        {
            return Results.BadRequest(new { message = "Documents directory not found. Please create a 'documents' folder with PDF files." });
        }

        var chunksIngested = await documentProcessor.IngestDocumentsAsync(documentsPath, cancellationToken);
        return Results.Ok(new 
        { 
            message = "Documents ingested successfully",
            chunksIngested
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Document ingestion failed",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("IngestDocuments")
.WithOpenApi();

app.MapGet("/api/documents/status", async (
    IVectorDatabase vectorDatabase,
    IDocumentProcessor documentProcessor,
    CancellationToken cancellationToken) =>
{
    try
    {
        var chunksCount = await vectorDatabase.GetCountAsync();
        var documentsCount = await documentProcessor.GetDocumentCountAsync();
        
        return Results.Ok(new 
        { 
            documentsLoaded = documentsCount,
            chunksIndexed = chunksCount
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Status check failed",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("GetDocumentStatus")
.WithOpenApi();

app.MapPost("/api/evaluation/run", async (
    IGenerationService generationService,
    IRetrievalService retrievalService,
    IEvaluationService evaluationService,
    CancellationToken cancellationToken) =>
{
    try
    {
        if (!await retrievalService.HasDocumentsAsync())
        {
            return Results.BadRequest(new { message = "No documents loaded. Please ingest documents first." });
        }

        var results = await evaluationService.RunEvaluationAsync(generationService, retrievalService, cancellationToken);
        return Results.Ok(new 
        { 
            message = "Evaluation completed",
            resultsCount = results.Count,
            results
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Evaluation failed",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("RunEvaluation")
.WithOpenApi();

app.MapPost("/api/evaluation/report", async (
    IGenerationService generationService,
    IRetrievalService retrievalService,
    IEvaluationService evaluationService,
    IPdfReportGenerator reportGenerator,
    CancellationToken cancellationToken) =>
{
    try
    {
        if (!await retrievalService.HasDocumentsAsync())
        {
            return Results.BadRequest(new { message = "No documents loaded. Please ingest documents first." });
        }

        // Run evaluation
        var results = await evaluationService.RunEvaluationAsync(generationService, retrievalService, cancellationToken);
        var summary = evaluationService.GetSummaryStatistics(results);

        // Generate report
        var reportsDirectory = Path.Combine(AppContext.BaseDirectory, "reports");
        Directory.CreateDirectory(reportsDirectory);
        
        var reportPath = Path.Combine(reportsDirectory, $"evaluation-report-{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.txt");
        await reportGenerator.GenerateReportAsync(results, reportPath, summary);

        return Results.Ok(new 
        { 
            message = "Report generated successfully",
            reportPath,
            summary
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Report generation failed",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("GenerateEvaluationReport")
.WithOpenApi();

app.Run();

public partial class Program;
