using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VendorRiskAPI.Domain.Entities;
using VendorRiskAPI.Infrastructure.Persistence.Configurations;

namespace VendorRiskAPI.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ILogger<ApplicationDbContext>? _logger;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ILogger<ApplicationDbContext> logger) 
        : base(options)
    {
        _logger = logger;
    }

    public DbSet<VendorProfile> VendorProfiles => Set<VendorProfile>();
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations in the assembly
        modelBuilder.ApplyConfiguration(new VendorProfileConfiguration());
        modelBuilder.ApplyConfiguration(new RiskAssessmentConfiguration());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entriesCount = ChangeTracker.Entries<Domain.Common.BaseEntity>().Count();
        
        // Update timestamps automatically
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        _logger?.LogDebug("Saving {EntriesCount} entities to database", entriesCount);

        return base.SaveChangesAsync(cancellationToken);
    }
}
