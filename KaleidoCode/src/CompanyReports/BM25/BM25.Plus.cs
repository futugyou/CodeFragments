
namespace CompanyReports.BM25;

public class BM25Plus : BM25Abstract
{
    public BM25Plus(IEnumerable<string> corpus, Func<string, List<string>>? tokenizer = null, double k1 = 1.5, double b = 0.75, double delta = 1.0)
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
            IDF[word] = Math.Log((CorpusSize + 1.0) / freq);
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

                double numerator = f * (K1 + 1.0);
                double denominator = K1 * (1.0 - B + B * docLen / AverageDocumentLength) + f;
                double score = idf * (Delta + numerator / denominator);
                scores[i] += score;
            }
        }

        return scores;
    }

    public override double[] GetBatchScores(List<string> query, List<int> docIds)
    {
        // Assert that all docIds are in range
        if (docIds.Any(di => di < 0 || di >= DocumentFrequencies.Count))
            throw new ArgumentException("docId out of range");

        var score = new double[docIds.Count];
        var docLen = docIds.Select(di => DocumentLengths[di]).ToArray();

        foreach (var q in query)
        {
            var qFreq = docIds.Select(di => DocumentFrequencies[di].TryGetValue(q, out int value) ? value : 0).ToArray();
            double idfValue = IDF.TryGetValue(q, out double value) ? value : 0;
            for (int i = 0; i < docIds.Count; i++)
            {
                double numerator = qFreq[i] * (K1 + 1);
                double denominator = K1 * (1 - B + B * docLen[i] / AverageDocumentLength) + qFreq[i];
                score[i] += idfValue * (Delta + numerator / denominator);
            }
        }
        return score;
    }
}
