using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Data
{
    public static class DashboardSeedData
    {
        public static async Task SeedDashboardDataAsync(PepScannerDbContext context)
        {
            try
            {
                // Check if we already have data
                if (await context.Customers.AnyAsync() && await context.Alerts.AnyAsync())
                {
                    return; // Data already exists
                }

                // Create sample organization if not exists
                var organization = await context.Organizations.FirstOrDefaultAsync();
                if (organization == null)
                {
                    organization = new Organization
                    {
                        Id = Guid.NewGuid(),
                        Name = "Demo Bank Ltd",
                        Type = "Bank",
                        Country = "India",
                        City = "Mumbai",
                        IsActive = true,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    context.Organizations.Add(organization);
                    await context.SaveChangesAsync();
                }

                // Create sample customers
                var customers = new List<Customer>();
                var customerNames = new[]
                {
                    ("Amit", "Shah", "amit.shah@email.com", "High"),
                    ("Priya", "Sharma", "priya.sharma@email.com", "Medium"),
                    ("Rajesh", "Kumar", "rajesh.kumar@email.com", "Low"),
                    ("Sunita", "Patel", "sunita.patel@email.com", "High"),
                    ("Vikram", "Singh", "vikram.singh@email.com", "Medium"),
                    ("Anita", "Gupta", "anita.gupta@email.com", "Low"),
                    ("Rohit", "Verma", "rohit.verma@email.com", "Critical"),
                    ("Kavita", "Jain", "kavita.jain@email.com", "Medium"),
                    ("Suresh", "Reddy", "suresh.reddy@email.com", "High"),
                    ("Meera", "Nair", "meera.nair@email.com", "Low")
                };

                foreach (var (firstName, lastName, email, riskLevel) in customerNames)
                {
                    var customer = new Customer
                    {
                        Id = Guid.NewGuid(),
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        Phone = $"+91-{new Random().Next(7000000000, 9999999999)}",
                        DateOfBirth = DateTime.Now.AddYears(-new Random().Next(25, 65)),
                        Nationality = "Indian",
                        Address = $"{new Random().Next(1, 999)} Sample Street",
                        City = "Mumbai",
                        State = "Maharashtra",
                        Country = "India",
                        PostalCode = $"{new Random().Next(400000, 499999)}",
                        CustomerType = "Individual",
                        RiskLevel = riskLevel,
                        Status = "Active",
                        OnboardingDate = DateTime.UtcNow.AddDays(-new Random().Next(1, 90)),
                        CreatedAtUtc = DateTime.UtcNow.AddDays(-new Random().Next(1, 90)),
                        UpdatedAtUtc = DateTime.UtcNow
                    };
                    customers.Add(customer);
                }

                context.Customers.AddRange(customers);
                await context.SaveChangesAsync();

                // Create sample alerts
                var alerts = new List<Alert>();
                var alertTypes = new[] { "PEP", "Sanctions", "Adverse Media", "Name Similarity" };
                var priorities = new[] { "Low", "Medium", "High", "Critical" };
                var statuses = new[] { "PendingReview", "UnderReview", "Approved", "Rejected" };

                foreach (var customer in customers)
                {
                    // Create 1-3 alerts per customer
                    var alertCount = new Random().Next(1, 4);
                    for (int i = 0; i < alertCount; i++)
                    {
                        var createdDate = DateTime.UtcNow.AddDays(-new Random().Next(1, 30));
                        var alert = new Alert
                        {
                            Id = Guid.NewGuid(),
                            CustomerId = customer.Id,
                            Context = "Onboarding",
                            AlertType = alertTypes[new Random().Next(alertTypes.Length)],
                            SimilarityScore = Math.Round(new Random().NextDouble() * 0.5 + 0.5, 2),
                            Status = "Open",
                            Priority = priorities[new Random().Next(priorities.Length)],
                            WorkflowStatus = statuses[new Random().Next(statuses.Length)],
                            SourceList = "Demo Watchlist",
                            SourceCategory = "Demo",
                            CreatedBy = "System",
                            CreatedAtUtc = createdDate,
                            UpdatedAtUtc = createdDate
                        };

                        // Set review/approval data based on status
                        if (alert.WorkflowStatus == "UnderReview" || alert.WorkflowStatus == "Approved" || alert.WorkflowStatus == "Rejected")
                        {
                            alert.ReviewedBy = "analyst@demo.com";
                            alert.ReviewedAtUtc = createdDate.AddHours(new Random().Next(1, 24));
                        }

                        if (alert.WorkflowStatus == "Approved")
                        {
                            alert.ApprovedBy = "manager@demo.com";
                            alert.ApprovedAtUtc = alert.ReviewedAtUtc?.AddHours(new Random().Next(1, 12));
                            alert.Outcome = "Confirmed";
                        }
                        else if (alert.WorkflowStatus == "Rejected")
                        {
                            alert.RejectedBy = "manager@demo.com";
                            alert.RejectedAtUtc = alert.ReviewedAtUtc?.AddHours(new Random().Next(1, 12));
                            alert.Outcome = "FalsePositive";
                        }

                        alerts.Add(alert);
                    }
                }

                context.Alerts.AddRange(alerts);
                await context.SaveChangesAsync();

                // Create sample notifications
                var notifications = new List<Notification>();
                foreach (var alert in alerts.Take(20)) // Create notifications for first 20 alerts
                {
                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        Type = "Alert",
                        Title = $"New {alert.AlertType} Alert",
                        Message = $"Alert created with {alert.Priority} priority",
                        TargetUserRole = alert.Priority == "Critical" ? "Manager" : "Analyst",
                        IsRead = new Random().NextDouble() > 0.3, // 70% chance of being unread
                        CreatedAtUtc = alert.CreatedAtUtc
                    };
                    notifications.Add(notification);
                }

                context.Notifications.AddRange(notifications);
                await context.SaveChangesAsync();

                Console.WriteLine($"Dashboard seed data created: {customers.Count} customers, {alerts.Count} alerts, {notifications.Count} notifications");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding dashboard data: {ex.Message}");
            }
        }
    }
}