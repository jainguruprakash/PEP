using Microsoft.EntityFrameworkCore;
using PEPScanner.API.Data;
using PEPScanner.Application.Abstractions;
using PEPScanner.Infrastructure.Services;
using AppIScreeningService = PEPScanner.Application.Abstractions.IScreeningService;
using InfraScreeningService = PEPScanner.Infrastructure.Services.ScreeningService;
using Serilog;
using PEPScanner.API;
using Hangfire;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/pep-scanner-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PEP Scanner API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PepScannerDbContext>(options =>
    options.UseSqlite(connectionString));

// Hangfire for background jobs (temporarily disabled for Clean Architecture demo)
// TODO: Re-enable with proper storage configuration
// builder.Services.AddHangfire(config =>
//     config.UseSimpleAssemblyNameTypeSerializer()
//           .UseRecommendedSerializerSettings());
// builder.Services.AddHangfireServer();

// HTTP Client
builder.Services.AddHttpClient();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Application Services - Temporarily commented out for compilation
// builder.Services.AddScoped<INameMatchingService, PEPScanner.Infrastructure.Services.NameMatchingService>();
// builder.Services.AddScoped<AppIScreeningService, InfraScreeningService>(); // Temporarily commented out - service doesn't exist
// builder.Services.AddScoped<IOfacDataService, PEPScanner.Infrastructure.Services.OfacDataService>();
// builder.Services.AddScoped<IUnSanctionsService, PEPScanner.Infrastructure.Services.UnSanctionsService>();
// builder.Services.AddScoped<IRbiWatchlistService, PEPScanner.Infrastructure.Services.RbiWatchlistService>();
// builder.Services.AddScoped<IInHouseFileProcessorService, PEPScanner.Infrastructure.Services.InHouseFileProcessorService>();

// New Watchlist Services - Temporarily commented out for compilation
// builder.Services.AddScoped<PEPScanner.Infrastructure.Services.SebiWatchlistService>();
// builder.Services.AddScoped<PEPScanner.Infrastructure.Services.IndianParliamentWatchlistService>();

// New Services - Temporarily commented out for compilation
// builder.Services.AddScoped<IBiometricMatchingService, PEPScanner.Infrastructure.Services.BiometricMatchingService>();
// builder.Services.AddScoped<IAdverseMediaService, PEPScanner.Infrastructure.Services.AdverseMediaService>();
// builder.Services.AddScoped<INotificationService, PEPScanner.Infrastructure.Services.NotificationService>();
// builder.Services.AddScoped<IScheduledJobService, ScheduledJobService>(); // Temporarily disabled with Hangfire

// Watchlist Service Registry - Temporarily commented out for compilation
// builder.Services.AddSingleton<IWatchlistServiceRegistry, PEPScanner.Infrastructure.Services.WatchlistServiceRegistry>();

// Authentication
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication("Dev")
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, PEPScanner.API.Dev.DevAuthenticationHandler>("Dev", options => { });
}
else
{
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = builder.Configuration["Jwt:Authority"];
            options.Audience = builder.Configuration["Jwt:Audience"];
            options.RequireHttpsMetadata = false; // For development
        });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ComplianceOfficer", policy =>
        policy.RequireRole("ComplianceOfficer", "Manager"));
    options.AddPolicy("Manager", policy =>
        policy.RequireRole("Manager"));
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PEP Scanner API v1");
        c.RoutePrefix = string.Empty; // Serve at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard (temporarily disabled)
// app.UseHangfireDashboard("/hangfire", new DashboardOptions
// {
//     Authorization = new[] { new HangfireAuthorizationFilter() }
// });

// Register watchlist services in registry - Temporarily commented out for compilation
/*
using (var scope = app.Services.CreateScope())
{
    var registry = scope.ServiceProvider.GetRequiredService<IWatchlistServiceRegistry>();
    
    // Register existing services as base watchlist services
    var rbiService = scope.ServiceProvider.GetRequiredService<IRbiWatchlistService>();
    if (rbiService is IBaseWatchlistService baseRbiService)
    {
        registry.RegisterService(baseRbiService);
    }
    
    // Register new services
    var sebiService = scope.ServiceProvider.GetRequiredService<SebiWatchlistService>();
    registry.RegisterService(sebiService);
    
    var parliamentService = scope.ServiceProvider.GetRequiredService<IndianParliamentWatchlistService>();
    registry.RegisterService(parliamentService);
}
*/

// Initialize scheduled jobs (temporarily disabled with Hangfire)
// using (var scope = app.Services.CreateScope())
// {
//     try
//     {
//         var scheduledJobService = scope.ServiceProvider.GetRequiredService<IScheduledJobService>();
//         
//         // Schedule all recurring jobs
//         await scheduledJobService.ScheduleWatchlistUpdateJobsAsync();
//         await scheduledJobService.ScheduleCustomerScreeningJobsAsync();
//         await scheduledJobService.ScheduleAdverseMediaScanJobsAsync();
//         await scheduledJobService.ScheduleReportGenerationJobsAsync();
//         
//         Log.Information("All scheduled jobs initialized successfully");
//     }
//     catch (Exception ex)
//     {
//         Log.Error(ex, "Error initializing scheduled jobs");
//     }
// }

// Map API endpoints
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

// Seed data for development - Temporarily commented out completely
/*
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<PepScannerDbContext>();
    // Apply migrations automatically in development
    await context.Database.MigrateAsync();
    // await SeedData.SeedAllDataAsync(context); // Temporarily commented out
}
*/

app.Run();


