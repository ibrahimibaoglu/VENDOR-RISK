using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VendorRiskAPI.Application.Interfaces;
using VendorRiskAPI.Application.Services;

namespace VendorRiskAPI.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IRiskScoringService, RiskScoringService>();

        // AutoMapper
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
