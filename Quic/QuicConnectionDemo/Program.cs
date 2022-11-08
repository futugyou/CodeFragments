
using System.Net.Quic;
using System.Net.Security;
using System.Net;
using System.Reflection.PortableExecutable;
using System.IO.Pipelines;
using System.Text;
using System.Buffers;

// this is quic client
Console.WriteLine("Quic Client Running...");

await Task.Delay(3000);

//  使用 QuicConnection.ConnectAsync 连接到服务端。
var connection = await QuicConnection.ConnectAsync(new QuicClientConnectionOptions
{
    DefaultCloseErrorCode = 0,
    DefaultStreamErrorCode = 0,
    RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 9999),
    ClientAuthenticationOptions = new SslClientAuthenticationOptions
    {
        ApplicationProtocols = new List<SslApplicationProtocol> { SslApplicationProtocol.Http3 },
        RemoteCertificateValidationCallback = (sender, certificate, chain, errors) =>
        {
            return true;
        }
    }
});

// 打开一个出站的双向流
var stream = await connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional);

var reader = PipeReader.Create(stream);
var writer = PipeWriter.Create(stream);

// 后台读取流数据，然后循环写入数据
_ = ProcessLinesAsync(stream);

Console.WriteLine();

// 写入数据
for (int i = 0; i < 7; i++)
{
    await Task.Delay(2000);

    var message = $"Hello Quic {i} \n";

    Console.Write("Send -> " + message);

    await writer.WriteAsync(Encoding.UTF8.GetBytes(message));
}

await writer.CompleteAsync();

Console.ReadKey();

// 使用 System.IO.Pipeline 读取流数据
async Task ProcessLinesAsync(QuicStream stream)
{
    while (true)
    {
        ReadResult result = await reader.ReadAsync();
        ReadOnlySequence<byte> buffer = result.Buffer;

        while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
        {
            // 处理行数据
            ProcessLine(line);
        }

        reader.AdvanceTo(buffer.Start, buffer.End);

        if (result.IsCompleted)
        {
            break;
        }
    }

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
        Console.Write("Recevied -> " + System.Text.Encoding.UTF8.GetString(segment.Span));
        Console.WriteLine();
    }

    Console.WriteLine();
}