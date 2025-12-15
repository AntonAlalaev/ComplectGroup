namespace ComplectGroup.Application.Interfaces;

using ComplectGroup.Domain.Entities;

public interface IWarehouseItemRepository
{
    /// <summary>Получить товар на складе по детали</summary>
    Task<WarehouseItem?> GetByPartIdAsync(int partId, CancellationToken ct);

    /// <summary>Получить все товары на складе</summary>
    Task<List<WarehouseItem>> GetAllAsync(CancellationToken ct);

    /// <summary>Добавить новый товар на склад</summary>
    Task AddAsync(WarehouseItem item, CancellationToken ct);

    /// <summary>Обновить количество товара</summary>
    Task UpdateAsync(WarehouseItem item, CancellationToken ct);
}
