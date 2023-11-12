
using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement;

namespace Aws.Extensions.AspNetCore.Configuration;

public class AwsParameterStoreConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly AWSOptions _options;
    private readonly IAmazonSimpleSystemsManagement _manager;
    private readonly CancellationTokenSource _cancellationToken;
    private bool _disposed;

    // TODO: read ssm
    public AwsParameterStoreConfigurationProvider(IAmazonSimpleSystemsManagement manager, AWSOptions options)
    {
        _options = options;
        _cancellationToken = new CancellationTokenSource();
        _manager = manager;
    }

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
}