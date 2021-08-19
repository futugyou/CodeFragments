using IdentityCenter.Data;
using IdentityCenter.Models;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace IdentityCenter
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
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "KongDemo", Version = "v1" });
            });

            var migrationsAssembly = typeof(Startup).Assembly.FullName;
            var serverVersion = new MySqlServerVersion(new Version(Configuration["MysqlVersion"]));

            // this is Microsoft.AspNetCore.Identity
            services.AddDbContext<ApplicationDbContext>(options =>
                   options.UseMySql(
                        Configuration.GetConnectionString("Identity"),
                        serverVersion,
                        ob =>
                        {
                            ob.MigrationsAssembly(migrationsAssembly);
                        })
                   );
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // this is identity server 4
            services.AddIdentityServer()
                .AddSigningCredential(Certificate.Certificate.Get())
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseMySql(
                        Configuration.GetConnectionString("IdentityConfigure"),
                        serverVersion,
                        ob =>
                        {
                            ob.MigrationsAssembly(migrationsAssembly);
                        });
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseMySql(
                        Configuration.GetConnectionString("IdentityOperate"),
                        serverVersion,
                        ob =>
                        {
                            ob.MigrationsAssembly(migrationsAssembly);
                        });
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            MigrateDatabase(app);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "KongDemo v1"));

            app.UseRouting();
            // this is identity server 4
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void MigrateDatabase(IApplicationBuilder app)
        {
            var scopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();
            if (scopeFactory != null)
            {
                using var serviceScope = scopeFactory.CreateScope();
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                if (!context.Clients.Any())
                {
                    foreach (var client in Config.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.IdentityResources)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiScopes.Any())
                {
                    foreach (var resource in Config.ApiScopes)
                    {
                        context.ApiScopes.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
