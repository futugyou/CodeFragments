using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthServer.Host.EntityFrameworkCore
{
    public class AuthServerDbContextFactory : IDesignTimeDbContextFactory<AuthServerDbContext>
    {
        public AuthServerDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var serverVersion = new MySqlServerVersion(new Version(configuration["DefaultMysqlVersion"]));
            var builder = new DbContextOptionsBuilder<AuthServerDbContext>()
                .UseMySql(configuration.GetConnectionString("Default"), serverVersion);

            return new AuthServerDbContext(builder.Options);
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            return builder.Build();
        }
    }
}
