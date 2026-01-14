using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VendorRiskAPI.Domain.Entities;
using VendorRiskAPI.Domain.Enums;

namespace VendorRiskAPI.Infrastructure.Persistence.Configurations;

public class RiskAssessmentConfiguration : IEntityTypeConfiguration<RiskAssessment>
{
    public void Configure(EntityTypeBuilder<RiskAssessment> builder)
    {
        builder.ToTable("RiskAssessments");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.VendorId)
            .IsRequired();

        builder.Property(r => r.FinancialRiskScore)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(r => r.OperationalRiskScore)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(r => r.SecurityComplianceRiskScore)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(r => r.FinalRiskScore)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(r => r.RiskLevel)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.Explanation)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(r => r.AssessedAt)
            .IsRequired();

        builder.Property(r => r.AssessedBy)
            .HasMaxLength(100)
            .IsRequired();

        // Indexes
        builder.HasIndex(r => r.VendorId);
        builder.HasIndex(r => r.RiskLevel);
        builder.HasIndex(r => r.AssessedAt);
        builder.HasIndex(r => r.FinalRiskScore);
    }
}
