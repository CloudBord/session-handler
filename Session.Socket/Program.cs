using Session.DataAccess.Context;
using Session.DataAccess.Models;
using Session.DataAccess.Repositories;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configure secrets & environment variables
builder.Configuration.AddUserSecrets<Program>();
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var redisConfiguration = configuration.GetSection("Redis:ConnectionString").Value ?? 
                    throw new MissingFieldException("Redis:ConnectionString missing in appsettings.json!");
    return ConnectionMultiplexer.Connect(redisConfiguration);
});

builder.Services.Configure<SnapshotsDatabaseSettings>(
    builder.Configuration.GetSection("MongoDB")
);

builder.Services.AddSingleton<SnapshotsContext>();
builder.Services.AddScoped<ISnapshotRepository, SnapshotRepository>();

var app = builder.Build();

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseWebSockets();
app.MapControllers();
await app.RunAsync();