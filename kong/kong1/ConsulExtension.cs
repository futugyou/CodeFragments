using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kong1
{
    public static class ConsulExtension
    {
        public static IServiceCollection AddConsul(this IServiceCollection services)
        {
            ServiceDiscoveryOptions options = (ServiceDiscoveryOptions)ConfigurationBinder.Get<ServiceDiscoveryOptions>((IConfiguration)PassportConfig.GetSection("ServiceDiscovery"));
            ServiceDiscoveryOptions discoveryOptions = options;
            if ((discoveryOptions != null ? (!discoveryOptions.Disable ? 1 : 0) : 1) != 0)
            {
                string healthHost = PassportConfig.GetHealthHost();
                if (string.IsNullOrWhiteSpace(options?.ServiceName) || string.IsNullOrWhiteSpace(options?.Consul?.HttpEndPoint))
                    throw new ArgumentNullException("ServiceDiscovery.ServiceName/Consul.HttpEndpoint cannot be null or empty!");
                ConsulClient client = new ConsulClient(x => x.Address = new Uri(options.Consul.HttpEndPoint));
                ServiceCollectionServiceExtensions.AddSingleton<ConsulClient>(services, client);
                OptionsServiceCollectionExtensions.Configure<ConsulOptions>(services, op =>
                {
                    op.HttpEndPoint = options.Consul.HttpEndPoint;
                    op.Token = options.Consul.Token;
                    op.TcpEndPoint = options.Consul.TcpEndPoint;
                });
                HttpHeathCheck httpHeathCheck = options.Consul.HttpHeathCheck;
                string url = string.Format("http://{0}:{1}{2}", (object)healthHost, (object)PassportConfig.GetCurrentPort(), (object)httpHeathCheck.Path);
                new ConsulBuilder(client).AddHttpHealthCheck(url, httpHeathCheck.TimeOunt, httpHeathCheck.Interval).RegisterService(options.ServiceName, healthHost, PassportConfig.GetCurrentPort(), options.Consul.Tags).Wait();
            }
            return services;
        }
    }
}
