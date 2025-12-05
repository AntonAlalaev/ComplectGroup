using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using ComplectGroup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplectGroup.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для работы с главами
/// </summary>
public class ChapterRepository : IChapterRepository
{
    private readonly AppDbContext _context;

    public ChapterRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Chapter?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Chapters
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Chapter>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Chapters
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Chapter chapter, CancellationToken cancellationToken)
    {
        await _context.Chapters.AddAsync(chapter, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Chapter chapter, CancellationToken cancellationToken)
    {
        _context.Chapters.Update(chapter);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Chapter chapter, CancellationToken cancellationToken)
    {
        _context.Chapters.Remove(chapter);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
