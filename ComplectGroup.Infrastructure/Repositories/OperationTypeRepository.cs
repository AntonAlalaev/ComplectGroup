using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities.Planning;
using ComplectGroup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplectGroup.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы с типами операций
/// </summary>
public class OperationTypeRepository : IOperationTypeRepository
{
    private readonly AppDbContext _context;

    public OperationTypeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<OperationType>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.OperationTypes
            .AsNoTracking()
            .Include(o => o.Operations)
            .OrderBy(o => o.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<OperationType?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.OperationTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task AddAsync(OperationType operationType, CancellationToken cancellationToken)
    {
        await _context.OperationTypes.AddAsync(operationType, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(OperationType operationType, CancellationToken cancellationToken)
    {
        _context.OperationTypes.Update(operationType);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(OperationType operationType, CancellationToken cancellationToken)
    {
        _context.OperationTypes.Remove(operationType);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, int? excludeId, CancellationToken cancellationToken)
    {
        var normalized = code.Trim().ToLower();

        return await _context.OperationTypes.AnyAsync(
            o => o.Code.ToLower() == normalized
                 && (!excludeId.HasValue || o.Id != excludeId.Value),
            cancellationToken);
    }

    public async Task<bool> HasOperationsAsync(int operationTypeId, CancellationToken cancellationToken)
    {
        return await _context.PartOperations.AnyAsync(o => o.OperationTypeId == operationTypeId, cancellationToken);
    }
}
