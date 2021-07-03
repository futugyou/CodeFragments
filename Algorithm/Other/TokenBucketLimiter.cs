using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Other
{
    public class TokenBucketLimiter
    {
        private long capacity;
        private long windowTimeSecond;
        long lastRefillTimeStamp;
        long refillCountPerSecond;
        long availableTokens;

        public TokenBucketLimiter(long capacity, long windowTimeSecond)
        {
            this.capacity = capacity;
            this.windowTimeSecond = windowTimeSecond;
            lastRefillTimeStamp = DateTime.Now.Ticks;
            refillCountPerSecond = capacity / windowTimeSecond;
            availableTokens = 0;
        }

        public long GetAvailableTokens()
        {
            return availableTokens;
        }

        public bool TryAcquire()
        {
            refill();
            if (availableTokens > 0)
            {
                --availableTokens;
                return true;
            }
            return false;
        }

        private void refill()
        {
            var now = DateTime.Now.Ticks;
            if (now > lastRefillTimeStamp)
            {
                long elapsedTime = now - lastRefillTimeStamp;
                int tokensToBeAdded = (int)((elapsedTime / 1000) * refillCountPerSecond);
                if (tokensToBeAdded > 0)
                {
                    availableTokens = Math.Min(capacity, availableTokens + tokensToBeAdded);
                    lastRefillTimeStamp = now;
                }
            }
        }
        static async Task Test()
        {
            TokenBucketLimiter limiter = new TokenBucketLimiter(1000, 1000);
            Task[] tasks = new Task[100];
            for (int i = 0; i < 100; i++)
            {
                var a = i;
                tasks[i] = Task.Run(() => { Console.WriteLine($"{a}  ,{Thread.CurrentThread.ManagedThreadId}   ,{limiter.TryAcquire()}  ,{limiter.GetAvailableTokens()}"); });
            }
            await Task.WhenAll(tasks);
            Console.WriteLine("Hello World!");
        }
    }
}
