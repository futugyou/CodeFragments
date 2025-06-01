
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AspnetcoreEx.KernelService.CompanyReports;

/// <summary>
/// Individual question for a company
/// </summary>
public class RephrasedQuestion
{
    [JsonPropertyName("company_name")]
    [Description("Company name, exactly as provided in quotes in the original question")]
    public string CompanyName { get; set; }

    [JsonPropertyName("question")]
    [Description("Rephrased question specific to this company")]
    public string Question { get; set; }
}

/// <summary>
/// List of rephrased questions
/// </summary>
public class RephrasedQuestions
{
    [JsonPropertyName("questions")]
    [Description("List of rephrased questions for each company")]
    public List<RephrasedQuestion> Questions { get; set; }
}

/// <summary>
/// Rank retrieved text block relevance to a query.
/// </summary>
public class RetrievalRankingSingleBlock
{
    [JsonPropertyName("reasoning")]
    [Description("Analysis of the block, identifying key information and how it relates to the query")]
    public string Reasoning { get; set; }

    [JsonPropertyName("relevance_score")]
    [Description("Relevance score from 0 to 1, where 0 is Completely Irrelevant and 1 is Perfectly Relevant")]
    public double RelevanceScore { get; set; }
}

/// <summary>
/// Rank retrieved multiple text blocks relevance to a query.
/// </summary>
public class RetrievalRankingMultipleBlocks
{
    [JsonPropertyName("block_rankings")]
    [Description("A list of text blocks and their associated relevance scores.")]
    public List<RetrievalRankingSingleBlock> BlockRankings { get; set; }
}

public class NameAnswerSchema
{
    [JsonPropertyName("step_by_step_analysis")]
    [Description("Detailed step-by-step analysis of the answer with at least 5 steps and at least 150 words. Pay special attention to the wording of the question to avoid being tricked. Sometimes it seems that there is an answer in the context, but this is might be not the requested value, but only a similar one.")]
    public string StepByStepAnalysis { get; set; }

    [JsonPropertyName("reasoning_summary")]
    [Description("Concise summary of the step-by-step reasoning process. Around 50 words.")]
    public string ReasoningSummary { get; set; }

    [JsonPropertyName("relevant_pages")]
    [Description("""
List of page numbers containing information directly used to answer the question. Include only:
- Pages with direct answers or explicit statements
- Pages with key information that strongly supports the answer
Do not include pages with only tangentially related information or weak connections to the answer.
At least one page should be included in the list.
""")]
    public List<int> RelevantPages { get; set; }

    [JsonPropertyName("final_answer")]
    [Description("""
If it is a company name, should be extracted exactly as it appears in question.
If it is a person name, it should be their full name.
If it is a product name, it should be extracted exactly as it appears in the context.
Without any extra information, words or comments.
- Return 'N/A' if information is not available in the context
""")]
    public string FinalAnswer { get; set; }
}

public class NumberAnswerSchema
{
    [JsonPropertyName("step_by_step_analysis")]
    [Description("""
Detailed step-by-step analysis of the answer with at least 5 steps and at least 150 words.
**Strict Metric Matching Required:**    

1. Determine the precise concept the question's metric represents. What is it actually measuring?
2. Examine potential metrics in the context. Don't just compare names; consider what the context metric measures.
3. Accept ONLY if: The context metric's meaning *exactly* matches the target metric. Synonyms are acceptable; conceptual differences are NOT.
4. Reject (and use 'N/A') if:
    - The context metric covers more or less than the question's metric.
    - The context metric is a related concept but not the *exact* equivalent (e.g., a proxy or a broader category).
    - Answering requires calculation, derivation, or inference.
    - Aggregation Mismatch: The question needs a single value but the context offers only an aggregated total
5. No Guesswork: If any doubt exists about the metric's equivalence, default to `N/A`."
""")]
    public string StepByStepAnalysis { get; set; }

    [JsonPropertyName("reasoning_summary")]
    [Description("Concise summary of the step-by-step reasoning process. Around 50 words.")]
    public string ReasoningSummary { get; set; }

    [JsonPropertyName("relevant_pages")]
    [Description("""
List of page numbers containing information directly used to answer the question. Include only:
- Pages with direct answers or explicit statements
- Pages with key information that strongly supports the answer
Do not include pages with only tangentially related information or weak connections to the answer.
At least one page should be included in the list.
""")]
    public List<int> RelevantPages { get; set; }

