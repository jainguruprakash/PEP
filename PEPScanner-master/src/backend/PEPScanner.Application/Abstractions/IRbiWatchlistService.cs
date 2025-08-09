using PEPScanner.Domain.Entities;

namespace PEPScanner.Application.Abstractions
{
    public interface IRbiWatchlistService : IBaseWatchlistService
    {
        Task<List<RbiEntry>> FetchRbiDataAsync();
        Task<WatchlistUpdateResult> UpdateFromRbiAsync();
        Task<List<RbiEntry>> ParseRbiWebDataAsync();
    }

    public class RbiEntry
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Category { get; set; }
        public string? Country { get; set; }
        public string? Nationality { get; set; }
        public string? Address { get; set; }
        public string? DateOfBirth { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? PassportNumber { get; set; }
        public string? IdNumber { get; set; }
        public string? Remarks { get; set; }
        public DateTime? ListedDate { get; set; }
        public string? Source { get; set; } = "RBI";
    }
}
