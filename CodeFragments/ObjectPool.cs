using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CodeFragments;

public class ObjectPoolUsecase
{
    public static void StringBuilderUsecase()
    {
        var pool = new DefaultObjectPoolProvider();
        var objectPool = pool.CreateStringBuilderPool(1024, 1024 * 1024);
        var builder = objectPool.Get();
        try
        {
            for (int index = 0; index < 100; index++)
            {
                builder.Append(index);
            }
            Console.WriteLine(builder);
        }
        finally
        {
            objectPool.Return(builder);
        }
    }

    public static void ObjectPolicyWithParameterConstructorUsecase()
    {
        var pool = new DefaultObjectPoolProvider();
        var objectPool = pool.Create(new FoobarListPolicy(1024, 1024 * 1024));
        string json;
        var list = objectPool.Get();
        try
        {
            list.AddRange(Enumerable.Range(1, 1000).Select(it => new Foobar(it, it)));
            json = JsonSerializer.Serialize(list);
            Console.WriteLine(json);
        }
        finally
        {
            objectPool.Return(list);
        }
    }

    public static void ObjectPolicyWithObjectLimitUsecase()
    {
        var policy = new DefaultPooledObjectPolicy<Foo>();
        DoTest(policy, 1);
        var policy2 = new FooPooledObjectPolicy();
        DoTest(policy2, 2);

        static void DoTest(IPooledObjectPolicy<Foo> policy, int limit)
        {
            var pool = new DefaultObjectPool<Foo>(policy, limit);
            var item1 = pool.Get();

            pool.Return(item1);
            Console.WriteLine("item 1: {0}", item1.Id);


            var item2 = pool.Get();
            pool.Return(item2);
            Console.WriteLine("item 2: {0}", item2.Id);


            var item3 = pool.Get();

            pool.Return(item1);
            pool.Return(item2);
            pool.Return(item3);
            var item4 = pool.Get();
            var item5 = pool.Get();
            var item6 = pool.Get();
            Console.WriteLine("item 3: {0}", item3.Id);
            Console.WriteLine("item 4: {0}", item4.Id);
            Console.WriteLine("item 5: {0}", item5.Id);
            Console.WriteLine("item 6: {0}", item6.Id);
            pool.Return(item4);
            pool.Return(item5);
            pool.Return(item6);
            Console.WriteLine("-------------------------------");
        }
    }

    public static async Task ObjectPolicyWithIDisposableUsecase()
    {
        var objectPool = new ServiceCollection()
            .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
            .BuildServiceProvider()
            .GetRequiredService<ObjectPoolProvider>()
            .Create(new FooPooledObjectPolicy());
        Console.WriteLine(Environment.ProcessorCount * 2);
        for (int i = 0; i < 1000; i++)
        {
            await Task.WhenAll(Enumerable.Range(1, Environment.ProcessorCount * 2 + 3).Select(_ => ExecuteFooAsync()));
            Console.WriteLine();
        }
        async Task ExecuteFooAsync()
        {
            var foo = objectPool.Get();
            try
            {
                await Task.Delay(1000);
            }
            finally
            {
                objectPool.Return(foo);
            }
        }
    }
}

/// <summary>
///  PooledObjectPolicy<T> 比 IPooledObjectPolicy 拥有更好的性能
/// </summary>
class FoobarListPolicy : PooledObjectPolicy<List<Foobar>>
{
    private readonly int _initCapacity;
    private readonly int _maxCapacity;

    public FoobarListPolicy(int initCapacity, int maxCapacity)
    {
        _initCapacity = initCapacity;
        _maxCapacity = maxCapacity;
    }
    public override List<Foobar> Create() => new List<Foobar>(_initCapacity);
    public override bool Return(List<Foobar> obj)
    {
        if (obj.Capacity <= _maxCapacity)
        {
            obj.Clear();
            return true;
        }
        return false;
    }
}

class Foobar
{
    public int Foo { get; }
    public int Bar { get; }
    public Foobar(int foo, int bar)
    {
        Foo = foo;
        Bar = bar;
    }
}

public class FooPooledObjectPolicy : IPooledObjectPolicy<Foo>
{
    public Foo Create()
    {
        return new Foo();
    }

    public bool Return(Foo obj)
    {
        return true;
    }
}
public class Foo : IDisposable
{
    public Foo()
    {
        var t = Interlocked.Increment(ref _id);
        Id = t.ToString();
    }
    static int _id;
    public string Id { get;}
    public void Dispose() => Console.Write($"{Id}-");
}