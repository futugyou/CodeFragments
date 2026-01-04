
namespace CompanyReports.HtmlExporter;

public class HtmlTagsExporter
{
    public static string ExportGridToHtml(List<List<string>> grid)
    {
        if (grid == null || grid.Count == 0)
            return string.Empty;

        var html = new HtmlTag("html");
        var head = new HtmlTag("head")
            .Append(new HtmlTag("meta").Attr("charset", "utf-8"))
            .Append(new HtmlTag("title").Text("Exported Table"));

        var body = new HtmlTag("body");
        var table = new HtmlTag("table");

        // table header
        var thead = new HtmlTag("thead");
        var headerRow = new HtmlTag("tr");
        foreach (var cell in grid[0])
        {
            headerRow.Append(new HtmlTag("th").Text(cell));
        }
        thead.Append(headerRow);
        table.Append(thead);

        // table body
        var tbody = new HtmlTag("tbody");
        for (int i = 1; i < grid.Count; i++)
        {
            var dataRow = new HtmlTag("tr");
            foreach (var cell in grid[i])
            {
                dataRow.Append(new HtmlTag("td").Text(cell));
            }
            tbody.Append(dataRow);
        }
        table.Append(tbody);

        body.Append(table);
        html.Append(head);
        html.Append(body);

        return html.ToString();
    }
}