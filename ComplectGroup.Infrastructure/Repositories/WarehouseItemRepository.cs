
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using ComplectGroup.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace ComplectGroup.Infrastructure.Repositories;

public class WarehouseItemRepository : IWarehouseItemRepository
{
    private readonly AppDbContext _context;

    public WarehouseItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WarehouseItem?> GetByPartIdAsync(int partId, CancellationToken ct)
    {
        return await _context.WarehouseItems
            .AsNoTracking()
            .Include(w => w.Part)
            .ThenInclude(p => p.Chapter)
            .FirstOrDefaultAsync(w => w.PartId == partId, ct);
    }

    public async Task<List<WarehouseItem>> GetAllAsync(CancellationToken ct)
    {
        return await _context.WarehouseItems
            .AsNoTracking()
            .Include(w => w.Part)
            .ThenInclude(p => p.Chapter)
            .OrderBy(w => w.Part.Chapter.Name)
            .ThenBy(w => w.Part.Name)
            .ToListAsync(ct);
    }

    public async Task AddAsync(WarehouseItem item, CancellationToken ct)
    {
        _context.WarehouseItems.Add(item);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(WarehouseItem item, CancellationToken ct)
    {
        _context.WarehouseItems.Update(item);
        await _context.SaveChangesAsync(ct);
    }
}
