using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace CodeFragments;
public static class PipelinesTest
{
    public static async Task Show()
    {
        Pipe pipe = new Pipe();
        // write something
        await WriteSomeDataAsync(pipe.Writer);
        // signal that there won't be anything else written
        pipe.Writer.Complete();
        // consume it
        await ReadSomeDataAsync(pipe.Reader);
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