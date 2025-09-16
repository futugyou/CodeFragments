
using OpenAI;

namespace AspnetcoreEx.KernelService.CompanyReports;

public static class CompanyReportExtensions
{
    public static IServiceCollection AddCompanyReportServices(this IServiceCollection services, IConfiguration configuration)
    {
        // configuration
        services.Configure<CompanyReportlOptions>(configuration.GetSection("CompanyReport"));
        var sp = services.BuildServiceProvider();
        var config = sp.GetRequiredService<IOptionsMonitor<CompanyReportlOptions>>()!.CurrentValue;

        var ghModelsClient = new OpenAIClient(config.LlmApiKey);
        services.AddKeyedScoped<OpenAIClient>("report", (sp, _) => ghModelsClient);
        services.AddScoped<IAPIProcessor, OpenaiProcessor>();
        services.AddScoped<APIProcessorManager>(sp =>
        {
            var apiProcessors = sp.GetServices<IAPIProcessor>();
            return new APIProcessorManager(apiProcessors, config.ApiProvider);
        });
        services.AddScoped<AsyncOpenaiProcessor>();

        services.AddScoped<TableSerializer>();

        services.AddKeyedScoped<IIngestor, BM25Ingestor>("BM25");
        services.AddKeyedScoped<IIngestor, VectorDBIngestor>("VectorDB");

        services.AddScoped<LLMReranker>();

        services.AddKeyedScoped<IPDFParser, DoclingPDFParser>("docling", (sp, _) =>
        {
            var logger = sp.GetRequiredService<ILogger<DoclingPDFParser>>();
            return new DoclingPDFParser(logger, config.ParsedReportsPath, config.SubsetPath);
        });
        services.AddKeyedScoped<IPDFParser, PdfPigParser>("pdfpig", (sp, _) =>
        {
            var logger = sp.GetRequiredService<ILogger<PdfPigParser>>();
            return new PdfPigParser(logger, config.ParsedReportsPath, config.SubsetPath);
        });

        services.AddKeyedScoped<IRetrieval, BM25Retriever>("BM25Retriever");
        services.AddKeyedScoped<IRetrieval, HybridRetriever>("HybridRetriever");
        services.AddKeyedScoped<IRetrieval, VectorRetriever>("VectorRetriever");

        services.AddScoped<Pipeline>();
        services.AddScoped<QuestionsProcessor>();

        return services;
    }
}