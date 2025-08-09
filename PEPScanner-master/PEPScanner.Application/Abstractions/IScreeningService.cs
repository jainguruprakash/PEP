using PEPScanner.Domain.Entities;

namespace PEPScanner.Application.Abstractions
{
    public interface IScreeningService
    {
        Task<ScreeningResult> ScreenCustomerAsync(Customer customer, string context = "Onboarding");
        Task<List<ScreeningResult>> BatchScreenCustomersAsync(List<Customer> customers, Guid jobId);
        Task<ScreeningResult> ScreenTransactionAsync(TransactionScreeningRequest transaction);
        Task<List<NameMatchResult>> SearchNamesAsync(NameSearchRequest searchRequest);
        Task<Customer> UpdateScreeningStatusAsync(Guid customerId, DateTime screeningDate);
    }
}
