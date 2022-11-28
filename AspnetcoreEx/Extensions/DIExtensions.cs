using Microsoft.Extensions.DependencyInjection.Extensions;
namespace AspnetcoreEx.Extensions;

public static class DIExtensions
{
    public static IServiceCollection AddDIExtension(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IUsefull>(new UsefullA()));
        Func<IServiceProvider, UsefullB> usefull = _ => new UsefullB();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IUsefull>(usefull));
        services.Replace(ServiceDescriptor.Singleton<IUsefull>(new UsefullC()));
        services.Remove(ServiceDescriptor.Singleton<IUsefull>(new UsefullB()));
        services.RemoveAll<IUsefull>();

        Func<IServiceProvider, IUsefull> _ = sp => ActivatorUtilities.CreateInstance<IUsefull>(sp, "thisisname");
        return services;
    }
}

public interface IUsefull { }
public class UsefullA : IUsefull { }
public class UsefullB : IUsefull { }
public class UsefullC : IUsefull { }
public class UsefullD : IUsefull
{
    public UsefullD(string name) { }
}

public interface IServiceScope : IDisposable
{
    IServiceProvider ServiceProvider { get; }
}
public interface IServiceScopeFactory
{
    IServiceScope CreateScope();
}
public static class ServiceProviderEx
{
    public static IServiceScope CreateScope(this IServiceProvider provider)
    {
        return provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
    }
}