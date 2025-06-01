
namespace AspnetcoreEx.KernelService.Tools;

public class LLMProcessorManager
{
    private readonly ILLMProcessor processor;
    public dynamic ResponseData => processor.ResponseData;

    public LLMProcessorManager(IEnumerable<ILLMProcessor> processors, string provider = "openai")
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
                    useSchemaPrompt ? Prompts.AnswerWithRAGContextNamePrompt.SystemPromptWithSchema : Prompts.AnswerWithRAGContextNamePrompt.SystemPrompt,
                    typeof(NameAnswerSchema),
                    Prompts.AnswerWithRAGContextNamePrompt.UserPrompt
                );
            case "number":
                return (
                    useSchemaPrompt ? Prompts.AnswerWithRAGContextNumberPrompt.SystemPromptWithSchema : Prompts.AnswerWithRAGContextNumberPrompt.SystemPrompt,
                    Prompts.AnswerWithRAGContextNumberPrompt.AnswerSchema,
                    Prompts.AnswerWithRAGContextNumberPrompt.UserPrompt
                );
            case "boolean":
                return (
                    useSchemaPrompt ? Prompts.AnswerWithRAGContextBooleanPrompt.SystemPromptWithSchema : Prompts.AnswerWithRAGContextBooleanPrompt.SystemPrompt,
                    Prompts.AnswerWithRAGContextBooleanPrompt.AnswerSchema,
                    Prompts.AnswerWithRAGContextBooleanPrompt.UserPrompt
                );
            case "names":
                return (
                    useSchemaPrompt ? Prompts.AnswerWithRAGContextNamesPrompt.SystemPromptWithSchema : Prompts.AnswerWithRAGContextNamesPrompt.SystemPrompt,
                    Prompts.AnswerWithRAGContextNamesPrompt.AnswerSchema,
                    Prompts.AnswerWithRAGContextNamesPrompt.UserPrompt
                );
            case "comparative":
                return (
                    useSchemaPrompt ? Prompts.ComparativeAnswerPrompt.SystemPromptWithSchema : Prompts.ComparativeAnswerPrompt.SystemPrompt,
                    Prompts.ComparativeAnswerPrompt.AnswerSchema,
                    Prompts.ComparativeAnswerPrompt.UserPrompt
                );
            default:
                throw new ArgumentException($"Unsupported schema: {schema}");
        }
    }

    public Dictionary<string, string> GetRephrasedQuestions(string originalQuestion, List<string> companies)
    {
        var answerDict = processor.SendMessage(
            systemContent: Prompts.RephrasedQuestionsPrompt.SystemPrompt,
            humanContent: string.Format(
                Prompts.RephrasedQuestionsPrompt.UserPrompt,
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