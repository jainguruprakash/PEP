using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.API.Data;

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

// HTTP Client for external API calls
builder.Services.AddHttpClient();

// Custom Services
builder.Services.AddScoped<PEPScanner.API.Services.IWatchlistUpdateService, PEPScanner.API.Services.WatchlistUpdateService>();

// Register OpenSanctions services
builder.Services.AddHttpClient<PEPScanner.Infrastructure.Services.IOpenSanctionsService, PEPScanner.Infrastructure.Services.OpenSanctionsService>();
builder.Services.AddHttpClient<PEPScanner.Infrastructure.Services.IOpenSanctionsDataService, PEPScanner.Infrastructure.Services.OpenSanctionsDataService>();
builder.Services.AddScoped<PEPScanner.Infrastructure.Services.IEnhancedScreeningService, PEPScanner.Infrastructure.Services.EnhancedScreeningService>();
builder.Services.AddScoped<PEPScanner.API.Services.IOpenSanctionsUpdateService, PEPScanner.API.Services.OpenSanctionsUpdateService>();
builder.Services.AddScoped<PEPScanner.Infrastructure.Services.IOrganizationCustomListService, PEPScanner.Infrastructure.Services.OrganizationCustomListService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:4201",
                "http://localhost:56733",
                "http://localhost:3000",
                "http://127.0.0.1:4200",
                "http://127.0.0.1:4201",
                "http://127.0.0.1:56733",
                "http://127.0.0.1:3000"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    // Add a more permissive policy for development
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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

// Use more permissive CORS in development
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCors");
}
else
{
    app.UseCors("AllowAngularApp");
}

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

app.UseAuthorization();

// Controllers
app.MapControllers();

// Health check endpoints
app.MapGet("/api/health", () => new { status = "PEP Scanner Backend is running!", timestamp = DateTime.UtcNow });
app.MapGet("/api/version", () => new { version = "1.0.0", environment = "Development" });

// Initialize recurring jobs and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PepScannerDbContext>();

    // Seed users and sample alerts
    await PEPScanner.API.Data.SeedData.SeedUsersAsync(context);
    await PEPScanner.API.Data.SeedData.SeedSampleAlertsAsync(context);

    var watchlistUpdateService = scope.ServiceProvider.GetRequiredService<PEPScanner.API.Services.IWatchlistUpdateService>();
    watchlistUpdateService.ScheduleRecurringUpdates();

    var openSanctionsUpdateService = scope.ServiceProvider.GetRequiredService<PEPScanner.API.Services.IOpenSanctionsUpdateService>();
    await openSanctionsUpdateService.ScheduleRecurringUpdatesAsync();
}

app.Run();
