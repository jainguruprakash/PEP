using Microsoft.EntityFrameworkCore;
using PEPScanner.Domain.Entities;
using PEPScanner.Infrastructure.Data;

namespace PEPScanner.Infrastructure.Services;

public interface IReportService
{
    // SAR Methods
    Task<SuspiciousActivityReport> CreateSarAsync(SuspiciousActivityReport sar);
    Task<SuspiciousActivityReport?> GetSarByIdAsync(Guid id);
    Task<IEnumerable<SuspiciousActivityReport>> GetSarsAsync(Guid organizationId, int page = 1, int pageSize = 20);
    Task<SuspiciousActivityReport> UpdateSarAsync(SuspiciousActivityReport sar);
    Task<bool> UpdateSarStatusAsync(Guid sarId, SarStatus newStatus, Guid changedById, string? reason = null);
    Task<bool> DeleteSarAsync(Guid id);
    Task<string> GenerateSarReportNumberAsync(Guid organizationId);

    // STR Methods
    Task<SuspiciousTransactionReport> CreateStrAsync(SuspiciousTransactionReport str);
    Task<SuspiciousTransactionReport?> GetStrByIdAsync(Guid id);
    Task<IEnumerable<SuspiciousTransactionReport>> GetStrsAsync(Guid organizationId, int page = 1, int pageSize = 20);
    Task<SuspiciousTransactionReport> UpdateStrAsync(SuspiciousTransactionReport str);
    Task<bool> UpdateStrStatusAsync(Guid strId, StrStatus newStatus, Guid changedById, string? reason = null);
    Task<bool> DeleteStrAsync(Guid id);
    Task<string> GenerateStrReportNumberAsync(Guid organizationId);

    // Comments
    Task<SarComment> AddSarCommentAsync(Guid sarId, Guid userId, string comment);
    Task<StrComment> AddStrCommentAsync(Guid strId, Guid userId, string comment);

    // Statistics
    Task<ReportStatistics> GetReportStatisticsAsync(Guid organizationId, DateTime? fromDate = null, DateTime? toDate = null);
}

public class ReportService : IReportService
{
    private readonly PepScannerDbContext _context;

    public ReportService(PepScannerDbContext context)
    {
        _context = context;
    }

    // SAR Implementation
    public async Task<SuspiciousActivityReport> CreateSarAsync(SuspiciousActivityReport sar)
    {
        sar.Id = Guid.NewGuid();
        sar.CreatedAt = DateTime.UtcNow;
        sar.UpdatedAt = DateTime.UtcNow;
        
        if (string.IsNullOrEmpty(sar.ReportNumber))
        {
            sar.ReportNumber = await GenerateSarReportNumberAsync(sar.OrganizationId);
        }

        _context.SuspiciousActivityReports.Add(sar);
        await _context.SaveChangesAsync();

        // Add initial status history
        await AddSarStatusHistoryAsync(sar.Id, SarStatus.Draft, sar.Status, sar.ReportedById, "Report created");

        return sar;
    }

    public async Task<SuspiciousActivityReport?> GetSarByIdAsync(Guid id)
    {
        return await _context.SuspiciousActivityReports
            .Include(sar => sar.Organization)
            .Include(sar => sar.ReportedBy)
            .Include(sar => sar.ReviewedBy)
            .Include(sar => sar.Customer)
            .Include(sar => sar.Comments)
                .ThenInclude(c => c.User)
            .Include(sar => sar.StatusHistory)
                .ThenInclude(sh => sh.ChangedBy)
            .FirstOrDefaultAsync(sar => sar.Id == id);
    }

