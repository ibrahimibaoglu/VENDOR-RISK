using VendorRiskAPI.Application.DTOs.Common;

namespace VendorRiskAPI.Application.DTOs.Response;

public class VendorResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int FinancialHealth { get; set; }
    public decimal SlaUptime { get; set; }
    public int MajorIncidents { get; set; }
    public List<string> SecurityCerts { get; set; } = new();
    public DocumentValidationDto Documents { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

