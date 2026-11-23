using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities.Planning;
using ComplectGroup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplectGroup.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы с бригадами
/// </summary>
public class BrigadeRepository : IBrigadeRepository
{
    private readonly AppDbContext _context;

    public BrigadeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Brigade>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Brigades
            .AsNoTracking()
            .Include(b => b.Operations)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Brigade?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Brigades
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task AddAsync(Brigade brigade, CancellationToken cancellationToken)
    {
        await _context.Brigades.AddAsync(brigade, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Brigade brigade, CancellationToken cancellationToken)
    {
        _context.Brigades.Update(brigade);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Brigade brigade, CancellationToken cancellationToken)
    {
        _context.Brigades.Remove(brigade);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken cancellationToken)
    {
        var normalized = name.Trim().ToLower();

        return await _context.Brigades.AnyAsync(
            b => b.Name.ToLower() == normalized
                 && (!excludeId.HasValue || b.Id != excludeId.Value),
            cancellationToken);
    }

    public async Task<bool> HasOperationsAsync(int brigadeId, CancellationToken cancellationToken)
    {
        return await _context.PartOperations.AnyAsync(o => o.BrigadeId == brigadeId, cancellationToken);
    }
}
