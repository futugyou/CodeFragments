using Microsoft.Extensions.DependencyInjection.Extensions;
namespace AspnetcoreEx.Extensions;

public static class DIExtensions
{
    public static IServiceCollection AddDIExtension(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IUsefull>(new UsefullA()));
        Func<IServiceProvider, IUsefull> usefull = _ => new UsefullB();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IUsefull>(usefull));
        services.Replace(ServiceDescriptor.Singleton<IUsefull>(new UsefullC()));
        services.Remove(ServiceDescriptor.Singleton<IUsefull>(new UsefullB()));
        services.RemoveAll<IUsefull>();
        return services;
    }
}

public interface IUsefull { }
public class UsefullA : IUsefull { }
public class UsefullB : IUsefull { }
public class UsefullC : IUsefull { }