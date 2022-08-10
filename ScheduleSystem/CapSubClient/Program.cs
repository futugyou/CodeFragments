using CapSubClient;
using DotNetCore.CAP;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
var mqoption = new RabbitMQOptions();
configuration.GetSection("RabbitMQ").Bind(mqoption);

builder.Services.AddTransient<ISubscriberService, SubscriberService>();

builder.Services.AddCap(x =>
{
    x.UseInMemoryStorage();
    x.UseRabbitMQ(r =>
    {
        r.UserName = mqoption.UserName;
        r.Password = mqoption.Password;
        r.Port = mqoption.Port;
    });
    x.UseDashboard();
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
