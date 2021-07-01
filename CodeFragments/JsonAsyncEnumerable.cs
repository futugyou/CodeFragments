using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CodeFragments
{
    public class JsonAsyncEnumerable
    {
        public static async IAsyncEnumerable<int> PrintNumbers(int n)
        {
            for (int i = 0; i < n; i++)
            {
                yield return i;
            }
        }

        public static async Task Serialize()
        {
            using Stream stream = Console.OpenStandardOutput();
            var data = new { Data = PrintNumbers(3) };
            await JsonSerializer.SerializeAsync(stream, data);
        }

        public static async Task Deserialize()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("[0,1,2,3,4]"));
            await foreach (int item in JsonSerializer.DeserializeAsyncEnumerable<int>(stream))
            {
                Console.WriteLine(item);
            }
        }
    }
}
