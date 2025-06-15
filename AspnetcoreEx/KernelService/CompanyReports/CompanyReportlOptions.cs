
using Path = System.IO.Path;

namespace AspnetcoreEx.KernelService.CompanyReports;

public class CompanyReportlOptions
{
    public bool UseSerializedTables { get; set; } = false;
    public bool SerializedTablesInsteadOfMarkdown { get; set; } = false;
    public bool ParentDocumentRetrieval { get; set; } = true;
    public bool LlmReranking { get; set; } = true;
    public int ParallelRequests { get; set; } = 10;
    public int LlmRerankingSampleSize { get; set; } = 10;
    public int TopNRetrieval { get; set; } = 5;
    public bool ReturnParentPages { get; set; }
    public bool NewChallengePipeline { get; set; }
    public string SubmissionName { get; set; } = "";
    public string PipelineDetails { get; set; } = "";
    public string AnsweringModel { get; set; } = "gpt-3.5-turbo";
    public string ConfigSuffix { get; set; } = "";
    public string ApiProvider { get; set; } = "OpenAI";
    public string LlmApiKey { get; set; } = "";
    public string PDFProvider { get; set; } = "docling";
    public bool FullContext { get; set; } = false;
    public bool SubmissionFile { get; set; }
    public string TeamEmail { get; set; } = "";
    public string RootPath { get; set; } = "./data";
    public string SubsetName { get; set; } = "subset.csv";
    public string QuestionsFileName { get; set; } = "questions.json";
    public string PdfReportsName { get; set; } = "pdf_reports";

    public string SubsetPath => Path.Combine(RootPath, SubsetName);
    public string QuestionsFilePath => Path.Combine(RootPath, QuestionsFileName);
    public string PdfReportsDir => Path.Combine(RootPath, PdfReportsName);
    public string AnswersFilePath => Path.Combine(RootPath, $"answers{ConfigSuffix}.json");

    public string DebugDataPath => Path.Combine(RootPath, "debug_data");

    public string Suffix => UseSerializedTables ? "_ser_tab" : "";
    public string DatabasesPath => Path.Combine(RootPath, $"databases{Suffix}");

    public string VectorDbDir => Path.Combine(DatabasesPath, "vector_dbs");
    public string DocumentsDir => Path.Combine(DatabasesPath, "chunked_reports");
    public string Bm25DbPath => Path.Combine(DatabasesPath, "bm25_dbs");

    public string ParsedReportsDirName => "01_parsed_reports";
    public string ParsedReportsDebugDirName => "01_parsed_reports_debug";
    public string MergedReportsDirName => $"02_merged_reports{Suffix}";
    public string ReportsMarkdownDirName => $"03_reports_markdown{Suffix}";

    public string ParsedReportsPath => Path.Combine(DebugDataPath, ParsedReportsDirName);
    public string ParsedReportsDebugPath => Path.Combine(DebugDataPath, ParsedReportsDebugDirName);
    public string MergedReportsPath => Path.Combine(DebugDataPath, MergedReportsDirName);
    public string ReportsMarkdownPath => Path.Combine(DebugDataPath, ReportsMarkdownDirName);
}