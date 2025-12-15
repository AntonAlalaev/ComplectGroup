using ComplectGroup.Infrastructure.Data;
using ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplectGroup.Infrastructure.Repositories;
public class PositionShipmentRepository : IPositionShipmentRepository
{
    private readonly AppDbContext _context;

    public PositionShipmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PositionShipment?> GetByPositionIdAsync(int positionId, CancellationToken ct)
    {
        return await _context.PositionShipments
            .AsNoTracking()
            .Include(ps => ps.Position)
            .ThenInclude(p => p.Part)
            .FirstOrDefaultAsync(ps => ps.PositionId == positionId, ct);
    }

    public async Task<List<PositionShipment>> GetAllAsync(CancellationToken ct)
    {
        return await _context.PositionShipments
            .AsNoTracking()
            .Include(ps => ps.Position)
            .ThenInclude(p => p.Part)
            .OrderByDescending(ps => ps.LastShippedDate)
            .ToListAsync(ct);
    }

    public async Task AddAsync(PositionShipment shipment, CancellationToken ct)
    {
        _context.PositionShipments.Add(shipment);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PositionShipment shipment, CancellationToken ct)
    {
        _context.PositionShipments.Update(shipment);
        await _context.SaveChangesAsync(ct);
    }
}
