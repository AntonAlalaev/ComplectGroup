using ComplectGroup.Domain.Entities;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Репозиторий для работы с главами (разделами)
/// </summary>
public interface IChapterRepository
{
    /// <summary>
    /// Получить главу по ID
    /// </summary>
    Task<Chapter?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Получить все главы
    /// </summary>
    Task<List<Chapter>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Добавить новую главу
    /// </summary>
    Task AddAsync(Chapter chapter, CancellationToken cancellationToken);

    /// <summary>
    /// Обновить главу
    /// </summary>
    Task UpdateAsync(Chapter chapter, CancellationToken cancellationToken);

    /// <summary>
    /// Удалить главу
    /// </summary>
    Task DeleteAsync(Chapter chapter, CancellationToken cancellationToken);
}
