using Microsoft.Data.Analysis;

namespace CodeFragments;

public static class ScottPlotDemo
{
    public static void ReadFile()
    {
        var df = DataFrame.LoadCsv("file/titanic.csv");

        df.Head(5);
    }
}