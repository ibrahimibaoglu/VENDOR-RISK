using VendorRiskAPI.Domain.Common;
using VendorRiskAPI.Domain.Enums;

namespace VendorRiskAPI.Domain.Entities;

public class RiskAssessment : BaseEntity
{
    public int VendorId { get; set; }
    public virtual VendorProfile Vendor { get; set; } = null!;
    
    // Risk scores (0.0 - 1.0 scale)
    public decimal FinancialRiskScore { get; set; }
    public decimal OperationalRiskScore { get; set; }
    public decimal SecurityComplianceRiskScore { get; set; }
    
    // Final calculated score (0.0 - 1.0 scale)
    public decimal FinalRiskScore { get; set; }
    
    // Risk level determination
    public RiskLevel RiskLevel { get; set; }
    
    // Human-readable explanation
    public string Explanation { get; set; } = string.Empty;
    
    // Assessment metadata
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
    public string AssessedBy { get; set; } = "System";

    // Helper methods
    public void CalculateFinalScore()
    {
        // Formula: (Financial * 0.4) + (Operational * 0.3) + (Security * 0.3)
        FinalRiskScore = Math.Round(
            (FinancialRiskScore * 0.4m) + 
            (OperationalRiskScore * 0.3m) + 
            (SecurityComplianceRiskScore * 0.3m), 
            2
        );
    }

    public void DetermineRiskLevel()
    {
        RiskLevel = FinalRiskScore switch
        {
            < 0.25m => RiskLevel.Low,
            < 0.50m => RiskLevel.Medium,
            < 0.75m => RiskLevel.High,
            _ => RiskLevel.Critical
        };
    }
}
