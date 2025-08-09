using Microsoft.EntityFrameworkCore;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Data
{
    public static class SeedData
    {
        public static async Task SeedAllDataAsync(PepScannerDbContext context)
        {
            if (!context.Organizations.Any())
            {
                await SeedOrganizationsAsync(context);
            }

            if (!context.OrganizationUsers.Any())
            {
                await SeedOrganizationUsersAsync(context);
            }

            if (!context.Customers.Any())
            {
                await SeedCustomersAsync(context);
            }

            if (!context.WatchlistEntries.Any())
            {
                await SeedWatchlistEntriesAsync(context);
            }

            if (!context.OrganizationWatchlists.Any())
            {
                await SeedOrganizationWatchlistsAsync(context);
            }

            if (!context.OrganizationConfigurations.Any())
            {
                await SeedOrganizationConfigurationsAsync(context);
            }
        }

        private static async Task SeedOrganizationsAsync(PepScannerDbContext context)
        {
            var organizations = new[]
            {
                new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = "State Bank of India",
                    Code = "SBI001",
                    Description = "Largest public sector bank in India",
                    Type = "Bank",
                    Industry = "Banking",
                    Country = "India",
                    State = "Maharashtra",
                    City = "Mumbai",
                    Address = "State Bank Bhavan, Madame Cama Road",
                    PostalCode = "400021",
                    ContactPerson = "Rajesh Kumar",
                    ContactEmail = "compliance@sbi.co.in",
                    ContactPhone = "+91-22-22021234",
                    Website = "https://www.sbi.co.in",
                    LicenseNumber = "SBI001",
                    RegulatoryBody = "RBI",
                    IsActive = true,
                    SubscriptionPlan = "Enterprise",
                    MaxUsers = 100,
                    MaxCustomers = 1000000,
                    IsTrial = false,
                    TimeZone = "Asia/Kolkata",
                    Currency = "INR",
                    Language = "en",
                    CreatedBy = "System"
                },
                new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = "HDFC Bank",
                    Code = "HDFC001",
                    Description = "Leading private sector bank in India",
                    Type = "Bank",
                    Industry = "Banking",
                    Country = "India",
                    State = "Maharashtra",
                    City = "Mumbai",
                    Address = "HDFC Bank House, H.T. Parekh Marg",
                    PostalCode = "400001",
                    ContactPerson = "Priya Sharma",
                    ContactEmail = "aml@hdfcbank.com",
                    ContactPhone = "+91-22-66521000",
                    Website = "https://www.hdfcbank.com",
                    LicenseNumber = "HDFC001",
                    RegulatoryBody = "RBI",
                    IsActive = true,
                    SubscriptionPlan = "Professional",
                    MaxUsers = 50,
                    MaxCustomers = 500000,
                    IsTrial = false,
                    TimeZone = "Asia/Kolkata",
                    Currency = "INR",
                    Language = "en",
                    CreatedBy = "System"
                },
                new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = "ICICI Bank",
                    Code = "ICICI001",
                    Description = "Major private sector bank in India",
                    Type = "Bank",
                    Industry = "Banking",
                    Country = "India",
                    State = "Maharashtra",
                    City = "Mumbai",
                    Address = "ICICI Bank Tower, Bandra Kurla Complex",
                    PostalCode = "400051",
                    ContactPerson = "Amit Patel",
                    ContactEmail = "compliance@icicibank.com",
                    ContactPhone = "+91-22-24937777",
                    Website = "https://www.icicibank.com",
                    LicenseNumber = "ICICI001",
                    RegulatoryBody = "RBI",
                    IsActive = true,
                    SubscriptionPlan = "Professional",
                    MaxUsers = 75,
                    MaxCustomers = 750000,
                    IsTrial = false,
                    TimeZone = "Asia/Kolkata",
                    Currency = "INR",
                    Language = "en",
                    CreatedBy = "System"
                }
            };

            context.Organizations.AddRange(organizations);
            await context.SaveChangesAsync();
        }

        private static async Task SeedOrganizationUsersAsync(PepScannerDbContext context)
        {
            var organizations = await context.Organizations.ToListAsync();
            
            var users = new List<OrganizationUser>();
            
            foreach (var org in organizations)
            {
                users.AddRange(new[]
                {
                    new OrganizationUser
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        Username = $"admin_{org.Code.ToLower()}",
                        Email = $"admin@{org.Code.ToLower()}.com",
                        FirstName = "Admin",
                        LastName = "User",
                        FullName = "Admin User",
                        Role = "Admin",
                        Department = "IT",
                        Position = "System Administrator",
                        PhoneNumber = "+91-9876543210",
                        IsActive = true,
                        IsEmailVerified = true,
                        TimeZone = "Asia/Kolkata",
                        Language = "en",
                        CreatedBy = "System"
                    },
                    new OrganizationUser
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        Username = $"compliance_{org.Code.ToLower()}",
                        Email = $"compliance@{org.Code.ToLower()}.com",
                        FirstName = "Compliance",
                        LastName = "Officer",
                        FullName = "Compliance Officer",
                        Role = "ComplianceOfficer",
                        Department = "Compliance",
                        Position = "Senior Compliance Officer",
                        PhoneNumber = "+91-9876543211",
                        IsActive = true,
                        IsEmailVerified = true,
                        TimeZone = "Asia/Kolkata",
                        Language = "en",
                        CreatedBy = "System"
                    },
                    new OrganizationUser
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        Username = $"manager_{org.Code.ToLower()}",
                        Email = $"manager@{org.Code.ToLower()}.com",
                        FirstName = "Manager",
                        LastName = "User",
                        FullName = "Manager User",
                        Role = "Manager",
                        Department = "Operations",
                        Position = "Operations Manager",
                        PhoneNumber = "+91-9876543212",
                        IsActive = true,
                        IsEmailVerified = true,
                        TimeZone = "Asia/Kolkata",
                        Language = "en",
                        CreatedBy = "System"
                    }
                });
            }

            context.OrganizationUsers.AddRange(users);
            await context.SaveChangesAsync();
        }

        private static async Task SeedCustomersAsync(PepScannerDbContext context)
        {
            var organizations = await context.Organizations.ToListAsync();
            
            var customers = new List<Customer>();
            
            foreach (var org in organizations)
            {
                customers.AddRange(new[]
                {
                    new Customer
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        FullName = "Rahul Kumar",
                        AliasNames = "Rahul K, RK",
                        DateOfBirth = new DateTime(1985, 5, 15),
                        Nationality = "Indian",
                        IdentificationNumber = "ABCD123456",
                        IdentificationType = "PAN",
                        Address = "123 Main Street, Andheri West",
                        City = "Mumbai",
                        State = "Maharashtra",
                        PostalCode = "400058",
                        Country = "India",
                        Occupation = "Software Engineer",
                        Employer = "Tech Solutions Ltd",
                        PhoneNumber = "+91-9876543210",
                        EmailAddress = "rahul.kumar@email.com",
                        IsPep = false,
                        RiskScore = 25,
                        RiskLevel = "Low",
                        IsActive = true,
                        ScreeningFrequency = "Monthly",
                        AccountNumber = $"ACC{org.Code}001",
                        AccountType = "Savings",
                        CreatedBy = "System"
                    },
                    new Customer
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        FullName = "Priya Sharma",
                        AliasNames = "Priya S, PS",
                        DateOfBirth = new DateTime(1990, 8, 22),
                        Nationality = "Indian",
                        IdentificationNumber = "EFGH789012",
                        IdentificationType = "PAN",
                        Address = "456 Park Avenue, Bandra East",
                        City = "Mumbai",
                        State = "Maharashtra",
                        PostalCode = "400051",
                        Country = "India",
                        Occupation = "Marketing Manager",
                        Employer = "Global Marketing Inc",
                        PhoneNumber = "+91-9876543211",
                        EmailAddress = "priya.sharma@email.com",
                        IsPep = false,
                        RiskScore = 30,
                        RiskLevel = "Low",
                        IsActive = true,
                        ScreeningFrequency = "Monthly",
                        AccountNumber = $"ACC{org.Code}002",
                        AccountType = "Current",
                        CreatedBy = "System"
                    },
                    new Customer
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        FullName = "Amit Patel",
                        AliasNames = "Amit P, AP",
                        DateOfBirth = new DateTime(1978, 12, 10),
                        Nationality = "Indian",
                        IdentificationNumber = "IJKL345678",
                        IdentificationType = "PAN",
                        Address = "789 Business Center, Worli",
                        City = "Mumbai",
                        State = "Maharashtra",
                        PostalCode = "400018",
                        Country = "India",
                        Occupation = "Business Owner",
                        Employer = "Patel Enterprises",
                        PhoneNumber = "+91-9876543212",
                        EmailAddress = "amit.patel@email.com",
                        IsPep = true,
                        RiskScore = 85,
                        RiskLevel = "High",
                        PepPosition = "Member of Legislative Assembly",
                        PepCountry = "India",
                        PepStartDate = new DateTime(2020, 1, 1),
                        RequiresEdd = true,
                        IsActive = true,
                        ScreeningFrequency = "Weekly",
                        AccountNumber = $"ACC{org.Code}003",
                        AccountType = "Business",
                        CreatedBy = "System"
                    }
                });
            }

            context.Customers.AddRange(customers);
            await context.SaveChangesAsync();
        }

        private static async Task SeedWatchlistEntriesAsync(PepScannerDbContext context)
        {
            var watchlistEntries = new[]
            {
                // OFAC Sanctions
                new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    Source = "OFAC",
                    ListType = "Sanctions",
                    PrimaryName = "John Doe",
                    AlternateNames = "Johnny Doe, J. Doe",
                    Country = "United States",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1970, 1, 1),
                    RiskLevel = "High",
                    EntityType = "Individual",
                    Nationality = "American",
                    Address = "123 Main Street, New York, NY",
                    City = "New York",
                    State = "NY",
                    PostalCode = "10001",
                    CountryOfResidence = "United States",
                    SanctionType = "SDN",
                    SanctionAuthority = "OFAC",
                    SanctionReference = "OFAC123456",
                    SanctionReason = "Terrorism",
                    DateAddedUtc = DateTime.UtcNow.AddDays(-30),
                    IsActive = true,
                    AddedBy = "System"
                },
                new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    Source = "OFAC",
                    ListType = "Sanctions",
                    PrimaryName = "Jane Smith",
                    AlternateNames = "Jane S. Smith, J. Smith",
                    Country = "United States",
                    Gender = "Female",
                    DateOfBirth = new DateTime(1980, 5, 15),
                    RiskLevel = "High",
                    EntityType = "Individual",
                    Nationality = "American",
                    Address = "456 Oak Avenue, Los Angeles, CA",
                    City = "Los Angeles",
                    State = "CA",
                    PostalCode = "90210",
                    CountryOfResidence = "United States",
                    SanctionType = "SDN",
                    SanctionAuthority = "OFAC",
                    SanctionReference = "OFAC789012",
                    SanctionReason = "Drug Trafficking",
                    DateAddedUtc = DateTime.UtcNow.AddDays(-45),
                    IsActive = true,
                    AddedBy = "System"
                },

                // UN Sanctions
                new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    Source = "UN",
                    ListType = "Sanctions",
                    PrimaryName = "Mohammed Al-Rashid",
                    AlternateNames = "M. Al-Rashid, Mohammed Rashid",
                    Country = "Syria",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1965, 3, 20),
                    RiskLevel = "Critical",
                    EntityType = "Individual",
                    Nationality = "Syrian",
                    Address = "789 Damascus Street, Damascus",
                    City = "Damascus",
                    CountryOfResidence = "Syria",
                    SanctionType = "UN Sanctions",
                    SanctionAuthority = "UN Security Council",
                    SanctionReference = "UN123456",
                    SanctionReason = "Terrorism Financing",
                    DateAddedUtc = DateTime.UtcNow.AddDays(-60),
                    IsActive = true,
                    AddedBy = "System"
                },

                // RBI Lists
                new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    Source = "RBI",
                    ListType = "Wilful Defaulters",
                    PrimaryName = "Rajesh Kumar",
                    AlternateNames = "R. Kumar, Rajesh K",
                    Country = "India",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1975, 7, 12),
                    RiskLevel = "High",
                    EntityType = "Individual",
                    Nationality = "Indian",
                    Address = "321 Business Park, Mumbai",
                    City = "Mumbai",
                    State = "Maharashtra",
                    PostalCode = "400001",
                    CountryOfResidence = "India",
                    SanctionType = "Wilful Defaulter",
                    SanctionAuthority = "RBI",
                    SanctionReference = "RBI001",
                    SanctionReason = "Wilful Default",
                    DateAddedUtc = DateTime.UtcNow.AddDays(-90),
                    IsActive = true,
                    AddedBy = "System"
                },

                // SEBI Lists
                new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    Source = "SEBI",
                    ListType = "Debarred Entities",
                    PrimaryName = "Vikram Singh",
                    AlternateNames = "V. Singh, Vikram S",
                    Country = "India",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1982, 11, 8),
                    RiskLevel = "Medium",
                    EntityType = "Individual",
                    Nationality = "Indian",
                    Address = "654 Financial District, Mumbai",
                    City = "Mumbai",
                    State = "Maharashtra",
                    PostalCode = "400001",
                    CountryOfResidence = "India",
                    SanctionType = "Debarred",
                    SanctionAuthority = "SEBI",
                    SanctionReference = "SEBI001",
                    SanctionReason = "Market Manipulation",
                    DateAddedUtc = DateTime.UtcNow.AddDays(-75),
                    IsActive = true,
                    AddedBy = "System"
                },

                // Indian Parliament PEP
                new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    Source = "IndianParliament",
                    ListType = "PEP",
                    PrimaryName = "Amit Patel",
                    AlternateNames = "A. Patel, Amit P",
                    Country = "India",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1978, 12, 10),
                    RiskLevel = "Medium",
                    EntityType = "Individual",
                    Nationality = "Indian",
                    Address = "789 Legislative Assembly, Mumbai",
                    City = "Mumbai",
                    State = "Maharashtra",
                    PostalCode = "400001",
                    CountryOfResidence = "India",
                    PepPosition = "Member of Legislative Assembly",
                    PepCountry = "India",
                    PepCategory = "State Legislature",
                    PepDescription = "Elected representative in Maharashtra Legislative Assembly",
                    DateAddedUtc = DateTime.UtcNow.AddDays(-120),
                    IsActive = true,
                    AddedBy = "System"
                }
            };

            context.WatchlistEntries.AddRange(watchlistEntries);
            await context.SaveChangesAsync();
        }

        private static async Task SeedOrganizationWatchlistsAsync(PepScannerDbContext context)
        {
            var organizations = await context.Organizations.ToListAsync();
            
            var watchlists = new List<OrganizationWatchlist>();
            
            foreach (var org in organizations)
            {
                watchlists.AddRange(new[]
                {
                    new OrganizationWatchlist
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        WatchlistSource = "OFAC",
                        WatchlistType = "Sanctions",
                        IsEnabled = true,
                        IsRequired = true,
                        Priority = 1,
                        MatchThreshold = 0.8,
                        RiskLevel = "High",
                        AutoAlert = true,
                        RequireReview = true,
                        ReviewRole = "ComplianceOfficer",
                        UpdateFrequency = "Daily",
                        CreatedBy = "System"
                    },
                    new OrganizationWatchlist
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        WatchlistSource = "UN",
                        WatchlistType = "Sanctions",
                        IsEnabled = true,
                        IsRequired = true,
                        Priority = 2,
                        MatchThreshold = 0.8,
                        RiskLevel = "High",
                        AutoAlert = true,
                        RequireReview = true,
                        ReviewRole = "ComplianceOfficer",
                        UpdateFrequency = "Daily",
                        CreatedBy = "System"
                    },
                    new OrganizationWatchlist
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        WatchlistSource = "RBI",
                        WatchlistType = "Local Lists",
                        IsEnabled = true,
                        IsRequired = true,
                        Priority = 3,
                        MatchThreshold = 0.7,
                        RiskLevel = "Medium",
                        AutoAlert = true,
                        RequireReview = true,
                        ReviewRole = "ComplianceOfficer",
                        UpdateFrequency = "Daily",
                        CreatedBy = "System"
                    },
                    new OrganizationWatchlist
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        WatchlistSource = "SEBI",
                        WatchlistType = "Local Lists",
                        IsEnabled = true,
                        IsRequired = false,
                        Priority = 4,
                        MatchThreshold = 0.7,
                        RiskLevel = "Medium",
                        AutoAlert = true,
                        RequireReview = true,
                        ReviewRole = "ComplianceOfficer",
                        UpdateFrequency = "Weekly",
                        CreatedBy = "System"
                    },
                    new OrganizationWatchlist
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        WatchlistSource = "IndianParliament",
                        WatchlistType = "PEP",
                        IsEnabled = true,
                        IsRequired = true,
                        Priority = 5,
                        MatchThreshold = 0.6,
                        RiskLevel = "Medium",
                        AutoAlert = true,
                        RequireReview = true,
                        ReviewRole = "ComplianceOfficer",
                        UpdateFrequency = "Monthly",
                        CreatedBy = "System"
                    }
                });
            }

            context.OrganizationWatchlists.AddRange(watchlists);
            await context.SaveChangesAsync();
        }

        private static async Task SeedOrganizationConfigurationsAsync(PepScannerDbContext context)
        {
            var organizations = await context.Organizations.ToListAsync();
            
            var configurations = new List<OrganizationConfiguration>();
            
            foreach (var org in organizations)
            {
                configurations.AddRange(new[]
                {
                    new OrganizationConfiguration
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        Category = "Screening",
                        Key = "DefaultMatchThreshold",
                        Value = "0.7",
                        DataType = "Number",
                        Description = "Default similarity threshold for name matching",
                        IsRequired = true,
                        IsEncrypted = false,
                        CreatedBy = "System"
                    },
                    new OrganizationConfiguration
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        Category = "Screening",
                        Key = "HighRiskThreshold",
                        Value = "0.8",
                        DataType = "Number",
                        Description = "Threshold for high-risk matches",
                        IsRequired = true,
                        IsEncrypted = false,
                        CreatedBy = "System"
                    },
                    new OrganizationConfiguration
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        Category = "Alerting",
                        Key = "AutoEscalationHours",
                        Value = "24",
                        DataType = "Number",
                        Description = "Hours before auto-escalation of alerts",
                        IsRequired = true,
                        IsEncrypted = false,
                        CreatedBy = "System"
                    },
                    new OrganizationConfiguration
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        Category = "Alerting",
                        Key = "SLAHours",
                        Value = "48",
                        DataType = "Number",
                        Description = "SLA hours for alert resolution",
                        IsRequired = true,
                        IsEncrypted = false,
                        CreatedBy = "System"
                    },
                    new OrganizationConfiguration
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        Category = "Reporting",
                        Key = "ReportRetentionDays",
                        Value = "2555",
                        DataType = "Number",
                        Description = "Days to retain reports (7 years)",
                        IsRequired = true,
                        IsEncrypted = false,
                        CreatedBy = "System"
                    }
                });
            }

            context.OrganizationConfigurations.AddRange(configurations);
            await context.SaveChangesAsync();
        }
    }
}
