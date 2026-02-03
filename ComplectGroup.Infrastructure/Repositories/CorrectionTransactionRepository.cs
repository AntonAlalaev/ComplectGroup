// ComplectGroup.Infrastructure/Repositories/CorrectionTransactionRepository.cs
using ComplectGroup.Infrastructure.Data;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplectGroup.Infrastructure.Repositories;

public class CorrectionTransactionRepository : ICorrectionTransactionRepository
{
    private readonly AppDbContext _context;
    
    public CorrectionTransactionRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<CorrectionTransaction?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.CorrectionTransactions
            .Include(c => c.OldPart)
            .ThenInclude(p => p.Chapter)
            .Include(c => c.NewPart)
            .ThenInclude(p => p.Chapter)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }
    
    public async Task<List<CorrectionTransaction>> GetAllAsync(CancellationToken ct)
    {
        return await _context.CorrectionTransactions
            .Include(c => c.OldPart)
            .ThenInclude(p => p.Chapter)
            .Include(c => c.NewPart)
            .ThenInclude(p => p.Chapter)
            .OrderByDescending(c => c.CorrectionDate)
            .ToListAsync(ct);
    }
    
    public async Task<CorrectionTransaction?> GetLastAsync(CancellationToken ct)
    {
        return await _context.CorrectionTransactions
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task AddAsync(CorrectionTransaction correction, CancellationToken ct)
    {
        _context.CorrectionTransactions.Add(correction);
        await _context.SaveChangesAsync(ct);
    }
}