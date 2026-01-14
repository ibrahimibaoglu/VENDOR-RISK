using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VendorRiskAPI.Domain.Entities;
using VendorRiskAPI.Domain.ValueObjects;
using VendorRiskAPI.Infrastructure.Persistence;
using VendorRiskAPI.Infrastructure.Seeders.Models;

namespace VendorRiskAPI.Infrastructure.Seeders;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await _context.VendorProfiles.AnyAsync())
            {
                _logger.LogInformation("Database already contains vendor data. Skipping seed.");
                return;
            }

            // Seed vendors
            await SeedVendorsAsync();

            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedVendorsAsync()
    {
        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SampleVendorData.json");
        
        if (!File.Exists(jsonPath))
        {
            _logger.LogWarning("Sample vendor data file not found at: {JsonPath}", jsonPath);
            return;
        }

        _logger.LogInformation("Loading sample vendor data from: {JsonPath}", jsonPath);

        var jsonContent = await File.ReadAllTextAsync(jsonPath);
        var seedData = JsonSerializer.Deserialize<VendorSeedData>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (seedData?.Vendors == null || !seedData.Vendors.Any())
        {
            _logger.LogWarning("No vendor data found in JSON file.");
            return;
        }

        _logger.LogInformation("Found {VendorCount} vendors to seed", seedData.Vendors.Count);

        foreach (var vendorDto in seedData.Vendors)
        {
            var vendor = new VendorProfile
            {
                Name = vendorDto.Name,
                FinancialHealth = vendorDto.FinancialHealth,
                SlaUptime = vendorDto.SlaUptime,
                MajorIncidents = vendorDto.MajorIncidents,
                SecurityCerts = vendorDto.SecurityCerts ?? new List<string>(),
                Documents = new DocumentValidation(
                    vendorDto.Documents.ContractValid,
                    vendorDto.Documents.PrivacyPolicyValid,
                    vendorDto.Documents.PentestReportValid
                ),
                CreatedAt = DateTime.UtcNow
            };

            await _context.VendorProfiles.AddAsync(vendor);
            _logger.LogDebug("Added vendor: {VendorName}", vendor.Name);
        }

        var savedCount = await _context.SaveChangesAsync();
        _logger.LogInformation("Successfully seeded {SavedCount} vendors", savedCount);
    }
}
