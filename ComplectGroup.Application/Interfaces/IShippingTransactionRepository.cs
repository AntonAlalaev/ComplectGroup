namespace ComplectGroup.Application.Interfaces;

using ComplectGroup.Domain.Entities;

public interface IShippingTransactionRepository
{
    /// <summary>
    /// Получить историю отгрузок по позиции
    /// </summary>
    Task<List<ShippingTransaction>> GetByPositionIdAsync(int positionId, CancellationToken ct);

    /// <summary>
    /// Получить историю отгрузок по детали
    /// </summary>
    Task<List<ShippingTransaction>> GetByPartIdAsync(int partId, CancellationToken ct);

    /// <summary>
    /// Получить все отгрузки
    /// </summary>
    Task<List<ShippingTransaction>> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Добавить новую отгрузку
    /// </summary>
    Task AddAsync(ShippingTransaction transaction, CancellationToken ct);
}
