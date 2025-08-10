using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Application.Services;
using Hangfire;
using Hangfire.MemoryStorage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<PepScannerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMemoryStorage());

builder.Services.AddHangfireServer();

// Application Services
builder.Services.AddHttpClient<IWatchlistDataFetchService, WatchlistDataFetchService>();
builder.Services.AddScoped<IWatchlistDataFetchService, WatchlistDataFetchService>();
builder.Services.AddScoped<IWatchlistJobService, WatchlistJobService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

app.UseAuthorization();

// Controllers
app.MapControllers();

// Health check endpoints
app.MapGet("/api/health", () => new { status = "PEP Scanner Backend is running!", timestamp = DateTime.UtcNow });
app.MapGet("/api/version", () => new { version = "1.0.0", environment = "Development" });

app.Run();
