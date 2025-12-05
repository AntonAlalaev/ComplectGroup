using ComplectGroup.Application.DTOs;

namespace ComplectGroup.Application.Interfaces;

/// <summary>
/// Сервис для работы с деталями
/// </summary>
public interface IPartService
{
    /// <summary>
    /// Получить деталь по ID
    /// </summary>
    Task<PartDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Получить все детали
    /// </summary>
    Task<List<PartDto>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Получить детали по ID главы
    /// </summary>
    Task<List<PartDto>> GetByChapterIdAsync(int chapterId, CancellationToken cancellationToken);

    /// <summary>
    /// Создать новую деталь
    /// </summary>
    Task<PartDto> CreateAsync(string name, int chapterId, CancellationToken cancellationToken);

    /// <summary>
    /// Обновить деталь
    /// </summary>
    Task<PartDto> UpdateAsync(int id, string name, int chapterId, CancellationToken cancellationToken);

    /// <summary>
    /// Удалить деталь
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
