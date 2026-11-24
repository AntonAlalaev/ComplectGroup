using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using ComplectGroup.Domain.Entities.Planning;
using ComplectGroup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplectGroup.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы с производственными планами
/// </summary>
public class ProductionPlanRepository : IProductionPlanRepository
{
    private readonly AppDbContext _context;

    public ProductionPlanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductionPlan>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await Query()
            .OrderByDescending(p => p.PeriodStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductionPlan?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        // Отслеживание включено, чтобы при обновлении не возникало конфликта
        // нескольких экземпляров одной сущности (например, Brigade) в графе.
        return await _context.ProductionPlans
            .Include(p => p.Items)
                .ThenInclude(i => i.Part)
            .Include(p => p.Items)
                .ThenInclude(i => i.PlannedOperations)
                    .ThenInclude(o => o.Brigade)
            .Include(p => p.Items)
                .ThenInclude(i => i.PlannedOperations)
                    .ThenInclude(o => o.PartOperation)
                        .ThenInclude(po => po.OperationType)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(ProductionPlan plan, CancellationToken cancellationToken)
    {
        await _context.ProductionPlans.AddAsync(plan, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ProductionPlan plan, CancellationToken cancellationToken)
    {
        _context.ProductionPlans.Update(plan);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ProductionPlan plan, CancellationToken cancellationToken)
    {
        _context.ProductionPlans.Remove(plan);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasOverlappingActivePlanAsync(
        DateTime periodStart,
        DateTime periodEnd,
        int? excludeId,
        bool allowOverlap,
        CancellationToken cancellationToken)
    {
        if (allowOverlap)
            return false;

        return await _context.ProductionPlans.AnyAsync(p =>
            p.Id != (excludeId ?? 0) &&
            p.Status != ProductionPlanStatus.Closed &&
            p.PeriodStart < periodEnd &&
            p.PeriodEnd > periodStart,
            cancellationToken);
    }

    private IQueryable<ProductionPlan> Query()
    {
        return _context.ProductionPlans
            .AsNoTracking()
            .Include(p => p.Items)
                .ThenInclude(i => i.Part)
            .Include(p => p.Items)
                .ThenInclude(i => i.PlannedOperations)
                    .ThenInclude(o => o.Brigade)
            .Include(p => p.Items)
                .ThenInclude(i => i.PlannedOperations)
                    .ThenInclude(o => o.PartOperation)
                        .ThenInclude(po => po.OperationType);
    }
}
