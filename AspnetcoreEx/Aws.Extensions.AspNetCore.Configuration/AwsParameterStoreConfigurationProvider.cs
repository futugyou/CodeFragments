
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace Aws.Extensions.AspNetCore.Configuration;

public class AwsParameterStoreConfigurationProvider(IAmazonSimpleSystemsManagement manager, AwsClientConfig config) : ConfigurationProvider, IDisposable
{
    private readonly CancellationTokenSource _cancellationToken = new();
    private bool _disposed;

    public override void Load() => LoadAsync().GetAwaiter().GetResult();
    private async Task LoadAsync()
    {
        string? nextToken = null;
        var response = await manager.GetParametersByPathAsync(new GetParametersByPathRequest { Path = "/", Recursive = true, WithDecryption = true, NextToken = nextToken }).ConfigureAwait(false);
        foreach (var item in response.Parameters)
        {
            Console.WriteLine(item.Value);
        }
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