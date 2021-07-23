using BenchmarkDotNet.Running;
using System;
using System.Tools;

namespace CodeFragments
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //await PipelinesTest.Show();

            //var cardid = "61021118850526301x";
            //Console.WriteLine(cardid.IsChineseIDCard());

            //var pool = new ObjectPoolTest();
            //pool.DefalultPolicy();
            //pool.CustomPolicy();

            //await JsonAsyncEnumerable.Deserialize();
            //await JsonAsyncEnumerable.Serialize();

            //JsonNodeDemo.Test();

            //NewLinqApiDemo.Test();
            //BufferDemo.Exection();
            //var summary = BenchmarkRunner.Run<SpanTest>();
            await AsyncEnumerableDemo.Exection();
            Console.ReadLine();
        }
    }
}
