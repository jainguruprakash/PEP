namespace PEPScanner.Application.Contracts;

public class ScreeningResult
{
    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public bool HasMatches { get; set; }
    public int RiskScore { get; set; }
    public string RiskLevel { get; set; } = "Low";
    public bool RequiresEdd { get; set; }
    public bool RequiresStr { get; set; }
    public bool RequiresSar { get; set; }
    public List<ScreeningResultAlert> Alerts { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; } = TimeSpan.Zero;
}

public class ScreeningResultAlert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AlertType { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public string MatchAlgorithm { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public string Priority { get; set; } = "Medium";
    public string RiskLevel { get; set; } = "Low";
    public string? SourceList { get; set; }
    public string? MatchedFields { get; set; }
}


