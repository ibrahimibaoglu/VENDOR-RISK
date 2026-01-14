using VendorRiskAPI.Domain.Common;
using VendorRiskAPI.Domain.ValueObjects;

namespace VendorRiskAPI.Domain.Entities;

public class VendorProfile : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
    // Financial metrics
    public int FinancialHealth { get; set; } // 0-100 scale
    
    // Operational metrics
    public decimal SlaUptime { get; set; } // Percentage (0-100)
    public int MajorIncidents { get; set; }
    
    // Security certifications
    public List<string> SecurityCerts { get; set; } = new();
    
    // Document validation
    public DocumentValidation Documents { get; set; } = null!;
    
    // Navigation property
    public virtual ICollection<RiskAssessment> RiskAssessments { get; set; } = new List<RiskAssessment>();

    // Business logic methods
    public bool HasISO27001() => SecurityCerts.Contains("ISO27001");
    public bool HasSOC2() => SecurityCerts.Contains("SOC2");
    public bool HasPCI() => SecurityCerts.Contains("PCI-DSS");
    
    public bool HasAnyCertifications() => SecurityCerts.Any();
    
    public bool IsHighRiskFinancially() => FinancialHealth < 50;
    public bool IsLowRiskFinancially() => FinancialHealth > 80;
    
    public bool HasPoorSLA() => SlaUptime < 95;
    public bool HasExcellentSLA() => SlaUptime >= 99;
    
    public bool HasMultipleIncidents() => MajorIncidents > 2;
}
