﻿
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

        // FileGlobbing.Base();

        // PrimitivesPackage.Token1();
        // PrimitivesPackage.Token2();
        // PrimitivesPackage.Token3();
        // PrimitivesPackage.String1();

        // ExpressionDemo.Base();
        // ExpressionDemo.ClosureError();
        // ExpressionDemo.Interpreting();
        // ExpressionDemo.CustomVisitor();
        // ExpressionDemo.StandardVisitor();
        // ExpressionDemo.Loop();

        CryptoDemo();

        Console.ReadLine();
    }

    static void CryptoDemo(){
        //var str = DesCrypto.Encrypt("SELECT sb.name [table], sc.name [column], st.name [typename] FROM syscolumns sc,systypes st ,sysobjects sb WHERE sc.xusertype=st.xusertype AND sc.id = sb.id AND sb.xtype='U'",
            //    Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("X-AppKey")),
            //    Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("2014_MyBMW837".Substring(0, 8)))
            //    );
            //var sttt = DesCrypto.Decrypt(str,
            //     Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("X-AppKey")),
            //    Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("2014_MyBMW837".Substring(0, 8)))
            //    );
            //Console.WriteLine(str);
            //Console.WriteLine(sttt);
            //var input = Console.ReadLine();
            //var dm5 = MD5Crypto.ToMD5(input.ToLower(), "ComposeEntity");
            //Console.WriteLine(dm5);
            //Console.WriteLine();
            //str = AesCrypto.AESEncrypt("select *  from {0} order by 1 desc", "X-AppKey", "2014_MyBMW837");
            //sttt = AesCrypto.AESDecrypt(str, "X-AppKey", "2014_MyBMW837");
            //Console.WriteLine(str);
            //Console.WriteLine(sttt);

            //while (true)
            //{
            //    var code_verifier = Console.ReadLine();//5d2309e5bb73b864f989753887fe52f79ce5270395e25862da6940d5
            //    if (code_verifier == "q")
            //    {
            //        break;
            //    }
            //    var e = Sha256Crypto.GetSHA256Base64urlEncoding(code_verifier);
            //    Console.WriteLine(e);
            //}

            var str = $"OrderId=2021052626353443&PayTime=2021-05-26 15:51:18&PayAmount=450.00&Secret=auooAAWP53I5RJ5y5sTK"; ;
            var print = Sha256Crypto.SHA256WithManaged(str);
            Console.WriteLine(print);
    }
}
