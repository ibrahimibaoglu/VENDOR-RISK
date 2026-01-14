using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VendorRiskAPI.Application.DTOs.Request;
using VendorRiskAPI.Application.DTOs.Response;
using VendorRiskAPI.Infrastructure.Persistence;
using Xunit;

namespace VendorRiskAPI.Tests.Integration;

public class VendorApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public VendorApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's ApplicationDbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add ApplicationDbContext using an in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryTestDb");
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                    // Ensure the database is created
                    db.Database.EnsureCreated();
                }
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateVendor_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateVendorRequest
        {
            Name = "Integration Test Vendor",
            FinancialHealth = 80,
            SlaUptime = 95m,
            MajorIncidents = 0,
            SecurityCerts = new List<string> { "ISO27001" },
            Documents = new Application.DTOs.Common.DocumentValidationDto
            {
                ContractValid = true,
                PrivacyPolicyValid = true,
                PentestReportValid = true
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/vendor", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var vendor = await response.Content.ReadFromJsonAsync<VendorResponse>();
        Assert.NotNull(vendor);
        Assert.Equal(request.Name, vendor.Name);
        Assert.True(vendor.Id > 0);
    }

    [Fact]
    public async Task GetVendor_AfterCreate_ShouldReturnVendor()
    {
        // Arrange - Create a vendor first
        var createRequest = new CreateVendorRequest
        {
            Name = "Test Vendor for Get",
            FinancialHealth = 75,
            SlaUptime = 93m,
            MajorIncidents = 1,
            SecurityCerts = new List<string> { "ISO27001" },
            Documents = new Application.DTOs.Common.DocumentValidationDto
            {
                ContractValid = true,
                PrivacyPolicyValid = false,
                PentestReportValid = true
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/vendor", createRequest);
        var createdVendor = await createResponse.Content.ReadFromJsonAsync<VendorResponse>();

        // Act - Get the vendor
        var getResponse = await _client.GetAsync($"/api/vendor/{createdVendor!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var vendor = await getResponse.Content.ReadFromJsonAsync<VendorResponse>();
        Assert.NotNull(vendor);
        Assert.Equal(createRequest.Name, vendor.Name);
    }

    [Fact]
    public async Task GetVendorRiskAssessment_ShouldCalculateAndReturnRisk()
    {
        // Arrange - Create a vendor first
        var createRequest = new CreateVendorRequest
        {
            Name = "Risk Assessment Test Vendor",
            FinancialHealth = 45,  // High risk
            SlaUptime = 88m,       // Below 95%
            MajorIncidents = 3,
            SecurityCerts = new List<string>(),  // No certs
            Documents = new Application.DTOs.Common.DocumentValidationDto
            {
                ContractValid = true,
                PrivacyPolicyValid = false,
                PentestReportValid = false
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/vendor", createRequest);
        var createdVendor = await createResponse.Content.ReadFromJsonAsync<VendorResponse>();

        // Act - Get risk assessment
        var riskResponse = await _client.GetAsync($"/api/vendor/{createdVendor!.Id}/risk");

        // Assert
        Assert.Equal(HttpStatusCode.OK, riskResponse.StatusCode);
        
        var assessment = await riskResponse.Content.ReadFromJsonAsync<RiskAssessmentResponse>();
        Assert.NotNull(assessment);
        Assert.True(assessment.FinalRiskScore > 0);
        Assert.True(assessment.FinancialRiskScore >= 0.60m); // High financial risk
        Assert.False(string.IsNullOrEmpty(assessment.Explanation));
    }

    [Fact]
    public async Task GetAllVendors_ShouldReturnPaginatedList()
    {
        // Arrange - Create multiple vendors
        for (int i = 1; i <= 5; i++)
        {
            var request = new CreateVendorRequest
            {
                Name = $"Vendor {i}",
                FinancialHealth = 70 + i,
                SlaUptime = 90m + i,
                MajorIncidents = 0,
                SecurityCerts = new List<string> { "ISO27001" },
                Documents = new Application.DTOs.Common.DocumentValidationDto
                {
                    ContractValid = true,
                    PrivacyPolicyValid = true,
                    PentestReportValid = true
                }
            };
            await _client.PostAsJsonAsync("/api/vendor", request);
        }

        // Act
        var response = await _client.GetAsync("/api/vendor?page=1&pageSize=3");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var vendors = await response.Content.ReadFromJsonAsync<List<VendorResponse>>();
        Assert.NotNull(vendors);
        Assert.True(vendors.Count <= 3); // Should respect page size
        
        // Check pagination headers
        Assert.True(response.Headers.Contains("X-Total-Count"));
        Assert.True(response.Headers.Contains("X-Page"));
    }

    [Fact]
    public async Task DeleteVendor_ShouldRemoveVendor()
    {
        // Arrange - Create a vendor first
        var createRequest = new CreateVendorRequest
        {
            Name = "Vendor to Delete",
            FinancialHealth = 70,
            SlaUptime = 95m,
            MajorIncidents = 0,
            SecurityCerts = new List<string>(),
            Documents = new Application.DTOs.Common.DocumentValidationDto
            {
                ContractValid = true,
                PrivacyPolicyValid = true,
                PentestReportValid = true
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/vendor", createRequest);
        var createdVendor = await createResponse.Content.ReadFromJsonAsync<VendorResponse>();

        // Act - Delete the vendor
        var deleteResponse = await _client.DeleteAsync($"/api/vendor/{createdVendor!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify vendor is deleted
        var getResponse = await _client.GetAsync($"/api/vendor/{createdVendor.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetVendor_WhenNotFound_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync("/api/vendor/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateVendor_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange - Invalid financial health (> 100)
        var request = new CreateVendorRequest
        {
            Name = "Invalid Vendor",
            FinancialHealth = 150,  // Invalid: > 100
            SlaUptime = 95m,
            MajorIncidents = 0,
            SecurityCerts = new List<string>(),
            Documents = new Application.DTOs.Common.DocumentValidationDto
            {
                ContractValid = true,
                PrivacyPolicyValid = true,
                PentestReportValid = true
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/vendor", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
