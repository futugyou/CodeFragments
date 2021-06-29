using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActixActor
{
    public interface IActor
    {
        void Started<T>(AsyncContext<T> asyncContext) where T : IActor;
        Running Stopping<T>(AsyncContext<T> asyncContext) where T : IActor => Running.Stop;
        void Stopped<T>(AsyncContext<T> asyncContext) where T : IActor;

        Addr<T> Start<T>() where T : IActor;
    }
    public struct SpawnHandle
    {
        public uint Usize { get; set; }

        public SpawnHandle Next()
        {
            var n = new SpawnHandle
            {
                Usize = Usize + 1
            };
            return n;
        }

        public uint IntoUsize() => Usize;
    }
}
