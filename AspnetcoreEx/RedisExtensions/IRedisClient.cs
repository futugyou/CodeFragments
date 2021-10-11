using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace AspnetcoreEx.RedisExtensions;

public interface IRedisClient
{
    Task<bool> Lock(string key, string value, int expireMilliSeconds);
    Task<bool> UnLock(string key, string value);
    Task<long> Publish(string key, string value);
    Task Subscribe(string key, Func<string, Task> handle);
}

public class RedisClient : IRedisClient, IDisposable
{
    private readonly ILogger<RedisClient> _logger;
    private readonly RedisConnection _redisConnection;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly List<IServer> _servers = new List<IServer>();
    private readonly ISubscriber _sub;
    private readonly RedisProfiler _redisProfiler;

    private const string LOCKSTRING = @"local isnx = redis.call('SETNX', @key, @value)
                                        if isnx == 1 then
                                            redis.call('PEXPIRE',@key,@time)
                                            return 1
                                        end
                                        return 0";

    private const string UNLOCKSTRING = @"local getlock = redis.call('GET', @key)
                                            if getlock == @value then
                                                redis.call('DEL', @key)
                                                return 1
                                            end
                                            return 0";

    public RedisClient(
        ILogger<RedisClient> logger,
        IOptionsMonitor<RedisConnection> optionsMonitor,
        RedisProfiler redisProfiler)
    {
        _logger = logger;
        _redisConnection = optionsMonitor.CurrentValue;
        _redis = ConnectionMultiplexer.Connect(_redisConnection.Host);
        foreach (var host in _redisConnection.Host.Split(','))
        {
            _servers.Add(_redis.GetServer(host));
        }
        _db = _redis.GetDatabase(_redisConnection.DatabaseNumber);
        _sub = _redis.GetSubscriber();
        _redisProfiler = redisProfiler;
        _redisProfiler.CreateSessionForCurrentRequest();
        _redis.RegisterProfiler(() => _redisProfiler.GetSession());
    }

    public async Task<bool> Lock(string key, string value, int expireMilliSeconds)
    {
        var _server = _servers.FirstOrDefault();
        RedisResult redisResult;
        if (_server == null)
        {
            var prepared = LuaScript.Prepare(LOCKSTRING);
            redisResult = await _db.ScriptEvaluateAsync(prepared, new { key = (RedisKey)key, value = value, time = expireMilliSeconds });
        }
        else
        {
            var prepared = LuaScript.Prepare(LOCKSTRING);
            var loaded = prepared.Load(_server);
            redisResult = await loaded.EvaluateAsync(_db, new { key = (RedisKey)key, value = value, time = expireMilliSeconds });
        }
        return "1".Equals(redisResult?.ToString());
    }

    public async Task<bool> UnLock(string key, string value)
    {
        var _server = _servers.FirstOrDefault();
        RedisResult redisResult;
        if (_server == null)
        {
            var prepared = LuaScript.Prepare(UNLOCKSTRING);
            redisResult = await _db.ScriptEvaluateAsync(prepared, new { key = (RedisKey)key, value = value });
        }
        else
        {
            var prepared = LuaScript.Prepare(UNLOCKSTRING);
            var loaded = prepared.Load(_server);
            redisResult = await loaded.EvaluateAsync(_db, new { key = (RedisKey)key, value = value });
        }
        return "1".Equals(redisResult?.ToString());
    }

    public async Task<long> Publish(string key, string value)
    {
        return await _sub.PublishAsync(key, value);
    }

    public async Task Subscribe(string key, Func<string, Task> handle)
    {
        var messageQueue = await _sub.SubscribeAsync(key);
        messageQueue.OnMessage(async channelMessage =>
        {
            var message = (string)channelMessage.Message;
            await handle(message);
        });
    }

    public void Dispose()
    {
        var session = _redisProfiler.GetSession();
        if (session != null)
        {
            var timings = session.FinishProfiling();
            _logger.LogInformation(string.Join(",", timings.ToList().Select(p => p.Command)));
            // do what you will with `timings` here
        }
    }
}