
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Path = System.IO.Path;

namespace AspnetcoreEx.KernelService.CompanyReports;

public class QuestionsProcessor
{
    // TODO: DI
    private readonly APIProcessorManager openaiProcessor;
    private string vectorDbDir;
    private string documentsDir;
    private string questionsFilePath;
    private bool v;
    private string subsetPath;
    private bool parentDocumentRetrieval;
    private bool llmReranking;
    private int llmRerankingSampleSize;
    private int topNRetrieval;
    private int parallelRequests;
    private string apiProvider;
    private string answeringModel;
    private bool fullContext;
    private ResponseData responseData;
    private bool newChallengePipeline = false;
    private bool returnParentPages = false;
    private List<AnswerDetail> AnswerDetailList = []; 
    private Dictionary<string, CsvMetadata> companies_datas = [];

    public QuestionsProcessor(string vectorDbDir, string documentsDir, string questionsFilePath, bool v, string subsetPath, bool parentDocumentRetrieval, bool llmReranking, int llmRerankingSampleSize, int topNRetrieval, int parallelRequests, string apiProvider, string answeringModel, bool fullContext)
    {
        this.vectorDbDir = vectorDbDir;
        this.documentsDir = documentsDir;
        this.questionsFilePath = questionsFilePath;
        this.v = v;
        this.subsetPath = subsetPath;
        this.parentDocumentRetrieval = parentDocumentRetrieval;
        this.llmReranking = llmReranking;
        this.llmRerankingSampleSize = llmRerankingSampleSize;
        this.topNRetrieval = topNRetrieval;
        this.parallelRequests = parallelRequests;
        this.apiProvider = apiProvider;
        this.answeringModel = answeringModel;
        this.fullContext = fullContext;
    }

    public async Task<ProcessQuestionsResult> ProcessAllQuestionsAsync(string outputPath, bool submissionFile, string teamEmail, string submissionName, string pipelineDetails, CancellationToken cancellationToken = default)
    {
        var datas = await File.ReadAllTextAsync(questionsFilePath, cancellationToken) ?? "[]";
        var reportData = JsonSerializer.Deserialize<List<QuestionRoot>>(datas) ?? [];
        return await ProcessQuestionsListAsync(reportData, outputPath, submissionFile, teamEmail, submissionName, pipelineDetails, cancellationToken);
    }

    public async Task<ProcessQuestionsResult> ProcessQuestionsListAsync(
               List<QuestionRoot> questionsList,
               string outputPath = "",
               bool submissionFile = false,
               string teamEmail = "",
               string submissionName = "",
               string pipelineDetails = "",
               CancellationToken cancellationToken = default)
    {
        int totalQuestions = questionsList.Count;
        // Add index to each question
        var questionsWithIndex = questionsList
            .Select((q, i) => { q.Index = i; return q; })
            .ToList();

        AnswerDetailList = [.. new AnswerDetail[totalQuestions]];
        var processedQuestions = new Question[totalQuestions];

        if (parallelRequests <= 1)
        {
            for (int i = 0; i < totalQuestions; i++)
            {
                var processed = await ProcessSingleQuestionAsync(questionsWithIndex[i], cancellationToken);
                processedQuestions[i] = processed;
                if (!string.IsNullOrEmpty(outputPath))
                {
                    await SaveProgressAsync([.. processedQuestions], outputPath, submissionFile, teamEmail, submissionName, pipelineDetails, cancellationToken);
                }
            }
        }
        else
        {
            // Use Parallel.ForEachAsync for parallel processing
            await Parallel.ForEachAsync(
                questionsWithIndex,
                new ParallelOptions { MaxDegreeOfParallelism = parallelRequests },
                async (question, cancellationToken) =>
                {
                    // Simulate async work if needed
                    var processed = await ProcessSingleQuestionAsync(question, cancellationToken);
                    processedQuestions[question.Index] = processed;
                    if (!string.IsNullOrEmpty(outputPath))
                    {
                        await SaveProgressAsync([.. processedQuestions], outputPath, submissionFile, teamEmail, submissionName, pipelineDetails, cancellationToken);
                    }
                });
        }

        var statistics = CalculateStatistics(processedQuestions.ToList(), printStats: true);

        return new ProcessQuestionsResult
        {
            Questions = processedQuestions.ToList(),
            AnswerDetails = AnswerDetailList,
            Statistics = statistics
        };
    }

