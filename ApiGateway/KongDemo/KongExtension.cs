using Kong;
using Kong.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace KongDemo
{
    public static class KongExtension
    {
        public static IServiceCollection RouteRegistToKong(this IServiceCollection services)
        {
            if (!PassportConfig.GetBool("Kong:Disable"))
            {
                string str = PassportConfig.Get("Kong:Host") ?? throw new ArgumentNullException("Kong:Host cannot be null or empty!");
                KongClient kongClient = new KongClient(new KongClientOptions(HttpClientFactory.Create(Array.Empty<DelegatingHandler>()), str));
                ServiceCollectionServiceExtensions.AddSingleton<KongClient>(services, kongClient);
                UpStream upStream = (UpStream)ConfigurationBinder.Get<UpStream>((IConfiguration)PassportConfig.GetSection("Kong:Upstream"));
                TargetInfo targetInfo = (TargetInfo)ConfigurationBinder.Get<TargetInfo>((IConfiguration)PassportConfig.GetSection("Kong:Target"));
                if (upStream != null && targetInfo != null)
                {
                    upStream.Created_at = new DateTime?(DateTime.Now);
                    UpStream result = kongClient.UpStream.UpdateOrCreate(upStream).Result;
                    targetInfo.Target = string.Format("{0}:{1}", (object)PassportConfig.GetHealthHost(), (object)PassportConfig.GetCurrentPort());
                    targetInfo.Id = new Guid?(PassportTools.GuidFromString(Dns.GetHostName() + targetInfo.Target));
                    targetInfo.Created_at = new DateTime?(DateTime.Now);
                    targetInfo.UpStream = new TargetInfo.UpStreamId()
                    {
                        Id = result.Id.Value
                    };
                    kongClient.Target.Add(targetInfo).Wait();
                    PassportConsole.Success("[Kong]UpStream registered:" + result.Name + " Target:" + targetInfo.Target);
                }
                ServiceInfo[] serviceInfoArray = ((IConfiguration)PassportConfig.GetSection("Kong:Services")).Get<ServiceInfo[]>();
                RouteInfo[] routeInfoArray = ((IConfiguration)PassportConfig.GetSection("Kong:Routes")).Get<RouteInfo[]>();
                if (serviceInfoArray != null && serviceInfoArray.Length != 0)
                {
                    foreach (ServiceInfo serviceInfo in serviceInfoArray)
                    {
                        serviceInfo.Updated_at = new DateTime?(DateTime.Now);
                        serviceInfo.Path = string.IsNullOrWhiteSpace(serviceInfo.Path) ? (string)null : serviceInfo.Path;
                        kongClient.Service.UpdateOrCreate(serviceInfo).Wait();
                        PassportConsole.Success("[Kong]Service registered:" + serviceInfo.Name);
                    }
                }
                if (routeInfoArray != null && routeInfoArray.Length != 0)
                {
                    foreach (RouteInfo routeInfo in routeInfoArray)
                    {
                        routeInfo.Updated_at = new DateTime?(DateTime.Now);
                        kongClient.Route.UpdateOrCreate(routeInfo).Wait();
                        PassportConsole.Success("[Kong]Route registered:" + routeInfo.Name);
                    }
                }
            }
            return services;
        }
    }
}
