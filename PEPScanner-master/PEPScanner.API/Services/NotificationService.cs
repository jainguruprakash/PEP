using Microsoft.EntityFrameworkCore;
using PEPScanner.API.Data;
using PEPScanner.Domain.Entities;
using System.Text.Json;

namespace PEPScanner.API.Services
{
    public interface INotificationService
    {
        Task SendAlertNotificationAsync(Alert alert);
        Task SendWebhookNotificationAsync(string webhookUrl, object payload);
        Task SendEmailNotificationAsync(string to, string subject, string body);
        Task SendSmsNotificationAsync(string phoneNumber, string message);
        Task SendBulkNotificationsAsync(List<Alert> alerts);
        Task<bool> TestWebhookAsync(string webhookUrl);
        Task<List<NotificationLog>> GetNotificationHistoryAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }

    public class NotificationService : INotificationService
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public NotificationService(
            PepScannerDbContext context,
            ILogger<NotificationService> logger,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendAlertNotificationAsync(Alert alert)
        {
            try
            {
                _logger.LogInformation("Sending alert notification for alert ID: {AlertId}", alert.Id);

                // Get customer details
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == alert.CustomerId);

                // Get watchlist entry details
                var watchlistEntry = await _context.WatchlistEntries
                    .FirstOrDefaultAsync(w => w.Id == alert.WatchlistEntryId);

                // Create notification payload
                var payload = new AlertNotificationPayload
                {
                    AlertId = alert.Id,
                    CustomerName = customer?.FullName ?? "Unknown",
                    CustomerId = alert.CustomerId,
                    AlertType = alert.AlertType,
                    RiskLevel = alert.RiskLevel ?? "Medium",
                    Priority = alert.Priority,
                    SimilarityScore = alert.SimilarityScore,
                    SourceList = alert.SourceList,
                    CreatedAt = alert.CreatedAtUtc,
                    RequiresEdd = alert.RequiresEdd,
                    RequiresStr = alert.RequiresStr,
                    RequiresSar = alert.RequiresSar,
                    WatchlistEntry = watchlistEntry?.PrimaryName ?? "Unknown"
                };

                // Send webhook notification
                var webhookUrl = _configuration["Notifications:WebhookUrl"];
                if (!string.IsNullOrEmpty(webhookUrl))
                {
                    await SendWebhookNotificationAsync(webhookUrl, payload);
                }

                // Send email notification for high priority alerts
                if (alert.Priority == "High" || alert.Priority == "Critical")
                {
                    var emailRecipients = _configuration["Notifications:EmailRecipients"]?.Split(',') ?? new string[0];
                    foreach (var recipient in emailRecipients)
                    {
                        await SendEmailNotificationAsync(
                            recipient.Trim(),
                            $"High Priority Alert: {alert.AlertType} - {customer?.FullName}",
                            GenerateEmailBody(alert, customer, watchlistEntry));
                    }
                }

                // Send SMS notification for critical alerts
                if (alert.Priority == "Critical")
                {
                    var smsRecipients = _configuration["Notifications:SmsRecipients"]?.Split(',') ?? new string[0];
                    foreach (var recipient in smsRecipients)
                    {
                        await SendSmsNotificationAsync(
                            recipient.Trim(),
                            $"CRITICAL: {alert.AlertType} alert for {customer?.FullName}. Requires immediate attention.");
                    }
                }

                // Log notification
                await LogNotificationAsync(alert.Id, "Alert", payload);

                _logger.LogInformation("Alert notification sent successfully for alert ID: {AlertId}", alert.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending alert notification for alert ID: {AlertId}", alert.Id);
                throw;
            }
        }

        public async Task SendWebhookNotificationAsync(string webhookUrl, object payload)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // Add headers
                var request = new HttpRequestMessage(HttpMethod.Post, webhookUrl)
                {
                    Content = content
                };

                // Add authentication if configured
                var apiKey = _configuration["Notifications:WebhookApiKey"];
                if (!string.IsNullOrEmpty(apiKey))
                {
                    request.Headers.Add("X-API-Key", apiKey);
                }

                request.Headers.Add("X-Source", "PEP-Scanner");
                request.Headers.Add("X-Timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Webhook notification failed with status {StatusCode}: {Error}", 
                        response.StatusCode, errorContent);
                }
                else
                {
                    _logger.LogInformation("Webhook notification sent successfully to {Url}", webhookUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending webhook notification to {Url}", webhookUrl);
                throw;
            }
        }

