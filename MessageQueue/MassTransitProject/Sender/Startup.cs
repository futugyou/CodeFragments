using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sender
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            #region hostedwork
            //services.AddMassTransit(x =>
            //{
            //    x.AddConsumer<MessageConsumer>();

            //    // in memory 
            //    //x.UsingInMemory((context, cfg) =>
            //    //{
            //    //    cfg.ConfigureEndpoints(context);
            //    //});

            //    // in rabbitmq
            //    x.UsingRabbitMq((context, cfg) =>
            //    {
            //        cfg.Host(Configuration["RabbitmqConfig:HostIP"], h =>
            //        {
            //            h.Username(Configuration["RabbitmqConfig:Username"]);
            //            h.Password(Configuration["RabbitmqConfig:Password"]);
            //        });
            //        cfg.ConfigureEndpoints(context);
            //    });
            //});
            //services.AddMassTransitHostedService();
            //services.AddHostedService<Worker>();
            #endregion

            #region publish messages from a controller
            //services.AddMassTransit(x =>
            //{
            //    x.SetKebabCaseEndpointNameFormatter();
            //    x.UsingRabbitMq((context, cfg) =>
            //    {
            //        cfg.Host(Configuration["RabbitmqConfig:HostIP"], h =>
            //        {
            //            h.Username(Configuration["RabbitmqConfig:Username"]);
            //            h.Password(Configuration["RabbitmqConfig:Password"]);

            //        });
            //        cfg.ConfigureEndpoints(context);
            //    });
            //});

            //services.AddMassTransitHostedService();
            //services.AddGenericRequestClient();
            #endregion

            #region add mediator
            services.AddHttpContextAccessor();
            //services.AddMediator(cfg =>
            //{
            //    cfg.AddConsumer<SubmitOrderConsumer>();
            //    cfg.AddConsumer<OrderStatusConsumer>();

            //    cfg.ConfigureMediator((context, mcfg) =>
            //    {
            //        mcfg.UseSendFilter(typeof(ValidateOrderStatusFilter<>), context);
            //        mcfg.UseHttpContextScopeFilter(context);
            //    });
            //});
            #endregion

            #region servicebus
            services.AddMassTransit(x =>
            {
                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(Configuration["azure:url"]);
                    cfg.ReceiveEndpoint("input-queue", e =>
                    {
                        // all of these are optional!!
                        e.PrefetchCount = 100;

                        // number of "threads" to run concurrently
                        e.MaxConcurrentCalls = 100;

                        // default, but shown for example
                        e.LockDuration = TimeSpan.FromMinutes(5);

                        // lock will be renewed up to 30 minutes
                        e.MaxAutoRenewDuration = TimeSpan.FromMinutes(30);
                    });
                });
            });
            #endregion
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sender", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sender v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
