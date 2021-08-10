using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using System.Threading;

namespace ConsulDemo.Consul
{
    public static class ConsulExtension
    {
        public static IServiceCollection AddConsul(this IServiceCollection services, IConfiguration configuration)
        {
            // this can only 'use' the value ,but it can not 'bind' value
            ConsulOptions options = ConfigurationBinder.Get<ConsulOptions>(configuration.GetSection("Consul"));
            // bind the value
            services.Configure<ConsulOptions>(configuration.GetSection("Consul"));

            if (options == null || string.IsNullOrWhiteSpace(options.ServerName)
                || string.IsNullOrWhiteSpace(options.Host))
                throw new ArgumentNullException("Consul.ServiceName or Consul.HttpEndpoint cannot be null or empty!");

            var host = options.Host;
            if (options.Port != 0)
            {
                host += ":" + options.Port;
            }
            services.AddSingleton<IConsulClient, ConsulClient>(
                sp => new ConsulClient(x => x.Address = new Uri(host))
                );
            return services;
        }

        public static IApplicationBuilder UseConsul(this IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
            var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            lifetime.ApplicationStarted.Register(() =>
            {
                consulClient.RegisterService(app);
            });

            //停止的时候移除服务
            lifetime.ApplicationStopped.Register(() =>
            {
                consulClient.UnRegisterService(app);
            });
            return app;
        }

        public static void RegisterService(this IConsulClient consul, IApplicationBuilder app)
        {
            // this is http://localhost:5000 and https://locahost:5001.
            //var features = app.Properties["server.Features"] as FeatureCollection;
            //var addresses = features.Get<IServerAddressesFeature>().Addresses.Select(p => new Uri(p));

            // this is all network.
            //var name = Dns.GetHostName(); // get container id
            //var ips = Dns.GetHostEntry(name).AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork);

            var serviceOptions = app.ApplicationServices.GetRequiredService<IOptions<ConsulOptions>>().Value;

            ServiceID = $"{serviceOptions.ServerName}_{serviceOptions.Host}:{serviceOptions.Port}";

            //serviceid必须是唯一的，以便以后再次找到服务的特定实例，以便取消注册。这里使用主机和端口以及实际的服务名

            var httpCheck = new AgentServiceCheck()
            {
                DeregisterCriticalServiceAfter = new TimeSpan?(TimeSpan.FromSeconds(serviceOptions.HttpHeathCheck.TimeOunt * 3)),
                Interval = new TimeSpan?(TimeSpan.FromSeconds(serviceOptions.HttpHeathCheck.Interval)),
                HTTP = new Uri($"{serviceOptions.HttpHeathCheck.Host}{serviceOptions.HttpHeathCheck.Path}").OriginalString,
                Timeout = new TimeSpan?(TimeSpan.FromSeconds(serviceOptions.HttpHeathCheck.TimeOunt)),
            };

            var registration = new AgentServiceRegistration()
            {
                Checks = new[] { httpCheck },
                Address = serviceOptions.Host,
                ID = ServiceID,
                Name = serviceOptions.ServerName,
                Port = serviceOptions.Port
            };
            consul.Agent.ServiceRegister(registration).GetAwaiter().GetResult();

        }

        private static string ServiceID;
        public static void UnRegisterService(this IConsulClient consul, IApplicationBuilder app)
        {
            consul.Agent?.ServiceDeregister(ServiceID, new CancellationToken()).GetAwaiter().GetResult();
        }
    }
}