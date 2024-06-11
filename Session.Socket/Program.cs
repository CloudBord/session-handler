using Session.Socket.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure secrets & environment variables
builder.Configuration.AddUserSecrets<Program>();
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<ISessionHandler, SessionHandler>();

var app = builder.Build();

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseWebSockets();

app.MapControllers();

app.Run();
