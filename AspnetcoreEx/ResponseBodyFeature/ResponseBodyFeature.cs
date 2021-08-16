using Microsoft.AspNetCore.Http.Features;
using System.IO.Pipelines;

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
        var json = System.Text.Encoding.UTF8.GetString(buffer).TrimEnd('\0');
        json = "{\"Status\":0, \"Info\":" + json + " }";
        buffer = System.Text.Encoding.UTF8.GetBytes(json);
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

        var customBody = new ResponseCustomBody(context, originalBodyFeature);
        context.Features.Set<IHttpResponseBodyFeature>(customBody);

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