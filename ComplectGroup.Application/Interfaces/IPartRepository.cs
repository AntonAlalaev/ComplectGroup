using ComplectGroup.Domain.Entities;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с деталями
/// </summary>
public interface IPartRepository
{
    /// <summary>
    /// Получить деталь по ID
    /// </summary>
    Task<Part?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Получить все детали
    /// </summary>
    Task<List<Part>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Получить детали по ID главы
    /// </summary>
    Task<List<Part>> GetByChapterIdAsync(int chapterId, CancellationToken cancellationToken);

    /// <summary>
    /// Добавить новую деталь
    /// </summary>
    Task AddAsync(Part part, CancellationToken cancellationToken);

    /// <summary>
    /// Обновить деталь
    /// </summary>
    Task UpdateAsync(Part part, CancellationToken cancellationToken);

    /// <summary>
    /// Удалить деталь
    /// </summary>
    Task DeleteAsync(Part part, CancellationToken cancellationToken);
}