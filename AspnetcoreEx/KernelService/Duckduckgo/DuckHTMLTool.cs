using HtmlAgilityPack;

namespace AspnetcoreEx.KernelService.Duckduckgo;

public class DuckHTMLTool
{
    public static List<DuckSearchResult> GenerationDuckSearchResult(string html, List<KeyValuePair<string, string>> nextData)
    {
        var results = new List<DuckSearchResult>();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        nextData.Clear();
        var nodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'result results_links')]");
        if (nodes == null)
        {
            var forms = doc.DocumentNode.SelectNodes("//form");
            if (forms != null && forms.Count > 0)
            {
                var hiddens = forms.Last().SelectNodes("//input[@type='hidden']");
                if (hiddens != null && hiddens.Count > 0)
                {
                    foreach (var hidden in hiddens)
                    {
                        var key = hidden.GetAttributeValue("name", "");
                        if (key != "")
                        {
                            var value = hidden.GetAttributeValue("value", "");
                            nextData.Add(new KeyValuePair<string, string>(key, value));
                        }
                    }
                }
            }
            return results;
        }

        foreach (var node in nodes)
        {
            var titleNode = node.SelectSingleNode(".//a[@class='result__a']");
            var snippetNode = node.SelectSingleNode(".//a[@class='result__snippet']");
            var iconNode = node.SelectSingleNode(".//img[contains(@class, 'result__icon__img')]");
            var displayUrlNode = node.SelectSingleNode(".//a[@class='result__url']");

            if (titleNode == null)
                continue;

            string iconUrl = iconNode?.GetAttributeValue("src", "") ?? "";
            if (!string.IsNullOrEmpty(iconUrl) && iconUrl.StartsWith("//"))
                iconUrl = "https:" + iconUrl;

            results.Add(new DuckSearchResult
            {
                Title = titleNode.InnerText.Trim(),
                Url = titleNode.GetAttributeValue("href", ""),
                Snippet = snippetNode?.InnerText.Trim() ?? "",
                IconUrl = iconUrl ?? "",
                DisplayUrl = displayUrlNode?.InnerText.Trim() ?? ""
            });
        }

        return results;
    }
}