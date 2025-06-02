
namespace AspnetcoreEx.KernelService.Tools;

public class BM25Plus : BM25
{
    private readonly double _k1;
    private readonly double _b;
    private readonly double _delta;

    public BM25Plus(IEnumerable<string> corpus, Func<string, List<string>>? tokenizer = null, double k1 = 1.5, double b = 0.75, double delta = 1.0)
        : base(corpus, tokenizer)
    {
        _k1 = k1;
        _b = b;
        _delta = delta;
    }
    protected override void CalcIdf(Dictionary<string, int> nd)
    {
        foreach (var pair in nd)
        {
            var word = pair.Key;
            var freq = pair.Value;
            Idf[word] = Math.Log((CorpusSize + 1.0) / freq);
        }
    }

    public override double[] GetScores(List<string> query)
    {
        var scores = new double[CorpusSize];

        for (int i = 0; i < CorpusSize; i++)
        {
            var docFreq = DocFrequencies[i];
            int docLen = DocLengths[i];

            foreach (var term in query)
            {
                docFreq.TryGetValue(term, out var f);
                if (!Idf.TryGetValue(term, out var idf)) continue;

                double numerator = f * (_k1 + 1.0);
                double denominator = _k1 * (1.0 - _b + _b * docLen / avgdl) + f;
                double score = idf * (_delta + numerator / denominator);
                scores[i] += score;
            }
        }

        return scores;
    }

    public override double[] GetBatchScores(List<string> query, List<int> docIds)
    {
        // Assert that all docIds are in range
        if (docIds.Any(di => di < 0 || di >= DocFrequencies.Count))
            throw new ArgumentException("docId out of range");

        var score = new double[docIds.Count];
        var docLen = docIds.Select(di => DocLengths[di]).ToArray();

        foreach (var q in query)
        {
            var qFreq = docIds.Select(di => DocFrequencies[di].TryGetValue(q, out int value) ? value : 0).ToArray();
            double idfValue = Idf.TryGetValue(q, out double value) ? value : 0;
            for (int i = 0; i < docIds.Count; i++)
            {
                double numerator = qFreq[i] * (_k1 + 1);
                double denominator = _k1 * (1 - _b + _b * docLen[i] / avgdl) + qFreq[i];
                score[i] += idfValue * (_delta + numerator / denominator);
            }
        }
        return score;
    }
}
