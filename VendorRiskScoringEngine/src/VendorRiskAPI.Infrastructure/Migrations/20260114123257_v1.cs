using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VendorRiskAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VendorProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FinancialHealth = table.Column<int>(type: "integer", nullable: false),
                    SlaUptime = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MajorIncidents = table.Column<int>(type: "integer", nullable: false),
                    SecurityCerts = table.Column<string>(type: "jsonb", nullable: false),
                    Documents_ContractValid = table.Column<bool>(type: "boolean", nullable: false),
                    Documents_PrivacyPolicyValid = table.Column<bool>(type: "boolean", nullable: false),
                    Documents_PentestReportValid = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    FinancialRiskScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    OperationalRiskScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    SecurityComplianceRiskScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    FinalRiskScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    RiskLevel = table.Column<string>(type: "text", nullable: false),
                    Explanation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AssessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssessedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskAssessments_VendorProfiles_VendorId",
                        column: x => x.VendorId,
                        principalTable: "VendorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_AssessedAt",
                table: "RiskAssessments",
                column: "AssessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_FinalRiskScore",
                table: "RiskAssessments",
                column: "FinalRiskScore");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_RiskLevel",
                table: "RiskAssessments",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_VendorId",
                table: "RiskAssessments",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfiles_FinancialHealth",
                table: "VendorProfiles",
                column: "FinancialHealth");

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfiles_Name",
                table: "VendorProfiles",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfiles_SlaUptime",
                table: "VendorProfiles",
                column: "SlaUptime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiskAssessments");

            migrationBuilder.DropTable(
                name: "VendorProfiles");
        }
    }
}
