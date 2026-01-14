using AutoMapper;
using VendorRiskAPI.Application.DTOs.Common;
using VendorRiskAPI.Application.DTOs.Request;
using VendorRiskAPI.Application.DTOs.Response;
using VendorRiskAPI.Domain.Entities;
using VendorRiskAPI.Domain.ValueObjects;

namespace VendorRiskAPI.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Request DTOs to Entities
        CreateMap<CreateVendorRequest, VendorProfile>()
            .ForMember(dest => dest.Documents, opt => opt.MapFrom(src => 
                new DocumentValidation(
                    src.Documents.ContractValid,
                    src.Documents.PrivacyPolicyValid,
                    src.Documents.PentestReportValid
                )))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.RiskAssessments, opt => opt.Ignore());

        // Entities to Response DTOs
        CreateMap<VendorProfile, VendorResponse>()
            .ForMember(dest => dest.Documents, opt => opt.MapFrom(src => 
                new DocumentValidationDto
                {
                    ContractValid = src.Documents.ContractValid,
                    PrivacyPolicyValid = src.Documents.PrivacyPolicyValid,
                    PentestReportValid = src.Documents.PentestReportValid
                }));

        CreateMap<RiskAssessment, RiskAssessmentResponse>()
            .ForMember(dest => dest.RiskLevel, opt => opt.MapFrom(src => src.RiskLevel.ToString()));
    }
}
