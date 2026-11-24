using ComplectGroup.Domain.Entities.Planning;

namespace ComplectGroup.Application.Interfaces;

public interface IProductionPlanRepository
{
    Task<List<ProductionPlan>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProductionPlan?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task AddAsync(ProductionPlan plan, CancellationToken cancellationToken);

    Task UpdateAsync(ProductionPlan plan, CancellationToken cancellationToken);

    Task DeleteAsync(ProductionPlan plan, CancellationToken cancellationToken);

    Task<bool> HasOverlappingActivePlanAsync(
        DateTime periodStart,
        DateTime periodEnd,
        int? excludeId,
        bool allowOverlap,
        CancellationToken cancellationToken);
}
