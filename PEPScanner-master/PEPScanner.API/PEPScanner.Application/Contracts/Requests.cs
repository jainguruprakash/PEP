namespace PEPScanner.Application.Contracts;

public class CustomerScreeningRequest
{
    public Guid? Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? Country { get; set; }
    public string? IdentificationNumber { get; set; }
    public string? IdentificationType { get; set; }
}

public class TransactionScreeningRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? SenderName { get; set; }
    public string? BeneficiaryName { get; set; }
    public string? SourceCountry { get; set; }
    public string? DestinationCountry { get; set; }
}

public class NameSearchRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }
    public double Threshold { get; set; } = 0.7;
    public int MaxResults { get; set; } = 50;
}

public class NameMatchResult
{
    public string Name { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public string? SourceList { get; set; }
    public string? RiskLevel { get; set; }
}

public class ScreeningStatistics
{
    public int AlertCount { get; set; }
    public int CustomersScreened { get; set; }
    public double AverageRisk { get; set; }
}


