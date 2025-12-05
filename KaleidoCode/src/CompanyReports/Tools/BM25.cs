
namespace CompanyReports.BM25;

[JsonSerializable(typeof(BM25Plus))]
[JsonSerializable(typeof(BM25Okapi))]
[JsonSerializable(typeof(BM25L))]
[JsonSerializable(typeof(BM25))]
internal sealed partial class BM25JsonContext : JsonSerializerContext;

public abstract class BM25
{
    [JsonPropertyName("corpus")]
    public List<List<string>> Corpus { get; set; }
    [JsonPropertyName("corpus_size")]
    public int CorpusSize { get; set; }

    [JsonPropertyName("doc_freqs")]
    public List<Dictionary<string, int>> DocumentFrequencies { get; set; }

    [JsonPropertyName("idf")]
    public Dictionary<string, double> IDF { get; set; }

    [JsonPropertyName("doc_len")]
    public List<int> DocumentLengths { get; set; }

    [JsonPropertyName("avgdl")]
    public double AverageDocumentLength { get; set; }

    [JsonPropertyName("k1")]
    public double K1 { get; set; }

    [JsonPropertyName("b")]
    public double B { get; set; }

    [JsonPropertyName("epsilon")]
    public double Epsilon { get; set; }

    [JsonPropertyName("delta")]
    public double Delta { get; set; }
    [JsonPropertyName("average_idf")]
    public double AverageIDF { get; set; }

    [JsonIgnore]
    protected Func<string, List<string>>? Tokenizer;

    protected BM25(IEnumerable<string> rawCorpus, Func<string, List<string>>? tokenizer = null)
    {
        CorpusSize = 0;
        AverageDocumentLength = 0;
        DocumentFrequencies = [];
        IDF = [];
        DocumentLengths = [];
        Tokenizer = tokenizer;

        Corpus = tokenizer != null
            ? rawCorpus.Select(tokenizer).ToList()
            : [.. rawCorpus.Select(doc => doc.Split(' ').ToList())];

        var nd = Initialize(Corpus);
        CalcIdf(nd);
    }

    protected Dictionary<string, int> Initialize(List<List<string>> corpus)
    {
        var nd = new Dictionary<string, int>();
        var totalTerms = 0;

        foreach (var document in corpus)
        {
            DocumentLengths.Add(document.Count);
            totalTerms += document.Count;

            var frequencies = new Dictionary<string, int>();
            foreach (var word in document)
            {
                if (!frequencies.ContainsKey(word))
                    frequencies[word] = 0;
                frequencies[word]++;
            }

            DocumentFrequencies.Add(frequencies);

            foreach (var word in frequencies.Keys)
            {
                nd[word] = nd.TryGetValue(word, out var count) ? count + 1 : 1;
            }

            CorpusSize++;
        }

        AverageDocumentLength = (double)totalTerms / CorpusSize;
        return nd;
    }

    protected abstract void CalcIdf(Dictionary<string, int> nd);
    public abstract double[] GetScores(List<string> query);
    public abstract double[] GetBatchScores(List<string> query, List<int> docIds);

    /// <summary>
    /// Get the top N document indexes and scores most relevant to the query.
    /// </summary>
    /// <returns>A list of tuples of document indexes and scores</returns>
    public List<(string Document, double Score)> GetTopN(List<string> query, List<string> documents, int n = 5)
    {
        if (CorpusSize != documents.Count)
            throw new ArgumentException("The documents given don't match the index corpus!");

        var scores = GetScores(query);
        return [.. documents
            .Select((doc, idx) => (Document: doc, Score: scores[idx]))
            .OrderByDescending(x => x.Score)
            .Take(n)];
    }
}
