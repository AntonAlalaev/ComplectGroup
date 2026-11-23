using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities.Planning;
using ComplectGroup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplectGroup.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы с операциями технологических маршрутов
/// </summary>
public class PartOperationRepository : IPartOperationRepository
{
    private readonly AppDbContext _context;

    public PartOperationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PartOperation>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await Query().ToListAsync(cancellationToken);
    }

    public async Task<List<PartOperation>> GetByPartIdAsync(int partId, CancellationToken cancellationToken)
    {
        return await Query()
            .Where(o => o.PartId == partId)
            .ToListAsync(cancellationToken);
    }

    public async Task<PartOperation?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await Query().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    private IQueryable<PartOperation> Query()
    {
        return _context.PartOperations
            .AsNoTracking()
            .Include(o => o.Part)
                .ThenInclude(p => p.Chapter)
            .Include(o => o.Brigade)
            .Include(o => o.OperationType)
            .OrderBy(o => o.PartId)
            .ThenBy(o => o.SequenceNumber);
    }

    public async Task AddAsync(PartOperation operation, CancellationToken cancellationToken)
    {
        await _context.PartOperations.AddAsync(operation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(PartOperation operation, CancellationToken cancellationToken)
    {
        _context.PartOperations.Update(operation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(PartOperation operation, CancellationToken cancellationToken)
    {
        _context.PartOperations.Remove(operation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SequenceExistsAsync(
        int partId,
        int sequenceNumber,
        int? excludeId,
        CancellationToken cancellationToken)
    {
        return await _context.PartOperations.AnyAsync(
            o => o.PartId == partId
                 && o.SequenceNumber == sequenceNumber
                 && (!excludeId.HasValue || o.Id != excludeId.Value),
            cancellationToken);
    }
}
