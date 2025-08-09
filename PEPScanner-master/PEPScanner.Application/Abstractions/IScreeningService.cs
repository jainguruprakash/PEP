using PEPScanner.Application.Contracts;
using PEPScanner.Application.Contracts;

namespace PEPScanner.Application.Abstractions;

public interface IScreeningService
{
    Task<ScreeningResult> ScreenCustomerAsync(CustomerScreeningRequest customer, string context);
    Task<ScreeningResult> ScreenTransactionAsync(TransactionScreeningRequest request);
    Task<List<NameMatchResult>> SearchNamesAsync(NameSearchRequest request);
    Task<ScreeningStatistics> GetScreeningStatisticsAsync(DateTime startDate, DateTime endDate);
    Task<object> UpdateScreeningStatusAsync(Guid customerId, DateTime screeningDate);
}


