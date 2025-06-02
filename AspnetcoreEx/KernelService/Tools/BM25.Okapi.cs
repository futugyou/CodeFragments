namespace AspnetcoreEx.KernelService.Tools;

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
