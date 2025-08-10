using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using System.Text.Json;

namespace PEPScanner.Application.Services
{
    public interface IRealTimeNotificationService
    {
        Task SendScanCompleteNotificationAsync(Guid customerId, CustomerScanResult scanResult);
        Task SendAlertCreatedNotificationAsync(Guid alertId, string alertType, string entityName);
        Task SendRiskScoreUpdateNotificationAsync(Guid customerId, AIRiskAssessment riskAssessment);
        Task SendSystemNotificationAsync(string message, string type, string? userId = null);
        Task SendBulkScanProgressAsync(string scanId, BulkScanProgress progress);
        Task JoinCustomerGroupAsync(string connectionId, Guid customerId);
        Task LeaveCustomerGroupAsync(string connectionId, Guid customerId);
        Task JoinUserGroupAsync(string connectionId, string userId);
        Task LeaveUserGroupAsync(string connectionId, string userId);
    }

    public class RealTimeNotificationService : IRealTimeNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<RealTimeNotificationService> _logger;
        private readonly PepScannerDbContext _context;

        public RealTimeNotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<RealTimeNotificationService> logger,
            PepScannerDbContext context)
        {
            _hubContext = hubContext;
            _logger = logger;
            _context = context;
        }

        public async Task SendScanCompleteNotificationAsync(Guid customerId, CustomerScanResult scanResult)
        {
            try
            {
                var notification = new RealTimeNotification
                {
                    Id = Guid.NewGuid(),
                    Type = "scan_complete",
                    Title = "Customer Scan Complete",
                    Message = $"Scan completed for {scanResult.CustomerName}",
                    Data = new
                    {
                        customerId,
                        scanResult.MediaResultsFound,
                        scanResult.HighRiskResults,
                        scanResult.AlertsCreated,
                        scanResult.Status
                    },
                    Timestamp = DateTime.UtcNow,
                    Priority = scanResult.HighRiskResults > 0 ? "High" : "Medium",
                    Category = "CustomerScan"
                };

                // Send to customer-specific group
                await _hubContext.Clients.Group($"customer_{customerId}")
                    .SendAsync("ReceiveNotification", notification);

                // Send to all compliance users
                await _hubContext.Clients.Group("compliance_users")
                    .SendAsync("ReceiveNotification", notification);

                // Store notification for persistence
                await StoreNotificationAsync(notification);

                _logger.LogInformation("Sent scan complete notification for customer {CustomerId}", customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending scan complete notification for customer {CustomerId}", customerId);
            }
        }

        public async Task SendAlertCreatedNotificationAsync(Guid alertId, string alertType, string entityName)
        {
            try
            {
                var notification = new RealTimeNotification
                {
                    Id = Guid.NewGuid(),
                    Type = "alert_created",
                    Title = "New Alert Created",
                    Message = $"New {alertType} alert created for {entityName}",
                    Data = new
                    {
                        alertId,
                        alertType,
                        entityName,
                        createdAt = DateTime.UtcNow
                    },
                    Timestamp = DateTime.UtcNow,
                    Priority = "High",
                    Category = "Alert",
                    ActionRequired = true,
                    ActionUrl = $"/alerts/{alertId}"
                };

                // Send to all compliance users
                await _hubContext.Clients.Group("compliance_users")
                    .SendAsync("ReceiveNotification", notification);

                // Send to alert managers
                await _hubContext.Clients.Group("alert_managers")
                    .SendAsync("ReceiveNotification", notification);

                await StoreNotificationAsync(notification);

                _logger.LogInformation("Sent alert created notification for alert {AlertId}", alertId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending alert created notification for alert {AlertId}", alertId);
            }
        }

        public async Task SendRiskScoreUpdateNotificationAsync(Guid customerId, AIRiskAssessment riskAssessment)
        {
            try
            {
                var riskLevel = GetRiskLevel(riskAssessment.RiskScore);
                var previousAssessment = await GetPreviousRiskAssessmentAsync(customerId);
                var riskChange = previousAssessment != null ? 
                    riskAssessment.RiskScore - previousAssessment.RiskScore : 0;

                var notification = new RealTimeNotification
                {
                    Id = Guid.NewGuid(),
                    Type = "risk_score_update",
                    Title = "AI Risk Score Updated",
                    Message = $"Risk score updated for {riskAssessment.CustomerName}: {riskAssessment.RiskScore:F1} ({riskLevel})",
                    Data = new
                    {
                        customerId,
                        riskScore = riskAssessment.RiskScore,
                        riskLevel,
                        riskChange,
                        confidenceLevel = riskAssessment.ConfidenceLevel,
                        riskTrend = riskAssessment.RiskTrend,
                        recommendedActions = riskAssessment.RecommendedActions.Take(3).Select(a => a.Action)
                    },
                    Timestamp = DateTime.UtcNow,
                    Priority = riskLevel == "High" || riskLevel == "Critical" ? "High" : "Medium",
                    Category = "RiskAssessment",
                    ActionRequired = riskAssessment.RecommendedActions.Any(a => a.Priority == "Critical")
                };

                // Send to customer-specific group
                await _hubContext.Clients.Group($"customer_{customerId}")
                    .SendAsync("ReceiveNotification", notification);

                // Send to compliance users if high risk
                if (riskAssessment.RiskScore >= 70)
                {
                    await _hubContext.Clients.Group("compliance_users")
                        .SendAsync("ReceiveNotification", notification);
                }

                await StoreNotificationAsync(notification);

                _logger.LogInformation("Sent risk score update notification for customer {CustomerId}: {RiskScore}", 
                    customerId, riskAssessment.RiskScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending risk score update notification for customer {CustomerId}", customerId);
            }
        }

        public async Task SendSystemNotificationAsync(string message, string type, string? userId = null)
        {
            try
            {
                var notification = new RealTimeNotification
                {
                    Id = Guid.NewGuid(),
                    Type = type,
                    Title = "System Notification",
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    Priority = "Medium",
                    Category = "System"
                };

                if (!string.IsNullOrEmpty(userId))
                {
                    await _hubContext.Clients.Group($"user_{userId}")
                        .SendAsync("ReceiveNotification", notification);
                }
                else
                {
                    await _hubContext.Clients.All
                        .SendAsync("ReceiveNotification", notification);
                }

                await StoreNotificationAsync(notification);

                _logger.LogInformation("Sent system notification: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending system notification: {Message}", message);
            }
        }

        public async Task SendBulkScanProgressAsync(string scanId, BulkScanProgress progress)
        {
            try
            {
                var notification = new RealTimeNotification
                {
                    Id = Guid.NewGuid(),
                    Type = "bulk_scan_progress",
                    Title = "Bulk Scan Progress",
                    Message = $"Bulk scan progress: {progress.CompletedCount}/{progress.TotalCount} customers processed",
                    Data = new
                    {
                        scanId,
                        progress.TotalCount,
                        progress.CompletedCount,
                        progress.SuccessCount,
                        progress.FailedCount,
                        progress.PercentComplete,
                        progress.EstimatedTimeRemaining,
                        progress.CurrentCustomer
                    },
                    Timestamp = DateTime.UtcNow,
                    Priority = "Low",
                    Category = "BulkScan"
                };

                // Send to bulk scan group
                await _hubContext.Clients.Group($"bulk_scan_{scanId}")
                    .SendAsync("ReceiveNotification", notification);

                // Don't store progress notifications to avoid clutter
                _logger.LogDebug("Sent bulk scan progress notification: {Progress}%", progress.PercentComplete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk scan progress notification for scan {ScanId}", scanId);
            }
        }

        public async Task JoinCustomerGroupAsync(string connectionId, Guid customerId)
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, $"customer_{customerId}");
            _logger.LogDebug("Connection {ConnectionId} joined customer group {CustomerId}", connectionId, customerId);
        }

        public async Task LeaveCustomerGroupAsync(string connectionId, Guid customerId)
        {
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, $"customer_{customerId}");
            _logger.LogDebug("Connection {ConnectionId} left customer group {CustomerId}", connectionId, customerId);
        }

        public async Task JoinUserGroupAsync(string connectionId, string userId)
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, $"user_{userId}");
            await _hubContext.Groups.AddToGroupAsync(connectionId, "compliance_users");
            _logger.LogDebug("Connection {ConnectionId} joined user group {UserId}", connectionId, userId);
        }

        public async Task LeaveUserGroupAsync(string connectionId, string userId)
        {
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, $"user_{userId}");
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, "compliance_users");
            _logger.LogDebug("Connection {ConnectionId} left user group {UserId}", connectionId, userId);
        }

        private async Task StoreNotificationAsync(RealTimeNotification notification)
        {
            try
            {
                var entity = new NotificationEntity
                {
                    Id = notification.Id,
                    Type = notification.Type,
                    Title = notification.Title,
                    Message = notification.Message,
                    DataJson = JsonSerializer.Serialize(notification.Data),
                    Timestamp = notification.Timestamp,
                    Priority = notification.Priority,
                    Category = notification.Category,
                    ActionRequired = notification.ActionRequired,
                    ActionUrl = notification.ActionUrl,
                    IsRead = false
                };

                _context.Notifications.Add(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing notification {NotificationId}", notification.Id);
            }
        }

        private async Task<AIRiskAssessment?> GetPreviousRiskAssessmentAsync(Guid customerId)
        {
            var previous = await _context.RiskAssessments
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CalculatedAt)
                .Skip(1)
                .FirstOrDefaultAsync();

            return previous != null ? new AIRiskAssessment
            {
                RiskScore = previous.RiskScore,
                ConfidenceLevel = previous.ConfidenceLevel
            } : null;
        }

        private string GetRiskLevel(double riskScore)
        {
            return riskScore switch
            {
                >= 90 => "Critical",
                >= 75 => "High",
                >= 50 => "Medium",
                >= 25 => "Low",
                _ => "Minimal"
            };
        }
    }

    // SignalR Hub
    public class NotificationHub : Hub
    {
        private readonly IRealTimeNotificationService _notificationService;
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(
            IRealTimeNotificationService notificationService,
            ILogger<NotificationHub> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task JoinCustomerGroup(string customerId)
        {
            if (Guid.TryParse(customerId, out var customerGuid))
            {
                await _notificationService.JoinCustomerGroupAsync(Context.ConnectionId, customerGuid);
            }
        }

        public async Task LeaveCustomerGroup(string customerId)
        {
            if (Guid.TryParse(customerId, out var customerGuid))
            {
                await _notificationService.LeaveCustomerGroupAsync(Context.ConnectionId, customerGuid);
            }
        }

        public async Task JoinUserGroup(string userId)
        {
            await _notificationService.JoinUserGroupAsync(Context.ConnectionId, userId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogDebug("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }

    // DTOs
    public class RealTimeNotification
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public DateTime Timestamp { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool ActionRequired { get; set; }
        public string? ActionUrl { get; set; }
    }

    public class BulkScanProgress
    {
        public int TotalCount { get; set; }
        public int CompletedCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public double PercentComplete => TotalCount > 0 ? (double)CompletedCount / TotalCount * 100 : 0;
        public TimeSpan? EstimatedTimeRemaining { get; set; }
        public string? CurrentCustomer { get; set; }
    }
}
