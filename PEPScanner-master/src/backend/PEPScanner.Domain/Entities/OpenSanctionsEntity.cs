using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    public class OpenSanctionsEntity
    {
        [Key]
        [MaxLength(100)]
        public string Id { get; set; } = "";
        
        [MaxLength(50)]
        public string Schema { get; set; } = "";
        
        [MaxLength(500)]
        public string Name { get; set; } = "";
        
        [MaxLength(2000)]
        public string? Aliases { get; set; } // JSON array
        
        public DateTime? BirthDate { get; set; }
        
        [MaxLength(1000)]
        public string? Countries { get; set; } // JSON array
        
        [MaxLength(2000)]
        public string? Addresses { get; set; } // JSON array
        
        [MaxLength(1000)]
        public string? Identifiers { get; set; } // JSON array
        
        [MaxLength(2000)]
        public string? Sanctions { get; set; } // JSON array
        
        [MaxLength(1000)]
        public string? Datasets { get; set; } // JSON array
        
        public double Score { get; set; }
        
        public DateTime? FirstSeen { get; set; }
        public DateTime? LastSeen { get; set; }
        public DateTime? LastChange { get; set; }
        
        // Audit fields
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public class OpenSanctionsMatchResult
    {
        public string Query { get; set; } = "";
        public int TotalResults { get; set; }
        public List<OpenSanctionsEntity> Matches { get; set; } = new();
        public DateTime SearchedAt { get; set; }
        public string? Error { get; set; }
        public double? BestScore { get; set; }
        public string? BestMatchId { get; set; }
    }

    public class OpenSanctionsMatchFeatures
    {
        public double NameScore { get; set; }
        public double? DateOfBirthScore { get; set; }
        public double? NationalityScore { get; set; }
        public double? AddressScore { get; set; }
        public double? IdentifierScore { get; set; }
        public double OverallScore { get; set; }
    }

    public class OpenSanctionsSanction
    {
        public string Authority { get; set; } = "";
        public string Type { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Reference { get; set; }
    }
}
