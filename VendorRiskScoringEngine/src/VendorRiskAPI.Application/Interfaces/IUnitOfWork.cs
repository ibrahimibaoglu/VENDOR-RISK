using VendorRiskAPI.Domain.Entities;

namespace VendorRiskAPI.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<VendorProfile> VendorProfiles { get; }
    IRepository<RiskAssessment> RiskAssessments { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
