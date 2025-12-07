
using AgentStack.Services;

namespace AgentStack;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AgentOptions>(configuration.GetSection("Agent"));
 
        services.AddScoped<AgentService>();
        services.AddScoped<WorkflowService>();

        return services;
    }
}