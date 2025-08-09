namespace PEPScanner.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public bool IsPep { get; set; }
    public int RiskScore { get; set; }
    public string RiskLevel { get; set; } = "Low";
}


