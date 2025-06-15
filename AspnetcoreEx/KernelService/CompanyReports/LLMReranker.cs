
using System.Text.Json.Schema;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AspnetcoreEx.KernelService.CompanyReports;

public class LLMReranker
{
    private readonly OpenAIClient _llmClient;
    private readonly string _systemPromptSingleBlock;
    private readonly string _systemPromptMultipleBlocks;
    private readonly string _schemaSingleBlock;
    private readonly string _schemaMultipleBlocks;

    public LLMReranker([FromKeyedServices("report")] OpenAIClient client)
    {
        _llmClient = client;
        _systemPromptSingleBlock = RerankingPrompt.SystemPrompt;
        _systemPromptMultipleBlocks = RerankingPrompt.SystemPromptMultipleBlocks;
        _schemaSingleBlock = Prompts.DefaultJsonOptions.GetJsonSchemaAsNode(typeof(RetrievalRankingSingleBlock)).ToString();
        _schemaMultipleBlocks = Prompts.DefaultJsonOptions.GetJsonSchemaAsNode(typeof(RetrievalRankingMultipleBlocks)).ToString();
    }

    public async Task<List<RetrievalResult>> RerankDocumentsAsync(
        string query,
        List<RetrievalResult> documents,
        int documentsBatchSize = 4,
        double llmWeight = 0.7,
        CancellationToken cancellationToken = default)
    {
        var docBatches = documents
            .Select((doc, idx) => new { doc, idx })
            .GroupBy(x => x.idx / documentsBatchSize)
            .Select(g => g.Select(x => x.doc).ToList())
            .ToList();

        double vectorWeight = 1 - llmWeight;
        var allResults = new List<RetrievalResult>();

        if (documentsBatchSize == 1)
        {
            var tasks = documents.Select(async doc =>
            {
                var ranking = await GetRankForSingleBlockAsync(query, doc.Text, cancellationToken);
                doc.RelevanceScore = double.Parse(ranking["relevance_score"].ToString() ?? "0.0");
                doc.CombinedScore = Math.Round(
                        llmWeight * Convert.ToDouble(ranking["relevance_score"]) +
                        vectorWeight * doc.Distance, 4);
                return doc;
            });
            allResults = [.. await Task.WhenAll(tasks)];
        }
        else
        {
            var tasks = docBatches.Select(async batch =>
            {
                var texts = batch.Select(d => d.Text).ToList();
                var rankings = await GetRankForMultipleBlocksAsync(query, texts);
                var blockRankings = rankings.TryGetValue("block_rankings", out object? value)
                    ? (List<Dictionary<string, object>>)value : [];

                while (blockRankings.Count < batch.Count)
                {
                    blockRankings.Add(new Dictionary<string, object>
                    {
                        ["relevance_score"] = 0.0,
                        ["reasoning"] = "Default ranking due to missing LLM response"
                    });
                }

                var results = batch.Zip(blockRankings, (doc, rank) =>
                {
                    doc.RelevanceScore = double.Parse(rank["relevance_score"].ToString() ?? "0.0");
                    doc.CombinedScore = Math.Round(
                            llmWeight * Convert.ToDouble(rank["relevance_score"]) +
                            vectorWeight * doc.Distance, 4);
                    return doc;
                }).ToList();

                return results;
            });

            var batchResults = await Task.WhenAll(tasks);
            allResults = [.. batchResults.SelectMany(x => x)];
        }

        // Sort results by combined_score in descending order
        allResults = [.. allResults.OrderByDescending(x => x.CombinedScore)];
        return allResults;
    }

    #region private methods
    private async Task<Dictionary<string, object>> GetRankForSingleBlockAsync(string query, string retrievedDocument, CancellationToken cancellationToken = default)
    {
        var userPrompt = $"\nHere is the query:\n\"{query}\"\n\nHere is the retrieved text block:\n\"\"\"\n{retrievedDocument}\n\"\"\"\n";
        if (!string.IsNullOrEmpty(_schemaSingleBlock))
        {
            userPrompt += $"\"\"\"\n{_schemaSingleBlock}\n\"\"\"\n";
        }
        var client = _llmClient.GetChatClient("gpt-4o-mini-2024-07-18").AsIChatClient();
        var history = new ChatMessage[]
               {
            new(ChatRole.System, _systemPromptSingleBlock),
            new(ChatRole.User, userPrompt)
       };
        var options = new ChatOptions
        {
            Temperature = 0,
        };
        var content = await client.GetResponseAsync(history, options, cancellationToken: cancellationToken);
        return JsonSerializer.Deserialize<Dictionary<string, object>>(content?.Text ?? "{}") ?? [];
    }

    private async Task<Dictionary<string, object>> GetRankForMultipleBlocksAsync(string query, List<string> retrievedDocuments, CancellationToken cancellationToken = default)
    {
        var formattedBlocks = string.Join("\n\n---\n\n", retrievedDocuments.Select((text, i) => $"Block {i + 1}:\n\n\"\"\"\n{text}\n\"\"\""));
        var userPrompt = $"Here is the query: \"{query}\"\n\nHere are the retrieved text blocks:\n{formattedBlocks}\n\nYou should provide exactly {retrievedDocuments.Count} rankings, in order.";
        if (!string.IsNullOrEmpty(_schemaMultipleBlocks))
        {
            userPrompt += $"\"\"\"\n{_schemaMultipleBlocks}\n\"\"\"\n";
        }
        var client = _llmClient.GetChatClient("gpt-4o-mini-2024-07-18").AsIChatClient();
        var history = new ChatMessage[]
               {
            new(ChatRole.System, _systemPromptMultipleBlocks),
            new(ChatRole.User, userPrompt)
       };
        var options = new ChatOptions
        {
            Temperature = 0,
        };
        var content = await client.GetResponseAsync(history, options, cancellationToken: cancellationToken);
        return JsonSerializer.Deserialize<Dictionary<string, object>>(content?.Text ?? "{}") ?? [];
    }

    #endregion

}