using System;
using System.Text;

namespace CryptoDome
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            //Console.WriteLine();
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
            Console.ReadLine();
        }
    }
}
