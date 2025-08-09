using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Services
{
    public interface IOfacDataService
    {
        /// <summary>
        /// Fetches the latest OFAC SDN (Specially Designated Nationals) list
        /// </summary>
        /// <returns>List of OFAC entries</returns>
        Task<List<OfacEntry>> FetchSdnListAsync();

        /// <summary>
        /// Fetches OFAC data for a specific entity by name
        /// </summary>
        /// <param name="name">Name to search</param>
        /// <returns>Matching OFAC entries</returns>
        Task<List<OfacEntry>> SearchByNameAsync(string name);

        /// <summary>
        /// Updates the local watchlist with latest OFAC data
        /// </summary>
        /// <returns>Number of entries updated</returns>
        Task<int> UpdateWatchlistFromOfacAsync();

        /// <summary>
        /// Downloads and processes OFAC CSV files
        /// </summary>
        /// <param name="fileType">Type of OFAC file (sdn, cons, add)</param>
        /// <returns>Processing result</returns>
        Task<OfacProcessingResult> ProcessOfacFileAsync(string fileType = "sdn");

        /// <summary>
        /// Gets the last update timestamp for OFAC data
        /// </summary>
        /// <returns>Last update timestamp</returns>
        Task<DateTime?> GetLastUpdateTimestampAsync();
    }

    public class OfacEntry
    {
        public string? EntityNumber { get; set; }
        public string? SdnType { get; set; }
        public string? Program { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? CallSign { get; set; }
        public string? VesselType { get; set; }
        public string? Tonnage { get; set; }
        public string? GrossRegisteredTonnage { get; set; }
        public string? VesselFlag { get; set; }
        public string? VesselOwner { get; set; }
        public string? Remarks { get; set; }
        public DateTime? SdnDate { get; set; }
        public string? Citizenship { get; set; }
        public string? Nationality { get; set; }
        public string? DateOfBirth { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? IdNumber { get; set; }
        public string? IdType { get; set; }
        public string? IdCountry { get; set; }
        public string? IdExpiration { get; set; }
        public string? IdNumber2 { get; set; }
        public string? IdType2 { get; set; }
        public string? IdCountry2 { get; set; }
        public string? IdExpiration2 { get; set; }
        public string? IdNumber3 { get; set; }
        public string? IdType3 { get; set; }
        public string? IdCountry3 { get; set; }
        public string? IdExpiration3 { get; set; }
        public string? IdNumber4 { get; set; }
        public string? IdType4 { get; set; }
        public string? IdCountry4 { get; set; }
        public string? IdExpiration4 { get; set; }
        public string? IdNumber5 { get; set; }
        public string? IdType5 { get; set; }
        public string? IdCountry5 { get; set; }
        public string? IdExpiration5 { get; set; }
        public string? IdNumber6 { get; set; }
        public string? IdType6 { get; set; }
        public string? IdCountry6 { get; set; }
        public string? IdExpiration6 { get; set; }
        public string? IdNumber7 { get; set; }
        public string? IdType7 { get; set; }
        public string? IdCountry7 { get; set; }
        public string? IdExpiration7 { get; set; }
        public string? IdNumber8 { get; set; }
        public string? IdType8 { get; set; }
        public string? IdCountry8 { get; set; }
        public string? IdExpiration8 { get; set; }
        public string? IdNumber9 { get; set; }
        public string? IdType9 { get; set; }
        public string? IdCountry9 { get; set; }
        public string? IdExpiration9 { get; set; }
        public string? IdNumber10 { get; set; }
        public string? IdType10 { get; set; }
        public string? IdCountry10 { get; set; }
        public string? IdExpiration10 { get; set; }
    }

    public class OfacProcessingResult
    {
        public bool Success { get; set; }
        public int TotalRecords { get; set; }
        public int NewRecords { get; set; }
        public int UpdatedRecords { get; set; }
        public int DeletedRecords { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ProcessingDate { get; set; } = DateTime.UtcNow;
    }
}
