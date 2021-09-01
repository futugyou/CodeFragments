using Duende.IdentityServer;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityCenter.Data;
using IdentityCenter.Extensions;
using IdentityCenter.Models;
using IdentityCenter.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

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
            services.AddTransient<IClaimsTransformation, UserClaimsTransformation>();
            services.AddSingleton<IEmailSender, EmailSender>();

            services.AddControllersWithViews();
            services.AddRazorPages();
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

            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AdditionalUserClaimsPrincipalFactory>();

            // this is identity server 4
            services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v5/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;
                options.Authentication.CookieLifetime = TimeSpan.FromHours(1);
                options.Authentication.CookieSlidingExpiration = false;

                options.EmitScopesAsSpaceDelimitedStringInJwt = true;
                options.Endpoints.EnableJwtRequestUri = true;

                // associate a client certificate with a client in your IdentityServer and enable MTLS support on the options.
                options.MutualTls.Enabled = true;

                options.KeyManagement.KeyPath = "/keys";
                // new key every 10 days
                options.KeyManagement.RotationInterval = TimeSpan.FromDays(10);
                // announce new key 2 days in advance in discovery
                options.KeyManagement.PropagationTime = TimeSpan.FromDays(2);
                // keep old key for 7 days in discovery for validation of tokens
                options.KeyManagement.RetentionDuration = TimeSpan.FromDays(7);
                options.KeyManagement.SigningAlgorithms = new[]
                {
                    // RS256 for older clients (with additional X.509 wrapping)
                    new SigningAlgorithmOptions(SecurityAlgorithms.RsaSha256) { UseX509Certificate = true },    
                    // PS256
                    new SigningAlgorithmOptions(SecurityAlgorithms.RsaSsaPssSha256),    
                    // ES256
                    new SigningAlgorithmOptions(SecurityAlgorithms.EcdsaSha256)
                };

            })
                // Parses a JWT on the client_assertion body field. Can be enabled by calling the AddJwtBearerClientAuthentication DI extension method.
                // Validates JWTs that are signed with either X.509 certificates or keys wrapped in a JWK. Can be enabled by calling the AddJwtBearerClientAuthentication DI extension method.
                .AddJwtBearerClientAuthentication()
                // Parses the client_id body field and TLS client certificate. Can be enabled by calling the AddMutualTlsSecretValidators DI extension method.
                // Validates X.509 client certificates based on a thumbprint. Can be enabled by calling the AddMutualTlsSecretValidators DI extension method.
                // Validates X.509 client certificates based on a common name. Can be enabled by calling the AddMutualTlsSecretValidators DI extension method.
                .AddMutualTlsSecretValidators()
                //.AddSigningCredential(Certificate.Certificate.Get())
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
                })
                // link id4 and identity
                .AddAspNetIdentity<ApplicationUser>();

            // configures the OpenIdConnect handlers to persist the state parameter into the server-side IDistributedCache.
            services.AddOidcStateDataFormatterCache();

            // TLS Client Certificates Authentication
            services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate(options =>
                {
                    options.AllowedCertificateTypes = CertificateTypes.All;
                    options.RevocationMode = X509RevocationMode.NoCheck;
                    options.Events = new CertificateAuthenticationEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnCertificateValidated = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            return Task.CompletedTask;
                        },
                    };
                })
                // Adding an ICertificateValidationCache results in certificate auth caching the results.
                // The default implementation uses a memory cache.
                .AddCertificateCache();

            services.AddAuthentication()
                .AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = "<insert here>";
                    options.ClientSecret = "<insert here>";
                })
                .AddOpenIdConnect("oidc", "Demo IdentityServer", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                    options.SaveTokens = true;

                    options.Authority = "https://demo.duendesoftware.com";
                    options.ClientId = "interactive.confidential";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                });

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            MigrateDatabase(app).Wait();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "KongDemo v1"));
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCertificateForwarding();
            // this is identity server 4
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }

        private static async Task MigrateDatabase(IApplicationBuilder app)
        {
            var scopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();
            if (scopeFactory != null)
            {
                using var serviceScope = scopeFactory.CreateScope();
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
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
                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.ApiResources)
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                var applicationDbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                applicationDbContext.Database.Migrate();
                if (!applicationDbContext.Users.Any())
                {
                    applicationDbContext.Users.AddRange(Config.ApplicationUsers());
                    applicationDbContext.SaveChanges();
                }
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                if (!applicationDbContext.UserClaims.Any())
                {
                    foreach (var user in applicationDbContext.Users.ToList())
                    {
                        await userManager.AddClaimAsync(user, new Claim("card", user.Card));
                    }
                }
            }
        }
    }
}
