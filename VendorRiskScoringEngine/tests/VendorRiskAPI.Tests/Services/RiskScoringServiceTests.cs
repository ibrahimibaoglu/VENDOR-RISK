using Microsoft.Extensions.Logging;
using Moq;
using VendorRiskAPI.Application.Services;
using VendorRiskAPI.Domain.Entities;
using VendorRiskAPI.Domain.Enums;
using VendorRiskAPI.Domain.ValueObjects;
using Xunit;

namespace VendorRiskAPI.Tests.Services;

public class RiskScoringServiceTests
{
    private readonly Mock<ILogger<RiskScoringService>> _loggerMock;
    private readonly RiskScoringService _service;

    public RiskScoringServiceTests()
    {
        _loggerMock = new Mock<ILogger<RiskScoringService>>();
        _service = new RiskScoringService(_loggerMock.Object);
    }

    #region Financial Risk Tests

    [Fact]
    public void CalculateFinancialRisk_WhenFinancialHealthBelow50_ShouldReturnHighRisk()
    {
        // Arrange
        var vendor = CreateVendor(financialHealth: 45);

        // Act
        var result = _service.CalculateFinancialRisk(vendor);

        // Assert
        Assert.Equal(0.80m, result);
    }

    [Fact]
    public void CalculateFinancialRisk_WhenFinancialHealthBetween50And60_ShouldReturnMediumHighRisk()
    {
        // Arrange
        var vendor = CreateVendor(financialHealth: 55);

        // Act
        var result = _service.CalculateFinancialRisk(vendor);

        // Assert
        Assert.Equal(0.60m, result);
    }

    [Fact]
    public void CalculateFinancialRisk_WhenFinancialHealthAbove80_ShouldReturnLowRisk()
    {
        // Arrange
        var vendor = CreateVendor(financialHealth: 85);

        // Act
        var result = _service.CalculateFinancialRisk(vendor);

        // Assert
        Assert.Equal(0.10m, result);
    }

    #endregion

    #region Operational Risk Tests

    [Fact]
    public void CalculateOperationalRisk_WhenSlaBelow90_ShouldIncludeHighPenalty()
    {
        // Arrange
        var vendor = CreateVendor(slaUptime: 85m, majorIncidents: 0);

        // Act
        var result = _service.CalculateOperationalRisk(vendor);

        // Assert
        Assert.True(result >= 0.50m);
    }

    [Fact]
    public void CalculateOperationalRisk_WhenMultipleIncidents_ShouldIncludeIncidentPenalty()
    {
        // Arrange
        var vendor = CreateVendor(slaUptime: 99m, majorIncidents: 4);

        // Act
        var result = _service.CalculateOperationalRisk(vendor);

        // Assert
        Assert.True(result >= 0.40m);
    }

    [Fact]
    public void CalculateOperationalRisk_WhenExcellentSlaAndNoIncidents_ShouldReturnLowRisk()
    {
        // Arrange
        var vendor = CreateVendor(slaUptime: 99.5m, majorIncidents: 0);

        // Act
        var result = _service.CalculateOperationalRisk(vendor);

        // Assert
        Assert.True(result <= 0.10m);
    }

    [Fact]
    public void CalculateOperationalRisk_ShouldNotExceedMaximum()
    {
        // Arrange - worst case scenario
        var vendor = CreateVendor(slaUptime: 80m, majorIncidents: 10);

        // Act
        var result = _service.CalculateOperationalRisk(vendor);

        // Assert
        Assert.True(result <= 1.0m);
    }

    #endregion

    #region Security & Compliance Risk Tests

    [Fact]
    public void CalculateSecurityComplianceRisk_WhenNoCertifications_ShouldIncludeHighPenalty()
    {
        // Arrange
        var vendor = CreateVendor(securityCerts: new List<string>());

        // Act
        var result = _service.CalculateSecurityComplianceRisk(vendor);

        // Assert
        Assert.True(result >= 0.40m);
    }

    [Fact]
    public void CalculateSecurityComplianceRisk_WhenMissingISO27001_ShouldIncludePenalty()
    {
        // Arrange
        var vendor = CreateVendor(securityCerts: new List<string> { "SOC2" });

        // Act
        var result = _service.CalculateSecurityComplianceRisk(vendor);

        // Assert
        Assert.True(result >= 0.20m);
    }

    [Fact]
    public void CalculateSecurityComplianceRisk_WhenAllDocumentsInvalid_ShouldIncludeMaxPenalty()
    {
        // Arrange
        var vendor = CreateVendor(
            contractValid: false,
            privacyPolicyValid: false,
            pentestReportValid: false
        );

        // Act
        var result = _service.CalculateSecurityComplianceRisk(vendor);

        // Assert
        Assert.True(result >= 0.50m);
    }

    [Fact]
    public void CalculateSecurityComplianceRisk_WhenAllValid_ShouldReturnLowRisk()
    {
        // Arrange
        var vendor = CreateVendor(
            securityCerts: new List<string> { "ISO27001", "SOC2" },
            contractValid: true,
            privacyPolicyValid: true,
            pentestReportValid: true
        );

        // Act
        var result = _service.CalculateSecurityComplianceRisk(vendor);

        // Assert
        Assert.True(result <= 0.15m);
    }

