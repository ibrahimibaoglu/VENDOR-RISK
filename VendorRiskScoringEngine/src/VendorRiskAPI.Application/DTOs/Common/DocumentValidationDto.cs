namespace VendorRiskAPI.Application.DTOs.Common;

public class DocumentValidationDto
{
    public bool ContractValid { get; set; }
    public bool PrivacyPolicyValid { get; set; }
    public bool PentestReportValid { get; set; }
}
