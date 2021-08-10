using Consul;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace ConsulDemo.Consul
{
    public class ConsulBuilder
    {
        private readonly ConsulClient _client;
        private readonly List<AgentServiceCheck> _checks = new List<AgentServiceCheck>();

        public ConsulBuilder(ConsulClient client) => this._client = client;

        public ConsulBuilder AddHealthCheck(AgentServiceCheck check)
        {
            this._checks.Add(check);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeout">unit: second</param>
        /// <param name="interval">check interval. unit: second</param>
        /// <returns></returns>
        public ConsulBuilder AddHttpHealthCheck(string url, int timeout = 10, int interval = 10)
        {
            this._checks.Add(new AgentServiceCheck()
            {
                DeregisterCriticalServiceAfter = new TimeSpan?(TimeSpan.FromSeconds((double)(timeout * 3))),
                Interval = new TimeSpan?(TimeSpan.FromSeconds((double)interval)),
                HTTP = url,
                Timeout = new TimeSpan?(TimeSpan.FromSeconds((double)timeout))
            });
            //PassportConsole.Success("[Consul]Add Http Healthcheck Success! CheckUrl:" + url);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint">GPRC service address.</param>
        /// <param name="grpcUseTls"></param>
        /// <param name="timeout">unit: second</param>
        /// <param name="interval">check interval. unit: second</param>
        /// <returns></returns>
        public ConsulBuilder AddGRPCHealthCheck(
          string endpoint,
          bool grpcUseTls = false,
          int timeout = 10,
          int interval = 10)
        {
            this._checks.Add(new AgentServiceCheck()
            {
                DeregisterCriticalServiceAfter = new TimeSpan?(TimeSpan.FromSeconds(20.0)),
                Interval = new TimeSpan?(TimeSpan.FromSeconds((double)interval)),
                GRPC = endpoint,
                GRPCUseTLS = grpcUseTls,
                Timeout = new TimeSpan?(TimeSpan.FromSeconds((double)timeout))
            });
            //PassportConsole.Success("[Consul]Add GRPC HealthCheck Success! Endpoint:" + endpoint);
            return this;
        }

        public async Task RegisterService(string name, string host, int port, string[] tags)
        {
            AgentServiceRegistration registration = new AgentServiceRegistration()
            {
                Checks = this._checks.ToArray(),
                ID = string.Format("{0}_{1}:{2}", (object)name, (object)host, (object)port),
                Name = name,
                Address = host,
                Port = port,
                Tags = tags
            };
            WriteResult writeResult1 = await this._client.Agent.ServiceRegister(registration, new CancellationToken());
            //PassportConsole.Success("[Consul]Register Service Success! Name:" + name + " ID:" + registration.ID);
            AppDomain.CurrentDomain.ProcessExit += (EventHandler)(async (sender, e) =>
            {
                //PassportConsole.Information("[Consul] Service Deregisting ....  ID:" + registration.ID);
                WriteResult writeResult2 = await this._client.Agent.ServiceDeregister(registration.ID, new CancellationToken());
            });
        }

        /// <summary>移除服务</summary>
        /// <param name="serviceId"></param>
        public async Task Deregister(string serviceId)
        {
            WriteResult writeResult = await this._client?.Agent?.ServiceDeregister(serviceId, new CancellationToken());
        }
    }
}
