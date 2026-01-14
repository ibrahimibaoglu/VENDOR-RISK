namespace VendorRiskAPI.Domain.ValueObjects;

public class DocumentValidation
{
    public bool ContractValid { get; private set; }
    public bool PrivacyPolicyValid { get; private set; }
    public bool PentestReportValid { get; private set; }

    private DocumentValidation() { } // EF Core için

    public DocumentValidation(bool contractValid, bool privacyPolicyValid, bool pentestReportValid)
    {
        ContractValid = contractValid;
        PrivacyPolicyValid = privacyPolicyValid;
        PentestReportValid = pentestReportValid;
    }

    public bool HasAllValidDocuments() => ContractValid && PrivacyPolicyValid && PentestReportValid;

    public int GetInvalidDocumentCount()
    {
        int count = 0;
        if (!ContractValid) count++;
        if (!PrivacyPolicyValid) count++;
        if (!PentestReportValid) count++;
        return count;
    }

    public List<string> GetInvalidDocuments()
    {
        var invalidDocs = new List<string>();
        if (!ContractValid) invalidDocs.Add("Contract");
        if (!PrivacyPolicyValid) invalidDocs.Add("Privacy Policy");
        if (!PentestReportValid) invalidDocs.Add("Pentest Report");
        return invalidDocs;
    }
}