    public async Task<RephrasedQuestions> GetAnswerForCompanyAsync(string companyName, string question, string schema, CancellationToken cancellation = default)
    {
        IRetrieval retriever;
        if (llmReranking)
        {
            retriever = new HybridRetriever(
                vectorDbDir: vectorDbDir,
                documentsDir: documentsDir
            );
        }
        else
        {
            retriever = new VectorRetriever(
                vectorDbDir: vectorDbDir,
                documentsDir: documentsDir
            );
        }

        List<RetrievalResult> retrievalResults;
        if (fullContext)
        {
            retrievalResults = await retriever.RetrieveAllAsync(companyName, cancellation);
        }
        else
        {
            retrievalResults = await retriever.RetrieveByCompanyNameAsync(
                companyName: companyName,
                query: question,
                llmRerankingSampleSize: llmRerankingSampleSize,
                topN: topNRetrieval,
                returnParentPages: returnParentPages,
                cancellationToken: cancellation);
        }

        if (retrievalResults == null || retrievalResults.Count == 0)
        {
            throw new ArgumentException("No relevant context found");
        }

        var ragContext = FormatRetrievalResults(retrievalResults);
        var answerDict = await openaiProcessor.GetAnswerFromRagContextAsync(
            question: question,
            ragContext: ragContext,
            schema: schema,
            model: answeringModel,
            cancellationToken: cancellation
        );
        responseData = openaiProcessor.ResponseData;

        if (newChallengePipeline)
        {
            var pages = answerDict.RelevantPages ?? [];
            var validatedPages = ValidatePageReferences(pages, retrievalResults);
            answerDict.RelevantPages = validatedPages;
            answerDict.References = ExtractReferences(validatedPages, companyName);
        }
        return answerDict;
    }

    public async Task<RephrasedQuestions> ProcessQuestionAsync(string question, string schema, CancellationToken cancellation = default)
    {
        List<string> extractedCompanies = [];
        if (newChallengePipeline)
        {
            extractedCompanies = ExtractCompaniesFromSubset(question);
        }
        else
        {
            var matches = Regex.Matches(question, "\"([^\"]*)\"");
            foreach (Match match in matches)
            {
                extractedCompanies.Add(match.Groups[1].Value);
            }
        }

        if (extractedCompanies.Count == 0)
        {
            throw new ArgumentException("No company name found in the question.");
        }

        if (extractedCompanies.Count == 1)
        {
            var companyName = extractedCompanies[0];
            return await GetAnswerForCompanyAsync(companyName, question, schema, cancellation);
        }
        else
        {
            return await ProcessComparativeQuestionAsync(question, extractedCompanies, schema, cancellation);
        }
    }

