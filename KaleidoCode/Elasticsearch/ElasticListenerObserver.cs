using System.Diagnostics;
using OpenSearch.Net.Diagnostics;

namespace KaleidoCode.Elasticsearch;

public class ElasticListenerObserver : IObserver<DiagnosticListener>, IDisposable
{
    private long _messagesWrittenToConsole = 0;
    public long MessagesWrittenToConsole => _messagesWrittenToConsole;

    public Exception SeenException { get; private set; }

    public void OnError(Exception error) => SeenException = error;
    public bool Completed { get; private set; }
    public void OnCompleted() => Completed = true;

    private void WriteToConsole<T>(string eventName, T data)
    {
        var a = Activity.Current;
        Interlocked.Increment(ref _messagesWrittenToConsole);
    }

    private List<IDisposable> Disposables { get; } = [];

    public void OnNext(DiagnosticListener value)
    {
        void TrySubscribe(string sourceName, Func<IObserver<KeyValuePair<string, object>>> listener)
        {
            if (value.Name != sourceName) return;

            var subscription = value.Subscribe(listener()!);
            Disposables.Add(subscription);
        }

        TrySubscribe(DiagnosticSources.AuditTrailEvents.SourceName,
            () => new AuditDiagnosticObserver(v => WriteToConsole(v.Key, v.Value)));

        TrySubscribe(DiagnosticSources.Serializer.SourceName,
            () => new SerializerDiagnosticObserver(v => WriteToConsole(v.Key, v.Value)));

        TrySubscribe(DiagnosticSources.RequestPipeline.SourceName,
            () => new RequestPipelineDiagnosticObserver(
                v => WriteToConsole(v.Key, v.Value),
                v => WriteToConsole(v.Key, v.Value)
            ));

        TrySubscribe(DiagnosticSources.HttpConnection.SourceName,
            () => new HttpConnectionDiagnosticObserver(
                v => WriteToConsole(v.Key, v.Value),
                v => WriteToConsole(v.Key, v.Value)
            ));
    }

    public void Dispose()
    {
        foreach (var d in Disposables) d.Dispose();
    }
}