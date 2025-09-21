
namespace KaleidoCode.KernelService.CompanyReports;

public class APIProcessorManager
{
    private readonly IAPIProcessor processor;
    public ResponseData ResponseData => processor.ResponseData;

    public APIProcessorManager(IEnumerable<IAPIProcessor> processors, string provider = "openai")
    {
        processor = processors.FirstOrDefault(p => p.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Unsupported provider: {provider}");
    }

    public async Task<RephrasedQuestions> SendMessageAsync(string model = "gpt-4o-2024-08-06", float temperature = 0.5f, long? seed = null, string systemContent = "You are a helpful assistant.", string humanContent = "Hello!", bool isStructured = false, object? responseFormat = null, Dictionary<string, object>? kwargs = null, CancellationToken cancellationToken = default)
    {
        model ??= processor.DefaultModel;
        return await processor.SendMessageAsync(
            model: model,
            temperature: temperature,
            seed: seed,
            systemContent: systemContent,
            humanContent: humanContent,
            isStructured: isStructured,
            responseFormat: responseFormat,
            kwargs: kwargs,
            cancellationToken: cancellationToken
        );
    }

    public async Task<RephrasedQuestions> GetAnswerFromRagContextAsync(string question, string ragContext, string schema, string model, CancellationToken cancellationToken = default)
    {
        var (systemPrompt, responseFormat, userPrompt) = BuildRagContextPrompts(schema);

        return await processor.SendMessageAsync(
            model: model,
            systemContent: systemPrompt,
            humanContent: string.Format(userPrompt, ragContext, question),
            isStructured: true,
            responseFormat: responseFormat,
            cancellationToken: cancellationToken
        );
    }

    private (string systemPrompt, object responseFormat, string userPrompt) BuildRagContextPrompts(string schema)
    {
        bool useSchemaPrompt = processor.Provider == "ibm" || processor.Provider == "gemini";
        return schema switch
        {
            "name" => ((string systemPrompt, object responseFormat, string userPrompt))(
                                useSchemaPrompt ? AnswerWithRAGContextNamePrompt.SystemPromptWithSchema : AnswerWithRAGContextNamePrompt.SystemPrompt,
                                typeof(NameAnswerSchema),
                                AnswerWithRAGContextNamePrompt.UserPrompt
                            ),
            "number" => ((string systemPrompt, object responseFormat, string userPrompt))(
                                useSchemaPrompt ? AnswerWithRAGContextNumberPrompt.SystemPromptWithSchema : AnswerWithRAGContextNumberPrompt.SystemPrompt,
                                typeof(NumberAnswerSchema),
                                AnswerWithRAGContextNumberPrompt.UserPrompt
                            ),
            "boolean" => ((string systemPrompt, object responseFormat, string userPrompt))(
                                useSchemaPrompt ? AnswerWithRAGContextBooleanPrompt.SystemPromptWithSchema : AnswerWithRAGContextBooleanPrompt.SystemPrompt,
                                typeof(BooleanAnswerSchema),
                                AnswerWithRAGContextBooleanPrompt.UserPrompt
                            ),
            "names" => ((string systemPrompt, object responseFormat, string userPrompt))(
                                useSchemaPrompt ? AnswerWithRAGContextNamesPrompt.SystemPromptWithSchema : AnswerWithRAGContextNamesPrompt.SystemPrompt,
                                typeof(NamesAnswerSchema),
                                AnswerWithRAGContextNamesPrompt.UserPrompt
                            ),
            "comparative" => ((string systemPrompt, object responseFormat, string userPrompt))(
                                useSchemaPrompt ? ComparativeAnswerPrompt.SystemPromptWithSchema : ComparativeAnswerPrompt.SystemPrompt,
                                typeof(ComparativeAnswerSchema),
                                ComparativeAnswerPrompt.UserPrompt
                            ),
            _ => throw new ArgumentException($"Unsupported schema: {schema}"),
        };
    }

    public async Task<Dictionary<string, string>> GetRephrasedQuestions(string originalQuestion, List<string> companies, CancellationToken cancellationToken = default)
    {
        var answerDict = await processor.SendMessageAsync(
            systemContent: RephrasedQuestionsPrompt.SystemPrompt,
            humanContent: string.Format(
                RephrasedQuestionsPrompt.UserPrompt,
                originalQuestion,
                string.Join(", ", companies.ConvertAll(c => $"\"{c}\""))
            ),
            isStructured: true,
            cancellationToken: cancellationToken
        );

        var questionsDict = new Dictionary<string, string>();
        foreach (var item in answerDict.Questions)
        {
            questionsDict[item.CompanyName] = item.Question;
        }
        return questionsDict;
    }
}