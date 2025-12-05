using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using ComplectGroup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplectGroup.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для работы с деталями
/// </summary>
public class PartRepository : IPartRepository
{
    private readonly AppDbContext _context;

    public PartRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Part?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Parts
            .Include(p => p.Chapter)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<Part>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Parts
            .Include(p => p.Chapter)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Part>> GetByChapterIdAsync(int chapterId, CancellationToken cancellationToken)
    {
        return await _context.Parts
            .Include(p => p.Chapter)
            .Where(p => p.Chapter.Id == chapterId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Part part, CancellationToken cancellationToken)
    {
        await _context.Parts.AddAsync(part, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Part part, CancellationToken cancellationToken)
    {
        _context.Parts.Update(part);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Part part, CancellationToken cancellationToken)
    {
        _context.Parts.Remove(part);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
