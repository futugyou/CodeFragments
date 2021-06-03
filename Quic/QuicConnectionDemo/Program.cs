using System;
using System.Net.Quic;
using System.Threading.Tasks;

namespace QuicConnectionDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var options = new System.Net.Security.SslClientAuthenticationOptions();
            options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            QuicConnection quic = new(System.Net.IPEndPoint.Parse("192.168.15.135:666"), options);

            await quic.ConnectAsync();

            QuicStream quicStream = await quic.AcceptStreamAsync();

            await quicStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("12345"));

            byte[] buffer;
            int count = await quicStream.ReadAsync(buffer = new byte[1000]);
            string str = System.Text.Encoding.UTF8.GetString(buffer, 0, count);

            Console.WriteLine(str);
            Console.WriteLine("Hello World!");

            Console.ReadKey();
        }
    }
}
