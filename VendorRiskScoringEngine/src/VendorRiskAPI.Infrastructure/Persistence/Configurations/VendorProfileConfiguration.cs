using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VendorRiskAPI.Domain.Entities;

namespace VendorRiskAPI.Infrastructure.Persistence.Configurations;

public class VendorProfileConfiguration : IEntityTypeConfiguration<VendorProfile>
{
    public void Configure(EntityTypeBuilder<VendorProfile> builder)
    {
        builder.ToTable("VendorProfiles");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.FinancialHealth)
            .IsRequired();

        builder.Property(v => v.SlaUptime)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(v => v.MajorIncidents)
            .IsRequired();

        // Configure SecurityCerts as JSON column for PostgreSQL
        builder.Property(v => v.SecurityCerts)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
            );

        // Configure Documents as owned type
        builder.OwnsOne(v => v.Documents, docs =>
        {
            docs.Property(d => d.ContractValid).IsRequired();
            docs.Property(d => d.PrivacyPolicyValid).IsRequired();
            docs.Property(d => d.PentestReportValid).IsRequired();
        });

        // Configure relationship
        builder.HasMany(v => v.RiskAssessments)
            .WithOne(r => r.Vendor)
            .HasForeignKey(r => r.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(v => v.Name);
        builder.HasIndex(v => v.FinancialHealth);
        builder.HasIndex(v => v.SlaUptime);
    }
}
