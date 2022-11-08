using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

// Quic Server
Console.WriteLine("Quic Server Running...");

// 下面创建了一个 QuicListener，用来监听入站的 Quic 连接，一个 QuicListener 可以接收多个 Quic 连接。
// 监听了本地端口 9999，指定了 ALPN 协议版本。
var listener = await QuicListener.ListenAsync(new QuicListenerOptions
{
    ApplicationProtocols = new List<SslApplicationProtocol> { SslApplicationProtocol.Http3 },
    ListenEndPoint = new IPEndPoint(IPAddress.Loopback, 9999),
    ConnectionOptionsCallback = (connection, ssl, token) => ValueTask.FromResult(new QuicServerConnectionOptions()
    {
        DefaultStreamErrorCode = 0,
        DefaultCloseErrorCode = 0,
        ServerAuthenticationOptions = new SslServerAuthenticationOptions()
        {
            ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http3 },
            ServerCertificate = GenerateManualCertificate()
        }
    })
});

// 阻塞线程，直到接收到一个 Quic 连接，一个 QuicListener 可以接收多个 连接。
var connection = await listener.AcceptConnectionAsync();
Console.WriteLine($"Client [{connection.RemoteEndPoint}]: connected");

var cts = new CancellationTokenSource();
while (!cts.IsCancellationRequested)
{
    // 支持接收多个 Quic 流
    // 接收一个入站的 Quic 流, 一个 QuicConnection 可以支持多个流。 可单向，也可双向
    var stream = await connection.AcceptInboundStreamAsync();
    Console.WriteLine($"Stream [{stream.Id}]: created");

    // 使用 System.IO.Pipeline 处理流数据，读取行数据，并回复一个 ack 消息。
    Console.WriteLine();
    _ = ProcessLinesAsync(stream);
}

Console.ReadKey();


// 处理流数据
async Task ProcessLinesAsync(QuicStream stream)
{
    var reader = PipeReader.Create(stream);
    var writer = PipeWriter.Create(stream);

    while (true)
    {
        ReadResult result = await reader.ReadAsync();
        ReadOnlySequence<byte> buffer = result.Buffer;

        while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
        {
            // 读取行数据
            ProcessLine(line);

            // 写入 ACK 消息
            await writer.WriteAsync(Encoding.UTF8.GetBytes($"Ack: {DateTime.Now.ToString("HH:mm:ss")} \n"));
        }

        reader.AdvanceTo(buffer.Start, buffer.End);

        if (result.IsCompleted)
        {
            break;
        }
    }

    Console.WriteLine($"Stream [{stream.Id}]: completed");

    await reader.CompleteAsync();
    await writer.CompleteAsync();
}

bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
{
    SequencePosition? position = buffer.PositionOf((byte)'\n');

    if (position == null)
    {
        line = default;
        return false;
    }

    line = buffer.Slice(0, position.Value);
    buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
    return true;
}

void ProcessLine(in ReadOnlySequence<byte> buffer)
{
    foreach (var segment in buffer)
    {
        Console.WriteLine("Recevied -> " + System.Text.Encoding.UTF8.GetString(segment.Span));
    }

    Console.WriteLine();
}

// 因为 Quic 需要 TLS 加密，所以要指定一个证书，GenerateManualCertificate 方法可以方便地创建一个本地的测试证书。
X509Certificate2 GenerateManualCertificate()
{
    X509Certificate2 cert = null;
    var store = new X509Store("KestrelWebTransportCertificates", StoreLocation.CurrentUser);
    store.Open(OpenFlags.ReadWrite);
    if (store.Certificates.Count > 0)
    {
        cert = store.Certificates[^1];

        // rotate key after it expires
        if (DateTime.Parse(cert.GetExpirationDateString(), null) < DateTimeOffset.UtcNow)
        {
            cert = null;
        }
    }
    if (cert == null)
    {
        // generate a new cert
        var now = DateTimeOffset.UtcNow;
        SubjectAlternativeNameBuilder sanBuilder = new();
        sanBuilder.AddDnsName("localhost");
        using var ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        CertificateRequest req = new("CN=localhost", ec, HashAlgorithmName.SHA256);
        // Adds purpose
        req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection
        {
            new("1.3.6.1.5.5.7.3.1") // serverAuth

        }, false));
        // Adds usage
        req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
        // Adds subject alternate names
        req.CertificateExtensions.Add(sanBuilder.Build());
        // Sign
        using var crt = req.CreateSelfSigned(now, now.AddDays(14)); // 14 days is the max duration of a certificate for this
        cert = new(crt.Export(X509ContentType.Pfx));

        // Save
        store.Add(cert);
    }
    store.Close();

    var hash = SHA256.HashData(cert.RawData);
    var certStr = Convert.ToBase64String(hash);
    Console.WriteLine($"\n\n\n\n\nCertificate: {certStr}\n\n\n\n"); // <-- you will need to put this output into the JS API call to allow the connection
    return cert;
}