    /// <summary>
    /// Handling comparative questions involving multiple companies:
    /// 1. Restate the comparative question as a separate question for each company
    /// 2. Process each company's question in parallel
    /// 3. Aggregate the results to generate the final comparative answer
    /// </summary>
    /// <param name="question">Original comparative question</param>
    /// <param name="companies">Company list</param>
    /// <param name="schema">Question schema</param>
    /// <returns>Comparative answer (including references)</returns>
    public async Task<RephrasedQuestions> ProcessComparativeQuestionAsync(string question, List<string> companies, string schema, CancellationToken cancellation = default)
    {
        // Step 1: Restate the comparative question as a separate question for each company
        var rephrasedQuestions = await openaiProcessor.GetRephrasedQuestions(originalQuestion: question, companies: companies, cancellationToken: cancellation);

        var individualAnswers = new Dictionary<string, RephrasedQuestions>();
        var aggregatedReferences = new List<ReferenceKey>();

        // Step 2: Process each company's problems in parallel
        var tasks = companies.Select(async company =>
        {
            // Handle issues with a single company
            if (!rephrasedQuestions.TryGetValue(company, out var subQuestion))
                throw new Exception($"Could not generate sub-question for company: {company}");

            var answerDict = await GetAnswerForCompanyAsync(
                companyName: company,
                question: subQuestion,
                schema: "number",
                cancellation
            );
            return (company, answerDict);
        }).ToList();

        try
        {
            var results = await Task.WhenAll(tasks);
            foreach (var (company, answerDict) in results)
            {
                individualAnswers[company] = answerDict;
                if (answerDict.References != null)
                {
                    aggregatedReferences.AddRange(answerDict.References);
                }
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions in parallel tasks
            throw new Exception("Error processing company questions", ex);
        }

        // Remove duplicate references 
        var uniqueRefs = new Dictionary<string, ReferenceKey>();

        foreach (var reference in aggregatedReferences)
        {
            uniqueRefs[reference.ToString()] = reference;
        }

        aggregatedReferences = [.. uniqueRefs.Values];

        // Step 3: Generate a comparative answer using all individual answers
        var contextStr = string.Join(Environment.NewLine, individualAnswers.Select(kv => $"{kv.Key}: {kv.Value}"));

        var comparativeAnswer = await openaiProcessor.GetAnswerFromRagContextAsync(question: question, ragContext: contextStr, schema: "comparative", model: answeringModel, cancellationToken: cancellation);
        responseData = openaiProcessor.ResponseData;

        // Add reference
        if (comparativeAnswer != null)
        {
            comparativeAnswer.References = aggregatedReferences;
            return comparativeAnswer;
        }
        else
        {
            throw new Exception("Comparative answer is not a dictionary");
        }
    }

    #region private methos

    private static string FormatRetrievalResults(List<RetrievalResult> retrievalResults)
    {
        if (retrievalResults == null)
            return string.Empty;

        var contextParts = new List<string>();
        foreach (var result in retrievalResults)
        {
            var pageNumber = result.Page;
            var text = result.Text;
            contextParts.Add($"Text retrieved from page {pageNumber}: \n\"\"\"\n{text}\n\"\"\"");
        }

        return string.Join("\n\n---\n\n", contextParts);
    }

    private static List<int> ValidatePageReferences(
        List<int> claimedPages,
        List<RetrievalResult> retrievalResults,
        int minPages = 2,
        int maxPages = 8)
    {
        // If claimedPages is null, initialize to an empty list
        claimedPages ??= [];

        // Extract the page field from the search results
        var retrievedPages = retrievalResults.Select(r => r.Page).ToList();

        // // Only keep the page numbers in the search results
        var validatedPages = claimedPages.Where(retrievedPages.Contains).ToList();

        if (validatedPages.Count < claimedPages.Count)
        {
            var removedPages = claimedPages.Except(validatedPages).ToList();
            Console.WriteLine($"Warning: Removed {removedPages.Count} hallucinated page references: {string.Join(", ", removedPages)}");
        }

        // If the number of valid pages is less than minPages, add more from the search results
        if (validatedPages.Count < minPages && retrievalResults.Count > 0)
        {
            var existingPages = new HashSet<int>(validatedPages);
            foreach (var result in retrievalResults)
            {
                int page = result.Page;
                if (!existingPages.Contains(page))
                {
                    validatedPages.Add(page);
                    existingPages.Add(page);
                    if (validatedPages.Count >= minPages)
                        break;
                }
            }
        }

        // If the maximum number of pages is exceeded, the page will be cropped
        if (validatedPages.Count > maxPages)
        {
            Console.WriteLine($"Trimming references from {validatedPages.Count} to {maxPages} pages");
            validatedPages = [.. validatedPages.Take(maxPages)];
        }

        return validatedPages;
    }

    private List<ReferenceKey> ExtractReferences(List<int> pagesList, string companyName)
    {
        if (string.IsNullOrEmpty(subsetPath))
            throw new ArgumentException("subsetPath is required for new challenge pipeline when processing references.");

        companies_datas = IPDFParser.ParseCsvMetadata(subsetPath);

        var company = companies_datas.FirstOrDefault(c => c.Value.GetCompanyName() == companyName);
        string companySha1 = company.Value.Sha1 ?? "";

        var refs = new List<ReferenceKey>();
        foreach (var page in pagesList)
        {
            refs.Add(new ReferenceKey(companySha1, page));
        }
        return refs;
    }

    private async Task SaveProgressAsync(
            List<Question> processedQuestions,
            string outputPath,
            bool submissionFile = false,
            string teamEmail = "",
            string submissionName = "",
            string pipelineDetails = "",
            CancellationToken cancellation = default)
    {
        if (!string.IsNullOrEmpty(outputPath))
        {
            var statistics = CalculateStatistics(processedQuestions);

            // Prepare debug content
            var result = new
            {
                questions = processedQuestions,
                answer_details = AnswerDetailList,
                statistics = statistics
            };

            var outputFile = new FileInfo(outputPath);
            var debugFileName = $"{Path.GetFileNameWithoutExtension(outputFile.Name)}_debug{outputFile.Extension}";
            var debugFilePath = Path.Combine(outputFile.DirectoryName ?? "./", debugFileName);

            // Write to debug file
            await File.WriteAllTextAsync(debugFilePath, JsonSerializer.Serialize(result), Encoding.UTF8, cancellationToken: cancellation);

            if (submissionFile)
            {
                // Submit the answer for post-processing
                var submissionAnswers = PostProcessSubmissionAnswers(processedQuestions);
                var submission = new
                {
                    answers = submissionAnswers,
                    team_email = teamEmail,
                    submission_name = submissionName,
                    details = pipelineDetails
                };

                await File.WriteAllTextAsync(outputFile.FullName, JsonSerializer.Serialize(submission), Encoding.UTF8, cancellationToken: cancellation);
            }
        }
    }

    private static QuestionStatistics CalculateStatistics(List<Question> processedQuestions, bool printStats = false)
    {
        int totalQuestions = processedQuestions.Count;
        int errorCount = processedQuestions.Count(q => !string.IsNullOrEmpty(q.Error));
        int naCount = processedQuestions.Count(q =>
        {
            if (q.Value != null)
                return q.Value.ToString() == "N/A";
            if (q.Answer != null)
                return q.Answer.ToString() == "N/A";
            return false;
        });
        int successCount = totalQuestions - errorCount - naCount;

        if (printStats)
        {
            Console.WriteLine("\nFinal Processing Statistics:");
            Console.WriteLine($"Total questions: {totalQuestions}");
            Console.WriteLine($"Errors: {errorCount} ({(totalQuestions > 0 ? (errorCount * 100.0 / totalQuestions) : 0):F1}%)");
            Console.WriteLine($"N/A answers: {naCount} ({(totalQuestions > 0 ? (naCount * 100.0 / totalQuestions) : 0):F1}%)");
            Console.WriteLine($"Successfully answered: {successCount} ({(totalQuestions > 0 ? (successCount * 100.0 / totalQuestions) : 0):F1}%)\n");
        }

        return new QuestionStatistics
        {
            TotalQuestions = totalQuestions,
            ErrorCount = errorCount,
            NaCount = naCount,
            SuccessCount = successCount
        };
    }

    private List<Answer> PostProcessSubmissionAnswers(List<Question> processedQuestions)
    {
        var submissionAnswers = new List<Answer>();

        foreach (var q in processedQuestions)
        {
            var questionText = q.QuestionText ?? q.QuestionRaw ?? "";
            var kind = q.Kind ?? q.Schema ?? "";
            string value = !string.IsNullOrEmpty(q.Error) ? "N/A" :
               q.Value?.ToString() ??
               q.Answer?.ToString() ??
               "";

            var references = q.References ?? [];

            // Extract step_by_step_analysis from answer details if present
            string stepByStepAnalysis = "";
            if (q.AnswerDetail != null)
            {
                var answerDetailsRef = q.AnswerDetail.Ref ?? "";
                if (!string.IsNullOrEmpty(answerDetailsRef) && answerDetailsRef.StartsWith("#/answer_details/"))
                {
                    var parts = answerDetailsRef.Split('/');
                    if (AnswerDetailList != null &&
                    int.TryParse(parts[^1], out int index) &&
                    index >= 0 && index < AnswerDetailList.Count &&
                    AnswerDetailList[index] != null)
                    {
                        stepByStepAnalysis = AnswerDetailList[index].StepByStepAnalysis;
                    }
                }
            }

            // Clear references if value is N/A
            if (value == "N/A")
            {
                references = [];
            }
            else
            {
                // Convert page indices from one-based to zero-based
                for (int i = 0; i < references.Count; i++)
                {
                    references[i].PageIndex = Math.Max(0, references[i].PageIndex - 1);
                }
            }

            var submissionAnswer = new Answer
            {
                QuestionText = questionText,
                Kind = kind,
                Value = value,
                References = references
            };

            if (!string.IsNullOrEmpty(stepByStepAnalysis))
            {
                submissionAnswer.ReasoningProcess = stepByStepAnalysis;
            }

            submissionAnswers.Add(submissionAnswer);
        }

        return submissionAnswers;
    }



    /// <summary>
    /// Processes a single question and returns a strongly typed result.
    /// </summary>
    /// <param name="questionData">Input question data as a dictionary.</param>
    /// <returns>Strongly typed result of the processing.</returns>
    private async Task<Question> ProcessSingleQuestionAsync(QuestionRoot questionData, CancellationToken cancellationToken = default)
    {
        int questionIndex = questionData.Index;

        string questionText;
        string schemaOrKind;

        if (newChallengePipeline)
        {
            questionText = questionData.Text ?? "";
            schemaOrKind = questionData.Kind ?? "";
        }
        else
        {
            questionText = questionData.Question ?? "";
            schemaOrKind = questionData.Schema ?? "";
        }

        try
        {
            // Assume ProcessQuestion returns a Dictionary<string, object>
            var answerDict = await ProcessQuestionAsync(questionText, schemaOrKind, cancellationToken);

            if (!string.IsNullOrEmpty(answerDict.Error))
            {
                var detailRef = CreateAnswerDetailRef(new(), questionIndex);

                if (newChallengePipeline)
                {
                    return new Question
                    {
                        QuestionText = questionText,
                        Kind = schemaOrKind,
                        Value = "",
                        References = [],
                        Error = answerDict.Error,
                        AnswerDetail = new AnswerDetail { Ref = detailRef }
                    };
                }
                else
                {
                    return new Question
                    {
                        QuestionText = questionText,
                        Schema = schemaOrKind,
                        Answer = "",
                        References = [],
                        Error = answerDict.Error,
                        AnswerDetail = new AnswerDetail { Ref = detailRef }
                    };
                }
            }

            var answerDetailsRef = CreateAnswerDetailRef(answerDict, questionIndex);
            if (newChallengePipeline)
            {
                return new Question
                {
                    QuestionText = questionText,
                    Kind = schemaOrKind,
                    Value = answerDict.FinalAnswer,
                    References = answerDict.References,
                    AnswerDetail = new AnswerDetail { Ref = answerDetailsRef }
                };
            }
            else
            {
                return new Question
                {
                    QuestionText = questionText,
                    Schema = schemaOrKind,
                    Value = answerDict.FinalAnswer,
                    References = answerDict.References,
                    AnswerDetail = new AnswerDetail { Ref = answerDetailsRef }
                };
            }
        }
        catch (Exception ex)
        {
            return HandleProcessingError(questionText, schemaOrKind, ex, questionIndex);
        }
    }

    private Question HandleProcessingError(string questionText, string schema, Exception err, int questionIndex)
    {
        string errorMessage = err.Message;
        string tb = err.ToString(); // Includes stack trace
        string errorRef = $"#/answer_details/{questionIndex}";
        var errorDetail = new AnswerDetail
        {
            ErrorTraceback = tb,
            Self = errorRef
        };

        AnswerDetailList[questionIndex] = errorDetail;

        if (newChallengePipeline)
        {
            return new Question
            {
                QuestionText = questionText,
                Kind = schema,
                Value = "",
                References = [],
                Error = $"{err.GetType().Name}: {errorMessage}",
                AnswerDetail = new AnswerDetail { Ref = errorRef }
            };
        }
        else
        {
            return new Question
            {
                QuestionRaw = questionText,
                Schema = schema,
                Answer = null,
                Error = $"{err.GetType().Name}: {errorMessage}",
                AnswerDetail = new AnswerDetail { Ref = errorRef }
            };
        }
    }

    /// <summary>
    /// Extract company names from a question by matching against companies in the subset file.
    /// </summary>
    private List<string> ExtractCompaniesFromSubset(string questionText)
    {
        // Load companies from CSV if not already loaded
        if (companies_datas == null)
        {
            if (string.IsNullOrEmpty(subsetPath))
                throw new InvalidOperationException("subsetPath must be provided to use subset extraction");

            companies_datas = IPDFParser.ParseCsvMetadata(subsetPath);
        }

        var foundCompanies = new List<string>();
        // Get unique company names, sorted by length descending
        var companyNames = companies_datas
            .Select(row => row.Value.GetCompanyName())
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .OrderByDescending(name => name.Length)
            .ToList();

        foreach (var company in companyNames)
        {
            var escapedCompany = Regex.Escape(company);
            var pattern = $@"{escapedCompany}(?:\W|$)";

            if (Regex.IsMatch(questionText, pattern, RegexOptions.IgnoreCase))
            {
                foundCompanies.Add(company);
                // Remove matched company from questionText to avoid duplicate matches
                questionText = Regex.Replace(questionText, pattern, "", RegexOptions.IgnoreCase);
            }
        }

        return foundCompanies;
    }

    private string CreateAnswerDetailRef(RephrasedQuestions answerDict, int questionIndex)
    {
        string refId = $"#/answer_details/{questionIndex}";

        AnswerDetailList[questionIndex] = new AnswerDetail
        {
            StepByStepAnalysis = answerDict.StepByStepAnalysis,
            ReasoningSummary = answerDict.ReasoningSummary,
            RelevantPages = answerDict.RelevantPages,
            ResponseData = responseData,
            Self = refId
        };
        return refId;
    }

    #endregion
}

public class ProcessQuestionsResult
{
    public List<Question> Questions { get; set; }
    public List<AnswerDetail> AnswerDetails { get; set; }
    public QuestionStatistics Statistics { get; set; }
}

public class QuestionRoot
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("kind")]
    public string Kind { get; set; }
    [JsonPropertyName("schema")]
    public string Schema { get; set; }

    [JsonPropertyName("question")]
    public string Question { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }
}

