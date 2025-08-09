using PEPScanner.Domain.Entities;

namespace PEPScanner.Application.Abstractions
{
    public interface IUnSanctionsService
    {
        Task<List<UnSanctionsEntry>> FetchUnSanctionsAsync();
        Task<WatchlistUpdateResult> UpdateWatchlistFromUnAsync();
        Task<List<UnSanctionsEntry>> SearchByNameAsync(string name);
    }

    public class UnSanctionsEntry
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Program { get; set; }
        public string? ListType { get; set; }
        public string? Country { get; set; }
        public string? Nationality { get; set; }
        public string? DateOfBirth { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? Address { get; set; }
        public string? AdditionalInfo { get; set; }
        public string? Comments { get; set; }
        public DateTime? ListedDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? Source { get; set; } = "UN";
    }

    public class UnSanctionsData
    {
        public List<UnSanctionsEntry> Entries { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public string Version { get; set; } = string.Empty;
    }
}
