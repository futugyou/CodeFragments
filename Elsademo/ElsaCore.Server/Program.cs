using Elsa;
using Elsa.Persistence.EntityFramework.Core.Extensions;
using Elsa.Activities.UserTask.Extensions;
using Elsa.Persistence.EntityFramework.SqlServer;

var builder = WebApplication.CreateBuilder(args);

// Elsa services.
var elsaSection = builder.Configuration.GetSection("Elsa");
builder.Services.AddElsa(elsa => elsa
                    .UseEntityFrameworkPersistence(ef => ef.UseSqlServer(builder.Configuration.GetConnectionString("Default"), typeof(Program)))
                    .AddConsoleActivities()
                    .AddJavaScriptActivities()
                    .AddUserTaskActivities()
                    .AddHttpActivities(elsaSection.GetSection("Server").Bind)
                    .AddQuartzTemporalActivities()
                    .AddWorkflowsFrom<Program>()
                )
                // Elsa API endpoints.
                .AddElsaApiEndpoints()

                // For Dashboard.
                .AddRazorPages();
var app = builder.Build();

app.UseStaticFiles()// For Dashboard.
    .UseHttpActivities()
    .UseRouting()
    .UseEndpoints(endpoints =>
    {
        // Elsa API Endpoints are implemented as regular ASP.NET Core API controllers.
        endpoints.MapControllers();
        // For Dashboard
        endpoints.MapFallbackToPage("/_Host");
    });
app.Run();
