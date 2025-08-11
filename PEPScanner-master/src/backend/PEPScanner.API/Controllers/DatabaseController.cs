using Microsoft.AspNetCore.Mvc;
using PEPScanner.Infrastructure.Data;
using PEPScanner.API.Data;

namespace PEPScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly PepScannerDbContext _context;

    public DatabaseController(PepScannerDbContext context)
    {
        _context = context;
    }

    [HttpPost("clear")]
    public async Task<IActionResult> ClearDatabase()
    {
        await PEPScanner.API.Data.ClearDatabase.ClearAllDataAsync(_context);
        return Ok(new { message = "Database cleared successfully" });
    }
}