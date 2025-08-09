using PEPScanner.Domain.Entities;

namespace PEPScanner.Application.Abstractions
{
    public interface IInHouseFileProcessorService
    {
        Task<WatchlistUpdateResult> ProcessFileAsync(IFormFile file, string sourceName, string? fileFormat = null);
        Task<List<string>> GetSupportedFormatsAsync();
        Task<FileValidationResult> ValidateFileAsync(IFormFile file);
    }
}