public class Answer
{
    [JsonPropertyName("question_text")]
    public string QuestionText { get; set; }

    [JsonPropertyName("kind")]
    public string Kind { get; set; }

    [JsonPropertyName("value")]
    public object Value { get; set; }

    [JsonPropertyName("references")]
    public List<ReferenceKey> References { get; set; }

    [JsonPropertyName("reasoning_process")]
    public string ReasoningProcess { get; set; }
}

public class AnswerInfo
{
    [JsonPropertyName("answers")]
    public List<Answer> Answers { get; set; }

    [JsonPropertyName("team_email")]
    public string TeamEmail { get; set; }

    [JsonPropertyName("submission_name")]
    public string SubmissionName { get; set; }

    [JsonPropertyName("details")]
    public string Details { get; set; }

    [JsonPropertyName("questions")]
    public List<Question> Questions { get; set; }

    [JsonPropertyName("answer_details")]
    public List<AnswerDetail> AnswerDetails { get; set; }

    [JsonPropertyName("statistics")]
    public QuestionStatistics Statistics { get; set; }
}

public class AnswerDetail
{
    [JsonPropertyName("step_by_step_analysis")]
    public string StepByStepAnalysis { get; set; }

    [JsonPropertyName("reasoning_summary")]
    public string ReasoningSummary { get; set; }

