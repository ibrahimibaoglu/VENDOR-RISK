using Microsoft.Extensions.Logging;
using VendorRiskAPI.Application.Interfaces;
using VendorRiskAPI.Domain.Entities;
using VendorRiskAPI.Domain.Enums;

namespace VendorRiskAPI.Application.Services;

public class RiskScoringService : IRiskScoringService
{
    private readonly ILogger<RiskScoringService> _logger;

    public RiskScoringService(ILogger<RiskScoringService> logger)
    {
        _logger = logger;
    }

    public async Task<RiskAssessment> CalculateRiskScoreAsync(VendorProfile vendor)
    {
        _logger.LogInformation("Starting risk assessment for vendor {VendorId}: {VendorName}", vendor.Id, vendor.Name);

        var assessment = new RiskAssessment
        {
            VendorId = vendor.Id,
            FinancialRiskScore = CalculateFinancialRisk(vendor),
            OperationalRiskScore = CalculateOperationalRisk(vendor),
            SecurityComplianceRiskScore = CalculateSecurityComplianceRisk(vendor),
            AssessedAt = DateTime.UtcNow
        };

        assessment.CalculateFinalScore();
        assessment.DetermineRiskLevel();
        assessment.Explanation = GenerateExplanation(vendor, assessment);

        _logger.LogInformation(
            "Risk assessment completed for vendor {VendorId}: Financial={FinancialScore:F2}, Operational={OperationalScore:F2}, Security={SecurityScore:F2}, Final={FinalScore:F2}, Level={RiskLevel}",
            vendor.Id,
            assessment.FinancialRiskScore,
            assessment.OperationalRiskScore,
            assessment.SecurityComplianceRiskScore,
            assessment.FinalRiskScore,
            assessment.RiskLevel);

        return await Task.FromResult(assessment);
    }

    public decimal CalculateFinancialRisk(VendorProfile vendor)
    {
        decimal riskScore = 0.0m;

        // Financial Health < 50 = High Risk (0.8)
        if (vendor.FinancialHealth < 50)
        {
            riskScore = 0.80m;
            _logger.LogWarning("High financial risk detected for vendor {VendorId}: FinancialHealth={FinancialHealth}", 
                vendor.Id, vendor.FinancialHealth);
        }
        // Financial Health < 60 = Medium-High Risk (0.60)
        else if (vendor.FinancialHealth < 60)
        {
            riskScore = 0.60m;
            _logger.LogInformation("Medium-high financial risk for vendor {VendorId}: FinancialHealth={FinancialHealth}", 
                vendor.Id, vendor.FinancialHealth);
        }
        // Financial Health < 70 = Medium Risk (0.40)
        else if (vendor.FinancialHealth < 70)
        {
            riskScore = 0.40m;
        }
        // Financial Health < 80 = Low-Medium Risk (0.25)
        else if (vendor.FinancialHealth < 80)
        {
            riskScore = 0.25m;
        }
        // Financial Health >= 80 = Low Risk (0.10)
        else
        {
            riskScore = 0.10m;
            _logger.LogDebug("Low financial risk for vendor {VendorId}: FinancialHealth={FinancialHealth}", 
                vendor.Id, vendor.FinancialHealth);
        }

        return riskScore;
    }

    public decimal CalculateOperationalRisk(VendorProfile vendor)
    {
        decimal riskScore = 0.0m;

        // SLA Uptime Risk
        if (vendor.SlaUptime < 90)
        {
            riskScore += 0.50m;
            _logger.LogWarning("Critical SLA uptime for vendor {VendorId}: {SlaUptime}%", vendor.Id, vendor.SlaUptime);
        }
        else if (vendor.SlaUptime < 95)
        {
            riskScore += 0.35m;
            _logger.LogWarning("Poor SLA uptime for vendor {VendorId}: {SlaUptime}%", vendor.Id, vendor.SlaUptime);
        }
        else if (vendor.SlaUptime < 99)
        {
            riskScore += 0.15m;
        }
        else
        {
            riskScore += 0.05m;
        }

        // Major Incidents Risk
        if (vendor.MajorIncidents > 3)
        {
            riskScore += 0.40m;
            _logger.LogWarning("High incident count for vendor {VendorId}: {MajorIncidents} incidents", 
                vendor.Id, vendor.MajorIncidents);
        }
        else if (vendor.MajorIncidents > 2)
        {
            riskScore += 0.25m;
            _logger.LogInformation("Multiple incidents for vendor {VendorId}: {MajorIncidents} incidents", 
                vendor.Id, vendor.MajorIncidents);
        }
        else if (vendor.MajorIncidents > 0)
        {
            riskScore += 0.10m;
        }

        // Normalize (max 1.0)
        return Math.Min(riskScore, 1.0m);
    }

