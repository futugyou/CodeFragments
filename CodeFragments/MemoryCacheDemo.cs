using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace CodeFragments;

public class MemoryCacheDemo
{
    public static async Task MemeoryCacheWithChangeTokenUsecase()
    {
        var fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
        var fileName = "time.txt";
        var @lock = new object();
        var cache = new ServiceCollection()
            .AddMemoryCache()
            .BuildServiceProvider()
            .GetRequiredService<IMemoryCache>();
        var options = new MemoryCacheEntryOptions();
        options.AddExpirationToken(fileProvider.Watch(fileName));
        options.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration { EvictionCallback = OnEvicted });

        Write(DateTime.Now.ToString());

        cache.Set("CurrentTime", Read(), options);

        while (true)
        {
            Write(DateTime.Now.ToString());
            await Task.Delay(1000);
            if (cache.TryGetValue("CurrentTime", out string? currentTime))
            {
                Console.WriteLine(currentTime);
            }
        }

        string Read()
        {
            lock (@lock)
            {
                return File.ReadAllText(fileName);
            }
        }

        void Write(string content)
        {
            lock (@lock)
            {
                File.WriteAllText(fileName, content);
            }
        }

        void OnEvicted(object key, object? value, EvictionReason reason, object? state)
        {
            options.ExpirationTokens.Clear();
            options.AddExpirationToken(fileProvider.Watch(fileName));
            cache.Set("CurrentTime", Read(), options);
        }
    }

    public static async Task MemeoryCacheWithCompactionUsecase()
    {
        var cache = new ServiceCollection()
            .AddMemoryCache(options => {
                options.SizeLimit = 10;
                options.CompactionPercentage = 0.2;
            })
            .BuildServiceProvider()
            .GetRequiredService<IMemoryCache>();
        for (int i = 0; i <= 5; i++)
        {
            cache.Set(i, i.ToString(), new MemoryCacheEntryOptions{
                Priority = CacheItemPriority.Low,
                Size = 1
            });
        }
        for (int i = 6; i <= 10; i++)
        {
            cache.Set(i, i.ToString(), new MemoryCacheEntryOptions{
                Priority = CacheItemPriority.Normal,
                Size = 1
            });
        }
        cache.Set(11, "11", new MemoryCacheEntryOptions{
            Priority = CacheItemPriority.Normal,
            Size = 1
        });

        await Task.Delay(1000);

        Console.WriteLine("Key\tValue");
        for (int i = 0; i <= 11; i++)
        {
            Console.WriteLine($"{i}\t{cache.Get<string>(i) ?? "N/A"}");
        }
    }
}