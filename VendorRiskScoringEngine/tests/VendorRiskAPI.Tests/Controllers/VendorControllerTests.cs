using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using VendorRiskAPI.API.Controllers;
using VendorRiskAPI.Application.DTOs.Common;
using VendorRiskAPI.Application.DTOs.Request;
using VendorRiskAPI.Application.DTOs.Response;
using VendorRiskAPI.Application.Interfaces;
using VendorRiskAPI.Application.Mappings;
using VendorRiskAPI.Domain.Entities;
using VendorRiskAPI.Domain.Enums;
using VendorRiskAPI.Domain.ValueObjects;
using Xunit;

namespace VendorRiskAPI.Tests.Controllers;

public class VendorControllerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRiskScoringService> _riskScoringServiceMock;
    private readonly Mock<ILogger<VendorController>> _loggerMock;
    private readonly IMapper _mapper;
    private readonly VendorController _controller;

    public VendorControllerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _riskScoringServiceMock = new Mock<IRiskScoringService>();
        _loggerMock = new Mock<ILogger<VendorController>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _controller = new VendorController(
            _unitOfWorkMock.Object,
            _riskScoringServiceMock.Object,
            _mapper,
            _loggerMock.Object
        );
    }

    #region CreateVendor Tests

    [Fact]
    public async Task CreateVendor_WithValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        var request = new CreateVendorRequest
        {
            Name = "Test Vendor",
            FinancialHealth = 80,
            SlaUptime = 95m,
            MajorIncidents = 0,
            SecurityCerts = new List<string> { "ISO27001" },
            Documents = new DocumentValidationDto
            {
                ContractValid = true,
                PrivacyPolicyValid = true,
                PentestReportValid = true
            }
        };

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _controller.CreateVendor(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<VendorResponse>(createdResult.Value);
        Assert.Equal(request.Name, response.Name);
        Assert.Equal(request.FinancialHealth, response.FinancialHealth);
    }

    [Fact]
    public async Task CreateVendor_ShouldCallUnitOfWorkAddAsync()
    {
        // Arrange
        var request = CreateValidVendorRequest();

        // Act
        await _controller.CreateVendor(request);

        // Assert
        _unitOfWorkMock.Verify(x => x.VendorProfiles.AddAsync(It.IsAny<VendorProfile>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    #endregion

    #region GetVendorById Tests

    [Fact]
    public async Task GetVendorById_WhenVendorExists_ShouldReturnOk()
    {
        // Arrange
        var vendor = CreateTestVendor(1, "Test Vendor");
        _unitOfWorkMock.Setup(x => x.VendorProfiles.GetByIdAsync(1))
            .ReturnsAsync(vendor);

        // Act
        var result = await _controller.GetVendorById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<VendorResponse>(okResult.Value);
        Assert.Equal(vendor.Name, response.Name);
    }

    [Fact]
    public async Task GetVendorById_WhenVendorNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.VendorProfiles.GetByIdAsync(999))
            .ReturnsAsync((VendorProfile?)null);

        // Act
        var result = await _controller.GetVendorById(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    #endregion

    #region GetAllVendors Tests

    [Fact]
    public async Task GetAllVendors_ShouldReturnPaginatedResults()
    {
        // Arrange
        var vendors = new List<VendorProfile>
        {
            CreateTestVendor(1, "Vendor 1"),
            CreateTestVendor(2, "Vendor 2"),
            CreateTestVendor(3, "Vendor 3")
        };

        _unitOfWorkMock.Setup(x => x.VendorProfiles.GetAllAsync())
            .ReturnsAsync(vendors);

        // Act
        var result = await _controller.GetAllVendors(page: 1, pageSize: 2);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsAssignableFrom<IEnumerable<VendorResponse>>(okResult.Value);
        Assert.Equal(2, response.Count());
    }

    [Fact]
    public async Task GetAllVendors_ShouldAddPaginationHeaders()
    {
        // Arrange
        var vendors = new List<VendorProfile>
        {
            CreateTestVendor(1, "Vendor 1"),
            CreateTestVendor(2, "Vendor 2")
        };

        _unitOfWorkMock.Setup(x => x.VendorProfiles.GetAllAsync())
            .ReturnsAsync(vendors);

        // Act
        await _controller.GetAllVendors(page: 1, pageSize: 10);

        // Assert
        Assert.True(_controller.Response.Headers.ContainsKey("X-Total-Count"));
        Assert.Equal("2", _controller.Response.Headers["X-Total-Count"].ToString());
    }

    #endregion

    #region GetVendorRiskAssessment Tests

    [Fact]
    public async Task GetVendorRiskAssessment_WhenVendorExists_ShouldReturnAssessment()
    {
        // Arrange
        var vendor = CreateTestVendor(1, "Test Vendor");
        var assessment = new RiskAssessment
        {
            Id = 1,
            VendorId = 1,
            FinancialRiskScore = 0.3m,
            OperationalRiskScore = 0.4m,
            SecurityComplianceRiskScore = 0.5m,
            FinalRiskScore = 0.4m,
            RiskLevel = RiskLevel.Medium,
            Explanation = "Test explanation"
        };

        _unitOfWorkMock.Setup(x => x.VendorProfiles.GetByIdAsync(1))
            .ReturnsAsync(vendor);
        _riskScoringServiceMock.Setup(x => x.CalculateRiskScoreAsync(vendor))
            .ReturnsAsync(assessment);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _controller.GetVendorRiskAssessment(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<RiskAssessmentResponse>(okResult.Value);
        Assert.Equal(assessment.FinalRiskScore, response.FinalRiskScore);
        Assert.Equal("Medium", response.RiskLevel);
    }

    [Fact]
    public async Task GetVendorRiskAssessment_ShouldCallRiskScoringService()
    {
        // Arrange
        var vendor = CreateTestVendor(1, "Test Vendor");
        _unitOfWorkMock.Setup(x => x.VendorProfiles.GetByIdAsync(1))
            .ReturnsAsync(vendor);
        _riskScoringServiceMock.Setup(x => x.CalculateRiskScoreAsync(vendor))
            .ReturnsAsync(new RiskAssessment());

        // Act
        await _controller.GetVendorRiskAssessment(1);

        // Assert
        _riskScoringServiceMock.Verify(x => x.CalculateRiskScoreAsync(vendor), Times.Once);
    }

    [Fact]
    public async Task GetVendorRiskAssessment_ShouldSaveAssessmentToDatabase()
    {
        // Arrange
        var vendor = CreateTestVendor(1, "Test Vendor");
        _unitOfWorkMock.Setup(x => x.VendorProfiles.GetByIdAsync(1))
            .ReturnsAsync(vendor);
        _riskScoringServiceMock.Setup(x => x.CalculateRiskScoreAsync(vendor))
            .ReturnsAsync(new RiskAssessment());

        // Act
        await _controller.GetVendorRiskAssessment(1);

        // Assert
        _unitOfWorkMock.Verify(x => x.RiskAssessments.AddAsync(It.IsAny<RiskAssessment>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetVendorRiskAssessment_WhenVendorNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.VendorProfiles.GetByIdAsync(999))
            .ReturnsAsync((VendorProfile?)null);

        // Act
        var result = await _controller.GetVendorRiskAssessment(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    #endregion

    #region DeleteVendor Tests

    [Fact]
    public async Task DeleteVendor_WhenVendorExists_ShouldReturnNoContent()
    {
        // Arrange
        var vendor = CreateTestVendor(1, "Test Vendor");
        _unitOfWorkMock.Setup(x => x.VendorProfiles.GetByIdAsync(1))
            .ReturnsAsync(vendor);

        // Act
        var result = await _controller.DeleteVendor(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteVendor_ShouldCallUnitOfWorkRemove()
    {
        // Arrange
        var vendor = CreateTestVendor(1, "Test Vendor");
        _unitOfWorkMock.Setup(x => x.VendorProfiles.GetByIdAsync(1))
            .ReturnsAsync(vendor);

        // Act
        await _controller.DeleteVendor(1);

        // Assert
        _unitOfWorkMock.Verify(x => x.VendorProfiles.Remove(vendor), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteVendor_WhenVendorNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.VendorProfiles.GetByIdAsync(999))
            .ReturnsAsync((VendorProfile?)null);

        // Act
        var result = await _controller.DeleteVendor(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion

    #region Helper Methods

    private CreateVendorRequest CreateValidVendorRequest()
    {
        return new CreateVendorRequest
        {
            Name = "Test Vendor",
            FinancialHealth = 75,
            SlaUptime = 95m,
            MajorIncidents = 0,
            SecurityCerts = new List<string> { "ISO27001" },
            Documents = new DocumentValidationDto
            {
                ContractValid = true,
                PrivacyPolicyValid = true,
                PentestReportValid = true
            }
        };
    }

    private VendorProfile CreateTestVendor(int id, string name)
    {
        return new VendorProfile
        {
            Id = id,
            Name = name,
            FinancialHealth = 75,
            SlaUptime = 95m,
            MajorIncidents = 0,
            SecurityCerts = new List<string> { "ISO27001" },
            Documents = new DocumentValidation(true, true, true),
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
