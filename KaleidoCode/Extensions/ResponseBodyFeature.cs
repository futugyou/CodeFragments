using Microsoft.AspNetCore.Http.Features;
using System.IO.Pipelines;
using System.Text.Json.Nodes;

namespace KaleidoCode.Extensions;

public class ReadCacheProxyStream : Stream
{
    private readonly Stream _innerStream;

    public MemoryStream CachedStream { get; } = new MemoryStream(1024);

    public override bool CanRead => _innerStream.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => _innerStream.Length;

    public override long Position { get => _innerStream.Length; set => throw new NotSupportedException(); }

    public ReadCacheProxyStream(Stream innerStream)
    {
        _innerStream = innerStream;
    }

    public override void Flush() => throw new NotSupportedException();

    public override Task FlushAsync(CancellationToken cancellationToken) => _innerStream.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var len = await _innerStream.ReadAsync(buffer, cancellationToken);
        if (len > 0)
        {
            CachedStream.Write(buffer.Span.Slice(0, len));
        }
        return len;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}

public class WriteCacheProxyStream : Stream
{
    private readonly Stream _innerStream;

    public MemoryStream CachedStream { get; } = new MemoryStream(1024);

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => _innerStream.CanWrite;

    public override long Length => _innerStream.Length;

    public override long Position { get => _innerStream.Length; set => throw new NotSupportedException(); }

    public WriteCacheProxyStream(Stream innerStream)
    {
        _innerStream = innerStream;
    }

    public override void Flush() => throw new NotSupportedException();

    public override Task FlushAsync(CancellationToken cancellationToken) => _innerStream.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await _innerStream.WriteAsync(buffer, cancellationToken);
        CachedStream.Write(buffer.Span);
    }
}

public class ResponseCustomBody : Stream, IHttpResponseBodyFeature
{
    private readonly HttpContext _context;
    private readonly IHttpResponseBodyFeature _innerBodyFeature;
    private readonly Stream _innerStream;

    public ResponseCustomBody(HttpContext context,
        IHttpResponseBodyFeature innerBodyFeature)
    {
        _context = context;
        _innerBodyFeature = innerBodyFeature;
        _innerStream = innerBodyFeature.Stream;
    }
    public Stream Stream => this;

    public PipeWriter Writer => throw new NotImplementedException();

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => _innerStream.CanWrite;

    public override long Length => throw new NotImplementedException();

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public async Task CompleteAsync()
    {
        await _innerBodyFeature.CompleteAsync();
    }

    public void DisableBuffering()
    {
        _innerBodyFeature.DisableBuffering();
    }

    public Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken = default)
    {
        return _innerBodyFeature.SendFileAsync(path, offset, count, cancellationToken);
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return _innerBodyFeature.StartAsync(cancellationToken);
    }

    public override void Flush()
    {
        _innerStream.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return _innerStream.FlushAsync(cancellationToken);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        // the buffer length is 16384, it include valid data and many many '\0' and other word like \u0001 
        string json = System.Text.Encoding.UTF8.GetString(buffer, offset, count).TrimEnd('\0');
        //json = $"{{\"Status\":0, \"Info\":{json}}}";
        //json = "{\"Status\":0, \"Info\":" + json + "}";
        if (string.IsNullOrEmpty(json))
        {
            return;
        }
        var jNode = JsonNode.Parse(json);
        if (jNode == null)
        {
            return;
        }
        var jObject = new JsonObject
        {
            ["Status"] = 0,
            ["Info"] = jNode,
        };
        buffer = System.Text.Encoding.UTF8.GetBytes(jObject.ToJsonString());
        count = buffer.Length;
        await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }
}

public class ResponseCustomMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var originalBodyFeature = context.Features.Get<IHttpResponseBodyFeature>();
        if (originalBodyFeature != null)
        {
            var customBody = new ResponseCustomBody(context, originalBodyFeature);
            context.Features.Set<IHttpResponseBodyFeature>(customBody);
        }

        try
        {
            await next(context);
        }
        finally
        {
            context.Features.Set(originalBodyFeature);
        }
    }
}