
namespace AspnetcoreEx.KernelService.Tools;

public class BM25L : BM25
{
    public BM25L(IEnumerable<string> corpus, Func<string, List<string>>? tokenizer = null, double k1 = 1.5, double b = 0.75, double delta = 0.5)
        : base(corpus, tokenizer)
    {
        K1 = k1;
        B = b;
        Delta = delta;
    }

    protected override void CalcIdf(Dictionary<string, int> nd)
    {
        foreach (var pair in nd)
        {
            var word = pair.Key;
            var freq = pair.Value;
            IDF[word] = Math.Log((CorpusSize + 1.0) / (freq + 0.5));
        }
    }

    public override double[] GetScores(List<string> query)
    {
        var scores = new double[CorpusSize];

        for (int i = 0; i < CorpusSize; i++)
        {
            var docFreq = DocumentFrequencies[i];
            int docLen = DocumentLengths[i];

            foreach (var term in query)
            {
                docFreq.TryGetValue(term, out var f);
                if (!IDF.TryGetValue(term, out var idf)) continue;

                double ctd = f / (1.0 - B + B * docLen / AverageDocumentLength);
                double score = idf * (K1 + 1.0) * (ctd + Delta) / (K1 + ctd + Delta);
                scores[i] += score;
            }
        }

        return scores;
    }

    public override double[] GetBatchScores(List<string> query, List<int> docIds)
    {
        // Assert that all docIds are in range
        var score = new double[docIds.Count];
        var docLen = docIds.Select(di => DocumentLengths[di]).ToArray();

        foreach (var q in query)
        {
            var qFreq = docIds.Select(di => DocumentFrequencies[di].TryGetValue(q, out int value) ? value : 0).ToArray();
            var ctd = qFreq.Select((freq, i) => freq / (1 - B + B * docLen[i] / AverageDocumentLength)).ToArray();
            var idfVal = IDF.TryGetValue(q, out double value) ? value : 0.0;
            for (int i = 0; i < score.Length; i++)
            {
                score[i] += idfVal * (K1 + 1) * (ctd[i] + Delta) / (K1 + ctd[i] + Delta);
            }
        }
        return score;
    }
}
