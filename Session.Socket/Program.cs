var builder = WebApplication.CreateBuilder(args);

// Configure secrets & environment variables
builder.Configuration.AddUserSecrets<Program>();
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseWebSockets();
app.MapControllers();


await app.RunAsync();