    public async Task<IEnumerable<SuspiciousActivityReport>> GetSarsAsync(Guid organizationId, int page = 1, int pageSize = 20)
    {
        return await _context.SuspiciousActivityReports
            .Where(sar => sar.OrganizationId == organizationId)
            .Include(sar => sar.ReportedBy)
            .Include(sar => sar.ReviewedBy)
            .Include(sar => sar.Customer)
            .OrderByDescending(sar => sar.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<SuspiciousActivityReport> UpdateSarAsync(SuspiciousActivityReport sar)
    {
        sar.UpdatedAt = DateTime.UtcNow;
        _context.SuspiciousActivityReports.Update(sar);
        await _context.SaveChangesAsync();
        return sar;
    }

    public async Task<bool> UpdateSarStatusAsync(Guid sarId, SarStatus newStatus, Guid changedById, string? reason = null)
    {
        var sar = await _context.SuspiciousActivityReports.FindAsync(sarId);
        if (sar == null) return false;

        var oldStatus = sar.Status;
        sar.Status = newStatus;
        sar.UpdatedAt = DateTime.UtcNow;

        if (newStatus == SarStatus.Submitted)
        {
            sar.SubmissionDate = DateTime.UtcNow;
        }
        else if (newStatus == SarStatus.Filed)
        {
            sar.RegulatoryFilingDate = DateTime.UtcNow;
        }

        await AddSarStatusHistoryAsync(sarId, oldStatus, newStatus, changedById, reason);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteSarAsync(Guid id)
    {
        var sar = await _context.SuspiciousActivityReports.FindAsync(id);
        if (sar == null) return false;

        _context.SuspiciousActivityReports.Remove(sar);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerateSarReportNumberAsync(Guid organizationId)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.SuspiciousActivityReports
            .CountAsync(sar => sar.OrganizationId == organizationId && sar.CreatedAt.Year == year);
        
        return $"SAR-{year}-{(count + 1):D6}";
    }

    // STR Implementation (similar to SAR)
    public async Task<SuspiciousTransactionReport> CreateStrAsync(SuspiciousTransactionReport str)
    {
        str.Id = Guid.NewGuid();
        str.CreatedAt = DateTime.UtcNow;
        str.UpdatedAt = DateTime.UtcNow;
        
        if (string.IsNullOrEmpty(str.ReportNumber))
        {
            str.ReportNumber = await GenerateStrReportNumberAsync(str.OrganizationId);
        }

        _context.SuspiciousTransactionReports.Add(str);
        await _context.SaveChangesAsync();

        // Add initial status history
        await AddStrStatusHistoryAsync(str.Id, StrStatus.Draft, str.Status, str.ReportedById, "Report created");

        return str;
    }

    public async Task<SuspiciousTransactionReport?> GetStrByIdAsync(Guid id)
    {
        return await _context.SuspiciousTransactionReports
            .Include(str => str.Organization)
            .Include(str => str.ReportedBy)
            .Include(str => str.ReviewedBy)
            .Include(str => str.Customer)
            .Include(str => str.Comments)
                .ThenInclude(c => c.User)
            .Include(str => str.StatusHistory)
                .ThenInclude(sh => sh.ChangedBy)
            .FirstOrDefaultAsync(str => str.Id == id);
    }

    public async Task<IEnumerable<SuspiciousTransactionReport>> GetStrsAsync(Guid organizationId, int page = 1, int pageSize = 20)
    {
        return await _context.SuspiciousTransactionReports
            .Where(str => str.OrganizationId == organizationId)
            .Include(str => str.ReportedBy)
            .Include(str => str.ReviewedBy)
            .Include(str => str.Customer)
            .OrderByDescending(str => str.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<SuspiciousTransactionReport> UpdateStrAsync(SuspiciousTransactionReport str)
    {
        str.UpdatedAt = DateTime.UtcNow;
        _context.SuspiciousTransactionReports.Update(str);
        await _context.SaveChangesAsync();
        return str;
    }

    public async Task<bool> UpdateStrStatusAsync(Guid strId, StrStatus newStatus, Guid changedById, string? reason = null)
    {
        var str = await _context.SuspiciousTransactionReports.FindAsync(strId);
        if (str == null) return false;

        var oldStatus = str.Status;
        str.Status = newStatus;
        str.UpdatedAt = DateTime.UtcNow;

        if (newStatus == StrStatus.Submitted)
        {
            str.SubmissionDate = DateTime.UtcNow;
        }
        else if (newStatus == StrStatus.Filed)
        {
            str.RegulatoryFilingDate = DateTime.UtcNow;
        }

        await AddStrStatusHistoryAsync(strId, oldStatus, newStatus, changedById, reason);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteStrAsync(Guid id)
    {
        var str = await _context.SuspiciousTransactionReports.FindAsync(id);
        if (str == null) return false;

        _context.SuspiciousTransactionReports.Remove(str);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerateStrReportNumberAsync(Guid organizationId)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.SuspiciousTransactionReports
            .CountAsync(str => str.OrganizationId == organizationId && str.CreatedAt.Year == year);
        
        return $"STR-{year}-{(count + 1):D6}";
    }

    // Comments
    public async Task<SarComment> AddSarCommentAsync(Guid sarId, Guid userId, string comment)
    {
        var sarComment = new SarComment
        {
            Id = Guid.NewGuid(),
            SarId = sarId,
            UserId = userId,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.SarComments.Add(sarComment);
        await _context.SaveChangesAsync();
        return sarComment;
    }

    public async Task<StrComment> AddStrCommentAsync(Guid strId, Guid userId, string comment)
    {
        var strComment = new StrComment
        {
            Id = Guid.NewGuid(),
            StrId = strId,
            UserId = userId,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.StrComments.Add(strComment);
        await _context.SaveChangesAsync();
        return strComment;
    }

    // Statistics
    public async Task<ReportStatistics> GetReportStatisticsAsync(Guid organizationId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        fromDate ??= DateTime.UtcNow.AddMonths(-1);
        toDate ??= DateTime.UtcNow;

        var sarQuery = _context.SuspiciousActivityReports
            .Where(sar => sar.OrganizationId == organizationId && 
                         sar.CreatedAt >= fromDate && sar.CreatedAt <= toDate);

        var strQuery = _context.SuspiciousTransactionReports
            .Where(str => str.OrganizationId == organizationId && 
                         str.CreatedAt >= fromDate && str.CreatedAt <= toDate);

        return new ReportStatistics
        {
            TotalSars = await sarQuery.CountAsync(),
            SarsByStatus = await sarQuery.GroupBy(sar => sar.Status)
                .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count()),
            TotalStrs = await strQuery.CountAsync(),
            StrsByStatus = await strQuery.GroupBy(str => str.Status)
                .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count()),
            PeriodStart = fromDate.Value,
            PeriodEnd = toDate.Value
        };
    }

    // Private helper methods
    private async Task AddSarStatusHistoryAsync(Guid sarId, SarStatus fromStatus, SarStatus toStatus, Guid changedById, string? reason)
    {
        var history = new SarStatusHistory
        {
            Id = Guid.NewGuid(),
            SarId = sarId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedById = changedById,
            Reason = reason,
            ChangedAt = DateTime.UtcNow
        };

        _context.SarStatusHistories.Add(history);
    }

    private async Task AddStrStatusHistoryAsync(Guid strId, StrStatus fromStatus, StrStatus toStatus, Guid changedById, string? reason)
    {
        var history = new StrStatusHistory
        {
            Id = Guid.NewGuid(),
            StrId = strId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedById = changedById,
            Reason = reason,
            ChangedAt = DateTime.UtcNow
        };

        _context.StrStatusHistories.Add(history);
    }
}

public class ReportStatistics
{
    public int TotalSars { get; set; }
    public Dictionary<string, int> SarsByStatus { get; set; } = new();
    public int TotalStrs { get; set; }
    public Dictionary<string, int> StrsByStatus { get; set; } = new();
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}
