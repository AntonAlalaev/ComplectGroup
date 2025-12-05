using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using ComplectGroup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplectGroup.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для работы с позициями
/// </summary>
public class PositionRepository : IPositionRepository
{
    private readonly AppDbContext _context;

    public PositionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Position?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Positions
            .Include(p => p.Part)
            .ThenInclude(part => part.Chapter)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<Position>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Positions
            .Include(p => p.Part)
            .ThenInclude(part => part.Chapter)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Position>> GetByComplectationIdAsync(int complectationId, CancellationToken cancellationToken)
    {
        return await _context.Positions
            .Include(p => p.Part)
            .ThenInclude(part => part.Chapter)
            .Where(p => p.ComplectationId == complectationId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Position position, CancellationToken cancellationToken)
    {
        await _context.Positions.AddAsync(position, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Position position, CancellationToken cancellationToken)
    {
        _context.Positions.Update(position);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Position position, CancellationToken cancellationToken)
    {
        _context.Positions.Remove(position);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
