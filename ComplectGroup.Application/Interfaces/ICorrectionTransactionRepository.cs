// ComplectGroup.Application/Interfaces/ICorrectionTransactionRepository.cs
namespace ComplectGroup.Application.Interfaces;
using ComplectGroup.Domain.Entities;

public interface ICorrectionTransactionRepository
{
    /// <summary>
    /// Получить корректировку по ID
    /// </summary>
    Task<CorrectionTransaction?> GetByIdAsync(int id, CancellationToken ct);
    
    /// <summary>
    /// Получить все корректировки
    /// </summary>
    Task<List<CorrectionTransaction>> GetAllAsync(CancellationToken ct);
    
    /// <summary>
    /// Получить последнюю корректировку
    /// </summary>
    Task<CorrectionTransaction?> GetLastAsync(CancellationToken ct);
    
    /// <summary>
    /// Добавить новую корректировку
    /// </summary>
    Task AddAsync(CorrectionTransaction correction, CancellationToken ct);
}