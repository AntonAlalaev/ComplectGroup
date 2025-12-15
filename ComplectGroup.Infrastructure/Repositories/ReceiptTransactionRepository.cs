using ComplectGroup.Infrastructure.Data;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplectGroup.Infrastructure.Repositories;


public class ReceiptTransactionRepository : IReceiptTransactionRepository
{
    private readonly AppDbContext _context;

    public ReceiptTransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReceiptTransaction>> GetByPartIdAsync(int partId, CancellationToken ct)
    {
        return await _context.ReceiptTransactions
            .AsNoTracking()
            .Include(r => r.Part)
            .ThenInclude(p => p.Chapter)
            .Where(r => r.PartId == partId)
            .OrderByDescending(r => r.ReceiptDate)
            .ToListAsync(ct);
    }

    public async Task<List<ReceiptTransaction>> GetAllAsync(CancellationToken ct)
    {
        return await _context.ReceiptTransactions
            .AsNoTracking()
            .Include(r => r.Part)
            .ThenInclude(p => p.Chapter)
            .OrderByDescending(r => r.ReceiptDate)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ReceiptTransaction transaction, CancellationToken ct)
    {
        _context.ReceiptTransactions.Add(transaction);
        await _context.SaveChangesAsync(ct);
    }
}
