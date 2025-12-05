using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using ComplectGroup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplectGroup.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для работы с комплектациями
/// </summary>
public class ComplectationRepository : IComplectationRepository
{
    private readonly AppDbContext _context;

    public ComplectationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Complectation?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Complectations
            .Include(c => c.Positions)
            .ThenInclude(p => p.Part)
            .ThenInclude(part => part.Chapter)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Complectation>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Complectations
            .Include(c => c.Positions)
            .ThenInclude(p => p.Part)
            .ThenInclude(part => part.Chapter)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Complectation complectation, CancellationToken cancellationToken)
    {
        await _context.Complectations.AddAsync(complectation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Complectation complectation, CancellationToken cancellationToken)
    {
        _context.Complectations.Update(complectation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Complectation complectation, CancellationToken cancellationToken)
    {
        _context.Complectations.Remove(complectation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Complectations
            .AnyAsync(c => c.Id == id, cancellationToken);
    }
}
