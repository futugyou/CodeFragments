
namespace Microsoft.Extensions.Configuration;

public class AwsParameterStoreConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly AwsParameterStoreManager _manager;
    private readonly int _reloadInterval;
    private readonly CancellationTokenSource _cancellationToken = new();
    private readonly IAmazonSimpleSystemsManagement _awsmanager;
    private readonly AwsClientConfig _config;
    private Dictionary<string, Parameter>? _loadedParameters;
    private bool _disposed;
    private Task? _pollingTask;

    public AwsParameterStoreConfigurationProvider(IAmazonSimpleSystemsManagement awsmanager, AwsClientConfig config)
    {
        _reloadInterval = config.ReloadInterval;
        _manager = new();
        _awsmanager = awsmanager;
        _config = config;
        _pollingTask = null;
    }

    public override void Load() => LoadAsync().GetAwaiter().GetResult();

    private async Task PollForParameterChangesAsync()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            await WaitForReload().ConfigureAwait(false);
            try
            {
                await LoadAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Ignore
            }
        }
    }

    private Task WaitForReload()
    {
        // WaitForReload is only called when the _reloadInterval has a value.
        return Task.Delay(_reloadInterval, _cancellationToken.Token);
    }

    private async Task LoadAsync()
    {
        var newLoadedParameters = new Dictionary<string, Parameter>();
        var oldLoadedParameters = Interlocked.Exchange(ref _loadedParameters, null);
        var needReload = false;

        IEnumerable<Parameter>? parameters = null;
        try
        {
            parameters = await _manager.ReadParameters(_awsmanager, _config);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        foreach (var parameter in parameters ?? [])
        {
            newLoadedParameters.Add(parameter.Name, parameter);
            if (!needReload && oldLoadedParameters != null
            && oldLoadedParameters.TryGetValue(parameter.Name, out var existingParameter)
            && IsUpToDate(existingParameter, parameter))
            {
                needReload = true;
            }
        }

        _loadedParameters = newLoadedParameters;

        if (_loadedParameters.Count > 0)
        {
            Data = _manager.GetData(_loadedParameters.Values);
            if (needReload)
            {
                OnReload();
            }
        }

        // schedule a polling task only if none exists and a valid delay is specified
        if (_pollingTask == null && _config.ReloadInterval > 0)
        {
            _pollingTask = PollForParameterChangesAsync();
        }
    }

    private static bool IsUpToDate(Parameter existingParameter, Parameter parameter)
    {
        if (existingParameter.Version != parameter.Version)
        {
            return false;
        }
        return true;
    }
    #region Dispose
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);

    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_disposed)
            {
                _cancellationToken.Cancel();
                _cancellationToken.Dispose();
            }

            _disposed = true;
        }
    }
    #endregion
}