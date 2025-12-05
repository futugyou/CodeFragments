using AngleSharp;
using AngleSharp.Dom;

namespace CompanyReports;

public class AngleSharpExporter
{
    public static async Task<string> ExportGridToHtml(List<List<string>> grid, CancellationToken cancellationToken = default)
    {
        if (grid == null || grid.Count == 0)
            return string.Empty;

        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        var document = await context.OpenNewAsync(cancellation: cancellationToken);

        // Set <head>
        var head = document.CreateElement("head");
        var meta = document.CreateElement("meta");
        meta.SetAttribute("charset", "utf-8");
        head.AppendChild(meta);

        var title = document.CreateElement("title");
        title.TextContent = "Exported Table";
        head.AppendChild(title);
        document.Head!.ReplaceWith(head);

        // Create table
        var table = document.CreateElement("table");

        // Thead (from first row)
        var thead = document.CreateElement("thead");
        var headRow = document.CreateElement("tr");
        foreach (var cellText in grid[0])
        {
            var th = document.CreateElement("th");
            th.TextContent = cellText;
            headRow.AppendChild(th);
        }
        thead.AppendChild(headRow);
        table.AppendChild(thead);

        // Tbody
        var tbody = document.CreateElement("tbody");
        for (int i = 1; i < grid.Count; i++)
        {
            var row = document.CreateElement("tr");
            foreach (var cellText in grid[i])
            {
                var td = document.CreateElement("td");
                td.TextContent = cellText;
                row.AppendChild(td);
            }
            tbody.AppendChild(row);
        }
        table.AppendChild(tbody);

        // Add table to body
        document.Body!.AppendChild(table);

        return document.DocumentElement.OuterHtml;
    }
}