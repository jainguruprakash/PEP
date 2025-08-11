using Microsoft.EntityFrameworkCore;
using PEPScanner.Domain.Entities;
using PEPScanner.Infrastructure.Data;

namespace PEPScanner.API.Data
{
    public static class ComplianceHierarchySeedData
    {
        public static async Task SeedComplianceHierarchyAsync(PepScannerDbContext context)
        {
            // Check if hierarchy data already exists
            if (await context.Teams.AnyAsync())
            {
                return; // Already seeded
            }

            // Get the test organization
            var organization = await context.Organizations.FirstOrDefaultAsync();
            if (organization == null)
            {
                return; // No organization to seed data for
            }

            // Create teams
            var complianceTeamA = new Team
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                Name = "Compliance Team A",
                Department = "Compliance",
                Territory = "North",
                ProductType = "Corporate",
                IsActive = true,
                MaxWorkload = 50,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "System"
            };

            var complianceTeamB = new Team
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                Name = "Compliance Team B",
                Department = "Compliance",
                Territory = "South",
                ProductType = "Retail",
                IsActive = true,
                MaxWorkload = 50,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "System"
            };

            var riskTeam = new Team
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                Name = "Risk Management Team",
                Department = "Risk",
                Territory = "All",
                ProductType = "All",
                IsActive = true,
                MaxWorkload = 30,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "System"
            };

            context.Teams.AddRange(complianceTeamA, complianceTeamB, riskTeam);
            await context.SaveChangesAsync();

            // Create users with hierarchy
            var manager = new OrganizationUser
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                Username = "manager",
                Email = "manager@test.com",
                FirstName = "John",
                LastName = "Manager",
                FullName = "John Manager",
                Role = "Manager",
                Department = "Compliance",
                Position = "Compliance Manager",
                IsActive = true,
                EscalationLevel = 2,
                MaxWorkload = 30,
                CurrentWorkload = 0,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "System"
            };

            var teamLeadA = new OrganizationUser
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                Username = "teamlead_a",
                Email = "teamlead.a@test.com",
                FirstName = "Sarah",
                LastName = "TeamLead",
                FullName = "Sarah TeamLead",
                Role = "ComplianceOfficer",
                Department = "Compliance",
                Position = "Senior Compliance Officer",
                IsActive = true,
                EscalationLevel = 1,
                MaxWorkload = 25,
                CurrentWorkload = 0,
                Territory = "North",
                TeamId = complianceTeamA.Id,
                ManagerId = manager.Id,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("teamlead123"),
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "System"
            };

            var teamLeadB = new OrganizationUser
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                Username = "teamlead_b",
                Email = "teamlead.b@test.com",
                FirstName = "Mike",
                LastName = "TeamLead",
                FullName = "Mike TeamLead",
                Role = "ComplianceOfficer",
                Department = "Compliance",
                Position = "Senior Compliance Officer",
                IsActive = true,
                EscalationLevel = 1,
                MaxWorkload = 25,
                CurrentWorkload = 0,
                Territory = "South",
                TeamId = complianceTeamB.Id,
                ManagerId = manager.Id,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("teamlead123"),
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "System"
            };

            var analyst1 = new OrganizationUser
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                Username = "analyst1",
                Email = "analyst1@test.com",
                FirstName = "Alice",
                LastName = "Analyst",
                FullName = "Alice Analyst",
                Role = "Analyst",
                Department = "Compliance",
                Position = "Compliance Analyst",
                IsActive = true,
                EscalationLevel = 0,
                MaxWorkload = 20,
                CurrentWorkload = 0,
                Territory = "North",
                TeamId = complianceTeamA.Id,
                ManagerId = teamLeadA.Id,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("analyst123"),
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "System"
            };

            var analyst2 = new OrganizationUser
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                Username = "analyst2",
                Email = "analyst2@test.com",
                FirstName = "Bob",
                LastName = "Analyst",
                FullName = "Bob Analyst",
                Role = "Analyst",
                Department = "Compliance",
                Position = "Compliance Analyst",
                IsActive = true,
                EscalationLevel = 0,
                MaxWorkload = 20,
                CurrentWorkload = 0,
                Territory = "South",
                TeamId = complianceTeamB.Id,
                ManagerId = teamLeadB.Id,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("analyst123"),
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "System"
            };

            var riskHead = new OrganizationUser
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                Username = "riskhead",
                Email = "riskhead@test.com",
                FirstName = "David",
                LastName = "RiskHead",
                FullName = "David RiskHead",
                Role = "RiskHead",
                Department = "Risk",
                Position = "Head of Risk",
                IsActive = true,
                EscalationLevel = 3,
                MaxWorkload = 15,
                CurrentWorkload = 0,
                TeamId = riskTeam.Id,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("riskhead123"),
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "System"
            };

            context.OrganizationUsers.AddRange(manager, teamLeadA, teamLeadB, analyst1, analyst2, riskHead);
            await context.SaveChangesAsync();

            // Update team leads
            complianceTeamA.TeamLeadId = teamLeadA.Id;
            complianceTeamB.TeamLeadId = teamLeadB.Id;
            riskTeam.TeamLeadId = riskHead.Id;
            await context.SaveChangesAsync();

            // Create notification rules
            var notificationRules = new List<NotificationRule>
            {
                new NotificationRule
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organization.Id,
                    AlertType = "PEP",
                    RiskLevel = "Critical",
                    NotifyAssignee = true,
                    NotifyTeamLead = true,
                    NotifyManager = true,
                    NotifyRiskTeam = false,
                    Level1EscalationHours = 4,
                    Level2EscalationHours = 8,
                    Level3EscalationHours = 12,
                    AutoEscalateOnSLA = true,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new NotificationRule
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organization.Id,
                    AlertType = "PEP",
                    RiskLevel = "High",
                    NotifyAssignee = true,
                    NotifyTeamLead = true,
                    NotifyManager = false,
                    NotifyRiskTeam = false,
                    Level1EscalationHours = 12,
                    Level2EscalationHours = 24,
                    Level3EscalationHours = 48,
                    AutoEscalateOnSLA = true,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new NotificationRule
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organization.Id,
                    AlertType = "Sanctions",
                    RiskLevel = "Critical",
                    NotifyAssignee = true,
                    NotifyTeamLead = true,
                    NotifyManager = true,
                    NotifyRiskTeam = true,
                    Level1EscalationHours = 2,
                    Level2EscalationHours = 4,
                    Level3EscalationHours = 6,
                    AutoEscalateOnSLA = true,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            };

            context.NotificationRules.AddRange(notificationRules);
            await context.SaveChangesAsync();
        }
    }
}