using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeFragments
{
    class RedisDemo
    {
        /// <summary>
        /// Acquires the lock.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token">随机值</param>
        /// <param name="expireSecond"></param>
        /// <param name="waitLockSeconds">非阻塞锁</param>
        static bool Lock(string key, string token, int expireSecond = 10, double waitLockSeconds = 0)
        {
            var waitIntervalMs = 50;
            bool isLock;

            DateTime begin = DateTime.Now;
            do
            {
                isLock = Connection.GetDatabase().StringSet(key, token, TimeSpan.FromSeconds(expireSecond), When.NotExists);
                if (isLock)
                    return true;
                //不等待锁则返回
                if (waitLockSeconds == 0) break;
                //超过等待时间，则不再等待
                if ((DateTime.Now - begin).TotalSeconds >= waitLockSeconds) break;
                Thread.Sleep(waitIntervalMs);
            } while (!isLock);
            return false;
        }

        /// <summary>  
        /// Releases the lock.  
        /// </summary>  
        /// <returns><c>true</c>, if lock was released, <c>false</c> otherwise.</returns>  
        /// <param name="key">Key.</param>  
        /// <param name="value">value</param>  
        static bool UnLock(string key, string value)
        {
            string lua_script = @"
                if (redis.call('GET', KEYS[1]) == ARGV[1]) then  
                    redis.call('DEL', KEYS[1])  
                    return true  
                else  
                    return false  
                end  
                ";
            try
            {
                var res = Connection.GetDatabase().ScriptEvaluate(
                    lua_script,
                    new RedisKey[] { key },
                    new RedisValue[] { value });
                return (bool)res;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ReleaseLock lock fail...{ex.Message}");
                return false;
            }
        }

        static async Task HashOperte()
        {
            // 模糊查询获取Redis的keys：
            var redisResult01 = await Connection.GetDatabase().ScriptEvaluateAsync(
               StackExchange.Redis.LuaScript.Prepare($"local ks = redis.call('KEYS', @keypattern) return ks "),
               new { @keypattern = "liveRpc:memberLiveList*" });

            // 查询全部
            var redisResult02 = await Connection.GetDatabase().ScriptEvaluateAsync(
               StackExchange.Redis.LuaScript.Prepare("local ks = redis.call('KEYS', @keypattern) local rst ={ }; for i, v in pairs(ks) do rst[i] = redis.call('hgetall', v) end; return rst "),
               new { @keypattern = "liveRpc:memberLiveList*" });


            // 指定key
            var keys = new List<StackExchange.Redis.RedisKey>();
            for (int i = 0; i < 2000; i++)
            {
                keys.Add($"liveRpc:memberLiveList:{i}");
            }
            var sum = 0L;
            for (int i = 0; i < 1; i++)
            {
                var ts1 = DateTime.Now.Ticks;

                // 指定key
                var redisResult03 = await Connection.GetDatabase().ScriptEvaluateAsync("local rst ={ }; for i, v in pairs(KEYS) do rst[i] = redis.call('hgetall', v) end; return rst", keys.ToArray());

                // 指定key和field
                var redisResult04 = await Connection.GetDatabase().ScriptEvaluateAsync("local rst ={ }; for i, v in pairs(KEYS) do rst[i] = redis.call('hmget', v, 'Id', 'GiftTotalMoney') end; return rst", keys.ToArray());
                var ts2 = DateTime.Now.Ticks;

                var tt = ts2 - ts1;

                sum = Interlocked.Add(ref sum, tt);
            }
        }

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            ConfigurationOptions configuration = new ConfigurationOptions
            {
                AbortOnConnectFail = false,
                ConnectTimeout = 5000,
            };
            configuration.EndPoints.Add("192.168.15.135", 6379);
            return ConnectionMultiplexer.Connect(configuration.ToString());
        });

        public static ConnectionMultiplexer Connection => lazyConnection.Value;

        public static void Exection()
        {
            Parallel.For(0, 3, x =>
            {
                string token = $"loki:{x}";
                bool isLocked = Lock("loki", token, 5, 10);

                if (isLocked)
                {
                    Console.WriteLine($"{token} begin reduce stocks (with lock) at {DateTime.Now}.");
                    Thread.Sleep(1000);
                    Console.WriteLine($"{token} release lock {UnLock("loki", token)} at {DateTime.Now}. ");
                }
                else
                {
                    Console.WriteLine($"{token} don't get lock at {DateTime.Now}.");
                }
            });
        }
    }
}
