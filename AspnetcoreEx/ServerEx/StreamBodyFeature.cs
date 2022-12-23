using System.IO;
using System.IO.Pipelines;
using Microsoft.AspNetCore.Http.Features;

public class StreamBodyFeature : IHttpResponseBodyFeature
{
    public Stream Stream { get; }
    public PipeWriter Writer { get; }

    public StreamBodyFeature(Stream stream)
    {
        Stream = stream;
        Writer = PipeWriter.Create(Stream);
    }

    public Task CompleteAsync()
    {
        return Task.CompletedTask;
    }

    public Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void DisableBuffering()
    {
        
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}