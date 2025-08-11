using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;

namespace PEPScanner.API.Data;

public static class ClearDatabase
{
    public static async Task ClearAllDataAsync(PepScannerDbContext context)
    {
        // Clear using Entity Framework DbSets instead of raw SQL
        if (context.Users.Any()) 
        {
            context.Users.RemoveRange(context.Users);
        }
        if (context.OrganizationUsers.Any()) 
        {
            context.OrganizationUsers.RemoveRange(context.OrganizationUsers);
        }
        if (context.Organizations.Any()) 
        {
            context.Organizations.RemoveRange(context.Organizations);
        }
        if (context.Alerts.Any()) 
        {
            context.Alerts.RemoveRange(context.Alerts);
        }
        if (context.Customers.Any()) 
        {
            context.Customers.RemoveRange(context.Customers);
        }
        if (context.WatchlistEntries.Any()) 
        {
            context.WatchlistEntries.RemoveRange(context.WatchlistEntries);
        }
        
        await context.SaveChangesAsync();
    }
}