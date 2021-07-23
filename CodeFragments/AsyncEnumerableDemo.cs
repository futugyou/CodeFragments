using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeFragments
{
    public class AsyncEnumerableDemo
    {
        public static async Task Exection()
        {
            Console.WriteLine(DateTime.Now + $"\tThreadId:{Thread.CurrentThread.ManagedThreadId}\r\n");

            await foreach (var html in FetchAllHtml1())
            {
                Console.WriteLine(DateTime.Now + $"\tThreadId:{Thread.CurrentThread.ManagedThreadId}\t" + $"\toutput:{html}");
            }
            Console.WriteLine("\r\n" + DateTime.Now + $"\tThreadId:{Thread.CurrentThread.ManagedThreadId}\t");

        }
        static async IAsyncEnumerable<string> FetchAllHtml()
        {
            for (int i = 5; i >= 1; i--)
            {
                var html = await Task.Delay(i * 1000).ContinueWith((t, i) => $"html{i}", i);    //  模拟长耗时
                yield return html;
            }
        }

        static async IAsyncEnumerable<string> FetchAllHtml1()
        {
            var tasklist = new List<Task<string>>();
            for (int i = 5; i >= 1; i--)
            {
                var t = Task.Delay(i * 1000).ContinueWith((t, i) => $"html{i}", i);      // 模拟长耗时任务
                tasklist.Add(t);
            }
            while (tasklist.Any())
            {
                var tFinlish = await Task.WhenAny(tasklist);
                tasklist.Remove(tFinlish);
                yield return await tFinlish;
            }
        }
    }
}
