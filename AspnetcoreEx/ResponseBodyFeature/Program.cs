using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<ResponseCustomMiddleware>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ResponseBodyFeature", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ResponseBodyFeature v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseMiddleware<ResponseCustomMiddleware>();
//app.Use(async (context, next) =>
//{
//    var newContent = string.Empty;

//    using (var newBody = new MemoryStream())
//    {
//        context.Response.Body = newBody;

//        await next();

//        context.Response.Body = new MemoryStream();

//        newBody.Seek(0, SeekOrigin.Begin);

//        newContent = new StreamReader(newBody).ReadToEnd();

//        newContent += ", World!";
//        await context.Response.WriteAsync(newContent);
//    }
//});
app.Run();
