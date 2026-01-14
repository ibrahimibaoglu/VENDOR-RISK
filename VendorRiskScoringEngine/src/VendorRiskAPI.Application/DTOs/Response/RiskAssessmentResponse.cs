namespace VendorRiskAPI.Application.DTOs.Response;

public class RiskAssessmentResponse
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public decimal FinancialRiskScore { get; set; }
    public decimal OperationalRiskScore { get; set; }
    public decimal SecurityComplianceRiskScore { get; set; }
    public decimal FinalRiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public DateTime AssessedAt { get; set; }
    public string AssessedBy { get; set; } = string.Empty;
}
