
using AgentStack.Services;

namespace AgentStack;

[Experimental("SKEXP0010")]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKernelMemoryServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AgentOptions>(configuration.GetSection("Agent"));
 
        services.AddScoped<AgentService>();
        services.AddScoped<WorkflowService>();

        return services;
    }
}