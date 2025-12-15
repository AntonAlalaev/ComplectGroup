namespace ComplectGroup.Application.Interfaces;

using ComplectGroup.Domain.Entities;

public interface IPositionShipmentRepository
{
    /// <summary>Получить отслеживание отгрузок по позиции</summary>
    Task<PositionShipment?> GetByPositionIdAsync(int positionId, CancellationToken ct);

    /// <summary>Получить все отслеживания</summary>
    Task<List<PositionShipment>> GetAllAsync(CancellationToken ct);

    /// <summary>Добавить новое отслеживание (при первой отгрузке)</summary>
    Task AddAsync(PositionShipment shipment, CancellationToken ct);

    /// <summary>Обновить отслеживание (при последующих отгрузках)</summary>
    Task UpdateAsync(PositionShipment shipment, CancellationToken ct);
}
