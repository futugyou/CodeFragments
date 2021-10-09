using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ResponseBodyFeature.RedisExtensions;

public interface IRedisClient
{
    Task<bool> Lock(string key, string value, int expireMilliSeconds);
    Task<bool> UnLock(string key, string value);
}

public class RedisClient : IRedisClient
{
    private readonly RedisConnection _redisConnection;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly IServer _server;

    private const string LOCKSTRING = @"local isnx = redis.call('SETNX',@key,@value)
                                        if isnx ==1 then
                                            redis.call('PEXPIRE',@key,@time)
                                            return 1
                                        end
                                        return 0";

    private const string UNLOCKSTRING = @"local getlock = redis.call('GET',@key)
                                            if getlock == @value then
                                                redis.call('DEL',@key)
                                                return 1
                                            end
                                            return 0";

    public RedisClient(IOptionsMonitor<RedisConnection> optionsMonitor)
    {
        _redisConnection = optionsMonitor.CurrentValue;
        _redis = ConnectionMultiplexer.Connect(_redisConnection.Host);
        _server = _redis.GetServer(_redisConnection.Host);
        _db = _redis.GetDatabase(_redisConnection.DatabaseNumber);
    }

    public async Task<bool> Lock(string key, string value, int expireMilliSeconds)
    {
        var prepared = LuaScript.Prepare(LOCKSTRING);
        var loaded = prepared.Load(_server);
        var result = await loaded.EvaluateAsync(_db, new { key = (RedisKey)key, value = value, time = expireMilliSeconds });
        return "1".Equals(result?.ToString());
    }

    public async Task<bool> UnLock(string key, string value)
    {
        var prepared = LuaScript.Prepare(UNLOCKSTRING);
        var loaded = prepared.Load(_server);
        var result = await loaded.EvaluateAsync(_db, new { key = (RedisKey)key, value = value });
        return "1".Equals(result?.ToString());
    }
}