    [JsonPropertyName("final_answer")]
    [Description("""
An exact metric number is expected as the answer.
- Example for percentages:
    Value from context: 58,3%
    Final answer: 58.3

Pay special attention to any mentions in the context about whether metrics are reported in units, thousands, or millions to adjust number in final answer with no changes, three zeroes or six zeroes accordingly.
Pay attention if value wrapped in parentheses, it means that value is negative.

- Example for negative values:
    Value from context: (2,124,837) CHF
    Final answer: -2124837

- Example for numbers in thousands:
    Value from context: 4970,5 (in thousands $)
    Final answer: 4970500

- Return 'N/A' if metric provided is in a different currency than mentioned in the question
    Example of value from context: 780000 USD, but question mentions EUR
    Final answer: 'N/A'

- Return 'N/A' if metric is not directly stated in context EVEN IF it could be calculated from other metrics in the context
    Example: Requested metric: Dividend per Share; Only available metrics from context: Total Dividends Paid ($5,000,000), and Number of Outstanding Shares (1,000,000); Calculated DPS = Total Dividends / Outstanding Shares.
    Final answer: 'N/A'

- Return 'N/A' if information is not available in the context
""")]
    public string FinalAnswer { get; set; }
}

public class BooleanAnswerSchema
{
    [JsonPropertyName("step_by_step_analysis")]
    [Description("""
Detailed step-by-step analysis of the answer with at least 5 steps and at least 150 words. Pay special attention to the wording of the question to avoid being tricked. Sometimes it seems that there is an answer in the context, but this is might be not the requested value, but only a similar one.
""")]
    public string StepByStepAnalysis { get; set; }

    [JsonPropertyName("reasoning_summary")]
    [Description("Concise summary of the step-by-step reasoning process. Around 50 words.")]
    public string ReasoningSummary { get; set; }

    [JsonPropertyName("relevant_pages")]
    [Description("""
List of page numbers containing information directly used to answer the question. Include only:
- Pages with direct answers or explicit statements
- Pages with key information that strongly supports the answer
Do not include pages with only tangentially related information or weak connections to the answer.
At least one page should be included in the list.
""")]
    public List<int> RelevantPages { get; set; }

    [JsonPropertyName("final_answer")]
    [Description("""
A boolean value (True or False) extracted from the context that precisely answers the question.
If question ask about did something happen, and in context there is information about it, return False.
""")]
    public bool FinalAnswer { get; set; }
}

public class NamesAnswerSchema
{
    [JsonPropertyName("step_by_step_analysis")]
    [Description("Detailed step-by-step analysis of the answer with at least 5 steps and at least 150 words. Pay special attention to the wording of the question to avoid being tricked. Sometimes it seems that there is an answer in the context, but this is might be not the requested value, but only a similar one.")]
    public string StepByStepAnalysis { get; set; }

    [JsonPropertyName("reasoning_summary")]
    [Description("Concise summary of the step-by-step reasoning process. Around 50 words.")]
    public string ReasoningSummary { get; set; }

    [JsonPropertyName("relevant_pages")]
    [Description("""
List of page numbers containing information directly used to answer the question. Include only:
- Pages with direct answers or explicit statements
- Pages with key information that strongly supports the answer
Do not include pages with only tangentially related information or weak connections to the answer.
At least one page should be included in the list.
""")]
    public List<int> RelevantPages { get; set; }

    [JsonPropertyName("final_answer")]
    [Description("""
Example:
Question:
"What are the names of all new executives that took on new leadership positions in company?"

Answer:
```
{
    "step_by_step_analysis": "1. The question asks for the names of all new executives who took on new leadership positions in the company.\n2. Exhibit 10.9 and 10.10, as listed in the Exhibit Index on page 89, mentions new Executive Agreements with Carly Kennedy and Brian Appelgate.\n3. Exhibit 10.9, Employment Agreement with Carly Kennedy, states her start date as April 4, 2022, and her position as Executive Vice President and General Counsel.\n4. Exhibit 10.10, Offer Letter with Brian Appelgate shows that his new role within the company is Interim Chief Operations Officer, and he was accepting the offer on November 8, 2022.\n5. Based on the documents, Carly Kennedy and Brian Appelgate are named as the new executives.",
    "reasoning_summary": "Exhibits 10.9 and 10.10 of the annual report, described as Employment Agreement and Offer Letter, explicitly name Carly Kennedy and Brian Appelgate taking on new leadership roles within the company in 2022.",
    "relevant_pages": [
        89
    ],
    "final_answer": [
        "Carly Kennedy",
        "Brian Appelgate"
    ]
}
```
""")]
    public List<string> FinalAnswer { get; set; }
}

public class ComparativeAnswerSchema
{
    [JsonPropertyName("step_by_step_analysis")]
    [Description("Detailed step-by-step analysis of the answer with at least 5 steps and at least 150 words.")]
    public string StepByStepAnalysis { get; set; }

    [JsonPropertyName("reasoning_summary")]
    [Description("Concise summary of the step-by-step reasoning process. Around 50 words.")]
    public string ReasoningSummary { get; set; }

    [JsonPropertyName("relevant_pages")]
    [Description("Just leave empty")]
    public List<int> RelevantPages { get; set; }

    [JsonPropertyName("final_answer")]
    [Description("""
Company name should be extracted exactly as it appears in question.
Answer should be either a single company name or 'N/A' if no company is applicable.
""")]
    public string FinalAnswer { get; set; }
}