    [JsonPropertyName("relevant_pages")]
    public List<int> RelevantPages { get; set; }

    [JsonPropertyName("response_data")]
    public ResponseData ResponseData { get; set; }

    [JsonPropertyName("self")]
    public string Self { get; set; }

    [JsonPropertyName("$ref")]
    public string Ref { get; set; }

    [JsonPropertyName("error_traceback")]
    public string ErrorTraceback { get; set; }
}

public class Question
{
    [JsonPropertyName("question_text")]
    public string QuestionText { get; set; }

    [JsonPropertyName("kind")]
    public string Kind { get; set; }

    [JsonPropertyName("value")]
    public object Value { get; set; }

    [JsonPropertyName("references")]
    public List<ReferenceKey> References { get; set; }

    [JsonPropertyName("answer_details")]
    public AnswerDetail AnswerDetail { get; set; }

    [JsonPropertyName("question")]
    public string QuestionRaw { get; set; }

    [JsonPropertyName("schema")]
    public string? Schema { get; set; }

    [JsonPropertyName("answer")]
    public object? Answer { get; set; }

    [JsonPropertyName("error")]
    public string Error { get; set; }
}

public class QuestionStatistics
{
    [JsonPropertyName("total_questions")]
    public int TotalQuestions { get; set; }

    [JsonPropertyName("error_count")]
    public int ErrorCount { get; set; }

    [JsonPropertyName("na_count")]
    public int NaCount { get; set; }

    [JsonPropertyName("success_count")]
    public int SuccessCount { get; set; }
}

