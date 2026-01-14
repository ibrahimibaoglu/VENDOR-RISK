namespace VendorRiskAPI.Application.Models;

public class RiskFactorMatrix
{
    public Dictionary<string, Dictionary<string, decimal>> FinancialRisk { get; set; } = new();
    public Dictionary<string, Dictionary<string, decimal>> OperationalRisk { get; set; } = new();
    public Dictionary<string, Dictionary<string, decimal>> SecurityRisk { get; set; } = new();
    public Dictionary<string, Dictionary<string, decimal>> ComplianceRisk { get; set; } = new();
}
