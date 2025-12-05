using ComplectGroup.Domain.Entities;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Репозиторий для работы с позициями комплектации
/// </summary>
public interface IPositionRepository
{
    /// <summary>
    /// Получить позицию по ID
    /// </summary>
    Task<Position?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Получить все позиции
    /// </summary>
    Task<List<Position>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Получить позиции по ID комплектации
    /// </summary>
    Task<List<Position>> GetByComplectationIdAsync(int complectationId, CancellationToken cancellationToken);

    /// <summary>
    /// Добавить новую позицию
    /// </summary>
    Task AddAsync(Position position, CancellationToken cancellationToken);

    /// <summary>
    /// Обновить позицию
    /// </summary>
    Task UpdateAsync(Position position, CancellationToken cancellationToken);

    /// <summary>
    /// Удалить позицию
    /// </summary>
    Task DeleteAsync(Position position, CancellationToken cancellationToken);
}
