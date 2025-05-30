namespace AspnetcoreEx.KernelService.Tools;

using System.Text.Json;
using System.Text.Json.Nodes;

public class TextSplitter
{
    private readonly ITokenCounter _tokenCounter = new SharpTokenCounter();
    public JsonObject SplitReport(JsonObject fileContent, string? serializedTablesPath = null)
    {
        var chunks = new JsonArray();
        int chunkId = 0;

        var tablesByPage = new Dictionary<int, List<JsonObject>>();
        if (!string.IsNullOrEmpty(serializedTablesPath) && File.Exists(serializedTablesPath))
        {
            var json = JsonNode.Parse(File.ReadAllText(serializedTablesPath))?.AsObject();
            var tables = json?["tables"]?.AsArray() ?? new JsonArray();
            tablesByPage = GetSerializedTablesByPage(tables);
        }

        foreach (var page in fileContent["content"]!["pages"]!.AsArray())
        {
            int pageNumber = page!["page"]!.GetValue<int>();
            var pageText = page!["text"]!.GetValue<string>();
            var pageChunks = SplitPage(pageText, pageNumber);

            foreach (var chunk in pageChunks)
            {
                chunk["id"] = chunkId++;
                chunk["type"] = "content";
                chunks.Add(chunk);
            }

            if (tablesByPage.TryGetValue(pageNumber, out List<JsonObject>? value))
            {
                foreach (var table in value)
                {
                    table["id"] = chunkId++;
                    table["type"] = "serialized_table";
                    chunks.Add(table);
                }
            }
        }

        fileContent["content"]!["chunks"] = chunks;
        return fileContent;
    }

    private Dictionary<int, List<JsonObject>> GetSerializedTablesByPage(JsonArray tables)
    {
        var result = new Dictionary<int, List<JsonObject>>();
        foreach (var t in tables)
        {
            var table = t!.AsObject();
            if (!table.ContainsKey("serialized")) continue;

            int page = table["page"]!.GetValue<int>();
            var infoBlocks = table["serialized"]!["information_blocks"]!.AsArray();
            var text = string.Join("\n", infoBlocks.Select(b => b!["information_block"]!.GetValue<string>()));

            if (!result.ContainsKey(page))
                result[page] = [];

            result[page].Add(new JsonObject
            {
                ["page"] = page,
                ["text"] = text,
                ["table_id"] = table["table_id"]!.GetValue<string>(),
                ["length_tokens"] = _tokenCounter.Count(text)
            });
        }

        return result;
    }

    private List<JsonObject> SplitPage(string text, int page, int chunkSize = 300, int overlap = 50)
    {
        var chunks = new List<JsonObject>();
        int pos = 0;

        while (pos < text.Length)
        {
            int len = Math.Min(chunkSize, text.Length - pos);
            string chunkText = text.Substring(pos, len);
            int tokens = _tokenCounter.Count(chunkText);

            chunks.Add(new JsonObject
            {
                ["page"] = page,
                ["text"] = chunkText,
                ["length_tokens"] = tokens
            });

            pos += chunkSize - overlap;
        }

        return chunks;
    }

    public void SplitAllReports(string inputDir, string outputDir, string? serializedTableDir = null)
    {
        Directory.CreateDirectory(outputDir);
        var files = Directory.GetFiles(inputDir, "*.json");

        foreach (var file in files)
        {
            string filename = System.IO.Path.GetFileName(file);
            string? serializedPath = serializedTableDir != null
                ? System.IO.Path.Combine(serializedTableDir, filename)
                : null;

            var content = JsonNode.Parse(File.ReadAllText(file))!.AsObject();
            var updated = SplitReport(content, serializedPath);
            File.WriteAllText(System.IO.Path.Combine(outputDir, filename), updated.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }

        Console.WriteLine($"Split {files.Length} files.");
    }
}