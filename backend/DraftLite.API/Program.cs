using Microsoft.EntityFrameworkCore;
using DraftLite.Api.DependencyInjection;
using DraftLite.Api.Hubs;
using DraftLite.Api.Security;
using DraftLite.Data;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(',') ?? new string[] { };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Add Logger
builder.Services.AddLogging();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DraftLiteDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddAppServices();
builder.Services.AddJwtRoutingSecurity(builder.Configuration);

// Or more explicitly:
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 App started and logging works!");

app.UseCors("AllowAllOrigins");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<CollaborationHub>("/hubs/collaboration");

app.Run();