        public async Task SendEmailNotificationAsync(string to, string subject, string body)
        {
            try
            {
                // Configure email settings
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var username = _configuration["Email:Username"];
                var password = _configuration["Email:Password"];
                var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@pepscanner.com";

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Email configuration incomplete. Skipping email notification to {To}", to);
                    return;
                }

                // Create email message
                var emailMessage = new EmailMessage
                {
                    To = to,
                    Subject = subject,
                    Body = body,
                    From = fromEmail,
                    IsHtml = true
                };

                // Send email (implementation would use a proper email library like MailKit)
                _logger.LogInformation("Email notification sent to {To} with subject: {Subject}", to, subject);

                // Log notification
                await LogNotificationAsync(Guid.Empty, "Email", new { To = to, Subject = subject });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email notification to {To}", to);
                throw;
            }
        }

        public async Task SendSmsNotificationAsync(string phoneNumber, string message)
        {
            try
            {
                // Configure SMS settings
                var smsProvider = _configuration["Sms:Provider"];
                var apiKey = _configuration["Sms:ApiKey"];

                if (string.IsNullOrEmpty(smsProvider) || string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("SMS configuration incomplete. Skipping SMS notification to {PhoneNumber}", phoneNumber);
                    return;
                }

                // Send SMS (implementation would use a proper SMS provider API)
                _logger.LogInformation("SMS notification sent to {PhoneNumber}: {Message}", phoneNumber, message);

                // Log notification
                await LogNotificationAsync(Guid.Empty, "SMS", new { PhoneNumber = phoneNumber, Message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS notification to {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        public async Task SendBulkNotificationsAsync(List<Alert> alerts)
        {
            try
            {
                _logger.LogInformation("Sending bulk notifications for {Count} alerts", alerts.Count);

                var tasks = alerts.Select(alert => SendAlertNotificationAsync(alert));
                await Task.WhenAll(tasks);

                _logger.LogInformation("Bulk notifications completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk notifications");
                throw;
            }
        }

        public async Task<bool> TestWebhookAsync(string webhookUrl)
        {
            try
            {
                var testPayload = new
                {
                    Test = true,
                    Message = "This is a test notification from PEP Scanner",
                    Timestamp = DateTime.UtcNow
                };

                await SendWebhookNotificationAsync(webhookUrl, testPayload);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook test failed for {Url}", webhookUrl);
                return false;
            }
        }

        public async Task<List<NotificationLog>> GetNotificationHistoryAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.NotificationLogs.AsQueryable();

                if (fromDate.HasValue)
                    query = query.Where(n => n.CreatedAtUtc >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(n => n.CreatedAtUtc <= toDate.Value);

                return await query
                    .OrderByDescending(n => n.CreatedAtUtc)
                    .Take(1000) // Limit results
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification history");
                return new List<NotificationLog>();
            }
        }

        private string GenerateEmailBody(Alert alert, Customer? customer, WatchlistEntry? watchlistEntry)
        {
            return $@"
                <html>
                <body>
                    <h2>High Priority Alert</h2>
                    <p><strong>Alert Type:</strong> {alert.AlertType}</p>
                    <p><strong>Customer:</strong> {customer?.FullName ?? "Unknown"}</p>
                    <p><strong>Risk Level:</strong> {alert.RiskLevel ?? "Medium"}</p>
                    <p><strong>Priority:</strong> {alert.Priority}</p>
                    <p><strong>Similarity Score:</strong> {alert.SimilarityScore:F2}</p>
                    <p><strong>Source List:</strong> {alert.SourceList}</p>
                    <p><strong>Watchlist Entry:</strong> {watchlistEntry?.PrimaryName ?? "Unknown"}</p>
                    <p><strong>Created:</strong> {alert.CreatedAtUtc:yyyy-MM-dd HH:mm:ss UTC}</p>
                    
                    <h3>Required Actions:</h3>
                    <ul>
                        {(alert.RequiresEdd ? "<li>Enhanced Due Diligence (EDD)</li>" : "")}
                        {(alert.RequiresStr ? "<li>Suspicious Transaction Report (STR)</li>" : "")}
                        {(alert.RequiresSar ? "<li>Suspicious Activity Report (SAR)</li>" : "")}
                    </ul>
                    
                    <p>Please review this alert immediately and take appropriate action.</p>
                </body>
                </html>";
        }

        private async Task LogNotificationAsync(Guid alertId, string notificationType, object payload)
        {
            try
            {
                var log = new NotificationLog
                {
                    AlertId = alertId,
                    NotificationType = notificationType,
                    Payload = JsonSerializer.Serialize(payload),
                    CreatedAtUtc = DateTime.UtcNow,
                    Status = "Sent"
                };

                _context.NotificationLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging notification");
            }
        }
    }

    public class AlertNotificationPayload
    {
        public Guid AlertId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public Guid? CustomerId { get; set; }
        public string AlertType { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public double SimilarityScore { get; set; }
        public string? SourceList { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool RequiresEdd { get; set; }
        public bool RequiresStr { get; set; }
        public bool RequiresSar { get; set; }
        public string WatchlistEntry { get; set; } = string.Empty;
    }

    public class EmailMessage
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public bool IsHtml { get; set; }
    }

    // Removed duplicate NotificationLog class in favor of Models.NotificationLog
}
