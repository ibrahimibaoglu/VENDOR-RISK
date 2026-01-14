using VendorRiskAPI.Domain.Entities;

namespace VendorRiskAPI.Application.Interfaces;

public interface IRiskScoringService
{
    Task<RiskAssessment> CalculateRiskScoreAsync(VendorProfile vendor);
    decimal CalculateFinancialRisk(VendorProfile vendor);
    decimal CalculateOperationalRisk(VendorProfile vendor);
    decimal CalculateSecurityComplianceRisk(VendorProfile vendor);
    string GenerateExplanation(VendorProfile vendor, RiskAssessment assessment);
}
