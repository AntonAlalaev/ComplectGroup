using ComplectGroup.Infrastructure.Data;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplectGroup.Infrastructure.Repositories;

public class ShippingTransactionRepository : IShippingTransactionRepository
{
    private readonly AppDbContext _context;

    public ShippingTransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ShippingTransaction>> GetByPositionIdAsync(int positionId, CancellationToken ct)
    {
        return await _context.ShippingTransactions
            .AsNoTracking()
            .Include(s => s.Part)
            .ThenInclude(p => p.Chapter)
            .Include(s => s.Position)
            .Where(s => s.PositionId == positionId)
            .OrderByDescending(s => s.ShippingDate)
            .ToListAsync(ct);
    }

    public async Task<List<ShippingTransaction>> GetByPartIdAsync(int partId, CancellationToken ct)
    {
        return await _context.ShippingTransactions
            .AsNoTracking()
            .Include(s => s.Part)
            .ThenInclude(p => p.Chapter)
            .Include(s => s.Position)
            .Where(s => s.PartId == partId)
            .OrderByDescending(s => s.ShippingDate)
            .ToListAsync(ct);
    }

    public async Task<List<ShippingTransaction>> GetAllAsync(CancellationToken ct)
    {
        return await _context.ShippingTransactions
            .AsNoTracking()
            .Include(s => s.Part)
            .ThenInclude(p => p.Chapter)
            .Include(s => s.Position)
            .OrderByDescending(s => s.ShippingDate)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ShippingTransaction transaction, CancellationToken ct)
    {
        _context.ShippingTransactions.Add(transaction);
        await _context.SaveChangesAsync(ct);
    }
}
