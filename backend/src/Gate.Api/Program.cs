using Gate.Api.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("Gate")
    ?? throw new InvalidOperationException(
        "Connection string 'Gate' is missing. Set ConnetionString:Gate in appsettings or the ConnectionStings__Gate enviroment variable.");

builder.Services.AddDbContext<GateDbContext>(options =>
    options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());


builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<GateDbContext>();

    db.Database.Migrate();
}
app.UseExceptionHandler();

app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseStaticFiles();
app.MapControllers();

app.MapFallback("/api/{**path}",()=> Results.NotFound());

app.MapFallbackToFile("index.html");

app.Run();
