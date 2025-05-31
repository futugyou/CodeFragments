
using System.Text.Json.Serialization;

namespace AspnetcoreEx.KernelService.Tools;


public static class Prompts
{
    public static class RephrasedQuestionsPrompt
    {
        public const string SystemPrompt = "TODO";
        public const string UserPrompt = "TODO";
    }

    public static class AnswerWithRAGContextNamePrompt
    {
        public const string SystemPrompt = "TODO";
        public const string SystemPromptWithSchema = "TODO";
        public const string UserPrompt = "TODO";
        public static readonly object AnswerSchema = new { Name = "string" };
    }

    public static class AnswerWithRAGContextNumberPrompt
    {
        public const string SystemPrompt = "TODO";
        public const string SystemPromptWithSchema = "TODO";
        public const string UserPrompt = "TODO";
        public static readonly object AnswerSchema = new { Number = "int" };
    }

    public static class AnswerWithRAGContextBooleanPrompt
    {
        public const string SystemPrompt = "TODO";
        public const string SystemPromptWithSchema = "TODO";
        public const string UserPrompt = "TODO";
        public static readonly object AnswerSchema = new { IsTrue = "bool" };
    }
    public static class AnswerWithRAGContextNamesPrompt
    {
        public const string SystemPrompt = "TODO";
        public const string SystemPromptWithSchema = "TODO";
        public const string UserPrompt = "TODO";
        public static readonly object AnswerSchema = new { Names = "List<string>" };
    }
    public static class ComparativeAnswerPrompt
    {
        public const string SystemPrompt = "TODO";
        public const string SystemPromptWithSchema = "TODO";
        public const string UserPrompt = "TODO";
        public static readonly object AnswerSchema = new { ComparisonResult = "string" };
    }
}

public class RephrasedQuestion
{
    [JsonPropertyName("company_name")]
    [JsonInclude]
    public string CompanyName { get; set; }

    [JsonPropertyName("question")]
    [JsonInclude]
    public string Question { get; set; }
}