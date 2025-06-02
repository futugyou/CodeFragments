
namespace AspnetcoreEx.KernelService.CompanyReports;

public class APIProcessorManager
{
    private readonly IAPIProcessor processor;
    public dynamic ResponseData => processor.ResponseData;

    public APIProcessorManager(IEnumerable<IAPIProcessor> processors, string provider = "openai")
    {
        processor = processors.FirstOrDefault(p => p.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Unsupported provider: {provider}");
    }

    public dynamic SendMessage(
       string model = null,
       double temperature = 0.5,
       int? seed = null,
       string systemContent = "You are a helpful assistant.",
       string humanContent = "Hello!",
       bool isStructured = false,
       object responseFormat = null,
       Dictionary<string, object> kwargs = null)
    {
        if (model == null)
        {
            model = processor.DefaultModel;
        }
        return processor.SendMessage(
            model: model,
            temperature: temperature,
            seed: seed,
            systemContent: systemContent,
            humanContent: humanContent,
            isStructured: isStructured,
            responseFormat: responseFormat,
            kwargs: kwargs
        );
    }

    public dynamic GetAnswerFromRagContext(string question, string ragContext, string schema, string model)
    {
        var (systemPrompt, responseFormat, userPrompt) = BuildRagContextPrompts(schema);

        var answerDict = processor.SendMessage(
            model: model,
            systemContent: systemPrompt,
            humanContent: string.Format(userPrompt, ragContext, question),
            isStructured: true,
            responseFormat: responseFormat
        );

        return answerDict;
    }

    private (string systemPrompt, object responseFormat, string userPrompt) BuildRagContextPrompts(string schema)
    {
        bool useSchemaPrompt = processor.Provider == "ibm" || processor.Provider == "gemini";
        switch (schema)
        {
            case "name":
                return (
                    useSchemaPrompt ? AnswerWithRAGContextNamePrompt.SystemPromptWithSchema : AnswerWithRAGContextNamePrompt.SystemPrompt,
                    typeof(NameAnswerSchema),
                    AnswerWithRAGContextNamePrompt.UserPrompt
                );
            case "number":
                return (
                    useSchemaPrompt ? AnswerWithRAGContextNumberPrompt.SystemPromptWithSchema : AnswerWithRAGContextNumberPrompt.SystemPrompt,
                    typeof(NumberAnswerSchema),
                    AnswerWithRAGContextNumberPrompt.UserPrompt
                );
            case "boolean":
                return (
                    useSchemaPrompt ? AnswerWithRAGContextBooleanPrompt.SystemPromptWithSchema : AnswerWithRAGContextBooleanPrompt.SystemPrompt,
                    typeof(BooleanAnswerSchema),
                    AnswerWithRAGContextBooleanPrompt.UserPrompt
                );
            case "names":
                return (
                    useSchemaPrompt ? AnswerWithRAGContextNamesPrompt.SystemPromptWithSchema : AnswerWithRAGContextNamesPrompt.SystemPrompt,
                    typeof(NamesAnswerSchema),
                    AnswerWithRAGContextNamesPrompt.UserPrompt
                );
            case "comparative":
                return (
                    useSchemaPrompt ? ComparativeAnswerPrompt.SystemPromptWithSchema : ComparativeAnswerPrompt.SystemPrompt,
                    typeof(ComparativeAnswerSchema),
                    ComparativeAnswerPrompt.UserPrompt
                );
            default:
                throw new ArgumentException($"Unsupported schema: {schema}");
        }
    }

    public Dictionary<string, string> GetRephrasedQuestions(string originalQuestion, List<string> companies)
    {
        var answerDict = processor.SendMessage(
            systemContent: RephrasedQuestionsPrompt.SystemPrompt,
            humanContent: string.Format(
                RephrasedQuestionsPrompt.UserPrompt,
                originalQuestion,
                string.Join(", ", companies.ConvertAll(c => $"\"{c}\""))
            ),
            isStructured: true
        );

        var questionsDict = new Dictionary<string, string>();
        foreach (var item in answerDict.questions)
        {
            questionsDict[item.company_name] = item.question;
        }
        return questionsDict;
    }
}