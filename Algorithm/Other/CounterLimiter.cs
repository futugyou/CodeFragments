using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Other
{
    /// <summary>
    /// 计数器法是限流算法里最简单也是最容易实现的一种算法。
    /// 假设一个接口限制一分钟内的访问次数不能超过100个，维护一个计数器，每次有新的请求过来，计数器加一，
    /// 这时候判断，如果计数器的值小于限流值，并且与上一次请求的时间间隔还在一分钟内，
    /// 允许请求通过，否则拒绝请求，如果超出了时间间隔，要将计数器清零。
    /// </summary>
    public class CounterLimiter
    {
        //初始时间
        private static long startTime = DateTime.Now.Ticks;
        //初始计数值
        private static int ZERO = 0;
        //时间窗口限制
        private static long interval = 10000;
        //限制通过请求
        private static int limit = 100;
        //请求计数
        private int requestCount = 0;

        //获取限流
        public (bool, int) TryAcquire()
        {
            long now = DateTime.Now.Ticks;
            if (now < startTime + interval)
            {
                if (requestCount < limit)
                {
                    Interlocked.Increment(ref requestCount);
                    return (true, requestCount);
                }
                return (false, requestCount);
            }
            else
            {
                startTime = now;
                requestCount = ZERO;
                return (true, requestCount);
            }
        }
        static async Task Test()
        {
            Console.WriteLine("Hello World!");
            CounterLimiter limiter = new CounterLimiter();
            Task[] tasks = new Task[1000];
            for (int i = 0; i < 1000; i++)
            {
                var a = i;
                tasks[i] = Task.Run(() => { Console.WriteLine($"{a}  ,{Thread.CurrentThread.ManagedThreadId}   ,{limiter.TryAcquire()}"); });
            }
            await Task.WhenAll(tasks);
            Console.ReadLine();
        }
    }
}
