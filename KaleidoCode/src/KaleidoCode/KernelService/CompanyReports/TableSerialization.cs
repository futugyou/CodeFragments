
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace KaleidoCode.KernelService.CompanyReports;

public class TableSerialization
{
    public const string SystemPrompt = """
        You are a table serialization agent.\n
        Your task is to create a set of contextually independent blocks of information based on the provided table and surrounding text.\n
        These blocks must be totally context-independent because they will be used as separate chunk to populate database.
        """;
}

/// <summary>
/// A single self-contained information block enriched with comprehensive context
/// </summary>
public class SerializedInformationBlock
{
    [JsonPropertyName("subject_core_entity")]
    [Description("A primary focus of what this block is about. Usually located in a row header. If one row in the table doesn't make sense without neighboring rows, you can merge information from neighboring rows into one block")]
    public string SubjectCoreEntity { get; set; }

    [JsonPropertyName("information_block")]
    [Description("""
    Detailed information about the chosen core subject from tables and additional texts. Information SHOULD include:\n
    1. All related header information\n
    2. All related units and their descriptions\n
        2.1. If header is Total, always write additional context about what this total represents in this block!\n
    3. All additional info for context enrichment to make ensure complete context-independency if it present in whole table. This can include:\n
        - The name of the table\n
        - Additional footnotes\n
        - The currency used\n
        - The way amounts are presented\n
        - Anything else that can make context even slightly richer\n
    SKIPPING ANY VALUABLE INFORMATION WILL BE HEAVILY PENALIZED!
    """)]
    public string InformationBlock { get; set; }
}

/// <summary>
/// Collection of serialized table blocks with their core entities and header relationships
/// </summary>
public class TableBlocksCollection
{
    [JsonPropertyName("subject_core_entities_list")]
    [Description("A complete list of core entities. Keep in mind, empty headers are possible - they should also be interpreted and listed (Usually it's a total or something similar). In most cases each row header represents a core entity")]
    public List<string> SubjectCoreEntitiesList { get; set; }

    [JsonPropertyName("relevant_headers_list")]
    [Description("A list of ALL headers relevant to the subject. These headers will serve as keys in each information block. In most cases each column header represents a core entity")]
    public List<string> RelevantHeadersList { get; set; } = [];

    [JsonPropertyName("information_blocks")]
    [Description("Complete list of fully described context-independent information blocks")]
    public List<SerializedInformationBlock> InformationBlocks { get; set; } = [];
}