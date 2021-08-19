using Microsoft.EntityFrameworkCore;

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
            var migrationsAssembly = typeof(Startup).Assembly.FullName;
            var serverVersion = new MySqlServerVersion(new Version(Configuration["MysqlVersion"]));

            services.AddIdentityServer()
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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
