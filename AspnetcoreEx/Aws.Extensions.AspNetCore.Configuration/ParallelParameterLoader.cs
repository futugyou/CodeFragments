
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace Aws.Extensions.AspNetCore.Configuration;

class ParallelParameterLoader : IDisposable
{
    private readonly int ParallelismLevel = Environment.ProcessorCount;
    private readonly IAmazonSimpleSystemsManagement _awsmanager;
    private readonly SemaphoreSlim _semaphore;
    private readonly List<Task<GetParametersResponse>> _tasks;

    public ParallelParameterLoader(IAmazonSimpleSystemsManagement awsmanager)
    {
        _awsmanager = awsmanager;
        _semaphore = new SemaphoreSlim(ParallelismLevel, ParallelismLevel);
        _tasks = [];
    }

    public void Add(string[] subNames)
    {
        _tasks.Add(GetParameters(subNames));
    }

    private async Task<GetParametersResponse> GetParameters(string[] subNames)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            GetParametersRequest request = new()
            {
                Names = [.. subNames],
                WithDecryption = true,
            };
            return await _awsmanager.GetParametersAsync(request).ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task<GetParametersResponse[]> WaitForAll()
    {
        return Task.WhenAll(_tasks);
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}
