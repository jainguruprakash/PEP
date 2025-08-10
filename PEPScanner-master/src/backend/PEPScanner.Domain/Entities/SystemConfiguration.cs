using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    public class SystemConfiguration
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = "";
        
        [Required]
        public string Value { get; set; } = "";
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
    }
}
