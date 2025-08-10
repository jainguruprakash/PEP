using PEPScanner.Domain.Entities;
using PEPScanner.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace PEPScanner.Application.Abstractions
{
    public interface IInHouseFileProcessorService
    {
        Task<WatchlistUpdateResult> ProcessFileAsync(IFormFile file, string sourceName, string? fileFormat = null);
        Task<List<string>> GetSupportedFormatsAsync();
        Task<FileValidationResult> ValidateFileAsync(IFormFile file);
    }
}
