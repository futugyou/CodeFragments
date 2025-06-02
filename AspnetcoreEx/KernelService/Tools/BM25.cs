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

public class BM25Okapi : BM25
{
    private readonly double _k1;
    private readonly double _b;
    private readonly double _epsilon;
    private double _averageIdf;

    public BM25Okapi(IEnumerable<string> corpus, Func<string, List<string>>? tokenizer = null, double k1 = 1.5, double b = 0.75, double epsilon = 0.25)
        : base(corpus, tokenizer)
    {
        _k1 = k1;
        _b = b;
        _epsilon = epsilon;
    }

    protected override void CalcIdf(Dictionary<string, int> nd)
    {
        double idfSum = 0;
        List<string> negativeIdfs = [];

        foreach (var (word, freq) in nd)
        {
            double idf = Math.Log((CorpusSize - freq + 0.5) / (freq + 0.5));
            Idf[word] = idf;
            idfSum += idf;

            if (idf < 0)
                negativeIdfs.Add(word);
        }

        _averageIdf = idfSum / Idf.Count;
        var eps = _epsilon * _averageIdf;
        foreach (var word in negativeIdfs)
        {
            Idf[word] = eps;
        }
    }

    public override double[] GetScores(List<string> query)
    {
        var scores = new double[CorpusSize];

        for (int i = 0; i < CorpusSize; i++)
        {
            double score = 0;
            var docFreq = DocFrequencies[i];
            int docLen = DocLengths[i];

            foreach (var term in query)
            {
                docFreq.TryGetValue(term, out int freq);
                Idf.TryGetValue(term, out double idf);

                double numerator = freq * (_k1 + 1);
                double denominator = freq + _k1 * (1 - _b + _b * docLen / AvgDocLength);
                score += idf * (numerator / denominator);
            }

            scores[i] = score;
        }

        return scores;
    }

    public override double[] GetBatchScores(List<string> query, List<int> docIds)
    {
        // Assert that all docIds are in range
        if (docIds.Any(di => di < 0 || di >= DocFrequencies.Count))
            throw new ArgumentException("docId out of range");

        var score = new double[docIds.Count];
        var docLenArr = docIds.Select(di => DocLengths[di]).ToArray();

        foreach (var q in query)
        {
            var qFreq = docIds.Select(di => DocFrequencies[di].TryGetValue(q, out int value) ? value : 0).ToArray();
            double idfVal = Idf.TryGetValue(q, out double value) ? value : 0.0;

            for (int i = 0; i < docIds.Count; i++)
            {
                double numerator = qFreq[i] * (_k1 + 1);
                double denominator = qFreq[i] + _k1 * (1 - _b + _b * docLenArr[i] / avgdl);
                score[i] += idfVal * (denominator == 0 ? 0 : numerator / denominator);
            }
        }
        return score;
    }
}
