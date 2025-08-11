using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.API.Data;
using PEPScanner.Infrastructure.Services;

using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR for real-time notifications
builder.Services.AddSignalR();

// Database
builder.Services.AddDbContext<PepScannerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// HTTP Client for external API calls
builder.Services.AddHttpClient();

// Custom Services
builder.Services.AddScoped<PEPScanner.API.Services.IWatchlistUpdateService, PEPScanner.API.Services.WatchlistUpdateService>();
builder.Services.AddScoped<PEPScanner.API.Services.IPublicDataScrapingService, PEPScanner.API.Services.PublicDataScrapingService>();
builder.Services.AddScoped<PEPScanner.API.Services.IAutomatedScreeningService, PEPScanner.API.Services.AutomatedScreeningService>();
builder.Services.AddScoped<PEPScanner.API.Services.INotificationService, PEPScanner.API.Services.NotificationService>();

// Compliance Hierarchy Services
builder.Services.AddScoped<PEPScanner.Infrastructure.Services.ISmartAssignmentService, PEPScanner.Infrastructure.Services.SmartAssignmentService>();
builder.Services.AddScoped<PEPScanner.Infrastructure.Services.IEscalationService, PEPScanner.Infrastructure.Services.EscalationService>();
builder.Services.AddScoped<PEPScanner.API.Services.IAlertAssignmentService, PEPScanner.API.Services.AlertAssignmentService>();

// Register OpenSanctions services
builder.Services.AddHttpClient<PEPScanner.Infrastructure.Services.IOpenSanctionsService, PEPScanner.Infrastructure.Services.OpenSanctionsService>();
builder.Services.AddHttpClient<PEPScanner.Infrastructure.Services.IOpenSanctionsDataService, PEPScanner.Infrastructure.Services.OpenSanctionsDataService>();
builder.Services.AddScoped<PEPScanner.Infrastructure.Services.IEnhancedScreeningService, PEPScanner.Infrastructure.Services.EnhancedScreeningService>();
builder.Services.AddScoped<PEPScanner.API.Services.IOpenSanctionsUpdateService, PEPScanner.API.Services.OpenSanctionsUpdateService>();
builder.Services.AddScoped<PEPScanner.Infrastructure.Services.IOrganizationCustomListService, PEPScanner.Infrastructure.Services.OrganizationCustomListService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<PEPScanner.Infrastructure.Services.IReportService, PEPScanner.Infrastructure.Services.ReportService>();

// AI Risk Scoring and Financial Intelligence Services (commented out for now)
// builder.Services.AddScoped<PEPScanner.Application.Services.IAIRiskScoringService, PEPScanner.Application.Services.AIRiskScoringService>();
// builder.Services.AddScoped<PEPScanner.Application.Services.IRealTimeNotificationService, PEPScanner.Application.Services.RealTimeNotificationService>();
// builder.Services.AddScoped<PEPScanner.Application.Services.IFinancialIntelligenceService, PEPScanner.Application.Services.FinancialIntelligenceService>();

// Dashboard service removed - using simple controller implementation

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "your-super-secret-jwt-key-that-should-be-at-least-32-characters-long";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "PEPScanner";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "PEPScanner-Users";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    
    // In development, don't require authentication
    if (builder.Environment.IsDevelopment())
    {
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                return Task.CompletedTask;
            }
        };
    }
});

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

// Only enforce authentication in production
if (!app.Environment.IsDevelopment())
{
    app.UseAuthentication();
    app.UseAuthorization();
}

// Controllers
app.MapControllers();

// SignalR Hub (commented out for now)
// app.MapHub<PEPScanner.Application.Services.NotificationHub>("/notificationHub");

// Health check endpoints
app.MapGet("/api/health", () => new { status = "PEP Scanner Backend is running!", timestamp = DateTime.UtcNow });
app.MapGet("/api/version", () => new { version = "1.0.0", environment = "Development" });

// Initialize recurring jobs
using (var scope = app.Services.CreateScope())
{
    var watchlistUpdateService = scope.ServiceProvider.GetRequiredService<PEPScanner.API.Services.IWatchlistUpdateService>();
    watchlistUpdateService.ScheduleRecurringUpdates();

    var openSanctionsUpdateService = scope.ServiceProvider.GetRequiredService<PEPScanner.API.Services.IOpenSanctionsUpdateService>();
    await openSanctionsUpdateService.ScheduleRecurringUpdatesAsync();
}

app.Run();
