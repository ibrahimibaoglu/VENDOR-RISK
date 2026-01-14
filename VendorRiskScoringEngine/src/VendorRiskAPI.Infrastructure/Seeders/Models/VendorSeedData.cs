namespace VendorRiskAPI.Infrastructure.Seeders.Models;

public class VendorSeedData
{
    public List<VendorDto> Vendors { get; set; } = new();
}

public class VendorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int FinancialHealth { get; set; }
    public decimal SlaUptime { get; set; }
    public int MajorIncidents { get; set; }
    public List<string> SecurityCerts { get; set; } = new();
    public DocumentsDto Documents { get; set; } = new();
}

public class DocumentsDto
{
    public bool ContractValid { get; set; }
    public bool PrivacyPolicyValid { get; set; }
    public bool PentestReportValid { get; set; }
}
