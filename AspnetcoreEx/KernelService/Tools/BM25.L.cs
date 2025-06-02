
namespace AspnetcoreEx.KernelService.Tools;

public class BM25L : BM25
{
    private readonly double _k1;
    private readonly double _b;
    private readonly double _delta;

    public BM25L(IEnumerable<string> corpus, Func<string, List<string>>? tokenizer = null, double k1 = 1.5, double b = 0.75, double delta = 0.5)
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
            Idf[word] = Math.Log((CorpusSize + 1.0) / (freq + 0.5));
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

                double ctd = f / (1.0 - _b + _b * docLen / avgdl);
                double score = idf * (_k1 + 1.0) * (ctd + _delta) / (_k1 + ctd + _delta);
                scores[i] += score;
            }
        }

        return scores;
    }

    public override double[] GetBatchScores(List<string> query, List<int> docIds)
    {
        // Assert that all docIds are in range
        var score = new double[docIds.Count];
        var docLen = docIds.Select(di => DocLengths[di]).ToArray();

        foreach (var q in query)
        {
            var qFreq = docIds.Select(di => DocFrequencies[di].TryGetValue(q, out int value) ? value : 0).ToArray();
            var ctd = qFreq.Select((freq, i) => freq / (1 - _b + _b * docLen[i] / avgdl)).ToArray();
            var idfVal = Idf.TryGetValue(q, out double value) ? value : 0.0;
            for (int i = 0; i < score.Length; i++)
            {
                score[i] += idfVal * (_k1 + 1) * (ctd[i] + _delta) / (_k1 + ctd[i] + _delta);
            }
        }
        return score;
    }
}
