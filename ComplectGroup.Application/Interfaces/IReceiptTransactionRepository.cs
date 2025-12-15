namespace ComplectGroup.Application.Interfaces;

using ComplectGroup.Domain.Entities;

public interface IReceiptTransactionRepository
{
    /// <summary>
    /// Получить историю приёмок по детали
    /// </summary>
    Task<List<ReceiptTransaction>> GetByPartIdAsync(int partId, CancellationToken ct);

    /// <summary>
    /// Получить все приёмки
    /// </summary>
    Task<List<ReceiptTransaction>> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Добавить новую приёмку
    /// </summary>
    Task AddAsync(ReceiptTransaction transaction, CancellationToken ct);
}
