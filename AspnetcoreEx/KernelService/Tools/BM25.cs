namespace AspnetcoreEx.KernelService.Tools;

public abstract class BM25
{
    protected readonly double avgdl;
    protected int CorpusSize;
    protected double AvgDocLength;
    protected List<Dictionary<string, int>> DocFrequencies;
    protected Dictionary<string, double> Idf;
    protected List<int> DocLengths;
    protected Func<string, List<string>>? Tokenizer;

    protected BM25(IEnumerable<string> rawCorpus, Func<string, List<string>>? tokenizer = null)
    {
        CorpusSize = 0;
        AvgDocLength = 0;
        DocFrequencies = [];
        Idf = [];
        DocLengths = [];
        Tokenizer = tokenizer;

        var corpus = tokenizer != null
            ? rawCorpus.Select(tokenizer).ToList()
            : [.. rawCorpus.Select(doc => doc.Split(' ').ToList())];

        var nd = Initialize(corpus);
        CalcIdf(nd);
    }

    protected Dictionary<string, int> Initialize(List<List<string>> corpus)
    {
        var nd = new Dictionary<string, int>();
        var totalTerms = 0;

        foreach (var document in corpus)
        {
            DocLengths.Add(document.Count);
            totalTerms += document.Count;

            var frequencies = new Dictionary<string, int>();
            foreach (var word in document)
            {
                if (!frequencies.ContainsKey(word))
                    frequencies[word] = 0;
                frequencies[word]++;
            }

            DocFrequencies.Add(frequencies);

            foreach (var word in frequencies.Keys)
            {
                nd[word] = nd.TryGetValue(word, out var count) ? count + 1 : 1;
            }

            CorpusSize++;
        }

        AvgDocLength = (double)totalTerms / CorpusSize;
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
