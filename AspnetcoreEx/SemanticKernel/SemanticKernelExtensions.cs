using AspnetcoreEx.SemanticKernel.Skills;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.Connectors.Memory.Qdrant;
using Microsoft.SemanticKernel.CoreSkills;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.Skills.Web;
using Microsoft.SemanticKernel.Skills.Web.Google;
using Microsoft.SemanticKernel.TemplateEngine;

namespace AspnetcoreEx.SemanticKernel;
public static class SemanticKernelExtensions
{
    internal static IServiceCollection AddSemanticKernelServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SemanticKernelOptions>(configuration.GetSection("SemanticKernel"));
        var sp = services.BuildServiceProvider();
        var config = sp.GetRequiredService<IOptionsMonitor<SemanticKernelOptions>>()!.CurrentValue;
        var logger = sp.GetRequiredService<ILogger<IKernel>>();

        services.AddHttpClient("qdrant", c =>
        {
            UriBuilder builder = new(config.QdrantHost);
            if (config.QdrantPort.HasValue) { builder.Port = config.QdrantPort.Value; }
            c.BaseAddress = builder.Uri;
            if (!string.IsNullOrEmpty(config.QdrantKey))
            {
                c.DefaultRequestHeaders.Add("api-key", config.QdrantKey);
            }
        });

        services.AddSingleton<IQdrantVectorDbClient>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("qdrant");
            var logger = sp.GetRequiredService<ILogger<IQdrantVectorDbClient>>();
            return new QdrantVectorDbClient(
                httpClient.BaseAddress!.ToString(),
                config.QdrantVectorSize,
                httpClient.BaseAddress.Port,
                httpClient,
                logger);
        });

        services.AddScoped<KernelConfig>(sp =>
        {
            var kernelConfig = new KernelConfig();
            kernelConfig.AddOpenAITextEmbeddingGenerationService(config.Embedding, config.Key);
            kernelConfig.AddOpenAIChatCompletionService(config.ChatGPT, config.Key);
            kernelConfig.AddOpenAIImageGenerationService(config.Key);
            kernelConfig.AddOpenAITextCompletionService(config.TextCompletion, config.Key);
            return kernelConfig;
        });

        //services.AddSingleton<IPromptTemplateEngine, PromptTemplateEngine>();

        //services.AddSingleton<IMemoryStore, VolatileMemoryStore>();
        //services.AddSingleton<IMemoryStore>(sp => new QdrantMemoryStore(
        //           config.QdrantHost, config.QdrantPort, config.QdrantVectorSize, sp.GetRequiredService<ILogger<QdrantMemoryStore>>()));
        services.AddSingleton<IMemoryStore>(sp =>
        {
            var dnClient = sp.GetRequiredService<IQdrantVectorDbClient>();
            return new QdrantMemoryStore(dnClient);
        });

        //services.AddScoped<ISemanticTextMemory>(sp => NullMemory.Instance);
        services.AddScoped<ISemanticTextMemory>(sp =>
        {
            var store = sp.GetRequiredService<IMemoryStore>();

            return new SemanticTextMemory(store, new OpenAITextEmbeddingGeneration(
                modelId: config.Embedding,
                apiKey: config.Key,
                logger: logger));
        });

        //services.AddScoped<ISkillCollection, SkillCollection>();

        services.AddScoped<IKernel>(sp =>
        {
            IKernel kernel = Kernel.Builder
                .WithLogger(logger)
                .WithMemory(sp.GetRequiredService<ISemanticTextMemory>())
                .WithConfiguration(sp.GetRequiredService<KernelConfig>())
                .Build();

            var skills = kernel.ImportSemanticSkillFromDirectory("SemanticKernel", "Skills");
            foreach (var skill in skills)
            {
                kernel.RegisterCustomFunction(skill.Key, skill.Value);
            }

            kernel.ImportSkill(new SteamSkill(), "MyCSharpSkill");
            kernel.ImportSkill(new TimeSkill(), "time");
            kernel.ImportSkill(new TextSkill()); // this will use default skill name: _GLOBAL_FUNCTIONS_
            kernel.ImportSkill(new ConversationSummarySkill(kernel));
            kernel.ImportSkill(new FileIOSkill(), "file");
            kernel.ImportSkill(new HttpSkill(), "http");
            kernel.ImportSkill(new MathSkill(), "math");

            var googleConnector = new GoogleConnector(config.GoogleApikey, config.GoogleEngine);
            kernel.ImportSkill(new WebSearchEngineSkill(googleConnector), "google");

            kernel.ImportSkill(new SearchUrlSkill(), "search");
            kernel.ImportSkill(new TextMemorySkill("contextQueryMemories", "0.3", "5"));
            kernel.ImportSkill(new TextMemorySkill(), "baseMemory");
            kernel.ImportSkill(new MarkupSkill(), "markup");

            return kernel;
        });

        return services;
    }
}