    public decimal CalculateSecurityComplianceRisk(VendorProfile vendor)
    {
        decimal riskScore = 0.0m;

        // Security Certifications
        if (!vendor.HasAnyCertifications())
        {
            riskScore += 0.40m;
            _logger.LogWarning("Vendor {VendorId} has no security certifications", vendor.Id);
        }
        else
        {
            if (!vendor.HasISO27001())
            {
                riskScore += 0.20m;
                _logger.LogInformation("Vendor {VendorId} missing ISO27001 certification", vendor.Id);
            }
            if (!vendor.HasSOC2() && !vendor.HasPCI())
            {
                riskScore += 0.10m;
            }
        }

        // Document Validation
        int invalidDocCount = vendor.Documents.GetInvalidDocumentCount();
        if (invalidDocCount > 0)
        {
            var invalidDocs = vendor.Documents.GetInvalidDocuments();
            _logger.LogWarning("Vendor {VendorId} has {InvalidDocCount} invalid document(s): {InvalidDocs}", 
                vendor.Id, invalidDocCount, string.Join(", ", invalidDocs));
        }

        if (invalidDocCount == 3)
        {
            riskScore += 0.50m;
        }
        else if (invalidDocCount == 2)
        {
            riskScore += 0.30m;
        }
        else if (invalidDocCount == 1)
        {
            riskScore += 0.15m;
        }

        // Privacy Policy specifically
        if (!vendor.Documents.PrivacyPolicyValid)
        {
            riskScore += 0.10m; // Extra weight for privacy
        }

        // Pentest Report specifically
        if (!vendor.Documents.PentestReportValid)
        {
            riskScore += 0.15m; // Extra weight for security testing
        }

        // Normalize (max 1.0)
        return Math.Min(riskScore, 1.0m);
    }

    public string GenerateExplanation(VendorProfile vendor, RiskAssessment assessment)
    {
        var reasons = new List<string>();

        // Financial reasons
        if (vendor.FinancialHealth < 50)
        {
            reasons.Add($"Critical financial health ({vendor.FinancialHealth}/100)");
        }
        else if (vendor.FinancialHealth < 70)
        {
            reasons.Add($"Moderate financial health ({vendor.FinancialHealth}/100)");
        }

        // Operational reasons
        if (vendor.SlaUptime < 95)
        {
            reasons.Add($"SLA uptime below 95% ({vendor.SlaUptime:F2}%)");
        }

        if (vendor.MajorIncidents > 2)
        {
            reasons.Add($"Multiple major incidents ({vendor.MajorIncidents} in last 12 months)");
        }
        else if (vendor.MajorIncidents > 0)
        {
            reasons.Add($"{vendor.MajorIncidents} major incident(s) in last 12 months");
        }

        // Security & Compliance reasons
        if (!vendor.HasISO27001())
        {
            reasons.Add("Missing ISO27001 certification");
        }

        var invalidDocs = vendor.Documents.GetInvalidDocuments();
        if (invalidDocs.Any())
        {
            reasons.Add($"Invalid/expired documents: {string.Join(", ", invalidDocs)}");
        }

        if (reasons.Count == 0)
        {
            return "Vendor meets all compliance and operational standards with minimal risk indicators.";
        }

        return string.Join(" + ", reasons);
    }
}
