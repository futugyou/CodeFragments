namespace AspnetcoreEx.KernelService.CompanyReports;

public class MarkdownBuilder
{
    public static string ToTable(List<List<string>> tableGrid)
    {
        if (tableGrid == null || tableGrid.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();

        // Check if there is a table header
        bool hasHeader = tableGrid.Count > 1 && tableGrid[0].Count > 0;
        int colCount = tableGrid[0].Count;

        // Output table header
        if (hasHeader)
        {
            sb.AppendLine($"| {string.Join(" | ", tableGrid[0])} |");
            sb.AppendLine($"| {string.Join(" | ", new string[colCount].Select(_ => "---"))} |");

            for (int i = 1; i < tableGrid.Count; i++)
            {
                sb.AppendLine($"| {string.Join(" | ", tableGrid[i])} |");
            }
        }
        else
        {
            foreach (var row in tableGrid)
            {
                sb.AppendLine($"| {string.Join(" | ", row)} |");
            }
        }

        return sb.ToString();
    }
}