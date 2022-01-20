using System.Text;

namespace Labuladong;
public class Code0648
{
    public static void Exection()
    {
        var dic = new string[] { "cat", "bat", "rat" };
        var sentence = "the cattle was rattled by the battery";
        string result = ReplaceWords(dic, sentence);
        Console.WriteLine(result);
    }

    private static string ReplaceWords(string[] dic, string sentence)
    {
        var set = new TrieSet();
        foreach (var c in dic)
        {
            set.Add(c);
        }
        var sb = new StringBuilder();
        var words = sentence.Split(" ");
        for (int i = 0; i < words.Length; i++)
        {
            var perfix = set.ShortestPrefixOf(words[i]);
            if (perfix == "")
            {
                sb.Append(words[i]);
            }
            else
            {
                sb.Append(perfix);
            }
            sb.Append(" ");
        }
        return sb.ToString();
    }
}