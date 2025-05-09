using System.Buffers;

namespace Messaging;

public sealed class TeeStream : Stream
{
    private readonly Stream _source;
    private readonly MemoryStream _replayStream;
    private readonly ArrayPool<byte> _bufferPool;
    private byte[] _buffer;

    public TeeStream(Stream source, int bufferSize = 4096)
    {
        _source = source;
        _replayStream = new MemoryStream();
        _bufferPool = ArrayPool<byte>.Shared;
        _buffer = _bufferPool.Rent(bufferSize);
    }

    public Stream GetReplayStream()
    {
        _replayStream.Position = 0;
        return _replayStream;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        int read = await _source.ReadAsync(buffer, cancellationToken);
        if (read > 0)
        {
            _replayStream.Write(buffer.Span.Slice(0, read));
        }
        return read;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int read = _source.Read(buffer, offset, count);
        if (read > 0)
        {
            _replayStream.Write(buffer, offset, read);
        }
        return read;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _replayStream?.Dispose();
            _bufferPool.Return(_buffer);
            _buffer = null!;
        }

        base.Dispose(disposing);
    }

    public override bool CanRead => _source.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override void Flush() => _source.Flush();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
