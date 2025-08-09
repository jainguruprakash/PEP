using PEPScanner.Application.Abstractions;
using PEPScanner.Application.Contracts;
using PEPScanner.Application.Contracts;

namespace PEPScanner.Infrastructure.Services;

public class ScreeningService : IScreeningService
{
    public Task<ScreeningStatistics> GetScreeningStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        return Task.FromResult(new ScreeningStatistics { AlertCount = 0, CustomersScreened = 0, AverageRisk = 0 });
    }

    public Task<List<NameMatchResult>> SearchNamesAsync(NameSearchRequest request)
    {
        return Task.FromResult(new List<NameMatchResult>());
    }

    public Task<ScreeningResult> ScreenCustomerAsync(CustomerScreeningRequest customer, string context)
    {
        return Task.FromResult(new ScreeningResult { CustomerId = customer.Id, CustomerName = customer.FullName, HasMatches = false, RiskScore = 0, RiskLevel = "Low" });
    }

    public Task<ScreeningResult> ScreenTransactionAsync(TransactionScreeningRequest request)
    {
        return Task.FromResult(new ScreeningResult { HasMatches = false });
    }

    public Task<object> UpdateScreeningStatusAsync(Guid customerId, DateTime screeningDate)
    {
        return Task.FromResult<object>(new { customerId, screeningDate });
    }
}


