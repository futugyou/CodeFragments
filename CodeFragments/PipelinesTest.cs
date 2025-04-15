using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CodeFragments;
public static class PipelinesTest
{
    public static async Task Base()
    {
        Pipe pipe = new Pipe();
        // write something
        await WriteSomeDataAsync(pipe.Writer);
        // signal that there won't be anything else written
        pipe.Writer.Complete();
        // consume it
        await ReadSomeDataAsync(pipe.Reader);
    }

    public static async Task PipeWithTcpClient()
    {
        var listener = new TcpListener(IPAddress.Any, 8080);
        listener.Start();
        Console.WriteLine("Server started. Listening on port 8080...");

        var client = await listener.AcceptTcpClientAsync();
        Console.WriteLine("Client connected.");

        var pipe = new Pipe();
        var readTask = TcpReadDataAsync(client, pipe.Writer);
        var writeTask = TcpWriteDataAsync(client, pipe.Reader);

        await Task.WhenAll(readTask, writeTask);

        client.Close();
    }

    public static async Task RunClient()
    {
        try
        {
            using var client = new TcpClient();
            Console.WriteLine("Connecting to server...");
            await client.ConnectAsync("127.0.0.1", 8080);
            Console.WriteLine("Connected to server.");

            using var stream = client.GetStream();
            var message = "Hello from client!";
            var data = Encoding.ASCII.GetBytes(message);

            // Send data to the server
            await stream.WriteAsync(data, 0, data.Length);
            Console.WriteLine($"Sent: {message}");

            // Optionally, read response from server
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Received: {response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async ValueTask WriteSomeDataAsync(PipeWriter writer)
    {
        // use an oversized size guess
        Memory<byte> workspace = writer.GetMemory(20);
        // write the data to the workspace
        int bytes = Encoding.ASCII.GetBytes("hello, world!", workspace.Span);
        // tell the pipe how much of the workspace we actually want to commit
        writer.Advance(bytes);
        // this is **not** the same as Stream.Flush!
        await writer.FlushAsync();
    }

    static async Task TcpReadDataAsync(TcpClient client, PipeWriter writer)
    {
        var stream = client.GetStream();
        while (true)
        {
            var memory = writer.GetMemory(1024);
            var bytesRead = await stream.ReadAsync(memory, default);

            if (bytesRead == 0)
            {
                break;
            }

            writer.Advance(bytesRead);
            await writer.FlushAsync();
        }

        writer.Complete();
    }

    static async Task TcpWriteDataAsync(TcpClient client, PipeReader reader)
    {
        var stream = client.GetStream();
        while (true)
        {
            // await some data being available
            ReadResult read = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = read.Buffer;
            // check whether we've reached the end and processed everything
            if (buffer.IsEmpty && read.IsCompleted)
                break; // exit loop 
                       // process what we received
            foreach (var segment in buffer)
            {
                await stream.WriteAsync(segment, default);
            }
            // tell the pipe that we used everything
            reader.AdvanceTo(buffer.End);
        }
    }

    static async ValueTask ReadSomeDataAsync(PipeReader reader)
    {
        while (true)
        {
            // await some data being available
            ReadResult read = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = read.Buffer;
            // check whether we've reached the end and processed everything
            if (buffer.IsEmpty && read.IsCompleted)
                break; // exit loop

            // process what we received
            foreach (var segment in buffer)
            {
                string s = Encoding.ASCII.GetString(segment.Span);
                Console.WriteLine(s);
            }
            // tell the pipe that we used everything
            reader.AdvanceTo(buffer.End);
        }
    }

    public static async Task PipeScheduler()
    {
        var writeScheduler = new SingleThreadPipeScheduler();
        var readScheduler = new SingleThreadPipeScheduler();

        // Tell the Pipe what schedulers to use and disable the SynchronizationContext.
        var options = new PipeOptions(readerScheduler: readScheduler,
                                      writerScheduler: writeScheduler,
                                      useSynchronizationContext: false);
        var pipe = new Pipe(options);
        await WriteSomeDataAsync(pipe.Writer);
        // signal that there won't be anything else written
        pipe.Writer.Complete();
        // consume it
        await ReadSomeDataAsync(pipe.Reader);
    }
}

public class SingleThreadPipeScheduler : PipeScheduler
{
    private readonly BlockingCollection<(Action<object> Action, object State)> _queue = [];
    private readonly Thread _thread;

    public SingleThreadPipeScheduler()
    {
        _thread = new Thread(DoWork);
        _thread.Start();
    }

    private void DoWork()
    {
        foreach (var item in _queue.GetConsumingEnumerable())
        {
            item.Action(item.State);
        }
    }

    public override void Schedule(Action<object?> action, object? state)
    {
        if (state is not null)
        {
            _queue.Add((action, state));
        }
        // else log the fact that _queue.Add was not called.
    }
}