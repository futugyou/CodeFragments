using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Other
{
    public class LeakyLimiter
    {
        //桶的容量
        private int capacity;
        //漏水速度
        private int ratePerMillSecond;
        //水量
        private double water;
        //上次漏水时间
        private long lastLeakTime;

        public LeakyLimiter(int capacity, int ratePerMillSecond)
        {
            this.capacity = capacity;
            this.ratePerMillSecond = ratePerMillSecond;
            this.water = 0;
        }

        public bool TryAcquire()
        {
            //执行漏水，更新剩余水量
            refresh();
            //尝试加水，水满则拒绝
            if (water + 1 > capacity)
            {
                return false;
            }
            water++;
            return true;
        }

        private void refresh()
        {
            //当前时间
            long now = DateTime.Now.Ticks;
            if (now > lastLeakTime)
            {
                //距上次漏水的时间间隔
                long millisSinceLastLeak = now - lastLeakTime;
                long leaks = millisSinceLastLeak * ratePerMillSecond;
                //允许漏水
                if (leaks > 0)
                {
                    if (water <= leaks)
                    {
                        water = 0;
                    }
                    else
                    {
                        water -= leaks;
                    }
                    this.lastLeakTime = now;
                }
            }
        }
        static async Task Test()
        {
            LeakyLimiter limiter = new LeakyLimiter(100, 10000);
            Task[] tasks = new Task[1000];
            for (int i = 0; i < 1000; i++)
            {
                var a = i;
                tasks[i] = Task.Run(() => { Console.WriteLine($"{a}  ,{Thread.CurrentThread.ManagedThreadId}   ,{limiter.TryAcquire()}"); });
            }
            await Task.WhenAll(tasks);
            Console.WriteLine("Hello World!");
        }
    }
}
