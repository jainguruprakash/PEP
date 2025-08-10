using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace PEPScanner.API.Data
{
    public static class SeedData
    {
        public static async Task SeedUsersAsync(PepScannerDbContext context)
        {
            // Check if users already exist
            if (await context.Users.AnyAsync())
            {
                return; // Users already seeded
            }

            var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "analyst@pepscanner.com",
                    FirstName = "John",
                    LastName = "Analyst",
                    Role = "Analyst",
                    Department = "Compliance",
                    JobTitle = "Junior Compliance Analyst",
                    IsActive = true,
                    CanCreateAlerts = true,
                    CanReviewAlerts = true,
                    CanApproveAlerts = false,
                    CanEscalateAlerts = false,
                    CanCloseAlerts = false,
                    CanViewAllAlerts = false,
                    CanAssignAlerts = false,
                    CanGenerateReports = false,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "compliance.officer@pepscanner.com",
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    Role = "ComplianceOfficer",
                    Department = "Compliance",
                    JobTitle = "Senior Compliance Officer",
                    IsActive = true,
                    CanCreateAlerts = true,
                    CanReviewAlerts = true,
                    CanApproveAlerts = true,
                    CanEscalateAlerts = false,
                    CanCloseAlerts = true,
                    CanViewAllAlerts = true,
                    CanAssignAlerts = true,
                    CanGenerateReports = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "manager@pepscanner.com",
                    FirstName = "Michael",
                    LastName = "Manager",
                    Role = "Manager",
                    Department = "Compliance",
                    JobTitle = "Compliance Manager",
                    IsActive = true,
                    CanCreateAlerts = true,
                    CanReviewAlerts = true,
                    CanApproveAlerts = true,
                    CanEscalateAlerts = true,
                    CanCloseAlerts = true,
                    CanViewAllAlerts = true,
                    CanAssignAlerts = true,
                    CanGenerateReports = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@pepscanner.com",
                    FirstName = "Admin",
                    LastName = "User",
                    Role = "Admin",
                    Department = "IT",
                    JobTitle = "System Administrator",
                    IsActive = true,
                    CanCreateAlerts = true,
                    CanReviewAlerts = true,
                    CanApproveAlerts = true,
                    CanEscalateAlerts = true,
                    CanCloseAlerts = true,
                    CanViewAllAlerts = true,
                    CanAssignAlerts = true,
                    CanGenerateReports = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            };

            context.Users.AddRange(users);
            await context.SaveChangesAsync();
        }

        public static async Task SeedSampleAlertsAsync(PepScannerDbContext context)
        {
            // Check if alerts already exist
            if (await context.Alerts.AnyAsync())
            {
                return; // Alerts already seeded
            }

            // Get some users for assignment
            var analyst = await context.Users.FirstOrDefaultAsync(u => u.Role == "Analyst");
            var complianceOfficer = await context.Users.FirstOrDefaultAsync(u => u.Role == "ComplianceOfficer");

            if (analyst == null || complianceOfficer == null)
            {
                return; // Users not seeded yet
            }

            var alerts = new List<Alert>
            {
                new Alert
                {
                    Id = Guid.NewGuid(),
                    AlertType = "PEP",
                    Context = "Onboarding",
                    SimilarityScore = 0.95,
                    MatchAlgorithm = "Levenshtein",
                    Status = "Open",
                    WorkflowStatus = "PendingReview",
                    Priority = "High",
                    RiskLevel = "High",
                    AssignedTo = analyst.Email,
                    CurrentReviewer = analyst.Email,
                    SourceList = "OFAC",
                    SourceCategory = "PEP",
                    MatchingDetails = "High confidence match on primary name",
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-2),
                    CreatedBy = "System",
                    DueDate = DateTime.UtcNow.AddDays(1),
                    SlaStatus = "OnTime"
                },
                new Alert
                {
                    Id = Guid.NewGuid(),
                    AlertType = "Sanctions",
                    Context = "Transaction",
                    SimilarityScore = 0.88,
                    MatchAlgorithm = "JaroWinkler",
                    Status = "UnderReview",
                    WorkflowStatus = "PendingApproval",
                    Priority = "Critical",
                    RiskLevel = "Critical",
                    AssignedTo = complianceOfficer.Email,
                    CurrentReviewer = complianceOfficer.Email,
                    ReviewedBy = analyst.Email,
                    ReviewedAtUtc = DateTime.UtcNow.AddHours(-2),
                    SourceList = "UN",
                    SourceCategory = "Sanctions",
                    MatchingDetails = "Partial name match with high confidence",
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
                    CreatedBy = "System",
                    DueDate = DateTime.UtcNow.AddHours(2),
                    SlaStatus = "OnTime"
                },
                new Alert
                {
                    Id = Guid.NewGuid(),
                    AlertType = "PEP",
                    Context = "Periodic",
                    SimilarityScore = 0.75,
                    MatchAlgorithm = "Soundex",
                    Status = "Open",
                    WorkflowStatus = "PendingReview",
                    Priority = "Medium",
                    RiskLevel = "Medium",
                    AssignedTo = analyst.Email,
                    CurrentReviewer = analyst.Email,
                    SourceList = "RBI",
                    SourceCategory = "PEP",
                    MatchingDetails = "Phonetic similarity match",
                    CreatedAtUtc = DateTime.UtcNow.AddHours(-6),
                    CreatedBy = "System",
                    DueDate = DateTime.UtcNow.AddDays(2),
                    SlaStatus = "OnTime"
                }
            };

            context.Alerts.AddRange(alerts);
            await context.SaveChangesAsync();

            // Create some sample alert actions
            var alertActions = new List<AlertAction>();
            foreach (var alert in alerts)
            {
                alertActions.Add(new AlertAction
                {
                    Id = Guid.NewGuid(),
                    AlertId = alert.Id,
                    ActionType = "Created",
                    PerformedBy = "System",
                    NewStatus = alert.WorkflowStatus,
                    Comments = "Alert automatically created by screening system",
                    ActionDateUtc = alert.CreatedAtUtc
                });

                if (alert.ReviewedBy != null)
                {
                    alertActions.Add(new AlertAction
                    {
                        Id = Guid.NewGuid(),
                        AlertId = alert.Id,
                        ActionType = "Reviewed",
                        PerformedBy = alert.ReviewedBy,
                        PreviousStatus = "PendingReview",
                        NewStatus = "PendingApproval",
                        Comments = "Initial review completed, escalating for approval",
                        ActionDateUtc = alert.ReviewedAtUtc ?? DateTime.UtcNow
                    });
                }
            }

            context.AlertActions.AddRange(alertActions);
            await context.SaveChangesAsync();
        }
    }
}
