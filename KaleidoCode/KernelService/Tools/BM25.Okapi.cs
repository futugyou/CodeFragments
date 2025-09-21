namespace KaleidoCode.KernelService.Tools;

public class BM25Okapi : BM25
{
    public BM25Okapi(IEnumerable<string> corpus, Func<string, List<string>>? tokenizer = null, double k1 = 1.5, double b = 0.75, double epsilon = 0.25)
        : base(corpus, tokenizer)
    {
        K1 = k1;
        B = b;
        Epsilon = epsilon;
    }

    protected override void CalcIdf(Dictionary<string, int> nd)
    {
        double idfSum = 0;
        List<string> negativeIdfs = [];

        foreach (var (word, freq) in nd)
        {
            double idf = Math.Log((CorpusSize - freq + 0.5) / (freq + 0.5));
            IDF[word] = idf;
            idfSum += idf;

            if (idf < 0)
                negativeIdfs.Add(word);
        }

        AverageIDF = idfSum / IDF.Count;
        var eps = Epsilon * AverageIDF;
        foreach (var word in negativeIdfs)
        {
            IDF[word] = eps;
        }
    }

    public override double[] GetScores(List<string> query)
    {
        var scores = new double[CorpusSize];

        for (int i = 0; i < CorpusSize; i++)
        {
            double score = 0;
            var docFreq = DocumentFrequencies[i];
            int docLen = DocumentLengths[i];

            foreach (var term in query)
            {
                docFreq.TryGetValue(term, out int freq);
                IDF.TryGetValue(term, out double idf);

                double numerator = freq * (K1 + 1);
                double denominator = freq + K1 * (1 - B + B * docLen / AverageDocumentLength);
                score += idf * (numerator / denominator);
            }

            scores[i] = score;
        }

        return scores;
    }

    public override double[] GetBatchScores(List<string> query, List<int> docIds)
    {
        // Assert that all docIds are in range
        if (docIds.Any(di => di < 0 || di >= DocumentFrequencies.Count))
            throw new ArgumentException("docId out of range");

        var score = new double[docIds.Count];
        var docLenArr = docIds.Select(di => DocumentLengths[di]).ToArray();

        foreach (var q in query)
        {
            var qFreq = docIds.Select(di => DocumentFrequencies[di].TryGetValue(q, out int value) ? value : 0).ToArray();
            double idfVal = IDF.TryGetValue(q, out double value) ? value : 0.0;

            for (int i = 0; i < docIds.Count; i++)
            {
                double numerator = qFreq[i] * (K1 + 1);
                double denominator = qFreq[i] + K1 * (1 - B + B * docLenArr[i] / AverageDocumentLength);
                score[i] += idfVal * (denominator == 0 ? 0 : numerator / denominator);
            }
        }
        return score;
    }
}
