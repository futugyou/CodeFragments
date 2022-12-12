namespace CodeFragments;

class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        Console.WriteLine("Hello World!");
        // await PipelinesTest.Show();

        // var cardid = "61021118850526301x";
        // Console.WriteLine(cardid.IsChineseIDCard());

        // ObjectPoolUsecase.StringBuilderUsecase();
        // ObjectPoolUsecase.ObjectPolicyWithParameterConstructorUsecase();
        // ObjectPoolUsecase.ObjectPolicyWithObjectLimitUsecase();
        // await ObjectPoolUsecase.ObjectPolicyWithIDisposableUsecase();
        // await ObjectPoolUsecase.ArrayPoolUsecase();
        // await ObjectPoolUsecase.MemoryPoolUsecase();

        // CommandDemo.Exection();

        // await JsonAsyncEnumerable.Deserialize();
        // await JsonAsyncEnumerable.Serialize();

        // JsonNodeDemo.Test();

        // NewLinqApiDemo.Test();
        // BufferDemo.Exection();
        // var summary = BenchmarkRunner.Run<SpanTest>();
        // await AsyncEnumerableDemo.Exection();

        // PriorityQueueDemo.Exection();
        // var m = new MetalamaTest();
        // Console.WriteLine(m.Add(1, 2));

        // await HttpClientChunked.PostWithChunked();
        // await HttpClientChunked.PostWithoutChunked();
        // await HttpClientChunked.HttpClientExceptionUsecase(args);
        await HttpClientChunked.HttpClientHttpMessageHandlerUsecase();

        // await MemoryCacheDemo.MemeoryCacheWithChangeTokenUsecase();
        // await MemoryCacheDemo.MemeoryCacheWithCompactionUsecase();
        Console.ReadLine();
    }
}
