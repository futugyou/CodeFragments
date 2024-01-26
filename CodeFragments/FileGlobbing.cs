using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace CodeFragments;

public class FileGlobbing
{
    public static void Base()
    {
        Matcher matcher = new();
        matcher.AddIncludePatterns(new[] { "*.csv" });

        string searchDirectory = "./file";

        PatternMatchingResult result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(searchDirectory)));
        if (result.HasMatches)
        {
            foreach (var file in result.Files)
            {
                Console.WriteLine(file.Stem);
            }
        }

        IEnumerable<string> matchingFiles = matcher.GetResultsInFullPath(searchDirectory);
        Console.WriteLine(string.Join(",", matchingFiles));
    }
}