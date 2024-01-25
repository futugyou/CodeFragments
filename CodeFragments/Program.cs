namespace CodeFragments;

class Program
{
    static async Task Main(string[] args)
    {
        await ValueTask.CompletedTask;
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
        // JsonUtf8JsonWriter.Base();
        // JsonUtf8JsonWriter.WriteRawValue();

        // JsonUtf8JsonReader.Base();
        // JsonUtf8JsonReader.Filter();

        // JsonSourceGeneration.Base();

        // JsonCustomContracts.Base();
        // JsonCustomContracts.PrivateFields();
        // JsonCustomContracts.AllowIntString();

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
        // await HttpClientChunked.HttpClientHttpMessageHandlerUsecase();
        // await HttpClientChunked.HttpClientCustomHttpMessageHandlerUsecase();

        // await MemoryCacheDemo.MemeoryCacheWithChangeTokenUsecase();
        // await MemoryCacheDemo.MemeoryCacheWithCompactionUsecase();

        // DataProtectionDemo.BaseUsecase();
        // await DataProtectionDemo.DataProtectionWithTimeLimitUsecase();
        // DataProtectionDemo.DataProtectorRevokeUsecase();
        // DataProtectionDemo.EphemeralDataProtectorUsecase();
        // DataProtectionDemo.GeneratorHashUsecase();
        // DataProtectionDemo.FileSystemDataProtectorUsecase();

        // ScottPlotDemo.ReadFile();

        if (args.Any(p => p.Contains("server")))
        {
            NamedPipesServer.Base();
        }
        if (args.Any(p => p.Contains("client")))
        {
            NamedPipesClient.Base();
        }

        // await PipelinesTest.PipeScheduler();

        if (args.Any(p => p.Contains("mmfw")))
        {
            Mmf.Write();
        }
        if (args.Any(p => p.Contains("mmfr")))
        {
            Mmf.Read();
        }

        // await Resilience.BaseAsync();

        if (args.Any(p => p.Contains("socket-c")))
        {
            await SocketDemo.Client();
        }
        if (args.Any(p => p.Contains("socket-s")))
        {
            await SocketDemo.Server();
        }

        if (args.Any(p => p.Contains("tcp-c")))
        {
            await SocketDemo.Client2();
        }
        if (args.Any(p => p.Contains("tcp-s")))
        {
            await SocketDemo.Listener2();
        }


        Console.ReadLine();
    }
}
