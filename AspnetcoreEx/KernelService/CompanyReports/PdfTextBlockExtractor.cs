using UglyToad.PdfPig.Content;

namespace AspnetcoreEx.KernelService.CompanyReports;

public static class PdfTextBlockExtractor
{
    public static List<TextBlock> ExtractTextBlocks(IEnumerable<Page> pages)
    {
        var allLetters = pages
            .SelectMany(p => p.Letters.Select(l => new LetterWithPage(l, p.Number)))
            .Where(x => !char.IsWhiteSpace(x.Letter.Value[0]))
            .OrderByDescending(x => x.Letter.GlyphRectangle.Bottom)
            .ToList();

        var blocks = new List<TextBlock>();

        // Group by page
        var lettersByPage = allLetters.GroupBy(x => x.PageNumber).OrderBy(g => g.Key);
        foreach (var pageGroup in lettersByPage)
        {
            var letters = pageGroup.Select(x => x.Letter).ToList();
            var lines = GroupLettersIntoLines(letters, 5);
            var paraBlocks = GroupLinesIntoBlocks(lines, 10);

            foreach (var block in paraBlocks)
            {
                var blockLetters = block.SelectMany(line => line).ToList();
                var text = string.Join("\n", block.Select(line => string.Concat(line.Select(l => l.Value))));
                var bounds = GetBlockBounds(blockLetters);
                var fontSize = blockLetters.Average(l => l.FontSize);
                var pageStart = pageGroup.Key;
                var pageEnd = pageGroup.Key;

                blocks.Add(new TextBlock
                {
                    Text = text,
                    Bounds = bounds,
                    FontSize = fontSize,
                    PageStart = pageStart,
                    PageEnd = pageEnd
                });
            }
        }

        // Optional: Merge across pages (if necessary, can be expanded according to actual needs)
        // 2. Merge across pages
        for (int i = blocks.Count - 2; i >= 0; i--)
        {
            var prev = blocks[i];
            var next = blocks[i + 1];
            if (ShouldMergeBlocks(prev, next))
            {
                prev.MergeWith(next);
                blocks.RemoveAt(i + 1);
            }
        }

        return blocks;
    }

    private static List<List<Letter>> GroupLettersIntoLines(List<Letter> letters, double lineSpacingThreshold)
    {
        var lines = new List<List<Letter>>();
        foreach (var letter in letters)
        {
            var matchedLine = lines.FirstOrDefault(line =>
                Math.Abs(line[0].GlyphRectangle.Bottom - letter.GlyphRectangle.Bottom) < lineSpacingThreshold);

            if (matchedLine != null)
                matchedLine.Add(letter);
            else
                lines.Add([letter]);
        }
        foreach (var line in lines)
            line.Sort((a, b) => a.GlyphRectangle.Left.CompareTo(b.GlyphRectangle.Left));
        lines.Sort((a, b) => b[0].GlyphRectangle.Bottom.CompareTo(a[0].GlyphRectangle.Bottom));
        return lines;
    }

    private static List<List<List<Letter>>> GroupLinesIntoBlocks(List<List<Letter>> lines, double paragraphSpacingThreshold)
    {
        var blocks = new List<List<List<Letter>>>();
        List<List<Letter>> currentBlock = [];

        for (int i = 0; i < lines.Count; i++)
        {
            var thisLine = lines[i];
            if (currentBlock.Count == 0)
            {
                currentBlock.Add(thisLine);
                continue;
            }
            var prevLine = currentBlock.Last();
            var yGap = prevLine[0].GlyphRectangle.Bottom - thisLine[0].GlyphRectangle.Bottom;
            if (yGap > paragraphSpacingThreshold)
            {
                blocks.Add(currentBlock);
                currentBlock = [];
            }
            currentBlock.Add(thisLine);
        }
        if (currentBlock.Count > 0)
            blocks.Add(currentBlock);
        return blocks;
    }

    private static ReportBbox GetBlockBounds(List<Letter> letters)
    {
        return new ReportBbox
        {
            L = letters.Min(l => l.GlyphRectangle.Left),
            T = letters.Max(l => l.GlyphRectangle.Top),
            R = letters.Max(l => l.GlyphRectangle.Right),
            B = letters.Min(l => l.GlyphRectangle.Bottom)
        };
    }

    // Used for Letter associated page number
    private class LetterWithPage
    {
        public Letter Letter { get; }
        public int PageNumber { get; }
        public LetterWithPage(Letter letter, int pageNumber)
        {
            Letter = letter;
            PageNumber = pageNumber;
        }
    }

    private static bool ShouldMergeBlocks(TextBlock prev, TextBlock next)
    {
        // 1. Page numbers must be continuous
        if (prev.PageEnd + 1 != next.PageStart)
            return false;

        // 2. Font sizes are close
        if (Math.Abs(prev.FontSize - next.FontSize) > 1.5)
            return false;

        // 3. Horizontal position overlaps (X interval)
        double overlap = Math.Min(prev.Bounds.R, next.Bounds.R) - Math.Max(prev.Bounds.L, next.Bounds.L);
        double minWidth = Math.Min(prev.Bounds.R - prev.Bounds.L, next.Bounds.R - next.Bounds.L);
        if (overlap < 0.3 * minWidth) // At least 30% overlap
            return false;

        // 4. The vertical distance cannot be too large (usually 0 or very small when crossing pages)
        double vGap = next.Bounds.T - prev.Bounds.B;
        if (vGap > 20) // Can be adjusted according to the actual PDF
            return false;

        // 5. The end of the previous block is not a paragraph terminator
        string[] endPunctuations = { ".", "。", "！", "!", "？", "？", "：", ":" };
        if (endPunctuations.Any(p => prev.Text.TrimEnd().EndsWith(p)))
            return false;

        return true;
    }
}

public class TextBlock
{
    public int PageStart { get; set; }
    public int PageEnd { get; set; }
    public string Text { get; set; }
    public ReportBbox Bounds { get; set; }
    public double FontSize { get; set; }

    public void MergeWith(TextBlock other)
    {
        Text += "\n" + other.Text;
        PageEnd = other.PageEnd;
        Bounds = ReportBbox.Union(Bounds, other.Bounds);
    }
}
