using System;
using System.IO;
using System.Net.Quic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace QuicListenerDemo
{
    class Program
    {
        static async Task Main(string[] args)
        { 
            var options = new System.Net.Security.SslServerAuthenticationOptions();
            options.CertificateRevocationCheckMode = X509RevocationMode.NoCheck;
            options.ServerCertificate = new X509Certificate("x509.crt", "*******");
            options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            options.ApplicationProtocols = new System.Collections.Generic.List<System.Net.Security.SslApplicationProtocol> { System.Net.Security.SslApplicationProtocol.Http2 };
            QuicListener quicListener = new(System.Net.IPEndPoint.Parse("192.168.15.135:666"), options);

            //quicListener.Start();

            QuicConnection quicConnection = await quicListener.AcceptConnectionAsync();

            QuicStream quicStream = await quicConnection.AcceptStreamAsync();
            await quicStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("67890"));

            byte[] buffer;
            int count = await quicStream.ReadAsync(buffer = new byte[1000]);
            string str = System.Text.Encoding.UTF8.GetString(buffer, 0, count);

            Console.WriteLine(str);
            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}