    #endregion

    #region Full Assessment Tests

    [Fact]
    public async Task CalculateRiskScoreAsync_ShouldCalculateAllRiskScores()
    {
        // Arrange
        var vendor = CreateVendor();

        // Act
        var result = await _service.CalculateRiskScoreAsync(vendor);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.FinancialRiskScore >= 0 && result.FinancialRiskScore <= 1);
        Assert.True(result.OperationalRiskScore >= 0 && result.OperationalRiskScore <= 1);
        Assert.True(result.SecurityComplianceRiskScore >= 0 && result.SecurityComplianceRiskScore <= 1);
    }

    [Fact]
    public async Task CalculateRiskScoreAsync_ShouldCalculateFinalScore()
    {
        // Arrange
        var vendor = CreateVendor(
            financialHealth: 50,  // 0.60 risk
            slaUptime: 90m,       // 0.35 risk
            majorIncidents: 0
        );

        // Act
        var result = await _service.CalculateRiskScoreAsync(vendor);

        // Assert
        // Final = (0.60 * 0.4) + (0.35 * 0.3) + (SecurityRisk * 0.3)
        Assert.True(result.FinalRiskScore > 0);
        Assert.True(result.FinalRiskScore <= 1);
    }

    [Fact]
    public async Task CalculateRiskScoreAsync_ShouldDetermineRiskLevel()
    {
        // Arrange - Low risk vendor
        var vendor = CreateVendor(
            financialHealth: 90,
            slaUptime: 99m,
            majorIncidents: 0,
            securityCerts: new List<string> { "ISO27001", "SOC2" },
            contractValid: true,
            privacyPolicyValid: true,
            pentestReportValid: true
        );

        // Act
        var result = await _service.CalculateRiskScoreAsync(vendor);

        // Assert
        Assert.Equal(RiskLevel.Low, result.RiskLevel);
    }

    [Fact]
    public async Task CalculateRiskScoreAsync_ShouldGenerateExplanation()
    {
        // Arrange
        var vendor = CreateVendor(
            financialHealth: 45,
            slaUptime: 88m,
            privacyPolicyValid: false
        );

        // Act
        var result = await _service.CalculateRiskScoreAsync(vendor);

        // Assert
        Assert.False(string.IsNullOrEmpty(result.Explanation));
        Assert.Contains("financial health", result.Explanation.ToLower());
    }

    #endregion

    #region Explanation Tests

    [Fact]
    public void GenerateExplanation_WhenMultipleIssues_ShouldListAllReasons()
    {
        // Arrange
        var vendor = CreateVendor(
            financialHealth: 45,
            slaUptime: 88m,
            majorIncidents: 3,
            privacyPolicyValid: false
        );
        var assessment = new RiskAssessment();

        // Act
        var result = _service.GenerateExplanation(vendor, assessment);

        // Assert
        Assert.Contains("financial health", result.ToLower());
        Assert.Contains("sla", result.ToLower());
        Assert.Contains("incident", result.ToLower());
        Assert.Contains("privacy policy", result.ToLower());
    }

    [Fact]
    public void GenerateExplanation_WhenNoIssues_ShouldReturnPositiveMessage()
    {
        // Arrange
        var vendor = CreateVendor(
            financialHealth: 90,
            slaUptime: 99m,
            majorIncidents: 0,
            securityCerts: new List<string> { "ISO27001" },
            contractValid: true,
            privacyPolicyValid: true,
            pentestReportValid: true
        );
        var assessment = new RiskAssessment();

        // Act
        var result = _service.GenerateExplanation(vendor, assessment);

        // Assert
        Assert.Contains("meets all compliance", result.ToLower());
    }

    #endregion

    #region Test Data Scenarios

    [Theory]
    [InlineData(95, RiskLevel.Low)]
    [InlineData(75, RiskLevel.Medium)]
    [InlineData(55, RiskLevel.High)]
    [InlineData(40, RiskLevel.Critical)]
    public async Task CalculateRiskScoreAsync_FinancialHealthScenarios_ShouldMapToCorrectRiskLevel(
        int financialHealth, RiskLevel expectedMinLevel)
    {
        // Arrange
        var vendor = CreateVendor(
            financialHealth: financialHealth,
            slaUptime: 95m,
            majorIncidents: 0,
            securityCerts: new List<string> { "ISO27001" }
        );

        // Act
        var result = await _service.CalculateRiskScoreAsync(vendor);

        // Assert
        Assert.True(result.RiskLevel >= expectedMinLevel);
    }

    #endregion

    #region Helper Methods

    private VendorProfile CreateVendor(
        int financialHealth = 75,
        decimal slaUptime = 95m,
        int majorIncidents = 0,
        List<string>? securityCerts = null,
        bool contractValid = true,
        bool privacyPolicyValid = true,
        bool pentestReportValid = true)
    {
        return new VendorProfile
        {
            Id = 1,
            Name = "Test Vendor",
            FinancialHealth = financialHealth,
            SlaUptime = slaUptime,
            MajorIncidents = majorIncidents,
            SecurityCerts = securityCerts ?? new List<string> { "ISO27001" },
            Documents = new DocumentValidation(contractValid, privacyPolicyValid, pentestReportValid)
        };
    }

    #endregion
}
