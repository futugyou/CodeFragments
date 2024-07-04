using StackExchange.Redis;

namespace AspnetcoreEx.RedisExtensions;

public interface IRedisClient
{
    Task<bool> Lock(string key, string value, int expireMilliSeconds);
    Task<bool> UnLock(string key, string value);
    Task<long> Publish(string key, string value);
    Task Subscribe(string key, Func<string, Task> handle);
    Task<string> WriteStream(string streamKey, string fieldName, string value);
    Task<string> WriteStream(string streamKey, Dictionary<string, string> streamPairs);
    Task<Dictionary<string, Dictionary<string, string>>> ReadStream(string streamKey, string position, int maxCount = 0);
    Task<(bool, string)> TokenBucket(string key);
}

public class RedisClient : IRedisClient, IDisposable
{
    private readonly ILogger<RedisClient> _logger;
    private readonly RedisOptions _redisOptions;
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

    private const string TOKENBUCKET = @"
                                        local prefix = ARGV[1]
                                        local cursor = '0'
                                        local found = false
                                        local selectedKey = nil

                                        repeat
                                            local result = redis.call('SCAN', cursor, 'MATCH', prefix .. '*', 'COUNT', '1000')
                                            cursor = result[1]
                                            local keys = result[2]

                                            for _, key in ipairs(keys) do
                                                local value = redis.call('GET', key)
                                                if value == '1' then
                                                    selectedKey = key
                                                    found = true
                                                    break
                                                end
                                            end

                                        until cursor == '0' or found

                                        if found and selectedKey then
                                            redis.call('SET', selectedKey, '0')
                                        end

                                        return {found, selectedKey}
                                    ";

    private const string UNLOCKSTRING = @"local getlock = redis.call('GET', @key)
                                            if getlock == @value then
                                                redis.call('DEL', @key)
                                                return 1
                                            end
                                            return 0";

    public RedisClient(
        ILogger<RedisClient> logger,
        IOptionsMonitor<RedisOptions> optionsMonitor,
        RedisProfiler redisProfiler)
    {
        _logger = logger;
        _redisOptions = optionsMonitor.CurrentValue;
        _redis = ConnectionMultiplexer.Connect(_redisOptions.Host);
        foreach (var host in _redisOptions.Host.Split(','))
        {
            _servers.Add(_redis.GetServer(host));
        }
        _db = _redis.GetDatabase(_redisOptions.DatabaseNumber);
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
        return await _sub.PublishAsync(RedisChannel.Literal(key), value);
    }

    public async Task Subscribe(string key, Func<string, Task> handle)
    {
        var messageQueue = await _sub.SubscribeAsync(RedisChannel.Literal(key));
        messageQueue.OnMessage(async channelMessage =>
        {
            if (channelMessage.Message.HasValue)
            {
                await handle(channelMessage.Message.ToString());
            }
        });
    }

    public void Dispose()
    {
        var session = _redisProfiler.GetSession();
        if (session != null)
        {
            var timings = session.FinishProfiling();
            string? message = string.Join(",", timings.ToList().Select(p => p.Command));
            _logger.LogInformation(message);
            // do what you will with `timings` here
        }
    }

    public async Task<string> WriteStream(string streamKey, string fieldName, string value)
    {
        string? messageId = await _db.StreamAddAsync(streamKey, fieldName, value);
        return messageId ?? "";
    }

    public async Task<string> WriteStream(string streamKey, Dictionary<string, string> streamPairs)
    {
        var values = new NameValueEntry[streamPairs.Count];
        foreach (var item in streamPairs)
        {
            values.Append(new NameValueEntry(item.Key, item.Value));
        }
        string? messageId = await _db.StreamAddAsync(streamKey, values);
        return messageId ?? "";
    }

    public async Task<Dictionary<string, Dictionary<string, string>>> ReadStream(string streamKey, string position, int maxCount = 0)
    {
        var result = new Dictionary<string, Dictionary<string, string>>();
        int? count = maxCount == 0 ? null : maxCount;
        var messages = await _db.StreamReadAsync(streamKey, position, count);
        foreach (var message in messages)
        {
            string? id = message.Id;
            if (string.IsNullOrEmpty(id))
            {
                continue;
            }
            var d = new Dictionary<string, string>();
            message.Values.ToList().ForEach(p =>
            {
                string? name = p.Name;
                string? value = p.Value;
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                {
                    d.Add(name, value);
                }
            });

            result.Add(id, d);
        }
        return result;
    }

    public async Task<(bool, string)> TokenBucket(string key)
    {
        var scriptParams = new RedisKey[] { key };
        var result = await _db.ScriptEvaluateAsync(TOKENBUCKET, scriptParams);

        // Process Lua script result
        var resultArray = (RedisResult[])result!;

        bool found = (bool)resultArray[0];
        string selectedKey = (string)resultArray[1]!;

        return (found, selectedKey);
    }
}